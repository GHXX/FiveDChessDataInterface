using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace FiveDChessDataInterface
{
    public class ChessBoard
    {
        public ChessPiece[] Pieces { get; set; }

        // 84 bytes remaining



        public int width;
        public int height;

        public ChessBoardMemory cbm;

        public ChessBoard(ChessBoardMemory mem, int width, int height)
        {
            this.width = width;
            this.height = height;
            this.cbm = mem;

            this.Pieces = new ChessPiece[width * height];
            for (int x = 0; x < width; x++)
            {
                for (int y = height - 1; y >= 0; y--)
                {
                    var srcIndex = (x * 8 + y) * 2;
                    this.Pieces[x * height + y] = ChessPiece.ParseFromTwoByteNotation(mem.positionData[srcIndex], mem.positionData[srcIndex + 1]);
                }
            }
        }

        public class ChessPiece
        {
            public PieceKind Kind { get; }
            public bool IsBlack { get; }
            public bool IsWhite { get => this.Kind != PieceKind.Empty && !this.IsBlack; }
            public bool IsEmpty { get => this.Kind == PieceKind.Empty; }

            public static ChessPiece ParseFromTwoByteNotation(int pieceByte, byte colorByte)
            {
                if (colorByte > 2)
                {
                    throw new InvalidDataException("Color data for this piece was bigger than 2! " + colorByte);
                }

                var isBlack = colorByte == 2;
                var kind = Enum.IsDefined(typeof(PieceKind), pieceByte) ? (PieceKind)pieceByte : PieceKind.Unknown;

                return new ChessPiece(kind, isBlack);
            }

            public override string ToString()
            {
                if (this.Kind == PieceKind.Empty)
                {
                    return string.Empty;
                }

                return $"[{(this.IsBlack ? "B" : "W")}]{this.Kind}";
            }

            public string SingleLetterNotation()
            {
                return this.Kind switch
                {
                    PieceKind.Unknown => "?",
                    PieceKind.Pawn => "P",
                    PieceKind.Knight => "N",
                    PieceKind.Bishop => "B",
                    PieceKind.Rook => "R",
                    PieceKind.Queen => "Q",
                    PieceKind.King => "K",
                    PieceKind.Unicorn => "U",
                    PieceKind.Dragon => "D",
                    PieceKind.Princess => "q",
                    _ => throw new NotImplementedException()
                };
            }

            public ChessPiece(PieceKind kind, bool isBlack)
            {
                this.Kind = kind;
                this.IsBlack = isBlack;
            }

            public enum PieceKind : int
            {
                Unknown = -1,
                Empty = 0,
                Pawn,
                Knight,
                Bishop,
                Rook,
                Queen,
                King,
                Unicorn,
                Dragon,
                Princess // TODO confirm value
            }
        }

        public override string ToString()
        {
            var nonempty = this.Pieces.Where(x => x.Kind != ChessPiece.PieceKind.Empty).ToList();
            return $"Id: {this.cbm.boardId}, T{this.cbm.turn + 1}L{this.cbm.timeline}, PieceCount: {nonempty.Count(x => x.IsWhite)}/{nonempty.Count(x => x.IsBlack)} ";
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct ChessBoardMemory
    {
        public const int structSize = 228;

        // I am 90% sure all of these are correct, the comments help define their exact behavior
        // it's possible that some of these are slightly wrong, or there are edge cases that I have
        // not fully tested


        // when a board is created it is given a boardID, these are given sequentially
        // and can be used to determine things like what order moves were made
        public int boardId;

        public int timeline;
        public int turn;
        public int isBlacksMove;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8 * 8 * 2)]
        public byte[] positionData;


        
        // this is -1 when no move has been made to advance this board
        // every time a single move is made, the boards that are advanced by that move get 
        // given a moveTurn value of the previous highest move turn+1 
        // generally each moveTurn value is only given to one board, except when a non branching jump is made
        // then two boards share the same move turn, because they were both advanced by the same single move
        public int moveTurn;


        // the below values through previousBoardMoveTurn are zero for a board that is active and waiting for a move

        // only the least significatn byte of this number is important (it's probaby an enum that got byte aligned in a struct
        // 0 means that no move has been made on this board
        // 1 means that a standard physical move has been made on this board
        // 2 means that a branching jump was made on this board
        // 3 means that a non-branching jump was made on this board
        // 4 means that a piece jump from another board onto this board
        public int moveType;
        // the number of the timeline that the move that advanced this board was from
        // generally this is equal to the universe of the board, for physical moves and jumps originating from this board,
        // but for jumps that land on this board it is not true
        public int moveSourceUniverse;
        // same deal as moveSourceUniverse except it's the time/turn of the move
        public int moveSourceTime;
        // color of the piece that did the move that advanced this board,
        // should be the same as the isBlacksMove
        public int movePieceColor;
        public int moveSourceY;
        public int moveSourceX;
        public int moveDestUniverse;
        public int moveDestTime;
        public int movePieceOwner;
        public int moveDestY;
        public int moveDestX;
        // the moveTurn of the board that made the move that created this board
        // if this board was created by a branching move, then this value is the value from
        // the board that the piece jump from
        // to be honest i'm not actually sure what is value is used for, but so far as i can tell,
        // this is the correct identification of it's behavior
        public int previousBoardMoveTurn;
        public int nextBoardId; // the id of the board that comes after this board, should always be the board one to the right on the same timeline as this board
        public int previousBoardId; // the id of the board that was before this board, or this board branches off after
        public int createdBoardID; // if the move made on this board is branching, this is the board id of the board of the new board

        public int ttPieceOriginId; // the board id where this piece came from, or -1 if no timetravel happened

        // unconfirmed :

        public int ttMoveSourceY; // source timetravel move y (on the board where the piece disappeared) if source x and y are -1 then the piece is appearing on this board, coming from somewhere else
        public int ttMoveSourceX; // source timetravel move X
        public int ttMoveDestY;  // dest timetravel move y (on the board where the piece appeared) if dest x and y are -1 then the piece is disappearing on this board, going to somewhere else
        public int ttMoveDestX;

        // -----------

        public static ChessBoardMemory ParseFromByteArray(byte[] bytes)
        {
            if (Marshal.SizeOf<ChessBoardMemory>() != structSize)
                throw new InvalidOperationException("The size of this struct is not what it should be.");


            var gch = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var s = Marshal.PtrToStructure<ChessBoardMemory>(gch.AddrOfPinnedObject());
            gch.Free();

            return s;
        }

        /**
         * Returns a dictionary of all the fields, with their name
         * this is just so we can easily print out all the fields with their name
         */
        public Dictionary<string, int> GetFieldsWithNames()
        {
            Dictionary<string, int> hash = new Dictionary<string, int>();
            hash.Add("boardId", boardId);
            hash.Add("timeline", timeline);
            hash.Add("turn", turn);
            hash.Add("isBlacksMove", isBlacksMove);

            // board data

            // after board data
            hash.Add("move turn", moveTurn);
            hash.Add("move type", moveType & 0xFF);
            hash.Add("move source universe", moveSourceUniverse);
            hash.Add("move source time", moveSourceTime);
            hash.Add("move piece color", movePieceColor);
            hash.Add("moveSourceY", moveSourceY);
            hash.Add("moveSourceX", moveSourceX);
            hash.Add("moveDestUniverse", moveDestUniverse);
            hash.Add("moveDestTime", moveDestTime);
            hash.Add("movePieceOwner", movePieceOwner);
            hash.Add("moveDestY", moveDestY);
            hash.Add("moveDestX", moveDestX);
            hash.Add("previous board identifier?", val16);
            hash.Add("nextBoardId", nextBoardId);
            hash.Add("previousBoardId", previousBoardId);
            hash.Add("createdBoardID", createdBoardID);

            hash.Add("ttPieceOriginId", ttPieceOriginId);

            // unconfirmed :

            hash.Add("ttMoveSourceY", ttMoveSourceY);
            hash.Add("ttMoveSourceX", ttMoveSourceX);
            hash.Add("ttMoveDestY", ttMoveDestY);
            hash.Add("ttMoveDestX", ttMoveDestX);

            return hash;
        }
    }
}