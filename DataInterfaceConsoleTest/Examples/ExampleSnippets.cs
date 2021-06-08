using DataInterfaceConsoleTest.Variants;
using FiveDChessDataInterface;
using FiveDChessDataInterface.Builders;
using FiveDChessDataInterface.MemoryHelpers;
using FiveDChessDataInterface.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using static DataInterfaceConsoleTest.Examples.CallableExMethodAttribute;

namespace DataInterfaceConsoleTest.Examples
{
    static class ExampleSnippets
    {
        private const bool ENABLE_SNIPPETS = true; // enables use of snippets, the individual snippet must also be set to true

        public static MethodInfo[] GetEnabledMethods() => !ENABLE_SNIPPETS ? new MethodInfo[] { } :
            typeof(ExampleSnippets).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(x => x.GetCustomAttribute<CallableExMethodAttribute>()?.Enabled == true).ToArray();

        [CallableExMethod(false, InvokeKind.MatchStart)]
        public static void DuplicateTimelines(DataInterface di)
        {
            Console.WriteLine("Shouldnt run");
        }

        [CallableExMethod(false, InvokeKind.MatchStart)]
        public static void ChangeBoardSize(DataInterface di) // changes board size for all boards
        {
            var height = 8;
            var width = 8;
            di.MemLocChessBoardSizeHeight.SetValue(height);
            di.MemLocChessBoardSizeWidth.SetValue(width);

            Console.WriteLine($"Set new height to {height} and width to {width}.");
        }

        [CallableExMethod(false, InvokeKind.MatchStart)]
        public static void LoadCustomVariant(DataInterface di)
        {
            var height = 3;
            var width = 3;
            // example for odd timelines (commented out)
            //var gb = new GameBuilderOdd(height, width);

            //gb["-4L"].SetTurnOffset(0, true).AddBoardFromFen("ppp/3/PPP").AddBoardFromFen("ppp/3/PPP");
            //gb["-3L"].SetTurnOffset(1, true).AddBoardFromFen("cyc/3/1P1").AddBoardFromFen("cyc/3/1P1");
            //gb["-2L"].SetTurnOffset(1, true).AddBoardFromFen("cyc/3/1P1").AddBoardFromFen("cyc/3/1P1");
            //gb["-1L"].SetTurnOffset(1, true).AddBoardFromFen("cyc/3/1P1").AddBoardFromFen("cyc/3/1P1");
            //gb["0L"].SetTurnOffset(0, true).AddBoardFromFen("ppp/3/PPP").AddBoardFromFen("ppp/3/PPP");
            //gb["1L"].SetTurnOffset(1, true).AddBoardFromFen("1p1/3/CYC").AddBoardFromFen("1p1/3/CYC");
            //gb["2L"].SetTurnOffset(1, true).AddBoardFromFen("1p1/3/CYC").AddBoardFromFen("1p1/3/CYC");
            //gb["3L"].SetTurnOffset(1, true).AddBoardFromFen("1p1/3/CYC").AddBoardFromFen("1p1/3/CYC");
            //gb["4L"].SetTurnOffset(0, true).AddBoardFromFen("ppp/3/PPP").AddBoardFromFen("ppp/3/PPP");

            // example for even timelines
            var gb = new GameBuilderEven(height, width);

            gb["-1L"].SetTurnOffset(0, true).AddBoardFromFen("ckc/3/PCP").CopyPrevious(1);
            gb["-0L"].SetTurnOffset(0, true).AddBoardFromFen("ppp/3/PPP").CopyPrevious(1);
            gb["+0L"].SetTurnOffset(0, true).AddBoardFromFen("ppp/3/PPP").CopyPrevious(1);
            gb["+1L"].SetTurnOffset(0, true).AddBoardFromFen("pcp/3/CKC").CopyPrevious(1);

            di.SetChessBoardArrayFromBuilder(gb);
        }

        [CallableExMethod(true, InvokeKind.MatchStart)]
        public static void LoadPredefinedOnlineVariant(DataInterface di)
        {
            var variants = GithubVariantGetter.GetAllVariants();

            Console.WriteLine("Select a variant from the following:");
            for (int i = 0; i < variants.Length; i++)
            {
                Console.WriteLine($"\t{i + 1,2}. {variants[i].Name} by {variants[i].Author}");
            }
            
            if (int.TryParse(Console.ReadLine(), out int input) && input > 0 && input <= variants.Length)
            {
                var chosenVariant = variants[input - 1];
                var gb = chosenVariant.GetGameBuilder();
                di.SetChessBoardArrayFromBuilder(gb);
            }
            else
            {
                Console.WriteLine("Invalid input. Not loading any variant.");
            }
        }

        [CallableExMethod(false, InvokeKind.MatchStart)]
        public static void LoadPredefinedVariant(DataInterface di)
        {
            Console.WriteLine("Select Variant From The Following");
            Variant[] variants = Variant.Variants;
            for (int i = 0; i < variants.Length; i++)
            {
                Console.WriteLine("\t" + (i + 1) + ". " + variants[i].name);
            }

            int input = Convert.ToInt32(Console.ReadLine());
            input -= 1;
            if (input >= 0 && input < variants.Length)
            {
                int size = variants[input].size;
                string[] fenStrings = variants[input].timelines;

                BaseGameBuilder gb;
                if (fenStrings.Length % 2 == 0)
                {
                    gb = new GameBuilderEven(size, size);
                }
                else
                {
                    gb = new GameBuilderOdd(size, size);
                }

                for (int i = 0; i < fenStrings.Length; i++)
                {
                    string[] vals = fenStrings[i].Split(":");
                    Console.WriteLine(vals[0]);
                    Console.WriteLine(vals[1]);
                    Console.WriteLine(vals[2]);
                    Console.WriteLine(vals[3]);
                    gb[vals[1]].SetTurnOffset(Convert.ToInt32(vals[2]), vals[3] == "1").AddBoardFromFen(vals[0]);
                }

                di.SetChessBoardArrayFromBuilder(gb);
            }
        }

        [CallableExMethod(false, InvokeKind.TurnChange)]
        public static void OnTurnChanged(DataInterface di)
        {
            Console.WriteLine($"The turn changed! Currently it is {(di.GetCurrentPlayersTurn() == 0 ? "WHITE" : "BLACK")}'s turn.");
        }

        [CallableExMethod(false, InvokeKind.TurnChange)]
        public static void HeapCorruptTest(DataInterface di)
        {
            var sz = 256;
            var heap = di.asmHelper.GameMalloc(sz, false);
            KernelMethods.WriteMemory(di.GetGameHandle(), heap, Enumerable.Repeat((byte)0, sz).ToArray());
        }

        [CallableExMethod(false, InvokeKind.BoardCountChanged | InvokeKind.MatchStart)]
        public static void UpgradePawnsToQueensAfterTurn3(DataInterface di) // turns pawns to queens after turn 3
        {
            di.ModifyChessBoards(cb =>
            {
                if (cb.cbm.moveType == 0 && // no move has been made on this board yet
                        cb.cbm.turn >= 3) // if its turn 4 or later
                {
                    cb.Pieces = cb.Pieces
                    .Select(x => new ChessBoard.ChessPiece((x.Kind == ChessBoard.ChessPiece.PieceKind.Pawn) ? ChessBoard.ChessPiece.PieceKind.Queen : x.Kind, x.IsBlack))
                    .ToArray();
                }

                return cb;
            });
        }


        [CallableExMethod(false, InvokeKind.MatchStart)]
        public static void PrependTurnZero(DataInterface di)
        {
            var baseBoards = di.GetChessBoards();

            if (baseBoards.First(x => x.cbm.boardId == 0).cbm.isBlacksMove != 0)
                return; // exit if this is a turnzero game

            var timelines = baseBoards.Select(x => x.cbm).GroupBy(x => x.timeline).ToList();

            int boardId = 0;
            var newBoards = timelines.SelectMany(timeLineBoards =>
            {
                var tlBoards = timeLineBoards.Prepend(timeLineBoards.First()).ToList();

                for (int boardIndex = 0; boardIndex < tlBoards.Count; boardIndex++)
                {
                    var cbm = tlBoards[boardIndex];
                    if (boardIndex == 0)
                    {
                        cbm.isBlacksMove = 1;
                        cbm.turn = 0;
                        cbm.moveTurn = 0;
                        cbm.moveType = 5;
                    }
                    else
                        cbm.turn++;

                    tlBoards[boardIndex] = cbm;
                }
                return tlBoards;
            })
                .OrderBy(x => x.turn)
                .ThenBy(x => x.timeline * x.timeline)
                .Select(x =>
                {
                    x.boardId = boardId++;
                    return x;
                })
                .GroupBy(x => x.timeline)
                .SelectMany(group =>
                {
                    var boards = group.ToArray();
                    for (int i = 1; i < boards.Length; i++)
                    {
                        boards[i].previousBoardId = boards[i - 1].boardId;
                        boards[i].creatingMoveNumber = boards[i - 1].boardId;
                        boards[i - 1].nextInTimelineBoardId = boards[i].boardId;
                    }

                    return boards;
                })
                .Select(x => new ChessBoard(x, baseBoards[0].width, baseBoards[0].height)).ToArray();

            di.SetChessBoardArray(newBoards.ToArray());
            di.MemLocCosmeticTurnOffset.SetValue(-1);
        }

        [CallableExMethod(false, InvokeKind.MatchStart)]
        public static void AddNewTimelines(DataInterface di)
        {
            Thread.Sleep(100);
            // adds the following amount of timelines for both black and white each.
            // e.g. a value of 1 would add one timeline on the bottom and one at the top
            int timelinesToAddForEachPlayer = 10;


            var baseBoards = di.GetChessBoards();

            var baseCbms = baseBoards.Select(x => x.cbm).ToList();
            int newId = baseBoards.Max(x => x.cbm.boardId) + 1;
            for (int _ = 0; _ < timelinesToAddForEachPlayer; _++)
            {
                var newBoards = baseCbms;

                // add new black timeline
                var minTL = baseCbms.Min(x => x.timeline);
                var newMinTLBoards = baseCbms.Where(x => x.timeline == minTL)
                    .Select(x =>
                    {
                        var newCbm = x;
                        newCbm.timeline--;
                        newCbm.boardId = newId++;
                        return newCbm;
                    })
                    .OrderBy(x => x.GetSubturnIndex()).ToList();


                // add new white timeline
                var maxTL = baseCbms.Max(x => x.timeline);
                var newMaxTLBoards = baseCbms.Where(x => x.timeline == maxTL)
                    .Select(x =>
                    {
                        var newCbm = x;
                        newCbm.timeline++;
                        newCbm.boardId = newId++;
                        return newCbm;
                    })
                    .OrderBy(x => x.GetSubturnIndex()).ToList();

                IEnumerable<int> RangeFromToInclusive(int start, int end)
                {
                    return Enumerable.Range(start, end - start + 1);
                }

                // combine new boards 
                var boardsToInsert = newMinTLBoards.Concat(newMaxTLBoards).ToList();
                newBoards.AddRange(boardsToInsert);

                baseCbms = newBoards;
            }

            var sortedBoards = GameUtil.ReassignBoardIds(baseCbms.ToArray());
            var boards = sortedBoards.OrderBy(x => x.boardId).Select(x => new ChessBoard(x, baseBoards[0].width, baseBoards[0].height)).ToArray();

            di.SetChessBoardArray(boards.ToArray());
        }

        [CallableExMethod(true, InvokeKind.BoardCountChanged | InvokeKind.Startup | InvokeKind.MatchStart | InvokeKind.MatchExited)]
        public static void DumpBoardsAndGeneralInfo(DataInterface di)
        {
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
                            Program.WriteConsoleColored(p.SingleLetterNotation(), p.IsBlack ? ConsoleColor.White : ConsoleColor.Black, p.IsBlack ? ConsoleColor.Black : ConsoleColor.White);
                        }
                        else
                        {
                            Program.WriteConsoleColored(" ", ConsoleColor.Gray, ConsoleColor.Gray);
                        }
                    }
                    Console.ResetColor();
                    Console.WriteLine(" ");
                }
                Console.WriteLine();
            }
        }
    }
}
