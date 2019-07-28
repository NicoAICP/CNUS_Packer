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
                Array.Reverse(temp); // we need to write in big endian
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
                Array.Reverse(temp); // we need to write in big endian
                ivstrm.Write(temp);
                IV iv = new IV(ivstrm.GetBuffer());

                EncryptSingleFile(input, output, file.Length, iv, BLOCKSIZE);
            }
        }

        private void EncryptSingleFile(Stream input, FileStream output, long inputLength, IV iv, int blockSize)
        {
            aes.IV = iv.iv;
            long targetSize = Utils.Align(inputLength, blockSize);

            int cur_position = 0;
            do
            {
                byte[] blockBuffer = new byte[blockSize];

                input.Read(blockBuffer, 0, blockSize);
                blockBuffer = Encrypt(blockBuffer);

                aes.IV = Utils.CopyOfRange(blockBuffer, blockSize - 16, blockSize);

                cur_position += blockSize;
                output.Write(blockBuffer);
            } while (cur_position < targetSize);
        }

        public void EncryptFileHashed(FileInfo inputFile, Content content, string outputFilename, ContentHashes hashes)
        {
            using (FileStream ins = inputFile.Open(FileMode.Open))
            using (FileStream outs = File.Open(outputFilename, FileMode.OpenOrCreate))
            {
                EncryptFileHashed(ins, outs, inputFile.Length, content, hashes);
                content.encryptedFileSize = outs.Length;
            }
        }

        private void EncryptFileHashed(FileStream input, FileStream output, long length, Content content, ContentHashes hashes)
        {
            const int hashBlockSize = 0xFC00;

            byte[] buffer = new byte[hashBlockSize];
            int read;
            int block = 0;
            do
            {
                read = input.Read(buffer, 0, hashBlockSize);

                output.Write(EncryptChunkHashed(buffer, block, hashes, content.ID));

                block++;
                if (block % 100 == 0)
                {
                    Console.WriteLine("\rEncryption: " + 100 * block * hashBlockSize / length + "%");
                }
            } while (read == hashBlockSize);
            Console.WriteLine("\rEncryption: 100%");
        }

        private byte[] EncryptChunkHashed(byte[] buffer, int block, ContentHashes hashes, int content_id)
        {
            MemoryStream iv_buffer = new MemoryStream(16);
            byte[] temp = BitConverter.GetBytes((short)content_id);
            Array.Reverse(temp); // we need to write in big endian
            iv_buffer.Write(temp);
            aes.IV = iv_buffer.GetBuffer();
            byte[] decryptedHashes = hashes.GetHashForBlock(block);
            decryptedHashes[1] ^= (byte)content_id;

            byte[] encryptedhashes = EncryptChunk(decryptedHashes, 0x0400);
            decryptedHashes[1] ^= (byte)content_id;
            int iv_start = (block % 16) * 20;

            aes.IV = Utils.CopyOfRange(decryptedHashes, iv_start, iv_start + 16);

            byte[] encryptedContent = EncryptChunk(buffer, 0xFC00);
            MemoryStream output_stream = new MemoryStream(0x10000);
            output_stream.Write(encryptedhashes);
            output_stream.Write(encryptedContent);

            return output_stream.GetBuffer();
        }

        private byte[] EncryptChunk(byte[] blockBuffer, int BLOCKSIZE)
        {
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
