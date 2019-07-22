using CNUS_packer.utils;

namespace CNUS_packer.crypto
{
    public class Key
    {
        private const int LENGTH = 0x10;
        private byte[] key = new byte[LENGTH];

        public Key()
        {
        }

        public Key(byte[] key)
        {
            setKey(key);
        }

        public Key(string s) : this(Utils.HexStringToByteArray(s))
        {
        }

        public byte[] getKey()
        {
            return key;
        }

        public void setKey(byte[] key)
        {
            if (key != null && key.Length == LENGTH)
            {
                this.key = key;
            }
        }

        public override string ToString()
        {
            return Utils.ByteArrayToHexString(key);
        }
    }
}
