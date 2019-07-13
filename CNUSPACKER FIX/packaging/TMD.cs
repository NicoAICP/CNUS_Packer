using System;
using System.Collections.Generic;
using System.Text;
using CNUS_packer.contents;
using CNUS_packer.crypto;

using System.IO;
using CNUS_packer.utils;

namespace CNUS_packer.packaging
{
    public class TMD
    {
        private int signatureType = 0x00010004;  
        private byte[] signature = new byte[0x100];                            
        private byte[] padding0 = new byte[0x3C];
        private static MemoryStream issuer_stream = new MemoryStream(0x40);


        private byte[] issuer;
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
        private byte[] SHA2 = new byte[0x20];                                  

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
            WriteIssues();
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
        void WriteIssues() {

            issuer_stream.Write(utils.utils.HexStringToByteArray("526F6F742D434130303030303030332D435030303030303030620000000000000000000000000000000000000000000000000000000000000000000000000000"));
            this.issuer = issuer_stream.ToArray();
        }

            
        public void update()
        {
            updateContents();
        }

        public void updateContents()
        {
            this.contentCount = (short)(contents.getContentCount());

            ContentInfo firstContentInfo = new ContentInfo(contents.getContentCount());
            byte[] randomHash = new byte[0x20];
            Random rnd = new Random();
            rnd.NextBytes(randomHash);

            firstContentInfo.setSHA2Hash(utils.HashUtil.hashSHA2(contents.getAsData()));
            getContentInfos().setContentInfo(0, firstContentInfo);
        }
        public void updateContentInfoHash()
        {
            this.SHA2 = utils.HashUtil.hashSHA2(getContentInfos().getAsData());
        }
        public byte[] getAsData()
        {
            MemoryStream bf_strm =  new MemoryStream(getDataSize());
            BinaryWriter buffer = new BinaryWriter(bf_strm);
            buffer.Write(signatureType);
            buffer.Write(signature);
            buffer.Write(padding0);
            buffer.Write(issuer);

            buffer.Write(version);
            buffer.Write(CACRLVersion);
            buffer.Write(signerCRLVersion);
            buffer.Write(padding1);

            buffer.Write(getSystemVersion());
            buffer.Write(getTicket().getTitleID());
            buffer.Write(titleType);
            buffer.Write(getGroupID());
            buffer.Write((int)getAppType());
            buffer.Write(random1);
            buffer.Write(random2);
            buffer.Write(reserved);
            buffer.Write(accessRights);
            buffer.Write(getTitleVersion());
            buffer.Write(contentCount);
            buffer.Write(bootIndex);

            buffer.Write(padding3);
            buffer.Write(SHA2);

            buffer.Write(getContentInfos().getAsData());
            buffer.Write(getContents().getAsData());
            //buffer.put(certs); not needed
            return bf_strm.ToArray();
        }
        public int getDataSize()
        {
            int staticSize = 0x204;
            int contentInfoSize = contentInfos.getDataSize();
            int contentsSize = contents.getDataSize();
            //int certSize = certs.length;
            return staticSize + contentInfoSize + contentsSize;// + certSize;
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
            MemoryStream iv_strm = new MemoryStream(0x10);
            BinaryWriter iv = new BinaryWriter(iv_strm);
            iv.Write(getTicket().getTitleID());
            Key key = getTicket().getDecryptedKey();
            return new Encryption(key, new IV(iv_strm.ToArray()));
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
