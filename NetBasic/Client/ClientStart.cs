using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class ClientStart
    {
        private const int port = 17333;
        // 本地IP
        private const string ip = "127.0.0.1";

        static void Main(string[] args)
        {
            // CreateTCPClient();
            CreateUDPClient();

            Console.Read();
        }

        static void CreateTCPClient()
        {
            try
            {
                var client = new TcpClient(ip, port);
                NetworkStream ns = client.GetStream();
                byte[] data = new byte[1024];
                int len = ns.Read(data, 0, data.Length);
                Console.WriteLine(Encoding.ASCII.GetString(data,0,len));
            }
            catch (Exception e)
            {
                Console.WriteLine("error:" + e);
            }
        }

        static void CreateUDPClient()
        {
            try
            {
                // 不使用port，随机分配一个未使用的端口来发送。
                UdpClient client = new UdpClient();
                IPEndPoint remoteIP = new IPEndPoint(IPAddress.Parse(ip), port);
                byte[] data = Encoding.ASCII.GetBytes("hello. (from server to client by udp)");
                client.Send(data, data.Length,remoteIP);
                Console.WriteLine("msg has been send.");
            }
            catch (Exception e)
            {
                Console.WriteLine("error:" + e);
            }
        }
    }
}
