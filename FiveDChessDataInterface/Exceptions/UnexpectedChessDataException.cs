using System;
using System.Runtime.Serialization;

namespace FiveDChessDataInterface.Exceptions {
    [Serializable]
    public class UnexpectedChessDataException : Exception {
        public UnexpectedChessDataException() : this("An unexpected set of data has been read, which likely means that the offsets of this library need to be updated.") { }
        public UnexpectedChessDataException(string message) : base(message) { }
        public UnexpectedChessDataException(string message, Exception inner) : base(message, inner) { }
        protected UnexpectedChessDataException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
