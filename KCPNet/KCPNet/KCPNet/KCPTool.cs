using System;
using System.Collections.Generic;
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
