using System;
using System.IO;
using CNUSPACKER.crypto;
using CNUSPACKER.utils;

namespace CNUSPACKER.packaging
{
    public class Ticket
    {
        public long titleID { get; }
        public Key decryptedKey { get; }
        public Key encryptWith { get; }

        public Ticket(long titleID, Key decryptedKey, Key encryptWith)
        {
            this.titleID = titleID;
            this.decryptedKey = decryptedKey;
            this.encryptWith = encryptWith;
        }

        public byte[] GetAsData()
        {
            Random rdm = new Random();
            MemoryStream buffer = new MemoryStream(0x350);
            buffer.Write(Utils.HexStringToByteArray("00010004"));
            byte[] randomData = new byte[0x100];
            rdm.NextBytes(randomData);
            buffer.Write(randomData);
            buffer.Seek(0x3C, SeekOrigin.Current);
            buffer.Write(Utils.HexStringToByteArray("526F6F742D434130303030303030332D58533030303030303063000000000000"));
            buffer.Seek(0x5C, SeekOrigin.Current);
            buffer.Write(Utils.HexStringToByteArray("010000"));
            buffer.Write(GetEncryptedKey().key);
            buffer.Write(Utils.HexStringToByteArray("000005"));
            randomData = new byte[0x06];
            rdm.NextBytes(randomData);
            buffer.Write(randomData);
            buffer.Seek(0x04, SeekOrigin.Current);
            byte[] temp = BitConverter.GetBytes(titleID);
            Array.Reverse(temp);
            buffer.Write(temp);
            buffer.Write(Utils.HexStringToByteArray("00000011000000000000000000000005"));
            buffer.Seek(0xB0, SeekOrigin.Current);
            buffer.Write(Utils.HexStringToByteArray("00010014000000AC000000140001001400000000000000280000000100000084000000840003000000000000FFFFFF01"));
            buffer.Seek(0x7C, SeekOrigin.Current);

            return buffer.GetBuffer();
        }

        public Key GetEncryptedKey()
        {
            MemoryStream iv_buffer = new MemoryStream(0x10);
            byte[] temp = BitConverter.GetBytes(titleID);
            Array.Reverse(temp);
            iv_buffer.Write(temp);
            Encryption encrypt = new Encryption(encryptWith, new IV(iv_buffer.GetBuffer()));

            return new Key(encrypt.Encrypt(decryptedKey.key));
        }
    }
}
