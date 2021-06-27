using DataInterfaceConsole.Types.Exceptions;
using FiveDChessDataInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DataInterfaceConsole.Actions
{
    internal abstract class BaseAction
    {
        public abstract string Name { get; }
        protected DataInterface di;

        public static BaseAction[] GetAndInstantiateAllActions() => typeof(BaseAction).Assembly.GetTypes()
            .Where(x => typeof(BaseAction).IsAssignableFrom(x) && typeof(BaseAction) != x)
            .Select(t => (BaseAction)Activator.CreateInstance(t))
            .ToArray();

        public void Run(DataInterface di)
        {
            this.di = di ?? throw new ArgumentNullException(nameof(di));
            Run();
        }

        protected abstract void Run();

        /// <summary>
        /// Raises an exception which terminates the currently executing <see cref="BaseAction"/>, if the datainterface has become invalid, likely due to a game crash.
        /// </summary>
        protected void AbortIfDiInvalid()
        {
            if (!this.di.IsValid())
                throw new DataInterfaceClosedException("The DataInterface instance is not, or no longer, valid.");
        }

        protected string ConsoleReadLineWhileDiValid() => Util.ConsoleReadLineWhileDiValid(this.di);

        protected void WaitForIngame()
        {
            bool shown = false;
            while (!this.di.IsGameRunning())
            {
                if (!shown)
                {
                    Console.WriteLine("Waiting for a match to be started.");
                    shown = true;
                }
                AbortIfDiInvalid();
                Thread.Sleep(150);
            }
            Console.WriteLine("A match has started. Continuing...");
        }

        protected void WriteLineIndented(string s, int level = 1)
        {
            Console.WriteLine($"{new string(' ', level * 2)}{s}");
        }
        protected void WriteLineIndented(IEnumerable<string> arr, int level = 1)
        {
            Console.WriteLine(string.Join("\n", arr.Select(s => $"{new string(' ', level * 2)}{s}")));
        }
    }
}
