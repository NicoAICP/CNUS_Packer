using System;
using System.IO;
using System.Security.Cryptography;

namespace CNUS_packer.utils
{
    public class HashUtil
    {
        public static byte[] hashSHA2(byte[] data)
        {
            SHA256 sha256 = SHA256.Create();

            return sha256.ComputeHash(data);
        }

        public static byte[] hashSHA1(byte[] data)
        {
            SHA1 sha1 = SHA1.Create();

            return sha1.ComputeHash(data);
        }

        public static byte[] hashSHA1(FileInfo file)
        {
            return hashSHA1(file, 0);
        }

        public static byte[] hashSHA1(FileInfo file, int alignment)
        {
            byte[] hash;
            SHA1 sha1 = SHA1.Create();
            using (FileStream fs = file.Open(FileMode.Open))
            {
                hash = Hash(sha1, fs, fs.Length, 0x8000, alignment);
            }

            return hash;
        }

        public static byte[] Hash(SHA1 digest, FileStream fs, long inputSize, int bufferSize, int alignment)
        {
            long target_size = (alignment == 0) ? inputSize : Utils.align(inputSize, alignment);

            long cur_position = 0;
            int inBlockBufferRead;

            ByteArrayBuffer overflow = new ByteArrayBuffer(bufferSize);
            MemoryStream finalbuffer = new MemoryStream();

            do
            {
                byte[] blockBuffer = new byte[bufferSize];
                if (cur_position + bufferSize > inputSize)
                {
                    long expectedSize = inputSize - cur_position;
                    Utils.getChunkFromStream(fs, blockBuffer, overflow, expectedSize);
                    inBlockBufferRead = bufferSize;
                }
                else
                {
                    int expectedSize = bufferSize;
                    inBlockBufferRead = Utils.getChunkFromStream(fs, blockBuffer, overflow, expectedSize);
                }
                finalbuffer.Write(blockBuffer, 0, inBlockBufferRead);

                cur_position += inBlockBufferRead;
            } while (cur_position < target_size && (inBlockBufferRead == bufferSize));

            return digest.ComputeHash(finalbuffer.ToArray());
        }
    }
}
