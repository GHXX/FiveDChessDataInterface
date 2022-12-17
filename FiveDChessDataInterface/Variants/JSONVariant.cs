using FiveDChessDataInterface.Builders;
using FiveDChessDataInterface.Exceptions;
using FiveDChessDataInterface.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiveDChessDataInterface.Variants
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Include)]
    public class JSONVariant
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Author")]
        public string Author { get; set; } = "Unknown";

        [JsonProperty("Timelines")]
        public Dictionary<string, string[]> Timelines { get; set; }

        [JsonProperty("CosmeticTurnOffset")]
        public int? CosmeticTurnOffset { get; set; } = null;

        private string GetAnyExpandedBoardFen() => FenUtil.ExpandFen(Timelines.SelectMany(x => x.Value).First(x => x != null));


        // Need to expand the fen first, because the digit 8 for example would represent a width of 8, but is only one wide
        [JsonIgnore()]
        public int Height => GetAnyExpandedBoardFen().Count(x => x == '/') + 1; // count number of newline delimeters. 

        [JsonIgnore()]
        public int Width => GetAnyExpandedBoardFen().TakeWhile(x => x != '/').Count(); // count number of pieces in the first line

        [JsonProperty("GameBuilderOverride")]
        public string GameBuilderOverride { get; set; } = null;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="VariantLoadException">If the given <see cref="GameBuilderOverride"/> is invalid and is not equal to the name of a class that inherits <see cref="BaseGameBuilder"/> </exception>
        public BaseGameBuilder GetGameBuilder()
        {
            var isEven = Timelines.Count % 2 == 0;
            BaseGameBuilder gameBuilder;
            if (GameBuilderOverride == null)
            {
                gameBuilder = isEven ? new GameBuilderEven(Width, Height) : (BaseGameBuilder)new GameBuilderOdd(Width, Height);
            }
            else
            {
                var gbs = typeof(BaseGameBuilder).Assembly.GetTypes().Where(x => typeof(BaseGameBuilder).IsAssignableFrom(x) && x != typeof(BaseGameBuilder)).ToDictionary(x => x.Name, x => x);
                if (gbs.TryGetValue(GameBuilderOverride, out var gbType))
                {
                    gameBuilder = (BaseGameBuilder)Activator.CreateInstance(gbType, new object[] { Width, Height });
                }
                else
                {
                    throw new VariantLoadException("Invalid gamebuilder given!");
                }
            }

            foreach (var tl in Timelines)
            {
                var timelineIndex = tl.Key;
                var boards = tl.Value;


                var nullBoardCount = boards.TakeWhile(board => board == null).Count(); // get the number of leading null-boards
                var isBlack = nullBoardCount % 2 == 1; // check if the first existent board will be black
                var turnOffset = nullBoardCount / 2;
                gameBuilder[timelineIndex].SetTurnOffset(turnOffset, isBlack);
                foreach (var board in boards.Skip(nullBoardCount))
                {
                    gameBuilder[timelineIndex].AddBoardFromFen(board);
                }
            }

            if (CosmeticTurnOffset.HasValue)
                gameBuilder.CosmeticTurnOffset = CosmeticTurnOffset.Value;

            return gameBuilder;
        }
    }
}
