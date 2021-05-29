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


        [DllImport("kernel32.dll")]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpBaseAddress, int dwSize, int flAllocationType, int flProtect);
        // lpBaseAddress = NULL -> automatically determine location

        public static IntPtr AllocProcessMemory(IntPtr handle, int size)
        {
            return VirtualAllocEx(handle, IntPtr.Zero, size, 0x00001000 | 0x00002000 /* MEM_COMMIT | MEM_RESERVE */, 0x40 /* PAGE_EXEC_READWRITE */ );
        }


        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, int dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, int dwCreationFlags, IntPtr lpThreadId);
        // lpThreadAttribs == 0 -> use default value
        // dwStackSize == 0 -> use default value
        // dwCreationFlags: 0 -> runs immediately; 4-> starts suspended
        // lpThreadId == NULL: then no thread identifier is returned

        public static IntPtr CreateRemoteThread(IntPtr handle, IntPtr startAddress, int stackSize = 0, bool startSuspended = false)
        {
            return CreateRemoteThread(handle, IntPtr.Zero, stackSize, startAddress, IntPtr.Zero, startSuspended ? 4 : 0, IntPtr.Zero);
        }
    }
}
