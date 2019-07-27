using CNUS_packer.contents;
using CNUS_packer.crypto;
using CNUS_packer.utils;

using System;
using System.IO;

namespace CNUS_packer.packaging
{
    public class TMD
    {
        private const int signatureType = 0x00010004;
        private readonly byte[] signature = new byte[0x100];
        private readonly byte[] padding0 = new byte[0x3C];
        private static readonly byte[] issuer = Utils.HexStringToByteArray("526F6F742D434130303030303030332D435030303030303030620000000000000000000000000000000000000000000000000000000000000000000000000000");

        private const byte version = 0x01;
        private const byte CACRLVersion = 0x00;
        private const byte signerCRLVersion = 0x00;
        private const byte padding1 = 0x00;

        private readonly long systemVersion;

        private const int titleType = 0x000100;
        private readonly short groupID;
        private readonly uint appType;
        private readonly int random1 = 0;
        private readonly int random2 = 0;
        private readonly byte[] reserved = new byte[50];
        private const int accessRights = 0x0000;
        private readonly short titleVersion;
        private short contentCount;
        private const short bootIndex = 0x00;
        private readonly byte[] padding3 = new byte[2];
        private byte[] SHA2 = new byte[0x20];

        public readonly ContentInfos contentInfos = new ContentInfos();
        private Contents contents = new Contents();

        private readonly Ticket ticket;

        public TMD(AppXMLInfo appInfo, FST fst, Ticket ticket)
        {
            groupID = appInfo.groupID;
            systemVersion = appInfo.osVersion;
            appType = appInfo.appType;
            titleVersion = appInfo.titleVersion;
            this.ticket = ticket;
            SetContents(fst.contents);
        }

        private void SetContents(Contents contents)
        {
            if (contents != null)
            {
                this.contents = contents;
                contentCount = contents.GetContentCount();
            }
        }

        public void Update()
        {
            UpdateContents();
        }

        private void UpdateContents()
        {
            contentCount = contents.GetContentCount();

            ContentInfo firstContentInfo = new ContentInfo(contents.GetContentCount())
            {
                SHA2Hash = HashUtil.HashSHA2(contents.GetAsData())
            };

            contentInfos.SetContentInfo(0, firstContentInfo);
        }

        public void UpdateContentInfoHash()
        {
            SHA2 = HashUtil.HashSHA2(contentInfos.GetAsData());
        }

        public byte[] GetAsData()
        {
            MemoryStream buffer = new MemoryStream(GetDataSize());
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

            temp = BitConverter.GetBytes(systemVersion);
            Array.Reverse(temp);
            buffer.Write(temp);
            temp = BitConverter.GetBytes(ticket.titleID);
            Array.Reverse(temp);
            buffer.Write(temp);
            temp = BitConverter.GetBytes(titleType);
            Array.Reverse(temp);
            buffer.Write(temp);
            temp = BitConverter.GetBytes(groupID);
            Array.Reverse(temp);
            buffer.Write(temp);
            temp = BitConverter.GetBytes(appType);
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
            temp = BitConverter.GetBytes(titleVersion);
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

            buffer.Write(contentInfos.GetAsData());
            buffer.Write(contents.GetAsData());

            return buffer.GetBuffer();
        }

        private int GetDataSize()
        {
            const int staticSize = 0x204;
            int contentInfoSize = ContentInfos.GetDataSize();
            int contentsSize = contents.GetDataSize();

            return staticSize + contentInfoSize + contentsSize;
        }

        public Encryption GetEncryption()
        {
            MemoryStream iv_buffer = new MemoryStream(0x10);
            byte[] temp = BitConverter.GetBytes(ticket.titleID);
            Array.Reverse(temp);
            iv_buffer.Write(temp);
            Key key = ticket.decryptedKey;

            return new Encryption(key, new IV(iv_buffer.GetBuffer()));
        }
    }
}
