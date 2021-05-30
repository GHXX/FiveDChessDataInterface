﻿using System;

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

        /// <summary>
        /// Sets a value in memory. 
        /// WARNING: MAKE SURE YOU ARE CERTAIN THAT YOU ARE WRITING A VALID VALUE.
        /// WRITING INVALID VALUES MAY RESULT IN AN INSTANT CRASH, OR CAUSE UNEXPECTED EVENTS.
        /// </summary>
        /// <param name="newValue"></param>
        public void SetValue(T newValue)
        {
            MemoryUtil.WriteValue<T>(this.Handle, this.Location, newValue);
        }

        public string GetFormattedValue()
        {
            var val = GetValue();
            return val switch
            {
                IntPtr ptr => "0x" + ptr.ToString("X8"),
                _ => val.ToString()
            };
        }

        public override string ToString()
        {
            return $"[{this.Location.ToString("X16")}] -> ({typeof(T).FullName}) {GetFormattedValue()}";
        }
    }
}
