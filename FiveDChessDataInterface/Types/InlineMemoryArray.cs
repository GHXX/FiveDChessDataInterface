using System;

namespace FiveDChessDataInterface.Types {
    /// <summary>
    /// Used by <see cref="MemoryHelpers.MemoryUtil.WriteValue{T}(IntPtr, IntPtr, T)"/>,
    /// Represents a series of elements of type T, stored sequentially, located DIRECTLY at the <see cref="MemoryHelpers.MemoryLocation{T}"/> position and not representing a by-ref-array.
    /// </summary>
    public class InlineMemoryArray<T> {
        internal readonly T[] backing;

        public InlineMemoryArray(T[] backing) {
            this.backing = backing;
        }
    }
}
