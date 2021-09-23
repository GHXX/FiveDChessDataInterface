using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace FiveDChessDataInterface.MemoryHelpers
{
    public class KernelMethods
    {
        [DllImport("Kernel32.dll")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, ref uint lpNumberOfBytesRead);

        public static byte[] ReadMemory(IntPtr handle, IntPtr address, uint size)
        {
            byte[] buffer = new byte[size];
            uint bytesRead = 0;
            ReadProcessMemory(handle, address, buffer, size, ref bytesRead);

            // validate read action
            if (bytesRead != size)
                throw new Exception($"Read operation from address 0x{address.ToString("X8")} was partial! Expected number of bytes read: {size}; Actual amount: {bytesRead}");

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

        public static IntPtr AllocProcessMemory(IntPtr handle, int size, bool executable)
        {
            return VirtualAllocEx(handle, IntPtr.Zero, size, 0x00001000 | 0x00002000 /* MEM_COMMIT | MEM_RESERVE */, executable ? 0x40 /* PAGE_EXEC_READWRITE */ : 0x04 /* PAGE_READWRITE */);
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr VirtualFreeEx(IntPtr hProcess, IntPtr lpBaseAddress, int dwSize, int deFreeType);

        public static IntPtr FreeProcessMemory(IntPtr handle, IntPtr baseAddress, int size)
        {
            return VirtualFreeEx(handle, baseAddress, size, 0x00008000 /* MEM_RELEASE */);
        }


        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string functionName);

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

        /// <summary>
        /// DO NOT USE DIRECTLY, TO LOCK THE GAME PROCESS. USE <see cref="Util.SuspendGameProcessLock.Lock(Action)"/> INSTEAD!
        /// </summary>
        [DllImport("ntdll.dll", PreserveSig = false)]
        internal static extern void NtSuspendProcess(IntPtr processHandle);

        /// <summary>
        /// DO NOT USE DIRECTLY, TO LOCK THE GAME PROCESS. USE <see cref="Util.SuspendGameProcessLock.Lock(Action)"/> INSTEAD!
        /// </summary>

        [DllImport("ntdll.dll", PreserveSig = false, SetLastError = true)]
        internal static extern void NtResumeProcess(IntPtr processHandle);

        [DllImport("kernel32.dll", PreserveSig = false, SetLastError = true)]
        private static extern int VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, int flNewProtect, ref int lpFlOldProtect);

        /// <summary>
        /// Changes the page protection to the specified value and returns the old one
        /// </summary>
        /// <param name="handle">the game handle to use</param>
        /// <param name="baseAddress">the base address of the range to change the proection of</param>
        /// <param name="size">the size of the range to change the proection of</param>
        /// <param name="newProtectionValue">a value indicating the new proection status</param>
        /// <returns>the old protection status</returns>
        public static FlPageProtect ChangePageProtection(IntPtr handle, IntPtr baseAddress, int size, FlPageProtect newProtectionValue)
        {
            int oldProtect = 0;
            VirtualProtectEx(handle, baseAddress, size, (int)newProtectionValue, ref oldProtect); // return value is always zero???
            var error_code = Marshal.GetLastWin32Error();
            if (error_code != 0)
                throw new InvalidOperationException($"ChangePageProtection failed with result code: {error_code}");

            return (FlPageProtect)oldProtect;
        }

        [Flags]
        public enum FlPageProtect : int
        {
            PAGE_NOACCESS = 0x1,
            PAGE_READONLY = 0x2,
            PAGE_READWRITE = 0x4,
            PAGE_WRITECOPY = 0x8,
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr LoadLibrary(string libname);

        public static IntPtr LoadLibraryRemote(Process remoteProc, string libname)
        {
            var hModule = remoteProc.Handle;
            var libPathBytes = Encoding.ASCII.GetBytes(libname).Append((byte)'\0').ToArray();

            // write libpath to memory
            var nameMem = AllocProcessMemory(hModule, libPathBytes.Length, false);
            WriteMemory(hModule, nameMem, libPathBytes);

            var k32 = GetModuleHandle("kernel32.dll");
            var loadLibPtr = GetProcAddress(k32, "LoadLibraryA");
            if (loadLibPtr == IntPtr.Zero)
                throw new Exception($"Function failed to run as the loadlib ptr is zero! LastWin32Error: {Marshal.GetLastWin32Error()}");


            var allocatedMem = AllocProcessMemory(remoteProc.Handle, 1024, true);
            var snippet = new AssemblySnippetBuilder(IntPtr.Zero)
                .WithCustomBytes(Enumerable.Repeat((byte)0, 8)) // returnvalue
                .WithCustomBytes(new byte[] { 0x48, 0xBB }) // MOV RBX, 
                    .WithCustomBytes(BitConverter.GetBytes(allocatedMem.ToInt64()))

                .WithPushRCX()
                .WithPushRCX()
                .WithPushRCX()
                .WithCall64VIARAX(loadLibPtr)
                .WithPopRCX()
                .WithPopRCX()
                .WithPopRCX()
                .WithCustomBytes(new byte[] { 0x48, 0x89, 0x03 }) // mov [rbx], rax
                .WithRet()
                .GetByteArray();
            WriteMemory(remoteProc.Handle, allocatedMem, snippet);

            var hThread = CreateRemoteThread(hModule, IntPtr.Zero, 0, allocatedMem + 8, nameMem, 0, IntPtr.Zero);

            IntPtr hLib = IntPtr.Zero;
            var expectedName = libname.Split(Path.DirectorySeparatorChar).Last();
            if (!SpinWait.SpinUntil(() =>
            {
                hLib = MemoryUtil.ReadValue<IntPtr>(remoteProc.Handle, allocatedMem);
                return hLib != IntPtr.Zero;
            },
                TimeSpan.FromSeconds(10))
            )
            {
                throw new Exception($"Libloading for dll with expected name '{expectedName}' timed out!");
            }

            return hLib;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

    }
}
