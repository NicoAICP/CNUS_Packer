namespace CNUS_packer
{
    public class settings
    {
        public static short groupid_code = 0x0000;
        public static short groupid_meta = 0x0400;

        public static short fstflags_code = 0x0000;
        public static short fstflags_meta = 0x0040;
        public static short fstflags_content = 0x0400;

        public static string encyptWithFile = "encyptKeyWith";

        public static string defaultEncryptionKey = "13371337133713371337133713371337";
        public static string defaultEncryptWithKey = "00000000000000000000000000000000";

        public static string pathToAppXml = @"\code\app.xml";

        public static string tmpDir = "tmp";
    }
}
