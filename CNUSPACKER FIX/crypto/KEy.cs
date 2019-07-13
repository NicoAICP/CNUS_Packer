using System;
using System.Collections.Generic;
using System.Text;

namespace CNUS_packer.crypto
{
    public class Key
    {
        private static int LENGTH = 0x10;
        private byte[] key = new byte[LENGTH];

        public Key()
        {
        }

        public Key(byte[] key)
        {
            setKey(key);
        }

        public Key(string s) : this(utils.utils.HexStringToByteArray(s))
        {
            
        }

        public byte[] getKey()
        {
            return key;
        }

        public void setKey(byte[] key)
        {
            if (key != null && key.Length == getKey().Length)
            {
                this.key = key;
            }
        }

        public int getLength()
        {
            return LENGTH;
        }

       
    public string toString()
        {
            return utils.utils.ByteArraytoString(key);
        }
    }
}
