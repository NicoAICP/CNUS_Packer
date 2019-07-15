using CNUS_packer.crypto;
using CNUS_packer.fst;
using CNUS_packer.packaging;
using CNUS_packer.utils;
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
            content.setID(contents.Count);
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
            return buffer.ToArray();
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
                Pair<byte[], long> result = c.getFSTContentHeaderAsData(content_offset);
                buffer.Write(result.getKey());
                content_offset = result.getValue();
            }
            return buffer.ToArray();
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
            Console.WriteLine("Packing the FST into " + fstContent.getID().ToString("00000000") + ".app");
            string fst_path = outputDir + "/" + fstContent.getID().ToString("00000000")+".app";
            encryption.encryptFileWithPadding(nuspackage.getFST(), fst_path, (short)getFSTContent().getID(), Content.CONTENT_FILE_PADDING);
            Console.WriteLine("-------------");
            Console.WriteLine("Packed all contents\n\n");
        }

        public void deleteContent(Content cur_content)
        {
            contents.Remove(cur_content);
        }
    }
}
