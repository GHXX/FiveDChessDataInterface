using System;
using System.Runtime.InteropServices;

namespace FiveDChessDataInterface.MemoryHelpers {
    public class MemoryLocation<T> {
        public IntPtr Handle { get; }
        public IntPtr Location { get; }

        public MemoryLocation(IntPtr handle, IntPtr location) {
            Handle = handle;
            Location = location;
        }

        public MemoryLocation(IntPtr handle, IntPtr location, int offset) : this(handle, IntPtr.Add(location, offset)) { }

        public T GetValue() {
            return MemoryUtil.ReadValue<T>(Handle, Location);
        }

        /// <summary>
        /// Sets a value in memory. 
        /// WARNING: MAKE SURE YOU ARE CERTAIN THAT YOU ARE WRITING A VALID VALUE.
        /// WRITING INVALID VALUES MAY RESULT IN AN INSTANT CRASH, OR CAUSE UNEXPECTED EVENTS.
        /// </summary>
        /// <param name="newValue"></param>
        public virtual void SetValue(T newValue) {
            MemoryUtil.WriteValue<T>(Handle, Location, newValue);
        }

        public string GetFormattedValue() {
            var val = GetValue();
            return val switch {
                IntPtr ptr => "0x" + ptr.ToString("X8"),
                _ => val.ToString()
            };
        }

        public override string ToString() {
            return $"[{Location.ToString("X16")}] -> ({typeof(T).FullName}) {GetFormattedValue()}";
        }

        public MemoryLocation<T> WithOffset(int offset) {
            return new MemoryLocation<T>(Handle, IntPtr.Add(Location, offset));
        }

        public MemoryLocation<NewT> WithOffset<NewT>(int offset) {
            return new MemoryLocation<NewT>(Handle, IntPtr.Add(Location, offset));
        }

        /// <summary>
        /// Changes page protection that this location is in to RWX. USE SPARINGLY.
        /// </summary>
        public void UnprotectPage() => KernelMethods.ChangePageProtection(Handle, Location, Marshal.SizeOf<T>(), KernelMethods.FlPageProtect.PAGE_EXECUTE_READWRITE);
    }
}
