using System;
using System.IO;

namespace CNUSPACKER.utils
{
    public static class Utils
    {
        public static void DeleteDir(string dir)
        {
            foreach (string filepath in Directory.EnumerateFiles(dir))
                File.Delete(filepath);

            Directory.Delete(dir);
        }

        public static long Align(long input, int alignment)
        {
            long newSize = input / alignment;
            if (newSize * alignment != input)
                newSize++;

            return newSize * alignment;
        }

        public static byte[] HexStringToByteArray(string s)
        {
            int outputLength = s.Length / 2;
            byte[] output = new byte[outputLength];
            for (int i = 0; i < outputLength; i++)
            {
                output[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);
            }

            return output;
        }

        public static string ByteArrayToHexString(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            int b;
            for (int i = 0; i < bytes.Length; i++)
            {
                b = bytes[i] >> 4;
                c[i * 2] = (char)(55 + b + (((b - 10) >> 31) & -7));
                b = bytes[i] & 0xF;
                c[i * 2 + 1] = (char)(55 + b + (((b - 10) >> 31) & -7));
            }

            //System.Text.StringBuilder hex = new System.Text.StringBuilder(bytes.Length * 2); // slower
            //foreach (byte b in bytes)
            //    hex.AppendFormat("{0:X2}", b);
            //return hex.ToString();

            return new string(c);
        }

        public static byte[] CopyOfRange(byte[] src, int start, int end)
        {
            int len = end - start;
            byte[] dest = new byte[len];
            Array.Copy(src, start, dest, 0, len);

            return dest;
        }

        public static void CopyFileInto(string path, FileStream output, string s = null)
        {
            if (s != null)
                Console.Write(s);

            long written = 0;
            using (FileStream fs = File.Open(path, FileMode.Open))
            {
                long filesize = fs.Length;
                byte[] buffer = new byte[0x10000];
                do
                {
                    int read = fs.Read(buffer);
                    if (read <= 0) break;
                    output.Write(buffer, 0, read);
                    written += read;
                    if (s != null)
                    {
                        int progress = (int)(100 * written / filesize);
                        Console.Write("\r" + s + " : " + progress + "%");
                    }
                } while (written < filesize);
                Console.WriteLine();
            }
        }
    }
}
