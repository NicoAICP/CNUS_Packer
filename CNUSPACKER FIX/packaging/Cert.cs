
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CNUS_packer.packaging
{
    public class Cert
    {
        public static byte[] getCertAsData()
        {
            MemoryStream ms = new MemoryStream(0xA00);

            BinaryWriter buffer = new BinaryWriter(ms);
            buffer.Write(0x010003);
            buffer.Write(0x010004);
            buffer.Write(0x010004);

            buffer.Seek(0x240, SeekOrigin.Begin);
            buffer.Write(utils.utils.HexStringToByteArray("526F6F74000000000000000000000000"));
            buffer.Seek(0x280, SeekOrigin.Begin);
            buffer.Write(utils.utils.HexStringToByteArray("00000001434130303030303030330000"));


            buffer.Seek(0x540, SeekOrigin.Begin);
            buffer.Write(utils.utils.HexStringToByteArray("526F6F742D4341303030303030303300"));
            buffer.Seek(0x580, SeekOrigin.Begin);
            buffer.Write(utils.utils.HexStringToByteArray("00000001435030303030303030620000"));

            buffer.Seek(0x840, SeekOrigin.Begin);
            buffer.Write(utils.utils.HexStringToByteArray("526F6F742D4341303030303030303300"));
            buffer.Seek(0x880, SeekOrigin.Begin);
            buffer.Write(utils.utils.HexStringToByteArray("00000001585330303030303030630000"));


            return ms.ToArray();
        }
       /* public static byte[] getTMDCertAsData()
        {
            ByteBuffer buffer = ByteBuffer.allocate(0x700);
            buffer.putInt(0x000, 0x010004);
            buffer.putInt(0x300, 0x010003);

            buffer.position(0x140);
            buffer.put(utils.utils.HexStringToByteArray("526F6F742D4341303030303030303300"));
            buffer.position(0x180);
            buffer.put(utils.utils.HexStringToByteArray("00000001435030303030303030620000"));


            buffer.position(0x540);
            buffer.put(utils.utils.HexStringToByteArray("526F6F74000000000000000000000000"));
            buffer.position(0x580);
            buffer.put(utils.utils.HexStringToByteArray("00000001434130303030303030330000"));


            return buffer.array();
        }*/
    }
}
