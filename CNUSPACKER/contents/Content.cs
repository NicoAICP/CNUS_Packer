using System;
using System.Collections.Generic;
using System.IO;
using CNUSPACKER.crypto;
using CNUSPACKER.fst;
using CNUSPACKER.packaging;
using CNUSPACKER.utils;

namespace CNUSPACKER.contents
{
    public class Content
    {
        public const int staticFSTContentHeaderDataSize = 32;
        public const int staticDataSize = 48;

        private const short TYPE_CONTENT = 8192;
        private const short TYPE_ENCRYPTED = 1;
        public const short TYPE_HASHED = 2;

        public int ID { get; set; }

        public short index { get; set; }

        private short type = TYPE_CONTENT | TYPE_ENCRYPTED;

        public long encryptedFileSize { get; set; }

        public byte[] SHA1 { get; set; } = new byte[20];

        private long curFileOffset;
        private const int ALIGNMENT_IN_CONTENT_FILE = 32;
        public const int CONTENT_FILE_PADDING = 32768;

        private List<FSTEntry> entries = new List<FSTEntry>();

        public int groupID { get; set; }

        public long parentTitleID { get; set; }

        public short entriesFlags { get; set; }

        public bool isFSTContent { get; set; }

        public void AddType(short type)
        {
            this.type |= type;
        }

        public void removeType(short type)
        {
            this.type &= (short)~type;
        }

        private bool IsHashed()
        {
            return (type & TYPE_HASHED) == TYPE_HASHED;
        }

        public KeyValuePair<long, byte[]> GetFSTContentHeaderAsData(long old_content_offset)
        {
            MemoryStream buffer = new MemoryStream(staticFSTContentHeaderDataSize);

            byte unkwn;
            long content_offset = old_content_offset;
            long fst_content_size = encryptedFileSize / CONTENT_FILE_PADDING;
            long fst_content_size_written = fst_content_size;

            if (IsHashed())
            {
                unkwn = 2;
                fst_content_size_written -= ((fst_content_size / 64) + 1) * 2;
                if (fst_content_size_written < 0) fst_content_size_written = 0;
            }
            else
            {
                unkwn = 1;
            }

            if (isFSTContent)
            {
                unkwn = 0;
                if(fst_content_size == 1)
                {
                    fst_content_size = 0;
                }
                content_offset += fst_content_size + 2;
            }
            else
            {
                content_offset += fst_content_size;
            }

            // we need to write with big endian, so we'll Array.Reverse a lot
            byte[] temp;

            temp = BitConverter.GetBytes((int)old_content_offset);
            Array.Reverse(temp);
            buffer.Write(temp);

            temp = BitConverter.GetBytes((int)fst_content_size_written);
            Array.Reverse(temp);
            buffer.Write(temp);

            temp = BitConverter.GetBytes(parentTitleID);
            Array.Reverse(temp);
            buffer.Write(temp);

            temp = BitConverter.GetBytes(groupID);
            Array.Reverse(temp);
            buffer.Write(temp);

            buffer.WriteByte(unkwn);

            return new KeyValuePair<long, byte[]>(content_offset, buffer.GetBuffer());
        }

        public long GetOffsetForFileAndIncrease(FSTEntry fstEntry)
        {
            long old_fileoffset = curFileOffset;
            curFileOffset = old_fileoffset + Utils.Align(fstEntry.GetFileSize(), ALIGNMENT_IN_CONTENT_FILE);
            return old_fileoffset;
        }

        public void ResetFileOffset()
        {
            curFileOffset = 0;
        }

        private List<FSTEntry> GetFSTEntries()
        {
            return entries;
        }

        private int GetFSTEntryNumber()
        {
            return entries.Count;
        }

        public byte[] GetAsData()
        {
            MemoryStream buffer = new MemoryStream(staticDataSize);
            byte[] temp; // We need to write in big endian, so we're gonna Array.Reverse a lot

            temp = BitConverter.GetBytes(ID);
            Array.Reverse(temp);
            buffer.Write(temp);

            temp = BitConverter.GetBytes(index);
            Array.Reverse(temp);
            buffer.Write(temp);

            temp = BitConverter.GetBytes(type);
            Array.Reverse(temp);
            buffer.Write(temp);

            temp = BitConverter.GetBytes(encryptedFileSize);
            Array.Reverse(temp);
            buffer.Write(temp);

            buffer.Write(SHA1);

            return buffer.GetBuffer();
        }

        public void PackContentToFile(string outputDir)
        {
            Console.WriteLine("Packing Content " + ID.ToString("X8") +"\n");

            NUSpackage nusPackage = NUSPackageFactory.GetPackageByContent(this);
            Encryption encryption = nusPackage.GetEncryption();
            Console.WriteLine("Packing files into one file:");
            //At first we need to create the decrypted file.
            FileInfo decryptedFile = packDecrypted();

            Console.WriteLine();
            Console.WriteLine("Generate hashes:");
            //Calculates the hashes for the decrypted content. If the content is not hashed,
            //only the hash of the decrypted file will be calculated

            ContentHashes contentHashes = new ContentHashes(decryptedFile, IsHashed());

            string h3_path = Path.Combine(outputDir, ID.ToString("X8") + ".h3");

            contentHashes.SaveH3ToFile(h3_path);
            SHA1 = contentHashes.TMDHash;
            Console.WriteLine();
            Console.WriteLine("Encrypt content (" + ID.ToString("X8") + ")");
            FileInfo encryptedFile = PackEncrypted(outputDir, decryptedFile, contentHashes, encryption);

            encryptedFileSize = encryptedFile.Length;

            Console.WriteLine();
            Console.WriteLine("Content " + ID.ToString("X8") + " packed to file \"" + encryptedFile.Name + "\"!");
            Console.WriteLine("-------------");
        }

        private FileInfo PackEncrypted(string outputDir, FileInfo decryptedFile, ContentHashes hashes, Encryption encryption)
        {
            string outputFilePath = Path.Combine(outputDir, ID.ToString("X8") + ".app");
            if((type & TYPE_HASHED) == TYPE_HASHED)
            {
                encryption.EncryptFileHashed(decryptedFile, this, outputFilePath, hashes);
            }
            else
            {
                encryption.EncryptFileWithPadding(decryptedFile, this, outputFilePath, CONTENT_FILE_PADDING);
            }

            return new FileInfo(Path.GetFullPath(outputFilePath));
        }

        private FileInfo packDecrypted()
        {
            string tmp_path = Path.Combine(Settings.tmpDir, ID.ToString("X8") + ".dec");
            using (FileStream fos = new FileStream(tmp_path, FileMode.Create))
            {
                int totalCount = GetFSTEntryNumber();
                int cnt_file = 1;
                long cur_offset = 0;
                foreach (FSTEntry entry in GetFSTEntries())
                {
                    if (!entry.notInPackage)
                    {
                        if (entry.IsFile())
                        {
                            if (cur_offset != entry.fileOffset)
                            {
                                Console.WriteLine("FAILED");
                            }
                            long old_offset = cur_offset;
                            cur_offset += Utils.Align(entry.GetFileSize(), ALIGNMENT_IN_CONTENT_FILE);
                            string output = "[" + cnt_file + "/" + totalCount + "] Writing at " + old_offset + " | FileSize: " + entry.GetFileSize() + " | " + entry.filename;

                            Utils.copyFileInto(entry.filepath, fos, output);

                            int padding = (int)(cur_offset - (old_offset + entry.GetFileSize()));
                            fos.Write(new byte[padding]);
                        }
                        else
                        {
                            Console.WriteLine("[" + cnt_file + "/" + totalCount + "] Wrote folder: \"" + entry.filename + "\"");
                        }
                    }
                    cnt_file++;
                }
            }

            return new FileInfo(Path.GetFullPath(tmp_path));
        }

        public void Update(List<FSTEntry> entries)
        {
            if(entries != null)
            {
                this.entries = entries;
            }
        }

        public bool Equals(Content other)
        {
            if (other == null)
            {
                return false;
            }

            return ID == other.ID;
        }
    }
}
