namespace CNUSPACKER
{
    public static class Settings
    {
        public const short groupid_code = 0x0000;
        public const short groupid_meta = 0x0400;

        public const short fstflags_code = 0x0000;
        public const short fstflags_meta = 0x0040;
        public const short fstflags_content = 0x0400;

        public const string encyptWithFile = "encyptKeyWith";

        public const string defaultEncryptionKey = "13371337133713371337133713371337";
        public const string defaultEncryptWithKey = "00000000000000000000000000000000";

        public const string pathToAppXml = @"\code\app.xml";

        public const string tmpDir = "tmp";
    }
}
