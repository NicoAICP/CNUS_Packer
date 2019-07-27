using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CNUS_packer.contents;
using CNUS_packer.packaging;

namespace CNUS_packer.fst
{
    public enum Types
    {
        DIR = 0x01,
        notInNUS = 0x80,
        WiiVC = 0x02
    }

    public class FSTEntry
    {
        public readonly string filepath;
        public readonly string filename = "";
        private FSTEntry parent;
        public readonly List<FSTEntry> children = new List<FSTEntry>();
        private int nameOffset;
        private int entryOffset;

        private short flags;

        public readonly bool isDir;
        public int parentOffset { get; set; }
        public int nextOffset { get; set; }

        private readonly long fileSize;
        public long fileOffset { get; private set; }

        private readonly bool isRoot;

        private int rootEntryCount;

        private Content content;

        public readonly bool notInPackage;

        public FSTEntry(string filepath, bool notInPackage = false)
        {
            this.filepath = Path.GetFullPath(filepath);
            if (Directory.Exists(filepath))
            {
                isDir = true;
            }
            else
            {
                fileSize = new FileInfo(filepath).Length;
            }
            filename = Path.GetFileName(filepath);

            this.notInPackage = notInPackage;
        }

        public FSTEntry(bool root)
        {
            filepath = null;
            if (root)
            {
                isRoot = true;
                isDir = true;
            }
        }

        public void AddChildren(FSTEntry fstEntry)
        {
            children.Add(fstEntry);
            fstEntry.parent = this;
        }

        public void SetContent(Content content)
        {
            flags = content.entriesFlags;
            this.content = content;
        }

        public void SetContentRecursive(Content content)
        {
            SetContent(content);
            foreach(FSTEntry entry in children)
            {
                entry.SetContentRecursive(content);
            }
        }

        public long GetFileSize()
        {
            return !IsFile() ? 0 : fileSize;
        }

        public bool IsFile()
        {
            return !(isDir || notInPackage);
        }

        private byte getType()
        {
            byte type = 0;
            if (isDir)
                type |= (byte)Types.DIR;
            if (notInPackage)
                type |= (byte)Types.notInNUS;
            if (filename.EndsWith("nfs"))
                type |= (byte)Types.WiiVC;
            return type;
        }

        public byte[] GetAsData()
        {
            MemoryStream buffer = new MemoryStream(GetDataSize());
            byte[] temp; // we need to write in big endian, so we're gonna Array.Reverse a lot
            if (isRoot)
            {
                buffer.WriteByte(1);
                buffer.Seek(7, SeekOrigin.Current);
                temp = BitConverter.GetBytes(rootEntryCount);
                Array.Reverse(temp);
                buffer.Write(temp);
                buffer.Seek(4, SeekOrigin.Current);
            }
            else
            {
                buffer.WriteByte(getType());
                buffer.WriteByte((byte)((nameOffset >> 16) & 0xFF)); // We need to write a 24bit int (big endian)
                buffer.WriteByte((byte)((nameOffset >> 8) & 0xFF));
                buffer.WriteByte((byte)(nameOffset & 0xFF));

                if (isDir)
                {
                    temp = BitConverter.GetBytes(parentOffset);
                    Array.Reverse(temp);
                    buffer.Write(temp);
                    temp = BitConverter.GetBytes(nextOffset);
                    Array.Reverse(temp);
                    buffer.Write(temp);
                }
                else if (IsFile())
                {
                    temp = BitConverter.GetBytes((int)(fileOffset >> 5));
                    Array.Reverse(temp);
                    buffer.Write(temp);
                    temp = BitConverter.GetBytes((int)fileSize);
                    Array.Reverse(temp);
                    buffer.Write(temp);
                }
                else if (notInPackage)
                {
                    Console.WriteLine("WTF IS HAPPENING");
                    buffer.Seek(4, SeekOrigin.Current);
                    temp = BitConverter.GetBytes((int)fileSize);
                    Array.Reverse(temp);
                    buffer.Write(temp);
                } else Console.WriteLine("WTF IS HAPPENING 2");
                temp = BitConverter.GetBytes(flags);
                Array.Reverse(temp);
                buffer.Write(temp);
                temp = BitConverter.GetBytes((short)content.ID);
                Array.Reverse(temp);
                buffer.Write(temp);
            }

            foreach (FSTEntry entry in children)
            {
                buffer.Write(entry.GetAsData());
            }

            return buffer.GetBuffer();
        }

        private int GetDataSize()
        {
            int size = 0x10;
            foreach (FSTEntry entry in children)
            {
                size += entry.GetDataSize();
            }
            return size;
        }

        private void SetNameOffset(int nameOffset)
        {
            if (nameOffset > 0xFFFFFF)
            {
                Console.WriteLine("Warning: filename offset is too big. Maximum is " + 0xFFFFFF + ", tried to set to " + nameOffset);
            }
            this.nameOffset = nameOffset;
        }

        public void Update()
        {
            SetNameOffset(FST.GetStringPosition());
            FST.addString(filename);
            entryOffset = FST.curEntryOffset;
            FST.curEntryOffset++;

            if (isDir && !isRoot)
            {
                parentOffset = parent.entryOffset;
            }

            if (content != null && IsFile())
            {
                fileOffset = content.GetOffsetForFileAndIncrease(this);
            }

            foreach (FSTEntry entry in children)
            {
                entry.Update();
            }
        }

        public FSTEntry UpdateDirRefs()
        {
            if (!(isDir || isRoot)) return null;
            if (parent != null)
            {
                parentOffset = parent.entryOffset;
            }

            FSTEntry result = null;

            for (int i = 0; i < GetDirChildren().Count; i++)
            {
                FSTEntry cur_dir = GetDirChildren()[i];
                if (GetDirChildren().Count > i + 1)
                    cur_dir.nextOffset = GetDirChildren()[i + 1].entryOffset;

                FSTEntry cur_result = cur_dir.UpdateDirRefs();

                if (cur_result != null)
                {
                    FSTEntry cur_foo = cur_result.parent;
                    while (cur_foo.nextOffset == 0)
                    {
                        cur_foo = cur_foo.parent;
                    }
                    cur_result.nextOffset = cur_foo.nextOffset;
                }

                if (GetDirChildren().Count > i)
                    result = cur_dir;
            }

            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (isDir) sb.Append("DIR: ").Append("\n");
            if (isDir) sb.Append("Filename: ").Append(filename).Append("\n");
            if (isDir) sb.Append("       ID:").Append(entryOffset).Append("\n");
            if (isDir) sb.Append(" ParentID:").Append(parentOffset).Append("\n");
            if (isDir) sb.Append("   NextID:").Append(nextOffset).Append("\n");
            foreach (FSTEntry child in children)
            {
                sb.Append(child);
            }

            return sb.ToString();
        }

        public void PrintRecursive(int space)
        {
            for (int i = 0; i < space; i++)
            {
                Console.Write(" ");
            }
            Console.Write(filename);
            if (notInPackage)
            {
                Console.Write(" (not in package) ");
            }
            Console.WriteLine();
            foreach (FSTEntry child in GetDirChildren(true))
            {
                child.PrintRecursive(space + 1);
            }
            foreach (FSTEntry child in GetFileChildren(true))
            {
                child.PrintRecursive(space + 1);
            }
        }

        public List<FSTEntry> GetFstEntriesByContent(Content content)
        {
            List<FSTEntry> entries = new List<FSTEntry>();
            if(this.content == null)
            {
                if (isDir)
                {
                    Console.Error.WriteLine("The folder \"" + filename + "\" is empty. Please add a dummy file to it.");
                }
                else
                {
                    Console.Error.WriteLine("The file \"" + filename + "\" is not assigned to any content (.app).");
                    Console.Error.WriteLine("Please delete it or write a corresponding content rule.");
                }
                Environment.Exit(0);
            }
            else if (this.content.Equals(content))
            {
                entries.Add(this);
            }

            foreach (FSTEntry child in children)
            {
                entries.AddRange(child.GetFstEntriesByContent(content));
            }
            return entries;
        }

        public int GetEntryCount()
        {
            int count = 1;
            foreach (FSTEntry child in children)
            {
                count += child.GetEntryCount();
            }
            return count;
        }

        public void SetEntryCount(int fstEntryCount)
        {
            rootEntryCount = fstEntryCount;
        }

        public List<FSTEntry> GetDirChildren(bool all = false)
        {
            List<FSTEntry> result = new List<FSTEntry>();
            foreach(FSTEntry child in children)
            {
                if(child.isDir && (all || !child.notInPackage))
                {
                    result.Add(child);
                }
            }
            return result;
        }

        public List<FSTEntry> GetFileChildren(bool all = false)
        {
            List<FSTEntry> result = new List<FSTEntry>();
            foreach(FSTEntry child in children)
            {
                if(child.IsFile() || (all && !child.isDir))
                {
                    result.Add(child);
                }
            }
            return result;
        }
    }
}
