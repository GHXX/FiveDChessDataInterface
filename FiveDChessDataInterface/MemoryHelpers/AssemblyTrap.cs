using System;

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

        public void PlaceTrap()
        {
            this.ah.di.ExecuteWhileGameSuspendedLocked(() =>
            {
                // the code which traps the thread
                var trapBytes = new byte[]
                {
                    0xE9, 0xFB, 0xFF, 0xFF, 0xFF  // jmp -5
                };

                this.originalCode = KernelMethods.ReadMemory(this.ah.gameHandle, this.originalLocation, (uint)trapBytes.Length); // backup old code
                KernelMethods.WriteMemory(this.ah.gameHandle, this.originalLocation, trapBytes); // write hook
            });
        }

        public void ReleaseTrap()
        {
            this.ah.di.ExecuteWhileGameSuspendedLocked(() =>
                    KernelMethods.WriteMemory(this.ah.gameHandle, this.originalLocation, this.originalCode)
                ); // restore original code
        }

        public static AssemblyTrap TrapLocation(IntPtr ptr, AssemblyHelper ah)
        {
            var at = new AssemblyTrap(ptr, ah);
            at.PlaceTrap();
            return at;
        }
    }
}
