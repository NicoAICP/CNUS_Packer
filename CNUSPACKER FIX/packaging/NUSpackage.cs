using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            return !System.IO.Directory.EnumerateFileSystemEntries(path).Any();
        }
        public void packContents(string outputDir)
        {
            if (outputDir != null && !IsDirectoryEmpty(outputDir))
            {
                setOutputdir(outputDir);
            }
            System.Console.WriteLine("Packing Contents");
            try
            {
                getFST().getContents().packContents(outputDir);
            }catch(Exception e1)
            {
                System.Console.WriteLine(e1.Message);
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

                FileStream fos = new FileStream(getOutputdir() + "/title.tmd", FileMode.OpenOrCreate);
                fos.Write(tmd.getAsData());
                fos.Close();
                System.Console.WriteLine("TMD saved to    " + getOutputdir() + "/title.tmd");

                fos = new FileStream(getOutputdir() + "/title.cert", FileMode.OpenOrCreate);
                fos.Write(Cert.getCertAsData());
                fos.Close();
                System.Console.WriteLine("Cert saved to   " + getOutputdir() + "/title.cert");

                fos = new FileStream(getOutputdir() + "/title.tik", FileMode.OpenOrCreate);
                fos.Write(ticket.getAsData());
                fos.Close();
                System.Console.WriteLine("Ticket saved to " + getOutputdir() + "/title.tik");
                System.Console.WriteLine();

            }
            catch (IOException e)
            {
                System.Console.WriteLine(e.Message);
            }

        }
        public void printTicketInfos()
        {
            System.Console.WriteLine("Encrypted with this key           : " + getTicket().getDecryptedKey());
            System.Console.WriteLine("Key encrypted with this key       : " + getTicket().getEncryptWith());
            System.Console.WriteLine();
            System.Console.WriteLine("Encrypted key                     : " + getTicket().getEncryptedKey());

        }
        public Encryption getEncryption()
        {
            return getTMD().getEncryption();
        }
    }
}
