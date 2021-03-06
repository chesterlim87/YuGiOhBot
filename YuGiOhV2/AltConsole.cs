﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhV2
{
    public static class AltConsole
    {
        
        private static StreamWriter _logger;
        private static object _loggerLock;

        public static void Initialize()
        {

            _loggerLock = new object();

            if (File.Exists("Log.txt"))
                File.Delete("Log.txt");

            _logger = new StreamWriter("Log.txt", true)
            {
                AutoFlush = false
            };

        }

        public static void Print(string firstBracket, string secondBracket, string message, bool log)
            => Print(firstBracket, secondBracket, message, false, null);

        public static void Print(string firstBracket, string secondBracket, string message, Exception exception = null)
            => Print(firstBracket, secondBracket, message, true, exception);

        public static void Print(string firstBracket, string secondBracket, string message, bool log, Exception exception = null)
        {

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{DateTime.Now.ToString()} ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"[{firstBracket}]");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"[{secondBracket}] ");
            Console.ForegroundColor = ConsoleColor.Gray;

            if (exception == null)
            {

                Console.WriteLine($"{message}");

            }
            else
            {

                Console.WriteLine($"{message}\t\t{exception}");

            }

            if(log)
                Log(message);

        }

        public static void InlinePrint(string firstBracket, string secondBracket, string message, bool log)
            => InlinePrint(firstBracket, secondBracket, message, log, null);

        public static void InlinePrint(string firstBracket, string secondBracket, string message, bool log, Exception exception = null)
        {

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{DateTime.Now.ToString()} ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"[{firstBracket}]");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"[{secondBracket}] ");
            Console.ForegroundColor = ConsoleColor.Gray;

            if (exception == null)
            {

                Console.Write($"{message}\r");

            }
            else
            {

                Console.WriteLine($"{message}\t\t{exception}");

            }
            
            if(log)
                Log(message);

        }

        private static void Log(string message)
        {

            lock (_loggerLock)
            {

                _logger.WriteLine($"{message}");
                _logger.Flush();

            }

        }

    }
}
