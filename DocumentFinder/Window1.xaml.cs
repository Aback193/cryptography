using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
using Xceed.Words.NET;      // https://www.c-sharpcorner.com/article/generate-word-document-using-c-sharp/
using Application = Microsoft.Office.Interop.Word.Application;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using Tesseract;

namespace DocumentFinder
{

    public partial class Window1 : System.Windows.Window
    {

        public MainWindow parent { get; set; }
        public string workDir { get; set; }
        public string searchTerm { get; set; }
        public bool caseSensitive { get; set; }
        public List<FileInfo> files { get; set; }
        public string task { get; set; }


        public Window1()
        {
            InitializeComponent();
        }


        private void Stop_Progress(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)delegate ()
            {
                this.Close();
            });
            //ovo treba da se ispravi
        }

        private void ExecuteTask()
        {
            switch(task)
            {
                case "search": { search(); break; }
                case "convert": { break; }
                case "scan": { break; }
                case "scan and convert": { break; }
            }
        }

        private void search()
        {
            this.Dispatcher.Invoke((Action)delegate ()
            {
                TitleBox.Text = "Searching for: " + searchTerm;
            });

            var resultList = new List<SearchResults>();

            if (searchTerm != "")
            {
                List<string> SearchWords = new List<string>();
                SearchWords = searchTerm.Split(' ').ToList();
                int counter = 1;

                foreach (var file in files)
                {

                    Dispatcher.Invoke(() =>
                    {
                        txtCounter.Text = counter.ToString() + ". " + file.Name;
                    });
                    counter++;
                    if (File.Exists(workDir + file.Name) && SearchWords.Count() > 0)
                    {
                        SearchWords.ForEach(x =>
                        {
                            var lines = File.ReadAllLines(workDir + file.Name);

                            int foundLines;

                            if (!caseSensitive)
                            {
                                foundLines = lines.Where(y => y.ToLower().Contains(x.ToLower())).Count();
                            }
                            else
                            {
                                foundLines = lines.Where(y => y.Contains(x)).Count();
                            }

                            if (foundLines > 0)
                            {
                                if (resultList.Where(z => z.FilePath == file.Name).Count() > 0)
                                {
                                    resultList.Where(z => z.FilePath == file.Name).ToList().First().WordsFound.Add(x);
                                }
                                else
                                {
                                    SearchResults AddResult = new SearchResults();
                                    AddResult.FilePath = file.Name;
                                    AddResult.WordsFound.Add(x);

                                    resultList.Add(AddResult);
                                }
                            }
                        });
                    }

                }

            }
            parent.resultList = resultList;

            this.Dispatcher.Invoke((Action)delegate ()
            {
                this.Close();
            });
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            new Thread(() =>
            {
                ExecuteTask();

            }).Start();
        }
    }
}
