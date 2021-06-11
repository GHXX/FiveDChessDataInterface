using System;

namespace FiveDChessDataInterface.Saving
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class RequiredForSaveAttribute : Attribute
    {
        public RequiredForSaveAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}
