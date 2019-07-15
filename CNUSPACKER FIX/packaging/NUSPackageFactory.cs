using CNUS_packer.contents;
using CNUS_packer.fst;

using System;
using System.Collections.Generic;
using System.IO;


namespace CNUS_packer.packaging
{
    public class NUSPackageFactory
    {
        private static Dictionary<Content,NUSpackage> contentDictionary = new Dictionary<Content,NUSpackage>();
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
            List<string> dir_paths = new List<string>();
            string[] dirs = Directory.GetDirectories(config.getDir());
            string[] files = Directory.GetFiles(config.getDir());
            foreach (string d in dirs)
            {
                dir_paths.Add(d);
            }
            foreach (string f in files)
            {
                dir_paths.Add(f);
            }
            //File dir_read = new File(config.getDir());
            readFiles(dir_paths, root);
            Console.WriteLine("Files read. Set it to content files.");
            ContentRulesService.applyRules(root, contents, config.getRules());
            addContentDictonary(contents, nusPackage);
            addContentsDictonary(contents, nusPackage);
            Console.WriteLine("Generating the FST.");

            fst.update();
            Console.WriteLine("Generating the Ticket");
            Ticket ticket = new Ticket(config.getAppInfo().GetTitleID(), config.getEncryptionKey(), config.getEncryptKeyWith());
            Console.WriteLine("Generating the TMD");
            TMD tmd = new TMD(config.getAppInfo(), fst, ticket);
            tmd.update();
            addTMDDictonary(tmd, nusPackage);
            nusPackage.setFST(fst);
            nusPackage.setTicket(ticket);
            nusPackage.setTMD(tmd);
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

        public static void readFiles(List<string> list, FSTEntry parent)
        {
            readFiles(list, parent, false);
        }
        public static void readFiles(List<string> path_dir_files, FSTEntry parent, bool notInNUSPackage)
        {
            /* foreach (File f in list)
             {
                 if (!f.isDirectory())
                 {
                     parent.addChildren(new FSTEntry(f, notInNUSPackage));
                 }
             }
             foreach (File f in list)
             {
                 if (f.isDirectory())
                 {
                     FSTEntry newdir = new FSTEntry(f, notInNUSPackage);
                     parent.addChildren(newdir);
                     readFiles(f.listFiles(), newdir, notInNUSPackage);
                 }
             }*/
            foreach (string f in path_dir_files)
            {
                if (!Directory.Exists(f))
                {
                    parent.addChildren(new FSTEntry(f, notInNUSPackage));
                }
            }
            foreach (string f in path_dir_files)
            {
                if (Directory.Exists(f))
                {
                    List<string> newpath = new List<string>();
                    string[] dir = Directory.GetDirectories(f);
                    string[] files = Directory.GetFiles(f);
                    foreach (string d in dir)
                    {
                        newpath.Add(d);
                    }
                    foreach (string fil in files)
                    {
                        newpath.Add(fil);
                    }
                    FSTEntry newdir = new FSTEntry(f, notInNUSPackage);
                    parent.addChildren(newdir);
                    readFiles(newpath, newdir, notInNUSPackage);
                }
            }
        }
    }
}
