using FiveDChessDataInterface;
using FiveDChessDataInterface.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        [CallableExMethod(false, InvokeKind.TurnChange)]
        public static void OnTurnChanged(DataInterface di)
        {
            Console.WriteLine($"The turn changed! Currently it is {(di.GetCurrentPlayersTurn() == 0 ? "WHITE" : "BLACK")}'s turn.");
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
            di.RecalculateBitboards();
        }

        [CallableExMethod(true, InvokeKind.MatchStart)]
        public static void AddNewTimelines(DataInterface di)
        {

            // adds the following amount of timelines for both black and white each.
            // e.g. a value of 1 would add one timeline on the bottom and one at the top
            int timelinesToAddForEachPlayer = 1;


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


            //int timelinesToAdd = timelinesToAddForEachPlayer * 2;
            //var baseBoards = di.GetChessBoards();
            //var baseCbms = baseBoards.Select(x => x.cbm).ToList();

            //int startingTimelineCount = baseCbms.DistinctBy(x => x.timeline).Count();
            //var originalBoardsByTurn = baseCbms.GroupBy(x => x.timeline).ToList();

            //var newBoards = new List<ChessBoardMemory>();
            //var subturns = baseCbms.Select(x => x.GetSubturnIndex()).Distinct();
            //int shiftAmount = 0;
            //for (int subturn = subturns.Min(); subturn <= subturns.Max(); subturn++) // iterate all turns
            //{
            //    var currTurnBoards = baseCbms.Where(x => x.GetSubturnIndex() == subturn).ToList();
            //    var highestBoardIndexInTurn = currTurnBoards.Max(x => x.boardId);
            //    // shift boards by the number of ids we added

            //    var sortedByTimeline = currTurnBoards.OrderBy(x => x.timeline).ToList();

            //    var minLBoard = sortedByTimeline.First(); // black-most timeline
            //    //bool allowAddingBlackTimelineBoards = ;
            //    var maxLBoard = sortedByTimeline.Last(); // white-most timeline
            //    //var originalMaxTimeline = maxLBoard.timeline;

            //    var shiftedNewBoards = new List<ChessBoardMemory>();
            //    foreach (var cbm in sortedByTimeline)
            //    {
            //        var shiftedCbm = cbm;
            //        shiftedCbm.boardId += shiftAmount;
            //        shiftedNewBoards.Add(shiftedCbm);
            //    }

            //    int GetNextId()
            //    {
            //        var newId = shiftedNewBoards.Max(x => x.boardId) + 1;
            //        if (newBoards.Any(x => x.boardId == newId))
            //            throw new Exception("A boardId has been assigned twice. This indicates a problem in this algorithm.");

            //        return newId;
            //    }

            //    for (int l = 0; l < timelinesToAddForEachPlayer; l++)
            //    {
            //        if (baseCbms.Any(x => x.timeline == minLBoard.timeline))
            //        {
            //            minLBoard.timeline--;
            //            minLBoard.boardId = GetNextId();
            //            shiftedNewBoards.Add(minLBoard);

            //            shiftAmount++;
            //        }

            //        if (baseCbms.Any(x => x.timeline == maxLBoard.timeline))
            //        {
            //            maxLBoard.timeline++;
            //            maxLBoard.boardId = GetNextId();
            //            shiftedNewBoards.Add(maxLBoard);
            //            shiftAmount++;
            //        }
            //    }

            //    newBoards.AddRange(shiftedNewBoards);
            //}


            //int dimcnt = baseBoards.Select(x => x.cbm.timeline).Distinct().Count() + 2;
            //int boardId = 0;
            //var boards = Enumerable.Range(-dimcnt / 2, dimcnt).SelectMany(timeline =>
            //      baseBoards.Select(baseBoard =>
            //      {
            //          var cbm = baseBoard.cbm;
            //          cbm.timeline = timeline;
            //          return cbm;
            //      })
            //    )
            //    .OrderBy(x => x.turn)
            //    .ThenBy(x => Math.Abs(x.timeline))
            //    .Select(x =>
            //    {
            //        x.boardId = boardId++;
            //        return x;
            //    })
            //    .GroupBy(x => x.timeline)
            //    .SelectMany(group =>
            //    {
            //        var boards = group.ToArray();
            //        for (int i = 1; i < boards.Length; i++)
            //        {
            //            boards[i].previousBoardId = boards[i - 1].boardId;
            //        }

            //        return boards;
            //    })
            //    .OrderBy(x => x.boardId)
            //    .Select(x => new ChessBoard(x, baseBoards[0].width, baseBoards[0].height)).ToArray();

            var boards = sortedBoards.OrderBy(x => x.boardId).Select(x => new ChessBoard(x, baseBoards[0].width, baseBoards[0].height)).ToArray();

            di.SetChessBoardArray(boards.ToArray());
            di.RecalculateBitboards();
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
