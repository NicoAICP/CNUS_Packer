
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Console = System.Console;

namespace CNUS_packer.utils
{
    public class HashUtil
    {
        public static byte[] hashSHA2(byte[] data)
        {
            SHA256 sha256;
            try
            {
                sha256 = SHA256Managed.Create();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new byte[0x20];
            }

            return sha256.ComputeHash(data);
        }
        public static byte[] hashSHA1(byte[] data)
        {
            SHA1 sha1 = SHA1.Create("SHA-1");
            byte[] returning;

            try
             {
                returning = sha1.ComputeHash(data);
            }
             catch (Exception e)
             {

                 returning = new byte[0x14];
             }

            return returning;
             
            
        }
      
        public static byte[] hashSHA1(string filepath)
        {
            return hashSHA1(filepath, 0);
        }
        public static byte[] hashSHA1(string filepath, int alignment)
        {
            byte[] hash = new byte[0x14];
            SHA1 sha1 = SHA1Managed.Create();
            try
            {

                FileStream fs = System.IO.File.Open(filepath, FileMode.Open);

                hash = Hash(sha1, fs, fs.Length, 0x8000, alignment);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return hash;
        }
       

        private static byte[] copyOfRange(byte[] src, int start, int end)
        {
            int len = end - start;
            byte[] dest = new byte[len];
            // note i is always from 0
            for (int i = 0; i < len; i++)
            {
                dest[i] = src[start + i]; // so 0..n = 0+x..n+x
            }
            return dest;
        }
        public static byte[] Hash(SHA1 digest, FileStream fs, long inputSize, int bufferSize, int alignment)
        {


            long target_size = alignment == 0 ? inputSize : utils.align(inputSize, alignment);

            long cur_position = 0;
            int check = 0;
            int inBlockBufferRead = 0;
            byte[] blockBuffer = new byte[bufferSize];

            ByteArrayBuffer overflow = new ByteArrayBuffer(bufferSize);

            do
            {
                if ((cur_position + bufferSize) > inputSize)
                {
                    long expectedSize = (inputSize - cur_position);
                    MemoryStream buffer = new MemoryStream(bufferSize);

                    inBlockBufferRead = utils.getChunkFromStream(fs, blockBuffer, overflow, expectedSize);

                    buffer.Write(copyOfRange(blockBuffer, 0, inBlockBufferRead));

                    blockBuffer = buffer.ToArray();
                    check = (int)expectedSize;
                    inBlockBufferRead = bufferSize;
                }
                else
                {
                    int expectedSize = bufferSize;
                    inBlockBufferRead = utils.getChunkFromStream(fs, blockBuffer, overflow, expectedSize);

                }


                digest.ComputeHash(blockBuffer, 0, check);

                cur_position += inBlockBufferRead;
            } while (cur_position < target_size && (inBlockBufferRead == bufferSize));
            fs.Close();

            
            return digest.ComputeHash(blockBuffer, 0, check);
        }
    }
}
