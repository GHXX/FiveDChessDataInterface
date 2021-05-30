using DataInterfaceConsoleTest.Examples;
using FiveDChessDataInterface;
using System;
using System.Reflection;
using System.Threading;
using static DataInterfaceConsoleTest.Examples.CallableExMethodAttribute;

namespace DataInterfaceConsoleTest
{
    class Program
    {
        static void Main()
        {
            while (true)
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

                ShowInfoAndRunExamples(di);
                Console.WriteLine("Game was closed, or died. Rescanning...");
            }
        }

        private static void ShowInfoAndRunExamples(DataInterface di)
        {
            Console.WriteLine($"The pointer to the chessboards is located at: 0x{di.MemLocChessArrayPointer.Location.ToString("X16")}");
            Console.WriteLine($"The chessboard array size is located at: 0x{di.MemLocChessArrayElementCount.Location.ToString("X16")}");
            Console.WriteLine($"The chessboard sizes width and height are located at 0x{di.MemLocChessBoardSizeWidth.Location.ToString("X16")} and 0x{di.MemLocChessBoardSizeHeight.Location.ToString("X16")}");
            Console.WriteLine($"Current chessboard count: {di.MemLocChessArrayElementCount.GetValue()}; Current chessboard array capacity: {di.MemLocChessArrayCapacity.GetValue()}");

            Console.WriteLine($"The current turn is stored at: 0x{di.MemLocCurrentPlayersTurn.Location.ToString("X16")}");
            Console.WriteLine($"Currently it's {(di.GetCurrentPlayersTurn() == 0 ? "WHITE's" : "BLACK's")} turn!");


            var chessboardLocation = di.MemLocChessArrayPointer.GetValue();

            Console.WriteLine($"The chessboards are currently located at: 0x{chessboardLocation.ToString("X16")}");

            InvokeEnabledExamples(di, InvokeKind.Startup);



            var previousPlayersTurn = di.GetCurrentPlayersTurn();

            var initialBoardCnt = di.GetChessBoardAmount();
            bool matchIsUnstarted = initialBoardCnt == 0;
            int previousBoardCount = initialBoardCnt;


            while (!di.GameProcess.HasExited)
            {
                var cnt = di.GetChessBoardAmount();
                var currPlayersTurn = di.GetCurrentPlayersTurn();

                if (matchIsUnstarted && cnt > 0) // a new match was started
                {
                    matchIsUnstarted = false;
                    InvokeEnabledExamples(di, InvokeKind.MatchStart);
                }

                if (previousPlayersTurn != currPlayersTurn && cnt > 0) // current turn changed (submit was pressed), and there are any chessboards
                {
                    previousPlayersTurn = currPlayersTurn;
                    InvokeEnabledExamples(di, InvokeKind.TurnChange);
                }

                if (previousBoardCount != cnt) // the board count changed
                {
                    if (previousBoardCount != 0) // if the boardCount was not zero before this means that the game was not just started
                    {
                        if (cnt > 0) // if there are boards right now, that means that the board count changed, but is not zero and wasnt zero before -> a board was added or removed
                            InvokeEnabledExamples(di, InvokeKind.BoardCountChanged);
                        else if (cnt == 0)// otherwise the count now is zero, meaning the board count changed from nonzero to zero, meaning the match was exited
                        {
                            InvokeEnabledExamples(di, InvokeKind.MatchExited);
                            matchIsUnstarted = true;
                        }
                    }

                    previousBoardCount = cnt;
                }
                Thread.Sleep(500);
            }
        }

        internal static void WriteConsoleColored(string text, ConsoleColor foreground, ConsoleColor background)
        {
            var fOld = Console.ForegroundColor;
            var bOld = Console.BackgroundColor;


            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;

            Console.Write(text);


            Console.ForegroundColor = fOld;
            Console.BackgroundColor = bOld;

        }

        private static void InvokeEnabledExamples(DataInterface di, InvokeKind ik)
        {
            var callableMethods = featureToggleSnippetss.GetEnabledMethods();
            foreach (var method in callableMethods)
            {
                var attrib = method.GetCustomAttribute<CallableExMethodAttribute>();
                if (attrib.Enabled && attrib.WhenToCall.HasFlag(ik))
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length == 0)
                        method.Invoke(null, null);
                    else if (parameters.Length == 1)
                        if (parameters[0].ParameterType == typeof(DataInterface))
                            method.Invoke(null, new object[] { di });
                        else
                            throw new TargetException($"The target method has an invalid signature. Make sure it only has one argument of the type {nameof(DataInterface)}");

                    else
                        throw new TargetException("Invoking a example-snippet-method with more than one parameter is not used and thus not supported currently.");
                }
            }
        }
    }
}
