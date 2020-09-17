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
    }
}
