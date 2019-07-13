using CNUS_packer.crypto;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
            MemoryStream ms = new MemoryStream(0x350);
            BinaryWriter buffer = new BinaryWriter(ms);
            buffer.Write(utils.utils.HexStringToByteArray("00010004"));
            byte[] randomData = new byte[0x100];
            rdm.NextBytes(randomData);
            ms.Write(randomData);
            ms.Write(new byte[0x3C]);
            ms.Write(utils.utils.HexStringToByteArray("526F6F742D434130303030303030332D58533030303030303063000000000000"));
            ms.Write(new byte[0x5C]);
            ms.Write(utils.utils.HexStringToByteArray("010000"));
            ms.Write(getEncryptedKey().getKey());
            ms.Write(utils.utils.HexStringToByteArray("000005"));
            randomData = new byte[0x06];
            rdm.NextBytes(randomData);
            ms.Write(randomData);
            ms.Write(new byte[0x04]);
            buffer.Write(getTitleID());
            ms.Write(utils.utils.HexStringToByteArray("00000011000000000000000000000005"));
            ms.Write(new byte[0xB0]);
            ms.Write(utils.utils.HexStringToByteArray("00010014000000AC000000140001001400000000000000280000000100000084000000840003000000000000FFFFFF01"));
            ms.Write(new byte[0x7C]);
            return ms.ToArray();
        }
        public Key getEncryptedKey()
        {
            MemoryStream ms = new MemoryStream(0x10);
            BinaryWriter iv = new BinaryWriter(ms);
            iv.Write(getTitleID());
            Encryption encrypt = new Encryption(getEncryptWith(), new IV(ms.ToArray()));
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
