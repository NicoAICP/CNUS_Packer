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

        public void EncryptFileWithPadding(FST fst, string output_filename, short contentID, int blockSize)
        {
            using FileStream output = new FileStream(output_filename, FileMode.Create);

            MemoryStream input = new MemoryStream(fst.GetAsData());
            BigEndianMemoryStream ivStream = new BigEndianMemoryStream(0x10);
            ivStream.WriteBigEndian(contentID);
            IV iv = new IV(ivStream.GetBuffer());

            EncryptSingleFile(input, output, fst.GetDataSize(), iv, blockSize);
        }

        public void EncryptFileWithPadding(FileInfo file, Content content, string output_filename, int blockSize)
        {
            using FileStream input = file.Open(FileMode.Open);
            using FileStream output = new FileStream(output_filename, FileMode.Create);

            BigEndianMemoryStream ivStream = new BigEndianMemoryStream(0x10);
            ivStream.WriteBigEndian((short)content.ID);
            IV iv = new IV(ivStream.GetBuffer());

            EncryptSingleFile(input, output, file.Length, iv, blockSize);
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
            using FileStream ins = inputFile.Open(FileMode.Open);
            using FileStream outs = File.Open(outputFilename, FileMode.Create);

            EncryptFileHashed(ins, outs, inputFile.Length, content, hashes);
            content.encryptedFileSize = outs.Length;
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
                    Console.Write($"\rEncryption: {(int)(100.0 * block * hashBlockSize / length)}%");
                }
            } while (read == hashBlockSize);
            Console.WriteLine("\rEncryption: 100%");
        }

        private byte[] EncryptChunkHashed(byte[] buffer, int block, ContentHashes hashes, int contentID)
        {
            BigEndianMemoryStream ivStream = new BigEndianMemoryStream(16);
            ivStream.WriteBigEndian((short) contentID);
            aes.IV = ivStream.GetBuffer();
            byte[] decryptedHashes = hashes.GetHashForBlock(block);
            decryptedHashes[1] ^= (byte)contentID;

            byte[] encryptedhashes = Encrypt(decryptedHashes);
            decryptedHashes[1] ^= (byte)contentID;
            int iv_start = (block % 16) * 20;

            aes.IV = Utils.CopyOfRange(decryptedHashes, iv_start, iv_start + 16);

            byte[] encryptedContent = Encrypt(buffer);
            MemoryStream outputStream = new MemoryStream(0x10000);
            outputStream.Write(encryptedhashes);
            outputStream.Write(encryptedContent);

            return outputStream.GetBuffer();
        }

        public byte[] Encrypt(byte[] input)
        {
            return aes.CreateEncryptor().TransformFinalBlock(input, 0, input.Length);
        }
    }
}
