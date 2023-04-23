using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace WPF进程管理器
{
    internal class XmlWith
    {
        static string xmlPath = "process.xml";
        /// <summary>
        /// 检查 XML 文件是否存在,不存在则创建
        /// </summary>
        /// <returns>存在返回true，不存在返回false</returns>
        public static bool CheckXmlFile()
        {
            if (File.Exists(xmlPath)) return true;
            else 
                using (XmlWriter writer = XmlWriter.Create(xmlPath))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("root");
                    writer.WriteAttributeString("AppBackground", "null"); // 在开始标签中写入属性 
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
            return false;
        }
        // 读取 XML 文件,打印所有节点
        public static Dictionary<string, string> ReadXmlFile()
        {
            if (!CheckXmlFile()) return null;
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);
            XmlNode root = doc.SelectSingleNode("root");
            XmlNodeList nodes = root.ChildNodes;
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (XmlNode node in nodes)
            {
                if (node.Name.Equals("AppBackground")) continue;
                dict.Add(node.Name, node.Attributes["value"].Value);
            }
            return dict;
        }

        // 将标签和值写入 XML 文件
        public static void WriteToXmlFile(string tag, string value)
        {
            CheckXmlFile();  //检查
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);
            XmlElement root = doc.DocumentElement;
            XmlElement newElement = doc.CreateElement(tag);
            newElement.SetAttribute("value", value);
            root.AppendChild(newElement);

            doc.Save(xmlPath);
        }
        //删除节点
        public static void DeleteToXmlFile(string name)
        {
            if(!CheckXmlFile()) return;
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);  // 加载 XML 文件
            XmlNode root = doc.SelectSingleNode("root");
            XmlNode newElement = root.SelectSingleNode(name);
            root.RemoveChild(newElement);  // 移除节点
            doc.Save(xmlPath);  // 保存修改后的 XML
        }
        //获取背景
        public static string GetBackgroundPath()
        {
            if (!CheckXmlFile()) return null;
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);  // 加载 XML 文件
            XmlNode root = doc.SelectSingleNode("root");
            return root.Attributes["AppBackground"].Value;
        }
        //改变背景
        public static void ChangeBackgroundPath(string backgroundPath)
        {
            CheckXmlFile();
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);  // 加载 XML 文件
            XmlNode root = doc.SelectSingleNode("root");
            root.Attributes["AppBackground"].Value = backgroundPath;
            doc.Save(xmlPath);
        }
    }
}
