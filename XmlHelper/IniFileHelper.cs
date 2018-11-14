using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace XmlHelper
{
    public class IniFileHelper
    {
        public string path;

        public IniFileHelper()
        {
        }

        public IniFileHelper(string INIPath)
        {
            this.path = INIPath;
        }

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        public string IniReadValue(string Section, string Key)
        {
            StringBuilder retVal = new StringBuilder(0xff);
            GetPrivateProfileString(Section, Key, "", retVal, 0xff, this.path);
            return retVal.ToString();
        }

        public void IniWriteValue(string Section, string Key, object Value)
        {
            WritePrivateProfileString(Section, Key, Value.ToString(), this.path);
        }

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
    }

    public sealed class AttributeParameter
    {
        // Fields
        private string name;
        private string value;

        // Methods
        public AttributeParameter()
        { }
        public AttributeParameter(string attributeName, string attributeValue)
        {
            this.name = attributeName;
            this.value = attributeValue;
        }

        // Properties
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
        public string Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }
    }

}
