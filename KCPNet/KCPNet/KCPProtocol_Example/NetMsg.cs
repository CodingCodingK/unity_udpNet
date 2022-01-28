using System;
using PENet;

namespace KCPProtocol_Example
{
    /// <summary>
    /// 网络通信数据协议
    /// </summary>
    [Serializable]
    public class NetMsg : KCPMsg
    {
        public string info;
        public CMD cmd;
        public Ping ping;
        public ReqLogin reqLogin;
    }

    /// <summary>
    /// 心跳机制
    /// </summary>
    [Serializable]
    public class Ping
    {
        public bool isOver;
    }

    #region 业务数据

    [Serializable]
    public class ReqLogin
    {
        public string acct;
        public string psd;
    }

    #endregion

    /// <summary>
    /// 本数据包传递的 业务类型
    /// </summary>
    public enum CMD
    {
        None,
        Ping,
        ReqLogin,
    }
}
