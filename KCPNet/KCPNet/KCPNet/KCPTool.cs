using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace PENet
{
    public class KCPTool
    {
        #region For Unity
        public static Action<string> LogFunc;
        public static Action<string> WarnFunc;
        public static Action<string> ErrorFunc;
        public static Action<KCPLogColor, string> ColorLogFunc;
        #endregion

        public static void Log(string msg,params object[] args)
        {
            msg = string.Format(msg, args);
            if (LogFunc != null)
            {
                LogFunc.Invoke(msg);
            }
            else
            {
                ConsoleLog(msg,KCPLogColor.None);
            }
        }

        public static void Warn(string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (WarnFunc != null)
            {
                WarnFunc.Invoke(msg);
            }
            else
            {
                ConsoleLog(msg, KCPLogColor.Yellow);
            }
        }
        public static void Error(string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (ErrorFunc != null)
            {
                ErrorFunc.Invoke(msg);
            }
            else
            {
                ConsoleLog(msg, KCPLogColor.Red);
            }
        }

        public static void ColorLog(KCPLogColor color, string msg, params object[] args)
        {
            msg = string.Format(msg, args);
            if (ColorLogFunc != null)
            {
                ColorLogFunc.Invoke(color, msg);
            }
            else
            {
                ConsoleLog(msg, color);
            }
        }


        private static void ConsoleLog(string msg, KCPLogColor color)
        {
            int tid = Thread.CurrentThread.ManagedThreadId;
            msg = string.Format("Thread:{0} {1}", tid, msg);

            switch (color)
            {
                case KCPLogColor.Red:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case KCPLogColor.Green:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case KCPLogColor.Blue:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case KCPLogColor.Cyan:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case KCPLogColor.Magenta:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case KCPLogColor.Yellow:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case KCPLogColor.None:
                default:
                    break;

            }

            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static byte[] Serialize<T>(T msg) where T : KCPMsg
        {
            using (MemoryStream ms = new MemoryStream())
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(ms, msg);
                    ms.Seek(0, SeekOrigin.Begin);
                    return ms.ToArray();
                }
                catch(SerializationException se)
                {
                    Error("Failed to serialized: {0}", se.Message);
                    throw se;
                }
            }
        }

        public static T DeSerialize<T>(byte[] bytes) where T : KCPMsg
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    T msg = (T)bf.Deserialize(ms);
                    return msg;
                }
                catch (Exception e)
                {
                    Error("Failed to Deserialized: {0}. bytesLength: {1}", e.Message, bytes.Length);
                    throw e;
                }
            }
        }

        public static byte[] Compress(byte[] input)
        {
            using (MemoryStream outMS = new MemoryStream())
            {
                using (GZipStream gzs = new GZipStream(outMS, CompressionMode.Compress, true))
                {
                    gzs.Write(input, 0, input.Length);
                    gzs.Close();
                    return outMS.ToArray();
                }
            }
        }

        public static byte[] DeCompress(byte[] input)
        {
            using (MemoryStream inputMS = new MemoryStream(input))
            {
                using (MemoryStream outputMS = new MemoryStream())
                {
                    using (GZipStream gzs = new GZipStream(inputMS, CompressionMode.Decompress))
                    {
                        byte[] bytes = new byte[1024];
                        int len = 0;
                        while ((len = gzs.Read(bytes,0,bytes.Length)) > 0)
                        {
                            outputMS.Write(bytes, 0, len);
                        }
                        
                        gzs.Close();
                        return outputMS.ToArray();
                    }
                }
            }
        }

        private static readonly DateTime utcStart = new DateTime(1970, 1, 1);
        public static ulong GetUTCStartMilliseconds()
        {
            return (ulong) (DateTime.UtcNow - utcStart).TotalMilliseconds;
        }
    }

    public enum KCPLogColor
    {
        None,
        Red,
        Green,
        Blue,
        Cyan,
        Magenta,
        Yellow
    }
}
