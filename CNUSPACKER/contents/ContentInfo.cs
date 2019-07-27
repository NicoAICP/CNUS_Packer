using System;
using System.IO;

namespace CNUSPACKER.contents
{
    public class ContentInfo
    {
        public const int staticDataSize = 0x24;
        private readonly short indexOffset;
        private readonly short contentCount;
        public byte[] SHA2Hash { get; set; }

        public ContentInfo() : this(0)
        {
        }

        public ContentInfo(short contentCount): this(0, contentCount)
        {
        }

        public ContentInfo(short indexOffset, short contentCount)
        {
            this.indexOffset = indexOffset;
            this.contentCount = contentCount;
        }

        public byte[] GetAsData()
        {
            MemoryStream buffer = new MemoryStream(0x24);
            byte[] temp;

            temp = BitConverter.GetBytes(indexOffset);
            Array.Reverse(temp);
            buffer.Write(temp);

            temp = BitConverter.GetBytes(contentCount);
            Array.Reverse(temp);
            buffer.Write(temp);

            buffer.Write(SHA2Hash);

            return buffer.GetBuffer();
        }
    }
}
