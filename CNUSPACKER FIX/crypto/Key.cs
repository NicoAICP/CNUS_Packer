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
            System.Console.WriteLine("was called for string " + s);
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

        public override string ToString()
        {
            return System.Text.Encoding.Default.GetString(key);
        }
    }
}
