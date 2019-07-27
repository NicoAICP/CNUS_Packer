using System;
using System.Collections.Generic;
using System.IO;
using CNUS_packer.contents;
using CNUS_packer.fst;

namespace CNUS_packer.packaging
{
    public static class NUSPackageFactory
    {
        private static readonly Dictionary<Content, NUSpackage> contentDictionary = new Dictionary<Content, NUSpackage>();
        private static readonly Dictionary<FST, NUSpackage> FSTDictionary = new Dictionary<FST, NUSpackage>();
        private static readonly Dictionary<TMD, NUSpackage> TMDDictionary = new Dictionary<TMD, NUSpackage>();
        private static readonly Dictionary<FSTEntries, NUSpackage> FSTEntriesDictionary = new Dictionary<FSTEntries, NUSpackage>();
        private static readonly Dictionary<Contents, NUSpackage> contentsDictionary = new Dictionary<Contents, NUSpackage>();

        public static NUSpackage CreateNewPackage(NusPackageConfiguration config)
        {
            NUSpackage nusPackage = new NUSpackage();
            Contents contents = new Contents();
            FST fst = new FST(contents);
            AddFSTDictonary(fst, nusPackage);
            FSTEntries entries = fst.fileEntries;
            AddFSTEntriesDictonary(fst.fileEntries, nusPackage);

            FSTEntry root = entries.GetRootEntry();
            root.SetContent(contents.fstContent);
            ReadFiles(Directory.EnumerateFiles(config.dir), Directory.EnumerateDirectories(config.dir), root);

            Console.WriteLine("Files read. Set it to content files.");

            ContentRulesService.ApplyRules(root, contents, config.rules);
            AddContentsDictonary(contents, nusPackage);
            AddContentDictonary(contents, nusPackage);

            Console.WriteLine("Generating the FST.");
            fst.Update();

            Console.WriteLine("Generating the Ticket");
            Ticket ticket = new Ticket(config.appInfo.titleID, config.encryptionKey, config.encryptKeyWith);

            Console.WriteLine("Generating the TMD");
            TMD tmd = new TMD(config.appInfo, fst, ticket);
            tmd.Update();

            AddTMDDictonary(tmd, nusPackage);

            nusPackage.fst = fst;
            nusPackage.ticket = ticket;
            nusPackage.tmd = tmd;

            return nusPackage;
        }

        private static void AddContentsDictonary(Contents contents, NUSpackage nusPackage)
        {
            contentsDictionary.Add(contents, nusPackage);
        }

        private static void AddContentDictonary(Contents contents, NUSpackage nusPackage)
        {
            foreach (Content c in contents.contents)
            {
                if (!contentDictionary.ContainsKey(c))
                {
                    contentDictionary.Add(c, nusPackage);
                }
            }
        }

        private static void AddTMDDictonary(TMD tmd, NUSpackage nusPackage)
        {
            TMDDictionary.Add(tmd, nusPackage);
        }

        private static void AddFSTDictonary(FST fst, NUSpackage nusPackage)
        {
            FSTDictionary.Add(fst, nusPackage);
        }

        private static void AddFSTEntriesDictonary(FSTEntries fstEntries, NUSpackage nusPackage)
        {
            FSTEntriesDictionary.Add(fstEntries, nusPackage);
        }

        public static NUSpackage GetPackageByContent(Content content)
        {
            return contentDictionary.ContainsKey(content) ? contentDictionary[content] : null;
        }

        public static NUSpackage GetPackageByFST(FST fst)
        {
            return FSTDictionary.ContainsKey(fst) ? FSTDictionary[fst] : null;
        }

        public static NUSpackage GetPackageByTMD(TMD tmd)
        {
            return TMDDictionary.ContainsKey(tmd) ? TMDDictionary[tmd] : null;
        }

        public static NUSpackage GetPackageByContents(Contents contents)
        {
            return contentsDictionary.ContainsKey(contents) ? contentsDictionary[contents] : null;
        }

        public static NUSpackage GetPackageByFSTEntires(FSTEntries fstEntries)
        {
            return FSTEntriesDictionary.ContainsKey(fstEntries) ? FSTEntriesDictionary[fstEntries] : null;
        }

        private static void ReadFiles(IEnumerable<string> file_paths, IEnumerable<string> dir_paths, FSTEntry parent, bool notInNUSPackage = false)
        {
            foreach (string file in file_paths) // files first
            {
                FSTEntry newFile = new FSTEntry(file, notInNUSPackage);
                parent.AddChildren(newFile);
            }

            foreach (string dir in dir_paths) // directories afterwards
            {
                FSTEntry newDir = new FSTEntry(dir, notInNUSPackage);
                parent.AddChildren(newDir);
                ReadFiles(Directory.EnumerateFiles(dir), Directory.EnumerateDirectories(dir), newDir, notInNUSPackage);
            }
        }
    }
}
