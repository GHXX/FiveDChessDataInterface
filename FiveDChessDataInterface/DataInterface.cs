using FiveDChessDataInterface.Builders;
using FiveDChessDataInterface.Exceptions;
using FiveDChessDataInterface.MemoryHelpers;
using FiveDChessDataInterface.Saving;
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

        [RequiredForSave("BoardWidth")]
        public MemoryLocation<int> MemLocChessBoardSizeWidth { get; private set; }

        [RequiredForSave("BoardHeight")]
        public MemoryLocation<int> MemLocChessBoardSizeHeight { get; private set; }

        [RequiredForSave("CurrentPlayersTurn")]
        public MemoryLocation<int> MemLocCurrentPlayersTurn { get; private set; }
        //public MemoryLocation<int> MemLocInGameEndedScreen { get; private set; } // if 1 then the "you lost" / "you won" screen is shown

        [RequiredForSave("GameEndedWinner")]
        public MemoryLocation<int> MemLocGameEndedWinner { get; private set; } // if 0xFFFF FFFF then the game is still running, 0 is a win for white, or unstarted, 1 a win for black or a draw

        [RequiredForSave("GameState")]
        public MemoryLocation<int> MemLocGameState { get; private set; } // if 0 then the game is running or unstarted, 1 means someone won, 2 is a draw

        /// <summary>
        /// The total time that this match has been running for.
        /// </summary>
        [RequiredForSave("TotalMatchTimeElapsed")]
        public MemoryLocation<float> MemLocTotalMatchTimeElapsed { get; private set; }
        [RequiredForSave("WhiteTime")]
        public MemoryLocation<int> MemLocWhiteTime { get; private set; }

        [RequiredForSave("BlackTime")]
        public MemoryLocation<int> MemLocBlackTime { get; private set; }


        [RequiredForSave("WhiteTimeIncrement")]
        public MemoryLocation<int> MemLocWhiteIncrement { get; private set; }

        [RequiredForSave("BlackTimeIncrement")]
        public MemoryLocation<int> MemLocBlackIncrement { get; private set; }
        public MemoryLocation<int> MemLocTimeTravelAnimationEnabled { get; private set; }

        [RequiredForSave("CosmeticTurnOffset")]
        public MemoryLocation<int> MemLocCosmeticTurnOffset { get; private set; }


        [RequiredForSave("TimelineValueOffset")]
        private MemoryLocation<int> MemLocTimelineValueOffset { get; set; }
        private MemoryLocation<uint> MemLocWhiteTimelineCountInternal { get; set; }
        private MemoryLocation<int> MemLocSomeTurnCountOrSomething { get; set; }
        private MemoryLocation<int> MemLocProbablyBoardCount { get; set; }
        private MemoryLocation<uint> MemLocBlackTimelineCountInternalInverted { get; set; }
        private MemoryLocation<int> MemLocSusProbablyBoardCntAgain { get; set; } // still a candidate, not sure if its actually needed or what it does, but it seems to equal to the boardcount

        // ONLY TESTED FOR ODD NUMBER OF STARTING BOARDS!!
        // TODO TEST ON EVEN NUMBER OF STARTING TIMELINES
        public uint GetNumberOfWhiteTimelines() => this.MemLocWhiteTimelineCountInternal.GetValue() - 1;
        public uint GetNumberOfBlackTimelines() => (uint)0xFFFF_FFFF - this.MemLocBlackTimelineCountInternalInverted.GetValue();


        public int GetWT() => this.MemLocWhiteTime.GetValue() + this.MemLocWhiteIncrement.GetValue();
        public int GetBT() => this.MemLocBlackTime.GetValue() + this.MemLocBlackIncrement.GetValue();
        public int GetCurT() => this.MemLocCurrentPlayersTurn.GetValue() == 0 ? GetWT() : GetBT();


        public IntPtr GetGameHandle() => this.GameProcess.Handle;
        public IntPtr GetEntryPoint() => this.GameProcess.MainModule.BaseAddress;

        public AssemblyHelper asmHelper;
        private readonly SuspendGameProcessLock suspendGameprocessLock;

        /// <summary>
        /// Returns whether this <see cref="DataInterface"/> instance is still valid. Possible reasons for this becoming invalid are:
        /// *) Game process exiting
        /// </summary>
        public bool IsValid() => !this.GameProcess.HasExited;

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

            try
            {
                if (filteredProcesses.Length == 1 && !filteredProcesses[0].HasExited)
                {
                    di = new DataInterface(filteredProcesses[0]);
                    return true;
                }
            }
            catch (InvalidOperationException) { } // in case the process is currently exiting

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
            this.MemLocTimeTravelAnimationEnabled = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, 0x3E8);

            // set to -1 for even starting timeline cnt, and to 0 for odd starting timeline cnt
            this.MemLocTimelineValueOffset = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, -0x34);
            this.MemLocWhiteTimelineCountInternal = new MemoryLocation<uint>(GetGameHandle(), chessboardPointerLocation, -0x30);
            this.MemLocBlackTimelineCountInternalInverted = new MemoryLocation<uint>(GetGameHandle(), chessboardPointerLocation, -0x30 + 4);
            this.MemLocSomeTurnCountOrSomething = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, -0x30 + 0x38);
            this.MemLocProbablyBoardCount = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, -0x28);
            this.MemLocCosmeticTurnOffset = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, -0x20);
            this.MemLocSusProbablyBoardCntAgain = new MemoryLocation<int>(GetGameHandle(), chessboardPointerLocation, -0x24);

            this.MemLocTotalMatchTimeElapsed = new MemoryLocation<float>(GetGameHandle(), chessboardPointerLocation, 0x27C); // same value at +4 after this one


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

        /// <summary>
        /// Sets the current match's chessboards to those and recalcualtes bitboards. Does not perform ANY sanity checks. 
        /// If you are unsure, consider using <see cref="SetChessBoardArray"/> instead.
        /// </summary>
        /// <param name="newBoards">A <see cref="ChessBoard[]"/> representing the new Chessboards to write into the memory.</param>
        public void SetChessBoardArrayUnchecked(ChessBoard[] newBoards) => SetChessBoardArrayInternal(newBoards);
        /// <summary>
        /// Sets the current match's chessboards and recalculates bitboards, but performs sanity checks before doing so. 
        /// To override those sanity checks, to produce possibly unstable behaviour, use <see cref="SetChessBoardArrayUnchecked(ChessBoard[])"/> instead.
        /// </summary>
        /// <param name="newBoards"></param>
        public void SetChessBoardArray(ChessBoard[] newBoards)
        {
            void AssertTrue(bool success, string msg)
            {
                if (!success)
                    throw new Exception($"Sanity check failed! Details: {msg}");
            }

            var minId = newBoards.Min(x => x.cbm.boardId);
            AssertTrue(minId == 0, "the smallest boardId should be 0");


            var idCount = newBoards.DistinctBy(x => x.cbm.boardId).Count();
            AssertTrue(idCount == newBoards.Count(), "all boardIds have to be unique");

            AssertTrue(newBoards.Skip(1).All(x => x.height == newBoards[0].height && x.width == newBoards[0].width), "all boards have to be of the same size");


            IEnumerable<int> RangeFromToInclusive(int start, int end)
            {
                return Enumerable.Range(start, end - start + 1);
            }

            var timelines = RangeFromToInclusive(newBoards.Min(x => x.cbm.timeline), newBoards.Max(x => x.cbm.timeline));
            var subTurns = RangeFromToInclusive(newBoards.Min(x => x.cbm.GetSubturnIndex()), newBoards.Max(x => x.cbm.GetSubturnIndex()));

            foreach (var timeline in timelines)
            {
                foreach (var subTurn in subTurns)
                {
                    var boardsAtLocation = newBoards.Where(x => x.cbm.timeline == timeline && x.cbm.GetSubturnIndex() == subTurn).ToList();
                    // allow holes (Count = 0), but do not allow two boards to be in the same place
                    AssertTrue(boardsAtLocation.Count <= 1,
                        $"there are {boardsAtLocation.Count} boards in the timeline {timeline} on turn {subTurn / 2} in {(subTurn % 2 == 0 ? "White" : "Black")}'s subturn");
                }
            }

            ExecuteWhileGameSuspendedLocked(() =>
            {
                this.MemLocChessBoardSizeHeight.SetValue(newBoards[0].height);
                this.MemLocChessBoardSizeWidth.SetValue(newBoards[0].width);
                SetChessBoardArrayInternal(newBoards);
            });
        }

        /// <summary>
        /// Same behaviour as <see cref="SetChessBoardArray(ChessBoard[])"/>, but sets additional values.
        /// </summary>
        /// <param name="b"></param>
        public void SetChessBoardArrayFromBuilder(BaseGameBuilder b)
        {
            ExecuteWhileGameSuspendedLocked(() =>
            {
                this.MemLocTimelineValueOffset.SetValue(b.EvenNumberOfStartingTimelines ? -1 : 0);
                // TODO maybe set the turn offset?
                //this.MemLocCosmeticTurnOffset = 
                SetChessBoardArray(b.Build());
            });
        }

        private void SetChessBoardArrayInternal(ChessBoard[] newBoards)
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
                {
                    this.asmHelper.EnsureArrayCapacity<ChessBoardMemory>(this.MemLocChessArrayPointer, newBoards.Length); // 30*
                    // one hanging reference ["5dchesswithmultiversetimetravel.exe"+0014BC20]+0x5F0
                    this.asmHelper.EnsureArrayCapacity(this.MemLocChessArrayPointer.WithOffset(0x10), 4, newBoards.Length); // 40* MemLocSomeTurnCountOrSomething+0x8 // element size is <= 170 bytes (decimal)
                    var sz50_60 = 1;
                    // one hanging reference ["5dchesswithmultiversetimetravel.exe"+0014BAF8]+0x680
                    this.asmHelper.EnsureArrayCapacity(this.MemLocChessArrayPointer.WithOffset(0x20), sz50_60, newBoards.Length); // 50*
                    // two hanging refs ["5dchesswithmultiversetimetravel.exe"+0014BEC0]+0xC90;["5dchesswithmultiversetimetravel.exe"+0014BC30]+F50
                    this.asmHelper.EnsureArrayCapacity(this.MemLocChessArrayPointer.WithOffset(0x30), sz50_60, newBoards.Length); // 60*
                    var szGlobalBitboards = 8;
                    this.asmHelper.EnsureArrayCapacity(this.MemLocChessArrayPointer.WithOffset(0x40), szGlobalBitboards, newBoards.Length); // 70*
                    // one hanging ref ["5dchesswithmultiversetimetravel.exe"+0014BAD8] + 0xBF0
                    this.asmHelper.EnsureArrayCapacity(this.MemLocChessArrayPointer.WithOffset(0x50), szGlobalBitboards, newBoards.Length); // 80*
                    // two hanging refs, but pretty deep in
                    this.asmHelper.EnsureArrayCapacity(this.MemLocChessArrayPointer.WithOffset(0x60), szGlobalBitboards, newBoards.Length); // 90*

                    // one hanging ref ["5dchesswithmultiversetimetravel.exe"+0014BC10]+0x990
                    this.asmHelper.EnsureArrayCapacity(this.MemLocChessArrayPointer.WithOffset(0x70), szGlobalBitboards, newBoards.Length); // a0*
                    this.asmHelper.EnsureArrayCapacity(this.MemLocChessArrayPointer.WithOffset(0x80), szGlobalBitboards, newBoards.Length); // b0*
                    this.asmHelper.EnsureArrayCapacity(this.MemLocChessArrayPointer.WithOffset(0x90), szGlobalBitboards, newBoards.Length); // c0*
                    this.asmHelper.EnsureArrayCapacity(this.MemLocChessArrayPointer.WithOffset(0xa0), szGlobalBitboards, newBoards.Length); // d0*

                    //this.asmHelper.EnsureArrayCapacity(this.MemLocChessArrayPointer.WithOffset(0x338), 4, newBoards.Length); // 368*
                    this.MemLocSusProbablyBoardCntAgain.SetValue(newBoards.Length);
                }

                var bytes = newBoards.SelectMany(x => ChessBoardMemory.ToByteArray(x.cbm)).ToArray();
                KernelMethods.WriteMemory(GetGameHandle(), this.MemLocChessArrayPointer.GetValue(), bytes);
                this.MemLocWhiteTimelineCountInternal.SetValue((uint)(newBoards.Max(x => x.cbm.timeline) + 1));
                this.MemLocSomeTurnCountOrSomething.SetValue(memlocsometurncountorsomethingNewValue);
                this.MemLocBlackTimelineCountInternalInverted.SetValue((uint)0xFFFF_FFFF - (uint)(-newBoards.Min(x => x.cbm.timeline)));
                this.MemLocProbablyBoardCount.SetValue(newBoards.Length);
                //Thread.Sleep(5000);
                this.MemLocChessArrayElementCount.SetValue(newBoards.Length);
            });

            Thread.Sleep(10);
            RecalculateBitboards();
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

        public bool IsMatchRunning()
        {
            try
            {
                return this.MemLocChessArrayPointer.GetValue() != IntPtr.Zero;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public GameState GetCurrentGameState(bool throwOnInvalidState = true)
        {
            GameState ThrowOrUnknown(string errormsg)
            {
                if (throwOnInvalidState)
                    throw new UnexpectedChessDataException("An invalid gamestate was read: " + errormsg);

                return GameState.Unknown;
            }

            if (!IsMatchRunning())
            {
                return GameState.NotStarted;
            }
            var whoWon = this.MemLocGameEndedWinner.GetValue();
            var gs = this.MemLocGameState.GetValue();
            if (gs == 0) // game is running
            {
                if (whoWon != -1)
                    return ThrowOrUnknown("Match is running, but winning player is not -1!"); // Unexpected Data - gs is 0(running) but winning player '{whoWon}' is not -1

                return GameState.Running;
            }
            else if (gs == 1) // game ended with winner
            {
                return whoWon switch
                {
                    0 => GameState.EndedWhiteWon,
                    1 => GameState.EndedBlackWon,
                    _ => ThrowOrUnknown("gs is 1(ended with winner) but winning player 'whoWon' is not 0 or 1")
                };
            }
            else if (gs == 2) // game ended with draw
            {
                return GameState.EndedDraw;
            }
            else if (gs == 3) // game ended with forfeit or the opponent left
            {
                return whoWon switch
                {
                    0 => GameState.EndedWhiteWon,
                    1 => GameState.EndedBlackWon,
                    _ => ThrowOrUnknown("gs is 3(forfeit) but winning player '{whoWon}' is not 0 or 1")
                };
            }
            else if (gs == 5) // game ended because the clock ran out
            {
                return whoWon switch
                {
                    0 => GameState.EndedWhiteWon,
                    1 => GameState.EndedBlackWon,
                    _ => ThrowOrUnknown("gs is 5(clock ran out) but winning player '{whoWon}' is not 0 or 1")
                };
            }
            else
            {
                // Unexpected Data - gs is not 0,1,2,3 or 5
                return ThrowOrUnknown("gs contains an unexpected value");
            }
        }
    }
}
