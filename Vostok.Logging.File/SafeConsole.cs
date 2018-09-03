﻿using System;

namespace Vostok.Logging.File
{
    internal static class SafeConsole
    {
        public static void ReportError(string message, Exception error)
        {
            try
            {
                Console.Out.Write(message);
                Console.Out.Write(error);
            }
            catch
            {
                // ignored
            }
        }
    }
}