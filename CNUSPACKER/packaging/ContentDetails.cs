namespace CNUSPACKER.packaging
{
    public class ContentDetails
    {
        private const bool isContent = true;
        private const bool isEncrypted = true;

        public readonly bool isHashed;
        public readonly short groupID;
        public readonly long parentTitleID;
        public readonly short entriesFlag;

        public ContentDetails(bool isHashed, short groupID, long parentTitleID, short entriesFlag)
        {
            this.isHashed = isHashed;
            this.groupID = groupID;
            this.parentTitleID = parentTitleID;
            this.entriesFlag = entriesFlag;
        }

        public override string ToString()
        {
            return $"ContentDetails [isContent={isContent}, isEncrypted={isEncrypted}, isHashed={isHashed}, groupID={groupID}, parentTitleID={parentTitleID}, entriesFlag={entriesFlag}]";
        }
    }
}
