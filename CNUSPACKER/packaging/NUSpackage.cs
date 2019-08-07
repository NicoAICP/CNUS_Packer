using System;
using System.IO;
using CNUSPACKER.contents;
using CNUSPACKER.crypto;
using CNUSPACKER.utils;

namespace CNUSPACKER.packaging
{
    public class NUSpackage
    {
        private readonly FST fst;
        private readonly Ticket ticket;
        private readonly TMD tmd;

        public NUSpackage(FST fst, Ticket ticket, TMD tmd)
        {
            this.fst = fst;
            this.ticket = ticket;
            this.tmd = tmd;
        }

        public void PackContents(string outputDir)
        {
            Console.WriteLine("Packing Contents.");

            Encryption encryption = GetEncryption();
            fst.contents.PackContents(outputDir, encryption);

            Console.WriteLine("Packing the FST into 00000000.app");
            string fstPath = Path.Combine(outputDir, "00000000.app");
            encryption.EncryptFileWithPadding(fst, fstPath, 0, Content.CONTENT_FILE_PADDING);
            Console.WriteLine("-------------");
            Console.WriteLine("Packed all contents\n\n");

            Content fstContent = fst.contents.fstContent;
            fstContent.SHA1 = HashUtil.HashSHA1(fst.GetAsData());
            fstContent.encryptedFileSize = fst.GetDataSize();

            tmd.contentInfo.SHA2Hash = HashUtil.HashSHA2(fst.contents.GetAsData());
            tmd.UpdateContentInfoHash();

            FileStream fos;
            using (fos = new FileStream(Path.Combine(outputDir, "title.tmd"), FileMode.Create))
            {
                fos.Write(tmd.GetAsData());
            }
            Console.WriteLine($"TMD saved to    {Path.Combine(outputDir, "title.tmd")}");

            using (fos = new FileStream(Path.Combine(outputDir, "title.cert"), FileMode.Create))
            {
                fos.Write(Cert.GetCertAsData());
            }
            Console.WriteLine($"Cert saved to   {Path.Combine(outputDir, "title.cert")}");

            using (fos = new FileStream(Path.Combine(outputDir, "title.tik"), FileMode.Create))
            {
                fos.Write(ticket.GetAsData());
            }
            Console.WriteLine($"Ticket saved to {Path.Combine(outputDir, "title.tik")}");
            Console.WriteLine();
        }

        public void PrintTicketInfos()
        {
            Console.WriteLine($"Encrypted with this key           : {ticket.decryptedKey}");
            Console.WriteLine($"Key encrypted with this key       : {ticket.encryptWith}");
            Console.WriteLine();
            Console.WriteLine($"Encrypted key                     : {ticket.GetEncryptedKey()}");
        }

        private Encryption GetEncryption()
        {
            return tmd.GetEncryption();
        }
    }
}
