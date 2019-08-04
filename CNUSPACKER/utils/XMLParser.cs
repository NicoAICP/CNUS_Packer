using System;
using System.Xml;

namespace CNUSPACKER.utils
{
    public class XMLParser
    {
        private readonly XmlDocument document = new XmlDocument();

        public void LoadDocument(string path)
        {
            document.Load(path);
        }

        public AppXMLInfo GetAppXMLInfo()
        {
            AppXMLInfo appxmlinfo = new AppXMLInfo
            {
                osVersion = GetValueOfElementAsLongHex("app/os_version"),
                titleID = GetValueOfElementAsLongHex("app/title_id"),
                titleVersion = (short) GetValueOfElementAsLongHex("app/title_version"),
                sdkVersion = GetValueOfElementAsUnsignedInt("app/sdk_version"),
                appType = (uint) GetValueOfElementAsLongHex("app/app_type"),
                groupID = (short) GetValueOfElementAsLongHex("app/group_id"),
                osMask = GetValueOfElementAsByteArray("app/os_mask"),
                commonID = GetValueOfElementAsLongHex("app/common_id")
            };

            return appxmlinfo;
        }

        private uint GetValueOfElementAsUnsignedInt(string element)
        {
            return uint.Parse(GetValueOfElement(element) ?? "0");
        }

        private long GetValueOfElementAsLongHex(string element)
        {
            return Convert.ToInt64(GetValueOfElement(element) ?? "0", 16);
        }

        private byte[] GetValueOfElementAsByteArray(string element)
        {
            return Utils.HexStringToByteArray(GetValueOfElement(element) ?? "");
        }

        private string GetValueOfElement(string element)
        {
            XmlNode node = document.SelectSingleNode(element);
            if (node == null)
            {
                Console.WriteLine("No xml entry for field \"" + element + "\", default value will be used.");
                return null;
            }

            return node.InnerText;
        }
    }
}

