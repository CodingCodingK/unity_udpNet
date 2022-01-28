using System;
using System.Net;
using System.Net.Sockets.Kcp;
using System.Threading;
using System.Threading.Tasks;

namespace PENet
{
    /// <summary>
    /// 具体通信协议,必须继承这个
    /// </summary>
    [Serializable]
    public abstract class KCPMsg
    {

    }

    public abstract class KCPSession<T> where T: KCPMsg
    {
        protected abstract void OnDisConnected();
        protected abstract void OnConnected();
        protected abstract void OnUpDate(DateTime now);
        protected abstract void OnReceiveMsg(T msg);

        protected uint m_sessionId;
        private IPEndPoint m_remotePoint;
        protected SessionState m_sessionState = SessionState.None;
        public Action<uint> OnSessionClose;
        private Action<byte[], IPEndPoint> m_udpSender;

        public KCPHandle m_handle;
        public Kcp m_kcp;

        private CancellationTokenSource cts;
        private CancellationToken ct;


        public void InitSession(uint sid, Action<byte[], IPEndPoint> udpSender, IPEndPoint remotePoint)
        {
            this.m_sessionId = sid;
            this.m_udpSender = udpSender;
            this.m_handle = new KCPHandle();
            this.m_remotePoint = remotePoint;
            this.m_sessionState = SessionState.Connected;

            // sid = kcp添加控制信息的包里，头4个字节的数字对应传入的int值，每一个字节可以转化为对应的0~255的数字。而4个字节刚好对应一个uint32，也就是传入new Kcp(sid, m_handle)的sid。
            m_kcp = new Kcp(sid, m_handle);
            m_kcp.NoDelay(1, 10, 2, 1);
            m_kcp.WndSize(64, 64);
            m_kcp.SetMtu(512);

            m_handle.Out = (Memory<byte> buffer) =>
            {
                byte[] bytes = buffer.ToArray();
                m_udpSender(bytes, m_remotePoint);
            };

            m_handle.Recv = (byte[] buffer) =>
            {
                buffer = KCPTool.DeCompress(buffer);
                T msg = KCPTool.DeSerialize<T>(buffer);
                if (msg != null)
                {
                    OnReceiveMsg(msg);
                }
            };

            OnConnected();

            cts = new CancellationTokenSource();
            ct = cts.Token;
            Task.Run(Update,ct);
        }

        async void Update()
        {
            try
            {
                while (true)
                {
                    DateTime now = DateTime.UtcNow;
                    OnUpDate(now);
                    if (ct.IsCancellationRequested)
                    {
                        KCPTool.ColorLog(KCPLogColor.Cyan, "SessionUpdate Task is Cancelled.");
                        break;
                    }
                    else
                    {
                        m_kcp.Update(now);
                        int len;
                        while ((len = m_kcp.PeekSize()) > 0)
                        {
                            var buffer = new byte[len];
                            if (m_kcp.Recv(buffer) >= 0)
                            {
                                m_handle.Receive(buffer);
                            }
                        }

                        await Task.Delay(10);
                    }
                }
            }
            catch (Exception e)
            {
                KCPTool.Warn("Session Update Exception {0}", e.ToString());
            }
        }

        public void ReceiveData(byte[] buffer)
        {
            m_kcp.Input(buffer.AsSpan());
        }

        public void SendMsg(T msg)
        {
            if (IsConnected())
            {
                byte[] bytes = KCPTool.Serialize(msg);
                if (bytes != null)
                {
                    SendMsg(bytes);
                }
            }
            else
            {
                KCPTool.Warn("Session Disconnected.Cannot send Msg.");
            }
        }

        public void SendMsg(byte[] bytes)
        {
            if (IsConnected())
            {
                bytes = KCPTool.Compress(bytes);
                m_kcp.Send(bytes.AsSpan());
            }
            else
            {
                KCPTool.Warn("Session Disconnected.Cannot send Msg.");
            }
        }

        public void CloseSession()
        {
            OnDisConnected();
            OnSessionClose?.Invoke(m_sessionId);

            m_sessionState = SessionState.DisConnected;
            OnSessionClose = null;
            m_remotePoint = null;
            m_udpSender = null;
            m_handle = null;
            m_kcp = null;
            m_sessionId = 0;

            cts.Cancel();
        }

        #region API
        public bool IsConnected()
        {
            return m_sessionState == SessionState.Connected;
        }

        public override bool Equals(object obj)
        {
            if (obj is KCPSession<T> us)
            {
                return us.m_sessionId == m_sessionId;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return m_sessionId.GetHashCode();
        }

        public uint GetSessionID()
        {
            return m_sessionId;
        }
        #endregion
    }

    public enum SessionState
    {
        None,
        Connected,
        DisConnected,
    }
}
