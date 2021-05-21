using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace FiveDChessDataInterface
{
    public class ChessBoard
    {
        private ChessPiece[] _pieces;

        /// <summary>
        /// Contains a neater representation of the pieces that are available.
        /// WARNING: WRITING TO THIS WILL UPDATE THE INTERNAL STATE, BUT NOT WRITE IT TO GAME MEMORY!
        /// </summary>
        public ChessPiece[] Pieces
        {
            get
            {
                return this._pieces; // TODO maybe make readonly
            }
            set
            {
                SetPieces(value);
            }
        }

        /// <summary>
        /// THIS MODIFIES THE CHESSBOARD VALUES, BUT DOES NOT UPDATE THE BITBOARDS JUST YET. SO ONLY REPLACE PIECES, BUT DONT ADD OR REMOVE!
        /// </summary>
        /// <param name="newPieces"></param>
        public void SetPieces(ChessPiece[] newPieces)
        {
            // TODO FIND AND UPDATE BITBOARDS!

            if (newPieces.Length * 2 > this.cbm.positionData.Length)
            {
                throw new ArgumentException("Array size mismatch!");
            }

            for (int i = 0; i < newPieces.Length; i++)
            {
                var newPiecesRow = i / this.width;
                var newPiecesCol = i % this.width;

                var cbmpos = (newPiecesRow * 8 + newPiecesCol) * 2;
                this.cbm.positionData[cbmpos] = (byte)newPieces[i].Kind;
                var color = newPieces[i];
                this.cbm.positionData[cbmpos + 1] = color.IsEmpty ? (byte)0 : color.IsWhite ? (byte)1 : (byte)2;
            }

            // recompute the _pieces array
            UpdatePieceArrayFromInternalData();
            // and validate the pieces
            if (this._pieces.Length != newPieces.Length)
                throw new Exception("Write validation length check failed!");

            for (int i = 0; i < this._pieces.Length; i++)
            {
                if (!this._pieces[i].Equals(newPieces[i]))
                    throw new Exception("Write validation failed!");
            }
        }





        public int width;
        public int height;

        public ChessBoardMemory cbm;

        public ChessBoard(ChessBoardMemory mem, int width, int height)
        {
            this.width = width;
            this.height = height;
            this.cbm = mem;
            UpdatePieceArrayFromInternalData();
        }

        private void UpdatePieceArrayFromInternalData()
        {
            this._pieces = new ChessPiece[this.width * this.height];
            for (int x = 0; x < this.width; x++)
            {
                for (int y = this.height - 1; y >= 0; y--)
                {
                    var srcIndex = (x * 8 + y) * 2;
                    this._pieces[x * this.height + y] = ChessPiece.ParseFromTwoByteNotation(this.cbm.positionData[srcIndex], this.cbm.positionData[srcIndex + 1]);
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
                Brawn = 10,
                Princess,
                RoyalQueen,
                CommonKing
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is ChessPiece cp)
                    return this.Kind == cp.Kind && this.IsBlack == cp.IsBlack;
                else
                    return false;

            }
        }

        public override string ToString()
        {
            var nonempty = this._pieces.Where(x => x.Kind != ChessPiece.PieceKind.Empty).ToList();
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
        public byte moveType;
        public byte bval1;
        public byte bval2;
        public byte bval3;


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

        public static byte[] ToByteArray(ChessBoardMemory cbm)
        {
            var size = Marshal.SizeOf<ChessBoardMemory>();

            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(cbm, ptr, true);
            var bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, size);
            Marshal.FreeHGlobal(ptr);

            return bytes;
        }

        /**
         * Returns a dictionary of all the fields, with their name
         * this is just so we can easily print out all the fields with their name
         */
        public Dictionary<string, int> GetFieldsWithNames()
        {
            Dictionary<string, int> hash = new Dictionary<string, int>();
            hash.Add("boardId", this.boardId);
            hash.Add("timeline", this.timeline);
            hash.Add("turn", this.turn);
            hash.Add("isBlacksMove", this.isBlacksMove);

            // board data

            // after board data
            hash.Add("move turn", this.moveTurn);
            hash.Add("move type", this.moveType & 0xFF);
            hash.Add("move source timeline", this.moveSourceL);
            hash.Add("move source time", this.moveSourceT);
            hash.Add("move source piece color", this.moveSourceIsBlack);
            hash.Add("moveSourceY", this.moveSourceY);
            hash.Add("moveSourceX", this.moveSourceX);
            hash.Add("move dest timeline", this.moveDestL);
            hash.Add("move source time", this.moveDestT);
            hash.Add("move dest piece color", this.moveDestIsBlack);
            hash.Add("moveDestY", this.moveDestY);
            hash.Add("moveDestX", this.moveDestX);
            hash.Add("previous board identifier?", this.creatingMoveNumber);
            hash.Add("nextBoardId", this.nextInTimelineBoardId);
            hash.Add("previousBoardId", this.previousBoardId);
            hash.Add("createdBoardID", this.createdBoardID);

            hash.Add("ttPieceOriginId", this.ttPieceOriginId);

            // unconfirmed :

            hash.Add("ttMoveSourceY", this.ttMoveSourceY);
            hash.Add("ttMoveSourceX", this.ttMoveSourceX);
            hash.Add("ttMoveDestY", this.ttMoveDestY);
            hash.Add("ttMoveDestX", this.ttMoveDestX);

            return hash;
        }
    }
}
