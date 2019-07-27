using System.Collections.Generic;
using CNUS_packer.crypto;
using CNUS_packer.utils;

namespace CNUS_packer.packaging
{
    public class NusPackageConfiguration
    {
        public string dir { get; }
        public AppXMLInfo appInfo { get; }
        public Key encryptionKey { get; }
        public Key encryptKeyWith { get; }
        public List<ContentRule> rules { get; }

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
