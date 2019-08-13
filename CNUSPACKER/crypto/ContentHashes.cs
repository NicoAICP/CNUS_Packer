using System;
using System.Collections.Generic;
using System.IO;
using CNUSPACKER.contents;
using CNUSPACKER.utils;

namespace CNUSPACKER.crypto
{
    public class ContentHashes
    {
        private readonly Dictionary<int, byte[]> h0Hashes = new Dictionary<int, byte[]>();
        private readonly Dictionary<int, byte[]> h1Hashes = new Dictionary<int, byte[]>();
        private readonly Dictionary<int, byte[]> h2Hashes = new Dictionary<int, byte[]>();
        private readonly Dictionary<int, byte[]> h3Hashes = new Dictionary<int, byte[]>();

        public readonly byte[] TMDHash;

        private int blockCount;

        public ContentHashes(string file, bool hashed)
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

        private void CalculateOtherHashes(int hashLevel, Dictionary<int, byte[]> inHashes, Dictionary<int, byte[]> outHashes)
        {
            int hash_level_pow = 1 << (4 * hashLevel);

            int hashesCount = (blockCount / hash_level_pow) + 1;
            for (int new_block = 0; new_block < hashesCount; new_block++)
            {
                byte[] cur_hashes = new byte[16 * 20];
                for (int i = new_block * 16; i < (new_block * 16) + 16; i++)
                {
                    if (inHashes.ContainsKey(i))
                        Array.Copy(inHashes[i], 0, cur_hashes, (i % 16) * 20, 20);
                }
                outHashes.Add(new_block, HashUtil.HashSHA1(cur_hashes));

                if (new_block % 100 == 0)
                {
                    Console.Write($"\rcalculating h{hashLevel}: {100 * new_block / hashesCount}%");
                }
            }
            Console.WriteLine($"\rcalculating h{hashLevel}: done");
        }

        private void CalculateH0Hashes(string file)
        {
            using FileStream input = new FileStream(file, FileMode.Open);

            const int bufferSize = 0xFC00;

            byte[] buffer = new byte[bufferSize];
            int total_blocks = (int)(input.Length / bufferSize) + 1;
            for (int block = 0; block < total_blocks; block++)
            {
                input.Read(buffer, 0, bufferSize);

                h0Hashes.Add(block, HashUtil.HashSHA1(buffer));

                if (block % 100 == 0)
                {
                    Console.Write($"\rcalculating h0: {100 * block / total_blocks}%");
                }
            }
            Console.WriteLine("\rcalculating h0: done");
            blockCount = total_blocks;
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

        public void SaveH3ToFile(string h3Path)
        {
            if (h3Hashes.Count > 0)
            {
                using FileStream fos = new FileStream(h3Path, FileMode.Create);

                fos.Write(GetH3Hashes());
            }
        }
    }
}
