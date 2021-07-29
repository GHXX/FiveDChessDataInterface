namespace FiveDChessDataInterface.Builders
{
    public class GameBuilderEven : BaseGameBuilder
    {
        public GameBuilderEven(int boardHeight, int boardWidth) : base(true, boardHeight, boardWidth)
        {

        }

        protected override void SetupInitialTimelines()
        {
            this.Timelines.Add(new Timeline(this.boardHeight, this.boardWidth, "+0L"));
            this.Timelines.Add(new Timeline(this.boardHeight, this.boardWidth, "-0L"));
        }
    }
}
