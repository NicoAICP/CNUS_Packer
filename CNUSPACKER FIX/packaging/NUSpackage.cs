using System;
using System.IO;
using System.Linq;
using CNUS_packer.contents;
using CNUS_packer.crypto;

namespace CNUS_packer.packaging
{
    public class NUSpackage
    {
        private Ticket ticket;
        private TMD tmd;
        private FST fst;

        private string outputdir = "output";

        public Ticket getTicket()
        {
            return ticket;
        }

        public void setTicket(Ticket ticket)
        {
            this.ticket = ticket;
        }

        public string getOutputdir()
        {
            return outputdir;
        }

        public void setOutputdir(string outputdir)
        {
            this.outputdir = outputdir;
        }

        public TMD getTMD()
        {
            return tmd;
        }

        public void setTMD(TMD tmd)
        {
            this.tmd = tmd;
        }

        public FST getFST()
        {
            return fst;
        }

        public void setFST(FST fst)
        {
            this.fst = fst;
        }

        public Contents getContents()
        {
            return getFST().getContents();
        }

        public ContentInfos getContentInfos()
        {
            return getTMD().getContentInfos();
        }

        public bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }

        public void packContents(string outputDir)
        {
            if (outputDir != null)
            {
                setOutputdir(outputDir);
            }
            Console.WriteLine("Packing Contents");
            try
            {
                getFST().getContents().packContents(outputDir);
            }
            catch (Exception e1)
            {
                Console.WriteLine(e1.ToString());
            }
            Content fstContent = getContents().getFSTContent();
            fstContent.setHash(utils.HashUtil.hashSHA2(getContents().getAsData()));
            fstContent.setEncryptedFileSize(getFST().getAsData().Length);
            ContentInfo contentInfo = getContentInfos().getContentInfo(0);
            contentInfo.setSHA2Hash(utils.HashUtil.hashSHA2(getContents().getAsData()));
            try
            {
                /*
                FileOutputStream fos = new FileOutputStream("fst.bin");
                fos.write(fst.getAsData());
                fos.close();*/

                FileStream fos;
                using (fos = new FileStream(Path.Combine(getOutputdir(), "title.tmd"), FileMode.OpenOrCreate))
                {
                    fos.Write(tmd.getAsData());
                }
                Console.WriteLine("TMD saved to    " + Path.Combine(getOutputdir(), "title.tmd"));

                using (fos = new FileStream(Path.Combine(getOutputdir(), "title.cert"), FileMode.OpenOrCreate))
                {
                    fos.Write(Cert.getCertAsData());
                }
                Console.WriteLine("Cert saved to   " + Path.Combine(getOutputdir(), "title.cert"));

                using (fos = new FileStream(Path.Combine(getOutputdir(), "title.tik"), FileMode.OpenOrCreate))
                {
                    fos.Write(ticket.getAsData());
                }
                Console.WriteLine("Ticket saved to " + Path.Combine(getOutputdir(), "title.tik"));
                Console.WriteLine();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void printTicketInfos()
        {
            Console.WriteLine("Encrypted with this key           : " + getTicket().getDecryptedKey());
            Console.WriteLine("Key encrypted with this key       : " + getTicket().getEncryptWith());
            Console.WriteLine();
            Console.WriteLine("Encrypted key                     : " + getTicket().getEncryptedKey());
        }

        public Encryption getEncryption()
        {
            return getTMD().getEncryption();
        }
    }
}
