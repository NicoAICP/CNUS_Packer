using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CNUSPACKER.contents;
using CNUSPACKER.packaging;
using CNUSPACKER.utils;

namespace CNUSPACKER.fst
{
    public enum Types
    {
        DIR = 0x01,
        WiiVC = 0x02
    }

    public class FSTEntry
    {
        public readonly string filepath;
        public readonly string filename = "";
        public readonly List<FSTEntry> children = new List<FSTEntry>();
        private FSTEntry parent;
        private int nameOffset;
        private int entryOffset;

        private short flags;

        private readonly bool isRoot;
        private int rootEntryCount;

        public readonly bool isDir;
        public bool isFile => !isDir;
        public int parentOffset { get; set; }
        public int nextOffset { get; set; }

        public readonly long fileSize;
        public long fileOffset { get; private set; }

        private Content content;

        public FSTEntry()
        {
            isRoot = true;
            isDir = true;
        }

        public FSTEntry(string filepath)
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

        private byte GetTypeAsByte()
        {
            byte type = 0;
            if (isDir)
                type |= (byte)Types.DIR;
            if (filename.EndsWith("nfs"))
                type |= (byte)Types.WiiVC;

            return type;
        }

        public byte[] GetAsData()
        {
            BigEndianMemoryStream buffer = new BigEndianMemoryStream(GetDataSize());
            if (isRoot)
            {
                buffer.WriteByte(1);
                buffer.Seek(7, SeekOrigin.Current);
                buffer.WriteBigEndian(rootEntryCount);
                buffer.Seek(4, SeekOrigin.Current);
            }
            else
            {
                buffer.WriteByte(GetTypeAsByte());
                buffer.WriteByte((byte)(nameOffset >> 16)); // We need to write a 24bit int (big endian)
                buffer.WriteByte((byte)(nameOffset >> 8));
                buffer.WriteByte((byte)nameOffset);

                if (isDir)
                {
                    buffer.WriteBigEndian(parentOffset);
                    buffer.WriteBigEndian(nextOffset);
                }
                else
                {
                    buffer.WriteBigEndian((int)(fileOffset >> 5));
                    buffer.WriteBigEndian((int)fileSize);
                }

                buffer.WriteBigEndian(flags);
                buffer.WriteBigEndian((short)content.ID);
            }

            foreach (FSTEntry entry in children)
            {
                buffer.Write(entry.GetAsData(), 0, entry.GetDataSize());
            }

            return buffer.GetBuffer();
        }

        private int GetDataSize()
        {
            return 0x10 + children.Sum(child => child.GetDataSize());
        }

        private void SetNameOffset(int nameOffset)
        {
            if (nameOffset > 0xFFFFFF)
            {
                Console.WriteLine($"Warning: filename offset is too big. Maximum is {0xFFFFFF}, tried to set to {nameOffset}");
            }
            this.nameOffset = nameOffset;
        }

        public void Update()
        {
            SetNameOffset(FST.GetStringPosition());
            FST.AddString(filename);
            entryOffset = FST.curEntryOffset;
            FST.curEntryOffset++;

            if (isDir && !isRoot)
                parentOffset = parent.entryOffset;

            if (content != null && !isDir)
                fileOffset = content.GetOffsetForFileAndIncrease(this);

            foreach (FSTEntry entry in children)
            {
                entry.Update();
            }
        }

        public FSTEntry UpdateDirRefs()
        {
            if (!isDir)
                return null;
            if (parent != null)
                parentOffset = parent.entryOffset;

            FSTEntry result = null;

            List<FSTEntry> dirChildren = GetDirChildren().ToList();
            for (int i = 0; i < dirChildren.Count; i++)
            {
                FSTEntry cur_dir = dirChildren[i];
                if (dirChildren.Count > i + 1)
                    cur_dir.nextOffset = dirChildren[i + 1].entryOffset;

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

                if (dirChildren.Count > i)
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

        public void PrintRecursive(int space, int level = 0)
        {
            Console.Write(new string(' ', space * level));
            Console.WriteLine(filename);
            foreach (FSTEntry child in GetDirChildren())
            {
                child.PrintRecursive(space, level + 1);
            }
            foreach (FSTEntry child in GetFileChildren())
            {
                child.PrintRecursive(space, level + 1);
            }
        }

        public IEnumerable<FSTEntry> GetFSTEntriesByContent(Content content)
        {
            List<FSTEntry> entries = new List<FSTEntry>();
            if (this.content == null)
            {
                if (isDir)
                {
                    Console.Error.WriteLine($"The folder \"{filename}\" is empty. Please add a dummy file to it.");
                }
                else
                {
                    Console.Error.WriteLine($"The file \"{filename}\" is not assigned to any content (.app).");
                    Console.Error.WriteLine("Please delete it or write a corresponding content rule.");
                }
                Environment.Exit(0);
            }
            else if (this.content.Equals(content))
            {
                entries.Add(this);
            }

            entries.AddRange(children.SelectMany(child => child.GetFSTEntriesByContent(content)));
            return entries;
        }

        public int GetEntryCount()
        {
            return 1 + children.Sum(child => child.GetEntryCount());
        }

        public void SetEntryCount(int fstEntryCount)
        {
            rootEntryCount = fstEntryCount;
        }

        private IEnumerable<FSTEntry> GetDirChildren()
        {
            return children.Where(child => child.isDir);
        }

        private IEnumerable<FSTEntry> GetFileChildren()
        {
            return children.Where(child => child.isFile);
        }
    }
}
