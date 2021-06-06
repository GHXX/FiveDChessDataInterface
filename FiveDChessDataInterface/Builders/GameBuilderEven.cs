using System;
using System.Collections.Generic;
using System.Text;

namespace FiveDChessDataInterface.Builders
{
    public class GameBuilderEven : BaseGameBuilder
    {
        public GameBuilderEven(int boardHeight, int boardWidth) : base(true, boardHeight, boardWidth)
        {

        }

        protected override void SetupInitialTimelines()
        {
            Timelines.Add(new Timeline(boardHeight, boardWidth, "+0L"));
            Timelines.Add(new Timeline(boardHeight, boardWidth, "-0L"));
        }
    }
}
