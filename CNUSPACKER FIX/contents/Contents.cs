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
        public readonly List<Content> contents = new List<Content>();
        public readonly Content fstContent;

        public Contents()
        {
            ContentDetails details = new ContentDetails(false, 0, 0, 0);
            fstContent = GetNewContent(details);
            fstContent.isFSTContent = true;
        }

        public Content GetNewContent(ContentDetails details)
        {
            Content content = new Content
            {
                ID = contents.Count,
                index = (short) contents.Count,
                entriesFlags = details.entriesFlag,
                groupID = details.groupID,
                parentTitleID = details.parentTitleID
            };
            if (details.isHashed)
            {
                content.AddType(Content.TYPE_HASHED);
            }
            contents.Add(content);

            return content;
        }

        public short GetContentCount()
        {
            return (short) contents.Count;
        }

        public byte[] GetAsData()
        {
            MemoryStream buffer = new MemoryStream(GetDataSize());
            foreach (Content c in contents)
            {
                buffer.Write(c.GetAsData());
            }

            return buffer.GetBuffer();
        }

        public int GetDataSize()
        {
            return GetContentCount() * Content.staticDataSize;
        }

        public byte[] GetFSTContentHeaderAsData()
        {
            long content_offset = 0;
            MemoryStream buffer = new MemoryStream(GetFSTContentHeaderDataSize());
            foreach (Content c in contents)
            {
                KeyValuePair<long, byte[]> result = c.GetFSTContentHeaderAsData(content_offset);
                content_offset = result.Key;
                buffer.Write(result.Value);
            }

            return buffer.GetBuffer();
        }

        public int GetFSTContentHeaderDataSize()
        {
            return GetContentCount() * Content.staticFSTContentHeaderDataSize;
        }

        public void ResetFileOffsets()
        {
            foreach (Content c in contents)
            {
                c.ResetFileOffset();
            }
        }

        public void Update(FSTEntries fileEntries)
        {
            foreach (Content c in contents)
            {
                c.Update(fileEntries.GetFSTEntriesByContent(c));
            }
        }

        public void PackContents(string outputDir)
        {
            foreach (Content c in contents)
            {
                if (!c.Equals(fstContent))
                {
                    c.PackContentToFile(outputDir);
                }
            }
            NUSpackage nuspackage = NUSPackageFactory.GetPackageByContents(this);
            Encryption encryption = nuspackage.GetEncryption();

            Console.WriteLine("Packing the FST into " + fstContent.ID.ToString("X8") + ".app");
            string fst_path = Path.Combine(outputDir, fstContent.ID.ToString("X8") + ".app");
            encryption.EncryptFileWithPadding(nuspackage.fst, fst_path, (short)fstContent.ID, Content.CONTENT_FILE_PADDING);

            Console.WriteLine("-------------");
            Console.WriteLine("Packed all contents\n\n");
        }

        public void DeleteContent(Content cur_content)
        {
            contents.Remove(cur_content);
        }
    }
}
