﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuGiOhBot.Core
{
    public static class AltConsole
    {

        public static async Task PrintAsync(string firstBracket, string secondBracket, string message, Exception exception = null)
        {

            Console.ForegroundColor = ConsoleColor.Red;
            await Console.Out.WriteAsync($"{DateTime.Now.ToString()} [{firstBracket}]");
            Console.ForegroundColor = ConsoleColor.Green;
            await Console.Out.WriteAsync($"[{secondBracket}] ");
            Console.ForegroundColor = ConsoleColor.Gray;

            if (exception == null)
            {

                await Console.Out.WriteLineAsync($"{message}");

            }
            else
            {

                await Console.Out.WriteLineAsync($"{message}\t\t{exception}");

            }

        }

    }
}