using System;
using System.Collections.Generic;
using System.IO;
using CNUSPACKER.crypto;
using CNUSPACKER.fst;
using CNUSPACKER.utils;

namespace CNUSPACKER.contents
{
    public class Content
    {
        public const int staticFSTContentHeaderDataSize = 32;
        public const int staticDataSize = 48;
        public const int CONTENT_FILE_PADDING = 32768;

        private const int ALIGNMENT_IN_CONTENT_FILE = 32;
        private const short TYPE_CONTENT = 8192;
        private const short TYPE_ENCRYPTED = 1;
        private const short TYPE_HASHED = 2;

        private readonly short type = TYPE_CONTENT | TYPE_ENCRYPTED;
        private bool IsHashed => (type & TYPE_HASHED) == TYPE_HASHED;
        private long curFileOffset;
        private List<FSTEntry> entries = new List<FSTEntry>();

        public readonly int ID;
        private readonly short index;
        public long encryptedFileSize { get; set; }
        public byte[] SHA1 { get; set; } = new byte[20];
        private readonly int groupID;
        private readonly long parentTitleID;
        public readonly short entriesFlags;
        private readonly bool isFSTContent;

        public Content(int ID, short index, short entriesFlags, int groupID, long parentTitleID, bool isHashed, bool isFSTContent)
        {
            this.ID = ID;
            this.index = index;
            this.entriesFlags = entriesFlags;
            this.groupID = groupID;
            this.parentTitleID = parentTitleID;
            if (isHashed)
                type |= TYPE_HASHED;
            this.isFSTContent = isFSTContent;
        }

        public KeyValuePair<long, byte[]> GetFSTContentHeaderAsData(long oldContentOffset)
        {
            BigEndianMemoryStream buffer = new BigEndianMemoryStream(staticFSTContentHeaderDataSize);

            byte unknown;
            long content_offset = oldContentOffset;
            long fst_content_size = encryptedFileSize / CONTENT_FILE_PADDING;
            long fst_content_size_written = fst_content_size;

            if (IsHashed)
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

            buffer.WriteBigEndian((int)oldContentOffset);
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

            buffer.Write(SHA1, 0, 20);

            return buffer.GetBuffer();
        }

        public void PackContentToFile(string outputDir, Encryption encryption)
        {
            Console.WriteLine($"Packing Content {ID:X8}\n");

            Console.WriteLine("Packing files into one file:");
            //At first we need to create the decrypted file.
            string decryptedFile = PackDecrypted();

            Console.WriteLine();
            Console.WriteLine("Generate hashes:");
            //Calculates the hashes for the decrypted content. If the content is not hashed,
            //only the hash of the decrypted file will be calculated

            ContentHashes contentHashes = new ContentHashes(decryptedFile, IsHashed);

            string h3Path = Path.Combine(outputDir, $"{ID:X8}.h3");

            contentHashes.SaveH3ToFile(h3Path);
            SHA1 = contentHashes.TMDHash;
            Console.WriteLine();
            Console.WriteLine($"Encrypt content ({ID:X8})");
            string outputFilePath = Path.Combine(outputDir, $"{ID:X8}.app");
            encryptedFileSize = PackEncrypted(decryptedFile, outputFilePath, contentHashes, encryption);

            Console.WriteLine();
            Console.WriteLine($"Content {ID:X8} packed to file \"{ID:X8}.app\"!");
            Console.WriteLine("-------------");
        }

        private long PackEncrypted(string decryptedFile, string outputFilePath, ContentHashes hashes, Encryption encryption)
        {
            using (FileStream input = new FileStream(decryptedFile, FileMode.Open))
            using (FileStream output = new FileStream(outputFilePath, FileMode.Create))
            {
                if (IsHashed)
                {
                    encryption.EncryptFileHashed(input, ID, output, hashes);
                }
                else
                {
                    encryption.EncryptFileWithPadding(input, ID, output, CONTENT_FILE_PADDING);
                }

                return output.Length;
            }
        }

        private string PackDecrypted()
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

                        Console.Write($"[{cnt_file}/{totalCount}] Writing at {cur_offset} | FileSize: {entry.fileSize} | {entry.filename}...");
                        using (FileStream input = new FileStream(entry.filepath, FileMode.Open))
                        {
                            input.CopyTo(fos);
                        }
                        Console.WriteLine($"\r[{cnt_file}/{totalCount}] Writing at {cur_offset} | FileSize: {entry.fileSize} | {entry.filename} : 100%");

                        long alignedFileSize = Utils.Align(entry.fileSize, ALIGNMENT_IN_CONTENT_FILE);
                        cur_offset += alignedFileSize;

                        long padding = alignedFileSize - entry.fileSize;
                        fos.Write(new byte[padding], 0, (int)padding);
                    }
                    else
                    {
                        Console.WriteLine($"[{cnt_file}/{totalCount}] Wrote folder: \"{entry.filename}\"");
                    }
                    cnt_file++;
                }
            }

            return tmpPath;
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
                return false;

            return ID == other.ID;
        }
    }
}
