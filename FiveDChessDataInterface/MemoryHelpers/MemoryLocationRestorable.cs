using System;

namespace FiveDChessDataInterface.MemoryHelpers {
    public class MemoryLocationRestorable<T> : MemoryLocation<T> {
        private bool valueWasEverChanged = false;
        private T originalValue;
        private T lastValue;

        public MemoryLocationRestorable(IntPtr handle, IntPtr location) : base(handle, location) { }

        public MemoryLocationRestorable(IntPtr handle, IntPtr location, int offset) : base(handle, location, offset) { }

        public override void SetValue(T newValue) {
            if (!this.valueWasEverChanged)
                this.lastValue = this.originalValue = GetValue();
            else
                this.lastValue = GetValue();

            this.valueWasEverChanged = true;


            base.SetValue(newValue);
        }

        public void RestoreLast() {
            if (this.valueWasEverChanged)
                base.SetValue(this.lastValue);
        }

        public void RestoreOriginal() {
            if (this.valueWasEverChanged)
                base.SetValue(this.originalValue);
        }

        public new MemoryLocationRestorable<NewT> WithOffset<NewT>(int offset) {
            return new MemoryLocationRestorable<NewT>(Handle, IntPtr.Add(Location, offset));
        }
    }
}
