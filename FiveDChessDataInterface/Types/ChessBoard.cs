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
                // TODO possibly add console message when an unknown piece has been loaded?
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
                    PieceKind.Princess => "S",
                    PieceKind.Brawn => "W",
                    PieceKind.RoyalQueen => "Y",
                    PieceKind.CommonKing => "C",
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
                // PIECE ID 9 is currently UNKNOWN
                Brawn=10,
                Princess,
                RoyalQueen,
                CommonKing
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

        // TODO: investiage if this is actually an int!

        // only the least significatn byte of this number is important (it's probaby an enum that got byte aligned in a struct)
        // 0 means that no move has been made on this board
        // 1 means that a standard physical move has been made on this board
        // 2 means that a branching jump was made on this board
        // 3 means that a non-branching jump was made on this board
        // 4 means that a piece jump from another board onto this board
        public int moveType;
        // Source and destination of the move made from this board
        public int moveSourceL;
        public int moveSourceT;
        public int moveSourceIsBlack; // the 5th dimension :)
        public int moveSourceY;
        public int moveSourceX;
        public int moveDestL;
        public int moveDestT;
        public int moveDestIsBlack;
        public int moveDestY;
        public int moveDestX;
        
        public int creatingMoveNumber; // the moveNumber of the move that created this board
        public int nextInTimelineBoardId;// The id of the next board in the same timeline as this one
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
            hash.Add("move source timeline", moveSourceL);
            hash.Add("move source time", moveSourceT);
            hash.Add("move source piece color", moveSourceIsBlack);
            hash.Add("moveSourceY", moveSourceY);
            hash.Add("moveSourceX", moveSourceX);
            hash.Add("move dest timeline", moveDestL);
            hash.Add("move source time", moveDestT);
            hash.Add("move dest piece color", moveDestIsBlack);
            hash.Add("moveDestY", moveDestY);
            hash.Add("moveDestX", moveDestX);
            hash.Add("previous board identifier?", creatingMoveNumber);
            hash.Add("nextBoardId", nextInTimelineBoardId);
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
