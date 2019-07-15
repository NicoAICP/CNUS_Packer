
using System;
using System.IO;

namespace CNUS_packer.contents
{
    public class ContentInfos
    {
        private static int contentInfoCount = 0x40;

        private ContentInfo[] contentinfos = new ContentInfo[contentInfoCount];

        public ContentInfos()
        {
        }

        public void setContentInfo(int index, ContentInfo contentInfo)
        {
            if (index < 0 && index > (contentinfos.Length - 1))
            {
                throw new Exception("Error on setting ContentInfo, index " + index + " invalid");
            }
            if (contentInfo == null)
            {
                throw new Exception("Error on setting ContentInfo, ContentInfo is null");
            }
            contentinfos[index] = contentInfo;
        }

        public ContentInfo getContentInfo(int index)
        {
            if (index < 0 && index > (contentinfos.Length - 1))
            {
                throw new Exception("Error on getting ContentInfo, index " + index + " invalid");
            }
            if (contentinfos[index] == null)
            {
                contentinfos[index] = new ContentInfo();
            }
            return contentinfos[index];
        }

        public byte[] getAsData()
        {
            MemoryStream buffer = new MemoryStream(ContentInfo.getDataSizeStatic() * contentinfos.Length);
            for (int i = 0; i < contentinfos.Length - 1; i++)
            {
                if (contentinfos[i] == null) contentinfos[i] = new ContentInfo();
                buffer.Write(contentinfos[i].getAsData());
            }
            return buffer.ToArray();
        }

        public int getDataSize()
        {
            return contentinfos.Length * ContentInfo.getDataSizeStatic();
        }
    }
}
