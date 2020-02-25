using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Web;
using System.Net;
using Newtonsoft.Json;
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
    class Translation
    {
        public string code { get; set; }
        public string lang { get; set; }
        public string[] text { get; set; }
    }

    class Program
    {
        public static string Translate(string s, string lang)
        {
            string sMem = s;
            //try
            //{
                if (s.Length > 0)
                {
                    string textOnly = "";
                    string resultText = "";
                    if (s.Contains("[img=music]"))
                    {
                        textOnly = s.Replace("[img=music]", "");
                    }
                    else textOnly = s;

                    WebRequest request = WebRequest.Create("https://translate.yandex.net/api/v1.5/tr.json/translate?"
                        + "key=trnsl.1.1.20200224T171821Z.806d8be2a2696ae7.3e788f2d4e10b3663b5b786d93813515e08b4a34"
                        + "&text=" + Uri.EscapeDataString(textOnly)
                        + "&lang=" + lang);
                    
                    WebResponse response = request.GetResponse();

                    using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                    {
                        string line;

                        if ((line = stream.ReadLine()) != null)
                        {
                            Translation translation = JsonConvert.DeserializeObject<Translation>(line);

                            

                            foreach (string str in translation.text)
                            {
                                resultText += str;
                            }
                            if (s.Contains("[img=music]"))
                            {
                                s = s.Replace(textOnly, resultText);
                            }
                            else s = resultText;
                        }
                    }
                    
                    return s;
                    
                }
                else
                    return "";
            //}
            //catch(Exception e)
            //{
            //    Console.WriteLine("Line \n" + sMem + "\nCould not be translated");
            //    return sMem;
            //}
        }
        
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
            string translationAnswer = "";
            bool translate;
            while (translationAnswer != "y" && translationAnswer!="n") {
                Console.WriteLine("Enable automatic translation? (y/n)");
                translationAnswer = Console.ReadLine();
            }
            if (translationAnswer == "y")
                translate = true;
            else
                translate = false;
            string startLang = "";
            string endLang = "";
            if (translate == true) { 
            Console.WriteLine("Original language: (en, ru, ja, es... etc.)");
            startLang = Console.ReadLine();
            Console.WriteLine("Result language: (en, ru, ja, es... etc.)");
            endLang = Console.ReadLine();
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
            file.Write("//// Localization table\n////Language: CHANGE_HERE\n[Info]\n\tguid = " + guid+"\n\tlanguage = CHANGE_HERE\n\tversion = " + version + "\n\ttranslator = PZParser\n[/Info]\n\n[Translations]\n");
            int counterTotal = linesGroups.Count();
            for(int i=0; i<linesGroups.Count(); i++)
            {
                
                file.WriteLine("[Collection]");
                if (translate)
                {
                    Console.WriteLine("Translating line " + i + "/" + counterTotal);
                    file.WriteLine("// " + linesGroups.ElementAt(i).Key);
                    file.WriteLine("\ttext = " + Translate(linesGroups.ElementAt(i).Key, startLang + "-" + endLang));
                }
                else file.WriteLine("\ttext = " + linesGroups.ElementAt(i).Key);
                foreach (var t in linesGroups.ElementAt(i))
                    file.WriteLine("\tmember = "+t.ID);
                file.WriteLine("[/Collection]");
            }
            file.WriteLine("[/Translations]");
            file.Close();
            Console.WriteLine("Finished");
        }
        
    }
}
