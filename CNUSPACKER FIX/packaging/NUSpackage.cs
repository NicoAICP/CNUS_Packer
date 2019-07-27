using System;
using System.IO;
using CNUS_packer.contents;
using CNUS_packer.crypto;
using CNUS_packer.utils;

namespace CNUS_packer.packaging
{
    public class NUSpackage
    {
        public Ticket ticket { get; set; }
        public TMD tmd { get; set; }
        public FST fst { get; set; }

        public void packContents(string outputDir)
        {
            Console.WriteLine("Packing Contents.");

            fst.contents.PackContents(outputDir);

            Content fstContent = fst.contents.fstContent;
            fstContent.SHA1 = HashUtil.HashSHA1(fst.GetAsData());
            fstContent.encryptedFileSize = fst.GetAsData().Length;

            ContentInfo contentInfo = tmd.contentInfos.GetContentInfo(0);
            contentInfo.SHA2Hash = HashUtil.HashSHA2(fst.contents.GetAsData());
            tmd.UpdateContentInfoHash();

            FileStream fos;
            using (fos = new FileStream(Path.Combine(outputDir, "title.tmd"), FileMode.OpenOrCreate))
            {
                fos.Write(tmd.GetAsData());
            }
            Console.WriteLine("TMD saved to    " + Path.Combine(outputDir, "title.tmd"));

            using (fos = new FileStream(Path.Combine(outputDir, "title.cert"), FileMode.OpenOrCreate))
            {
                fos.Write(Cert.GetCertAsData());
            }
            Console.WriteLine("Cert saved to   " + Path.Combine(outputDir, "title.cert"));

            using (fos = new FileStream(Path.Combine(outputDir, "title.tik"), FileMode.OpenOrCreate))
            {
                fos.Write(ticket.GetAsData());
            }
            Console.WriteLine("Ticket saved to " + Path.Combine(outputDir, "title.tik"));
            Console.WriteLine();
        }

        public void PrintTicketInfos()
        {
            Console.WriteLine("Encrypted with this key           : " + ticket.decryptedKey);
            Console.WriteLine("Key encrypted with this key       : " + ticket.encryptWith);
            Console.WriteLine();
            Console.WriteLine("Encrypted key                     : " + ticket.GetEncryptedKey());
        }

        public Encryption GetEncryption()
        {
            return tmd.GetEncryption();
        }
    }
}
