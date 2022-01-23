using System;
using System.Net.Sockets;
using System.Text;

namespace UDP_Client
{
    class ClientStart
    {
        private const int port = 17333;
        // 本地IP
        private const string ip = "127.0.0.1";

        static void Main(string[] args)
        {
            CreateUDPClient();

            Console.Read();
        }

        static void CreateUDPClient()
        {
            try
            {
                var client = new TcpClient(ip, port);
                NetworkStream ns = client.GetStream();
                byte[] data = new byte[1024];
                int len = ns.Read(data, 0, data.Length);
                Console.WriteLine(Encoding.ASCII.GetString(data, 0, len));
            }
            catch (Exception e)
            {
                Console.WriteLine("error:" + e);
            }
        }
    }
}
