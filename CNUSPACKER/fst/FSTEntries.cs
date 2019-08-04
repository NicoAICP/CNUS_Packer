using System.Collections.Generic;
using System.Linq;
using CNUSPACKER.contents;
using CNUSPACKER.packaging;

namespace CNUSPACKER.fst
{
    public class FSTEntries
    {
        private readonly List<FSTEntry> entries = new List<FSTEntry>();

        public FSTEntries()
        {
            FSTEntry root = new FSTEntry();
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
            return entries.SelectMany(entry => entry.GetFSTEntriesByContent(content)).ToList();
        }

        public int GetFSTEntryCount()
        {
            return entries.Sum(entry => entry.GetEntryCount());
        }

        public byte[] GetAsData()
        {
            return entries.SelectMany(entry => entry.GetAsData()).ToArray();
        }

        public int GetDataSize()
        {
            return GetFSTEntryCount() * 0x10;
        }

        public FSTEntry GetRootEntry()
        {
            return entries[0];
        }
    }
}
