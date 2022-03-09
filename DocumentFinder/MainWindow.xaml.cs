using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace DocumentFinder
{
    public partial class MainWindow : System.Windows.Window
    {
        HelperMethods helperMethods = new HelperMethods();
        List<string> docConversionFilePaths = new List<string>();
        List<string> docxConversionFilePaths = new List<string>();
        List<string> pdfConversionFilePaths = new List<string>();
        List<string> allConversionFilePaths = new List<string>();
        public static List<string> extensions = new List<string> { ".txt", ".pgn", ".pdf", ".docx", ".doc" };
        public static List<string> excludeDirs = new List<string>() { "C:\\Windows", "C:\\Recovery", "C:\\Program Files", "C:\\ProgramData", "C:\\$Recycle.Bin" };
        public List<SearchResults> resultList = new List<SearchResults>();
        public bool wasScanned = false;
        //public Window1 progressWindow = new Window1();
        //public bool progressWindowIsRunning = false;
        string path = "C:"; //da bude na C DocumentFinder, dodati opciju da moze da se menja
        string folderForFileCopy = "\\TransferedFiles";

        public MainWindow()
        {
            InitializeComponent();
            setBottomStatusBar();
            conversionDestination.Content = "Conversion destination: " + path + folderForFileCopy;
        }

        public void toogleElemets(bool isEnabled)
        {
            btnFind.IsEnabled = isEnabled;
            btnFindAuto.IsEnabled = isEnabled;
            btnConvert.IsEnabled = isEnabled;
            btnSearch.IsEnabled = isEnabled;
            btnPick.IsEnabled = isEnabled;
            ocrOption.IsEnabled = isEnabled;
            cbxCopy.IsEnabled = isEnabled;
            autoScanConvert.IsEnabled = isEnabled;
            cbxCS.IsEnabled = isEnabled;
        }

        public void setBottomStatusBar()
        {
            DriveInfo[] foundDrivesInfo = DriveInfo.GetDrives();
            string outText = "";
            foreach (DriveInfo drive in foundDrivesInfo)
            {
                outText = outText + "  [ " + drive.VolumeLabel + " " + drive.Name + " ]";
            }
            statusBar.Text = "Drives found: " + outText;
        }

        private void btnFindAutoClick(object sender, RoutedEventArgs e)
        {
            scan();
        }
        private void btnFindClick(object sender, RoutedEventArgs e)
        {
            scan();
        }
        private void btnConvertClick(object sender, RoutedEventArgs e)
        {
            convert();
        }
        private void btnSearchClick(object sender, RoutedEventArgs e)
        {
            search();
        }        

        // Scan Folders and Files
        public void scan()
        {
            try
            {
                tb1.Clear();
                wasScanned = true;
                bool copyFilesCheckValid = cbxCopy.IsChecked == true ? true : false;
                BuildFileList b = new BuildFileList(excludeDirs, extensions);

                // Display all Drives on BottomStatusBar
                setBottomStatusBar();

                toogleElemets(false);

                // Get current directory & make new directory for file transfer if non existent
                string targetD = path + folderForFileCopy;
                if (!Directory.Exists(targetD))
                {
                    Directory.CreateDirectory(targetD);
                }
                else if (File.Exists(path + folderForFileCopy + "\\_TransferedFilesPaths.txt"))
                {
                    File.Delete(path + folderForFileCopy + "\\_TransferedFilesPaths.txt");
                }

                Thread trSearch = new Thread(() =>
                {
                    var files = b.GetFiles();
                    for (int i = 0; i < files.Count(); i++)
                    {
                        // Update UI thread TextBox with paths. Just for easy testing
                        this.Dispatcher.Invoke((Action)delegate ()
                        {
                            tb1.Text = tb1.Text + files[i].DirectoryName.ToString() + "\\" + files[i].ToString() + "\n";
                        });
                    }

                    for (int i = 0; i < files.Count(); i++)
                    {
                        string sourceFile = System.IO.Path.Combine(files[i].DirectoryName.ToString(), files[i].ToString());
                        string destFile = System.IO.Path.Combine(targetD, files[i].ToString());

                        // Copy original files to destination if checkbox is checked
                        try
                        {
                            if (copyFilesCheckValid == true)
                            {
                                File.Copy(sourceFile, destFile, true);
                            } 
                        }
                        catch (Exception)
                        {
                            Trace.WriteLine("Exception, file copy");
                        }

                        // Save all file paths to txt
                        string transferedFilesPathSave = path + folderForFileCopy + "\\_TransferedFilesPaths.txt";
                        using (StreamWriter w = File.AppendText(transferedFilesPathSave))
                        {
                            w.WriteLine(sourceFile);                            
                            Trace.WriteLine("SOURCE FILE: " + sourceFile);
                        }

                        // Save file paths for conversion
                        if(sourceFile.Substring(sourceFile.Length - 4).ToLower() != ".txt" && !allConversionFilePaths.Contains(sourceFile))
                            allConversionFilePaths.Add(sourceFile);

                        if (sourceFile.Substring(sourceFile.Length - 4).ToLower() == ".pdf" && !pdfConversionFilePaths.Contains(sourceFile))
                            pdfConversionFilePaths.Add(sourceFile);
                        else if (sourceFile.Substring(sourceFile.Length - 4).ToLower() == ".doc" && !docConversionFilePaths.Contains(sourceFile))
                            docConversionFilePaths.Add(sourceFile);
                        else if (sourceFile.Substring(sourceFile.Length - 5).ToLower() == ".docx" && !docxConversionFilePaths.Contains(sourceFile))
                            docxConversionFilePaths.Add(sourceFile);
                    }
                });
                trSearch.Start();
                new Thread(() =>
                {
                    while (trSearch.IsAlive)
                    {
                    }
                    this.Dispatcher.Invoke((Action)delegate ()
                    {
                        toogleElemets(true);
                        if (autoScanConvert.IsChecked == true)
                        {
                            convert();
                        }
                    });
                }).Start();
            }
            catch (System.UnauthorizedAccessException)
            {
                Trace.WriteLine("UnauthorizedAccessException");
            }
        }

        // Document conversion
        public void convert()
        {
            try
            {
                // Get file paths from log file for conversion
                if (wasScanned == false)
                {
                    string[] pathLogLines = File.ReadAllLines(path + folderForFileCopy + "\\_TransferedFilesPaths.txt");
                    foreach(string line in pathLogLines)
                    {
                        string lineFinal = line.Trim();                        
                        string lineFileExtension = helperMethods.extensionExtraction(line.Trim());
                        Trace.WriteLine("LINES PRINT: " + lineFinal);
                        Trace.WriteLine("LINES PRINT EXTENSION: " + lineFileExtension);
                        if (lineFileExtension != ".txt")
                            allConversionFilePaths.Add(lineFinal);
                        if (lineFileExtension == ".pdf")
                            pdfConversionFilePaths.Add(lineFinal);
                        else if (lineFileExtension == ".doc")
                            docConversionFilePaths.Add(lineFinal);
                        else if (lineFileExtension == ".docx")
                            docxConversionFilePaths.Add(lineFinal);
                    }
                }
                ConvertWord convertWord = new ConvertWord();
                ConvertPdf convertPdf = new ConvertPdf();                
                string targetD = path + folderForFileCopy;
                toogleElemets(false);
                bool ocrCheckValid = ocrOption.IsChecked == true ? true : false;

                                
                Enumerable.Range(0, allConversionFilePaths.Count()-1).ToList().ForEach(j =>
                {
                    ThreadPool.QueueUserWorkItem((o) =>
                    {
                        Trace.WriteLine("THREAD NO: " + j.ToString());
                        if (File.Exists(allConversionFilePaths[j]))
                        {
                            Trace.WriteLine("docConversionFilePaths: " + allConversionFilePaths[j].ToString());
                            string targetFilePath = targetD + "\\" + helperMethods.fileNameExtraction(allConversionFilePaths[j].ToString()) + ".txt";
                            Trace.WriteLine("docConversionFilePaths_DESTINATION: " + targetFilePath);

                            string fileText = "";
                            string lineFileExtension = helperMethods.extensionExtraction(allConversionFilePaths[j].Trim());

                            if (lineFileExtension == ".pdf")
                                fileText = ocrCheckValid == true ? convertPdf.ExtractTextFromPdfWithOCR(allConversionFilePaths[j]) : convertPdf.ExtractTextFromPdf(allConversionFilePaths[j]);
                            else if (lineFileExtension == ".doc")
                                fileText = convertWord.ExtractTextFromDoc(allConversionFilePaths[j].ToString());
                            else if (lineFileExtension == ".docx")
                                fileText = convertWord.ExtractTextFromDocX(allConversionFilePaths[j].ToString());

                            if (fileText != "")
                            {
                                using (StreamWriter w = File.AppendText(targetFilePath))
                                {
                                    w.WriteLine(fileText);
                                }
                            }
                        }
                    });
                });

                toogleElemets(true);

                /*Thread conversion = new Thread(() =>
                {
                    for (int j = 0; j < docConversionFilePaths.Count(); j++)
                    {
                        if (File.Exists(docConversionFilePaths[j]))
                        {                            
                            Trace.WriteLine("docConversionFilePaths: " + docConversionFilePaths[j].ToString());
                            string targetFilePathDOC = targetD + "\\" + helperMethods.fileNameExtraction(docConversionFilePaths[j].ToString()) + ".txt";
                            Trace.WriteLine("docConversionFilePaths_DESTINATION: " + targetFilePathDOC);

                            string docText = convertWord.ExtractTextFromDoc(docConversionFilePaths[j].ToString());
                            if (docText != "")
                            {
                                using (StreamWriter w = File.AppendText(targetFilePathDOC))
                                {
                                    w.WriteLine(docText);
                                }
                            }
                        }                        
                    }

                    for (int k = 0; k < docxConversionFilePaths.Count(); k++)
                    {
                        if (File.Exists(docxConversionFilePaths[k]))
                        {
                            Trace.WriteLine("docXConversionFilePaths: " + docxConversionFilePaths[k].ToString());
                            string targetFilePathDOCX = targetD + "\\" + helperMethods.fileNameExtraction(docxConversionFilePaths[k].ToString()) + ".txt";
                            Trace.WriteLine("docxConversionFilePaths_DESTINATION: " + targetFilePathDOCX);

                            string docxText = convertWord.ExtractTextFromDocX(docxConversionFilePaths[k].ToString());
                            if (docxText != "")
                            {
                                using (StreamWriter w = File.AppendText(targetFilePathDOCX))
                                {
                                    w.WriteLine(docxText);
                                }
                            }
                        }                            
                    }

                    for (int i = 0; i < pdfConversionFilePaths.Count(); i++)
                    {   
                        if (File.Exists(pdfConversionFilePaths[i]))
                        {
                            Trace.WriteLine("pdfConversionFilePaths: " + pdfConversionFilePaths[i].ToString());
                            string targetFilePathPDF = targetD + "\\" + helperMethods.fileNameExtraction(pdfConversionFilePaths[i].ToString()) + ".txt";
                            Trace.WriteLine("docxConversionFilePaths_DESTINATION: " + targetFilePathPDF);

                            string pdfText = ocrCheckValid == true ? convertPdf.ExtractTextFromPdfWithOCR(pdfConversionFilePaths[i]) : convertPdf.ExtractTextFromPdf(pdfConversionFilePaths[i]);
                            if (pdfText != "")
                            {
                                using (StreamWriter w = File.AppendText(targetFilePathPDF))
                                {
                                    w.WriteLine(pdfText);
                                }
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
                    this.Dispatcher.Invoke((Action)delegate ()
                    {
                        toogleElemets(true);
                    });
                }).Start();*/
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }            
        }

        // Searching for text inside all files within TransferedFiles folder
        public void search()
        {
            try
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

                    var files = new List<FileInfo>();

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

                        if (searchTerm != "")
                        {

                            bool caseSensitive = false;

                            this.Dispatcher.Invoke((Action)delegate ()
                            {
                                if (cbxCS.IsChecked == true)
                                    caseSensitive = true;

                                Window1 win = new Window1();

                                win.parent = this;
                                win.files = files;
                                win.caseSensitive = caseSensitive;
                                win.workDir = workDir;
                                win.searchTerm = searchTerm;
                                win.task = "search";

                                win.ShowDialog();
                            });

                            resultList.Sort(delegate (SearchResults x, SearchResults y)
                            {
                                if (x.WordsFound.Count() > y.WordsFound.Count()) return -1;
                                else if (x.WordsFound.Count() < y.WordsFound.Count()) return 1;
                                else if (String.Compare(x.FilePath, y.FilePath) < 0) return -1;
                                return 1;
                            });

                            //display results
                            this.Dispatcher.Invoke((Action)delegate ()
                            {
                                searchResultTB.Items.Clear();
                            });
                            foreach (var result in resultList)
                            {
                                this.Dispatcher.Invoke((Action)delegate ()
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
                        this.Dispatcher.Invoke((Action)delegate ()
                        {
                            toogleElemets(true);
                        });
                    }).Start();
                }
                else
                {
                    MessageBox.Show("No files were coppied yet! Please click on " + btnFind.Content);
                    toogleElemets(true);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }            
        }
        private void btnPickClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
                if (dialog.ShowDialog(this).GetValueOrDefault())
                {
                    string pathSplit = dialog.SelectedPath;
                    path = pathSplit.Trim();
                    conversionDestination.Content = "Conversion destination: " + path + folderForFileCopy;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void autoScanConvertClick(object sender, RoutedEventArgs e)
        {
            if (autoScanConvert.IsChecked == true)
            {
                btnConvert.Visibility = Visibility.Collapsed;
                btnFind.Visibility = Visibility.Collapsed;
                btnFindAuto.Visibility = Visibility.Visible;
                Grid.SetColumnSpan(btnFindAuto, 2);
            } else
            {
                btnFindAuto.Visibility = Visibility.Collapsed;
                Grid.SetColumnSpan(btnFindAuto, 1);
                btnConvert.Visibility = Visibility.Visible;
                btnFind.Visibility = Visibility.Visible;
            }
        }

        private void Close_App(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = this.FindResource("cmScan") as ContextMenu;
            cm.PlacementTarget = sender as Button;

            if (cm.Visibility == Visibility.Visible)
            {
                cm.Visibility = Visibility.Hidden;
            }
            else
            {
                cm.IsOpen = true;
                cm.Visibility = Visibility.Visible;
            }
        }

        private void menuScanClick(object sender, RoutedEventArgs e)
        {
            scan();
        }
        private void menuConvertClick(object sender, RoutedEventArgs e)
        {
            convert();
        }
        private void menuAutoScanConvertClick(object sender, RoutedEventArgs e)
        {
            scan();
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            //ovde dodati kod za otvaranje dokumentacije
            return;
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

        private void OpenOriginalFile(string filePathWithExtension)
        {
            string fileName = helperMethods.fileNameExtraction(filePathWithExtension);
            string dirPath = helperMethods.fileDirectory(filePathWithExtension);
            string originalFile = helperMethods.originalFile(fileName, dirPath);

            if (originalFile != "")
            {
                Trace.WriteLine("Clicked File - Original File: " + originalFile);
                if (File.Exists(originalFile))
                    Process.Start(originalFile); // opens original file if coppied .PDF .DOC .DOCX
            }
            else
            {
                Trace.WriteLine("Clicked File - Original File: none found");
            }
            if (File.Exists(filePathWithExtension))
                Process.Start(filePathWithExtension); // opens .txt
        }        
    }
}
