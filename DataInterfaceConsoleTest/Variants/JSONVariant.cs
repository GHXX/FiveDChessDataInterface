using FiveDChessDataInterface.Builders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataInterfaceConsoleTest.Variants
{
    class JSONVariant
    {
        [JsonProperty("Name")]
        string Name;
        
        [JsonProperty("Author")]
        string Author;

        [JsonProperty("Timelines")]
        Dictionary<string, string[]> Timelines;

        public BaseGameBuilder GetGameBuilder()
        {
            var isEven = Timelines.Count % 2 == 0;
            var firstBoard = Timelines.First().Value.First(board => board != null);
            var width = firstBoard.Split("/")[0].Length; 
            var height = firstBoard.Split("/").Length;
            BaseGameBuilder gameBuilder = isEven ? 
                (BaseGameBuilder) new GameBuilderEven(width, height) : 
                (BaseGameBuilder) new GameBuilderOdd(width, height) ;
            foreach (var key in Timelines.Keys)
            {
                var nullBoards = Timelines[key].TakeWhile(board => board == null).Count();
                var isBlack = nullBoards % 2 == 1;
                var turnOffset = nullBoards / 2;
                gameBuilder[key].SetTurnOffset(turnOffset, isBlack);
                foreach(var value in Timelines[key])
                {
                    gameBuilder[key].AddBoardFromFen(value);
                }
            }
            return gameBuilder;
        }
    }
}
