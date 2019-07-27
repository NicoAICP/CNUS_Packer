namespace CNUS_packer.packaging
{
    public class ContentDetails
    {
        private const bool isContent = true;
        private const bool isEncrypted = true;
        public bool isHashed { get; }
        public short groupID { get; }
        public long parentTitleID { get; }

        public short entriesFlag { get; }

        public ContentDetails(bool isHashed, short groupID, long parentTitleID, short entriesFlag)
        {
            this.isHashed = isHashed;
            this.groupID = groupID;
            this.parentTitleID = parentTitleID;
            this.entriesFlag = entriesFlag;
        }

        public override string ToString()
        {
            return "ContentDetails [isContent=" + isContent + ", isEncrypted=" + isEncrypted + ", isHashed=" + isHashed
                + ", groupID=" + groupID + ", parentTitleID=" + parentTitleID + ", entriesFlag=" + entriesFlag + "]";
        }
    }
}
