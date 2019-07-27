using System;
using System.IO;

namespace CNUS_packer.contents
{
    public class ContentInfos
    {
        private const int contentInfoCount = 0x40;

        private readonly ContentInfo[] contentinfos = new ContentInfo[contentInfoCount];

        public void SetContentInfo(int index, ContentInfo contentInfo)
        {
            if (index < 0 || index >= contentInfoCount)
            {
                throw new Exception("Error on setting ContentInfo, index " + index + " invalid");
            }

            contentinfos[index] = contentInfo ?? throw new Exception("Error on setting ContentInfo, ContentInfo is null.");
        }

        public ContentInfo GetContentInfo(int index)
        {
            if (index < 0 || index >= contentInfoCount)
            {
                throw new Exception("Error on getting ContentInfo, index " + index + " invalid");
            }

            return contentinfos[index] ?? (contentinfos[index] = new ContentInfo());
        }

        public byte[] GetAsData()
        {
            MemoryStream buffer = new MemoryStream(ContentInfo.staticDataSize * contentInfoCount);
            for (int i = 0; i < contentInfoCount; i++)
            {
                if (contentinfos[i] == null)
                    contentinfos[i] = new ContentInfo();
                buffer.Write(contentinfos[i].GetAsData());
            }

            return buffer.GetBuffer();
        }

        public static int GetDataSize()
        {
            return contentInfoCount * ContentInfo.staticDataSize;
        }
    }
}
