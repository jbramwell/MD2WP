using System;

namespace MD2WP.Shared.Helpers
{
    public class Logger
    {
        public static bool DebugMode { get; set; }

        public static void LogMessage(string message)
        {
            if (DebugMode)
            {
                Console.WriteLine($"{message}");
            }
        }

        public static void LogError(string message)
        {
            Console.WriteLine($"ERROR: {message}");
        }
    }
}