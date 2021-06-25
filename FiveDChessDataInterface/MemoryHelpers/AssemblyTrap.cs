using System;

namespace FiveDChessDataInterface.MemoryHelpers
{
    public class AssemblyTrap
    {
        IntPtr originalLocation;
        byte[] originalCode;
        private readonly AssemblyHelper ah;
        IntPtr detourLocation;

        public AssemblyTrap(IntPtr originalLocation, AssemblyHelper ah)
        {
            this.originalLocation = originalLocation;
            this.ah = ah;
        }

        public void PlaceTrap()
        {
            this.ah.di.ExecuteWhileGameSuspendedLocked(() =>
            {
                //detourLocation = KernelMethods.AllocProcessMemory(ah.gameHandle, 1024, true);

                //// write the code which actually traps the thread
                //var trapBytes = new byte[]
                //{

                //};


                //// these bytes replace the original code
                //var hookBytes = new byte[]
                //{
                //    0x50, // push rax
                //    0x48, 0xb8 // movabs rax,
                //}
                //.Concat(BitConverter.GetBytes(detourLocation.ToInt64())) // DETOURLOCATION
                //.Concat(new byte[] { 0xff, 0xd0 }).ToArray(); // call rax

                //originalCode = KernelMethods.ReadMemory(ah.gameHandle, originalLocation, (uint)hookBytes.Length); // backup old code
                //KernelMethods.WriteMemory(ah.gameHandle, originalLocation, hookBytes); // write hook


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
