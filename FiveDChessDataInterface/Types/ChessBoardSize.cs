namespace FiveDChessDataInterface.Types {
    public struct ChessBoardSize {
        public int Width { get; }
        public int Height { get; }

        public ChessBoardSize(int width, int height) {
            Width = width;
            Height = height;
        }
    }
}
