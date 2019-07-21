using System;
using System.IO;
using System.Numerics;

namespace CNUS_packer.utils
{
    public class utils
    {
        public static void deleteDir(string dir)
        {
            string[] contents = Directory.GetFiles(dir);
            if(contents != null)
            {
                foreach(string dirpath in contents)
                {
                    if (Directory.Exists(dirpath))
                    {
                        deleteDir(dirpath);
                    }
                    else
                    {
                        File.Delete(dirpath);
                    }
                }
            }
            Directory.Delete(dir);
        }

        public static long align(long input, int alignment)
        {
            long newSize = (input / alignment);
            if (newSize * alignment != input)
            {
                newSize++;
            }
            newSize *= alignment;

            return newSize;
        }

        public static byte[] HexStringToByteArray(string s)
        {
            int len = s.Length;
            byte[] data = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                data[i / 2] = Convert.ToByte(s.Substring(i, 2), 16);
            }

            return data;
        }

        public static long HexStringToLong(string s)
        {
            try
            {
                BigInteger bi = BigInteger.Parse(s, System.Globalization.NumberStyles.HexNumber);
                return (long)bi;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return 0L;
            }
        }

        /*public static int getChunkFromStream(InputStream inputStream, byte[] output, ByteArrayBuffer overflowbuffer, long expectedSize)
        {
            int bytesRead = -1;
            int inBlockBuffer = 0;
            do
            {
                bytesRead = inputStream.read(overflowbuffer.buffer, overflowbuffer.getLengthOfDataInBuffer(), overflowbuffer.getSpaceLeft());
                if (bytesRead <= 0) break;
                overflowbuffer.addLengthOfDataInBuffer(bytesRead);
                if (inBlockBuffer + overflowbuffer.getLengthOfDataInBuffer() > expectedSize)
                {
                    long tooMuch = (inBlockBuffer + bytesRead) - expectedSize;
                    long toRead = expectedSize - inBlockBuffer;
                    Array.Copy(overflowbuffer.buffer, 0, output, inBlockBuffer, (int)toRead);
                    inBlockBuffer += (int)toRead;
                    Array.Copy(overflowbuffer.buffer, (int)toRead, overflowbuffer.buffer, 0, (int)tooMuch);
                    overflowbuffer.setLengthOfDataInBuffer((int)tooMuch);
                }
                else
                {
                    Array.Copy(overflowbuffer.buffer, 0, output, inBlockBuffer, overflowbuffer.getLengthOfDataInBuffer());
                    inBlockBuffer += overflowbuffer.getLengthOfDataInBuffer();
                    overflowbuffer.resetLengthOfDataInBuffer();

                }
            } while (inBlockBuffer != expectedSize);
            return inBlockBuffer;
        }*/

        public static int getChunkFromStream(Stream s, byte[] output, ByteArrayBuffer overflowbuffer, long expectedSize)
        {
            int inBlockBuffer = 0;
            do
            {
                Console.WriteLine("buf, lenofdata, spaceleft: " + overflowbuffer.buffer + ", " + overflowbuffer.getLengthOfDataInBuffer() + ", " + overflowbuffer.getSpaceLeft());
                int bytesRead = s.Read(overflowbuffer.buffer, overflowbuffer.getLengthOfDataInBuffer(), overflowbuffer.getSpaceLeft());
                Console.WriteLine("inBlockBuffer: " + inBlockBuffer);
                if (bytesRead <= 0) break;

                overflowbuffer.addLengthOfDataInBuffer(bytesRead);
                if (inBlockBuffer + overflowbuffer.getLengthOfDataInBuffer() > expectedSize)
                {
                    long tooMuch = (inBlockBuffer + bytesRead) - expectedSize;
                    long toRead = expectedSize - inBlockBuffer;
                    Array.Copy(overflowbuffer.buffer, 0, output, inBlockBuffer, (int)toRead);
                    inBlockBuffer += (int)toRead;
                    Array.Copy(overflowbuffer.buffer, (int)toRead, overflowbuffer.buffer, 0, (int)tooMuch);
                    overflowbuffer.setLengthOfDataInBuffer((int)tooMuch);
                }
                else
                {
                    Array.Copy(overflowbuffer.buffer, 0, output, inBlockBuffer, overflowbuffer.getLengthOfDataInBuffer());
                    inBlockBuffer += overflowbuffer.getLengthOfDataInBuffer();
                    overflowbuffer.resetLengthOfDataInBuffer();

                }
            } while (inBlockBuffer != expectedSize);

            return inBlockBuffer;
        }

        public static long copyFileInto(string path, FileStream output)
        {
            return copyFileInto(path, output, null);
        }

        public static long copyFileInto(string path, FileStream output, string s)
        {
            if (s != null)
            {
                Console.WriteLine(s);
            }

            long written = 0;
            using (FileStream fs = File.Open(path, FileMode.Open))
            {
                long filesize = fs.Length;
                int buffer_size = 0x10000;
                byte[] buffer = new byte[buffer_size];
                do
                {
                    int read = fs.Read(buffer);
                    if (read <= 0) break;
                    output.Write(buffer, 0, read);
                    written += read;
                    if (s != null)
                    {
                        int progress = (int)(100 * written / filesize);
                        Console.WriteLine("\r" + s + " : " + progress + "%");
                    }

                } while (written < filesize);

                if (s != null)
                {
                    Console.WriteLine("\r" + output + ": 100%");
                }
            }

            return written;
        }
    }
}
