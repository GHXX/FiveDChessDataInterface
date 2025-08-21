using System;
using System.Collections.Generic;
using System.Linq;

namespace FiveDChessDataInterface.Util {
    public static class GameUtil {
        /// <summary>
        /// Reassigns the chessboard ids, to match the required constraints in the game. Preserves references.
        /// </summary>
        /// <param name="cbms"></param>
        /// <returns></returns>
        public static ChessBoardMemory[] ReassignBoardIds(ChessBoardMemory[] cbms) {
            if (cbms.DistinctBy(x => x.boardId).Count() != cbms.Length)
                throw new ArgumentException("There are duplicate board ids!");

            if (cbms.Min(x => x.boardId) != 0)
                throw new ArgumentException("The smallest boardId must be 0!");

            if (cbms.Max(x => x.boardId) != cbms.Length - 1)
                throw new ArgumentException($"The smallest boardId must be {cbms.Length - 1} for an array of length {cbms.Length}!");

            // constraits: boardid starts at 0, ends at boardcount-1
            // if boardid is sorted ascending, then the boards are also sorted by subturnIndex() and secondly, by abs(timeline), both ascending

            // TODO ThenBy(...) possibly ambiguous as 1L and -1L are the same value
            var sorted = cbms.OrderBy(x => x.GetSubturnIndex()).ThenBy(x => Math.Abs(x.timeline)).ToArray();

            var lastId = sorted.Last();
            var retArr = sorted;
            for (int currWantedBoardId = 0; currWantedBoardId < cbms.Length; currWantedBoardId++) {
                var board = retArr[currWantedBoardId];
                if (board.boardId != currWantedBoardId)
                    retArr = SwapTwoBoardIds(retArr, board.boardId, currWantedBoardId);
            }

            // at this point, sorting should be finished.
            if (retArr.Select((x, i) => (x, i)).Any(tpl => tpl.x.boardId != tpl.i))
                throw new Exception("Reassigning the board ids failed!");


            return retArr;
        }

        private static ChessBoardMemory[] SwapTwoBoardIds(ChessBoardMemory[] cbms, int id1, int id2) {
            if (id1 == id2)
                return cbms;

            var tempId = cbms.Max(x => x.boardId) + 1;
            var renamed1ToTemp = ChangeBoardId(cbms, id1, tempId); // rename id1 -> temp
            var renamed2To1 = ChangeBoardId(renamed1ToTemp, id2, id1); // rename id2 -> id1
            var renamedTempTo2 = ChangeBoardId(renamed2To1, tempId, id2); // renamde temp -> id2

            return renamedTempTo2;
        }

        private static ChessBoardMemory[] ChangeBoardId(ChessBoardMemory[] cbms, int idToChange, int newId) {
            if (idToChange == newId)
                return cbms;

            var ret = new List<ChessBoardMemory>();

            void SwapIntIfEq(ref int baseValue, int search, int newValue) {
                if (baseValue == search)
                    baseValue = newValue;
            }

            foreach (var c in cbms) {
                var c2 = c;

                SwapIntIfEq(ref c2.boardId, idToChange, newId);
                SwapIntIfEq(ref c2.nextInTimelineBoardId, idToChange, newId);
                SwapIntIfEq(ref c2.previousBoardId, idToChange, newId);
                SwapIntIfEq(ref c2.createdBoardID, idToChange, newId);
                ret.Add(c2);
            }

            return ret.ToArray();
        }
    }
}
