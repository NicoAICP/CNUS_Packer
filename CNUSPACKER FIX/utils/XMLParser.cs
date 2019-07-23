using System.Xml;

namespace CNUS_packer.utils
{
    public class XMLParser
    {
        private XmlDocument document = new XmlDocument();

        public void loadDocument(string path)
        {
            document.Load(path);
        }

        public AppXMLInfo getAppXMLInfo()
        {
            AppXMLInfo appxmlinfo = new AppXMLInfo();
            appxmlinfo.SetOsVersion(getValueOfElementAsLongHex("app/os_version"));
            appxmlinfo.SetTitleID(getValueOfElementAsLongHex("app/title_id"));
            appxmlinfo.SetTitleVersion((short)getValueOfElementAsLongHex("app/title_version"));
            appxmlinfo.SetSdkVersion(getValueOfElementAsUnsignedInt("app/sdk_version"));
            appxmlinfo.SetAppType((uint)getValueOfElementAsLongHex("app/app_type"));
            appxmlinfo.SetGroupID((short)getValueOfElementAsLongHex("app/group_id"));
            appxmlinfo.SetOSMask(getValueOfElementAsByteArray("app/os_mask"));
            appxmlinfo.SetCommon_id(getValueOfElementAsLongHex("app/common_id"));

            return appxmlinfo;
        }

        public uint getValueOfElementAsUnsignedInt(string element)
        {
            return uint.Parse(getValueOfElement(element) ?? "0");
        }

        public long getValueOfElementAsLongHex(string element)
        {
            return System.Convert.ToInt64(getValueOfElement(element) ?? "0", 16);
        }

        public byte[] getValueOfElementAsByteArray(string element)
        {
            return Utils.HexStringToByteArray(getValueOfElement(element) ?? "");
        }

        public string getValueOfElement(string element)
        {
            XmlNode node = document.SelectSingleNode(element);
            if (node == null)
            {
                System.Console.WriteLine("No xml entry for field \"" + element + "\", default value will be used.");
                return null;
            }

            return node.InnerText;
        }
    }
}

