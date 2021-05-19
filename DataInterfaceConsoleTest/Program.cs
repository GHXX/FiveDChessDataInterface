using FiveDChessDataInterface;
using System;
using System.Threading;
using System.Collections.Generic;

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

            //GameState? gs = null;
            //while (true)
            //{
            //    var ngs = di.GetCurrentGameState();
            //    if (ngs != gs)
            //    {
            //        Console.WriteLine($"Gamestate changed:  {gs} -> {ngs}");
            //        gs = ngs;
            //    }

            //    Thread.Sleep(100);
            //}

            var ccForegroundDefault = Console.ForegroundColor;
            var ccBackgoundDefault = Console.BackgroundColor;

            int oldCnt = -1;
            // infinite loop waiting for the numbers of boards to change
            while (true)
            {
                var cnt = di.GetChessBoardAmount();
                // if there is a different number of boards, the game has changed
                // so draw the boards again
                if (cnt != oldCnt)
                {
                    oldCnt = cnt;


                    var cbs = di.GetChessBoards();

                    Console.Clear();
                    Console.WriteLine($"Current chessboard ptr: {di.MemLocChessArrayPointer.ToString()}");
                    Console.WriteLine("Chessboards: \n");
                    foreach (ChessBoard b in cbs)
                    {
                        DrawBoard(b);
                    }
                }
                else
                {
                    Thread.Sleep(500);
                }
            }


            Console.WriteLine("Done!");
            Console.ReadLine();
            Environment.Exit(0);
        }

        /**
         * Draws the board, and called drawCBM which will check our hypothesis
         * for board memory behavior and print out all the board memory is any hypothesis is incorrect
         */
        private static void DrawBoard(ChessBoard board)
        {
            ChessBoardMemory cbm = board.cbm;
            
            Console.WriteLine($"Board: L{board.cbm.timeline:+#;-#;0}T{board.cbm.turn + 1}");
            DrawCBM(cbm);
            for (int y = board.height - 1; y >= 0; y--)
            {
                for (int x = 0; x < board.width; x++)
                {
                    var p = board.Pieces[x * board.width + y];

                    if (!p.IsEmpty)
                    {
                        WriteConsoleColored(p.SingleLetterNotation(), p.IsBlack ? ConsoleColor.White : ConsoleColor.Black, p.IsBlack ? ConsoleColor.Black : ConsoleColor.White);
                    }
                    else
                    {
                        WriteConsoleColored(" ", ConsoleColor.Gray, ConsoleColor.Gray);
                    }
                }
                Console.ResetColor();
                Console.WriteLine(" ");
            }
            Console.WriteLine();
        }

        /**
         * Check our hypothesis, if any are wrong then print out all the board data
         * otherwise just say we were correct
         */
        private static void DrawCBM(ChessBoardMemory cbm)
        {
            
            if (CheckCBMHypothesis(cbm))
            {
                Dictionary<string, int> fields = cbm.GetFieldsWithNames();

                foreach (KeyValuePair<string, int> pair in fields)
                {
                    Console.WriteLine($"{pair.Key} = {pair.Value}");
                    // Console.WriteLine("VALUE: " + pair.Value);
                }
            }
            else
            {
                Console.WriteLine("All Hypothesis Correct");
            }
            

        }

        /**
         * This is a function to check assumptions about how the board memory behaves
         * check ChessBoardMemory for more info on some of these values
         * some of the variable names are also a little messed up to do my own laziness
         * 
         * If all the hypothesis are correct this function returns false
         * if any hypothesis is wrong this function returns true
         */
        private static bool CheckCBMHypothesis(ChessBoardMemory cbm)
        {
            Dictionary<string, int> fields = cbm.GetFieldsWithNames();
            bool wrong_hypothesis = false;
            // if a move has been made on the board...
            fields.TryGetValue("move type", out int moveType);
            if (moveType > 0)
            {
                // standard physical move
                if (moveType == 1)
                {
                    // timeline and timeline number 2 are the same
                    fields.TryGetValue("timeline", out int timeline1);
                    fields.TryGetValue("move source universe", out int timeline2);
                    if (timeline1 != timeline2)
                    {
                        WriteConsoleColored("timeline and timeline number 2 are not equal!\n", ConsoleColor.Red, ConsoleColor.Black);
                        wrong_hypothesis = true;
                    }

                    // turn and time position 2 are the same
                    fields.TryGetValue("turn", out int time1);
                    fields.TryGetValue("move source time", out int time2);
                    if (time1 != time2)
                    {
                        WriteConsoleColored("turn and time position 2 are not equal!\n", ConsoleColor.Red, ConsoleColor.Black);
                        wrong_hypothesis = true;
                    }

                    // isBlacksMove and player to move are the same
                    fields.TryGetValue("isBlacksMove", out int isBlack);
                    fields.TryGetValue("move piece color", out int isBlack2);
                    if (isBlack != isBlack2)
                    {
                        WriteConsoleColored("isBlacksMove and player to move are not equal!\n", ConsoleColor.Red, ConsoleColor.Black);
                        wrong_hypothesis = true;
                    }
                }

                // branching jump move
                if (moveType == 2)
                {
                    // timeline and timeline number 2 are the same
                    fields.TryGetValue("timeline", out int timeline1);
                    fields.TryGetValue("move source universe", out int timeline2);
                    if (timeline1 != timeline2)
                    {
                        WriteConsoleColored("timeline and timeline number 2 are not equal!\n", ConsoleColor.Red, ConsoleColor.Black);
                        wrong_hypothesis = true;
                    }

                    // turn and time position 2 are the same
                    fields.TryGetValue("turn", out int time1);
                    fields.TryGetValue("move source time", out int time2);
                    if (time1 != time2)
                    {
                        WriteConsoleColored("turn and time position 2 are not equal!\n", ConsoleColor.Red, ConsoleColor.Black);
                        wrong_hypothesis = true;
                    }

                    // isBlacksMove and player to move are the same
                    fields.TryGetValue("isBlacksMove", out int isBlack);
                    fields.TryGetValue("move piece color", out int isBlack2);
                    if (isBlack != isBlack2)
                    {
                        WriteConsoleColored("isBlacksMove and player to move are not equal!\n", ConsoleColor.Red, ConsoleColor.Black);
                        wrong_hypothesis = true;
                    }
                }

                // non-branching jump move
                if (moveType == 3)
                {
                    // timeline and timeline number 2 are the same
                    fields.TryGetValue("timeline", out int timeline1);
                    fields.TryGetValue("move source universe", out int timeline2);
                    if (timeline1 != timeline2)
                    {
                        WriteConsoleColored("timeline and timeline number 2 are not equal!\n", ConsoleColor.Red, ConsoleColor.Black);
                        wrong_hypothesis = true;
                    }

                    // turn and time position 2 are the same
                    fields.TryGetValue("turn", out int time1);
                    fields.TryGetValue("move source time", out int time2);
                    if (time1 != time2)
                    {
                        WriteConsoleColored("turn and time position 2 are not equal!\n", ConsoleColor.Red, ConsoleColor.Black);
                        wrong_hypothesis = true;
                    }

                    // isBlacksMove and player to move are the same
                    fields.TryGetValue("isBlacksMove", out int isBlack);
                    fields.TryGetValue("move piece color", out int isBlack2);
                    if (isBlack != isBlack2)
                    {
                        WriteConsoleColored("isBlacksMove and player to move are not equal!\n", ConsoleColor.Red, ConsoleColor.Black);
                        wrong_hypothesis = true;
                    }
                }

                // a piece arrived from another board (do to a non-branching jump move)
                if (moveType == 4)
                {
                    // timeline and timeline number 2 are the same
                    fields.TryGetValue("timeline", out int timeline1);
                    fields.TryGetValue("move source universe", out int timeline2);
                    if (timeline1 == timeline2)
                    {
                        WriteConsoleColored("timeline and timeline number 2 are equal when they shouldn't be!\n", ConsoleColor.Red, ConsoleColor.Black);
                        wrong_hypothesis = true;
                    }

                    // isBlacksMove and player to move are the same
                    fields.TryGetValue("isBlacksMove", out int isBlack);
                    fields.TryGetValue("move piece color", out int isBlack2);
                    if (isBlack != isBlack2)
                    {
                        WriteConsoleColored("isBlacksMove and player to move are not equal!\n", ConsoleColor.Red, ConsoleColor.Black);
                        wrong_hypothesis = true;
                    }
                }

                if (moveType > 4)
                {
                    WriteConsoleColored("move type is greater than 4\n", ConsoleColor.Red, ConsoleColor.Black);
                    wrong_hypothesis = true;
                }
                
            }
            // moveType == 0
            else
            {
                fields.TryGetValue("move source universe", out int sourceUniverse);
                fields.TryGetValue("move source time", out int sourceTime);
                fields.TryGetValue("move piece color", out int movePieceColor);
                if (sourceUniverse != 0)
                {
                    WriteConsoleColored("move source universe not zero\n", ConsoleColor.Red, ConsoleColor.Black);
                    wrong_hypothesis = true;
                }

                if (sourceTime != 0)
                {
                    WriteConsoleColored("move source time not zero\n", ConsoleColor.Red, ConsoleColor.Black);
                    wrong_hypothesis = true;
                }

                if (movePieceColor != 0)
                {
                    WriteConsoleColored("move piece color not zero\n", ConsoleColor.Red, ConsoleColor.Black);
                    wrong_hypothesis = true;
                }
            }


            return wrong_hypothesis;
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
    }
}
