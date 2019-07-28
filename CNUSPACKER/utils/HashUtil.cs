using System.IO;
using System.Security.Cryptography;

namespace CNUSPACKER.utils
{
    public static class HashUtil
    {
        public static byte[] HashSHA2(byte[] data)
        {
            SHA256 sha256 = SHA256.Create();

            return sha256.ComputeHash(data);
        }

        public static byte[] HashSHA1(byte[] data)
        {
            SHA1 sha1 = SHA1.Create();

            return sha1.ComputeHash(data);
        }

        public static byte[] HashSHA1(FileInfo file, int alignment)
        {
            SHA1 sha1 = SHA1.Create();
            using (FileStream fs = file.Open(FileMode.Open))
            {
                return Hash(sha1, fs, alignment);
            }
        }

        private static byte[] Hash(SHA1 digest, FileStream input, int alignment)
        {
            long targetSize = Utils.Align(input.Length, alignment);
            byte[] alignedFileContents = new byte[targetSize];
            input.Read(alignedFileContents);

            return digest.ComputeHash(alignedFileContents);
        }
    }
}
