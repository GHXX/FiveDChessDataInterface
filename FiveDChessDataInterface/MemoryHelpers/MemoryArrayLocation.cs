using System;
using System.Runtime.InteropServices;

namespace FiveDChessDataInterface.MemoryHelpers
{
    class MemoryArrayLocation<ElementType> where ElementType : unmanaged
    {
        private readonly MemoryLocation<IntPtr> memLocBaseArrayPtr;
        private readonly MemoryLocation<int> memLocCapacity;
        private readonly MemoryLocation<int> memLocElementCount;

        private readonly int elementSize;

        public MemoryArrayLocation(MemoryLocation<IntPtr> baseArrayPtr)
        {
            this.memLocBaseArrayPtr = baseArrayPtr;
            this.memLocCapacity = baseArrayPtr.WithOffset<int>(-0x4);
            this.memLocElementCount = baseArrayPtr.WithOffset<int>(-0x8);

            this.elementSize = Marshal.SizeOf(typeof(ElementType));
        }

        public ElementType[] GetArray()
        {
            var cnt = this.memLocElementCount.GetValue();
            var cap = this.memLocCapacity.GetValue();

            if (cnt > cap)
                throw new InvalidOperationException("Array size was larger than the array capacity!");

            var arr = new ElementType[cnt];
            var basePtr = this.memLocBaseArrayPtr.GetValue();
            for (int i = 0; i < cnt; i++)
            {
                arr[i] = (ElementType)Marshal.PtrToStructure(IntPtr.Add(basePtr, i * this.elementSize), typeof(ElementType));
            }

            return arr;
        }

        public ElementType this[int index]
        {
            get
            {
                return GetArray()[index]; // TODO optimize            
            }
            set
            {
                if (index < 0 || index > this.memLocCapacity.GetValue())
                    throw new IndexOutOfRangeException($"Array is too small for the specified index {index}");

                var elementPtr = IntPtr.Add(this.memLocBaseArrayPtr.GetValue(), index * this.elementSize);
                Marshal.StructureToPtr(value, elementPtr, false);
            }
        }

    }
}
