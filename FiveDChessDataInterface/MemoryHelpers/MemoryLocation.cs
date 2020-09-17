using System;

namespace FiveDChessDataInterface.MemoryHelpers
{
    public class MemoryLocation<T>
    {
        public IntPtr Handle { get; }
        public IntPtr Location { get; }

        public MemoryLocation(IntPtr handle, IntPtr location)
        {
            this.Handle = handle;
            this.Location = location;
        }

        public MemoryLocation(IntPtr handle, IntPtr location, int offset) : this(handle, IntPtr.Add(location, offset)) { }

        public T GetValue()
        {
            return MemoryUtil.ReadValue<T>(this.Handle, this.Location);
        }

        public override string ToString()
        {
            return $"[{this.Location.ToString("X16")}] -> ({typeof(T).FullName}) {GetValue()}";
        }
    }
}
