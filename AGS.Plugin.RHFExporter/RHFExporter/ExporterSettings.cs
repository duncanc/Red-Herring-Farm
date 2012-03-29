using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace RedHerringFarm
{
    [XmlRoot(ElementName="Settings")]
    public class ExporterSettings
    {
        private string pngTool;
        public string PngTool
        {
            get { return pngTool; }
            set { pngTool = value; }
        }

        private int maxImageSheetWidth = 2880;
        public int MaxImageSheetWidth
        {
            get { return maxImageSheetWidth; }
            set { maxImageSheetWidth = value; }
        }

        private int maxImageSheetHeight = 2880;
        public int MaxImageSheetHeight
        {
            get { return maxImageSheetHeight; }
            set { maxImageSheetHeight = value; }
        }

        public readonly static ExporterSettings Default;
        static ExporterSettings()
        {
            Default = new ExporterSettings();
        }

        public void WriteXml(XmlWriter output)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ExporterSettings), "");
            serializer.Serialize(output, this);
        }

        public static ExporterSettings ReadXml(XmlNode node)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ExporterSettings), "");
            using (XmlNodeReader nodeReader = new XmlNodeReader(node))
            {
                return (ExporterSettings)serializer.Deserialize(nodeReader);
            }
        }
    }
}
