using CNUS_packer.contents;
using CNUS_packer.crypto;

using System;
using System.IO;

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

            fst.getContents().packContents(outputDir);

            Content fstContent = fst.getContents().getFSTContent();
            fstContent.SHA1 = utils.HashUtil.hashSHA1(fst.getAsData());
            fstContent.setEncryptedFileSize(fst.getAsData().Length);

            ContentInfo contentInfo = tmd.getContentInfos().getContentInfo(0);
            contentInfo.setSHA2Hash(utils.HashUtil.hashSHA2(fst.getContents().getAsData()));
            tmd.updateContentInfoHash();

            FileStream fos;
            using (fos = new FileStream(Path.Combine(outputDir, "title.tmd"), FileMode.OpenOrCreate))
            {
                fos.Write(tmd.getAsData());
            }
            Console.WriteLine("TMD saved to    " + Path.Combine(outputDir, "title.tmd"));

            using (fos = new FileStream(Path.Combine(outputDir, "title.cert"), FileMode.OpenOrCreate))
            {
                fos.Write(Cert.getCertAsData());
            }
            Console.WriteLine("Cert saved to   " + Path.Combine(outputDir, "title.cert"));

            using (fos = new FileStream(Path.Combine(outputDir, "title.tik"), FileMode.OpenOrCreate))
            {
                fos.Write(ticket.getAsData());
            }
            Console.WriteLine("Ticket saved to " + Path.Combine(outputDir, "title.tik"));
            Console.WriteLine();
        }

        public void printTicketInfos()
        {
            Console.WriteLine("Encrypted with this key           : " + ticket.getDecryptedKey());
            Console.WriteLine("Key encrypted with this key       : " + ticket.getEncryptWith());
            Console.WriteLine();
            Console.WriteLine("Encrypted key                     : " + ticket.getEncryptedKey());
        }

        public Encryption getEncryption()
        {
            return tmd.getEncryption();
        }
    }
}
