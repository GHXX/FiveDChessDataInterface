using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace FiveDChessDataInterface.MemoryHelpers
{
    class AssemblyHelper
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
        internal void EnsureArrayCapacity(MemoryLocation<IntPtr> existingArrayPointerLocation, int arrayElementSize, int minCapacity)
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

                // otherwise allocate a new array
                var newArrayPtr = KernelMethods.AllocProcessMemory(this.gameHandle, newCapacity * arrayElementSize, false);

                // copy contents
                existingArrayPointerLocation.SetValue(newArrayPtr);
                KernelMethods.WriteMemory(this.gameHandle, newArrayPtr, oldContents);

                memLocCapacity.SetValue(newCapacity);
            });
        }
        internal void EnsureArrayCapacity<ArrayElemType>(MemoryLocation<IntPtr> existingArrayPointerLocation, int minCapacity)
            => EnsureArrayCapacity(existingArrayPointerLocation, Marshal.SizeOf(typeof(ArrayElemType)), minCapacity);
    }
}
