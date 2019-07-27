using CNUS_packer.contents;
using CNUS_packer.packaging;

using System;
using System.Collections.Generic;
using System.IO;


namespace CNUS_packer.fst
{
    public class FSTEntries
    {
        private readonly List<FSTEntry> entries = new List<FSTEntry>();

        public FSTEntries()
        {
            FSTEntry root = new FSTEntry(true);
            entries.Add(root);
        }

        public bool AddEntry(FSTEntry entry)
        {
            if (!entry.isDir)
            {
                Console.WriteLine("FSTEntries in root need to be directories.");
                return false;
            }
            entries.Add(entry);
            return true;
        }

        public void Update()
        {
            foreach (FSTEntry entry in entries)
            {
                entry.Update();
            }
            UpdateDirRefs();
        }

        public List<FSTEntry> GetFSTEntriesByContent(Content content)
        {
            List<FSTEntry> result = new List<FSTEntry>();
            foreach (FSTEntry curEntry in entries)
            {
                if (!curEntry.notInPackage)
                {
                    result.AddRange(curEntry.GetFstEntriesByContent(content));
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

        public void UpdateDirRefs()
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
    }
}
