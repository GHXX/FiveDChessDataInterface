using FiveDChessDataInterface.Builders;
using FiveDChessDataInterface.Util;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace DataInterfaceConsoleTest.Variants
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Include)]
    class JSONVariant
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Author")]
        public string Author { get; set; } = "Unknown";

        [JsonProperty("Timelines")]
        public Dictionary<string, string[]> Timelines { get; set; }

        private string GetAnyExpandedBoardFen() => FenUtil.ExpandFen(this.Timelines.SelectMany(x => x.Value).First(x => x != null));


        // Need to expand the fen first, because the digit 8 for example would represent a width of 8, but is only one wide
        [JsonIgnore()]
        public int Height => GetAnyExpandedBoardFen().Count(x => x == '/') + 1; // count number of newline delimeters. 

        [JsonIgnore()]
        public int Width => GetAnyExpandedBoardFen().TakeWhile(x => x != '/').Count(); // count number of pieces in the first line


        public BaseGameBuilder GetGameBuilder()
        {
            var isEven = this.Timelines.Count % 2 == 0;
            BaseGameBuilder gameBuilder = isEven ? new GameBuilderEven(Width, Height) : (BaseGameBuilder)new GameBuilderOdd(Width, Height);

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
