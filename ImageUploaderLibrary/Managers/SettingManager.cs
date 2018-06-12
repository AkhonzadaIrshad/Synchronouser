using System;
using System.IO;
using System.Linq;
using System.Xml;

namespace ImageUploaderLibrary.Managers
{
    public class SettingHelper
    {
        public SettingHelper()
        {
            const string settingFile = "Settings.xml";
            Document = new XmlDocument();
            if (!new FileInfo(settingFile).Exists)
            {
                Document.LoadXml(
                    $"<CustomImageDirectory>{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}</CustomImageDirectory>");
                Document.Save(settingFile);
            }
            else
            {
                Document.Load(settingFile);
            }
        }

        private XmlDocument Document { get; }

        public string CustomPath
        {
            get
            {
                var data = Document.GetElementsByTagName("CustomImageDirectory").OfType<XmlElement>().First();
                return data.InnerText;
            }
            set
            {
                var data = Document.GetElementsByTagName("CustomImageDirectory").OfType<XmlElement>().First();
                data.InnerText = value;
                Save();
            }
        }

        private void Save()
        {
            Document.Save("Settings.xml");
        }
    }
}