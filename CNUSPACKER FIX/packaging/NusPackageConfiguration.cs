using CNUS_packer.crypto;
using CNUS_packer.utils;
using System;

namespace CNUS_packer.packaging
{
   public class NusPackageConfiguration
    {
        private string dir;
        private AppXMLInfo appInfo;
        private Key encryptionKey;
        private Key encryptKeyWith;
        private ContentRules rules;
        private string fullGameDir = null;

        public NusPackageConfiguration(string dir, AppXMLInfo appInfo, Key encryptionKey, Key encryptKeyWith, ContentRules rules)
        {
            setDir(dir);
            setAppInfo(appInfo);
            setEncryptionKey(encryptionKey);
            setEncryptKeyWith(encryptKeyWith);
            setRules(rules);
        }

        public string getDir()
        {
            return dir;
        }

        public void setDir(string dir)
        {
            this.dir = dir;
        }

        public AppXMLInfo getAppInfo()
        {
            return appInfo;
        }

        public void setAppInfo(AppXMLInfo appInfo)
        {
            this.appInfo = appInfo;
        }

        public Key getEncryptionKey()
        {
            return encryptionKey;
        }

        public void setEncryptionKey(Key encryptionKey)
        {
            this.encryptionKey = encryptionKey;
        }

        public Key getEncryptKeyWith()
        {
            return encryptKeyWith;
        }

        public void setEncryptKeyWith(Key encryptKeyWith)
        {
            this.encryptKeyWith = encryptKeyWith;
        }

        public ContentRules getRules()
        {
            return rules;
        }

        public void setRules(ContentRules rules)
        {
            this.rules = rules;
        }

        public String getFullGameDir()
        {
            return fullGameDir;
        }

        public void setFullGameDir(String fullGameDir)
        {
            this.fullGameDir = fullGameDir;
        }
    }
}
