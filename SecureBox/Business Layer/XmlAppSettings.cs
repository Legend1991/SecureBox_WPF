using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace SecureBox.BL
{
    class XmlAppSettings
    {
        private const string startElem = "settings";
        private const string driveElem = "drive";
        private const string letterElem = "letter";
        private const string labelElem = "label";
        private const string mountedElem = "mounted";
        private const string rootElem = "root";
        private const string settingsFile = "\\appsettings.xml";
        private const string emptyElemValue = "0";
        private const int firstLetter = 0;
        private const char mountedDrive = '1';
        XmlDocument xmlDoc;


        public XmlAppSettings(string root)
        {
            xmlDoc = new XmlDocument();
            if (!File.Exists(settingsFile))
            {
                CreateSettings();
            }
            xmlDoc.Load(settingsFile);
        }

        public void AddDrive(DriveInfo drive)
        {
            XmlNode driveNode = xmlDoc.CreateElement(driveElem);

            XmlNode letterNode = xmlDoc.CreateElement(letterElem);
            letterNode.InnerText = drive.Letter.ToString(); ;
            driveNode.AppendChild(letterNode);

            XmlNode labelNode = xmlDoc.CreateElement(labelElem);
            labelNode.InnerText = drive.Label;
            driveNode.AppendChild(labelNode);

            XmlNode rootNode = xmlDoc.CreateElement(rootElem);
            rootNode.InnerText = drive.Root;
            driveNode.AppendChild(rootNode);

            XmlNode mountedNode = xmlDoc.CreateElement(mountedElem);
            mountedNode.InnerText = drive.Mounted ? mountedDrive.ToString() : emptyElemValue;
            driveNode.AppendChild(mountedNode);

            xmlDoc.DocumentElement.AppendChild(driveNode);
            xmlDoc.Save(settingsFile);
        }

        public List<DriveInfo> GetDrives()
        {
            List<DriveInfo> drives = new List<DriveInfo>();

            foreach (XmlNode table in xmlDoc.DocumentElement.ChildNodes)
            {
                char letter = emptyElemValue[firstLetter];
                string label = emptyElemValue;
                string root = emptyElemValue;
                bool mounted = false;

                foreach (XmlNode ch in table.ChildNodes)
                {
                    switch (ch.Name)
                    {
                        case letterElem:
                            letter = ch.InnerText[firstLetter];
                            break;
                        case labelElem:
                            label = ch.InnerText;
                            break;
                        case rootElem:
                            root = ch.InnerText;
                            break;
                        case mountedElem:
                            mounted = ch.InnerText[firstLetter] == mountedDrive;
                            break;
                    }
                }
                drives.Add(new DriveInfo(letter, label, root, mounted));
            }

            return drives;
        }

        public bool RemoveDrive(DriveInfo drive)
        {
            foreach (XmlNode table in xmlDoc.DocumentElement.ChildNodes)
            {
                string root = string.Empty;
                foreach (XmlNode ch in table.ChildNodes)
                {
                    if (ch.Name == rootElem)
                    {
                        root = ch.InnerText;
                        break;
                    }
                }
                if (drive.Root == root)
                {
                    xmlDoc.DocumentElement.RemoveChild(table);
                    xmlDoc.Save(settingsFile);
                    return true;
                }
            }
            return false;
        }

        public bool UnMountDrive(DriveInfo drive, bool forMount)
        {
            foreach (XmlNode table in xmlDoc.DocumentElement.ChildNodes)
            {
                string root = string.Empty;
                foreach (XmlNode ch in table.ChildNodes)
                {
                    if (ch.Name == rootElem)
                    {
                        root = ch.InnerText;
                        break;
                    }
                }
                if (drive.Root == root)
                {
                    foreach (XmlNode ch in table.ChildNodes)
                    {
                        if (ch.Name == mountedElem)
                        {
                            ch.InnerText = forMount ? mountedDrive.ToString() : emptyElemValue;
                            break;
                        }
                    }
                    xmlDoc.Save(settingsFile);
                    return true;
                }
            }
            return false;
        }

        private void CreateSettings()
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;

            using (XmlWriter writer = XmlWriter.Create(settingsFile, xmlSettings))
            {
                writer.WriteStartElement(startElem);
                writer.WriteEndElement();
                writer.Flush();
            }
        }
    }
}
