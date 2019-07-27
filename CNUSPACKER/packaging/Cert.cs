using CNUS_packer.utils;

using System.IO;

namespace CNUS_packer.packaging
{
    public class Cert
    {
        public static byte[] getCertAsData()
        {
            MemoryStream buffer = new MemoryStream(0xA00);

            buffer.Write(Utils.HexStringToByteArray("00010003"));
            buffer.Seek(0x400, SeekOrigin.Begin);
            buffer.Write(Utils.HexStringToByteArray("00010004"));
            buffer.Seek(0x700, SeekOrigin.Begin);
            buffer.Write(Utils.HexStringToByteArray("00010004"));

            buffer.Seek(0x240, SeekOrigin.Begin);
            buffer.Write(Utils.HexStringToByteArray("526F6F74000000000000000000000000"));
            buffer.Seek(0x280, SeekOrigin.Begin);
            buffer.Write(Utils.HexStringToByteArray("00000001434130303030303030330000"));


            buffer.Seek(0x540, SeekOrigin.Begin);
            buffer.Write(Utils.HexStringToByteArray("526F6F742D4341303030303030303300"));
            buffer.Seek(0x580, SeekOrigin.Begin);
            buffer.Write(Utils.HexStringToByteArray("00000001435030303030303030620000"));

            buffer.Seek(0x840, SeekOrigin.Begin);
            buffer.Write(Utils.HexStringToByteArray("526F6F742D4341303030303030303300"));
            buffer.Seek(0x880, SeekOrigin.Begin);
            buffer.Write(Utils.HexStringToByteArray("00000001585330303030303030630000"));

            return buffer.GetBuffer();
        }
    }
}
