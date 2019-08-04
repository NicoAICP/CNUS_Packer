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
        public const int CONTENT_FILE_PADDING = 32768;
        public const short TYPE_HASHED = 2;

        private const int ALIGNMENT_IN_CONTENT_FILE = 32;
        private const short TYPE_CONTENT = 8192;
        private const short TYPE_ENCRYPTED = 1;

        private short type = TYPE_CONTENT | TYPE_ENCRYPTED;
        private long curFileOffset;
        private List<FSTEntry> entries = new List<FSTEntry>();

        public int ID { get; set; }
        public short index { get; set; }
        public long encryptedFileSize { get; set; }
        public byte[] SHA1 { get; set; } = new byte[20];
        public int groupID { get; set; }
        public long parentTitleID { get; set; }
        public short entriesFlags { get; set; }
        public bool isFSTContent { get; set; }

        public void AddType(short type)
        {
            this.type |= type;
        }

        public void RemoveType(short type)
        {
            this.type &= (short)~type;
        }

        private bool IsHashed()
        {
            return (type & TYPE_HASHED) == TYPE_HASHED;
        }

        public KeyValuePair<long, byte[]> GetFSTContentHeaderAsData(long old_content_offset)
        {
            BigEndianMemoryStream buffer = new BigEndianMemoryStream(staticFSTContentHeaderDataSize);

            byte unknown;
            long content_offset = old_content_offset;
            long fst_content_size = encryptedFileSize / CONTENT_FILE_PADDING;
            long fst_content_size_written = fst_content_size;

            if (IsHashed())
            {
                unknown = 2;
                fst_content_size_written -= ((fst_content_size / 64) + 1) * 2;
                if (fst_content_size_written < 0)
                    fst_content_size_written = 0;
            }
            else
            {
                unknown = 1;
            }

            if (isFSTContent)
            {
                unknown = 0;
                if (fst_content_size == 1)
                    fst_content_size = 0;

                content_offset += fst_content_size + 2;
            }
            else
            {
                content_offset += fst_content_size;
            }

            buffer.WriteBigEndian((int)old_content_offset);
            buffer.WriteBigEndian((int)fst_content_size_written);
            buffer.WriteBigEndian(parentTitleID);
            buffer.WriteBigEndian(groupID);
            buffer.WriteByte(unknown);

            return new KeyValuePair<long, byte[]>(content_offset, buffer.GetBuffer());
        }

        public long GetOffsetForFileAndIncrease(FSTEntry fstEntry)
        {
            long old_fileoffset = curFileOffset;
            curFileOffset = old_fileoffset + Utils.Align(fstEntry.fileSize, ALIGNMENT_IN_CONTENT_FILE);

            return old_fileoffset;
        }

        public void ResetFileOffset()
        {
            curFileOffset = 0;
        }

        public byte[] GetAsData()
        {
            BigEndianMemoryStream buffer = new BigEndianMemoryStream(staticDataSize);
            buffer.WriteBigEndian(ID);
            buffer.WriteBigEndian(index);
            buffer.WriteBigEndian(type);
            buffer.WriteBigEndian(encryptedFileSize);

            buffer.Write(SHA1);

            return buffer.GetBuffer();
        }

        public void PackContentToFile(string outputDir)
        {
            Console.WriteLine($"Packing Content {ID:X8}\n");

            NUSpackage nusPackage = NUSPackageFactory.GetPackageByContent(this);
            Encryption encryption = nusPackage.GetEncryption();
            Console.WriteLine("Packing files into one file:");
            //At first we need to create the decrypted file.
            FileInfo decryptedFile = PackDecrypted();

            Console.WriteLine();
            Console.WriteLine("Generate hashes:");
            //Calculates the hashes for the decrypted content. If the content is not hashed,
            //only the hash of the decrypted file will be calculated

            ContentHashes contentHashes = new ContentHashes(decryptedFile, IsHashed());

            string h3Path = Path.Combine(outputDir, $"{ID:X8}.h3");

            contentHashes.SaveH3ToFile(h3Path);
            SHA1 = contentHashes.TMDHash;
            Console.WriteLine();
            Console.WriteLine($"Encrypt content ({ID:X8})");
            FileInfo encryptedFile = PackEncrypted(outputDir, decryptedFile, contentHashes, encryption);

            encryptedFileSize = encryptedFile.Length;

            Console.WriteLine();
            Console.WriteLine($"Content {ID:X8} packed to file \"{encryptedFile.Name}\"!");
            Console.WriteLine("-------------");
        }

        private FileInfo PackEncrypted(string outputDir, FileInfo decryptedFile, ContentHashes hashes, Encryption encryption)
        {
            string outputFilePath = Path.Combine(outputDir, $"{ID:X8}.app");
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

        private FileInfo PackDecrypted()
        {
            string tmpPath = Path.Combine(Settings.tmpDir, $"{ID:X8}.dec");
            using (FileStream fos = new FileStream(tmpPath, FileMode.Create))
            {
                int totalCount = entries.Count;
                int cnt_file = 1;
                long cur_offset = 0;
                foreach (FSTEntry entry in entries)
                {
                    if (entry.isFile)
                    {
                        if (cur_offset != entry.fileOffset)
                        {
                            Console.WriteLine("FAILED");
                        }
                        long old_offset = cur_offset;
                        cur_offset += Utils.Align(entry.fileSize, ALIGNMENT_IN_CONTENT_FILE);
                        string output = $"[{cnt_file}/{totalCount}] Writing at {old_offset} | FileSize: {entry.fileSize} | {entry.filename}";

                        Utils.CopyFileInto(entry.filepath, fos, output);

                        int padding = (int)(cur_offset - (old_offset + entry.fileSize));
                        fos.Write(new byte[padding]);
                    }
                    else
                    {
                        Console.WriteLine($"[{cnt_file}/{totalCount}] Wrote folder: \"{entry.filename}\"");
                    }
                    cnt_file++;
                }
            }

            return new FileInfo(Path.GetFullPath(tmpPath));
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
