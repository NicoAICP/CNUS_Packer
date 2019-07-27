using System;
using System.Collections.Generic;
using System.IO;
using CNUS_packer.contents;
using CNUS_packer.utils;

namespace CNUS_packer.crypto
{
    public class ContentHashes
    {
        private readonly Dictionary<int, byte[]> h0Hashes = new Dictionary<int, byte[]>();
        private readonly Dictionary<int, byte[]> h1Hashes = new Dictionary<int, byte[]>();
        private readonly Dictionary<int, byte[]> h2Hashes = new Dictionary<int, byte[]>();
        private readonly Dictionary<int, byte[]> h3Hashes = new Dictionary<int, byte[]>();

        public byte[] TMDHash { get; }

        private int blockCount;

        public ContentHashes(FileInfo file, bool hashed)
        {
            if (hashed)
            {
                CalculateH0Hashes(file);
                CalculateOtherHashes(1, h0Hashes, h1Hashes);
                CalculateOtherHashes(2, h1Hashes, h2Hashes);
                CalculateOtherHashes(3, h2Hashes, h3Hashes);
                TMDHash = HashUtil.HashSHA1(GetH3Hashes());
            }
            else
            {
                TMDHash = HashUtil.HashSHA1(file, Content.CONTENT_FILE_PADDING);
            }
        }

        private void CalculateOtherHashes(int hash_level, Dictionary<int, byte[]> in_hashes, Dictionary<int, byte[]> out_hashes)
        {
            int hash_level_pow = 1 << (4 * hash_level);

            int hashescount = (blockCount / hash_level_pow) + 1;
            int new_blocks = 0;

            for (int j = 0; j < hashescount; j++)
            {
                byte[] cur_hashes = new byte[16 * 20];
                for (int i = j * 16; i < (j * 16) + 16; i++)
                {
                    if (in_hashes.ContainsKey(i))
                        Array.Copy(in_hashes[i], 0, cur_hashes, (i % 16) * 20, 20);
                }
                out_hashes.Add(new_blocks, HashUtil.HashSHA1(cur_hashes));
                new_blocks++;

                int progress = 100 * new_blocks / hashescount;
                if (new_blocks % 100 == 0)
                {
                    Console.Write("\rcalculating h" + hash_level + ": " + progress + "%");
                }
            }
            Console.WriteLine("\rcalculating h" + hash_level + ": done");
        }

        private void CalculateH0Hashes(FileInfo file)
        {
            using (FileStream fs = file.Open(FileMode.Open))
            {
                const int buffer_size = 0xFC00;
                byte[] buffer = new byte[buffer_size];
                ByteArrayBuffer overflowbuffer = new ByteArrayBuffer(buffer_size);
                int read;
                int block = 0;
                int total_blocks = (int)(file.Length / buffer_size) + 1;
                do
                {
                    read = Utils.GetChunkFromStream(fs, buffer, overflowbuffer, buffer_size);

                    h0Hashes.Add(block, HashUtil.HashSHA1(buffer));

                    block++;
                    int progress = 100 * block / total_blocks;
                    if (block % 100 == 0)
                    {
                        Console.Write("\rcalculating h0: " + progress + "%");
                    }
                } while (read == buffer_size);
                Console.WriteLine("\rcalculating h0: done");
                blockCount = block;
            }
        }

        public byte[] GetHashForBlock(int block)
        {
            if (block > blockCount)
            {
                throw new Exception("This shouldn't happen.");
            }

            MemoryStream hashes = new MemoryStream(0x400);
            int h0_hash_start = (block / 16) * 16;
            for (int i = 0; i < 16; i++)
            {
                int index = h0_hash_start + i;
                if (h0Hashes.ContainsKey(index))
                {
                    hashes.Write(h0Hashes[index]);
                }
                else
                {
                    hashes.Seek(20, SeekOrigin.Current);
                }
            }

            int h1_hash_start = (block / 256) * 16;
            for (int i = 0; i < 16; i++)
            {
                int index = h1_hash_start + i;
                if (h1Hashes.ContainsKey(index))
                {
                    hashes.Write(h1Hashes[index]);
                }
                else
                {
                    hashes.Seek(20, SeekOrigin.Current);
                }
            }

            int h2_hash_start = (block / 4096) * 16;
            for (int i = 0; i < 16; i++)
            {
                int index = h2_hash_start + i;
                if (h2Hashes.ContainsKey(index))
                {
                    hashes.Write(h2Hashes[index]);
                }
                else
                {
                    hashes.Seek(20, SeekOrigin.Current);
                }
            }

            return hashes.GetBuffer();
        }

        private byte[] GetH3Hashes()
        {
            MemoryStream buffer = new MemoryStream(h3Hashes.Count * 20);
            for (int i = 0; i < h3Hashes.Count; i++)
            {
                buffer.Write(h3Hashes[i]);
            }

            return buffer.GetBuffer();
        }

        public void SaveH3ToFile(string h3_path)
        {
            if (h3Hashes.Count > 0)
            {
                using (FileStream fos = new FileStream(h3_path, FileMode.Create))
                {
                    fos.Write(GetH3Hashes());
                }
            }
        }
    }
}
