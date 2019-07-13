using System;
using System.Collections.Generic;
using System.Text;

namespace CNUS_packer.crypto
{
    public class IV
    {
        private static int LENGTH = 0x10;
        private byte[] Iv = new byte[LENGTH];

        public IV()
        {
        }

        public IV(byte[] array)
        {
            setIV(array);
        }

        public byte[] getIV()
        {
            return Iv;
        }

        public void setIV(byte[] IV)
        {
            if (IV != null && IV.Length == getIV().Length)
            {
                this.Iv = IV;
            }
        }

        public int getLength()
        {
            return LENGTH;
        }
    }
}
