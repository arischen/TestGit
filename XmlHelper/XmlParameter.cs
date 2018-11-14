using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XmlHelper
{
    public sealed class XmlParameter
    {
        private AttributeParameter[] attributes;
        private string innerText;
        private string name;
        private string namespaceOfPrefix;

        public XmlParameter()
        {
            this.namespaceOfPrefix = null;
        }

        public XmlParameter(string name, params AttributeParameter[] attParas)
        {
            this.name = name;
            this.namespaceOfPrefix = null;
            this.attributes = attParas;
        }

        public XmlParameter(string name, string innerText, params AttributeParameter[] attParas)
        {
            this.name = name;
            this.innerText = innerText;
            this.namespaceOfPrefix = null;
            this.attributes = attParas;
        }

        public XmlParameter(string name, string innerText, string namespaceOfPrefix, params AttributeParameter[] attParas)
        {
            this.name = name;
            this.innerText = innerText;
            this.namespaceOfPrefix = namespaceOfPrefix;
            this.attributes = attParas;
        }

        public AttributeParameter[] Attributes
        {
            get
            {
                return this.attributes;
            }
            set
            {
                this.attributes = value;
            }
        }

        public string InnerText
        {
            get
            {
                return this.innerText;
            }
            set
            {
                this.innerText = value;
            }
        }

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

        public string NamespaceOfPrefix
        {
            get
            {
                return this.namespaceOfPrefix;
            }
            set
            {
                this.namespaceOfPrefix = value;
            }
        }
    }
}
