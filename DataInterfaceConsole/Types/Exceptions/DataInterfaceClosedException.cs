using System;

namespace DataInterfaceConsole.Types.Exceptions;


[Serializable]
public class DataInterfaceClosedException : Exception {
    public DataInterfaceClosedException() { }
    public DataInterfaceClosedException(string message) : base(message) { }
    public DataInterfaceClosedException(string message, Exception inner) : base(message, inner) { }
}
