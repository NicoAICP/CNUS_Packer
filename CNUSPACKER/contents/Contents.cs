using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CNUSPACKER.crypto;
using CNUSPACKER.fst;
using CNUSPACKER.packaging;

namespace CNUSPACKER.contents
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
            return contents.SelectMany(content => content.GetAsData()).ToArray();
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
                (long key, byte[] value) = c.GetFSTContentHeaderAsData(content_offset);
                content_offset = key;
                buffer.Write(value);
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

            Console.WriteLine($"Packing the FST into {fstContent.ID:X8}.app");
            string fstPath = Path.Combine(outputDir, $"{fstContent.ID:X8}.app");
            encryption.EncryptFileWithPadding(nuspackage.fst, fstPath, (short)fstContent.ID, Content.CONTENT_FILE_PADDING);

            Console.WriteLine("-------------");
            Console.WriteLine("Packed all contents\n\n");
        }

        public void DeleteContent(Content curContent)
        {
            contents.Remove(curContent);
        }
    }
}
