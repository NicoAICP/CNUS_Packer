using CNUS_packer.utils;
using System;
using System.IO;

using CNUS_packer.packaging;
using CNUS_packer.crypto;

namespace CNUS_packer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CNUS_Packer v0.01 by NicoAICP [C# Port of NUSPacker by timogus]\n\n");

            Directory.CreateDirectory(settings.tmpDir);

            string inputPath = "input";
            string outputPath = "output";

            string encryptionKey = "";
            string encryptKeyWith = "";

            long titleID = 0x0L;
            long osVersion = 0x000500101000400AL;
            uint appType = 0x80000000;
            short titleVersion = 0;

            bool skipXMLReading = false;

            if (args.Length == 0)
            {
                Console.WriteLine("Please provide at least the in and out parameter");

                showHelp();
                Environment.Exit(0);
            }
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-in"))
                {
                    if (args.Length > i + 1)
                    {
                        inputPath = args[i + 1];
                        i++;
                    }
                }
                else if (args[i].Equals("-out"))
                {
                    if (args.Length > i + 1)
                    {
                        outputPath = args[i + 1];
                        Directory.CreateDirectory(outputPath);
                        i++;
                    }
                }
                else if (args[i].Equals("-tID"))
                {
                    if(args.Length > i + 1)
                    {
                        titleID = utils.utils.HexStringToLong(args[i + 1]);
                        i++;
                    }
                }
                else if (args[i].Equals("-OSVersion"))
                {
                    if (args.Length > i + 1)
                    {
                        osVersion = utils.utils.HexStringToLong(args[i + 1]);
                        i++;
                    }
                }
                else if (args[i].Equals("-appType"))
                {
                    if (args.Length > i + 1)
                    {
                        appType = (uint) utils.utils.HexStringToLong(args[i + 1]);
                        i++;
                    }
                }
                else if (args[i].Equals("-titleVersion"))
                {
                    if (args.Length > i + 1)
                    {
                        titleVersion = (short) utils.utils.HexStringToLong(args[i + 1]);
                        i++;
                    }
                }
                else if (args[i].Equals("-encryptionKey"))
                {
                    if (args.Length > i + 1)
                    {
                        encryptionKey = args[i + 1];
                        i++;
                    }
                }
                else if (args[i].Equals("-encryptKeyWith"))
                {
                    if (args.Length > i + 1)
                    {
                        encryptKeyWith = args[i + 1];
                        i++;
                    }
                }
                else if (args[i].Equals("-skipXMLParsing"))
                {
                    skipXMLReading = true;
                }
                else if (args[i].Equals("-help"))
                {
                    showHelp();
                    Environment.Exit(0);
                }
            }

            if(!Directory.Exists(inputPath + @"/code") || !Directory.Exists(inputPath + @"/content") || !Directory.Exists(inputPath + @"/meta"))
            {
                Console.WriteLine("Invalid input dir (" + Directory.GetDirectoryRoot(inputPath) +"): It's missing either the code, content or meta folder");
                Environment.Exit(0);
            }
            AppXMLInfo appinfo = new AppXMLInfo();
            appinfo.SetTitleID(titleID);
            appinfo.SetGroupID((short)((titleID >> 8) & 0xFFFF));
            appinfo.SetAppType(appType);
            appinfo.SetOsVersion(osVersion);
            appinfo.SetTitleVersion(titleVersion);

            if(encryptionKey == "" || encryptionKey.Length != 32)
            {
                encryptionKey = settings.defaultEncryptionKey;
                Console.WriteLine(encryptionKey);
                Console.WriteLine("Empty or invalid encryption provided. Will use " + encryptionKey + " instead");
            }
            Console.WriteLine();
            if(encryptKeyWith == "" || encryptKeyWith.Length != 32)
            {
                Console.WriteLine("Will try to load the encryptionWith key from the file \"" + settings.encyptWithFile + "\"");
                encryptKeyWith = loadEncryptWithKey();
            }
            if (encryptKeyWith == "" || encryptKeyWith.Length != 32)
            {
                encryptKeyWith = settings.defaultEncryptWithKey;
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!");
                Console.WriteLine("WARNING:Empty or invalid encryptWith key provided. Will use " + encryptKeyWith + " instead");
                Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!");
            }

            string appxml = inputPath + settings.pathToAppXml;

            if (!skipXMLReading)
            {
                try
                {
                    Console.WriteLine("Parsing app.xml (values will be overwritten. Use the -skipXMLParsing argument to disable it)");
                    XMLParser parser = new XMLParser();
                    parser.loadDocument(appxml);

                    appinfo = parser.getAppXMLInfo();
                }
                catch (Exception  e)
                {
                    Console.WriteLine("Error while parsing the app.xml from path \"" + settings.pathToAppXml + "\": " + e);
                }
            }
            else
            {
                Console.WriteLine("Skipped app.xml parsing");
            }
            short content_group = appinfo.GetGroupID();
            titleID = appinfo.GetTitleID();

            long parentID = titleID & ~0x0000000F00000000L;
            Console.WriteLine();
            Console.WriteLine("Configuration:");
            Console.WriteLine("Input            : \"" + inputPath + "\"");
            Console.WriteLine("Output           : \"" + outputPath + "\"");

            Console.WriteLine("TitleID          : " + appinfo.GetTitleID().ToString("X"));
            Console.WriteLine("GroupID          : " + appinfo.GetGroupID().ToString("X"));
            Console.WriteLine("ParentID         : " + parentID.ToString("X"));
            Console.WriteLine("AppType          : " + appinfo.GetAppType().ToString("X"));
            Console.WriteLine("OSVersion        : " + appinfo.GetOsVersion().ToString("X"));
            Console.WriteLine("Encryption key   : " + encryptionKey);
            Console.WriteLine("Encrypt key with : " + encryptKeyWith);
            Console.WriteLine();

            Console.WriteLine("---");
            ContentRules rules = ContentRules.getCommonRules(content_group, parentID);

            NusPackageConfiguration config = new NusPackageConfiguration(inputPath, appinfo, new Key(encryptionKey), new Key(encryptKeyWith), rules);
            NUSpackage nuspackage = NUSPackageFactory.createNewPackage(config);
            nuspackage.packContents(outputPath);
            nuspackage.printTicketInfos();

            utils.utils.deleteDir(settings.tmpDir);
        }

        public static string loadEncryptWithKey()
        {
            string encryptPath = settings.encyptWithFile;
            if (!File.Exists(encryptPath)) return "";
            string key = "";
            using (StreamReader input = new StreamReader(encryptPath))
            {
                try
                {
                    key = input.ReadLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to read \"" + settings.encyptWithFile + "\"");
                    Console.WriteLine(e.ToString());
                }
            }

            return key;
        }

        private static void showHelp()
        {
            Console.WriteLine("help:");
            Console.WriteLine("-in             ; is the dir where you have your decrypted data. Make this pointing to the root folder with the folder code,content and meta.");
            Console.WriteLine("-out            ; Where the installable package will be saves");
            Console.WriteLine("");
            Console.WriteLine("(optional! will be parsed from app.xml if missing)");
            Console.WriteLine("-tID            ; titleId of this package. Will be saved in the TMD and provided as 00050000XXXXXXXX");
            Console.WriteLine("-OSVersion      ; target OS version");
            Console.WriteLine("-appType        ; app type");
            Console.WriteLine("-skipXMLParsing ; disables the app.xml parsing");
            Console.WriteLine("");
            Console.WriteLine("(optional! defaults values will be used if missing (or loaded from external file))");
            Console.WriteLine("-encryptionKey  ; the key that is used to encrypt the package");
            Console.WriteLine("-encryptKeyWith ; the key that is used to encrypt the encryption key");
            Console.WriteLine("");
        }
    }
}
