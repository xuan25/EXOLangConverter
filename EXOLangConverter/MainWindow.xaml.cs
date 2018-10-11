using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace EXOLangConverter
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        XmlDocument iFile;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Thread load = new Thread(delegate ()
            {
                iFile = new XmlDocument();
                XmlReaderSettings xSettings = new XmlReaderSettings();
                xSettings.IgnoreComments = true;
                XmlReader xReader = XmlReader.Create(Environment.CurrentDirectory + "\\index.xml", xSettings);
                iFile.Load(xReader);

                XmlNodeList languageNodes = iFile.SelectSingleNode("table").SelectSingleNode("declaration").ChildNodes;
                Dispatcher.Invoke(new Action(() =>
                {
                    foreach(XmlNode lang in languageNodes)
                    {
                        fromBox.Items.Add(lang.Attributes["name"].Value);
                        toBox.Items.Add(lang.Attributes["name"].Value);
                    }
                    fromBox.SelectedIndex = 0;
                    toBox.SelectedIndex = 1;
                    stateBox.Text = "Please drag in files to be converted";
                    this.AllowDrop = true;
                }));
            });
            load.Start();
            
        }

        private string findReplacement(string content, XmlNodeList replacmentNodes, XmlNode from, XmlNode to)
        {
            foreach (XmlNode r in replacmentNodes)
            {
                if(r.Attributes[from.Attributes["mark"].Value].Value == content)
                {
                    return r.Attributes[to.Attributes["mark"].Value].Value;
                }
            }
            return null;
        }

        private string convertEXO(XmlNode indexNode, string originalFile, string newFile, XmlNode from, XmlNode to)
        {
            List<string> reportList = new List<string>();

            StreamReader oFile = new StreamReader(originalFile, Encoding.GetEncoding(from.Attributes["charset"].Value));
            string content = oFile.ReadToEnd();
            oFile.Close();

            XmlNodeList replacmentNodes = indexNode.ChildNodes;
            string[] lines = content.Split(new char[] { '\n' });
            for (int i= 0; i<lines.LongLength; i++)
            {
                if (!lines[i].StartsWith("[") && lines[i] != "")
                {
                    string[] p = lines[i].Split(new char[] { '=' });
                    if (lines[i].StartsWith("_name"))
                    {
                        string re = findReplacement(p[1].Trim(), replacmentNodes, from, to);
                        if(re != null)
                        {
                            p[1] = p[1].Replace(p[1].Trim(), re);
                        }
                        else
                        {
                            reportList.Add("Cannot found Object: " + p[1].Trim());
                        }

                    }
                    else
                    {
                        if (p[0].Trim() != "")
                        {
                            string re = findReplacement(p[0].Trim(), replacmentNodes, from, to);
                            if (re != null)
                            {
                                p[0] = p[0].Replace(p[0].Trim(), re);
                            }
                            else
                            {
                                reportList.Add("Cannot found Attribute: " + p[0].Trim());
                            }
                        }
                    }
                    lines[i] = p[0] + "=" + p[1];
                }
            }
            content = string.Join("\n", lines);

            StreamWriter nFile = new StreamWriter(newFile, false, Encoding.GetEncoding(to.Attributes["charset"].Value));
            nFile.Write(content);
            nFile.Close();

            if (reportList.Count != 0)
            {
                string report = "  " + string.Join("\n  ", reportList.Distinct<string>().ToArray<string>()) + "\n";
                return report;
            }
            else
            {
                return "";
            }
            
            
        }

        private void Window_PreviewDrop(object sender, DragEventArgs e)
        {
            stateBox.Text = "Processing...";
            this.AllowDrop = false;
            string fromName = fromBox.SelectedItem.ToString();
            string toName = toBox.SelectedItem.ToString();

            Thread process = new Thread(delegate ()
            {
                string report = "";
                Array pathArr = (Array)e.Data.GetData(DataFormats.FileDrop);
                foreach (string path in pathArr)
                {
                    if (path.ToLower().EndsWith(".exo"))
                    {
                        XmlNode from = null, to = null;
                        foreach(XmlNode i in iFile.SelectSingleNode("table").SelectSingleNode("declaration").ChildNodes)
                        {
                            if(i.Attributes["name"].Value == fromName)
                            {
                                from = i;
                            }
                            if (i.Attributes["name"].Value == toName)
                            {
                                to = i;
                            }
                        }
                        if(from != null && to != null)
                        {
                            report += convertEXO(iFile.SelectSingleNode("table").SelectSingleNode("index"), path, path.Substring(0, path.Length - 4) + "_new.exo", from, to);
                            report += "Completed: " + path.Substring(path.LastIndexOf("\\") + 1) + "\n";
                        }
                    }
                    else
                    {
                        report += "Skiped: " + path.Substring(path.LastIndexOf("\\") + 1) + "\n";
                    }
                }
                Dispatcher.Invoke(new Action(() =>{
                    stateBox.Text = "Please drag in files to be converted";
                    this.AllowDrop = true;
                    MessageBoxEx.Show(this, report, "Conversion completed");
                }));
            });
            process.Start();
            
        }

    }
}
