using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace FiveDChessDataInterface.MemoryHelpers
{
    public class AssemblyHelper
    {
        private readonly IntPtr gameHandle;
        private readonly DataInterface di;

        public AssemblyHelper(DataInterface di)
        {
            this.gameHandle = di.GetGameHandle();
            this.di = di;
        }

        /// <summary>
        /// Allocates a RWExec Memory region and fills it with the specified code
        /// </summary>
        /// <param name="codeToLoad"></param>
        /// <returns></returns>
        public IntPtr AllocMemAndInjectCode(byte[] codeToLoad)
        {
            var ptr = KernelMethods.AllocProcessMemory(this.gameHandle, codeToLoad.Length, true);
            KernelMethods.WriteMemory(this.gameHandle, ptr, codeToLoad);
            return ptr;
        }

        public IntPtr AllocCodeInTargetProcessWithJump(byte[] codeToLoad, IntPtr addressToJumpTo)
        {
            var bytes = codeToLoad.ToList().Concat(GetAbsoluteJumpCode(addressToJumpTo)).ToArray();
            return AllocMemAndInjectCode(bytes);
        }

        private byte[] GetAbsoluteJumpCode(IntPtr targetLocation)
        {
            var bytes = new byte[]
            {
                0x48, 0xB8, // MOVABS RAX,
                0, 0, 0, 0, 0, 0, 0, 0, // 8 byte (jump dest)

                0x50,  // PUSH RAX
                0xC3 // RET            
            };
            var targetBytes = BitConverter.GetBytes((long)targetLocation);
            Array.Copy(targetBytes, 0, bytes, 2, targetBytes.Length);
            return bytes;
        }

        // TODO maybe free the old memory, if the game allows it
        public void EnsureArrayCapacity(MemoryLocation<IntPtr> existingArrayPointerLocation, int arrayElementSize, int minCapacity)
        {
            this.di.ExecuteWhileGameSuspendedLocked(() =>
            {
                var memLocElementCnt = new MemoryLocation<int>(this.gameHandle, existingArrayPointerLocation.Location - 0x8);
                var memLocCapacity = new MemoryLocation<int>(this.gameHandle, existingArrayPointerLocation.Location - 0x4);

                if (memLocCapacity.GetValue() >= minCapacity) // skip if the required capacity is already available
                    return;

                // read old contents
                var oldContents = KernelMethods.ReadMemory(this.gameHandle, existingArrayPointerLocation.GetValue(), (uint)(memLocCapacity.GetValue() * arrayElementSize));


                var newCapacity = minCapacity + 16;

                var newArraySize = newCapacity * arrayElementSize;

                if (newArraySize < oldContents.Length)
                    throw new Exception("Heap array size miscalculation!");


                // otherwise allocate a new array
                var newArrayPtr = AllocHeapMem(newArraySize);

                // copy contents
                existingArrayPointerLocation.SetValue(newArrayPtr);
                KernelMethods.WriteMemory(this.gameHandle, newArrayPtr, oldContents);

                memLocCapacity.SetValue(newCapacity);
            });
        }

        public void EnsureArrayCapacity<ArrayElemType>(MemoryLocation<IntPtr> existingArrayPointerLocation, int minCapacity)
            => EnsureArrayCapacity(existingArrayPointerLocation, Marshal.SizeOf(typeof(ArrayElemType)), minCapacity);

        // TODO implement memory freeing if memleak is a problem
        public IntPtr AllocHeapMem(int size)
        {
            ProcessModule k32Module = null;
            var modules = this.di.GameProcess.Modules;
            for (int i = 0; i < modules.Count; i++)
            {
                var m = modules[i];
                if (m.ModuleName == "KERNEL32.DLL")
                {
                    k32Module = m;
                    break;
                }
            }

            if (k32Module == null)
                throw new DllNotFoundException("Could not resolve KERNEL32.DLL in the game process!");

            var getProcessHeapPtr = KernelMethods.GetProcAddress(k32Module.BaseAddress, "GetProcessHeap");
            var heapAllocPtr = KernelMethods.GetProcAddress(k32Module.BaseAddress, "HeapAlloc");

            /*
            // result location
            mov rbx, RESULT_LOC
            push rbx
            // call GetProcessHeap
            mov rax, 0x00007ff8e8035bb0
            call rax
            // rax = heaphandle

            // heaphandle
            mov rcx, rax
            // heapalloc-flags
            mov rdx, 0
            // heapalloc-size
            mov r8, ALLC_SIZE

            // push junk
            push 0
            // call HeapAlloc
            mov rax, 0x00007ff8e8f9a9a0
            call rax
            // pop some junk
            pop rbx
            // pop the original rbx
            pop rbx
            mov [rbx], rax
            ret
            */

            var memPtr = KernelMethods.AllocProcessMemory(this.gameHandle, 8, false);
            var memLocHeapAllocResult = new MemoryLocation<IntPtr>(this.gameHandle, memPtr);
            memLocHeapAllocResult.SetValue(new IntPtr(-1L));

            var code = new byte[] {
                0x48, 0xBB, // mov rbx,
                }
            .Concat(BitConverter.GetBytes(memPtr.ToInt64())) // TARGET_ADDRESS 8byte
            .Concat(
            new byte[]{
                0x53, // push   rbx
                0x48, 0xB8, // mov rax
                })
            .Concat(BitConverter.GetBytes(getProcessHeapPtr.ToInt64())) // GetProcessHeap() 8byte
            .Concat(
            new byte[]{
                0xFF, 0xD0, // call rax
                0x48, 0x89, 0xC1, // mov rcx, rax -- heapHandle
                0x48, 0xC7, 0xC2, 0x00, 0x00, 0x00, 0x00, // mov rdx, 0, -- flags
                0x49, 0xC7, 0xC0,  // mov r8
                })
            .Concat(BitConverter.GetBytes((int)size)) // allocation size, 4byte
            .Concat(
            new byte[]{
                0x6A, 0x00, // push 0
                0x48, 0xB8, // mov rax,
                })
            .Concat(BitConverter.GetBytes(heapAllocPtr.ToInt64())) // HeapAlloc() 8byte
            .Concat(
            new byte[]{
                0xFF, 0xD0, // call rax
                0x5B, // pop rbx -- junk
                0x5B, // pop rbx -- allocated heap address
                0x48, 0x89, 0x03, // mov [rbx], rax -- mov allocated heap address to TARGET_ADDRESS
                0xC3 // ret
            }).ToArray();

            var ptr = AllocMemAndInjectCode(code);
            KernelMethods.CreateRemoteThread(this.gameHandle, ptr);
            IntPtr result = IntPtr.Zero;
            SpinWait.SpinUntil(() =>
            {
                result = memLocHeapAllocResult.GetValue();
                return result.ToInt64() != -1;
            });
            if (result == IntPtr.Zero)
                throw new Exception("Heap memory allocation failed!");

            KernelMethods.FreeProcessMemory(this.gameHandle, memLocHeapAllocResult.Location, 8);

            return result;
        }
    }
}
