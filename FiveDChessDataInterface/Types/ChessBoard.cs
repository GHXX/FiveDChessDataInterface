using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

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
                PieceKind kind;
                if(Enum.IsDefined(typeof(PieceKind), pieceByte)){
                    kind=(PieceKind)pieceByte;
                }
                else{
                    Console.WriteLine($"{pieceByte}");
                    kind=PieceKind.Unknown;
                }

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
                    PieceKind.AlsoUnknown => "?",
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
                    PieceKind.Commoner => "C",
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
                AlsoUnknown,
                Brawn,
                Princess,
                RoyalQueen,
                Commoner
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

        public int boardId;
        public int timeline;
        public int turn;
        public int isBlacksMove;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8 * 8 * 2)]
        public byte[] positionData;

        public int moveNumber; //-1 until a before is made from this board. After a move is made, it becomes the number of moves made before this one.
        
        //val05 probably isn't an int - values seen: 1 257, 513, 1009807616
        // The low byte indicates the move type:
        // 0 means that no move has been made on this board
        // 1 means that a standard physical move has been made on this board
        // 2 means that a branching jump was made on this board
        // 3 means that a non-branching jump was made on this board
        // 4 means that a piece jumped from another board onto this board
        public int val05;
        
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
        public int val19;

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
    }
}
