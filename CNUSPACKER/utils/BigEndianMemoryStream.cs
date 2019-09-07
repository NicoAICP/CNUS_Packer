using System;
using System.IO;

namespace CNUSPACKER.utils
{
    public class BigEndianMemoryStream : MemoryStream
    {
        public BigEndianMemoryStream(int capacity) : base(capacity)
        {
        }

        public void WriteBigEndian(short value)
        {
            byte[] shortBytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(shortBytes);

            base.Write(shortBytes, 0, 2);
        }

        public void WriteBigEndian(int value)
        {
            byte[] intBytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);

            base.Write(intBytes, 0, 4);
        }

        public void WriteBigEndian(uint value)
        {
            byte[] uintBytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(uintBytes);

            base.Write(uintBytes, 0, 4);
        }

        public void WriteBigEndian(long value)
        {
            byte[] longBytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(longBytes);

            base.Write(longBytes, 0, 8);
        }
    }
}
