namespace FiveDChessDataInterface.Builders
{
    public class GameBuilderOdd : BaseGameBuilder
    {
        public GameBuilderOdd(int boardHeight, int boardWidth) : base(false, boardHeight, boardWidth)
        {

        }

        protected override void SetupInitialTimelines()
        {
            this.Timelines.Add(new Timeline(this.boardHeight, this.boardWidth, "0L"));
        }
    }
}
