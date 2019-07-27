using CNUS_packer.contents;
using CNUS_packer.fst;
using CNUS_packer.utils;

using System.IO;
using System.Text;
using System;

namespace CNUS_packer.packaging
{
    public class FST
    {
        private byte[] magicbytes = new byte[] { 0x46, 0x53, 0x54, 0x00 };
        private int unknown = 0x20;
        private int contentCount = 0;

        private Contents contents = null;
        private FSTEntries fileEntries = null;

        private static MemoryStream strings = new MemoryStream();

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

        public byte[] getAsData()
        {
            MemoryStream buffer = new MemoryStream(getDataSize());
            byte[] temp;

            buffer.Write(magicbytes);
            temp = BitConverter.GetBytes(unknown);
            Array.Reverse(temp);
            buffer.Write(temp);
            temp = BitConverter.GetBytes(contentCount);
            Array.Reverse(temp);
            buffer.Write(temp);
            buffer.Seek(20, SeekOrigin.Current);
            buffer.Write(contents.getFSTContentHeaderAsData());
            buffer.Write(fileEntries.getAsData());
            buffer.Write(strings.ToArray());
            buffer.Write(alignment);

            return buffer.GetBuffer();
        }

        public int getDataSize()
        {
            int size = 0;
            size += magicbytes.Length;
            size += 0x04; // unknown
            size += 0x04; // contentCount
            size += 20; // padding
            size += contents.getFSTContentHeaderDataSize();
            size += fileEntries.getDataSize();
            size += (int)strings.Position;
            int newsize = (int)Utils.align(size, 0x8000);
            alignment = new byte[newsize - size];
            return newsize;
        }

        public FSTEntries getFSTEntries()
        {
            if (fileEntries == null)
            {
                fileEntries = new FSTEntries();
            }
            return fileEntries;
        }

        public Contents getContents()
        {
            return contents;
        }
    }
}
