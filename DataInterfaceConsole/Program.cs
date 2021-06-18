using DataInterfaceConsole.Actions;
using DataInterfaceConsole.Types;
using DataInterfaceConsole.Types.Exceptions;
using FiveDChessDataInterface;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace DataInterfaceConsole
{
    class Program
    {
        internal static Program instance = new Program();
        private Thread backgroundThread;
        private DataInterface di;

        static void Main()
        {
            ConsoleNonBlocking.Init();
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            instance.Run();
        }

        private void Run()
        {
            Console.WriteLine("Some output will occasionally be provided via the console title.");

            this.backgroundThread = new Thread(BackgroundThreadRun)
            {
                Name = "BackgroundThread"
            };
            this.backgroundThread.Start();

            var actions = BaseAction.GetAndInstantiateAllActions().Select((x, i) => (i, x)).ToDictionary(a => a.i + 1, a => a.x);
            var idWidth = (int)Math.Log10(actions.Count) + 1;

            while (true)
            {
                Console.WriteLine("Please launch the game...");
                SpinWait.SpinUntil(() => this.di?.IsValid() == true); // wait till the datainterface instance is valid
                Console.WriteLine($"Select an action from the following (by typing the number to the left and pressing enter):\n" +
                    $"{string.Join("\n", actions.Select(a => $"[{a.Key.ToString().PadLeft(idWidth)}] {a.Value.Name}"))}");
                try
                {
                    while (true)
                    {
                        var choice = Util.ConsoleReadLineThrowIfDiInvalid(this.di);
                        if (int.TryParse(choice, out int res) && res > 0 && res <= actions.Count)
                        {
                            var a = actions[res];
                            Console.WriteLine($"Chosen action: {a.Name}");
                            try
                            {
                                a.Run(this.di);

                                Console.WriteLine("Action executed. Returning to menu...");
                                Thread.Sleep(1000);
                            }
                            catch (DataInterfaceClosedException ex)
                            {
                                Util.WriteColored($"Execution of the current action '{a.Name}' was aborted because the game closed or crashed: \n{ex}\n" +
                                    $"To continue, please press ENTER.", ConsoleColor.Red);
                                ConsoleNonBlocking.ClearInputLines();
                            }
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Invalid number entered. Please try again.");
                        }
                    }
                }
                catch (DataInterfaceClosedException ex)
                {
                    Util.WriteColored($"Execution of the current action selection was aborted because the game closed or crashed: \n{ex}\nTo continue, please press ENTER.", ConsoleColor.Red);
                    ConsoleNonBlocking.ClearInputLines();
                }
            }
        }

        private void BackgroundThreadRun()
        {
            while (true)
            {
                bool tooManyProcesses = false;
                if (this.di?.IsValid() != true)
                {
                    if (!DataInterface.TryCreateAutomatically(out this.di, out int procCnt))
                    {
                        if (procCnt > 1)
                        {
                            tooManyProcesses = true;
                        }
                    }
                }

                if (tooManyProcesses)
                {
                    SetConsoleTitleWithPrefix($"Too many game instances found!");
                }
                else
                {
                    var isValid = this.di?.IsValid();
                    string gameStatus = isValid switch
                    {
                        true => $"Running - ProcessId: {this.di.GameProcess.Id}",
                        false => "closed",
                        null => "Not found"
                    };

                    SetConsoleTitleWithPrefix($"GameProcess: {gameStatus}");
                }

                Thread.Sleep(500);
            }
        }

        private void SetConsoleTitleWithPrefix(string s)
        {
            Console.Title = $"5D Data Interface Console - {s}";
        }
    }
}
