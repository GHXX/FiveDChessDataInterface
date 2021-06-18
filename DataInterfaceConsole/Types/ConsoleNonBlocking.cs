using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DataInterfaceConsole.Types
{
    internal static class ConsoleNonBlocking
    {
        private static readonly ConcurrentQueue<string> readLines = new ConcurrentQueue<string>();

        internal static void Init()
        {
            Task.Run(ReadLines);
        }

        private static async Task ReadLines()
        {
            while (true)
            {
                var s = new StreamReader(Console.OpenStandardInput(), Console.InputEncoding);
                var l = await s.ReadLineAsync();
                readLines.Enqueue(l);
            }
        }

        public static int Count => readLines.Count;
        public static bool TryPeek(out string val) => readLines.TryPeek(out val);
        public static bool TryDequeue(out string val) => readLines.TryDequeue(out val);

        public static string ReadLineBlocking()
        {
            string s = null;
            SpinWait.SpinUntil(() => TryDequeue(out s));
            return s;
        }

        public static void ClearInputLines() => readLines.Clear();
    }
}
