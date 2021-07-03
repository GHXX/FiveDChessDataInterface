using System;
using System.Linq;
using System.Threading;

namespace FiveDChessDataInterface.MemoryHelpers
{
    public class AssemblyTrapAdvanced
    {
        IntPtr originalLocation;
        byte[] originalCode;
        private readonly AssemblyHelper ah;

        private MemoryLocation<byte> secondByte = null;
        public bool HasBeenHit => this.secondByte?.GetValue() != 0;

        public AssemblyTrapAdvanced(IntPtr originalLocation, AssemblyHelper ah)
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
                    0xEB, 0x00,  // jmp 00
                    0xC6, 0x05, 0xF8, 0xFF, 0xFF, 0xFF, 0xFE, // mov 0xfe into the second byte of this array
                    0xEB, 0xF5 // jmp to start of this array
                };

                this.originalCode = KernelMethods.ReadMemory(this.ah.gameHandle, this.originalLocation, (uint)trapBytes.Length); // backup old code

                var trapWasThereBefore = true;
                for (int i = 0; i < trapBytes.Length; i++)
                {
                    var old = this.originalCode[i];
                    var expected = trapBytes[i];
                    if (i == 1)
                    {
                        if (new byte[] { 0xFE, trapBytes[i] }.Contains(old)) // if the existing memory at this location is either 0x00 or 0xfe then this might be an old trap
                            continue;
                        else
                        {
                            trapWasThereBefore = false;
                            break;
                        }
                    }
                    else
                    {
                        if (old != expected)
                        {
                            trapWasThereBefore = false;
                            break;
                        }
                    }
                }


                if (!trapWasThereBefore) // if the lock doesnt exist
                {
                    KernelMethods.ChangePageProtection(this.ah.gameHandle, this.originalLocation, trapBytes.Length, KernelMethods.FlPageProtect.PAGE_EXECUTE_READWRITE);

                    KernelMethods.WriteMemory(this.ah.gameHandle, this.originalLocation, trapBytes); // write hook

                    this.secondByte = new MemoryLocation<byte>(this.ah.gameHandle, IntPtr.Add(this.originalLocation, 1));
                }
                else if (throwOnExistingLock) // if it does exist, check if we want to throw
                {
                    throw new InvalidOperationException($"A lock already exists on the location {this.originalLocation}!");
                }
            });
        }

        public void ReleaseTrap()
        {
            if (this.ah.di.IsValid())
                this.ah.di.ExecuteWhileGameSuspendedLocked(() =>
                        KernelMethods.WriteMemory(this.ah.gameHandle, this.originalLocation, this.originalCode)
                    ); // restore original code
        }

        public static AssemblyTrapAdvanced TrapLocation(IntPtr ptr, AssemblyHelper ah, bool throwOnExistingLock = false)
        {
            var at = new AssemblyTrapAdvanced(ptr, ah);
            at.PlaceTrap(throwOnExistingLock);
            return at;
        }

        public void WaitTillHit() => SpinWait.SpinUntil(() => this.HasBeenHit);
    }
}
