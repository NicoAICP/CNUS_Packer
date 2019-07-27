using System;
using System.IO;
using System.Security.Cryptography;
using CNUSPACKER.contents;
using CNUSPACKER.packaging;
using CNUSPACKER.utils;

namespace CNUSPACKER.crypto
{
    public class Encryption
    {
        private readonly Aes aes = Aes.Create();

        public Encryption(Key key, IV iv)
        {
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            aes.Key = key.key;
            aes.IV = iv.iv;
        }

        public void EncryptFileWithPadding(FST fst, string output_filename, short contentID, int BLOCKSIZE)
        {
            using (FileStream output = new FileStream(output_filename, FileMode.Create))
            {
                MemoryStream input = new MemoryStream(fst.GetAsData());
                MemoryStream ivstrm = new MemoryStream(0x10);
                byte[] temp = BitConverter.GetBytes(contentID);
                Array.Reverse(temp);
                ivstrm.Write(temp);
                IV iv = new IV(ivstrm.GetBuffer());

                EncryptSingleFile(input, output, fst.GetDataSize(), iv, BLOCKSIZE);
            }
        }

        public void EncryptFileWithPadding(FileInfo file, Content content, string output_filename, int BLOCKSIZE)
        {
            using (FileStream input = file.Open(FileMode.Open))
            using (FileStream output = new FileStream(output_filename, FileMode.Create))
            {
                MemoryStream ivstrm = new MemoryStream(0x10);
                byte[] temp = BitConverter.GetBytes((short)content.ID);
                Array.Reverse(temp); // we need to write in big-endian
                ivstrm.Write(temp);
                IV iv = new IV(ivstrm.GetBuffer());

                EncryptSingleFile(input, output, file.Length, iv, BLOCKSIZE);
            }
        }

        private void EncryptSingleFile(Stream input, FileStream output, long length, IV iv, int BLOCKSIZE)
        {
            long inputSize = length;
            long targetSize = Utils.Align(inputSize, BLOCKSIZE);

            byte[] blockBuffer = new byte[BLOCKSIZE];
            int inBlockBufferRead;

            long cur_position = 0;
            ByteArrayBuffer overflow = new ByteArrayBuffer(BLOCKSIZE);

            bool first = true;
            do
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    iv = null;
                }

                if (cur_position + BLOCKSIZE > inputSize)
                {
                    long expectedSize = inputSize - cur_position;
                    MemoryStream buffer = new MemoryStream(BLOCKSIZE);
                    inBlockBufferRead = Utils.GetChunkFromStream(input, blockBuffer, overflow, expectedSize);

                    buffer.Write(Utils.copyOfRange(blockBuffer, 0, inBlockBufferRead));
                    blockBuffer = buffer.GetBuffer();
                    inBlockBufferRead = BLOCKSIZE;
                }
                else
                {
                    inBlockBufferRead = Utils.GetChunkFromStream(input, blockBuffer, overflow, BLOCKSIZE);
                }

                byte[] output_byte = EncryptChunk(blockBuffer, inBlockBufferRead, iv);

                aes.IV = Utils.copyOfRange(output_byte, BLOCKSIZE - 16, BLOCKSIZE);

                cur_position += inBlockBufferRead;

                output.Write(output_byte, 0, inBlockBufferRead);

            } while (cur_position < targetSize && (inBlockBufferRead == BLOCKSIZE));
        }

        public void EncryptFileHashed(FileInfo file, Content content, string output_filename, ContentHashes hashes)
        {
            using (FileStream ins = file.Open(FileMode.Open))
            using (FileStream outs = File.Open(output_filename, FileMode.OpenOrCreate))
            {
                EncryptFileHashed(ins, outs, file.Length, content, hashes);
                content.encryptedFileSize = outs.Length;
            }
        }

        private void EncryptFileHashed(FileStream input, FileStream output, long length, Content content, ContentHashes hashes)
        {
            const int BLOCKSIZE = 0x10000;
            const int HASHBLOCKSIZE = 0xFC00;

            byte[] buffer = new byte[HASHBLOCKSIZE];
            ByteArrayBuffer overflowbuffer = new ByteArrayBuffer(HASHBLOCKSIZE);
            int read;
            int block = 0;
            do
            {
                read = Utils.GetChunkFromStream(input, buffer, overflowbuffer, HASHBLOCKSIZE);

                byte[] output_byte_arr = EncryptChunkHashed(buffer, block, hashes, content.ID);
                if (output_byte_arr.Length != BLOCKSIZE)
                    Console.WriteLine("WTF?");
                output.Write(output_byte_arr);

                block++;
                int progress = (int)(100 * block * HASHBLOCKSIZE / length);
                if (block % 100 == 0)
                {
                    Console.WriteLine("\rEncryption: " + progress + "%");
                }
            } while (read == HASHBLOCKSIZE);
            Console.WriteLine("\rEncryption: 100%");
        }

        private byte[] EncryptChunkHashed(byte[] buffer, int block, ContentHashes hashes, int content_id)
        {
            MemoryStream ivstrm = new MemoryStream(16);
            byte[] temp = BitConverter.GetBytes((short)content_id);
            Array.Reverse(temp); // we need to write big-endian
            ivstrm.Write(temp);
            IV iv = new IV(ivstrm.GetBuffer());
            byte[] decryptedHashes = hashes.GetHashForBlock(block);
            decryptedHashes[1] ^= (byte)content_id;

            byte[] encryptedhashes = EncryptChunk(decryptedHashes, 0x0400, iv);
            decryptedHashes[1] ^= (byte)content_id;
            int iv_start = (block % 16) * 20;

            iv = new IV(Utils.copyOfRange(decryptedHashes, iv_start, iv_start + 16));

            byte[] encryptedContent = EncryptChunk(buffer, 0xFC00, iv);
            MemoryStream output_stream = new MemoryStream(0x10000);
            output_stream.Write(encryptedhashes);
            output_stream.Write(encryptedContent);

            return output_stream.GetBuffer();
        }

        private byte[] EncryptChunk(byte[] blockBuffer, int BLOCKSIZE, IV iv)
        {
            if (iv != null)
                aes.IV = iv.iv;

            return Encrypt(blockBuffer, 0, BLOCKSIZE);
        }

        public byte[] Encrypt(byte[] input)
        {
            return Encrypt(input, 0, input.Length);
        }

        public byte[] Encrypt(byte[] input, int offset, int len)
        {
            return aes.CreateEncryptor().TransformFinalBlock(input, offset, len);
        }
    }
}
