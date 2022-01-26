using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PENet
{
    public class KCPNet<T> where T : KCPSession, new()
    {
        UdpClient udp;
        IPEndPoint remotePoint;

        public T clientSession;
        #region

        public void StartAsClient(string ip,int port)
        {
            udp = new UdpClient(0);
            remotePoint = new IPEndPoint(IPAddress.Parse(ip), port);
            KCPTool.ColorLog(KCPLogColor.Green, "Client Starting...");
        }

        public void ConnectServer()
        {
            // 初次连接，传4个空字节过去。当服务端收到后知道是新客户端，就生成全局唯一uuid并返回，返回的形式是“4个空字节+uuid”。客户端收到后设置KCPSession的sid。
            SendUDPMsg(new byte[4], remotePoint);
        }

        #endregion

        void SendUDPMsg(byte[] bytes, IPEndPoint remotePoint)
        {
            if (udp != null)
            {
                udp.SendAsync(bytes, bytes.Length, remotePoint);
            }
        }

        async void ClientReceive()
        {
            UdpReceiveResult result;
            while (true)
            {
                try
                {
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
                                KCPTool.ColorLog(KCPLogColor.Green,"Udp Request Conv Sid:{0}",sid);

                                // 会话处理
                                clientSession = new T();
                                clientSession.InitSession(sid, SendUDPMsg, remotePoint);
                            }
                        }
                        else
                        {
                            // 处理业务逻辑数据

                        }
                    }
                    else
                    {
                        KCPTool.Warn("Client Udp Receive illegal target Data, ip:{0}, port:{1}", result.RemoteEndPoint.Address, result.RemoteEndPoint.Port);
                    }
                }
                catch(Exception e)
                {
                    KCPTool.Warn("Client Udp Receive Data Exception:{0}", e.ToString());
                }
            }
        }
    }
}
