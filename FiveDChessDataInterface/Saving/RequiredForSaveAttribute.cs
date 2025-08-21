using System;

namespace FiveDChessDataInterface.Saving {
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    internal sealed class RequiredForSaveAttribute : Attribute {
        public RequiredForSaveAttribute(string name) {
            Name = name;
        }

        public string Name { get; }
    }
}
