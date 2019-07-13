using System;
using System.Collections.Generic;
using System.Text;
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
                    deleteDir(dirpath);
                }
            }
            System.IO.File.Delete(dir);
        }
        public static long align(long input, int alignment)
        {
            long newSize = (input / alignment);
            if( newSize * alignment != input)
            {
                newSize++;
            }
            newSize = newSize * alignment;
            return newSize;
        }
        public static byte[] HexStringToByteArray(string s)
        {
            int len = s.Length;
            byte[] data = new byte[len / 2];
            for (int i = 0; i < len; i += 2)
            {
                data[i / 2] = (byte)(Convert.ToInt32(s[i].ToString(), 16) << 4 + Convert.ToInt32(s[i + 1].ToString(), 16));

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
                return 0L;
            }
        }
        public static string ByteArraytoString(byte[] ba)
        {
            if (ba == null) return null;
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach(byte b in ba)
            {
                hex.Append(string.Format("%02X", b));
            }
            return hex.ToString();
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
        public static int getChunkFromStream(FileStream fs, byte[] output, ByteArrayBuffer overflowbuffer, long expectedSize)
        {
            int bytesRead = -1;
            int inBlockBuffer = 0;
            do
            {
                bytesRead = fs.Read(overflowbuffer.buffer, overflowbuffer.getLengthOfDataInBuffer(), overflowbuffer.getSpaceLeft());

                if (bytesRead <= 0) break;
                overflowbuffer.addLengthOfDataInBuffer(bytesRead);
                if(inBlockBuffer + overflowbuffer.getLengthOfDataInBuffer() > expectedSize)
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
            if(s != null)
            {
                System.Console.WriteLine(s);

            }
           
            FileStream fs = System.IO.File.Open(path, FileMode.OpenOrCreate);
            
            long written = 0;
            long filesize = fs.Length;
            int buffer_size = 0x10000;
            byte[] buffer = new byte[buffer_size];
            long cycle = 0;
            do
            {
                
                int read = fs.Read(buffer);
                if (read <= 0) break;
                output.Write(buffer, 0, read);
                written += read;
                if ((cycle % 10) == 0 && s != null)
                {
                    int progress = (int)((written * 1.0 / filesize * 1.0) * 100);
                    System.Console.WriteLine("\r" + s + " : " + progress + "%");

                }

            } while (written < filesize);
            if(s != null)
            {
                System.Console.WriteLine("\r" + output + ": 100%");
            }
            fs.Close();
            return written;
        }
}
}
