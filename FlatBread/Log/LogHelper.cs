using System;
using System.Collections.Generic;
using System.Text;

namespace FlatBread.Log
{
    /// <summary>
    /// 控制台日志方案
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// 信息日志
        /// </summary>
        /// <param name="Message"></param>
        public static void LogInfo(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[INFO]:{Message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="Message"></param>
        public static void LogWarn(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[WARN]:{Message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="Message"></param>
        public static void LogError(string Message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR]:{Message}");
            Console.ForegroundColor = ConsoleColor.White;
        }

    }
}
