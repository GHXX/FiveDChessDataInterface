using FiveDChessDataInterface;
using System;
using System.Threading;


namespace DataInterfaceConsoleTest
{
    class Program
    {
        static void Main()
        {
            DataInterface di;
            while (!DataInterface.TryCreateAutomatically(out di, out int numberOfProcesses))
            {
                Thread.Sleep(1000);
                Console.WriteLine("Current number of processes: " + numberOfProcesses);
            }

            Console.WriteLine("Process found. Initializing...");
            di.Initialize();
            Console.WriteLine("Ready!");

            DoDataDump(di);
            
            const int pollingIntervalMs = 10;

            var lastPlayer = -1;
            bool gameRunning = false;
            while (true)
            {
                while (di.GetChessBoardAmount() > 0)
                {
                    if (!gameRunning)
                    {
                        Console.WriteLine("Game has started!");
                        gameRunning = true;
                    }

                    var cp = di.GetCurrentPlayersTurn();
                    if (cp >= 0 && lastPlayer != cp) // if its any players turn, and the player changed
                    {
                        Console.WriteLine($"It's now {(cp == 0 ? "WHITE" : "BLACK")}'s turn!");
                        lastPlayer = cp;
                    }
                    Thread.Sleep(pollingIntervalMs);
                }

                if (gameRunning)
                {
                    Console.WriteLine("Game has ended!");
                    gameRunning = false;
                }

                Thread.Sleep(pollingIntervalMs);
            }
        }

        private static void DoDataDump(DataInterface di)
        {
            Console.WriteLine($"The pointer to the chessboards is located at: 0x{di.MemLocChessArrayPointer.Location.ToString("X16")}");
            Console.WriteLine($"The chessboard array size is located at: 0x{di.MemLocChessArraySize.Location.ToString("X16")}");
            Console.WriteLine($"The chessboard sizes width and height are located at 0x{di.MemLocChessBoardSizeWidth.Location.ToString("X16")} and 0x{di.MemLocChessBoardSizeHeight.Location.ToString("X16")}");

            Console.WriteLine($"The current turn is stored at: 0x{di.MemLocCurrentPlayersTurn.Location.ToString("X16")}");
            Console.WriteLine($"Currently it's {(di.GetCurrentPlayersTurn() == 0 ? "WHITE's" : "BLACK's")} turn!");


            var chessboardLocation = di.MemLocChessArrayPointer.GetValue();

            Console.WriteLine($"The chessboards are currently located at: 0x{chessboardLocation.ToString("X16")}");

            Console.WriteLine("Done!");
            Console.ReadLine();
            Environment.Exit(0);
        }
    }
}
