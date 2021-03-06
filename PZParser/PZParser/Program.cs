﻿using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Web;
using System.Net;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace PZParser
{
    class LineEntry
    {
        public string Line;
        public string ID;
        public int r;
        public int g;
        public int b;
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
            try
            {
                if (s.Length > 0)
                {
                    string textOnly = "";
                    string resultText = "";
                    if (s.Contains("[img=music]"))
                    {
                        return s;
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
            }
            catch(Exception e)
            {
                Console.WriteLine("Line \n" + sMem + "\nCould not be translated");
                return sMem;
            }
        }
        
        [STAThread]
        static void Main(string[] args)
        {
            string filePath = "";
            Console.WriteLine("Select radio translation file\n");
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
            string translate = "y";
            Console.WriteLine("Apply the translation? y/n");
            translate = Console.ReadLine();
            if(translate == "y")
            {
                string startLang = "";
                string endLang = "";
                Console.WriteLine("Original language: (en, ru, ja, es... etc.)");
                startLang = Console.ReadLine();
                Console.WriteLine("Result language: (en, ru, ja, es... etc.)");
                endLang = Console.ReadLine();

                int counter = 0;
                string textBuffer = "";
                string translationBuffer = "";
                int textStartingPosition;
                int textEndPosition;
                string line;

                Regex regex = new Regex(@"\w*ORIGINAL\w*");
                System.IO.StreamReader file =
                new System.IO.StreamReader(filePath);
                StreamWriter fileOutput = new StreamWriter(resultFilePath);
                while ((line = file.ReadLine()) != null)
                {
                    counter++;
                    System.Console.WriteLine("Processing line number " + counter + " : " + line);
                    MatchCollection matches = regex.Matches(line);
                    if (matches.Count > 0)
                    {
                        textBuffer = matches[0].ToString();
                        textStartingPosition = line.IndexOf("]: ") + 3;
                        textEndPosition = line.IndexOf("\n");
                        textBuffer = line.Substring(textStartingPosition, line.Length - textStartingPosition);
                        translationBuffer = Translate(textBuffer, startLang + "-" + endLang);
                        fileOutput.WriteLine(line);
                    }
                    else if (Regex.IsMatch(line, @"\w*[-0-9a-fA-F]{36}\w*", RegexOptions.Compiled) && !line.Contains("guid"))
                    {
                        fileOutput.WriteLine(line + translationBuffer);
                    }
                    else fileOutput.WriteLine(line);

                }


                fileOutput.Close();
                file.Close();

            }
            else
            {
                int counter = 0;
                string textBuffer = "";
                int textStartingPosition;
                int textEndPosition;
                string line;

                Regex regex = new Regex(@"\w*ORIGINAL\w*");
                System.IO.StreamReader file =
                new System.IO.StreamReader(filePath);
                StreamWriter fileOutput = new StreamWriter(resultFilePath);
                while ((line = file.ReadLine()) != null)
                {
                    counter++;
                    System.Console.WriteLine("Processing line number " + counter + " : " + line);
                    MatchCollection matches = regex.Matches(line);
                    if (matches.Count > 0)
                    {
                        textBuffer = matches[0].ToString();
                        textStartingPosition = line.IndexOf("]: ") + 3;
                        textEndPosition = line.IndexOf("\n");
                        textBuffer = line.Substring(textStartingPosition, line.Length - textStartingPosition);
                        fileOutput.WriteLine(textBuffer);
                    }

                }
                fileOutput.Close();
                file.Close();
            }


            Console.WriteLine("Finished");
        }
        
    }
}
