using FiveDChessDataInterface.Builders;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace DataInterfaceConsoleTest.Variants
{
    class JSONVariant
    {
        [JsonProperty("Name")]
        public string Name { get; private set; }

        [JsonProperty("Author")]
        public string Author { get; private set; }

        [JsonProperty("Timelines")]
        public Dictionary<string, string[]> Timelines { get; private set; }

        public BaseGameBuilder GetGameBuilder()
        {
            var isEven = this.Timelines.Count % 2 == 0;
            var anyBoard = this.Timelines.SelectMany(x => x.Value).First(board => board != null);
            var width = anyBoard.Split("/")[0].Length;
            var height = anyBoard.Split("/").Length;
            BaseGameBuilder gameBuilder = isEven ? new GameBuilderEven(width, height) : (BaseGameBuilder)new GameBuilderOdd(width, height);

            foreach (var (timelineIndex, boards) in this.Timelines)
            {
                var nullBoards = boards.TakeWhile(board => board == null).Count(); // get the number of leading null-boards
                var isBlack = nullBoards % 2 == 1; // check if the first existent board will be black
                var turnOffset = nullBoards / 2;
                gameBuilder[timelineIndex].SetTurnOffset(turnOffset, isBlack);
                foreach (var board in boards)
                {
                    gameBuilder[timelineIndex].AddBoardFromFen(board);
                }
            }
            return gameBuilder;
        }
    }
}
