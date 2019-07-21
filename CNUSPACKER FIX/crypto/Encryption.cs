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
            Console.WriteLine("key passed right here: " + key);
            try
            {
                cry.Mode = CipherMode.CBC;
                cry.Padding = PaddingMode.None;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
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
                Console.WriteLine(e.Message);
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
                byte[] temp = BitConverter.GetBytes(content.getID());
                Array.Reverse(temp); // we need to write in big-endian
                ivstrm.Write(temp);
                IV iv = new IV(ivstrm.GetBuffer());

                encryptSingleFile(input, output, file.Length, iv, BLOCKSIZE);
            }
        }

        public void encryptSingleFile(Stream input, FileStream output, long length, IV iv, int BLOCKSIZE)
        {
            long inputSize = length;
            long targetSize = utils.utils.align(inputSize, BLOCKSIZE);

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
                    Console.WriteLine("was 1");
                    long expectedSize = (inputSize - cur_position);
                    MemoryStream buffer = new MemoryStream(BLOCKSIZE);
                    inBlockBufferRead = utils.utils.getChunkFromStream(input, blockBuffer, overflow, expectedSize);
                    Console.WriteLine("inBlockBufferRead here is: " + inBlockBufferRead);

                    buffer.Write(copyOfRange(blockBuffer, 0, inBlockBufferRead));
                    blockBuffer = buffer.GetBuffer();
                    inBlockBufferRead = BLOCKSIZE;
                }
                else
                {
                    Console.WriteLine("was 2");
                    inBlockBufferRead = utils.utils.getChunkFromStream(input, blockBuffer, overflow, BLOCKSIZE);
                }

                byte[] output_byte = encryptChunk(blockBuffer, inBlockBufferRead, iv);

                Console.WriteLine("output length: " + output_byte.Length);

                setIV(new IV(copyOfRange(output_byte, BLOCKSIZE - 16, BLOCKSIZE)));

                cur_position += inBlockBufferRead;

                output.Write(output_byte, 0, inBlockBufferRead);

            } while (cur_position < targetSize && (inBlockBufferRead == BLOCKSIZE));
        }

        public void encryptFileHashed(FileInfo file, Content content, string output_filename, ContentHashes hashes)
        {
            Console.WriteLine("file: " + file);

            using (FileStream ins = file.Open(FileMode.Open))
            using (FileStream outs = File.Open(output_filename, FileMode.OpenOrCreate))
            {
                encryptFileHashed(ins, outs, file.Length, content, hashes);
                content.setEncryptedFileSize(outs.Length);
            }
        }

        private void encryptFileHashed(FileStream input, FileStream output, long length, Content content, ContentHashes hashes)
        {
            Console.WriteLine("File length: " + length);
            int BLOCKSIZE = 0x10000;
            int HASHBLOCKSIZE = 0xFC00;

            int buffer_size = HASHBLOCKSIZE;
            byte[] buffer = new byte[buffer_size];
            ByteArrayBuffer overflowbuffer = new ByteArrayBuffer(buffer_size);
            int read;
            int block = 0;
            do
            {
                read = utils.utils.getChunkFromStream(input, buffer, overflowbuffer, buffer_size);
                if (read != buffer_size) // TODO: probably does literally nothing, re-check later
                {
                    MemoryStream new_buffer = new MemoryStream(buffer_size);
                    new_buffer.Write(buffer);
                    buffer = new_buffer.ToArray();
                }

                byte[] output_byte_arr = encryptChunkHashed(buffer, block, hashes, content.getID());
                if (output_byte_arr.Length != BLOCKSIZE) Console.Write("");
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

        private byte[] copyOfRange(byte[] src, int start, int end)
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

        private byte[] encryptChunkHashed(byte[] buffer, int block, ContentHashes hashes, int content_id)
        {
            MemoryStream ivstrm = new MemoryStream(16);
            byte[] temp = BitConverter.GetBytes((short)content_id);
            Array.Reverse(temp); // we need to write big-endian
            ivstrm.Write(temp);
            IV iv = new IV(ivstrm.GetBuffer());
            byte[] decryptedHashes = null;
            try
            {
                decryptedHashes = hashes.getHashForBlock(block);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            decryptedHashes[1] ^= (byte)content_id;

            byte[] encryptedhashes = encryptChunk(decryptedHashes, 0x0400, iv);
            decryptedHashes[1] ^= (byte)content_id;
            int iv_start = (block % 16) * 20;

            iv = new IV(copyOfRange(decryptedHashes, iv_start, iv_start + 16));

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
            // Console.WriteLine("input array that is to be transformed: " + System.Text.Encoding.Default.GetString(input));
            try
            {
                return cry.CreateEncryptor().TransformFinalBlock(input, offset, len);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Environment.Exit(2);
            }

            return input;
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
