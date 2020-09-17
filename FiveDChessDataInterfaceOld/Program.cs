using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace FiveDChessDataInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "5D Chess Data Interface";

            Process gameProcessHandle;

            const string executableName = "5dchesswithmultiversetimetravel";
            while (true)
            {
                Console.WriteLine("Finding 5d chess process...");
                var filteredProcesses = Process.GetProcessesByName(executableName);
                if (filteredProcesses.Length == 1)
                {
                    gameProcessHandle = filteredProcesses[0];
                    break;
                }

                if (filteredProcesses.Length == 0)
                    Console.WriteLine("5dChess Process not detected. Please start the game.");
                else
                    Console.WriteLine("Multiple 5dChess Processes detected. Please exit all additional game instances.");
                Thread.Sleep(2000);
            }

            IntPtr entryPoint = gameProcessHandle.MainModule.BaseAddress;
            Console.WriteLine($"Entrypoint: 0x{entryPoint.ToString("X16")}");

            var bytesToFind = new byte[] { 0x4c, 0x8b, 0x35,
                0x90, 0x90, 0x90, 0x90,// = 0x55, 0xfa, 0x0c, 0x00 WILDCARDS
                0x4c, 0x69, 0xf8,
                0x90, 0x90, 0x90, 0x90,
                0x4c, 0x89, 0xf0,
                0x4c, 0x01, 0xf8
            };

            var results =
                MemoryUtil.FindMemoryWithWildcards(gameProcessHandle.Handle, entryPoint, (uint)gameProcessHandle.MainModule.ModuleMemorySize, bytesToFind);


            if (results.Count != 1)
            {
                throw new AmbiguousMatchException($"{results.Count} memory locations matched, which is not 1!");
            }


            Console.WriteLine($"Found chessboard pointer information. Calculating pointer...");

            var result = results.First();
            var resultAddress = result.Key;
            var resultBytes = result.Value;

            var chessboardPointerLocation = IntPtr.Add(resultAddress, BitConverter.ToInt32(resultBytes, 3) + 7);
            var chessboardSizeLocation = IntPtr.Subtract(chessboardPointerLocation, 8);

            Console.WriteLine($"The pointer to the chessboards is located at: 0x{chessboardPointerLocation.ToString("X16")}");
            Console.WriteLine($"The chessboard array size is located at: 0x{chessboardSizeLocation.ToString("X16")}");
            Console.WriteLine($"The chessboard sizes width and height are located at 0x{(chessboardPointerLocation + 0x98).ToString("X16")} and 0x{(chessboardPointerLocation + 0x98 + 0x4).ToString("X16")}");

            var currTurnLocation = IntPtr.Add(chessboardPointerLocation, 256);
            Console.WriteLine($"The current turn is stored at: 0x{currTurnLocation.ToString("X16")}");
            Console.WriteLine($"Currently it's {(MemoryUtil.ReadValue<int>(gameProcessHandle.Handle, currTurnLocation) == 0 ? "WHITE's" : "BLACK's")} turn!");


            var chessboardLocation = MemoryUtil.ReadValue<IntPtr>(gameProcessHandle.Handle, chessboardPointerLocation);

            Console.WriteLine($"The chessboards are currently located at: 0x{chessboardLocation.ToString("X16")}");



            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
    }
}
