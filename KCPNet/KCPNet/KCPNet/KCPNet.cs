using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PENet
{
    public class KCPNet<T,K> 
        where T : KCPSession<K>, new() 
        where K : KCPMsg,new()
    {

        UdpClient udp;
        IPEndPoint remotePoint;

        private CancellationTokenSource cts;
        private CancellationToken ct;

        public T clientSession;

        public KCPNet()
        {
            cts = new CancellationTokenSource();
            ct = cts.Token;
        }

        #region Client

        public void StartAsClient(string ip,int port)
        {
            udp = new UdpClient(0); // 让操作系统自己分配端口
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                udp.Client.IOControl((IOControlCode)(-1744830452), new byte[] { 0, 0, 0, 0 }, null);
            }
            remotePoint = new IPEndPoint(IPAddress.Parse(ip), port);
            KCPTool.ColorLog(KCPLogColor.Green, "Client Starting...");

            Task.Run(ClientReceive,ct);
        }

        public void CloseClient()
        {
            
            clientSession?.CloseSession();
            
        }

        /// <summary>
        /// 连接服务器,发送4个空字节
        /// </summary>
        /// <param name="interal">重传频率 ms</param>
        /// <param name="maxInternal">最大重传时长 ms</param>
        public Task<bool> ConnectServer(int interal = 200,int maxTime = 5000)
        {
            // 初次连接，传4个空字节过去。当服务端收到后知道是新客户端，就生成全局唯一uuid并返回，返回的形式是“4个空字节+uuid”。客户端收到后设置KCPSession的sid。
            SendUDPMsg(new byte[4], remotePoint);
            int checkTimes = 0;
            Task<bool> task = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(interal);
                    checkTimes += interal;
                    if (clientSession != null && clientSession.IsConnected())
                    {
                        return true;
                    }
                    else if (checkTimes > maxTime)
                    {
                        return false;
                    }
                }
            });

            return task;
        }

        async void ClientReceive()
        {
            UdpReceiveResult result;
            while (true)
            {
                try
                {
                    if (ct.IsCancellationRequested)
                    {
                        KCPTool.ColorLog(KCPLogColor.Cyan, "ClientReceive Task is Cancelled.");
                        break;
                    }

                    result = await udp.ReceiveAsync();

                    if (Equals(remotePoint, result.RemoteEndPoint))
                    {
                        uint sid = BitConverter.ToUInt32(result.Buffer, 0);
                        if (sid == 0)
                        {
                            // sid数据
                            if (clientSession != null && clientSession.IsConnected())
                            {
                                // 已经建立连接，初始化完成了却收到了多次sid，只以第一次收到的为准，所以丢弃！
                                KCPTool.Warn("Client is Init Done, Sid Surplus");
                            }
                            else
                            {
                                // 未初始化，收到服务器分配的sid数据，初始化一个客户端session
                                sid = BitConverter.ToUInt32(result.Buffer, 4);
                                KCPTool.ColorLog(KCPLogColor.Green, "Udp Request Conv Sid:{0}", sid);

                                // 会话处理
                                clientSession = new T();
                                clientSession.InitSession(sid, SendUDPMsg, remotePoint);
                                clientSession.OnSessionClose = OnClientSessionClose;
                            }
                        }
                        else
                        {
                            if (clientSession != null && clientSession.IsConnected())
                            {
                                // 处理业务逻辑数据
                                clientSession.ReceiveData(result.Buffer);
                            }
                            else
                            {
                                // 没初始化且sid!=0时，数据消息提前到了，直接丢弃，直到初始化完成后会重传
                                KCPTool.Warn("Client is Initing...{0}", sid);
                            }
                        }
                    }
                    else
                    {
                        KCPTool.Warn("Client Udp Receive illegal target Data, ip:{0}, port:{1}", result.RemoteEndPoint.Address, result.RemoteEndPoint.Port);
                    }
                }
                catch (Exception e)
                {
                    KCPTool.Warn("Client Udp Receive Data Exception:{0}", e.ToString());
                }
            }
        }

        void OnClientSessionClose(uint sid)
        {
            cts.Cancel(); // 取消ClientReceive
            udp?.Close();
            udp = null;
            KCPTool.Warn("Client Session Close,sid:{0}", sid);
        }

        #endregion

        #region Server

        private Dictionary<uint, T> sessionDic;
        private uint now_sid = 0;

        public uint GetNewClientSessionID()
        {
            lock (sessionDic)
            {
                while (true)
                {
                    ++now_sid;
                    if (now_sid == uint.MaxValue)
                    {
                        now_sid = 1;
                    }

                    if (!sessionDic.ContainsKey(now_sid))
                    {
                        break;
                    }
                }
            }

            return now_sid;
        }

        public void StartAsServer(string ip, int port)
        {
            sessionDic = new Dictionary<uint, T>();

            udp = new UdpClient(new IPEndPoint(IPAddress.Parse(ip), port)); // 服务器的端口必须确定
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                udp.Client.IOControl((IOControlCode)(-1744830452), new byte[]{0,0,0,0}, null);
            }
            remotePoint = new IPEndPoint(IPAddress.Parse(ip), port);
            KCPTool.ColorLog(KCPLogColor.Green, "Server Starting...");

            Task.Run(ServerReceive, ct);
        }

        public void CloseServer()
        {
            foreach (var item in sessionDic)
            {
                item.Value.CloseSession();
            }

            sessionDic = null;
            cts.Cancel(); // 取消ServerReceive
            udp?.Close();
            udp = null;
            KCPTool.Warn("Server closed.");
        }

        async void ServerReceive()
        {
            UdpReceiveResult result;
            while (true)
            {
                try
                {
                    if (ct.IsCancellationRequested)
                    {
                        KCPTool.ColorLog(KCPLogColor.Cyan, "ServerReceive Task is Cancelled.");
                        break;
                    }

                    result = await udp.ReceiveAsync();

                    uint sid = BitConverter.ToUInt32(result.Buffer, 0);
                    if (sid == 0)
                    {
                        // sid=0，即未初始化 => 服务器sid发番，返回回去
                        sid = GetNewClientSessionID();
                        byte[] sid_bytes = BitConverter.GetBytes(sid);
                        byte[] conv_bytes = new byte[8];
                        Array.Copy(sid_bytes, 0 , conv_bytes, 4, 4);
                        SendUDPMsg(conv_bytes, result.RemoteEndPoint);
                    }
                    else
                    {
                        if (!sessionDic.TryGetValue(sid,out T session))
                        {
                            session = new T();
                            session.InitSession(sid,SendUDPMsg, result.RemoteEndPoint);
                            session.OnSessionClose = OnServerSessionClose;

                            lock (sessionDic)
                            {
                                sessionDic.Add(sid, session);
                            }
                        }
                        else
                        {
                            session = sessionDic[sid];
                        }

                        session.ReceiveData(result.Buffer);
                    }

                }
                catch (Exception e)
                {
                    KCPTool.Warn("Server Udp Receive Data Exception:{0}", e.ToString());
                }
            }
        }

        /// <summary>
        /// 当在服务端，有客户端Session关闭时
        /// </summary>
        /// <param name="sid">关闭的客户端Session的 session id</param>
        void OnServerSessionClose(uint sid)
        {
            if (sessionDic.ContainsKey(sid))
            {
                lock (sessionDic)
                {
                    sessionDic.Remove(sid);
                }
                KCPTool.Warn("Session:{0} remove from sessionDic.", sid);
            }
            else
            {
                KCPTool.Error("Session:{0} cannot find from sessionDic when closing.", sid);
            }
        }

        /// <summary>
        /// 广播消息给所有客户端
        /// </summary>
        public void BroadCastMsg(K msg)
        {
            byte[] bytes = KCPTool.Serialize<K>(msg);
            foreach (var item in sessionDic)
            {
                item.Value.SendMsg(bytes);
            }
        }
        #endregion

        void SendUDPMsg(byte[] bytes, IPEndPoint remotePoint)
        {
            if (udp != null)
            {
                udp.SendAsync(bytes, bytes.Length, remotePoint);
            }
        }
    }
}
