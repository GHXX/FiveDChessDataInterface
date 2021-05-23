using FiveDChessDataInterface;
using System;
using System.Linq;
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
            Console.WriteLine($"The chessboard array size is located at: 0x{di.MemLocChessArrayElementCount.Location.ToString("X16")}");
            Console.WriteLine($"The chessboard sizes width and height are located at 0x{di.MemLocChessBoardSizeWidth.Location.ToString("X16")} and 0x{di.MemLocChessBoardSizeHeight.Location.ToString("X16")}");
            Console.WriteLine($"Current chessboard count: {di.MemLocChessArrayElementCount.GetValue()}; Current chessboard array capacity: {di.MemLocChessArrayCapacity.GetValue()}");

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

            // set custom starting boards

            var ccForegroundDefault = Console.ForegroundColor;
            var ccBackgoundDefault = Console.BackgroundColor;

            bool timelinesDuplicated = false;

            int oldCnt = -1;
            int lastPlayersTurn = -1;
            while (true)
            {
                var cnt = di.GetChessBoardAmount();
                var currPlayersTurn = di.GetCurrentPlayersTurn();

                if (currPlayersTurn == 0 && !timelinesDuplicated && cnt == 1 && false)
                {
                    timelinesDuplicated = true;
                    var baseBoard = di.GetChessBoards()[0];
                    int dimcnt = 3;
                    int boardId = 0;
                    var boards = Enumerable.Range(0, dimcnt).Select(x =>
                    {
                        var cbm = baseBoard.cbm;
                        cbm.timeline = x;
                        return cbm;
                    })
                        .OrderBy(x => x.timeline * x.timeline)
                        .Select(x =>
                        {
                            x.boardId = boardId++;
                            return x;
                        }
                    ).Select(x => new ChessBoard(x, baseBoard.width, baseBoard.height)).ToArray();

                    di.SetChessBoardArray(boards);
                }
                else if (cnt == 0)
                {
                    timelinesDuplicated = false;
                }

                if (cnt != oldCnt)
                {
                    oldCnt = cnt;


                    var cbs = di.GetChessBoards();

                    Console.Clear();
                    Console.WriteLine($"Current chessboard ptr: {di.MemLocChessArrayPointer.ToString()}");
                    Console.WriteLine($"Current timeline stats: White: {di.GetNumberOfWhiteTimelines()}; Black: {di.GetNumberOfBlackTimelines()}");
                    Console.WriteLine("Chessboards: \n");
                    for (int i = 0; i < cbs.Count; i++)
                    {
                        var board = cbs[i];
                        Console.WriteLine($"Board: L{board.cbm.timeline:+#;-#;0}T{board.cbm.turn + 1}");



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
                }
                else
                {
                    // piece replacement
                    if (false) // if the turn changed
                    {
                        di.ModifyChessBoards(cb =>
                        {
                            if (cb.cbm.moveType == 0 && // no move has been made on this board yet
                                    cb.cbm.turn >= 3) // if its turn 3
                            {
                                cb.Pieces = cb.Pieces
                                .Select(x => new ChessBoard.ChessPiece((x.Kind == ChessBoard.ChessPiece.PieceKind.Pawn) ? ChessBoard.ChessPiece.PieceKind.Queen : x.Kind, x.IsBlack))
                                .ToArray();
                            }




                            return cb;
                        });

                    }
                    Thread.Sleep(500);
                }
            }


            Console.WriteLine("Done!");
            Console.ReadLine();
            Environment.Exit(0);
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
