using DataInterfaceConsole.Types.Exceptions;
using FiveDChessDataInterface;
using System;
using System.Linq;

namespace DataInterfaceConsole.Actions
{
    internal abstract class BaseAction
    {
        public abstract string Name { get; }
        protected DataInterface di;

        public static BaseAction[] GetAndInstantiateAllActions() => typeof(BaseAction).Assembly.GetTypes()
            .Where(x=>typeof(BaseAction).IsAssignableFrom(x) && typeof(BaseAction) != x)
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
    }
}
