using System;
using System.Net.Sockets.Kcp;
using System.Text;
using System.Threading;

namespace KCPTest
{
    class KCPTestStart
    {
        private const uint conv = 256;
        static void Main(string[] args)
        {
            KCPItem kcpServer = new KCPItem(conv,"server");
            KCPItem kcpClient = new KCPItem(conv,"client");

            Random rd = new Random();

            // -----------省略了UDP通信，直接设置-----------
            kcpServer.SetOutCallback((Memory<byte> buffer) =>
            {
                kcpClient.InputData(buffer.Span);
            });

            kcpClient.SetOutCallback((Memory<byte> buffer) =>
            {
                // 模拟丢包
                int next = rd.Next(100);
                if (next >= 10)
                {
                    Console.WriteLine($"Send Pkg Succ:{GetByteString(buffer.ToArray())}");
                    kcpServer.InputData(buffer.Span);
                }
                else
                {
                    Console.WriteLine("Send Pkg Miss");
                }
            });

            byte[] data = Encoding.ASCII.GetBytes("TEST");
            kcpClient.SendMsg(data);

            while (true)
            {
                kcpServer.Update();
                kcpClient.Update();
                Thread.Sleep(10);
            }
        }

        static string GetByteString(byte[] bytes)
        {
            string str = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                str += string.Format($"\n    [{i}]:{bytes[i]}");
            }
            return str;
        }
    }
}
