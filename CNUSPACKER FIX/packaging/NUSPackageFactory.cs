using CNUS_packer.contents;
using CNUS_packer.fst;

using System;
using System.Collections.Generic;
using System.IO;


namespace CNUS_packer.packaging
{
    public class NUSPackageFactory
    {
        private static Dictionary<Content, NUSpackage> contentDictionary = new Dictionary<Content, NUSpackage>();
        private static Dictionary<FST, NUSpackage> FSTDictionary = new Dictionary<FST, NUSpackage>();
        private static Dictionary<TMD, NUSpackage> TMDDictionary = new Dictionary<TMD, NUSpackage>();
        private static Dictionary<FSTEntries, NUSpackage> FSTEntriesDictionary = new Dictionary<FSTEntries, NUSpackage>();
        private static Dictionary<Contents, NUSpackage> contentsDictionary = new Dictionary<Contents, NUSpackage>();

        public static NUSpackage createNewPackage(NusPackageConfiguration config)
        {
            NUSpackage nusPackage = new NUSpackage();
            Contents contents = new Contents();
            FST fst = new FST(contents);
            addFSTDictonary(fst, nusPackage);
            FSTEntries entries = fst.getFSTEntries();
            addFSTEntriesDictonary(fst.getFSTEntries(), nusPackage);

            FSTEntry root = entries.getRootEntry();
            root.setContent(contents.getFSTContent());
            readFiles(Directory.EnumerateFiles(config.getDir()), Directory.EnumerateDirectories(config.getDir()), root);

            Console.WriteLine("Files read. Set it to content files.");

            ContentRulesService.applyRules(root, contents, config.getRules());
            addContentsDictonary(contents, nusPackage);
            addContentDictonary(contents, nusPackage);

            Console.WriteLine("Generating the FST.");
            fst.update();

            Console.WriteLine("Generating the Ticket");
            Ticket ticket = new Ticket(config.getAppInfo().GetTitleID(), config.getEncryptionKey(), config.getEncryptKeyWith());

            Console.WriteLine("Generating the TMD");
            TMD tmd = new TMD(config.getAppInfo(), fst, ticket);
            tmd.update();

            addTMDDictonary(tmd, nusPackage);

            nusPackage.fst = fst;
            nusPackage.ticket = ticket;
            nusPackage.tmd = tmd;

            return nusPackage;
        }

        private static void addContentsDictonary(Contents contents, NUSpackage nusPackage)
        {
            contentsDictionary.Add(contents, nusPackage);
        }

        private static void addContentDictonary(Contents contents, NUSpackage nusPackage)
        {
            foreach (Content c in contents.getContents())
            {
                if (!contentDictionary.ContainsKey(c))
                {
                    contentDictionary.Add(c, nusPackage);
                }
            }
        }

        private static void addTMDDictonary(TMD tmd, NUSpackage nusPackage)
        {
            TMDDictionary.Add(tmd, nusPackage);
        }

        private static void addFSTDictonary(FST fst, NUSpackage nusPackage)
        {
            FSTDictionary.Add(fst, nusPackage);
        }

        private static void addFSTEntriesDictonary(FSTEntries fstEntries, NUSpackage nusPackage)
        {
            FSTEntriesDictionary.Add(fstEntries, nusPackage);
        }

        public static NUSpackage getPackageByContent(Content content)
        {
            if (contentDictionary.ContainsKey(content))
            {
                return contentDictionary[content];
            }
            return null;
        }

        public static NUSpackage getPackageByFST(FST fst)
        {
            if (FSTDictionary.ContainsKey(fst))
            {
                return FSTDictionary[fst];
            }
            return null;
        }

        public static NUSpackage getPackageByTMD(TMD tmd)
        {
            if (TMDDictionary.ContainsKey(tmd))
            {
                return TMDDictionary[tmd];
            }
            return null;
        }

        public static NUSpackage getPackageByContents(Contents contents)
        {
            if (contentsDictionary.ContainsKey(contents))
            {
                return contentsDictionary[contents];
            }

            return null;
        }

        public static NUSpackage getPackageByFSTEntires(FSTEntries fstEntries)
        {
            if (FSTEntriesDictionary.ContainsKey(fstEntries))
            {
                return FSTEntriesDictionary[fstEntries];
            }

            return null;
        }

        public static void readFiles(IEnumerable<string> file_paths, IEnumerable<string> dir_paths, FSTEntry parent)
        {
            readFiles(file_paths, dir_paths, parent, false);
        }

        public static void readFiles(IEnumerable<string> file_paths, IEnumerable<string> dir_paths, FSTEntry parent, bool notInNUSPackage)
        {
            foreach (string file in file_paths) // files first
            {
                FSTEntry newFile = new FSTEntry(file, notInNUSPackage);
                parent.addChildren(newFile);
            }

            foreach (string dir in dir_paths) // directories afterwards
            {
                FSTEntry newDir = new FSTEntry(dir, notInNUSPackage);
                parent.addChildren(newDir);
                readFiles(Directory.EnumerateFiles(dir), Directory.EnumerateDirectories(dir), newDir, notInNUSPackage);
            }
        }
    }
}
