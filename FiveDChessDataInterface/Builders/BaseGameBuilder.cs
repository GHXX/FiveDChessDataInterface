using FiveDChessDataInterface.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiveDChessDataInterface.Builders
{
    public abstract class BaseGameBuilder
    {
        public readonly int boardHeight;
        public readonly int boardWidth;

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
                // last subturn + 1, or the offset if there are no boards
                var nextSubturn = this.Boards.Any() ? this.Boards.Max(x => x.turn * 2 + (x.isBlackBoard ? 1 : 0)) + 1 : this.subturnOffset;

                var b = new ChessBoardData(this.boardHeight, this.boardWidth, nextSubturn / 2, nextSubturn % 2 == 1, fen);
                this.Boards.Add(b);
                return this;
            }

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

                public override bool Equals(object obj)
                {
                    return obj is TimelineIndex tli && this == tli;
                }

                public override int GetHashCode()
                {
                    return (this.isNegative ? (0x1 << 31) : 0) | this.timeline; // simply set the msb to the sign.
                }
            }

            public class ChessBoardData
            {
                private readonly int boardHeight;
                private readonly int boardWidth;
                internal ChessBoard.ChessPiece[,] pieces;
                internal int turn;
                internal bool isBlackBoard;

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


                private void LoadFEN(string fen)
                {
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
            var allBoards = new List<ChessBoardMemory>();
            int nextFreeId = 0;
            foreach (var tl in this.Timelines)
            {
                int lastBoardId = -1;
                var timelineCbms = new List<ChessBoardMemory>();
                foreach (var board in tl.Boards)
                {
                    var cbm = new ChessBoardMemory();

                    cbm.boardId = nextFreeId++;
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

                    if (lastBoardId != -1) // set the nextintimelineboardid of the last board to the current id
                    {
                        var lastCbm = timelineCbms.Last();
                        timelineCbms.RemoveAt(timelineCbms.Count - 1);
                        lastCbm.nextInTimelineBoardId = cbm.boardId;
                        lastCbm.moveType = 5;
                        timelineCbms.Add(lastCbm);
                    }

                    cbm.previousBoardId = lastBoardId;
                    cbm.createdBoardID = -1;

                    cbm.ttPieceOriginId = -1;
                    cbm.ttMoveSourceY = -1;
                    cbm.ttMoveSourceX = -1;
                    cbm.ttMoveDestY = -1;
                    cbm.ttMoveDestX = -1;
                    lastBoardId = cbm.boardId;

                    timelineCbms.Add(cbm);
                }

                allBoards.AddRange(timelineCbms);
            }

            var idsFixed = GameUtil.ReassignBoardIds(allBoards.ToArray());
            return idsFixed;
        }


        public ChessBoard[] Build() => BuildCbms().Select(x => new ChessBoard(x, this.boardWidth, this.boardHeight)).ToArray();

    }
}
