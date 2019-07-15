using System.Xml;

namespace CNUS_packer.utils
{
    public static class XmlNodeExtensions
    {
        /// <summary>
        /// Copy all child XmlNodes from the source to the destination.
        /// </summary>
        /// <param name="source">Copy children FROM this XmlNode</param>
        /// <param name="destination">Copy children TO this XmlNode</param>
    }

    public class XMLParser
    {
        //private org.w3c.dom.Document document;
        private XmlDocument document = new XmlDocument();

        public void loadDocument(string path)
        {
            XmlDocument document = new XmlDocument();
            //System.Console.WriteLine(path);
            this.document.Load(path);

            //string xmlcontents = this.document.InnerXml;

            //System.Console.WriteLine(xmlcontents);
        }

        public AppXMLInfo getAppXMLInfo()
        {
            AppXMLInfo appxmlinfo = new AppXMLInfo();
            appxmlinfo.SetOsVersion(getValueOfElementAsLongHex("app/os_version", 0));
            appxmlinfo.SetTitleID(getValueOfElementAsLongHex("app/title_id", 0));
            appxmlinfo.SetTitleVersion((short)getValueOfElementAsLongHex("app/title_version", 0));
            appxmlinfo.SetSdkVersion((int)getValueOfElementAsInt("app/sdk_version", 0));
            appxmlinfo.SetAppType((uint)getValueOfElementAsLongHex("app/app_type", 0));
            appxmlinfo.SetGroupID((short)getValueOfElementAsLongHex("app/group_id", 0));
            appxmlinfo.SetOSMask1(getValueOfElementAsByteArray("app/os_mask", 0));
            //appxmlinfo.SetCommon_id(getValueOfElementAsLongHex("app/common_id", 0));

            return appxmlinfo;
        }

        public long getValueOfElementAsInt(string element, int index)
        {
            return int.Parse(getValueOfElement(element, index));
        }

        public long getValueOfElementAsLong(string element, int index)
        {
            return long.Parse(getValueOfElement(element, index));
        }

        public long getValueOfElementAsLongHex(string element, int index)
        {
            return utils.HexStringToLong(getValueOfElement(element, index));
        }

        public byte[] getValueOfElementAsByteArray(string element, int index)
        {
            return utils.HexStringToByteArray(getValueOfElement(element, index));
        }

        public string getValueOfElement(string element)
        {
            return getValueOfElement(element, 0);
        }

        /* public string getValueOfElement(string element, int index)
        {
            if (document == null)
            {
                System.Console.WriteLine("Please load the document first.");
            }
            NodeList list = document.getElementsByTagName(element);
            if (list == null)
            {
                //System.out.println("NodeList is null");
                return "";
            }
            Node node = list.item(index);
            if (node == null)
            {
                //System.out.println("Node is null");
                return "";
            }
            return node.getTextContent().ToString();
        }*/

        public string getValueOfElement(string element, int index)
        {
            if(document == null)
            {
                System.Console.WriteLine("Please load a document first!");
            }
            //XmlNodeList list = document.GetElementsByTagName(element);
            //if(list == null)
            //{
            //    System.Console.WriteLine("List is null");
            //    return "";
            //}
            //System.Console.WriteLine(document.InnerXml);
            XmlNode node = document.SelectSingleNode(element);
            if(node == null || node.InnerText == null)
            {
                System.Console.WriteLine("Node is null");
                return "";
            }
            return node.InnerText;
        }
    }
}

