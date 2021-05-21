using System;
using System.Runtime.InteropServices;

namespace FiveDChessDataInterface.MemoryHelpers
{
    class KernelMethods
    {
        [DllImport("Kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, ref uint lpNumberOfBytesRead);

        public static byte[] ReadMemory(IntPtr handle, IntPtr address, uint size, out uint bytesRead)
        {
            byte[] buffer = new byte[size];
            bytesRead = 0;
            ReadProcessMemory(handle, address, buffer, size, ref bytesRead);
            return buffer;
        }


        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, ref uint lpNumberOfBytesWritten);

        public static void WriteMemory(IntPtr handle, IntPtr address, byte[] newData)
        {
            uint bytesWrittenCount = 0;
            WriteProcessMemory(handle, address, newData, newData.Length, ref bytesWrittenCount);

            // validate write action
            if (bytesWrittenCount != newData.Length)
                throw new Exception($"Write operation to address 0x{address.ToString("X8")} failed! Expected number of bytes written: {newData.Length}; Actual amount: {bytesWrittenCount}");
        }


    }
}
