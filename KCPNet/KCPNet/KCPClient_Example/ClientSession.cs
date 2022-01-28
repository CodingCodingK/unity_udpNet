using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KCPProtocol_Example;
using PENet;

namespace KCPClient_Example
{
    /// <summary>
    /// 客户端Session连接 : KCPSession<数据协议>
    /// </summary>
    public class ClientSession : KCPSession<NetMsg>
    {
        protected override void OnDisConnected()
        {
            
        }

        protected override void OnConnected()
        {
            
        }

        protected override void OnUpDate(DateTime now)
        {
            
        }

        protected override void OnReceiveMsg(NetMsg msg)
        {
            KCPTool.ColorLog(KCPLogColor.Magenta, "From Server:Sid:{0}, Msg:{1}",m_sessionId,msg.info);
        }
    }
}
