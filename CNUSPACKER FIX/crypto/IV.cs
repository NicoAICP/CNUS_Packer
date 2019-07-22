namespace CNUS_packer.crypto
{
    public class IV
    {
        private static int LENGTH = 0x10;
        private byte[] iv = new byte[LENGTH];

        public IV()
        {
        }

        public IV(byte[] array)
        {
            setIV(array);
        }

        public byte[] getIV()
        {
            return iv;
        }

        public void setIV(byte[] iv)
        {
            if (iv != null && iv.Length == getIV().Length)
            {
                this.iv = iv;
            }
        }
    }
}
