using System;
using System.Linq;

namespace FiveDChessDataInterface.MemoryHelpers
{
    class AssemblyHelper
    {
        private readonly IntPtr gameHandle;

        public AssemblyHelper(IntPtr gameHandle)
        {
            this.gameHandle = gameHandle;
        }

        /// <summary>
        /// Allocates a RWExec Memory region and fills it with the specified code
        /// </summary>
        /// <param name="codeToLoad"></param>
        /// <returns></returns>
        public IntPtr AllocMemAndInjectCode(byte[] codeToLoad)
        {
            var ptr = KernelMethods.AllocProcessMemory(this.gameHandle, codeToLoad.Length);
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
    }
}
