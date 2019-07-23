using CNUS_packer.crypto;
using CNUS_packer.fst;
using CNUS_packer.packaging;

using System;
using System.Collections.Generic;
using System.IO;

namespace CNUS_packer.contents
{
    public class Contents
    {
        private List<Content> contents = new List<Content>();
        private Content fstContent;

        public Contents()
        {
            setFSTContent(getNewContent());
        }

        public Content getFSTContent()
        {
            return this.fstContent;
        }

        public void setFSTContent(Content content)
        {
            this.fstContent = content;
            content.setFSTContent(true);
        }

        public Content GetContent()
        {
            return this.fstContent;
        }

        public Content getNewContent()
        {
            return getNewContent(false);
        }

        public Content getNewContent(bool isHashed)
        {
            ContentDetails details = new ContentDetails(isHashed, 0, 0, 0);
            return getNewContent(details);
        }

        public Content getNewContent(ContentDetails details)
        {
            Content content = new Content();
            content.ID = contents.Count;
            content.setIndex((short)contents.Count);
            if (details.GetisContent())
            {
                content.addType(Content.TYPE_CONTENT);
            }
            if (details.GetisEncrypted())
            {
                content.addType(Content.TYPE_ENCRYPTED);
            }
            if (details.GetisHashed())
            {
                content.addType(Content.TYPE_HASHED);
            }
            content.setEntriesFlags(details.getEntriesFlag());
            content.setGroupID(details.getGroupID());
            content.setParentTitleID(details.getParentTitleID());
            getContents().Add(content);

            return content;
        }

        public short getContentCount()
        {
            return (short)getContents().Count;
        }

        public byte[] getAsData()
        {
            MemoryStream buffer = new MemoryStream(getDataSize());
            foreach (Content c in getContents())
            {
                buffer.Write(c.getAsData());
            }

            return buffer.GetBuffer();
        }

        public int getDataSize()
        {
            int size = 0x00;
            foreach (Content c in getContents())
            {
                size += c.getDataSize();
            }
            return size;
        }

        public byte[] getFSTContentHeaderAsData()
        {
            long content_offset = 0;
            MemoryStream buffer = new MemoryStream(getFSTContentHeaderDataSize());
            foreach (Content c in getContents())
            {
                KeyValuePair<long, byte[]> result = c.getFSTContentHeaderAsData(content_offset);
                content_offset = result.Key;
                buffer.Write(result.Value);
            }

            return buffer.GetBuffer();
        }

        public int getFSTContentHeaderDataSize()
        {
            int size = 0;
            foreach (Content c in getContents())
            {
                size += c.getFSTContentHeaderDataSize();
            }
            return size;
        }

        public List<Content> getContents()
        {
            if (contents == null)
            {
                contents = new List<Content>();
            }
            return contents;
        }

        public void resetFileOffsets()
        {
            foreach (Content c in getContents())
            {
                c.resetFileOffsets();
            }
        }

        public void update(FSTEntries fileEntries)
        {

            foreach (Content c in getContents())
            {
                c.update(fileEntries.getFSTEntriesByContent(c));
            }
        }

        public void packContents(string outputDir)
        {
            foreach(Content c in getContents())
            {
                if (!c.equals(getFSTContent()))
                {
                    c.packContentToFile(outputDir);
                }
            }
            NUSpackage nuspackage = NUSPackageFactory.getPackageByContents(this);
            Encryption encryption = nuspackage.getEncryption();

            Console.WriteLine("Packing the FST into " + fstContent.ID.ToString("X8") + ".app");
            string fst_path = Path.Combine(outputDir, fstContent.ID.ToString("X8") + ".app");
            encryption.encryptFileWithPadding(nuspackage.fst, fst_path, (short)getFSTContent().ID, Content.CONTENT_FILE_PADDING);

            Console.WriteLine("-------------");
            Console.WriteLine("Packed all contents\n\n");
        }

        public void deleteContent(Content cur_content)
        {
            contents.Remove(cur_content);
        }
    }
}
