using System;
using System.Linq;

namespace FiveDChessDataInterface.MemoryHelpers
{
    public class AssemblyTrap
    {
        IntPtr originalLocation;
        byte[] originalCode;
        private readonly AssemblyHelper ah;

        public AssemblyTrap(IntPtr originalLocation, AssemblyHelper ah)
        {
            this.originalLocation = originalLocation;
            this.ah = ah;
        }

        private void PlaceTrap(bool throwOnExistingLock)
        {
            this.ah.di.ExecuteWhileGameSuspendedLocked(() =>
            {
                // the code which traps the thread
                var trapBytes = new byte[]
                {
                    0xE9, 0xFB, 0xFF, 0xFF, 0xFF  // jmp -5
                };

                this.originalCode = KernelMethods.ReadMemory(this.ah.gameHandle, this.originalLocation, (uint)trapBytes.Length); // backup old code

                if (!this.originalCode.SequenceEqual(trapBytes)) // if the lock doesnt exist
                {
                    KernelMethods.WriteMemory(this.ah.gameHandle, this.originalLocation, trapBytes); // write hook
                }
                else if (throwOnExistingLock) // if it does exist, check if we want to throw
                {
                    throw new InvalidOperationException($"A lock already exists on the location {this.originalLocation}!");
                }
            });
        }

        public void ReleaseTrap()
        {
            this.ah.di.ExecuteWhileGameSuspendedLocked(() =>
                    KernelMethods.WriteMemory(this.ah.gameHandle, this.originalLocation, this.originalCode)
                ); // restore original code
        }

        public static AssemblyTrap TrapLocation(IntPtr ptr, AssemblyHelper ah, bool throwOnExistingLock = false)
        {
            var at = new AssemblyTrap(ptr, ah);
            at.PlaceTrap(throwOnExistingLock);
            return at;
        }
    }
}
