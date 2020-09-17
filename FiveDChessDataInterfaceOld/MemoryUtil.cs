using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace FiveDChessDataInterface
{
    static class MemoryUtil
    {
        private static Dictionary<IntPtr, byte[]> FindMemoryInternal(IntPtr gameHandle, IntPtr start, uint length, byte[] bytesToFind, bool allowNop90Wildcards)
        {
            var foundElements = new Dictionary<IntPtr, byte[]>();

            var bytes = KernelMethods.ReadMemory(gameHandle, start, length, out uint _);

            int index2 = 0;
            for (int i = 0; i < bytes.Length; i++)
            {
                var currByte = bytes[i];
                if (bytesToFind[index2] == currByte || (allowNop90Wildcards && bytesToFind[index2] == 0x90))
                {
                    index2++;
                }
                else
                {
                    index2 = 0;
                    continue;
                }

                if (index2 >= bytesToFind.Length)
                {
                    var inArrOffset = i - index2 + 1;
                    foundElements.Add(IntPtr.Add(start, inArrOffset), bytes.Skip(inArrOffset).Take(bytesToFind.Length).ToArray());

                    index2 = 0;
                }
            }

            return foundElements;
        }

        internal static List<IntPtr> FindMemory(IntPtr gameHandle, IntPtr start, uint length, byte[] bytesToFind)
            => FindMemoryInternal(gameHandle, start, length, bytesToFind, false).Keys.ToList();

        internal static Dictionary<IntPtr, byte[]> FindMemoryWithWildcards(IntPtr gameHandle, IntPtr start, uint length, byte[] bytesToFind)
            => FindMemoryInternal(gameHandle, start, length, bytesToFind, true);


        internal static T ReadValue<T>(IntPtr gameHandle, IntPtr location)
        {
            var bytes = KernelMethods.ReadMemory(gameHandle, location, (uint)Marshal.SizeOf<T>(), out _);
            var t = typeof(T);

            switch (Type.GetTypeCode(t))
            {
                case TypeCode.Int16:
                    return (dynamic)BitConverter.ToInt16(bytes, 0);
                case TypeCode.Int32:
                    return (dynamic)BitConverter.ToInt32(bytes, 0);
                case TypeCode.Int64:
                    return (dynamic)BitConverter.ToInt64(bytes, 0);
                case TypeCode.UInt16:
                    return (dynamic)BitConverter.ToUInt16(bytes, 0);
                case TypeCode.UInt32:
                    return (dynamic)BitConverter.ToUInt32(bytes, 0);
                case TypeCode.UInt64:
                    return (dynamic)BitConverter.ToUInt64(bytes, 0);

                case TypeCode.Object:
                    switch (t.FullName)
                    {
                        case "System.IntPtr":
                            return (dynamic)new IntPtr(BitConverter.ToInt64(bytes, 0));
                        default:
                            throw new NotImplementedException("Invalid obj type");
                    }

                default:
                    throw new NotImplementedException("Invalid type");
            }
        }
    }
}
