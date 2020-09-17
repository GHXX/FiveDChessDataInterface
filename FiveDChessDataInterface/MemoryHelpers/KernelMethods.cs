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
    }
}
