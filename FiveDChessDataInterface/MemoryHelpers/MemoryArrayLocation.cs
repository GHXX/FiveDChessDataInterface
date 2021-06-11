using System;
using System.Collections.Generic;
using System.Text;

namespace FiveDChessDataInterface.MemoryHelpers
{
    class MemoryArrayLocation<ElementType>
    {
        private readonly MemoryLocation<IntPtr> memLocBaseArrayPtr;
        private readonly MemoryLocation<int> memLocCapacity;
        private readonly MemoryLocation<int> memLocElementCount;

        public MemoryArrayLocation(MemoryLocation<IntPtr> baseArrayPtr)
        {
            this.memLocBaseArrayPtr = baseArrayPtr;
            this.memLocCapacity = baseArrayPtr.WithOffset<int>(-0x4);
            this.memLocElementCount = baseArrayPtr.WithOffset<int>(-0x8);
        }

        public ElementType[] GetValues()
        {
            
        }
    }
}
