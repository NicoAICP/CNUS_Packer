using CNUS_packer.contents;
using CNUS_packer.fst;

using System.IO;
using System.Text;

namespace CNUS_packer.packaging
{
    public class FST
    {
        private byte[] magicbytes = new byte[] { 0x46, 0x53, 0x54, 0x00 };
        private int unknown = 0x20;
        private int contentCount = 0;

        private byte[] padding = new byte[0x14];

        private Contents contents = null;
        private FSTEntries fileEntries = null;

        private static MemoryStream strings = new MemoryStream(0x300000);

        public static int curEntryOffset = 0x00;

        private byte[] alignment = null;

        public FST(Contents contents)
        {
            this.contents = contents;
        }

        public void update()
        {
            strings.SetLength(0);
            curEntryOffset = 0;

            contents.resetFileOffsets();
            fileEntries.update();
            contents.update(fileEntries);
            fileEntries.getRootEntry().setEntryCount(fileEntries.getFSTEntryCount());

            contentCount = contents.getContentCount();
        }

        public static int getStringPos()
        {
            return (int)strings.Position;
        }

        public static void addString(string filename)
        {
            strings.Write(Encoding.ASCII.GetBytes(filename));
            strings.WriteByte(0x00);
        }

        private byte[] copyOfRange(byte[] src, int start, int end)
        {
            int len = end - start;
            byte[] dest = new byte[len];
            // note i is always from 0
            for (int i = 0; i < len; i++)
            {
                dest[i] = src[start + i]; // so 0..n = 0+x..n+x
            }
            return dest;
        }

        public byte[] getAsData()
        {
            MemoryStream stream = new MemoryStream(getDataSize());
            BinaryWriter buffer = new BinaryWriter(stream);
            buffer.Write(magicbytes);
            buffer.Write(unknown);
            buffer.Write(contentCount);
            buffer.Write(padding);
            buffer.Write(contents.getFSTContentHeaderAsData());
            buffer.Write(fileEntries.getAsData());
            buffer.Write(copyOfRange(strings.ToArray(), 0, (int)strings.Position));
            buffer.Write(alignment);
            return stream.ToArray();
        }

        public int getDataSize()
        {
            int size = 0;
            size += magicbytes.Length;
            size += 0x04; // unknown
            size += 0x04; // contentCount
            size += padding.Length;
            size += contents.getFSTContentHeaderDataSize();
            size += fileEntries.getDataSize();
            size += (int)strings.Position;
            int newsize = (int)utils.utils.align(size, 0x8000);
            alignment = new byte[newsize - size];
            return newsize;
        }

        public FSTEntries getFSTEntries()
        {
            if (fileEntries == null)
            {
                fileEntries = new FSTEntries();
            }
            return this.fileEntries;
        }

        public Contents getContents()
        {
            return contents;
        }
    }
}
