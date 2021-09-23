using System;
using System.Collections.Generic;
using System.Linq;

namespace FiveDChessDataInterface.MemoryHelpers
{
    public class AssemblySnippetBuilder
    {
        private readonly List<byte> bytes;
        private readonly IntPtr snippetLocation;

        public AssemblySnippetBuilder(IntPtr snippetLocation)
        {
            this.bytes = new List<byte>();
            this.snippetLocation = snippetLocation;
        }

        public AssemblySnippetBuilder WithCustomBytes(IEnumerable<byte> b)
        {
            this.bytes.AddRange(b);
            return this;
        }
        public AssemblySnippetBuilder WithNop(int length) => WithCustomBytes(Enumerable.Repeat((byte)0x90, length));

        public AssemblySnippetBuilder WithCall(IntPtr targetAddress)
        {
            var b = new byte[] {
                0xe8,
                0,0,0,0
            };
            var nextInstructionAddress = this.snippetLocation.ToInt64() - (this.bytes.Count + b.Length);
            var callArglong = (targetAddress.ToInt64() - nextInstructionAddress); // offset is 4 byte (32 bit)

            if (callArglong > int.MaxValue || callArglong < int.MinValue)
                throw new ArgumentOutOfRangeException("target address is too far away from the source addres. Use an absolute jump instead.");

            var callArg = (uint)callArglong;
            var callArgAsBytes = BitConverter.GetBytes(callArg)/*.Reverse()*/.ToArray();
            Array.ConstrainedCopy(callArgAsBytes, 0, b, 1, callArgAsBytes.Length);

            this.bytes.AddRange(b);
            return this;
        }

        /// <summary>
        /// WARNING: REGISTER RAX WILL BE MODIFIED! 
        /// </summary>
        public AssemblySnippetBuilder WithCall64VIARAX(IntPtr targetAddress)
        {
            var b = new byte[]
            {
                0x48, 0xB8,
                0, 0, 0, 0, 0, 0, 0, 0,
                0xFF, 0xD0
            };

            var callArgAsBytes = BitConverter.GetBytes(targetAddress.ToInt64())/*.Reverse()*/.ToArray();
            Array.ConstrainedCopy(callArgAsBytes, 0, b, 2, callArgAsBytes.Length);

            this.bytes.AddRange(b);
            return this;
        }


        public AssemblySnippetBuilder WithCustomByte(byte b)
        {
            this.bytes.Add(b);
            return this;
        }

        public AssemblySnippetBuilder WithRet() => WithCustomByte(0xc3);
        public AssemblySnippetBuilder WithPushRAX() => WithCustomByte(0x50);
        public AssemblySnippetBuilder WithPushRBX() => WithCustomByte(0x53);
        public AssemblySnippetBuilder WithPushRCX() => WithCustomByte(0x51);
        public AssemblySnippetBuilder WithPushRDX() => WithCustomByte(0x52);
        public AssemblySnippetBuilder WithPopRAX() => WithCustomByte(0x58);
        public AssemblySnippetBuilder WithPopRBX() => WithCustomByte(0x5b);
        public AssemblySnippetBuilder WithPopRCX() => WithCustomByte(0x59);
        public AssemblySnippetBuilder WithPopRDX() => WithCustomByte(0x5a);

        /// <summary>
        /// WARNING: REGISTER RAX WILL BE MODIFIED! 
        /// LENGTH = 12
        /// </summary>
        public AssemblySnippetBuilder WithAbsoluteRAXJumpTo(IntPtr targetAddressAbsolute)
        {
            var b = new byte[]
{
                0x48, 0xB8, // MOVABS RAX,
                0, 0, 0, 0, 0, 0, 0, 0, // 8 byte (jump dest)

                0x50,  // PUSH RAX
                0xC3 // RET            
};
            var targetBytes = BitConverter.GetBytes((long)targetAddressAbsolute);
            Array.Copy(targetBytes, 0, b, 2, targetBytes.Length);

            this.bytes.AddRange(b);

            return this;
        }

        public byte[] GetByteArray() => this.bytes.ToArray();
    }
}
