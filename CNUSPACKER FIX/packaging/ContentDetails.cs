namespace CNUS_packer.packaging
{
    public class ContentDetails
    {
        private bool isContent = true;
        private bool isEncrypted = true;
        private bool isHashed = false;
        private short groupID = 0x0000;
        private long parentTitleID = 0x0;

        private short entriesFlag = 0x0000;

        public ContentDetails(bool isHashed, short groupID, long parentTitleID, short entriesFlags)
        {
            setHashed(isHashed);
            setGroupID(groupID);
            setParentTitleID(parentTitleID);
            setEntriesFlag(entriesFlags);
        }

        public bool GetisHashed()
        {
            return isHashed;
        }

        public void setHashed(bool isHashed)
        {
            this.isHashed = isHashed;
        }

        public short getGroupID()
        {
            return groupID;
        }

        public void setGroupID(short groupID)
        {
            this.groupID = groupID;
        }

        public long getParentTitleID()
        {
            return parentTitleID;
        }

        public void setParentTitleID(long parentTitleID)
        {
            this.parentTitleID = parentTitleID;
        }

        public bool GetisContent()
        {
            return isContent;
        }

        public void setContent(bool isContent)
        {
            this.isContent = isContent;
        }

        public bool GetisEncrypted()
        {
            return isEncrypted;
        }

        public void setEncrypted(bool isEncrypted)
        {
            this.isEncrypted = isEncrypted;
        }

        public short getEntriesFlag()
        {
            return entriesFlag;
        }

        public void setEntriesFlag(short entriesFlag)
        {
            this.entriesFlag = entriesFlag;
        }

        public override string ToString()
        {
            return "ContentDetails [isContent=" + isContent + ", isEncrypted=" + isEncrypted + ", isHashed=" + isHashed
                + ", groupID=" + groupID + ", parentTitleID=" + parentTitleID + ", entriesFlag=" + entriesFlag + "]";
        }
    }
}
