using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace XmlHelper
{
    public class XMLHelper
    {
        public static void AddAttribute(string fileFullName, string nodeName, string namespaceOfPrefix, params AttributeParameter[] attps)
        {
            XmlDocument xDoc = xmlDoc(fileFullName);
            XmlNodeList childNodes = xDoc.DocumentElement.ChildNodes;
            for (int i = 0; i < childNodes.Count; i++)
            {
                XmlNode node = GetNode(childNodes[i], nodeName);
                if (node == null)
                {
                    break;
                }
                AddAttribute(xDoc, node, namespaceOfPrefix, attps);
            }
            xDoc.Save(fileFullName);
        }

        public static void AddAttribute(string fileFullName, XmlNode node, string namespaceOfPrefix, params AttributeParameter[] attps)
        {
            XmlDocument xDoc = xmlDoc(fileFullName);
            AddAttribute(xDoc, node, namespaceOfPrefix, attps);
            xDoc.Save(fileFullName);
        }

        public static void AddAttribute(XmlDocument xDoc, string nodeName, string namespaceOfPrefix, params AttributeParameter[] attps)
        {
            XmlNodeList childNodes = xDoc.DocumentElement.ChildNodes;
            for (int i = 0; i < childNodes.Count; i++)
            {
                XmlNode node = GetNode(childNodes[i], nodeName);
                if (node == null)
                {
                    return;
                }
                string str = (namespaceOfPrefix == null) ? null : node.GetNamespaceOfPrefix(namespaceOfPrefix);
                foreach (AttributeParameter parameter in attps)
                {
                    XmlNode node2 = xDoc.CreateNode(XmlNodeType.Attribute, parameter.Name, (str == "") ? null : str);
                    node2.Value = parameter.Value;
                    node.Attributes.SetNamedItem(node2);
                }
            }
        }

        public static void AddAttribute(XmlDocument xDoc, XmlNode node, string namespaceOfPrefix, params AttributeParameter[] attps)
        {
            string str = (namespaceOfPrefix == null) ? null : node.GetNamespaceOfPrefix(namespaceOfPrefix);
            foreach (AttributeParameter parameter in attps)
            {
                XmlNode node2 = xDoc.CreateNode(XmlNodeType.Attribute, parameter.Name, (str == "") ? null : str);
                node2.Value = parameter.Value;
                node.Attributes.SetNamedItem(node2);
            }
        }

        public static void AddAttribute(string fileFullName, string nodeName, string namespaceOfPrefix, string attributeName, string attributeValue)
        {
            XmlDocument xDoc = xmlDoc(fileFullName);
            XmlNodeList childNodes = xDoc.DocumentElement.ChildNodes;
            for (int i = 0; i < childNodes.Count; i++)
            {
                XmlNode node = GetNode(childNodes[i], nodeName);
                if (node == null)
                {
                    break;
                }
                AddAttribute(xDoc, node, namespaceOfPrefix, attributeName, attributeValue);
            }
            xDoc.Save(fileFullName);
        }

        public static void AddAttribute(string fileFullName, XmlNode node, string namespaceOfPrefix, string attributeName, string attributeValue)
        {
            XmlDocument xDoc = xmlDoc(fileFullName);
            AddAttribute(xDoc, node, namespaceOfPrefix, attributeName, attributeValue);
            xDoc.Save(fileFullName);
        }

        public static void AddAttribute(XmlDocument xDoc, string nodeName, string namespaceOfPrefix, string attributeName, string attributeValue)
        {
            XmlNodeList childNodes = xDoc.DocumentElement.ChildNodes;
            for (int i = 0; i < childNodes.Count; i++)
            {
                XmlNode node = GetNode(childNodes[i], nodeName);
                if (node == null)
                {
                    return;
                }
                string str = (namespaceOfPrefix == null) ? null : node.GetNamespaceOfPrefix(namespaceOfPrefix);
                XmlNode node2 = xDoc.CreateNode(XmlNodeType.Attribute, attributeName, (str == "") ? null : str);
                node2.Value = attributeValue;
                node.Attributes.SetNamedItem(node2);
            }
        }

        public static void AddAttribute(XmlDocument xDoc, XmlNode node, string namespaceOfPrefix, string attributeName, string attributeValue)
        {
            string str = (namespaceOfPrefix == null) ? null : node.GetNamespaceOfPrefix(namespaceOfPrefix);
            XmlNode node2 = xDoc.CreateNode(XmlNodeType.Attribute, attributeName, (str == "") ? null : str);
            node2.Value = attributeValue;
            node.Attributes.SetNamedItem(node2);
        }

        private static void AddEveryNode(XmlDocument xDoc, XmlNode parentNode, params XmlParameter[] paras)
        {
            foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
            {
                if (node.Name == parentNode.Name)
                {
                    AppendChild(xDoc, node, paras);
                }
                else
                {
                    foreach (XmlNode node2 in node)
                    {
                        if (node2.Name == parentNode.Name)
                        {
                            AppendChild(xDoc, node2, paras);
                        }
                    }
                }
            }
        }

        public static bool AddNewNode(string fileFullName, string parentName, params XmlParameter[] paras)
        {
            XmlDocument xDoc = xmlDoc(fileFullName);
            XmlNode parentNode = GetNode(xDoc, parentName);
            if (parentNode == null)
            {
                return false;
            }
            if (parentNode.Name == xDoc.DocumentElement.Name)
            {
                XmlNode node2 = xDoc.CreateNode(XmlNodeType.Element, xDoc.DocumentElement.ChildNodes[0].Name, null);
                AppendChild(xDoc, node2, paras);
                xDoc.DocumentElement.AppendChild(node2);
            }
            else
            {
                AddEveryNode(xDoc, parentNode, paras);
            }
            xDoc.Save(fileFullName);
            return true;
        }

        public static bool AddNewNode(string fileFullName, XmlNode parentNode, params XmlParameter[] paras)
        {
            XmlDocument xDoc = xmlDoc(fileFullName);
            if (parentNode.Name == xDoc.DocumentElement.Name)
            {
                XmlNode node = xDoc.CreateNode(XmlNodeType.Element, xDoc.DocumentElement.ChildNodes[0].Name, null);
                AppendChild(xDoc, node, paras);
                xDoc.DocumentElement.AppendChild(node);
            }
            else
            {
                AddEveryNode(xDoc, parentNode, paras);
            }
            xDoc.Save(fileFullName);
            return true;
        }

        public static bool AddNewNode(XmlDocument xDoc, string parentName, params XmlParameter[] paras)
        {
            XmlNode parentNode = GetNode(xDoc, parentName);
            if (parentNode == null)
            {
                return false;
            }
            if (parentNode.Name == xDoc.DocumentElement.Name)
            {
                XmlNode node2 = xDoc.CreateNode(XmlNodeType.Element, xDoc.DocumentElement.ChildNodes[0].Name, null);
                AppendChild(xDoc, node2, paras);
                xDoc.DocumentElement.AppendChild(node2);
            }
            else
            {
                AddEveryNode(xDoc, parentNode, paras);
            }
            return true;
        }

        public static bool AddNewNode(XmlDocument xDoc, XmlNode parentNode, params XmlParameter[] paras)
        {
            if (parentNode.Name == xDoc.DocumentElement.Name)
            {
                XmlNode node = xDoc.CreateNode(XmlNodeType.Element, xDoc.DocumentElement.ChildNodes[0].Name, null);
                AppendChild(xDoc, node, paras);
                xDoc.DocumentElement.AppendChild(node);
            }
            else
            {
                AddEveryNode(xDoc, parentNode, paras);
            }
            return true;
        }

        private static void AppendChild(XmlDocument xDoc, XmlNode parentNode, params XmlParameter[] paras)
        {
            foreach (XmlParameter parameter in paras)
            {
                XmlNode newChild = xDoc.CreateNode(XmlNodeType.Element, parameter.Name, null);
                string str = (parameter.NamespaceOfPrefix == null) ? "" : newChild.GetNamespaceOfPrefix(parameter.NamespaceOfPrefix);
                foreach (AttributeParameter parameter2 in parameter.Attributes)
                {
                    XmlNode node = xDoc.CreateNode(XmlNodeType.Attribute, parameter2.Name, (str == "") ? null : str);
                    node.Value = parameter2.Value;
                    newChild.Attributes.SetNamedItem(node);
                }
                newChild.InnerText = parameter.InnerText;
                parentNode.AppendChild(newChild);
            }
        }

        public static void CreateXMLFile(string fileFullName, string rootName, string elemName, params XmlParameter[] paras)
        {
            XmlDocument xDoc = new XmlDocument();
            XmlNode newChild = xDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xDoc.AppendChild(newChild);
            XmlNode node2 = xDoc.CreateElement(rootName);
            xDoc.AppendChild(node2);
            XmlNode parentNode = xDoc.CreateNode(XmlNodeType.Element, elemName, null);
            AppendChild(xDoc, parentNode, paras);
            node2.AppendChild(parentNode);
            try
            {
                xDoc.Save(fileFullName);
            }
            catch
            {
                throw;
            }
        }

        public static void CreateXMLFile(string fileFullName, string rootName, XmlParameter elemp, params XmlParameter[] paras)
        {
            XmlDocument xDoc = new XmlDocument();
            XmlNode newChild = xDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xDoc.AppendChild(newChild);
            XmlNode node2 = xDoc.CreateElement(rootName);
            xDoc.AppendChild(node2);
            XmlNode parentNode = xDoc.CreateNode(XmlNodeType.Element, elemp.Name, null);
            string str = (elemp.NamespaceOfPrefix == null) ? "" : parentNode.GetNamespaceOfPrefix(elemp.NamespaceOfPrefix);
            foreach (AttributeParameter parameter in elemp.Attributes)
            {
                XmlNode node = xDoc.CreateNode(XmlNodeType.Attribute, parameter.Name, (str == "") ? null : str);
                node.Value = parameter.Value;
                parentNode.Attributes.SetNamedItem(node);
            }
            AppendChild(xDoc, parentNode, paras);
            node2.AppendChild(parentNode);
            try
            {
                xDoc.Save(fileFullName);
            }
            catch
            {
                throw;
            }
        }

        public static void CreateXMLFile(string fileFullName, XmlParameter rootp, string elemName, params XmlParameter[] paras)
        {
            XmlDocument xDoc = new XmlDocument();
            XmlNode newChild = xDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xDoc.AppendChild(newChild);
            XmlNode node2 = xDoc.CreateElement(rootp.Name);
            string str = (rootp.NamespaceOfPrefix == null) ? "" : node2.GetNamespaceOfPrefix(rootp.NamespaceOfPrefix);
            foreach (AttributeParameter parameter in rootp.Attributes)
            {
                XmlNode node = xDoc.CreateNode(XmlNodeType.Attribute, parameter.Name, (str == "") ? null : str);
                node.Value = parameter.Value;
                node2.Attributes.SetNamedItem(node);
            }
            xDoc.AppendChild(node2);
            XmlNode parentNode = xDoc.CreateNode(XmlNodeType.Element, elemName, null);
            AppendChild(xDoc, parentNode, paras);
            node2.AppendChild(parentNode);
            try
            {
                xDoc.Save(fileFullName);
            }
            catch
            {
                throw;
            }
        }

        public static void CreateXMLFile(string fileFullName, XmlParameter rootp, XmlParameter elemp, params XmlParameter[] paras)
        {
            XmlDocument xDoc = new XmlDocument();
            XmlNode newChild = xDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
            xDoc.AppendChild(newChild);
            XmlNode node2 = xDoc.CreateElement(rootp.Name);
            string str = (rootp.NamespaceOfPrefix == null) ? "" : node2.GetNamespaceOfPrefix(rootp.NamespaceOfPrefix);
            foreach (AttributeParameter parameter in rootp.Attributes)
            {
                XmlNode node = xDoc.CreateNode(XmlNodeType.Attribute, parameter.Name, (str == "") ? null : str);
                node.Value = parameter.Value;
                node2.Attributes.SetNamedItem(node);
            }
            xDoc.AppendChild(node2);
            XmlNode parentNode = xDoc.CreateNode(XmlNodeType.Element, elemp.Name, null);
            str = (elemp.NamespaceOfPrefix == null) ? "" : parentNode.GetNamespaceOfPrefix(elemp.NamespaceOfPrefix);
            foreach (AttributeParameter parameter2 in elemp.Attributes)
            {
                XmlNode node5 = xDoc.CreateNode(XmlNodeType.Attribute, parameter2.Name, (str == "") ? null : str);
                node5.Value = parameter2.Value;
                parentNode.Attributes.SetNamedItem(node5);
            }
            AppendChild(xDoc, parentNode, paras);
            node2.AppendChild(parentNode);
            try
            {
                xDoc.Save(fileFullName);
            }
            catch
            {
                throw;
            }
        }

        public static void DeleteNode(string fileFullName, int Index)
        {
            XmlDocument xDoc = xmlDoc(fileFullName);
            DeleteNode(xDoc, Index);
            xDoc.Save(fileFullName);
        }

        public static void DeleteNode(string fileFullName, params XmlNode[] xns)
        {
            XmlDocument xDoc = xmlDoc(fileFullName);
            DeleteNode(xDoc, xns);
            xDoc.Save(fileFullName);
        }

        public static void DeleteNode(XmlDocument xDoc, int Index)
        {
            XmlNodeList childNodes = xDoc.DocumentElement.ChildNodes;
            childNodes[Index].ParentNode.RemoveChild(childNodes[Index]);
        }

        public static void DeleteNode(XmlDocument xDoc, params XmlNode[] xns)
        {
            foreach (XmlNode node in xns)
            {
                foreach (XmlNode node2 in xDoc.DocumentElement.ChildNodes)
                {
                    if (node.Equals(node2))
                    {
                        node2.ParentNode.RemoveChild(node2);
                        break;
                    }
                }
            }
        }

        public static void DeleteNode(string fileFullName, string nodeName, string nodeText)
        {
            XmlDocument xDoc = xmlDoc(fileFullName);
            DeleteNode(xDoc, nodeName, nodeText);
            xDoc.Save(fileFullName);
        }

        public static void DeleteNode(XmlDocument xDoc, string nodeName, string nodeText)
        {
            foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
            {
                if (node.Name == nodeName)
                {
                    if (!(node.InnerText == nodeText))
                    {
                        continue;
                    }
                    node.ParentNode.RemoveChild(node);
                    break;
                }
                XmlNode node2 = GetNode(node, nodeName);
                if ((node2 != null) && (node2.InnerText == nodeText))
                {
                    node2.ParentNode.ParentNode.RemoveChild(node2.ParentNode);
                    break;
                }
            }
        }

        public static XmlNode GetNode(string fileFullName, string nodeName)
        {
            XmlDocument document = xmlDoc(fileFullName);
            if (document.DocumentElement.Name == nodeName)
            {
                return document.DocumentElement;
            }
            foreach (XmlNode node in document.DocumentElement.ChildNodes)
            {
                if (node.Name.ToLower() == nodeName.ToLower())
                {
                    return node;
                }
                XmlNode node2 = GetNode(node, nodeName);
                if (node2 != null)
                {
                    return node2;
                }
            }
            return null;
        }

        public static XmlNode GetNode(XmlDocument xDoc, string nodeName)
        {
            if (xDoc.DocumentElement.Name == nodeName)
            {
                return xDoc.DocumentElement;
            }
            foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
            {
                if (node.Name.ToLower() == nodeName.ToLower())
                {
                    return node;
                }
                XmlNode node2 = GetNode(node, nodeName);
                if (node2 != null)
                {
                    return node2;
                }
            }
            return null;
        }

        public static XmlNode GetNode(XmlDocument xDoc, XmlParameter xpar)
        {
            return GetNode(xDoc, xpar.Name, xpar.InnerText);
        }

        public static XmlNode GetNode(XmlNode node, string nodeName)
        {
            foreach (XmlNode node2 in node)
            {
                if (node2.Name.ToLower() == nodeName.ToLower())
                {
                    return node2;
                }
                XmlNode node3 = GetNode(node2, nodeName);
                if (node3 != null)
                {
                    return node3;
                }
            }
            return null;
        }

        public static XmlNode GetNode(XmlNode node, XmlParameter xpar)
        {
            return GetNode(node, xpar.Name, node.InnerText);
        }

        public static XmlNode GetNode(string fileFullName, int Index, string nodeName)
        {
            XmlNodeList childNodes = xmlDoc(fileFullName).DocumentElement.ChildNodes;
            if (childNodes.Count > Index)
            {
                if (childNodes[Index].Name.ToLower() == nodeName.ToLower())
                {
                    return childNodes[Index];
                }
                foreach (XmlNode node in childNodes[Index])
                {
                    return GetNode(node, nodeName);
                }
            }
            return null;
        }

        public static XmlNode GetNode(XmlDocument xDoc, int Index, string nodeName)
        {
            XmlNodeList childNodes = xDoc.DocumentElement.ChildNodes;
            if (childNodes.Count > Index)
            {
                if (childNodes[Index].Name.ToLower() == nodeName.ToLower())
                {
                    return childNodes[Index];
                }
                foreach (XmlNode node in childNodes[Index])
                {
                    return GetNode(node, nodeName);
                }
            }
            return null;
        }

        public static XmlNode GetNode(XmlDocument xDoc, string nodeName, string innerText)
        {
            foreach (XmlNode node in xDoc.DocumentElement.ChildNodes)
            {
                if ((node.Name.ToLower() == nodeName.ToLower()) && (node.InnerText == innerText))
                {
                    return node;
                }
                XmlNode node2 = GetNode(node, nodeName, innerText);
                if (node2 != null)
                {
                    return node2;
                }
            }
            return null;
        }

        public static XmlNode GetNode(XmlNode node, string nodeName, string innerText)
        {
            foreach (XmlNode node2 in node)
            {
                if ((node2.Name.ToLower() == nodeName.ToLower()) && (node2.InnerText == innerText))
                {
                    return node2;
                }
                XmlNode node3 = GetNode(node2, nodeName, innerText);
                if (node3 != null)
                {
                    return node3;
                }
            }
            return null;
        }

        public static void SetAttribute(XmlElement elem, params AttributeParameter[] attps)
        {
            foreach (AttributeParameter parameter in attps)
            {
                elem.SetAttribute(parameter.Name, parameter.Value);
            }
        }

        public static void SetAttribute(XmlNode node, params AttributeParameter[] attps)
        {
            XmlElement element = (XmlElement)node;
            foreach (AttributeParameter parameter in attps)
            {
                element.SetAttribute(parameter.Name, parameter.Value);
            }
        }

        public static void SetAttribute(string fileFullName, XmlParameter xpara, params AttributeParameter[] attps)
        {
            XmlDocument xDoc = xmlDoc(fileFullName);
            SetAttribute(xDoc, xpara, attps);
            xDoc.Save(fileFullName);
        }

        public static void SetAttribute(XmlDocument xDoc, XmlParameter xpara, params AttributeParameter[] attps)
        {
            XmlElement node = (XmlElement)GetNode(xDoc, xpara);
            if (node != null)
            {
                SetAttribute(node, attps);
            }
        }

        public static void SetAttribute(XmlElement elem, string attributeName, string attributeValue)
        {
            elem.SetAttribute(attributeName, attributeValue);
        }

        public static void SetAttribute(XmlNode node, string attributeName, string attributeValue)
        {
            ((XmlElement)node).SetAttribute(attributeName, attributeValue);
        }

        public static void SetAttribute(string fileFullName, XmlParameter xpara, string attributeName, string newValue)
        {
            XmlDocument xDoc = xmlDoc(fileFullName);
            SetAttribute(xDoc, xpara, attributeName, newValue);
            xDoc.Save(fileFullName);
        }

        public static void SetAttribute(XmlDocument xDoc, XmlParameter xpara, string attributeName, string newValue)
        {
            XmlElement node = (XmlElement)GetNode(xDoc, xpara);
            if (node != null)
            {
                SetAttribute(node, attributeName, newValue);
            }
        }

        public static void UpdateNode(XmlNode node, string nodeText)
        {
            node.InnerText = nodeText;
        }

        public static void UpdateNode(XmlNode node, XmlParameter para)
        {
            node.InnerText = para.InnerText;
            for (int i = 0; i < para.Attributes.Length; i++)
            {
                node.Attributes.Item(i).Value = para.Attributes[i].Value;
            }
        }

        public static void UpdateNode(string fileFullName, int Index, XmlParameter para)
        {
            XmlDocument xDoc = xmlDoc(fileFullName);
            UpdateNode(xDoc, Index, para);
            xDoc.Save(fileFullName);
        }

        public static void UpdateNode(XmlDocument xDoc, int Index, XmlParameter para)
        {
            UpdateNode(GetNode(xDoc, Index, para.Name), para);
        }

        public static void UpdateNode(XmlNode node, int childIndex, string nodeText)
        {
            node.ChildNodes[childIndex].InnerText = nodeText;
        }

        public static void UpdateNode(string fileFullName, int Index, string nodeName, string nodeText)
        {
            XmlDocument xDoc = xmlDoc(fileFullName);
            UpdateNode(xDoc, Index, nodeName, nodeText);
            xDoc.Save(fileFullName);
        }

        public static void UpdateNode(XmlDocument xDoc, int Index, string nodeName, string nodeText)
        {
            UpdateNode(GetNode(xDoc, Index, nodeName), nodeText);
        }

        public static void UpdateNode(string fileFullName, int Index, string nodeName, int childIndex, string nodeText)
        {
            XmlDocument xDoc = xmlDoc(fileFullName);
            UpdateNode(xDoc, Index, nodeName, childIndex, nodeText);
            xDoc.Save(fileFullName);
        }

        public static void UpdateNode(XmlDocument xDoc, int Index, string nodeName, int childIndex, string nodeText)
        {
            UpdateNode(GetNode(xDoc, Index, nodeName), childIndex, nodeText);
        }

        public static XmlDocument xmlDoc(string PathOrString)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                if (File.Exists(PathOrString))
                {
                    document.Load(PathOrString);
                }
                else
                {
                    document.LoadXml(PathOrString);
                }
                return document;
            }
            catch
            {
                return null;
            }
        }
    }
}
