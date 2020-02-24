using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace PZParser
{
    class LineEntry
    {
        public string Line;
        public string ID;
        public LineEntry(string line, string id)
        {
            this.Line = line;
            this.ID = id;
        }
    }


    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            string filePath = "";
            Console.WriteLine("Select radio .XML file\n");
            while (filePath == "")
            {
                OpenFileDialog ofd = new OpenFileDialog();
                using (ofd)
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        filePath = ofd.FileName;
                    }
                }
                if(filePath != "")
                {
                    Console.WriteLine("Loading: " + filePath+"\n\n");
                    
                }
            }
            string resultFilePath = "";
            Console.WriteLine("Select result file folder\n");
            while (resultFilePath == "")
            {
                FolderBrowserDialog ofd = new FolderBrowserDialog();
                using (ofd)
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        resultFilePath = ofd.SelectedPath + "\\RadioTranslationTemplate.txt";
                    }
                }
                if (resultFilePath != "")
                {
                    Console.WriteLine("The result file will be stored as: " + resultFilePath + "\n\n");

                }
            }


            XmlDocument xRadio = new XmlDocument();
            xRadio.Load(filePath);
            XmlNodeList guidTemp = xRadio.GetElementsByTagName("FileGUID");
            string guid = guidTemp.Item(0).InnerXml;
            XmlNodeList versionTemp = xRadio.GetElementsByTagName("Version");
            string version = versionTemp.Item(0).InnerXml;
            XmlNodeList elemList = xRadio.GetElementsByTagName("LineEntry");
            List<LineEntry> linesList = new List<LineEntry>();
            for (int i = 0; i < elemList.Count; i++)
            {
                linesList.Add(new LineEntry(elemList[i].InnerXml, elemList[i].Attributes["ID"].Value));
                
            }
            var linesGroups = from lineEntry in linesList group lineEntry by lineEntry.Line;


            StreamWriter file = new StreamWriter(resultFilePath);
            file.Write("[Info]\n\tguid = " + guid+"\n\tlanguage = EN\n\tversion = " + version + "\n\ttranslator = PZParser\n[/Info]\n\n[Translations]\n");
            
            foreach(IGrouping<string, LineEntry> g in linesGroups)
            {
                file.WriteLine("[Collection]");
                file.WriteLine("\ttext = "+g.Key);
                foreach (var t in g)
                    file.WriteLine("\tmember = "+t.ID);
                file.WriteLine("[/Collection]");
            }
            file.WriteLine("[/Translations]");
            file.Close();
            Console.WriteLine("Finished");
        }
        
    }
}
