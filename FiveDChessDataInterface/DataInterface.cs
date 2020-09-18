using FiveDChessDataInterface.Exceptions;
using FiveDChessDataInterface.MemoryHelpers;
using FiveDChessDataInterface.Types;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace FiveDChessDataInterface
{
    public class DataInterface
    {
        const string executableName = "5dchesswithmultiversetimetravel";
        public Process GameProcess { get; }
        public MemoryLocation<IntPtr> MemLocChessArrayPointer { get; private set; }
        public MemoryLocation<int> MemLocChessArraySize { get; private set; }
        public MemoryLocation<int> MemLocChessBoardSizeWidth { get; private set; }
        public MemoryLocation<int> MemLocChessBoardSizeHeight { get; private set; }
        public MemoryLocation<int> MemLocCurrentPlayersTurn { get; private set; }

        public IntPtr GetGameHandle() => this.GameProcess.Handle;
        public IntPtr GetEntryPoint() => this.GameProcess.MainModule.BaseAddress;

        public static bool TryCreateAutomatically(out DataInterface di)
        {
            var filteredProcesses = Process.GetProcessesByName(executableName);
            if (filteredProcesses.Length == 1)
            {
                di = new DataInterface(filteredProcesses[0]);
                return true;
            }

            di = null;
            return false;
        }

        public static bool TryCreateAutomatically(out DataInterface di, out int numberOfProcesses)
        {
            var filteredProcesses = Process.GetProcessesByName(executableName);
            numberOfProcesses = filteredProcesses.Length;

            if (filteredProcesses.Length == 1)
            {
                di = new DataInterface(filteredProcesses[0]);
                return true;
            }

            di = null;
            return false;
        }

        public static DataInterface CreateAutomatically()
        {
            if (TryCreateAutomatically(out var di, out int numberOfProcesses))
                return di;

            if (numberOfProcesses == 0)
                throw new AmbiguousMatchException("There was more than one 5d chess process running.");
            else
                throw new ProcessNotFoundException("The 5d chess process could not be found.");
        }

        public DataInterface(Process gameProcess)
        {
            this.GameProcess = gameProcess;
        }

        public void Initialize()
        {
            CalculatePointers();
        }



        private void CalculatePointers()
        {
            var bytesToFind = new byte[] { 0x4c, 0x8b, 0x35,
                0x90, 0x90, 0x90, 0x90,// = 0x55, 0xfa, 0x0c, 0x00 WILDCARDS
                0x4c, 0x69, 0xf8,
                0x90, 0x90, 0x90, 0x90,
                0x4c, 0x89, 0xf0,
                0x4c, 0x01, 0xf8
            };

            var results = MemoryUtil.FindMemoryWithWildcards(GetGameHandle(), GetEntryPoint(), (uint)this.GameProcess.MainModule.ModuleMemorySize, bytesToFind);

            if (results.Count != 1)
            {
                throw new AmbiguousMatchException($"{results.Count} memory locations matched, which is not 1!");
            }

            var result = results.First();
            var resultAddress = result.Key;
            var resultBytes = result.Value;

            var chessboardPointerLocation = IntPtr.Add(resultAddress, BitConverter.ToInt32(resultBytes, 3) + 7);

            this.MemLocChessArrayPointer = new MemoryLocation<IntPtr>(GetGameHandle(), chessboardPointerLocation);
            this.MemLocChessArraySize = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, -8);
            this.MemLocChessBoardSizeWidth = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, 0xA8+0x4);
            this.MemLocChessBoardSizeHeight = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, 0xA8);
            this.MemLocCurrentPlayersTurn = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, 0x110);
        }

        /// <summary>
        /// Gets all currently available chessboards. Chessboards are ordered by their id, indicating when they were created. (<see cref="ChessBoard.Id"/>).
        /// </summary>
        /// <returns>An array of <see cref="ChessBoard"/>, each representing a single chessboard.</returns>
        public ChessBoard[] GetChessBoards()
        {
            throw new NotImplementedException();
        }

        public int GetChessBoardAmount() => this.MemLocChessArraySize.GetValue();

        /// <summary>
        /// Gets the current chessboard size.
        /// </summary>
        /// <returns>A <see cref="ChessBoardSize"/> object representing the size of all chessboards.</returns>
        public ChessBoardSize GetChessBoardSize() => new ChessBoardSize(this.MemLocChessBoardSizeWidth.GetValue(), this.MemLocChessBoardSizeHeight.GetValue());

        /// <summary>
        /// Gets the current player's turn.
        /// </summary>
        /// <returns>Returns 0 if it's WHITE's turn, and 1 if it's BLACK's turn.</returns>
        public int GetCurrentPlayersTurn() => this.MemLocCurrentPlayersTurn.GetValue();

    }
}
