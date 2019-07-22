using CNUS_packer.contents;
using CNUS_packer.crypto;
using CNUS_packer.utils;

using System;
using System.IO;

namespace CNUS_packer.packaging
{
    public class TMD
    {
        private int signatureType = 0x00010004;
        private byte[] signature = new byte[0x100];
        private byte[] padding0 = new byte[0x3C];
        private static byte[] issuer = Utils.HexStringToByteArray("526F6F742D434130303030303030332D435030303030303030620000000000000000000000000000000000000000000000000000000000000000000000000000");

        private byte version = 0x01;
        private byte CACRLVersion = 0x00;
        private byte signerCRLVersion = 0x00;
        private byte padding1 = 0x00;

        private long systemVersion = 0x000500101000400AL;

        private int titleType = 0x000100;
        private short groupID = 0x0000;
        private uint appType = 0x80000000;
        private int random1 = 0;
        private int random2 = 0;
        private byte[] reserved = new byte[50];
        private int accessRights = 0x0000;
        private short titleVersion = 0x00;
        private short contentCount = 0x00;
        private short bootIndex = 0x00;
        private byte[] padding3 = new byte[2];
        public byte[] SHA2 = new byte[0x20];

        private ContentInfos contentInfos = null;
        private Contents contents = null;

        private Ticket ticket;

        public TMD(AppXMLInfo appInfo, FST fst, Ticket ticket)
        {
            setGroupID(appInfo.GetGroupID());
            setSystemVersion(appInfo.GetOsVersion());
            setAppType(appInfo.GetAppType());
            setTitleVersion(appInfo.GetTitleVersion());
            setTicket(ticket);
            setContents(fst.getContents());
            contentInfos = new ContentInfos();
        }

        private void setContents(Contents contents)
        {
            if (contents != null)
            {
                this.contents = contents;
                contentCount = contents.getContentCount();
            }
        }

        public void update()
        {
            updateContents();
        }

        public void updateContents()
        {
            this.contentCount = contents.getContentCount();

            ContentInfo firstContentInfo = new ContentInfo(contents.getContentCount());

            firstContentInfo.setSHA2Hash(HashUtil.hashSHA2(contents.getAsData()));
            getContentInfos().setContentInfo(0, firstContentInfo);
        }

        public void updateContentInfoHash()
        {
            this.SHA2 = HashUtil.hashSHA2(getContentInfos().getAsData());
        }

        public byte[] getAsData()
        {
            MemoryStream buffer = new MemoryStream(getDataSize());
            byte[] temp; // We need to write in big endian, so we're gonna Array.Reverse a lot

            temp = BitConverter.GetBytes(signatureType);
            Array.Reverse(temp);
            buffer.Write(temp);
            buffer.Write(signature);
            buffer.Write(padding0);
            buffer.Write(issuer);

            buffer.WriteByte(version);
            buffer.WriteByte(CACRLVersion);
            buffer.WriteByte(signerCRLVersion);
            buffer.WriteByte(padding1);

            temp = BitConverter.GetBytes(getSystemVersion());
            Array.Reverse(temp);
            buffer.Write(temp);
            temp = BitConverter.GetBytes(getTicket().getTitleID());
            Array.Reverse(temp);
            buffer.Write(temp);
            temp = BitConverter.GetBytes(titleType);
            Array.Reverse(temp);
            buffer.Write(temp);
            temp = BitConverter.GetBytes(getGroupID());
            Array.Reverse(temp);
            buffer.Write(temp);
            temp = BitConverter.GetBytes(getAppType());
            Array.Reverse(temp);
            buffer.Write(temp);
            temp = BitConverter.GetBytes(random1);
            Array.Reverse(temp);
            buffer.Write(temp);
            temp = BitConverter.GetBytes(random2);
            Array.Reverse(temp);
            buffer.Write(temp);
            buffer.Write(reserved);
            temp = BitConverter.GetBytes(accessRights);
            Array.Reverse(temp);
            buffer.Write(temp);
            temp = BitConverter.GetBytes(getTitleVersion());
            Array.Reverse(temp);
            buffer.Write(temp);
            temp = BitConverter.GetBytes(contentCount);
            Array.Reverse(temp);
            buffer.Write(temp);
            temp = BitConverter.GetBytes(bootIndex);
            Array.Reverse(temp);
            buffer.Write(temp);

            buffer.Write(padding3);
            buffer.Write(SHA2);

            buffer.Write(getContentInfos().getAsData());
            buffer.Write(getContents().getAsData());

            return buffer.GetBuffer();
        }

        public int getDataSize()
        {
            int staticSize = 0x204;
            int contentInfoSize = contentInfos.getDataSize();
            int contentsSize = contents.getDataSize();

            return staticSize + contentInfoSize + contentsSize;
        }

        public ContentInfos getContentInfos()
        {
            if (contentInfos == null)
            {
                contentInfos = new ContentInfos();
            }

            return contentInfos;
        }

        public void setContentInfos(ContentInfos contentInfos)
        {
            this.contentInfos = contentInfos;
        }

        public Contents getContents()
        {
            if (contents == null)
            {
                contents = new Contents();
            }

            return contents;
        }

        public Ticket getTicket()
        {
            return ticket;
        }

        public void setTicket(Ticket ticket)
        {
            this.ticket = ticket;
        }

        public Encryption getEncryption()
        {
            MemoryStream iv_buffer = new MemoryStream(0x10);
            byte[] temp = BitConverter.GetBytes(getTicket().getTitleID());
            Array.Reverse(temp);
            iv_buffer.Write(temp);
            Key key = getTicket().getDecryptedKey();

            return new Encryption(key, new IV(iv_buffer.GetBuffer()));
        }

        public long getSystemVersion()
        {
            return systemVersion;
        }

        public void setSystemVersion(long systemVersion)
        {
            this.systemVersion = systemVersion;
        }

        public short getGroupID()
        {
            return groupID;
        }

        public void setGroupID(short groupID)
        {
            this.groupID = groupID;
        }

        public uint getAppType()
        {
            return appType;
        }

        public void setAppType(uint appType)
        {
            this.appType = appType;
        }

        public short getTitleVersion()
        {
            return titleVersion;
        }

        public void setTitleVersion(short titleVersion)
        {
            this.titleVersion = titleVersion;
        }
    }
}
