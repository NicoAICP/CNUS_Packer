using CNUS_packer.contents;
using CNUS_packer.packaging;
using CNUS_packer.utils;

using System;
using System.IO;
using System.Security.Cryptography;

namespace CNUS_packer.crypto
{
    public class Encryption
    {
        private Key key = new Key();
        private IV iv = new IV();

        Aes cry = Aes.Create();

        public Encryption(Key key, IV iv)
        {
            cry.Mode = CipherMode.CBC;
            cry.Padding = PaddingMode.None;
            init(key, iv);
        }

        public void init(IV iv)
        {
            init(getKey(), iv);
        }

        public void init(Key key) {
            init(key, new IV());
        }

        public void init(Key key, IV iv)
        {
            setKey(key);
            setIV(iv);

            try
            {
                cry.Key = getKey().getKey();
                cry.IV = getIV().getIV();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Environment.Exit(2);
            }
        }

        public void encryptFileWithPadding(FST fst, string output_filename, short contentID, int BLOCKSIZE)
        {
            using (FileStream output = new FileStream(output_filename, FileMode.Create))
            {
                MemoryStream input = new MemoryStream(fst.getAsData());
                MemoryStream ivstrm = new MemoryStream(0x10);
                byte[] temp = BitConverter.GetBytes(contentID);
                Array.Reverse(temp);
                ivstrm.Write(temp);
                IV iv = new IV(ivstrm.GetBuffer());

                encryptSingleFile(input, output, fst.getDataSize(), iv, BLOCKSIZE);
            }
        }

        public void encryptFileWithPadding(FileInfo file, Content content, string output_filename, int BLOCKSIZE)
        {
            using (FileStream input = file.Open(FileMode.Open))
            using (FileStream output = new FileStream(output_filename, FileMode.Create))
            {
                MemoryStream ivstrm = new MemoryStream(0x10);
                byte[] temp = BitConverter.GetBytes((short)content.ID);
                Array.Reverse(temp); // we need to write in big-endian
                ivstrm.Write(temp);
                IV iv = new IV(ivstrm.GetBuffer());

                encryptSingleFile(input, output, file.Length, iv, BLOCKSIZE);
            }
        }

        public void encryptSingleFile(Stream input, FileStream output, long length, IV iv, int BLOCKSIZE)
        {
            long inputSize = length;
            long targetSize = Utils.align(inputSize, BLOCKSIZE);

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
                    inBlockBufferRead = Utils.getChunkFromStream(input, blockBuffer, overflow, expectedSize);

                    buffer.Write(Utils.copyOfRange(blockBuffer, 0, inBlockBufferRead));
                    blockBuffer = buffer.GetBuffer();
                    inBlockBufferRead = BLOCKSIZE;
                }
                else
                {
                    inBlockBufferRead = Utils.getChunkFromStream(input, blockBuffer, overflow, BLOCKSIZE);
                }

                byte[] output_byte = encryptChunk(blockBuffer, inBlockBufferRead, iv);

                setIV(new IV(Utils.copyOfRange(output_byte, BLOCKSIZE - 16, BLOCKSIZE)));

                cur_position += inBlockBufferRead;

                output.Write(output_byte, 0, inBlockBufferRead);

            } while (cur_position < targetSize && (inBlockBufferRead == BLOCKSIZE));
        }

        public void encryptFileHashed(FileInfo file, Content content, string output_filename, ContentHashes hashes)
        {
            using (FileStream ins = file.Open(FileMode.Open))
            using (FileStream outs = File.Open(output_filename, FileMode.OpenOrCreate))
            {
                encryptFileHashed(ins, outs, file.Length, content, hashes);
                content.setEncryptedFileSize(outs.Length);
            }
        }

        private void encryptFileHashed(FileStream input, FileStream output, long length, Content content, ContentHashes hashes)
        {
            int BLOCKSIZE = 0x10000;
            int HASHBLOCKSIZE = 0xFC00;

            int buffer_size = HASHBLOCKSIZE;
            byte[] buffer = new byte[buffer_size];
            ByteArrayBuffer overflowbuffer = new ByteArrayBuffer(buffer_size);
            int read;
            int block = 0;
            do
            {
                read = Utils.getChunkFromStream(input, buffer, overflowbuffer, buffer_size);

                byte[] output_byte_arr = encryptChunkHashed(buffer, block, hashes, content.ID);
                if (output_byte_arr.Length != BLOCKSIZE) Console.WriteLine("WTF?");
                output.Write(output_byte_arr);

                block++;
                int progress = (int)(100 * block * length / HASHBLOCKSIZE);
                if ((block % 100) == 0)
                {
                    Console.WriteLine("\rEncryption: " + progress + "%");
                }
            } while (read == buffer_size);
            Console.WriteLine("\rEncryption: 100%");
        }

        private byte[] encryptChunkHashed(byte[] buffer, int block, ContentHashes hashes, int content_id)
        {
            MemoryStream ivstrm = new MemoryStream(16);
            byte[] temp = BitConverter.GetBytes((short)content_id);
            Array.Reverse(temp); // we need to write big-endian
            ivstrm.Write(temp);
            IV iv = new IV(ivstrm.GetBuffer());
            byte[] decryptedHashes = hashes.getHashForBlock(block);
            decryptedHashes[1] ^= (byte)content_id;

            byte[] encryptedhashes = encryptChunk(decryptedHashes, 0x0400, iv);
            decryptedHashes[1] ^= (byte)content_id;
            int iv_start = (block % 16) * 20;

            iv = new IV(Utils.copyOfRange(decryptedHashes, iv_start, iv_start + 16));

            byte[] encryptedContent = encryptChunk(buffer, 0xFC00, iv);
            MemoryStream output_stream = new MemoryStream(0x10000);
            output_stream.Write(encryptedhashes);
            output_stream.Write(encryptedContent);

            return output_stream.GetBuffer();
        }

        public byte[] encryptChunk(byte[] blockBuffer, int BLOCKSIZE, IV IV)
        {
            return encryptChunk(blockBuffer, 0, BLOCKSIZE, IV);
        }

        public byte[] encryptChunk(byte[] blockBuffer, int offset, int BLOCKSIZE, IV IV)
        {
            if (IV != null) setIV(IV);

            init(getIV());

            byte[] output = encrypt(blockBuffer, offset, BLOCKSIZE);

            return output;
        }

        public byte[] encrypt(byte[] input)
        {
            return encrypt(input, input.Length);
        }

        public byte[] encrypt(byte[] input, int len)
        {
            return encrypt(input, 0, len);
        }

        public byte[] encrypt(byte[] input, int offset, int len)
        {
            return cry.CreateEncryptor().TransformFinalBlock(input, offset, len);
        }

        public Key getKey()
        {
            return key;
        }

        public void setKey(Key key)
        {
            this.key = key;
        }

        public IV getIV()
        {
            return iv;
        }

        public void setIV(IV iv)
        {
            this.iv = iv;
        }
    }
}
