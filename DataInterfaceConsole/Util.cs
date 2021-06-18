using DataInterfaceConsole.Types;
using DataInterfaceConsole.Types.Exceptions;
using FiveDChessDataInterface;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DataInterfaceConsole
{
    static class Util
    {
        public static string ConsoleReadLineWhile(Func<bool> doRead)
        {
            while (doRead.Invoke())
            {
                if (ConsoleNonBlocking.TryDequeue(out string res))
                    return res;
                Thread.Sleep(150);
            }

            return null;
        }

        public static string ConsoleReadLineWhileDiValid(DataInterface di) => ConsoleReadLineWhile(() => di?.IsValid() == true);

        /// <summary>
        /// Reads a line from the console, blocking. Aborts if the specified <see cref="DataInterface"/> instance becomes invalid.
        /// </summary>
        /// <param name="di">The <see cref="DataInterface"/> instance to monitor.</param>
        /// <returns>The string that was read from the console input.</returns>
        /// <exception cref="DataInterfaceClosedException">if the datainterface instance becomes invalid while reading.</exception>
        public static string ConsoleReadLineThrowIfDiInvalid(DataInterface di)
        {
            var res = ConsoleReadLineWhileDiValid(di);
            if (res == null)
                throw new DataInterfaceClosedException("Reading the console was interrupted because the datainterface instance became invalid!");

            return res;
        }

        private static readonly object cwlock = new object();
        public static void WriteColored(string s, ConsoleColor c)
        {
            lock (cwlock)
            {
                var oldc = Console.ForegroundColor;
                Console.ForegroundColor = c;
                Console.WriteLine(s);
                Console.ForegroundColor = oldc;
            }
        }
    }
}
