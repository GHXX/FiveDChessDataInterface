using FiveDChessDataInterface.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace FiveDChessDataInterface.MemoryHelpers {
    public static class MemoryUtil {
        [Obsolete("use the overload which accepts byte?[] as a paramter instead!")]
        private static Dictionary<IntPtr, byte[]> FindMemoryInternal(IntPtr gameHandle, IntPtr start, uint length, byte[] bytesToFind, bool treatNop90AsWildcard)
            => FindMemoryInternal(gameHandle, start, length, bytesToFind.Select(x => (treatNop90AsWildcard && x == 0x90) ? (byte?)null : x).ToArray());

        private static Dictionary<IntPtr, byte[]> FindMemoryInternal(IntPtr gameHandle, IntPtr start, uint length, byte?[] bytesToFind) {
            var foundElements = new Dictionary<IntPtr, byte[]>();

            var bytes = KernelMethods.ReadMemory(gameHandle, start, length);

            for (int basePos = 0; basePos < bytes.Length - bytesToFind.Length; basePos++) {
                bool found = true;
                for (int i = 0; i < bytesToFind.Length; i++) {
                    var currByte = bytes[basePos + i];
                    if (bytesToFind[i].HasValue && bytesToFind[i].Value != currByte) {
                        found = false;
                        break;
                    }

                }

                if (found) {
                    foundElements.Add(IntPtr.Add(start, basePos), bytes.Skip(basePos).Take(bytesToFind.Length).ToArray());
                }
            }

            return foundElements;
        }

        [Obsolete]
        internal static List<IntPtr> FindMemory(IntPtr gameHandle, IntPtr start, uint length, byte[] bytesToFind)
            => FindMemoryInternal(gameHandle, start, length, bytesToFind, false).Keys.ToList();

        [Obsolete]
        internal static Dictionary<IntPtr, byte[]> FindMemoryWithWildcards(IntPtr gameHandle, IntPtr start, uint length, byte[] bytesToFind)
            => FindMemoryInternal(gameHandle, start, length, bytesToFind, true);

        public static Dictionary<IntPtr, byte[]> FindMemoryWithWildcards(IntPtr gameHandle, IntPtr start, uint length, byte?[] bytesToFind)
            => FindMemoryInternal(gameHandle, start, length, bytesToFind);


        public static T ReadValue<T>(IntPtr gameHandle, IntPtr location) {
            var bytes = KernelMethods.ReadMemory(gameHandle, location, (uint)Marshal.SizeOf<T>());
            var t = typeof(T);

            switch (Type.GetTypeCode(t)) {
                case TypeCode.Byte:
                    return (dynamic)bytes[0];

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
                case TypeCode.Single:
                    return (dynamic)BitConverter.ToSingle(bytes, 0);
                case TypeCode.Double:
                    return (dynamic)BitConverter.ToDouble(bytes, 0);

                case TypeCode.Object:
                    switch (t.FullName) {
                        case "System.IntPtr":
                            return (dynamic)new IntPtr(BitConverter.ToInt64(bytes, 0));
                        default:
                            throw new NotImplementedException("Invalid obj type");
                    }

                default:
                    throw new NotImplementedException("Invalid type");
            }
        }

        public static void WriteValue<T>(IntPtr handle, IntPtr location, T newValue) {
            var t = typeof(T);
            byte[] bytesToWrite = null;
            switch (Type.GetTypeCode(t)) {
                case TypeCode.Byte:
                    bytesToWrite = new byte[1] { (byte)(object)newValue }; ;
                    break;
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                    bytesToWrite = BitConverter.GetBytes((dynamic)newValue);
                    break;
                case TypeCode.Object:
                    switch (newValue) {
                        case IntPtr v:
                            bytesToWrite = BitConverter.GetBytes(v.ToInt64());
                            break;
                        case InlineMemoryArray<int> v:
                            bytesToWrite = v.backing.SelectMany(x => BitConverter.GetBytes(x)).ToArray();                            
                            break;
                        default:
                            throw new NotImplementedException("Invalid obj type");
                    }
                    break;
                default:
                    throw new NotImplementedException("Invalid type");
            }
            KernelMethods.WriteMemory(handle, location, bytesToWrite);
        }

        public static IntPtr LoadFunctionIntoMemory(string assembly, string functionName) {
            var handle = KernelMethods.LoadLibrary(assembly);
            return KernelMethods.GetProcAddress(handle, functionName);
        }
    }
}
