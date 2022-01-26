using System;
using System.Net;
using System.Net.Sockets.Kcp;

namespace PENet
{
    public abstract class KCPSession
    {
        protected uint m_sessionId;
        private IPEndPoint m_remotePoint;
        protected SessionState m_sessionState = SessionState.None;
        private Action<byte[], IPEndPoint> m_udpSender;

        public KCPHandle m_handle;
        public Kcp kcp;

        public void InitSession(uint sid, Action<byte[], IPEndPoint> udpSender, IPEndPoint remotePoint)
        {
            this.m_sessionId = sid;
            this.m_udpSender = udpSender;
            this.m_handle = new KCPHandle();
            this.m_remotePoint = remotePoint;
            this.m_sessionState = SessionState.Connected;

            // sid = kcp添加控制信息的包里，头4个字节的数字对应传入的int值，每一个字节可以转化为对应的0~255的数字。而4个字节刚好对应一个uint32，也就是传入new Kcp(sid, m_handle)的sid。
            kcp = new Kcp(sid, m_handle);
            kcp.NoDelay(1, 10, 2, 1);
            kcp.WndSize(64, 64);
            kcp.SetMtu(512);

            m_handle.Out = (Memory<byte> buffer) =>
            {
                byte[] bytes = buffer.ToArray();
                m_udpSender(bytes, m_remotePoint);
            };
        }

        public bool IsConnected()
        {
            return m_sessionState == SessionState.Connected;
        }
    }

    public enum SessionState
    {
        None,
        Connected,
        DisConnected,
    }
}
