using DataInterfaceConsole.Actions;
using DataInterfaceConsole.Actions.EphemeralSettings;
using DataInterfaceConsole.Actions.Settings;
using DataInterfaceConsole.Types;
using DataInterfaceConsole.Types.Exceptions;
using FiveDChessDataInterface;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace DataInterfaceConsole;

internal class Program {
    internal static Program instance = new Program();
    private Thread backgroundThread;
    public DataInterface di;
    public PersistentSettingsContainer sh;
    public EphemeralSettingsContainer eh;

    private static void Main() {
        ConsoleNonBlocking.Init();
        Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        instance.Run();
    }

    private void Run() {
        Console.WriteLine("Some output will occasionally be provided via the console title.");
        this.sh = PersistentSettingsContainer.LoadOrCreateNew();
        this.eh = new EphemeralSettingsContainer();

        this.backgroundThread = new Thread(BackgroundThreadRun) {
            Name = "BackgroundThread"
        };
        this.backgroundThread.Start();

        var actions = BaseAction.GetAndInstantiateAllActions().Select((x, i) => (i, x)).ToDictionary(a => a.i + 1, a => a.x);
        var idWidth = (int)Math.Log10(actions.Count) + 1;

        Thread.Sleep(200);
        while (true) {
            if (this.di?.IsValid() != true)
                Console.WriteLine("Please launch the game...");

            SpinWait.SpinUntil(() => this.di?.IsValid() == true); // wait till the datainterface instance is valid
            Console.WriteLine($"Select an action from the following (by typing the number to the left and pressing enter):\n" +
                $"{string.Join("\n", actions.Select(a => $"[{a.Key.ToString().PadLeft(idWidth)}] {a.Value.Name}"))}");
            try {
                while (true) {
                    var choice = Util.ConsoleReadLineThrowIfDiInvalid(this.di);
                    if (int.TryParse(choice, out int res) && res > 0 && res <= actions.Count) {
                        var a = actions[res];
                        var header = $"====== Executing action: {a.Name} ======";
                        Console.WriteLine(header);
                        try {
                            a.Run(this.di);

                            Console.WriteLine($"Action executed. Returning to menu...\n{new string('=', header.Length)}");
                            Thread.Sleep(1000);
                        } catch (DataInterfaceClosedException ex) {
                            Util.WriteColored($"Execution of the current action '{a.Name}' was aborted because the game closed or crashed: \n{ex.ToSanitizedString()}", ConsoleColor.Red);
                            ConsoleNonBlocking.ClearInputLines();
                        }
                        break;
                    } else {
                        Console.WriteLine("Invalid number entered. Please try again.");
                    }
                }
            } catch (DataInterfaceClosedException ex) {
                Util.WriteColored($"Execution of the current action selection was aborted because the game closed or crashed: \n{ex.ToSanitizedString()}", ConsoleColor.Red);
                ConsoleNonBlocking.ClearInputLines();
            }
        }
    }

    private void BackgroundThreadRun() {
        while (true) {
            bool tooManyProcesses = false;
            if (this.di?.IsValid() != true) {
                if (!DataInterface.TryCreateAutomatically(out this.di, out int procCnt)) {
                    if (procCnt > 1) {
                        tooManyProcesses = true;
                    }
                } else {
                    this.di.Initialize();
                }
            }

            if (tooManyProcesses) {
                SetConsoleTitleWithPrefix($"Too many game instances found!");
            } else {
                var isValid = this.di?.IsValid();
                string gameStatus = isValid switch {
                    true => $"Running - ProcessId: {this.di.GameProcess.Id}",
                    false => "closed",
                    null => "Not found"
                };

                SetConsoleTitleWithPrefix($"GameProcess: {gameStatus}");
            }

            this.sh.Tick();

            Thread.Sleep(500);
        }
    }

    private void SetConsoleTitleWithPrefix(string s) {
        Console.Title = $"5D Data Interface Console - {s}";
    }
}
