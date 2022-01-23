using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UDP_Server
{
    public class ServerStart
    {
        private const int port = 17333;
        static void Main(string[] args)
        {
            CreateUDPServer();

            Console.Read();
        }

        static void CreateUDPServer()
        {
            UDPListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            Console.WriteLine("Waiting for connection...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Connection accepted.");
                NetworkStream ns = client.GetStream();
                byte[] data = Encoding.ASCII.GetBytes("hello. (from server to client by tcp-ip)");
                try
                {
                    ns.Write(data, 0, data.Length);
                    ns.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine("error:" + e);
                }
            }
        }
    }
}
