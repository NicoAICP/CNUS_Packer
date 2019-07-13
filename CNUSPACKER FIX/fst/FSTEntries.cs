using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CNUS_packer.contents;
using CNUS_packer.packaging;


namespace CNUS_packer.fst
{
   public class FSTEntries
    {
        private List<FSTEntry> entries = new List<FSTEntry>();
        public FSTEntries()
        {
            FSTEntry root = new FSTEntry(true);
            entries.Add(root);
        }
        public List<FSTEntry> getEntries()
        {
            if (entries == null)
            {
                entries = new List<FSTEntry>();
            }
            return entries;
        }
        /* public bool isEmpty()
         {
             return entries.isEmpty();
         }*/
        public bool addEntry(FSTEntry entry)
        {
            if (!entry.getIsDir())
            {
                Console.WriteLine("FSTEntries in root need to be directories.");
                return false;
            }
            getEntries().Add(entry);
            return true;
        }
        public void update()
        {
            foreach (FSTEntry entry in getEntries())
            {
                entry.update();
            }
            updateDirRefs();
        }
        public List<FSTEntry> getFSTEntriesByContent(Content content)
        {
            List<FSTEntry> result = new List<FSTEntry>();
            foreach (FSTEntry curEntry in getEntries())
            {
                if (!curEntry.isNotInPackage())
                {
                    result.AddRange(curEntry.getFSTEntriesByContent(content));
                }
            }
            return result;

        }
        public int getFSTEntryCount()
        {
            int count = 0;
            foreach (FSTEntry entry in getEntries())
            {
                count += entry.getEntryCount();
            }
            return count;
        }
        public byte[] getAsData()
        {
            MemoryStream buffer = new MemoryStream(getDataSize());
            foreach (FSTEntry entry in getEntries())
            {
                buffer.Write(entry.getAsData());
            }
            return buffer.ToArray();
        }
        public int getDataSize()
        {
            return getFSTEntryCount() * 0x10;
        }

        public FSTEntry getRootEntry()
        {
            List<FSTEntry> entries = getEntries();
            if (entries.Count == 0) return null;
            return entries[0];
        }

        public void updateDirRefs()
        {
            List<FSTEntry> entries = getEntries();
            if (entries.Count == 0) return;

            FSTEntry root = entries[0];
            root.setParentOffset(0);
            root.setNextOffset(FST.curEntryOffset);
            FSTEntry lastdir = root.updateDirRefs();
            if (lastdir != null)
            {
                lastdir.setNextOffset(FST.curEntryOffset);
            }
        }
    }
}
