using CNUS_packer.utils;

namespace CNUS_packer.crypto
{
    public class Key
    {
        private const int LENGTH = 0x10;
        public readonly byte[] key = new byte[LENGTH];

        public Key(byte[] key)
        {
            if (key != null && key.Length == LENGTH)
            {
                this.key = key;
            }
        }

        public Key(string s) : this(Utils.HexStringToByteArray(s))
        {
        }

        public override string ToString()
        {
            return Utils.ByteArrayToHexString(key);
        }
    }
}
