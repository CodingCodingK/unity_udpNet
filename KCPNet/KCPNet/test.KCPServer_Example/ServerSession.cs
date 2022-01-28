using System;
using System.Collections.Generic;
using System.Text;
using KCPProtocol_Example;
using PENet;

namespace KCPServer_Example
{
    /// <summary>
    /// 服务端Session连接 : KCPSession<数据协议>
    /// </summary>
    public class ServerSession : KCPSession<NetMsg>
    {
        protected override void OnDisConnected()
        {
            KCPTool.Warn("Client Offline {0}", m_sessionId);
        }

        protected override void OnConnected()
        {
            KCPTool.ColorLog(KCPLogColor.Green,"Client Online {0}",m_sessionId);
        }

        private int checkCounter;
        private DateTime checkTime = DateTime.UtcNow.AddSeconds(5);
        protected override void OnUpDate(DateTime now)
        {
            if (now > checkTime)
            {
                checkTime = now.AddSeconds(5);
                checkCounter++;
                if (checkCounter > 3)
                {
                    NetMsg pingMsg = new NetMsg
                    {
                        cmd = CMD.Ping,
                        ping = new Ping {isOver = false},
                    };
                    // 3次心跳检测超时，本地模拟关闭消息
                    OnReceiveMsg(pingMsg);
                }
            }
        }

        protected override void OnReceiveMsg(NetMsg msg)
        {
            KCPTool.ColorLog(KCPLogColor.Magenta, "Get Msg from client. Sid:{0}, CMD:{1}, Msg:{2}", m_sessionId, msg.cmd, msg.info);

            if (msg.cmd == CMD.Ping)
            {
                if (msg.ping.isOver)
                {
                    // TODO ??
                    CloseSession();
                }
                else
                {
                    // 收到ping请求，重置检查计数
                    checkCounter = 0;
                    var pingMsg = new NetMsg
                    {
                        cmd = CMD.Ping,
                        ping = new Ping {isOver = false}
                    };
                    SendMsg(pingMsg);
                }
            }
        }
    }
}
