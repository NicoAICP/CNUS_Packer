using System.Collections.Generic;
using System.IO;
using CNUSPACKER.contents;
using CNUSPACKER.packaging;

namespace CNUSPACKER.fst
{
    public class FSTEntries
    {
        private readonly List<FSTEntry> entries = new List<FSTEntry>();

        public FSTEntries()
        {
            FSTEntry root = new FSTEntry(true);
            entries.Add(root);
        }

        public void Update()
        {
            foreach (FSTEntry entry in entries)
            {
                entry.Update();
            }
            UpdateDirRefs();
        }

        private void UpdateDirRefs()
        {
            if (entries.Count == 0)
                return;

            FSTEntry root = entries[0];
            root.parentOffset = 0;
            root.nextOffset = FST.curEntryOffset;
            FSTEntry lastdir = root.UpdateDirRefs();
            if (lastdir != null)
            {
                lastdir.nextOffset = FST.curEntryOffset;
            }
        }

        public List<FSTEntry> GetFSTEntriesByContent(Content content)
        {
            List<FSTEntry> result = new List<FSTEntry>();
            foreach (FSTEntry curEntry in entries)
            {
                if (!curEntry.notInPackage)
                {
                    result.AddRange(curEntry.GetFSTEntriesByContent(content));
                }
            }

            return result;
        }

        public int GetFSTEntryCount()
        {
            int count = 0;
            foreach (FSTEntry entry in entries)
            {
                count += entry.GetEntryCount();
            }

            return count;
        }

        public byte[] GetAsData()
        {
            MemoryStream buffer = new MemoryStream(GetDataSize());
            foreach (FSTEntry entry in entries)
            {
                buffer.Write(entry.GetAsData());
            }

            return buffer.GetBuffer();
        }

        public int GetDataSize()
        {
            return GetFSTEntryCount() * 0x10;
        }

        public FSTEntry GetRootEntry()
        {
            return (entries.Count == 0) ? null : entries[0];
        }
    }
}
