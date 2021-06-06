using System;
using System.Collections.Generic;
using System.Text;

namespace FiveDChessDataInterface.Builders
{
    public class GameBuilderOdd : BaseGameBuilder
    {
        public GameBuilderOdd(int boardHeight, int boardWidth) : base(false, boardHeight, boardWidth)
        {

        }

        protected override void SetupInitialTimelines()
        {            
            Timelines.Add(new Timeline(boardHeight, boardWidth, "0L"));
        }
    }
}
