using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace XmlHelper
{
    public class XmlSerializerHelper
    {
        public static object ReadXML(string XmlFilePath, Type type,out bool flag )
        {
            flag = true;
            object obj2 = new object();
            XmlSerializer serializer = new XmlSerializer(type);
            if (!File.Exists(XmlFilePath))
            {
                return new object();
            }
            FileStream stream = null; 
            try
            {
                stream= new FileStream(XmlFilePath, FileMode.Open);
                obj2 = serializer.Deserialize(stream);
            }
            catch(Exception e)
            {
                flag = false;
            }
            finally
            {
                if(stream !=null)
                {
                    stream.Close();
                }      
            }
            return obj2;
        }

        public static bool WriteXML(object myDs, string XmlFilePath, Type type)
        {
            bool flag = true;
            StreamWriter writer = null;
            XmlSerializer serializer = new XmlSerializer(type);
            try
            {
                writer = new StreamWriter(XmlFilePath, false);
                serializer.Serialize((TextWriter)writer, myDs);
            }
            catch (Exception e)
            {
                flag = false;
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
            return flag;
        }
    }
}
