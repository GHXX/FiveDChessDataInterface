using System;
using System.Threading;

namespace DataInterfaceConsole.Types.Exceptions
{

    [Serializable]
    public class DataInterfaceClosedException : Exception
    {
        public DataInterfaceClosedException() { }
        public DataInterfaceClosedException(string message) : base(message) { }
        public DataInterfaceClosedException(string message, Exception inner) : base(message, inner) { }
        protected DataInterfaceClosedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
