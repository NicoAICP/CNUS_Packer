using System;
using System.IO;
using CNUSPACKER.contents;
using CNUSPACKER.fst;

namespace CNUSPACKER.packaging
{
    public static class NUSPackageFactory
    {
        public static NUSpackage CreateNewPackage(NusPackageConfiguration config)
        {
            Contents contents = new Contents();
            FST fst = new FST(contents);

            FSTEntry root = fst.fileEntries.GetRootEntry();
            root.SetContent(contents.fstContent);

            PopulateFSTEntries(config.dir, root);

            Console.WriteLine("Finished reading in input files. Files read. Applying content rules.");

            ContentRulesService.ApplyRules(root, contents, config.rules);

            Console.WriteLine("Generating the FST.");
            fst.Update();

            Console.WriteLine("Generating the Ticket");
            Ticket ticket = new Ticket(config.appInfo.titleID, config.encryptionKey, config.encryptKeyWith);

            Console.WriteLine("Generating the TMD");
            TMD tmd = new TMD(config.appInfo, fst, ticket);

            return new NUSpackage(fst, ticket, tmd);
        }

        private static void PopulateFSTEntries(string directory, FSTEntry parent)
        {
            foreach (string file in Directory.EnumerateFiles(directory)) // files first
            {
                FSTEntry newFile = new FSTEntry(file);
                parent.AddChildren(newFile);
            }

            foreach (string dir in Directory.EnumerateDirectories(directory)) // directories afterwards
            {
                FSTEntry newDir = new FSTEntry(dir);
                parent.AddChildren(newDir);
                PopulateFSTEntries(dir, newDir);
            }
        }
    }
}
