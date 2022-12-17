using System;
using System.Runtime.Serialization;

namespace FiveDChessDataInterface.Exceptions
{
    [Serializable]
    public class VariantLoadException : Exception
    {
        public VariantLoadException() { }
        public VariantLoadException(string message) : base(message) { }
        public VariantLoadException(string message, Exception inner) : base(message, inner) { }
        protected VariantLoadException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
