using FiveDChessDataInterface.Exceptions;
using FiveDChessDataInterface.MemoryHelpers;
using FiveDChessDataInterface.Types;
using FiveDChessDataInterface.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace FiveDChessDataInterface
{
    public class DataInterface
    {
        const string executableName = "5dchesswithmultiversetimetravel";
        public Process GameProcess { get; }
        public MemoryLocation<IntPtr> MemLocChessArrayPointer { get; private set; } // points to the chessboard array.
        public MemoryLocation<int> MemLocChessArrayElementCount { get; private set; } // located right before the chessboard array capactiy
        public MemoryLocation<int> MemLocChessArrayCapacity { get; private set; } // located right before the chessboard array pointer
        public MemoryLocation<int> MemLocChessBoardSizeWidth { get; private set; }
        public MemoryLocation<int> MemLocChessBoardSizeHeight { get; private set; }
        public MemoryLocation<int> MemLocCurrentPlayersTurn { get; private set; }
        //public MemoryLocation<int> MemLocInGameEndedScreen { get; private set; } // if 1 then the "you lost" / "you won" screen is shown
        public MemoryLocation<int> MemLocGameEndedWinner { get; private set; } // if 0xFFFF FFFF then the game is still running, 0 is a win for white, or unstarted, 1 a win for black or a draw
        public MemoryLocation<int> MemLocGameState { get; private set; } // if 0 then the game is running or unstarted, 1 means someone won, 2 is a draw
        public MemoryLocation<int> MemLocWhiteTime { get; private set; }
        public MemoryLocation<int> MemLocBlackTime { get; private set; }
        public MemoryLocation<int> MemLocWhiteIncrement { get; private set; }
        public MemoryLocation<int> MemLocBlackIncrement { get; private set; }
        public MemoryLocation<int> MemLocCosmeticTurnOffset { get; private set; }

        private MemoryLocation<uint> MemLocWhiteTimelineCountInternal { get; set; }
        private MemoryLocation<int> MemLocSomeTurnCountOrSomething { get; set; }
        private MemoryLocation<int> MemLocProbablyBoardCount { get; set; }
        private MemoryLocation<uint> MemLocBlackTimelineCountInternalInverted { get; set; }

        // ONLY TESTED FOR ODD NUMBER OF STARTING BOARDS!!
        // TODO TEST ON EVEN NUMBER OF STARTING TIMELINES
        public uint GetNumberOfWhiteTimelines() => this.MemLocWhiteTimelineCountInternal.GetValue() - 1;
        public uint GetNumberOfBlackTimelines() => (uint)0xFFFF_FFFF - this.MemLocBlackTimelineCountInternalInverted.GetValue();


        public int GetWT() => this.MemLocWhiteTime.GetValue() + this.MemLocWhiteIncrement.GetValue();
        public int GetBT() => this.MemLocBlackTime.GetValue() + this.MemLocBlackIncrement.GetValue();
        public int GetCurT() => this.MemLocCurrentPlayersTurn.GetValue() == 0 ? GetWT() : GetBT();


        public IntPtr GetGameHandle() => this.GameProcess.Handle;
        public IntPtr GetEntryPoint() => this.GameProcess.MainModule.BaseAddress;

        private AssemblyHelper asmHelper;
        private readonly SuspendGameProcessLock suspendGameprocessLock;

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
            this.suspendGameprocessLock = new SuspendGameProcessLock(this.GameProcess.Handle);
        }

        public void Initialize()
        {
            Thread.Sleep(250); // wait 250ms so that all read/write memory commands work fine
            CalculatePointers();
            SetupAssemblyHelper();
        }


        IntPtr recalcBitboardsMemLoc;
        private void SetupAssemblyHelper()
        {
            this.asmHelper = new AssemblyHelper(this);
            var recalc_bitboards_func = FindMemoryInGameCode(new byte[]
            {
                0x55, 0x41, 0x57, 0x41, 0x56, 0x41, 0x55, 0x41, 0x54, 0x56, 0x57, 0x53, 0x48, 0x83, 0xEC, 0x78,
                0x48, 0x8D, 0x6C, 0x24, 0x70, 0x49, 0x89, 0xCC, 0xC7, 0x41, 0x40, 0x00, 0x00, 0x00, 0x00, 0xC7,
                0x41, 0x50, 0x00, 0x00, 0x00, 0x00, 0xC7, 0x41, 0x60, 0x00, 0x00, 0x00, 0x00, 0xC7, 0x41, 0x70,
                0x00, 0x00, 0x00, 0x00, 0xC7, 0x81, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC7, 0x81,
                0x90, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC7, 0x81, 0xA0, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0xC7, 0x81, 0xB0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC7, 0x81, 0xC0, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xC7, 0x81, 0xD0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
            }).Keys.Single();

            var recalcBitBoardsThreadSetupCode = new List<byte>
            {
                0x48, 0xB9 // MOV RCX
            };
            recalcBitBoardsThreadSetupCode.AddRange(BitConverter.GetBytes((long)(this.MemLocChessArrayPointer.Location - 0x38)));

            this.recalcBitboardsMemLoc = this.asmHelper.AllocCodeInTargetProcessWithJump(recalcBitBoardsThreadSetupCode.ToArray(), recalc_bitboards_func);
        }


        private Dictionary<IntPtr, byte[]> FindMemoryInGameCode(byte[] bytesToFind) =>
            MemoryUtil.FindMemoryWithWildcards(GetGameHandle(), GetEntryPoint(), (uint)this.GameProcess.MainModule.ModuleMemorySize, bytesToFind);


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
            this.MemLocChessArrayElementCount = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, -8);
            this.MemLocChessArrayCapacity = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, -4);
            this.MemLocChessBoardSizeWidth = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, 0xA8 + 0x4);
            this.MemLocChessBoardSizeHeight = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, 0xA8);
            this.MemLocCurrentPlayersTurn = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, 0x130);
            this.MemLocGameEndedWinner = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, 0xCC);
            this.MemLocGameState = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, 0xD0);
            this.MemLocWhiteTime = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, 0x1A8);
            this.MemLocBlackTime = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, 0x1AC);
            this.MemLocWhiteIncrement = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, 0x1B0);
            this.MemLocBlackIncrement = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, 0x1B4);


            this.MemLocWhiteTimelineCountInternal = new MemoryLocation<uint>(GetGameHandle(), chessboardPointerLocation, -0x30);
            this.MemLocBlackTimelineCountInternalInverted = new MemoryLocation<uint>(GetGameHandle(), chessboardPointerLocation, -0x30 + 4);
            this.MemLocSomeTurnCountOrSomething = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, -0x30 + 0x38);
            this.MemLocProbablyBoardCount = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, -0x28);
            this.MemLocCosmeticTurnOffset = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, -0x20);


        }

        /// <summary>
        /// Creates a new thread in the game's process which calls a function to recalculate bitboards and fix chessboards.
        /// WARNING: THIS IS AN ACITION WHICH CAN POTENTIALLY CRASH THE GAME.
        /// </summary>
        public void RecalculateBitboards()
        {
            KernelMethods.CreateRemoteThread(GetGameHandle(), this.recalcBitboardsMemLoc);
        }

        /// <summary>
        /// Gets all currently available chessboards. Chessboards are ordered by their id, indicating when they were created. (<see cref="ChessBoard.Id"/>).
        /// </summary>
        /// <returns>A list of <see cref="ChessBoard"/>, each representing a single chessboard.</returns>
        public List<ChessBoard> GetChessBoards()
        {
            var len = this.MemLocChessArrayElementCount.GetValue();
            var bytesToRead = (uint)(len * ChessBoardMemory.structSize);
            var boardLoc = this.MemLocChessArrayPointer.GetValue();
            var bytes = KernelMethods.ReadMemory(GetGameHandle(), boardLoc, bytesToRead);

            var chunks = new List<byte[]>();
            for (int i = 0; i < len; i++)
            {
                var dest = new byte[ChessBoardMemory.structSize];
                Array.Copy(bytes, i * ChessBoardMemory.structSize, dest, 0, ChessBoardMemory.structSize);
                chunks.Add(dest);
            }

            var cbms = chunks.Select(x => ChessBoardMemory.ParseFromByteArray(x)).ToList();
            var chessboardSize = GetChessBoardSize();
            var cbs = cbms.Select(x => new ChessBoard(x, chessboardSize.Width, chessboardSize.Height)).ToList();
            return cbs;
        }

        public void ModifyChessBoards(Func<ChessBoard, ChessBoard> lambda)
        {
            var len = this.MemLocChessArrayElementCount.GetValue();
            var bytesToRead = (uint)(len * ChessBoardMemory.structSize);
            var boardLoc = this.MemLocChessArrayPointer.GetValue();
            var bytes = KernelMethods.ReadMemory(GetGameHandle(), boardLoc, bytesToRead);

            var size = GetChessBoardSize();
            for (int i = 0; i < len; i++)
            {
                var dest = new byte[ChessBoardMemory.structSize];
                Array.Copy(bytes, i * ChessBoardMemory.structSize, dest, 0, ChessBoardMemory.structSize);
                var board = ChessBoardMemory.ParseFromByteArray(dest);
                var modifiedBoard = lambda.Invoke(new ChessBoard(board, size.Width, size.Height));
                var newBytes = ChessBoardMemory.ToByteArray(modifiedBoard.cbm);

                if (newBytes.Length != ChessBoardMemory.structSize)
                    throw new Exception("For some reason the modified ChessBoardMemory struct is of different length than the original which is not allowed");

                var updateRequired = false;
                for (int j = 0; j < newBytes.Length; j++)
                {
                    if (newBytes[j] != bytes[i * ChessBoardMemory.structSize + j]) // if any of the bytes changed
                    {
                        updateRequired = true;
                        break;
                    }
                }

                // resolve deltas, to not overwrite potentially modified data -- experimental but seems to work, but doesnt solve the problem where an AI gets stuck
                // if we swap pieces while the ai is thinking
                if (updateRequired)
                {
                    // var byteDelta = newBytes.Select((x, i) => x == bytes[i] ? (byte?)null : x).ToArray();
                    var lengths = new List<(int, int)>(); // tuples of start index, length

                    int currStartIndex = -1;
                    for (int bi = 0; bi < newBytes.Length; bi++)
                    {
                        var byteDiffers = bytes[bi] != newBytes[bi];
                        if (currStartIndex == -1) // not currently in a group
                        {
                            if (byteDiffers)
                            {
                                currStartIndex = bi;
                            }
                        }
                        else
                        {
                            if (!byteDiffers)
                            {
                                lengths.Add((currStartIndex, bi - currStartIndex));
                                currStartIndex = -1;
                            }
                        }
                    }

                    if (currStartIndex != -1)
                    {
                        lengths.Add((currStartIndex, newBytes.Length - currStartIndex));
                    }

                    foreach (var (startindex, length) in lengths)
                        KernelMethods.WriteMemory(GetGameHandle(), boardLoc + i * ChessBoardMemory.structSize + startindex, newBytes.Skip(startindex).Take(length).ToArray());
                }
            }
        }

        /// <summary>
        /// Acquires a lock, and suspends the game process while this lock is held.
        /// Threadsafe.
        /// </summary>
        /// <param name="a">The inner action to be executed, while the process is suspended.</param>
        public void ExecuteWhileGameSuspendedLocked(Action a) => this.suspendGameprocessLock.Lock(a);

        public void SetChessBoardArray(ChessBoard[] newBoards)
        {
            Thread.Sleep(250);
            ExecuteWhileGameSuspendedLocked(() =>
            {
                var firstTurn = newBoards.Min(x => x.cbm.turn * 2); // first board subturn
                var boardCountToAdd = 1 - firstTurn;

                var timelineLengths = newBoards
                    .GroupBy(x => x.cbm.timeline)
                    .Select(group => (timeline: group.Key, boardVirtualSubTurnCount: group.Last().cbm.turn * 2 + (group.Last().cbm.isBlacksMove == 1 ? 1 : 0) + boardCountToAdd, lastBoard: group.Last()))
                    .ToArray();


                //var timelinesByLength = timelineLengths.Select((x, index) =>
                //            (index, boardVirtualSubTurnCount: x.lastBoard.cbm.turn * 2 + (x.lastBoard.cbm.isBlacksMove == 1 ? 1 : 0), timeline: x.timeline))
                //    .OrderByDescending(x => x.boardVirtualSubTurnCount).ToList();


                var lastTurn = timelineLengths.Max(x => x.lastBoard.cbm.turn * 2 + (x.lastBoard.cbm.isBlacksMove == 1 ? 1 : 0)); // last board subturn
                var maxTimeLineLen = lastTurn - firstTurn + 1;

                var timelinesByTimeline = timelineLengths.OrderByDescending(x => x.timeline).ToArray(); // go from white to black (+L -> -L)
                bool replaceValues = false;

                for (int i = 0; i < timelinesByTimeline.Length; i++)
                {
                    if (replaceValues)
                        timelinesByTimeline[i].boardVirtualSubTurnCount = maxTimeLineLen;
                    else if (timelinesByTimeline[i].boardVirtualSubTurnCount == maxTimeLineLen)
                        replaceValues = true;
                    else
                        timelinesByTimeline[i].boardVirtualSubTurnCount = maxTimeLineLen - 1;

                }


                var memlocsometurncountorsomethingNewValue = timelinesByTimeline.Sum(x => x.boardVirtualSubTurnCount);

                var maxCapacity = this.MemLocChessArrayCapacity.GetValue();
                if (newBoards.Length > maxCapacity)
                    asmHelper.EnsureArrayCapacity<ChessBoardMemory>(MemLocChessArrayPointer, newBoards.Length);



                var bytes = newBoards.SelectMany(x => ChessBoardMemory.ToByteArray(x.cbm)).ToArray();
                KernelMethods.WriteMemory(GetGameHandle(), this.MemLocChessArrayPointer.GetValue(), bytes);
                this.MemLocWhiteTimelineCountInternal.SetValue((uint)(newBoards.Max(x => x.cbm.timeline) + 1));
                this.MemLocSomeTurnCountOrSomething.SetValue(memlocsometurncountorsomethingNewValue);
                this.MemLocBlackTimelineCountInternalInverted.SetValue((uint)0xFFFF_FFFF - (uint)(-newBoards.Min(x => x.cbm.timeline)));
                this.MemLocProbablyBoardCount.SetValue(newBoards.Length);
                //Thread.Sleep(5000);
                this.MemLocChessArrayElementCount.SetValue(newBoards.Length);
            });
        }

        public int GetChessBoardAmount() => this.MemLocChessArrayElementCount.GetValue();

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

        public bool IsGameRunning() => this.MemLocChessArrayPointer.GetValue() != IntPtr.Zero;

        public GameState GetCurrentGameState()
        {
            if (!IsGameRunning())
            {
                return GameState.NotStarted;
            }
            else
            {
                var whoWon = this.MemLocGameEndedWinner.GetValue();

                if (whoWon == -1)
                {
                    return GameState.Running;
                }
                else
                {
                    if (whoWon == 0)
                    {
                        return GameState.EndedWhiteWon;
                    }
                    else
                    {
                        var gs = this.MemLocGameState.GetValue();

                        if (gs == 2)
                        {
                            return GameState.EndedDraw;
                        }
                        else if (gs == 1) // someone won, which can only be black
                        {
                            return GameState.EndedBlackWon;
                        }
                        else
                        {
                            throw new UnexpectedChessDataException();
                        }
                    }
                }
            }
        }
    }
}
