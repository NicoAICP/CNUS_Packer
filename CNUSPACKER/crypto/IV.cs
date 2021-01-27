namespace CNUSPACKER.crypto
{
    public class IV
    {
        private const int LENGTH = 0x10;
        public readonly byte[] iv = new byte[LENGTH];

        public IV(byte[] iv)
        {
            if (iv?.Length == LENGTH)
            {
                this.iv = iv;
            }
        }
    }
}
