using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace SecureBox.BL
{
    class XmlDriveSettings
    {
        private string path;
        private const string startElem = "settings";
        private const string passwordElem = "encodedPassword";
        private const string keyElem = "encodedKey";
        private const string md5passwordElem = "md5Password";
        private const string settingsFile = "\\.settings.xml";
        private const string emptyElemValue = "0";
        XmlDocument xmlDoc;


        public XmlDriveSettings(string root)
        {
            path = root + settingsFile;
            xmlDoc = new XmlDocument();
            if (!File.Exists(path))
            {
                CreateSettings();
            }
            xmlDoc.Load(path);
        }

        public string ReadPassword()
        {
            XmlNode node = findElem(passwordElem);
            return node.InnerText;
        }

        public string ReadKey()
        {
            XmlNode node = findElem(keyElem);
            return node.InnerText;
        }

        public string ReadMd5Password()
        {
            XmlNode node = findElem(md5passwordElem);
            return node.InnerText;
        }

        public void WritePassword(string password)
        {
            XmlNode node = findElem(passwordElem);
            node.InnerText = password;
            xmlDoc.Save(path);
        }

        public void WriteKey(string key)
        {
            XmlNode node = findElem(keyElem);
            node.InnerText = key;
            xmlDoc.Save(path);
        }

        public void WriteMd5Password(string md5Password)
        {
            XmlNode node = findElem(md5passwordElem);
            node.InnerText = md5Password;
            xmlDoc.Save(path);
        }

        private void CreateSettings()
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create(path, xmlSettings))
            {
                writer.WriteStartElement(startElem);
                writer.WriteElementString(passwordElem, emptyElemValue);
                writer.WriteElementString(keyElem, emptyElemValue);
                writer.WriteElementString(md5passwordElem, emptyElemValue);
                writer.WriteEndElement();
                writer.Flush();
            }
        }

        private XmlNode findElem(string elemName)
        {
            foreach (XmlNode table in xmlDoc.DocumentElement.ChildNodes)
            {
                if (table.Name == elemName)
                {
                    return table;
                }
            }

            return null; //new XmlElement();
        }
    }
}
