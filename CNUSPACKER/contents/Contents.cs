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
        private readonly List<Content> contents = new List<Content>();
        public readonly Content fstContent;

        public Contents()
        {
            ContentDetails details = new ContentDetails(false, 0, 0, 0);
            fstContent = GetNewContent(details, true);
        }

        public Content GetNewContent(ContentDetails details, bool isFSTContent = false)
        {
            Content content = new Content(contents.Count, (short) contents.Count, details.entriesFlag, details.groupID, details.parentTitleID, details.isHashed, isFSTContent);
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
            foreach (Content content in contents)
            {
                KeyValuePair<long, byte[]> data = content.GetFSTContentHeaderAsData(content_offset);
                content_offset = data.Key;
                buffer.Write(data.Value, 0, data.Value.Length);
            }

            return buffer.GetBuffer();
        }

        public int GetFSTContentHeaderDataSize()
        {
            return GetContentCount() * Content.staticFSTContentHeaderDataSize;
        }

        public void ResetFileOffsets()
        {
            foreach (Content content in contents)
            {
                content.ResetFileOffset();
            }
        }

        public void Update(FSTEntries fileEntries)
        {
            foreach (Content content in contents)
            {
                content.Update(fileEntries.GetFSTEntriesByContent(content));
            }
        }

        public void PackContents(string outputDir, Encryption encryption)
        {
            foreach (Content content in contents)
            {
                if (!content.Equals(fstContent))
                {
                    content.PackContentToFile(outputDir, encryption);
                }
            }
        }

        public void DeleteContent(Content curContent)
        {
            contents.Remove(curContent);
        }
    }
}
