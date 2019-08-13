using System.IO;
using CNUSPACKER.contents;
using CNUSPACKER.crypto;
using CNUSPACKER.utils;

namespace CNUSPACKER.packaging
{
    public class TMD
    {
        private const int signatureType = 0x00010004;
        private readonly byte[] signature = new byte[0x100];
        private static readonly byte[] issuer = Utils.HexStringToByteArray("526F6F742D434130303030303030332D435030303030303030620000000000000000000000000000000000000000000000000000000000000000000000000000");

        private const byte version = 0x01;
        private const byte CACRLVersion = 0x00;
        private const byte signerCRLVersion = 0x00;

        private readonly long systemVersion;

        private const int titleType = 0x000100;
        private readonly short groupID;
        private readonly uint appType;
        private const int accessRights = 0x0000;
        private readonly short titleVersion;
        private readonly short contentCount;
        private const short bootIndex = 0x00;
        private byte[] SHA2;

        public readonly ContentInfo contentInfo;
        private readonly Contents contents;

        private readonly Ticket ticket;

        public TMD(AppXMLInfo appInfo, FST fst, Ticket ticket)
        {
            groupID = appInfo.groupID;
            systemVersion = appInfo.osVersion;
            appType = appInfo.appType;
            titleVersion = appInfo.titleVersion;
            this.ticket = ticket;
            contents = fst.contents;
            contentCount = contents.GetContentCount();
            contentInfo = new ContentInfo(contentCount)
            {
                SHA2Hash = HashUtil.HashSHA2(contents.GetAsData())
            };
        }

        public void UpdateContentInfoHash()
        {
            SHA2 = HashUtil.HashSHA2(contentInfo.GetAsData());
        }

        public byte[] GetAsData()
        {
            BigEndianMemoryStream buffer = new BigEndianMemoryStream(GetDataSize());

            buffer.WriteBigEndian(signatureType);
            buffer.Write(signature);
            buffer.Seek(60, SeekOrigin.Current);
            buffer.Write(issuer);

            buffer.WriteByte(version);
            buffer.WriteByte(CACRLVersion);
            buffer.WriteByte(signerCRLVersion);
            buffer.Seek(1, SeekOrigin.Current);

            buffer.WriteBigEndian(systemVersion);
            buffer.WriteBigEndian(ticket.titleID);
            buffer.WriteBigEndian(titleType);
            buffer.WriteBigEndian(groupID);
            buffer.WriteBigEndian(appType);
            buffer.Seek(58, SeekOrigin.Current);
            buffer.WriteBigEndian(accessRights);
            buffer.WriteBigEndian(titleVersion);
            buffer.WriteBigEndian(contentCount);
            buffer.WriteBigEndian(bootIndex);
            buffer.Seek(2, SeekOrigin.Current);

            buffer.Write(SHA2);

            buffer.Write(contentInfo.GetAsData());
            buffer.Write(contents.GetAsData());

            return buffer.GetBuffer();
        }

        private int GetDataSize()
        {
            const int staticSize = 0x204;
            const int contentInfoSize = 0x40 * ContentInfo.staticDataSize;
            int contentsSize = contents.GetDataSize();

            return staticSize + contentInfoSize + contentsSize;
        }

        public Encryption GetEncryption()
        {
            BigEndianMemoryStream ivStream = new BigEndianMemoryStream(0x10);
            ivStream.WriteBigEndian(ticket.titleID);

            return new Encryption(ticket.decryptedKey, new IV(ivStream.GetBuffer()));
        }
    }
}
