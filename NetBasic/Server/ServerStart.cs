using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class ServerStart
    {
        private const int port = 17333;
        static void Main(string[] args)
        {
            // CreateTCPServer();
            CreateUDPServer();

            Console.Read();
        }

        static void CreateTCPServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            Console.WriteLine("Waiting for connection...");

            while (true)
            {
                // 每一个client都对应一个server
                // 封装得更复杂
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Connection accepted.");
                NetworkStream ns = client.GetStream();
                byte[] data = Encoding.ASCII.GetBytes("hello. (from server to client by tcp)");
                try
                {
                    ns.Write(data, 0, data.Length);
                    ns.Close();
                }
                catch(Exception e)
                {
                    Console.WriteLine("error:" + e);
                }
            }
        }

        static void CreateUDPServer()
        {
            // 只建立一次server，所有客户端请求都接受
            UdpClient listener = new UdpClient(port);
            IPEndPoint remoteIP = new IPEndPoint(IPAddress.Any, port);

            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for connection...");
                    byte[] bytes = listener.Receive(ref remoteIP);
                    Console.WriteLine($"Received msg from {remoteIP}");
                    Console.WriteLine($"{Encoding.ASCII.GetString(bytes, 0, bytes.Length)}");

                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error:" + e);
            }
            finally
            {
                listener.Close();
            }
        }
    }
}
