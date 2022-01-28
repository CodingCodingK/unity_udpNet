using System;
using System.Threading.Tasks;
using KCPProtocol_Example;
using PENet;

namespace KCPServer_Example
{
    /// <summary>
    /// 控制台模拟服务端
    /// </summary>
    class ServerStart
    {
        public const string ip = "192.168.1.13";
        public const int port = 17666;

        static KCPNet<ServerSession, NetMsg> server;

        static void Main(string[] args)
        {
            server = new KCPNet<ServerSession, NetMsg>();
            server.StartAsServer(ip, port);

            while (true)
            {
                string input = Console.ReadLine();
                if (input == "quit")
                {
                    server.CloseServer();
                    break;
                }
                else
                {
                    server.BroadCastMsg(new NetMsg {info = input});
                }
            }

            Console.ReadKey();
        }
    }
}
