using DataInterfaceConsoleTest.Variants;
using FiveDChessDataInterface;
using FiveDChessDataInterface.Builders;
using FiveDChessDataInterface.MemoryHelpers;
using FiveDChessDataInterface.Saving;
using FiveDChessDataInterface.Util;
using FiveDChessDataInterface.Variants;
using System;
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

        [CallableExMethod(true, InvokeKind.Startup)]
        public static void TrapLocationOnMatchStart(DataInterface di)
        {
            // TODO autoresolve
            // 5dchesswithmultiversetimetravel.exe+91843 
            var at = di.asmHelper.PlaceAssemblyTrapAdvanced(IntPtr.Add(di.GameProcess.MainModule.BaseAddress, 0x289C2));
            Console.WriteLine("Trap placed!");
            at.WaitTillHit();
            // other trap addresses:
            // 0x91843 -- inside main update loop
            // 0x289f0 -- load_variant
            // 5dchesswithmultiversetimetravel.exe+289C2 -- post load_variant

            var gb2 = new GameBuilderOdd(8, 8);
            gb2["0L"].SetTurnOffset(0, true).AddBoardFromFen("rnbqkbnr/8*p/8/8/8/8/8*P/RNBQKBNR").CopyPrevious(1);
            gb2.Add5DPGNMoves("[Mode \"5D\"]\n1.(0T1)e2e3 / (0T1)Ng8f6 \n2.(0T2)Ng1f3 / (0T2)c7c5 \n3.(0T3)c2c4 / (0T3)Nb8c6 \n4.(0T4)Nb1c3 / (0T4)h7h6 \n5.(0T5)h2h3 / (0T5)a7a6 \n6.(0T6)a2a3 / (0T6)Ra8b8 \n7.(0T7)Bf1d3 / (0T7)g7g6 \n8.(0T8)Nc3d5 / (0T8)e7e6 \n9.(0T9)Qd1c2 / (0T9)Rh8g8 \n10.(0T10)g2g3 / (0T10)Nc6d4 \n11.(0T11)Qc2d1 / (0T11)b7b5 \n12.(0T12)Nf3e5 / (0T12)Nd4f5 \n13.(0T13)Qd1f3 / (0T13)Bf8e7 \n14.(0T14)Bd3f5 / (0T14)e6f5 \n15.(0T15)Nd5f6 / (0T15)Be7f6 \n16.(0T16)Qf3d5 / (0T16)Rg8f8 \n17.(0T17)Qd5>>(0T15)f7 / (0T17)Qd8>>(0T14)a5 \n18.(-1T15)Nd5f6 / (-1T15)Ke8>>(0T14)f8 \n19.(-2T15)Nd5f6");
            di.SetChessBoardArrayFromBuilder(gb2);

            // demo code
            /*
            var timelineCnt = 9;
            var turnCnt = 8;
            var boardHeight = 8;
            var boardWidth = 8;
            bool useWhite = true;
            var piece = "q";

           piece = useWhite ? piece.ToUpperInvariant() : piece.ToLowerInvariant();

            int subturnOffset = useWhite ? 1 : 0;

            var gb2 = new GameBuilderOdd(boardHeight, boardWidth);
            var timelineMatrix = Enumerable.Range(-timelineCnt / 2, timelineCnt).Select(x => $"{x}L");
            foreach (var tl in timelineMatrix)
            {
                gb2[tl].AddBoardFromFen(string.Join("/", Enumerable.Repeat(boardWidth.ToString(), boardHeight))).CopyPrevious(turnCnt * 2 - 1 - 1 + subturnOffset);

                if (tl == "0L")
                {
                    var leftspace = boardWidth / 2;
                    var rightspace = boardWidth - 1 - leftspace;
                    gb2[tl].AddBoardFromFen(string.Join("/", Enumerable.Repeat(boardWidth.ToString(), boardHeight - 1)) + $"/{leftspace}{piece}{rightspace}");
                }
                else
                {
                    gb2[tl].CopyPrevious();
                }
            }
            di.SetChessBoardArrayFromBuilder(gb2);*/

            #region oldcode
            //var height = 3;
            //var width = 3;
            //// example for odd timelines
            //var gb = new GameBuilderOdd(height, width);

            //gb["-4L"].SetTurnOffset(0, true).AddBoardFromFen("ppp/3/PPP").AddBoardFromFen("ppp/3/PPP").CopyPrevious(10);
            //gb["-3L"].SetTurnOffset(1, true).AddBoardFromFen("cyc/3/1P1").AddBoardFromFen("cyc/3/1P1").CopyPrevious(10);
            //gb["-2L"].SetTurnOffset(1, true).AddBoardFromFen("cyc/3/1P1").AddBoardFromFen("cyc/3/1P1").CopyPrevious(10);
            //gb["-1L"].SetTurnOffset(1, true).AddBoardFromFen("cyc/3/1P1").AddBoardFromFen("cyc/3/1P1").CopyPrevious(10);
            //gb["0L"].SetTurnOffset(0, true).AddBoardFromFen("ppp/3/PPP").AddBoardFromFen("ppp/3/PPP").CopyPrevious(10);
            //gb["1L"].SetTurnOffset(1, true).AddBoardFromFen("1p1/3/CYC").AddBoardFromFen("1p1/3/CYC").CopyPrevious(10);
            //gb["2L"].SetTurnOffset(1, true).AddBoardFromFen("1p1/3/CYC").AddBoardFromFen("1p1/3/CYC").CopyPrevious(10);
            //gb["3L"].SetTurnOffset(1, true).AddBoardFromFen("1p1/3/CYC").AddBoardFromFen("1p1/3/CYC").CopyPrevious(10);
            //gb["4L"].SetTurnOffset(0, true).AddBoardFromFen("ppp/3/PPP").AddBoardFromFen("ppp/3/PPP").CopyPrevious(10);

            //// example for even timelines
            //var gb = new GameBuilderEven(height, width);

            //gb["-1L"].SetTurnOffset(0, true).AddBoardFromFen("ckc/3/PCP").CopyPrevious(1);
            //gb["-0L"].SetTurnOffset(0, true).AddBoardFromFen("ppp/3/PPP").CopyPrevious(1);
            //gb["+0L"].SetTurnOffset(0, true).AddBoardFromFen("ppp/3/PPP").CopyPrevious(1);
            //gb["+1L"].SetTurnOffset(0, true).AddBoardFromFen("pcp/3/CKC").CopyPrevious(1);
            //Console.WriteLine("Old Array ptr:" + di.MemLocChessArrayPointer.ToString());
            //di.SetChessBoardArrayFromBuilder(gb);
            //Console.WriteLine("New Array ptr:" + di.MemLocChessArrayPointer.ToString());
            #endregion

            at.ReleaseTrap();
            Console.WriteLine("Trap released!");
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
            var gb = new GameBuilderOdd(8, 8);
            gb["0L"].SetTurnOffset(0, true).AddBoardFromFen("rnbqkbnr/8*p/8/8/8/8/8*P/RNBQKBNR").CopyPrevious(1);
            gb.Add5DPGNMoves("[Mode \"5D\"]\n1.(0T1)e2e3 / (0T1)Ng8f6 \n2.(0T2)Ng1f3 / (0T2)c7c5 \n3.(0T3)c2c4 / (0T3)Nb8c6 \n4.(0T4)Nb1c3 / (0T4)h7h6 \n5.(0T5)h2h3 / (0T5)a7a6 \n6.(0T6)a2a3 / (0T6)Ra8b8 \n7.(0T7)Bf1d3 / (0T7)g7g6 \n8.(0T8)Nc3d5 / (0T8)e7e6 \n9.(0T9)Qd1c2 / (0T9)Rh8g8 \n10.(0T10)g2g3 / (0T10)Nc6d4 \n11.(0T11)Qc2d1 / (0T11)b7b5 \n12.(0T12)Nf3e5 / (0T12)Nd4f5 \n13.(0T13)Qd1f3 / (0T13)Bf8e7 \n14.(0T14)Bd3f5 / (0T14)e6f5 \n15.(0T15)Nd5f6 / (0T15)Be7f6 \n16.(0T16)Qf3d5 / (0T16)Rg8f8 \n17.(0T17)Qd5>>(0T15)f7 / (0T17)Qd8>>(0T14)a5 \n18.(-1T15)Nd5f6 / (-1T15)Ke8>>(0T14)f8 \n19.(-2T15)Nd5f6");

            //gb["-1L"].SetTurnOffset(0, true).AddBoardFromFen("ckc/3/PCP").CopyPrevious(1);
            //gb["-0L"].SetTurnOffset(0, true).AddBoardFromFen("ppp/3/PPP").CopyPrevious(1);
            //gb["+0L"].SetTurnOffset(0, true).AddBoardFromFen("ppp/3/PPP").CopyPrevious(1);
            //gb["+1L"].SetTurnOffset(0, true).AddBoardFromFen("pcp/3/CKC").CopyPrevious(1);

            di.SetChessBoardArrayFromBuilder(gb);
        }

        [CallableExMethod(false, InvokeKind.MatchStart)]
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
        public static void UpgradePawnsToQueensAfterSomeTurn(DataInterface di) // turns pawns to queens after turn 3
        {
            di.ModifyChessBoards(cb =>
            {
                if (cb.cbm.moveType == 0 && // no move has been made on this board yet
                        cb.cbm.turn >= 15) // if its turn 4 or later
                {
                    cb.Pieces = cb.Pieces
                    .Select(x => new ChessBoard.ChessPiece((x.Kind == ChessBoard.ChessPiece.PieceKind.Pawn || x.Kind == ChessBoard.ChessPiece.PieceKind.Brawn && ((cb.cbm.isBlacksMove == 1) != x.IsBlack)) ? ChessBoard.ChessPiece.PieceKind.Queen : x.Kind, x.IsBlack))
                    .ToArray();
                }

                return cb;
            });
        }

        private static string savedGame = null;
        [CallableExMethod(false, InvokeKind.BoardCountChanged | InvokeKind.MatchStart)]
        public static void LoadSaveTest(DataInterface di)
        {
            var sh = new SaveHandler(di);
            var maxTurn = di.GetChessBoards().Max(x => x.cbm.turn);
            if (maxTurn == 0 && savedGame != null)
            {
                Console.WriteLine("Loading...");
                sh.LoadFromJson(savedGame);
                savedGame = null;
                Console.WriteLine("Loaded!");
            }
            else if (maxTurn == 2 && savedGame == null)
            {
                Console.WriteLine("Saving...");
                savedGame = sh.SaveToJson();
                Console.WriteLine("Saved!");
            }

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
