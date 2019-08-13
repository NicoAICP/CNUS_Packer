using System.Collections.Generic;
using CNUSPACKER.crypto;
using CNUSPACKER.utils;

namespace CNUSPACKER.packaging
{
    public class NusPackageConfiguration
    {
        public readonly string dir;
        public readonly AppXMLInfo appInfo;
        public readonly Key encryptionKey;
        public readonly Key encryptKeyWith;
        public readonly List<ContentRule> rules;

        public NusPackageConfiguration(string dir, AppXMLInfo appInfo, Key encryptionKey, Key encryptKeyWith, List<ContentRule> rules)
        {
            this.dir = dir;
            this.appInfo = appInfo;
            this.encryptionKey = encryptionKey;
            this.encryptKeyWith = encryptKeyWith;
            this.rules = rules;
        }
    }
}
