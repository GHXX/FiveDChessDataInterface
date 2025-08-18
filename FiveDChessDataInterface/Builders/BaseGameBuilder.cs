using FiveDChessDataInterface.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FiveDChessDataInterface.Builders
{
    public abstract class BaseGameBuilder
    {
        public readonly int boardHeight;
        public readonly int boardWidth;
        public int CosmeticTurnOffset { get; set; }

        public List<Timeline> Timelines { get; }
        public bool EvenNumberOfStartingTimelines { get; }

        public BaseGameBuilder(bool evenNumberOfStartingTimelines, int boardHeight, int boardWidth)
        {
            this.boardHeight = boardHeight;
            this.boardWidth = boardWidth;

            this.Timelines = new List<Timeline>();
            this.EvenNumberOfStartingTimelines = evenNumberOfStartingTimelines;

            SetupInitialTimelines();
        }

        public Timeline this[Timeline.TimelineIndex tli, bool autocreate = true]
        {
            get
            {
                if (!this.EvenNumberOfStartingTimelines && tli.timeline == 0 && tli.isNegative)
                    throw new ArgumentException("You cannot specify a timeline of -0L for a game with an odd number of starting timelines.");

                if (!this.Timelines.Any(x => x.timelineIndex == tli))
                {
                    if (autocreate)
                        CreateAndAddNewTimeline(tli);
                    else
                        throw new KeyNotFoundException("The specified timeline could not be found!");
                }

                return this.Timelines.Single(x => x.timelineIndex == tli);
            }
        }

        /// <summary>
        /// Should add one timeline to odd number of starting boards and two to even
        /// </summary>
        protected abstract void SetupInitialTimelines();

        public class Timeline
        {
            private readonly int boardHeight;
            private readonly int boardWidth;
            public ChessBoardData previousBoardIfBranching = null;
            public List<ChessBoardData> Boards { get; }
            public readonly TimelineIndex timelineIndex;
            private int subturnOffset = 0; // TODO set the cosmetic turn offset somehow

            public Timeline(int boardHeight, int boardWidth, TimelineIndex timelineIndex)
            {
                this.boardHeight = boardHeight;
                this.boardWidth = boardWidth;
                this.timelineIndex = timelineIndex;
                this.Boards = new List<ChessBoardData>();
            }

            public Timeline SetTurnOffset(int firstBoardTurn, bool isBlackSubturn)
            {
                if (this.Boards.Any())
                    throw new InvalidOperationException("The turn offset cannot be set if any boards were added to this timeline. Set it before adding any boards.");

                if (firstBoardTurn < 0)
                    throw new NotImplementedException($"Currently {nameof(firstBoardTurn)} cannot be negative. Use the cosmetic turn offset instead!");

                this.subturnOffset = firstBoardTurn * 2 + (isBlackSubturn ? 1 : 0);
                return this;
            }

            public Timeline AddBoardFromFen(string fen)
            {

                if (fen != null)
                {
                    // last subturn + 1, or the offset if there are no boards
                    int nextSubturn;
                    if (this.Boards.Any())
                    {
                        var lastValid = this.Boards.FindLastIndex(x => x != null);
                        var nullBoardsAfter = this.Boards.Count() - lastValid - 1; // count nullboards
                        var b2 = this.Boards[lastValid];
                        nextSubturn = b2.turn * 2 + (b2.isBlackBoard ? 1 : 0) + 1 + nullBoardsAfter;
                    }
                    else
                    {
                        nextSubturn = this.subturnOffset;
                    }
                    var b = new ChessBoardData(this.boardHeight, this.boardWidth, nextSubturn / 2, nextSubturn % 2 == 1, fen);
                    this.Boards.Add(b);
                }
                else
                {
                    this.Boards.Add(null);
                }
                return this;
            }

            public Timeline AddEmptyBoard() => AddBoardFromFen(string.Join("/", Enumerable.Repeat(this.boardWidth, this.boardHeight)));

            public Timeline CopyPrevious(int repetitionCount = 1)
            {
                if (!this.Boards.Any())
                    throw new InvalidOperationException("There is no board in this timeline. Therefore no board can be copied!");

                for (int i = 0; i < repetitionCount; i++)
                {
                    // last subturn + 1, or the offset if there are no boards
                    var nextSubturn = this.Boards.Any() ? this.Boards.Max(x => x.turn * 2 + (x.isBlackBoard ? 1 : 0)) + 1 : this.subturnOffset;

                    var b = new ChessBoardData(this.boardHeight, this.boardWidth, nextSubturn / 2, nextSubturn % 2 == 1, this.Boards.Last().pieces);
                    this.Boards.Add(b);
                }

                return this;
            }

            public override string ToString() => $"{this.timelineIndex} Boardcnt: {this.Boards.Count}";

            public readonly struct TimelineIndex
            {
                public readonly bool isNegative;
                public readonly int timeline;
                public TimelineIndex(bool isNegative, int timeline)
                {
                    if (timeline < 0)
                        throw new ArgumentOutOfRangeException(nameof(timeline) + " may not be negative! Set the sign instead.");

                    this.isNegative = isNegative;
                    this.timeline = timeline;
                }

                public static implicit operator TimelineIndex(string s)
                {
                    // TODO cleanup

                    if (s[0] != '+' && s[0] != '-') // automatically add plus signs, if necessary
                        s = '+' + s;

                    var sign = s[0];

                    if (s.Last() != 'L')
                        throw new ArgumentException("Timeline values have to end with an L. e.g. +0L or -2L");

                    var number = int.Parse(s.Substring(1, s.Length - 2));

                    if (sign == '+' || sign == '-')
                    {
                        return new TimelineIndex(sign == '-', number);
                    }

                    throw new ArgumentException("The provided number did not have a sign.");
                }

                public static bool operator ==(TimelineIndex a, TimelineIndex b) => a.isNegative == b.isNegative && a.timeline == b.timeline;
                public static bool operator !=(TimelineIndex a, TimelineIndex b) => !(a == b);

                public override string ToString() => $"{(this.isNegative ? "-" : "+")}{this.timeline}L";

                public override bool Equals(object obj)
                {
                    return obj is TimelineIndex tli && this == tli;
                }

                public override int GetHashCode()
                {
                    return (this.isNegative ? (0x1 << 31) : 0) | this.timeline; // simply set the msb to the sign.
                }
            }

            public struct TravelMove5D {
                public int sourceL;
                public int sourceT;
                public int sourceIsBlack;
                public int sourceY;
                public int sourceX;
                
                public int destL;
                public int destT;
                public int destIsBlack;
                public int destY;
                public int destX;

                public TravelMove5D(int sourceL, int sourceT, bool isBlackMove, int sourceY, int sourceX, int destL, int destT, int destY, int destX) {
                    this.sourceL = sourceL;
                    this.sourceT = sourceT;
                    this.sourceIsBlack = isBlackMove ? 1 : 0;
                    this.sourceY = sourceY;
                    this.sourceX = sourceX;
                    this.destL = destL;
                    this.destT = destT;
                    this.destIsBlack = this.sourceIsBlack;
                    this.destY = destY;
                    this.destX = destX;
                }
            }

            public struct Move2D {
                public int sourceY;
                public int sourceX;
                public int destY;
                public int destX;

                public Move2D(int sourceY, int sourceX, int destY, int destX) {
                    this.sourceY = sourceY;
                    this.sourceX = sourceX;
                    this.destY = destY;
                    this.destX = destX;
                }
            }


            public class ChessBoardData
            {
                private readonly int boardHeight;
                private readonly int boardWidth;
                internal ChessBoard.ChessPiece[,] pieces;
                internal int turn;
                internal bool isBlackBoard;
                internal TravelMove5D? travelMove;
                internal Move2D? normalMove;
                internal ChessBoardData previousBoardOverride;

                private ChessBoardData(int boardHeight, int boardWidth, int turn, bool isBlackBoard)
                {
                    this.boardHeight = boardHeight;
                    this.boardWidth = boardWidth;
                    this.turn = turn;
                    this.isBlackBoard = isBlackBoard;


                    this.pieces = new ChessBoard.ChessPiece[boardHeight, boardWidth];
                    for (int y = 0; y < boardHeight; y++)
                        for (int x = 0; x < boardWidth; x++)
                            this.pieces[y, x] = new ChessBoard.ChessPiece(ChessBoard.ChessPiece.PieceKind.Empty, false);

                }

                public ChessBoardData(int boardHeight, int boardWidth, int turn, bool isBlackBoard, string fenCode)
                    : this(boardHeight, boardWidth, turn, isBlackBoard)
                {
                    LoadFEN(fenCode);
                }

                public ChessBoardData(int boardHeight, int boardWidth, int turn, bool isBlackBoard, ChessBoard.ChessPiece[,] pieceData)
                    : this(boardHeight, boardWidth, turn, isBlackBoard)
                {
                    this.pieces = pieceData;
                }

                public ChessBoardData(ChessBoardData original) : this(original.boardHeight, original.boardWidth, original.turn,
                    original.isBlackBoard, (ChessBoard.ChessPiece[,])original.pieces.Clone())
                { }


                private void LoadFEN(string compressedFen)
                {
                    if (compressedFen is null)
                    {
                        throw new ArgumentNullException(nameof(compressedFen));
                    }

                    var fen = FenUtil.ExpandFen(compressedFen);

                    var lines = fen.Split('/');
                    if (lines.Length != this.boardHeight)
                        throw new ArgumentException($"The number of line-segments in the provided board is {lines.Length}, which does not match the board height of {this.boardHeight}!");

                    for (int y = 0; y < this.boardHeight; y++)
                    {
                        var currentLine = lines[y];
                        var x = 0;
                        for (int i = 0; i < currentLine.Length; i++)
                        {
                            var currChar = currentLine[i];
                            if (char.IsDigit(currChar)) // empty pieces
                            {
                                for (int n = 0; n < int.Parse(currChar.ToString()); n++)
                                {
                                    this.pieces[x++, this.boardHeight - y - 1] = new ChessBoard.ChessPiece(ChessBoard.ChessPiece.PieceKind.Empty, false);
                                }
                            }
                            else // not an empty piece
                            {
                                var isWhite = char.IsUpper(currChar);
                                this.pieces[x++, this.boardHeight - y - 1] = new ChessBoard.ChessPiece(ChessBoard.ChessPiece.SingleLetterPieceTable.Keys
                                        .Single(x => string.Equals(ChessBoard.ChessPiece.SingleLetterPieceTable[x], currChar.ToString(), StringComparison.InvariantCultureIgnoreCase)
                                    ), !isWhite);
                            }
                        }

                        if (x != this.boardWidth)
                            throw new Exception($"Failed to parse the line at index y={y}. Expected line length x:{this.boardWidth}, Actual: {x}");
                    }
                }
            }
        }

        public Timeline CreateAndAddNewTimeline(Timeline.TimelineIndex tli)
        {
            var newTl = new Timeline(this.boardHeight, this.boardWidth, tli);
            this.Timelines.Add(newTl);
            return newTl;
        }


        public Timeline CreateAndAddNewWhiteTimeline()
        {
            var newWhiteIndex = this.Timelines.Where(x => !x.timelineIndex.isNegative).Max(x => x.timelineIndex.timeline) + 1;
            return CreateAndAddNewTimeline(new Timeline.TimelineIndex(false, newWhiteIndex));
        }
        public Timeline CreateAndAddNewBlackTimeline()
        {
            var newBlackIndex = this.Timelines.Where(x => x.timelineIndex.isNegative).Max(x => x.timelineIndex.timeline) + 1;
            return CreateAndAddNewTimeline(new Timeline.TimelineIndex(true, newBlackIndex));
        }

        public ChessBoardMemory[] BuildCbms()
        {
            // TODO maybe validate all timelines are nonempty?
            var boardToIdMap = new Dictionary<Timeline.ChessBoardData, int>();
            var allBoards = new List<ChessBoardMemory>();
            int nextFreeId = 0;
            foreach (var tl in this.Timelines)
            {
                int lastBoardId = -1;
                var timelineCbms = new List<ChessBoardMemory>();
                foreach (var board in tl.Boards.Where(x => x != null))
                {
                    var cbm = new ChessBoardMemory();

                    cbm.boardId = nextFreeId++;
                    boardToIdMap.Add(board, cbm.boardId); 
                    cbm.timeline = tl.timelineIndex.isNegative ? -(tl.timelineIndex.timeline + (this.EvenNumberOfStartingTimelines ? 1 : 0)) : tl.timelineIndex.timeline;
                    cbm.turn = board.turn;
                    cbm.isBlacksMove = board.isBlackBoard ? 1 : 0;

                    // array of (KIND,COLOR) Color: 0 = empty, 1 = white, 2 = black
                    cbm.positionData = new byte[8 * 8 * 2];
                    for (int col = 0; col < this.boardWidth; col++)
                    {
                        for (int row = 0; row < this.boardWidth; row++)
                        {
                            var index = (row * 8 + col) * 2;
                            var piece = board.pieces[row, col];
                            var pb = piece.ToByteArray();
                            cbm.positionData[index] = pb[0];
                            cbm.positionData[index + 1] = pb[1];
                        }
                    }

                    cbm.moveTurn = -1;
                    cbm.moveType = 0;

                    cbm.moveSourceL = -1;
                    cbm.moveSourceT = -1;
                    cbm.moveSourceIsBlack = -1;
                    cbm.moveSourceY = -1;
                    cbm.moveSourceX = -1;
                    cbm.moveDestL = -1;
                    cbm.moveDestT = -1;
                    cbm.moveDestIsBlack = -1;
                    cbm.moveDestY = -1;
                    cbm.moveDestX = -1;

                    cbm.creatingMoveNumber = lastBoardId;
                    cbm.nextInTimelineBoardId = -1;
                    cbm.previousBoardId = lastBoardId;
                    cbm.createdBoardID = -1;

                    cbm.ttPieceOriginBoardId = lastBoardId;
                    cbm.moveSourcePosY = -1;
                    cbm.moveSourcePosX = -1;
                    cbm.moveDestPosY = -1;
                    cbm.moveDestPosX = -1;

                    if (board.travelMove.HasValue) {
                        var m = board.travelMove.Value;
                        cbm.moveSourceL = m.sourceL;
                        cbm.moveSourceT = m.sourceT;
                        cbm.moveSourceY = m.sourceY;
                        cbm.moveSourceX = m.sourceX;
                        cbm.moveSourceIsBlack = m.sourceIsBlack;

                        cbm.moveDestL = m.destL;
                        cbm.moveDestT = m.destT;
                        cbm.moveDestY = m.destY;
                        cbm.moveDestX = m.destX;
                        cbm.moveDestIsBlack = m.destIsBlack;

                        //cbm.moveSourcePosY = m.sourceY;
                        //cbm.moveSourcePosX = m.sourceX;
                        //cbm.moveDestPosY = m.destY;
                        //cbm.moveDestPosX = m.destX;
                    }

                    if (board.normalMove.HasValue) {
                        var m = board.normalMove.Value;
                        cbm.SetHighlightPos(m.sourceX, m.sourceY, m.destX, m.destY);
                    }

                    if (lastBoardId != -1) // set the nextintimelineboardid of the last board to the current id
                    {
                        var lastCbm = timelineCbms.Last();
                        timelineCbms.RemoveAt(timelineCbms.Count - 1);
                        lastCbm.nextInTimelineBoardId = cbm.boardId;
                        if(lastCbm.moveType != 2)
                            lastCbm.moveType = 5;
                        timelineCbms.Add(lastCbm);
                    }
                    if (board.travelMove.HasValue) {
                        var m = board.travelMove.Value;
                        cbm.moveType = (byte)((m.sourceT != m.destT || m.sourceL != m.destL) ? 2 : 1);
                    }

                    if (board.previousBoardOverride != null) {
                        cbm.previousBoardId = boardToIdMap[board.previousBoardOverride];
                    }


                    lastBoardId = cbm.boardId;

                    timelineCbms.Add(cbm);
                }

                allBoards.AddRange(timelineCbms);
            }

            var idsFixed = GameUtil.ReassignBoardIds(allBoards.ToArray());
            return idsFixed;
        }

        private List<ChessBoardMemory> CachedCbms = new List<ChessBoardMemory>();
        public ChessBoardMemory[] BuildCbms2() => this.CachedCbms.ToArray();


        public ChessBoard[] Build() => BuildCbms().Select(x => new ChessBoard(x, this.boardWidth, this.boardHeight)).ToArray();

        public BaseGameBuilder Add5DPGNMoves(string pgn)
        {
            if (string.IsNullOrWhiteSpace(pgn))
                throw new ArgumentNullException("Argument " + nameof(pgn) + " was empty!");

            var lines = pgn.Replace("\r\n", "\n").Replace("\r", "\n").Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

            const string modeValue = "[Mode";
            if (lines[0].StartsWith(modeValue))
            {
                var mode = string.Join(null, lines[0].Substring(modeValue.Length).SkipWhile(c => c != '"').Skip(1).TakeWhile(c => c != '"'));
                if (mode.ToLowerInvariant() == "5d")
                {
                    lines.RemoveAt(0);
                }
                else
                {
                    throw new FormatException($"Invalid pgn supplied. Mode was expected to be 5D, but instead is '{mode}'!");
                }
            }

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var dotcnt = line.Count(x => x == '.');
                if (dotcnt == 0)
                    continue;
                else if (dotcnt == 1)
                {
                    var splitted = line.Split('.');
                    if (!int.TryParse(splitted[0], out _))
                    {
                        throw new FormatException($"Period on line {i + 1} was interpreted as a delimeter between turncount and moveset. But the left side of the period was not a number.");
                    }

                    lines[i] = splitted[1].Trim();
                    continue;
                }
                else
                {
                    throw new FormatException($"Line {i + 1} contained {dotcnt} periods, even though at most one was expected.");
                }
            }

            int? cosmeticTurnOffset = null;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                var cnt = line.Count(x => x == '/');
                if (cnt > 1)
                    throw new FormatException($"More than one slash was encountered on line {i + 1}.");


                var playerMoveSets = line.Split('/');
                for (int pmsi = 0; pmsi < playerMoveSets.Length; pmsi++)
                {
                    string moveSet = playerMoveSets[pmsi];

                    var moves = moveSet.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var moveRaw in moves)
                    {
                        var moveold = Regex.Replace(moveRaw, "{.*?}", "");
                        var branchExpected = moveold.Contains(">>");
                        var moveFixed = moveold.Replace(">>", ">").Replace(">", ">>"); // treat >> and > the same
                        var srcBoardName = string.Join(null, moveFixed.Skip(1).TakeWhile(x => x != ')'));
                        var srcBoardSplit = srcBoardName.Split('T');
                        var srcTimelineId = int.Parse(srcBoardSplit[0]);
                        var srcTL = this.Timelines.Single(x => x.timelineIndex == $"{srcTimelineId}L");
                        var pgnTurn = int.Parse(srcBoardSplit[1]) - 1;


                        var lastboard = srcTL.Boards.Last();
                        var expectedOffset = pgnTurn - lastboard.turn;
                        if (cosmeticTurnOffset == null)
                            cosmeticTurnOffset = expectedOffset;
                        else if (cosmeticTurnOffset.Value != expectedOffset)
                            throw new Exception("Inconsistent turn value.");

                        var turn = pgnTurn - cosmeticTurnOffset;


                        var srcBoard = srcTL.Boards.Single(x => x.turn == turn && x.isBlackBoard == (pmsi == 1));

                        if (srcBoard.isBlackBoard != lastboard.isBlackBoard)
                            throw new Exception("Tried to play a move on a board that belongs to the other player!");


                        if (srcBoard.turn != lastboard.turn)
                            throw new Exception("Board that a move was made on is not the last board in the timeline.");


                        var isTT = moveFixed.Contains(">>");
                        if (isTT)
                        {
                            var splitted = moveFixed.Split(new[] { ">>" }, StringSplitOptions.None);
                            var dstBoardName = string.Join(null, splitted[1].Skip(1).TakeWhile(x => x != ')'));
                            var dstBoardSplit = dstBoardName.Split('T');
                            var dstTimelineId = int.Parse(dstBoardSplit[0]);
                            var dstTL = this.Timelines.Single(x => x.timelineIndex == $"{dstTimelineId}L");
                            var dstTurn = int.Parse(dstBoardSplit[1]) - 1 - (cosmeticTurnOffset??0);
                            var dstindex = dstTurn * 2 + pmsi;
                            // if the destboard has already been played on
                            var dstBoardAlreadyPlayed = dstTL.Boards.Any(x => (x.turn * 2 + (x.isBlackBoard ? 1 : 0) > dstindex));
                            var dstBoard = dstTL.Boards.Single(x => x.turn == dstTurn && x.isBlackBoard == (pmsi == 1));

                            var srcPos = string.Join(null, splitted[0].Reverse().Take(2).Reverse());
                            var dstPos = string.Join(null, splitted[1].Reverse().Take(2).Reverse());

                            // add new board after srcboard
                            var newCbm1 = new Timeline.ChessBoardData(srcBoard);
                            // incrememnt turn
                            newCbm1.turn += newCbm1.isBlackBoard ? 1 : 0;
                            newCbm1.isBlackBoard ^= true;
                            // ---
                            newCbm1.pieces[srcPos.ToLowerInvariant()[0] - 97, int.Parse(srcPos.Substring(1, 1)) - 1] = new ChessBoard.ChessPiece(ChessBoard.ChessPiece.PieceKind.Empty, false);
                            

                            srcTL.Boards.Add(newCbm1);

                            // add new destboard
                            var newCbm2 = new Timeline.ChessBoardData(dstBoard);
                            // incrememnt turn
                            newCbm2.turn += newCbm2.isBlackBoard ? 1 : 0;
                            newCbm2.isBlackBoard ^= true;
                            // ---
                            var dstPosX = dstPos.ToLowerInvariant()[0] - 97;
                            var dstPosY = int.Parse(dstPos.Substring(1, 1)) - 1;
                            var srcPosX = srcPos.ToLowerInvariant()[0] - 97;
                            var srcPosY = int.Parse(srcPos.Substring(1, 1)) - 1;
                            newCbm2.pieces[dstPosX, dstPosY] = srcBoard.pieces[srcPosX, srcPosY];

                            srcBoard.travelMove = new Timeline.TravelMove5D(srcTimelineId, pgnTurn, srcBoard.isBlackBoard, srcPosY, srcPosX, dstTimelineId, dstTurn, dstPosY,dstPosX);
                            newCbm1.normalMove = new Timeline.Move2D(srcPosY, srcPosX, -1, -1);
                            newCbm2.previousBoardOverride = dstBoard;
                            if (branchExpected != dstBoardAlreadyPlayed)
                                throw new FormatException($"Expected a different type of move (branching vs nonbranching timtetravel move). At move: {moveold}");

                            newCbm2.normalMove = new Timeline.Move2D(-1, -1, dstPosY, dstPosX);

                            if (dstBoardAlreadyPlayed) // make a new TL
                            {
                                var orderedTls = this.Timelines.OrderBy(x => (x.timelineIndex.timeline + 0.5) * (x.timelineIndex.isNegative ? -1 : 1));
                                var newTLIndex = srcBoard.isBlackBoard ?
                                    // if black made the move
                                    orderedTls.First().timelineIndex :
                                    orderedTls.Last().timelineIndex;

                                var tl = new Timeline(this.boardHeight, this.boardWidth, new Timeline.TimelineIndex(srcBoard.isBlackBoard, newTLIndex.timeline + 1)) // TODO set previous board properly
                                {
                                    previousBoardIfBranching = srcBoard
                                };
                                this.Timelines.Add(tl);
                                tl.Boards.Add(newCbm2);
                            }
                            else // append onto dest tl
                            {
                                dstTL.Boards.Add(newCbm2); // TODO set previous board properly
                            }
                        }
                        else
                        {
                            var movechars = string.Join(null, moveFixed.Reverse().Take(4).Reverse());
                            var srcPos = movechars.Substring(0, 2);
                            var dstPos = movechars.Substring(2, 2);
                            var dstPosX = dstPos.ToLowerInvariant()[0] - 97;
                            var dstPosY = int.Parse(dstPos.Substring(1, 1)) - 1;
                            var srcPosX = srcPos.ToLowerInvariant()[0] - 97;
                            var srcPosY = int.Parse(srcPos.Substring(1, 1)) - 1;

                            var newCbm = new Timeline.ChessBoardData(srcBoard);
                            // incrememnt turn
                            newCbm.turn += newCbm.isBlackBoard ? 1 : 0;
                            newCbm.isBlackBoard ^= true;
                            // ---
                            newCbm.pieces[dstPos.ToLowerInvariant()[0] - 97, int.Parse(dstPos.Substring(1, 1)) - 1] = newCbm.pieces[srcPos.ToLowerInvariant()[0] - 97, int.Parse(srcPos.Substring(1, 1)) - 1];
                            newCbm.pieces[srcPos.ToLowerInvariant()[0] - 97, int.Parse(srcPos.Substring(1, 1)) - 1] = new ChessBoard.ChessPiece(ChessBoard.ChessPiece.PieceKind.Empty, false);

                            newCbm.normalMove = new Timeline.Move2D(srcPosY, srcPosX, dstPosY, dstPosX);
                            srcTL.Boards.Add(newCbm);
                        }
                    }
                }
            }

            this.CosmeticTurnOffset = cosmeticTurnOffset ?? 0;
            return this;
        }

        private struct PgnFenBoard {
            public string Fen;
            public string Timeline;
            public int Turn;
            public bool IsBlack;

            public PgnFenBoard(string s) {
                var splitted = s.Split(':').Select(x=>x.Trim()).ToArray();
                Fen = splitted[0];
                Timeline = splitted[1].TrimEnd('L') + "L";
                Turn = int.Parse(splitted[2]); 
                IsBlack = splitted[3].Single() switch { 'w' => false, 'b' => true, _=> throw new Exception("invalid color")};
            }

            public PgnFenBoard NormalizeSign() {
                return new PgnFenBoard() {
                    Fen = this.Fen,
                    Timeline = this.Timeline[0] == '-' ? this.Timeline : ("+"+this.Timeline.TrimStart('+')),
                    Turn = this.Turn,
                    IsBlack = this.IsBlack
                };
            }
        }
        public static BaseGameBuilder CreateFromFullPgn(string fullPgn) {
            var lines = fullPgn.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').Select(x=>x.Trim()).Where(x=>!string.IsNullOrWhiteSpace(x)).ToArray();
            var fens = lines.Reverse().SkipWhile(x => char.IsDigit(x[0])).TakeWhile(x => x[0] == '[' && !x.Contains('"')).Reverse().Select(x=>new PgnFenBoard(x.Substring(1,x.Length-2))).ToArray();

            var headerFields = lines.TakeWhile(x => x[0] == '[' && x.Contains('"'))
                .Select(x => x.Substring(1, x.Length - 2).Split(' ').Where(x => x.Length > 0).ToArray())
                .ToDictionary(x => x[0], x => x[1].Trim('"'));

            if (headerFields["Mode"] != "5D")
                throw new Exception("\"Mode\" must be 5D");

            var size = headerFields["Size"].Split('x').Select(x=>int.Parse(x)).ToArray();
            bool isEven = fens.Any(x => x.Timeline == "+0" || x.Timeline == "-0");

            BaseGameBuilder gb2 = isEven ? (BaseGameBuilder)new GameBuilderEven(size[0], size[1]) : new GameBuilderOdd(size[0], size[1]);
            var timelines = fens.Select(x => x.NormalizeSign()).GroupBy(x => x.Timeline);
            int cosmeticTurnOffset = timelines.Min(grp => grp.First().Turn);

            gb2.CosmeticTurnOffset = cosmeticTurnOffset;
            foreach (var tl in timelines) {
                var ordered = tl.OrderBy(x => x.Turn + (x.IsBlack ? 0.5f : 0)).ToArray();
                var builderTimeline = gb2[tl.Key];
                builderTimeline.SetTurnOffset(ordered[0].Turn - cosmeticTurnOffset, ordered[0].IsBlack);
                foreach (var board in ordered) {
                    builderTimeline.AddBoardFromFen(board.Fen);
                }
            }

            var pgnMoves = string.Join("\n", lines.Reverse().TakeWhile(x => Regex.IsMatch(x, @"^\d{1,}.")).Reverse().ToArray());
            gb2.Add5DPGNMoves(pgnMoves);
            return gb2;
        }
    }
}
