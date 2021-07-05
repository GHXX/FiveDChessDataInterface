namespace FiveDChessDataInterface.Types
{
    public enum GameState : int
    {
        NotStarted,
        Running,
        EndedDraw,
        EndedWhiteWon,
        EndedBlackWon,
        /// <summary>
        /// Only returned if reading yielded an unexpected constellation of values
        /// </summary>
        Unknown
    }
}