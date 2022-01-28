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
    /// 控制台模拟客户端
    /// </summary>
    class ClientStart
    {
        public const string ip = "192.168.1.13";
        public const int port = 17666;

        static KCPNet<ClientSession, NetMsg> client;
        static Task<bool> checkTask;
        
        static void Main(string[] args)
        {
            client = new KCPNet<ClientSession, NetMsg>();
            client.StartAsClient(ip,port);
            checkTask = client.ConnectServer(200,5000);

            Task.Run(ConnectCheck);

            while (true)
            {
                string input = Console.ReadLine();
                if (input == "quit")
                {
                    client.CloseClient();
                    break;
                }
                else
                {
                    client.clientSession.SendMsg(new NetMsg
                    {
                        info = input,
                    });

                }
            }

            Console.ReadKey();
        }

        private static int linkCounter;
        static async Task ConnectCheck()
        {
            while (true)
            {
                await Task.Delay(3000);
                if (checkTask != null && checkTask.IsCompleted)
                {
                    if (checkTask.Result)
                    {
                        KCPTool.ColorLog(KCPLogColor.Green, "ConnectServer Success.");
                        checkTask = null;
                        await Task.Run(SendPingMsg);
                    }
                    else
                    {
                        if (++linkCounter > 4)
                        {
                            KCPTool.Error("Connect failed too many times, Check your Network.");
                            checkTask = null;
                            break;
                        }
                        else
                        {
                            KCPTool.Error($"Connect failed {linkCounter} Times, Reconnecting...");
                            checkTask = client.ConnectServer(200,500);
                        }
                    }
                }

            }
        }

        static async Task SendPingMsg()
        {
            while (true)
            {
                await Task.Delay(5000);
                if (client?.clientSession != null)
                {
                    client.clientSession.SendMsg(new NetMsg
                    {
                        cmd = CMD.Ping,
                        ping = new Ping
                        {
                            isOver = false,
                        }
                    });

                    KCPTool.ColorLog(KCPLogColor.Green, "Client Send Ping Msg.");
                }
                else
                {
                    KCPTool.ColorLog(KCPLogColor.Green, "Ping Task Cancel.");
                    break;
                }
            }
        }
    }
}
