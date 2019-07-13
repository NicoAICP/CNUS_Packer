
using System;
using System.Collections.Generic;
using System.Text;

namespace CNUS_packer.utils
{
    public class AppXMLInfo
    {
        private int version = 0;
        private long osVersion = 0x0L;
        private long titleID = 0x0L;
        private short titleVersion = 0;
        private int sdkVersion = 0;
        private uint appType = 0x0;
        private short groupID = 0;
        private byte[] osMask = new byte[32];
        private long common_id = 0x0L;
        public AppXMLInfo()
        {

        }

        public int GetVersion()
        {
            return version;
        }

        public void SetVersion(int value)
        {
            version = value;
        }

        public long GetOsVersion()
        {
            return osVersion;
        }

        public void SetOsVersion(long value)
        {
            osVersion = value;
        }

        public long GetTitleID()
        {
            return titleID;
        }

        public void SetTitleID(long value)
        {
            titleID = value;
        }

        public short GetTitleVersion()
        {
            return titleVersion;
        }

        public void SetTitleVersion(short value)
        {
            titleVersion = value;
        }

        public int GetSdkVersion()
        {
            return sdkVersion;
        }

        public void SetSdkVersion(int value)
        {
            sdkVersion = value;
        }

        public uint GetAppType()
        {
            return appType;
        }

        public void SetAppType(uint value)
        {
            appType = value;
        }

        public short GetGroupID()
        {
            return groupID;
        }

        public void SetGroupID(short value)
        {
            groupID = value;
        }

        public byte[] GetOSMask1()
        {
            return osMask;
        }

        public void SetOSMask1(byte[] value)
        {
            osMask = value;
        }

        public long GetCommon_id()
        {
            return common_id;
        }

        public void SetCommon_id(long value)
        {
            common_id = value;
        }
        public string toString()
        {
            return "AppXMLInfo [version=" + version + ", OSVersion=" + osVersion + ", titleID=" + titleID
                + ", titleVersion=" + titleVersion + ", SDKVersion=" + sdkVersion + ", appType=" + appType
                + ", groupID=" + groupID + ", OSMask=" + osMask.ToString() + ", common_id=" + common_id + "]";
        }
    }
}
