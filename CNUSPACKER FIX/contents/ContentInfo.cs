using System;
using System.IO;

namespace CNUS_packer.contents
{
    public class ContentInfo
    {
        private short indexOffset = 0x00;
        private short commandCount = 0x0B;
        private byte[] SHA2hash = null;

        public ContentInfo() : this(0)
        {
        }

        public ContentInfo(short contentCount): this(0, contentCount)
        {
        }

        public ContentInfo(short indexOffset, short contentCount)
        {
            this.indexOffset = indexOffset;
            this.commandCount = contentCount;
        }

        public byte[] getAsData()
        {
            MemoryStream buffer = new MemoryStream(0x24);
            byte[] temp;

            temp = BitConverter.GetBytes(getIndexOffset());
            Array.Reverse(temp);
            buffer.Write(temp);

            temp = BitConverter.GetBytes(getCommandCount());
            Array.Reverse(temp);
            buffer.Write(temp);

            buffer.Write(getSHA2Hash());

            return buffer.GetBuffer();
        }
        public static int getDataSizeStatic()
        {
            return 0x24;
        }

        public int getDataSize()
        {
            return 0x24;
        }

        public short getCommandCount()
        {
            return commandCount;
        }

        public short getIndexOffset()
        {
            return indexOffset;
        }

        public void setIndexOffset(short indexOffset)
        {
            this.indexOffset = indexOffset;
        }

        public byte[] getSHA2Hash()
        {
            return SHA2hash;
        }

        public void setSHA2Hash(byte[] SHA2Hash)
        {
            this.SHA2hash = SHA2Hash;
        }

        public void setCommandCount(short commandCount)
        {
            this.commandCount = commandCount;
        }
    }
}
