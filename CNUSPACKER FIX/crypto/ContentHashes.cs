using System;
using System.Collections.Generic;
using System.IO;
using CNUS_packer.contents;
using CNUS_packer.utils;


namespace CNUS_packer.crypto
{
    public class ContentHashes
    {
        Dictionary<int, byte[]> h0hashes = new Dictionary<int, byte[]>();
        Dictionary<int, byte[]> h1hashes = new Dictionary<int, byte[]>();
        Dictionary<int, byte[]> h2hashes = new Dictionary<int, byte[]>();
        Dictionary<int, byte[]> h3hashes = new Dictionary<int, byte[]>();

        byte[] TMDHash = new byte[0x14];

        private int blockCount = 0;

        public ContentHashes(FileInfo file, bool hashed)
        {
            if (hashed)
            {
                try
                {
                    calculateH0Hashes(file);
                    calculateOtherHashes(1, h0hashes, h1hashes);
                    calculateOtherHashes(2, h1hashes, h2hashes);
                    calculateOtherHashes(3, h2hashes, h3hashes);
                    setTMDHash(HashUtil.hashSHA1(getH3Hashes()));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            else
            {
                setTMDHash(HashUtil.hashSHA1(file, Content.CONTENT_FILE_PADDING));
            }
        }

        private void calculateOtherHashes(int hash_level, Dictionary<int, byte[]> in_hashes, Dictionary<int, byte[]> out_hashes)
        {
            int hash_level_pow = (int)Math.Pow(16, hash_level);

            int hashescount = (blockCount / (hash_level_pow)) + 1;
            int new_blocks = 0;
            for (int j = 0; j < hashescount; j++)
            {
                byte[] cur_hashes = new byte[16 * 20];
                for (int i = j * 16; i < ((j + 16) + 16); i++)
                {
                    if (in_hashes.ContainsKey(i))
                    {
                        byte[] cur_hash = in_hashes[i];
                        Array.Copy(cur_hash, 0, cur_hashes, (i % 16) * 20, 20);
                    }
                    else
                    {
                        Array.Copy(new byte[20], 0, cur_hashes, (i % 16) * 20, 20);
                    }
                }
                out_hashes.Add(new_blocks, HashUtil.hashSHA1(cur_hashes));
                new_blocks++;
                int progress = (int)((new_blocks * 1.0 / hashescount * 1.0) * 100);
                if (new_blocks % 100 == 0)
                {
                    Console.WriteLine("\rcalculating h" + hash_level + ": " + progress + "%");
                }
            }
            Console.WriteLine("\rcalculating h" + hash_level + ": done");
        }

        private void calculateH0Hashes(FileInfo file)
        {
            using (FileStream fs = file.Open(FileMode.Open))
            {
                int buffer_size = 0xFC00;
                byte[] buffer = new byte[buffer_size];
                ByteArrayBuffer overflowbuffer = new ByteArrayBuffer(buffer_size);
                int read;
                int block = 0;
                int total_blocks = (int)(file.Length / buffer_size) + 1;
                do
                {
                    read = utils.utils.getChunkFromStream(fs, buffer, overflowbuffer, buffer_size);
                    if (read != buffer_size && false) // TODO: probably does literally nothing, will have to re-check
                    {
                        MemoryStream new_buffer = new MemoryStream(buffer_size);

                        new_buffer.Write(buffer);
                        buffer = new_buffer.ToArray();
                    }

                    byte[] hashtest = HashUtil.hashSHA1(buffer);

                    h0hashes.Add(block, hashtest);

                    block++;
                    int progress = (int)((block * 1.0 / total_blocks * 1.0) * 100);
                    if (block % 100 == 0)
                    {
                        Console.Write("\rcalculating h0: " + progress + "%");
                    }
                } while (read == buffer_size);

                Console.WriteLine("\rcalculating h0: done");
                setBlockCount(block);
            }
        }

        public byte[] getHashForBlock(int block)
        {
            if (block > blockCount)
            {
                throw new Exception("fofof");
            }

            MemoryStream hashes = new MemoryStream(0x400);
            int h0_hash_start = (block / 16) * 16;
            for (int i = 0; i < 16; i++)
            {
                int index = h0_hash_start + i;
                if (h0hashes.ContainsKey(index))
                {
                    hashes.Write(h0hashes[index]);
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
                if (h1hashes.ContainsKey(index))
                {
                    hashes.Write(h1hashes[index]);
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
                if (h2hashes.ContainsKey(index))
                {
                    hashes.Write(h2hashes[index]);
                }
                else
                {
                    hashes.Seek(20, SeekOrigin.Current);
                }
            }

            return hashes.GetBuffer();
        }

        public int getBlockCount()
        {
            return blockCount;
        }

        public void setBlockCount(int blockCount)
        {
            this.blockCount = blockCount;
        }

        public byte[] getH3Hashes()
        {
            MemoryStream buffer = new MemoryStream(h3hashes.Count * 0x14);
            for (int i = 0; i < h3hashes.Count; i++)
            {
                buffer.Write(h3hashes[i]);
            }

            return buffer.GetBuffer();
        }

        public byte[] getTMDHash()
        {
            return TMDHash;
        }

        public void setTMDHash(byte[] TMDHash)
        {
            this.TMDHash = TMDHash;
        }

        public void saveH3ToFile(string h3_path)
        {
            if (h3hashes.Count > 0)
            {
                using (FileStream fos = new FileStream(h3_path, FileMode.Create))
                {
                    fos.Write(getH3Hashes());
                }
            }
        }
    }
}
