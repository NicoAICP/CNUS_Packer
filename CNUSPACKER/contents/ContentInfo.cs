using CNUSPACKER.utils;

namespace CNUSPACKER.contents
{
    public class ContentInfo
    {
        public const int staticDataSize = 0x24;
        private readonly short indexOffset;
        private readonly short contentCount;
        public byte[] SHA2Hash { get; set; }

        public ContentInfo(short contentCount): this(0, contentCount)
        {
        }

        private ContentInfo(short indexOffset, short contentCount)
        {
            this.indexOffset = indexOffset;
            this.contentCount = contentCount;
        }

        public byte[] GetAsData()
        {
            BigEndianMemoryStream buffer = new BigEndianMemoryStream(2304);
            buffer.WriteBigEndian(indexOffset);
            buffer.WriteBigEndian(contentCount);

            buffer.Write(SHA2Hash);

            return buffer.GetBuffer();
        }
    }
}
