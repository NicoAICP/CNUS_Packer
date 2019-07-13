using System;
using System.Collections.Generic;
using System.Text;

namespace CNUS_packer.utils
{
    public class ByteArrayBuffer
    {
        public byte[] buffer;
        int lengthOfDataInBuffer;
        public ByteArrayBuffer(int length)
        {
            buffer = new byte[(int)length];
        }
        public int getLengthOfDataInBuffer()
        {
            return lengthOfDataInBuffer;
        }
        public void setLengthOfDataInBuffer(int lengthOfDataInBuffer)
        {
            this.lengthOfDataInBuffer = lengthOfDataInBuffer;
        }

        public int getSpaceLeft()
        {
            return buffer.Length - getLengthOfDataInBuffer();
        }

        public void addLengthOfDataInBuffer(int bytesRead)
        {
            lengthOfDataInBuffer += bytesRead;
        }

        public void resetLengthOfDataInBuffer()
        {
            setLengthOfDataInBuffer(0);
        }
    }
}
