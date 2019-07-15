using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CNUS_packer.contents;


namespace CNUS_packer.fst
{
    public enum Types
    {
        DIR = 0x01,
        notInNUS = 0x80,
        WiiVC = 0x02
    }
    public enum Flags
    {
        NOBIGFILE = 0x04,
        HASHED = 0x400
    }

    public class FSTEntry
    {
        //private File file;
        private string filepath;
        private string filename = "";
        private FSTEntry parent = null;
        private List<FSTEntry> children = null;
        private int nameOffset = 0;
        private int entryOffset = 0x00;

        private short flags;

        private bool isDir = false;
        private int parentOffset = 0;
        private int nextOffset = 0;

        private long filesize = 0;
        private long fileoffset = 0;

        private bool isRoot = false;

        private int root_entryCount = 0;

        private Content content = null;

        private byte[] decryptedSHA1 = new byte[0x14];

        private bool bigFile = false;

        private bool hashedFile = false;

        private bool notInPackage = false;

        /* public FSTEntry(File file) : this(file, false)
        {

        }*/
        public FSTEntry(string filepath) : this(filepath, false)
        {

        }
        /* public FSTEntry(File file, bool notInPackage)
        {
            if(file == null || !file.exists())
            {
                throw new Exception("Couldn't create FSTEntry, file is null or doesn't exists");

            }
            this.file = new File(file.getAbsolutePath());
            setDir(file.isDirectory());
            setFileName(file.getName());
            setFileSize(file.length());

            setNotInPackage(notInPackage);

            if (isFile())
            {
                decryptedSHA1 = null;
            }
        }*/

        public FSTEntry(string filepath, bool notInPackage)
        {
            //if(filepath == null || !System.IO.File.Exists(filepath) || !Directory.Exists(filepath))
            //{
            //    throw new Exception("Could not create FSTEntry, File is null or it doesnt exist");
            //}
            this.filepath = Path.GetFullPath(filepath);
            setDir(Directory.Exists(filepath));
            setFileName(Path.GetFileName(filepath));

            setFileSize(filepath);
            setNotInPackage(notInPackage);
            if (File.Exists(filepath))
            {
                decryptedSHA1 = null;
            }
        }

        public FSTEntry(bool root)
        {
            filepath = null;
            if (root)
            {
                setIsRoot(true);
                setDir(true);
            }
        }

        public void addChildren(FSTEntry fstEntry)
        {
            getChildren().Add(fstEntry);
            fstEntry.setParent(this);
        }

        public bool isNotInPackage()
        {
            return notInPackage;
        }

        public void setNotInPackage(bool notInPackage)
        {
            this.notInPackage = notInPackage;
        }

        public FSTEntry getParent()
        {
            return this.parent;
        }

        public void setParent(FSTEntry child)
        {
            this.parent = child;
        }

        private Content getContent()
        {
            return content;
        }

        public void setContent(Content content)
        {
            setFlags(content.getEntriesFlags());
            this.content = content;
        }

        public void setContentRecursive(Content content)
        {
            setContent(content);
            foreach(FSTEntry entry in getChildren())
            {
                entry.setContentRecursive(content);
            }
        }

        public string getFile()
        {
            return filepath;
        }

        public string getFilename()
        {
            return filename;
        }

        public void setFileName(string filename)
        {
            this.filename = filename;
        }

        public long getFilesize()
        {
            if (!isFile()) return 0;
            return filesize;
        }

        public void setFileSize(string file)
        {
            if (File.Exists(file))
            {
                FileInfo f1 = new FileInfo(file);
                this.filesize = f1.Length;
            }
        }

        public long getFileOffset()
        {
            return this.fileoffset;
        }

        public void setFileOffset(long fileOffset)
        {
            this.fileoffset = fileOffset;
        }

        private void setIsRoot(bool isRoot)
        {
            this.isRoot = isRoot;
        }

        public bool getIsRoot()
        {
            return isRoot;
        }

        public void setDir(bool isDir)
        {
            this.isDir = isDir;
        }

        public bool getIsDir()
        {
            return isDir;
        }

        public bool isFile()
        {
            return !(getIsDir() || isNotInPackage());
        }

        public bool isBigFile()
        {
            return bigFile;
        }

        public void setBigFile(bool bigFile)
        {
            this.bigFile = bigFile;
        }

        public bool isHashedFile()
        {
            return hashedFile;
        }

        public void setHashedFile(bool hashedFile)
        {
            this.hashedFile = hashedFile;
        }

        public byte getType()
        {
            byte type = 0;
            if (getIsDir()) type |= (byte)Types.DIR;
            if (isNotInPackage()) type |= (byte)Types.notInNUS;
            if (getFilename().EndsWith("nfs")) type |= (byte)Types.WiiVC;
            return type;
        }

        public short getFlags()
        {
            return flags;
        }

        public FSTEntry getEntryByName(string name)
        {
            FSTEntry result = null;
            foreach(FSTEntry f in getChildren())
            {
                if (f.getFilename().Equals(name))
                {
                    result = f;
                    break;
                }
            }
            return result;
        }

        public byte[] getAsData()
        {
            MemoryStream ms = new MemoryStream(getDataSize());
            BinaryWriter buffer = new BinaryWriter(ms);
            if (getIsRoot())
            {
                buffer.Write((byte)1);
                buffer.Write(new byte[0x07]);
                buffer.Write(root_entryCount);
                buffer.Write(new byte[0x04]);
            }
            else
            {
                buffer.Write(getType());
                buffer.Write((byte)((nameOffset >> 16) & 0xFF));       //We need to write a 24bit int..
                buffer.Write((short)((nameOffset) & 0xFFFF));
                if (getIsDir())
                {
                    buffer.Write(parentOffset);
                    buffer.Write(nextOffset);

                }
                else if (isFile())
                {
                    buffer.Write((int)(fileoffset >> 5));
                    buffer.Write((int)filesize);
                }
                else if (isNotInPackage())
                {
                    buffer.Write(0);
                    buffer.Write((int)filesize);
                }
                buffer.Write(getFlags());
                buffer.Write((short)content.getID());
            }
            if(children != null)
            {
                foreach(FSTEntry entry in getChildren())
                {
                    buffer.Write(entry.getAsData());
                }
            }
            return ms.ToArray();
        }

        public int getDataSize()
        {
            int size = 0x10;
            foreach(FSTEntry entry in getChildren())
            {
                size += entry.getDataSize();
            }
            return size;
        }

        public byte[] getDecryptedHash()
        {
            if(decryptedSHA1 == null)
            {
                calculateDecryptedHash();
            }
            return decryptedSHA1;
        }

        public void setNameOffset(int offset)
        {
            if(offset > 0xFFFFFF)
            {
                Console.WriteLine("Warning: filename offset is too big. Maximum is "+0xFFFFFF+" tried to set to " + offset);
            }
            this.nameOffset = offset;
        }

        public void update()
        {
            setNameOffset(packaging.FST.getStringPos());
            packaging.FST.addString(filename);
            setEntryOffset(packaging.FST.curEntryOffset);
            packaging.FST.curEntryOffset++;

            if(getIsDir() && !getIsRoot())
            {
                setParentOffset(getParent().getEntryOffset());
            }
            if(getContent() != null && isFile())
            {
                long fileoffset = getContent().getOffsetForFileAndIncrease(this);
                setFileOffset(fileoffset);
            }
            foreach(FSTEntry entry in getChildren())
            {
                entry.update();
            }
        }

        public FSTEntry updateDirRefs()
        {
            if (!(getIsDir() || getIsRoot())) return null;
            if(parent != null)
            {
                setParentOffset(getParent().getEntryOffset());
            }
            FSTEntry result = null;
            for (int  i = 0; i < getDirChildren().Count; i++)
            {
                FSTEntry cur_dir = getDirChildren()[i];
                if (i + 1 < getDirChildren().Count)
                {
                    cur_dir.setNextOffset(getDirChildren()[i + 1].entryOffset);
                }
                FSTEntry cur_result = cur_dir.updateDirRefs();
                if (cur_result != null)
                {
                    FSTEntry cur_foo = cur_result.getParent();
                    while(cur_foo.getNextOffset() == 0)
                    {
                        cur_foo = cur_foo.getParent();
                    }
                    cur_result.setNextOffset(cur_foo.getNextOffset());
                }
                if (!(i+1 < getDirChildren().Count))
                {
                    result = cur_dir;
                }
            }
            return result;
        }

        private int getNextOffset()
        {
            return nextOffset;
        }

        public void setEntryOffset(int entryOffset)
        {
            this.entryOffset = entryOffset;
        }

        public int getEntryOffset()
        {
            return entryOffset;
        }

        public void setNextOffset(int nextOffset)
        {
            this.nextOffset = nextOffset;
        }

        public string toString()
        {
            StringBuilder sb = new StringBuilder();
            if (getIsDir()) sb.Append("DIR: ").Append("\n");
            if (getIsDir()) sb.Append("Filename: ").Append(getFilename()).Append("\n");
            if (getIsDir()) sb.Append("       ID:").Append(getEntryOffset()).Append("\n");
            if (getIsDir()) sb.Append(" ParentID:").Append(parentOffset).Append("\n");
            if (getIsDir()) sb.Append("   NextID:").Append(nextOffset).Append("\n");
            foreach(FSTEntry e in getChildren())
            {
                sb.Append(e.toString());
            }
            return sb.ToString();
        }

        public void printRecursive(int space)
        {
            for(int i = 0; i<space; i++)
            {
                Console.Write(" ");
            }
            Console.Write(getFilename());
            if (isNotInPackage())
            {
                Console.Write(" (not in package) ");
            }
            Console.WriteLine();
            foreach(FSTEntry child in getDirChildren(true))
            {
                child.printRecursive(space + 1);
            }
            foreach (FSTEntry child in getFileChildren(true))
            {
                child.printRecursive(space + 1);
            }
        }

        public List<FSTEntry> getFSTEntriesByContent(Content content)
        {
            List<FSTEntry> entries = new List<FSTEntry>();
            if(this.content == null)
            {
                if (getIsDir())
                {
                    Console.WriteLine("The folder \"" + getFilename() + "\" is emtpy. Please add a dummy file to it.");
                }
                else
                {
                    Console.WriteLine("The file \"" + getFilename() + "\" is not assigned to any content (.app).");
                    Console.WriteLine("Please delete it or write a corresponding content rule");
                }
                Environment.Exit(0);
            }
            else
            {
                if (this.content.equals(content))
                {
                    entries.Add(this);
                }
            }
            foreach(FSTEntry child in getChildren())
            {
                entries.AddRange(child.getFSTEntriesByContent(content));
            }
            return entries;
        }

        public List<FSTEntry> getChildren()
        {
            if(children == null)
            {
                children = new List<FSTEntry>();
            }
            return children;
        }

        public int getEntryCount()
        {
            int count = 1;
            foreach (FSTEntry entry in getChildren())
            {
                count += entry.getEntryCount();
            }
            return count;
        }

        public void setParentOffset(int i)
        {
            this.parentOffset = i;
        }

        public void setEntryCount(int fstEntryCount)
        {
            this.root_entryCount = fstEntryCount;
        }

        public List<FSTEntry> getDirChildren()
        {
            return getDirChildren(false);
        }

        public List<FSTEntry> getDirChildren(bool all)
        {
            List<FSTEntry> result = new List<FSTEntry>();
            foreach(FSTEntry child in getChildren())
            {
                if(child.getIsDir() && (all || !child.isNotInPackage()))
                {
                    result.Add(child);
                }

            }
            return result;
        }

        public List<FSTEntry> getFileChildren()
        {
            return getFileChildren(false);
        }

        public List<FSTEntry> getFileChildren(bool all)
        {
            List<FSTEntry> result = new List<FSTEntry>();
            foreach(FSTEntry child in getChildren())
            {
                if(child.isFile() || (all && !child.getIsDir()))
                {
                    result.Add(child);
                }
            }
            return result;
        }

        public void calculateDecryptedHash()
        {
            decryptedSHA1 = utils.HashUtil.hashSHA1(filepath, 0x8000);
        }

        public void setFlags(short flags)
        {
            this.flags = flags;
        }
    }
}
