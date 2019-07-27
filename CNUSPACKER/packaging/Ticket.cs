using CNUS_packer.crypto;
using CNUS_packer.utils;

using System;
using System.IO;

namespace CNUS_packer.packaging
{
    public class Ticket
    {
        private long titleID;
        private Key decryptedKey = new Key();
        private Key encryptWith = new Key();

        public Ticket(long titleID, Key decryptedKey, Key encryptWith)
        {
            setTitleID(titleID);
            setDecryptedKey(decryptedKey);
            setEncryptWith(encryptWith);
        }

        public byte[] getAsData()
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
            buffer.Write(getEncryptedKey().getKey());
            buffer.Write(Utils.HexStringToByteArray("000005"));
            randomData = new byte[0x06];
            rdm.NextBytes(randomData);
            buffer.Write(randomData);
            buffer.Seek(0x04, SeekOrigin.Current);
            byte[] temp = BitConverter.GetBytes(getTitleID());
            Array.Reverse(temp);
            buffer.Write(temp);
            buffer.Write(Utils.HexStringToByteArray("00000011000000000000000000000005"));
            buffer.Seek(0xB0, SeekOrigin.Current);
            buffer.Write(Utils.HexStringToByteArray("00010014000000AC000000140001001400000000000000280000000100000084000000840003000000000000FFFFFF01"));
            buffer.Seek(0x7C, SeekOrigin.Current);

            return buffer.GetBuffer();
        }

        public Key getEncryptedKey()
        {
            MemoryStream iv_buffer = new MemoryStream(0x10);
            byte[] temp = BitConverter.GetBytes(getTitleID());
            Array.Reverse(temp);
            iv_buffer.Write(temp);
            Encryption encrypt = new Encryption(getEncryptWith(), new IV(iv_buffer.GetBuffer()));

            return new Key(encrypt.encrypt(getDecryptedKey().getKey()));
        }

        public long getTitleID()
        {
            return titleID;
        }

        public void setTitleID(long titleID)
        {
            this.titleID = titleID;
        }

        public Key getDecryptedKey()
        {
            return decryptedKey;
        }

        public void setDecryptedKey(Key decryptedKey)
        {
            this.decryptedKey = decryptedKey;
        }

        public Key getEncryptWith()
        {
            return encryptWith;
        }

        public void setEncryptWith(Key encryptWith)
        {
            this.encryptWith = encryptWith;
        }
    }
}
