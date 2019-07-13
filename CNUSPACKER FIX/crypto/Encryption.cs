using CNUS_packer.contents;
using CNUS_packer.packaging;
using CNUS_packer.utils;

using System;

using System.Collections.Generic;
using System.IO;
using System.Text;
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
            try
            {
                cry.Mode = CipherMode.CBC;
                cry.Padding = PaddingMode.None;
            }
            catch(Exception e)
            {
                System.Console.WriteLine(e.Message);
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
            FileStream input = System.IO.File.Create("test");
            input.Flush();
            
            MemoryStream memoryStream = new MemoryStream(fst.getAsData());
            memoryStream.WriteTo(input);
            FileStream output = new FileStream(output_filename, FileMode.Create);
            MemoryStream ivstrm = new MemoryStream(0x10);
            BinaryWriter ivbin = new BinaryWriter(ivstrm);
            ivbin.Write(contentID);
            
        IV iv = new IV(ivstrm.ToArray());
        encryptSingleFile(input, output, fst.getDataSize(), iv, BLOCKSIZE);
    }
        public void encryptFileWithPadding(string file, Content content, string output_filename, int BLOCKSIZE) 
        {
            FileInfo f1 = new FileInfo(file);
            FileStream input = System.IO.File.Open(file, FileMode.Open);
        FileStream output = new FileStream(output_filename, FileMode.Create);
            MemoryStream ivstrm = new MemoryStream(0x10);
            BinaryWriter ivbin = new BinaryWriter(ivstrm);
            ivbin.Write(content.getID());
            IV iv = new IV(ivstrm.ToArray()) ;

        encryptSingleFile(input, output, f1.Length, iv, BLOCKSIZE);
    }
        public void encryptSingleFile(FileStream input, FileStream output, long length, IV iv, int BLOCKSIZE)
        {

            long inputSize = length;
            long targetSIze = utils.utils.align(inputSize, BLOCKSIZE);

            byte[] blockBuffer = new byte[BLOCKSIZE];
            int inBlockBufferRead;

            long cur_position = 0;
            ByteArrayBuffer overflow = new ByteArrayBuffer(BLOCKSIZE);

            bool first = true;
            do
            {
                int size = 0;
                if (first)
                {
                    first = false;
                }
                else
                {
                    iv = null;
                }

                if ((cur_position + BLOCKSIZE) > inputSize)
                {

                    long expectedSize = (inputSize - cur_position);
                    size = (int)expectedSize;
                   MemoryStream buffer = new MemoryStream(BLOCKSIZE);
                    inBlockBufferRead = utils.utils.getChunkFromStream(input, blockBuffer, overflow, expectedSize);

                    buffer.Write(copyOfRange(blockBuffer, 0, BLOCKSIZE));

                    blockBuffer = buffer.ToArray();
                    inBlockBufferRead = BLOCKSIZE;
                }
                else
                {
                    int expectedSize = BLOCKSIZE;
                    inBlockBufferRead = utils.utils.getChunkFromStream(input, blockBuffer, overflow, expectedSize);


                }

                byte[] output_byte = encryptChunk(blockBuffer, inBlockBufferRead, iv);

                setIV(new IV(copyOfRange(output_byte, output_byte.Length - 16, 32768)));

                cur_position += inBlockBufferRead;

                output.Write(output_byte, 0, size);


            } while (cur_position < targetSIze && (inBlockBufferRead == BLOCKSIZE));
        }
       
        public void encryptFileHashed(string file, Content content, string output_filename, ContentHashes hashes)
        {
            string input = file;
            string output = output_filename;
            FileStream ins = System.IO.File.Open(file, System.IO.FileMode.OpenOrCreate);
            FileStream outs = new FileStream(output, FileMode.OpenOrCreate);
            encryptFileHashed(ins, outs, input.Length, content, hashes);
            FileInfo fi = new FileInfo(output);
            content.setEncryptedFileSize((int)fi.Length);

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
            int totalblock = (int)(length / HASHBLOCKSIZE);
            do
            {
                read = utils.utils.getChunkFromStream(input, buffer, overflowbuffer, buffer_size);
                if (read != buffer_size)
                {
                    MemoryStream new_buffer = new MemoryStream(buffer_size);
                    new_buffer.Write(buffer);
                    buffer = new_buffer.ToArray();
                }
                byte[] output_byte_arr = encryptChunkHashed(buffer, block, hashes, content.getID());
                if (output_byte_arr.Length != BLOCKSIZE) System.Console.Write("");
                output.Write(output_byte_arr);
                block++;
                int progress = (int)((block * 1.0 / totalblock * 1.0) * 100);
                if ((block % 100) == 0)
                {
                    System.Console.WriteLine("\rEncryption: " + progress + "%");

                }


            } while (read == buffer_size);
            System.Console.WriteLine("\rEncryption: 100%");
            input.Close();
            output.Close();
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
            BinaryWriter ivbin = new BinaryWriter(ivstrm);
            ivbin.Write((short)content_id);
            IV iv = new IV(ivstrm.ToArray());
            byte[] decryptedHashes = null;
            try
            {
                decryptedHashes = hashes.getHashForBlock(block);

            }catch(Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
            decryptedHashes[1] ^= (byte)content_id;

            byte[] encryptedhashes = encryptChunk(decryptedHashes, 0x0400, iv);
            decryptedHashes[1] ^= (byte)content_id;
            int iv_start = (block % 16) * 20;

            iv = new IV(copyOfRange(decryptedHashes, iv_start, iv_start + 16));

            byte[] encryptedContent = encryptChunk(buffer, 0xFC00, iv);
            MemoryStream output_bb = new MemoryStream(0x10000);
            output_bb.Write(encryptedhashes);
            output_bb.Write(encryptedContent);
            return output_bb.ToArray();
            
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
            try
            {


                return cry.CreateEncryptor().TransformFinalBlock(input, offset, input.Length);
                
            }
            catch (Exception e) {
                System.Console.WriteLine(e.Message);
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
