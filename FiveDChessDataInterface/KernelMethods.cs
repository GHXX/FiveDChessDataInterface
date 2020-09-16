using System;
using System.Runtime.InteropServices;

namespace FiveDChessDataInterface
{
    class KernelMethods
    {
        [DllImport("Kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, UInt32 nSize, ref UInt32 lpNumberOfBytesRead);

        public static byte[] ReadMemory(IntPtr handle, IntPtr address, UInt32 size, out UInt32 bytesRead)
        {
            byte[] buffer = new byte[size];
            bytesRead = 0;
            ReadProcessMemory(handle, address, buffer, size, ref bytesRead);
            return buffer;
        }
    }
}
