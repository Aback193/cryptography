using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
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
    public partial class MainWindow : System.Windows.Window
    {
        List<string> docConversionFilePaths = new List<string>();
        List<string> docxConversionFilePaths = new List<string>();
        List<string> pdfConversionFilePaths = new List<string>();
        string path = Directory.GetCurrentDirectory();
        string folderForFileCopy = "\\TransferedFiles";

        public MainWindow()
        {
            InitializeComponent();            
        }
        public void toogleElemets(bool isEnabled)
        {
            btnFind.IsEnabled = isEnabled;
            btnConvert.IsEnabled = isEnabled;
            btnSearch.IsEnabled = isEnabled;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {                
                var b = new BuildFileList();

                toogleElemets(false);

                // Get current directory & make new directory for file transfer if non existent
                
                string targetD = path + folderForFileCopy;
                if (!Directory.Exists(targetD))
                {
                    Directory.CreateDirectory(targetD);
                }
                else if (File.Exists(path + "\\TransferedFilesPaths.txt"))
                {
                    File.Delete(path + "\\TransferedFilesPaths.txt");
                }

                Thread trSearch = new Thread(() =>
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    var files = b.GetFiles();
                    sw.Stop();

                    for (int i = 0; i < files.Count(); i++)
                    {
                        // Update UI thread TextBox with paths. Just for easy testing
                        Dispatcher.Invoke((Action)delegate ()
                        {
                            tb1.Text = tb1.Text + files[i].DirectoryName.ToString() + "\\" + files[i].ToString() + "\n";
                        });
                    }

                    for (int i = 0; i < files.Count(); i++)
                    {
                        string sourceFile = System.IO.Path.Combine(files[i].DirectoryName.ToString(), files[i].ToString());
                        string destFile = System.IO.Path.Combine(targetD, files[i].ToString());

                        // Copy file to another location and overwrite the destination file
                        try
                        {
                            File.Copy(sourceFile, destFile, true);
                        }
                        catch (Exception)
                        {
                            Trace.WriteLine("Exception, file copy");
                        }

                        // Save all file paths to txt
                        string transferedFilesPathSave = path + "\\TransferedFilesPaths.txt";
                        using (StreamWriter w = File.AppendText(transferedFilesPathSave))
                        {
                            w.WriteLine(sourceFile);
                        }

                        // Save file paths for conversion
                        if (sourceFile.Substring(sourceFile.Length - 4).ToLower() == ".pdf" && !pdfConversionFilePaths.Contains(destFile))
                        {
                            pdfConversionFilePaths.Add(destFile);
                        }
                        else if (sourceFile.Substring(sourceFile.Length - 4).ToLower() == ".doc" && !docConversionFilePaths.Contains(destFile))
                        {
                            docConversionFilePaths.Add(destFile);
                        }
                        else if (sourceFile.Substring(sourceFile.Length - 5).ToLower() == ".docx" && !docxConversionFilePaths.Contains(destFile))
                        {
                            docxConversionFilePaths.Add(destFile);
                        }
                    }
                });
                trSearch.Start();
                new Thread(() =>
                {
                    while (trSearch.IsAlive)
                    {
                    }
                    Dispatcher.Invoke((Action)delegate ()
                    {
                        toogleElemets(true);
                    });                    
                }).Start();                
            }
            catch (System.UnauthorizedAccessException)
            {
                Trace.WriteLine("UnauthorizedAccessException");
            }
        }

        public class BuildFileList
        {
            List<string> excludeDirs = new List<string>() { "C:\\Windows", "C:\\Recovery", "C:\\Program Files", "C:\\ProgramData" };

            // Find all Drives
            string[] drives = Directory.GetLogicalDrives();

        public List<FileInfo> GetFiles()
            {
                var files = new List<FileInfo>();
                foreach (string drive in drives)
                {
                    Trace.WriteLine("ALL WINDOWS DRIVES: " + drive);
                    var di = new DirectoryInfo(drive);
                    var directories = di.GetDirectories();
                    foreach (var directoryInfo in directories)
                    {
                        try
                        {
                            if (!excludeDirs.Any( s => directoryInfo.FullName.ToString().Contains(s)))
                            {
                                Trace.WriteLine("directoryInfo.FullName: " + directoryInfo.FullName.ToString());
                                GetFilesFromDirectory(directoryInfo.FullName, files);
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine(ex.Message);
                        }
                    }
                }
                return files;
            }

            private void GetFilesFromDirectory(string directory, List<FileInfo> files)
            {
                var di = new DirectoryInfo(directory);
                var extensions = new List<string> { ".txt", ".pgn", ".pdf", ".docx", ".doc" };
                var fs = di.GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(f => extensions.Contains(f.Extension.ToLower())).ToArray();
                files.AddRange(fs);
                var directories = di.GetDirectories();
                foreach (var directoryInfo in directories)
                {
                    try
                    {
                        if (!excludeDirs.Any( s => directoryInfo.FullName.ToString().Contains(s)))
                        {
                            Trace.WriteLine("directoryInfo.FullName: " + directoryInfo.FullName.ToString());
                            GetFilesFromDirectory(directoryInfo.FullName, files);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        // PDF to Text
        public static string ExtractTextFromPdf(string pdfFile)
        {
            try
            {
                using (iTextSharp.text.pdf.PdfReader reader = new PdfReader(pdfFile))
                {
                    StringBuilder text = new StringBuilder();

                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
                    }
                    return text.ToString();
                }
            }
            catch (Exception)
            {
                Trace.WriteLine("PDF to Text Exception");
                return "";
            }            
        }


        public string ExtractTextFromPdfWithOCR(string pdfFile)
        {
            StringBuilder text = new StringBuilder();
            try
            {
                using (var engine = new TesseractEngine(@"./testdata", "eng", EngineMode.Default))
                {
                    using (iTextSharp.text.pdf.PdfReader pdf = new PdfReader(pdfFile))
                    {

                        for (int pageNumber = 1; pageNumber <= pdf.NumberOfPages; pageNumber++)
                        {
                            text.Append(PdfTextExtractor.GetTextFromPage(pdf, pageNumber));
                            PdfDictionary pg = pdf.GetPageN(pageNumber);

                            IList<System.Drawing.Image> listImages = GetImagesFromPdfDict(pg, pdf);
                            if (listImages == null)
                            {
                                continue;
                            }
                            var imageNumber = 1;
                            foreach (var obj in listImages)
                            {

                                try
                                {
                                    var bmp = new Bitmap(obj);
                                    var img = PixConverter.ToPix(bmp);
                                    using (var image = engine.Process(img))
                                    {
                                        var textFromImage = image.GetText();

                                        text.Append(textFromImage);

                                    }
                                }
                                catch (Exception ex)
                                {
                                    Trace.WriteLine("Execption for image on page:" + pageNumber + "Image Number:" + imageNumber);
                                }
                                imageNumber++;
                            }

                        }
                    }
                }
            }
            catch (Exception)
            {
                Trace.WriteLine("PDF to Text Exception");
            }

            return text.ToString();
        }

        // DOCX to Text with DocX library
        public string ExtractTextFromDocX(string filePath)
        {
            try
            {
                var docFile = DocX.Load(filePath);
                return docFile.Text.ToString();
            }
            catch (Exception)
            {
                Trace.WriteLine("DOCX to Text Exception");
                return "";
            }            
        }

        // DOC to Text with Microsoft.Office.Interop.Word. Word must be installed
        public string ExtractTextFromDoc(string filePath)
        {
            try
            {
                Microsoft.Office.Interop.Word.Application app = new Microsoft.Office.Interop.Word.Application();
                Document doc = app.Documents.Open(filePath);
                string docFileText = doc.Content.Text;
                app.Quit();
                return docFileText.ToString();
            }
            catch (Exception)
            {
                Trace.WriteLine("DOC to Text Exception");
                return "";
            }
        }

        // Document conversion
        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            toogleElemets(false);

            Thread conversion = new Thread(() =>
            {
                String[] pdfFiles = pdfConversionFilePaths.ToArray();
                String[] docFiles = docConversionFilePaths.ToArray();

                for (int j = 0; j < docConversionFilePaths.Count(); j++)
                {
                    Trace.WriteLine("docConversionFilePaths: " + docConversionFilePaths[j].ToString());

                    string docText = ExtractTextFromDoc(docConversionFilePaths[j].ToString());
                    string docToTextFile = docConversionFilePaths[j].ToString() + ".txt";
                    if (docText != "")
                    {
                        using (StreamWriter w = File.AppendText(docToTextFile))
                        {
                            w.WriteLine(docText);
                        }
                    }
                }

                for (int k = 0; k < docxConversionFilePaths.Count(); k++)
                {
                    Trace.WriteLine("docXConversionFilePaths: " + docxConversionFilePaths[k].ToString());

                    string docText = ExtractTextFromDocX(docxConversionFilePaths[k].ToString());
                    string docToTextFile = docxConversionFilePaths[k].ToString() + ".txt";
                    if (docText != "")
                    {
                        using (StreamWriter w = File.AppendText(docToTextFile))
                        {
                            w.WriteLine(docText);
                        }
                    }
                }

                for (int i = 0; i < pdfConversionFilePaths.Count(); i++)
                {
                    Trace.WriteLine("pdfConversionFilePaths: " + pdfConversionFilePaths[i].ToString());

                    string pdfText = ocrOption.IsChecked == true ? ExtractTextFromPdfWithOCR(pdfConversionFilePaths[i]) : ExtractTextFromPdf(pdfConversionFilePaths[i]);
                    string pdfToTextFile = pdfConversionFilePaths[i] + ".txt";
                    if (pdfText != "")
                    {
                        using (StreamWriter w = File.AppendText(pdfToTextFile))
                        {
                            w.WriteLine(pdfText);
                        }
                    }
                }
            });
            conversion.Start();
            new Thread(() =>
            {
                while (conversion.IsAlive)
                {
                }
                Dispatcher.Invoke((Action)delegate ()
                {
                    toogleElemets(true);
                });
            }).Start();
        }

        // Class for storing Search Results
        public class SearchResults
        {
            public string FilePath { get; set; }

            public List<string> WordsFound { get; set; }

            public SearchResults()
            {
                WordsFound = new List<string>();
            }
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            toogleElemets(false);

            string targetD = path + folderForFileCopy;
            if (Directory.Exists(targetD))
            {
                string workDir = path + folderForFileCopy + "\\";
                string searchTerm = "";
                if (searchTb.Text.ToString() != "")
                {
                    searchTerm = searchTb.Text.ToString();
                }
                List<string> SearchWords = searchTerm.Split(' ').ToList();

                var files = new List<FileInfo>();
                var resultList = new List<SearchResults>();

                Thread getTxtFiles = new Thread(() =>
                {
                    var di = new DirectoryInfo(workDir);
                    var extensions = new List<string> { ".txt" };
                    var fs = di.GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(f => extensions.Contains(f.Extension.ToLower())).ToArray();
                    files.AddRange(fs);
                });
                getTxtFiles.Start();

                Thread keyWordSearch = new Thread(() =>
                {
                    while (getTxtFiles.IsAlive)
                    {
                    }
                    if (SearchWords.Count() > 0)
                    {
                        foreach (var file in files)
                        {
                            if (File.Exists(workDir + file.Name) && SearchWords.Count() > 0)
                            {
                                SearchWords.ForEach(x => 
                                {
                                    var lines = File.ReadAllLines(workDir + file.Name);

                                    int foundLines;
                                    bool caseSensitive = false;

                                    Dispatcher.Invoke((Action)delegate ()
                                    {
                                        if (cbxCS.IsChecked == true)
                                            caseSensitive = true;
                                    });

                                    if (!caseSensitive)
                                    {
                                        foundLines = lines.Where(y => y.ToLower().Contains(x.ToLower())).Count();
                                    }
                                    else
                                    {
                                        foundLines = lines.Where(y => y.Contains(x)).Count();
                                    }

                                    if(foundLines > 0)
                                    {
                                        if(resultList.Where(z => z.FilePath == file.Name).Count() > 0)
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

                        //sort results
                        resultList.Sort(delegate(SearchResults x,SearchResults y)
                        {
                            if (x.WordsFound.Count() > y.WordsFound.Count()) return -1;
                            else if (x.WordsFound.Count() < y.WordsFound.Count()) return 1;
                            else if (String.Compare(x.FilePath, y.FilePath) < 0) return -1;
                            return 1;
                            
                        });

                        //display results
                        Dispatcher.Invoke((Action)delegate ()
                        {
                            searchResultTB.Items.Clear();
                        });
                        foreach (var result in resultList)
                        {
                            Dispatcher.Invoke((Action)delegate ()
                            {
                                ListViewItem item = new ListViewItem();
                                item.Content = result.FilePath;
                                item.DataContext = result.FilePath;
                                item.MouseDoubleClick += ListItemMouseDoubleClick;
                                item.Content = item.Content.ToString() + "    Words found: "; 
                                result.WordsFound.ForEach(x => item.Content = item.Content.ToString() + " " + x + ", ");
								item.Content = item.Content.ToString().Remove(item.Content.ToString().Length - 2);
                                searchResultTB.Items.Add(item);
                            });
                        }
                    }
                    else
                    {
                        MessageBox.Show("Search term can't be empty");
                    }                    
                });
                keyWordSearch.Start();

                new Thread(() =>
                {
                    while (keyWordSearch.IsAlive)
                    {
                    }
                    Dispatcher.Invoke((Action)delegate ()
                    {
                        toogleElemets(true);
                    });
                }).Start();
            }
            else
            {
                MessageBox.Show("No files were coppied yet! Please click on " + btnFind.Content);
            }
        }

        private void Close_App(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }

        private void ListItemMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            var item = sender as ListViewItem;
            OpenOriginalFile(path + folderForFileCopy + "\\" + item.DataContext.ToString());
        }

        private void OpenOriginalFile(string filename)
        {
            string filepath = path + folderForFileCopy + "\\" + System.IO.Path.GetFileNameWithoutExtension(filename);
            if (File.Exists(filepath))
                System.Diagnostics.Process.Start(filepath); //opens .docx .doc .pdf
            else
                System.Diagnostics.Process.Start(filename); //original file is .txt

        }



        private IList<System.Drawing.Image> GetImagesFromPdfDict(PdfDictionary dict, PdfReader doc)
        {
            List<System.Drawing.Image> images = new List<System.Drawing.Image>();
            PdfDictionary res = (PdfDictionary)(PdfReader.GetPdfObject(dict.Get(PdfName.RESOURCES)));
            PdfDictionary xobj = (PdfDictionary)(PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT)));

            if (xobj != null)
            {
                foreach (PdfName name in xobj.Keys)
                {
                    PdfObject obj = xobj.Get(name);
                    if (obj.IsIndirect())
                    {
                        PdfDictionary tg = (PdfDictionary)(PdfReader.GetPdfObject(obj));
                        PdfName subtype = (PdfName)(PdfReader.GetPdfObject(tg.Get(PdfName.SUBTYPE)));
                        if (PdfName.IMAGE.Equals(subtype))
                        {
                            int xrefIdx = ((PRIndirectReference)obj).Number;
                            PdfObject pdfObj = doc.GetPdfObject(xrefIdx);
                            PdfStream str = (PdfStream)(pdfObj);

                            iTextSharp.text.pdf.parser.PdfImageObject pdfImage =
                                new iTextSharp.text.pdf.parser.PdfImageObject((PRStream)str);
                            System.Drawing.Image img = pdfImage.GetDrawingImage();

                            images.Add(img);
                        }
                        else if (PdfName.FORM.Equals(subtype) || PdfName.GROUP.Equals(subtype))
                        {
                            images.AddRange(GetImagesFromPdfDict(tg, doc));
                        }
                    }
                }
            }

            return images;
        }
    }
}
