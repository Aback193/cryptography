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
        public static MainWindow main;
        HelperMethods helperMethods = new HelperMethods();
        List<string> docConversionFilePaths = new List<string>();
        List<string> docxConversionFilePaths = new List<string>();
        List<string> pdfConversionFilePaths = new List<string>();
        List<string> allConversionFilePaths = new List<string>();
        public List<SearchResults> resultList = new List<SearchResults>();
        public bool isMultiThreading = false;
        public bool wasScanned = false;
        public bool stopWork = false;
        public static List<string> extensions = new List<string> { ".txt", ".pgn", ".pdf", ".docx", ".doc" };
        public static List<string> excludeDirs = new List<string>() { "C:\\Windows", "C:\\Recovery", "C:\\Program Files", "C:\\ProgramData", "C:\\$Recycle.Bin" };
        string path = "C:";
        string folderForFileCopy = "\\TransferedFiles";

        public MainWindow()
        {
            InitializeComponent();
            setBottomStatusBar();
            conversionDestination.Content = "Conversion destination: " + path + folderForFileCopy;
            main = this;
        }

        public void toogleElemets(bool isEnabled)
        {
            btnFind.IsEnabled = isEnabled;
            btnFindAuto.IsEnabled = isEnabled;
            btnConvert.IsEnabled = isEnabled;
            btnSearch.IsEnabled = isEnabled;
            btnPick.IsEnabled = isEnabled;
            btnStopWork.IsEnabled = !isEnabled;
            ocrOption.IsEnabled = isEnabled;
            cbxCopy.IsEnabled = isEnabled;
            autoScanConvert.IsEnabled = isEnabled;
            cbxCS.IsEnabled = isEnabled;
        }

        private void btnAutoFindConvertClick(object sender, RoutedEventArgs e)
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
                setBottomStatusBar();
                toogleElemets(false);
                stopButtonReset();
                bool copyFilesCheckValid = cbxCopy.IsChecked == true ? true : false;
                BuildFileList b = new BuildFileList(excludeDirs, extensions);

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

                    for (int i = 0; i < files.Count; i++)
                    {
                        if(stopWork == false)
                        {
                            string sourceFile = Path.Combine(files[i].DirectoryName.ToString(), files[i].ToString());
                            string destFile = Path.Combine(targetD, files[i].ToString());

                            // Update UI thread TextBox with paths.
                            tb1.Dispatcher.Invoke((Action)delegate ()
                            {
                                tb1.Text = tb1.Text + files[i].DirectoryName.ToString() + "\\" + files[i].ToString() + "\n";
                            });

                            updateProgress(files.Count, i + 1, files[i].DirectoryName.ToString() + "\\" + files[i].ToString(), "scan", false);

                            // Copy original files to destination if checkbox is checked
                            try
                            {
                                if (copyFilesCheckValid == true && File.Exists(sourceFile))
                                {
                                    File.Copy(sourceFile, destFile, true);
                                }
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine("Exception, file copy: " + ex);
                            }

                            // Save all file paths to txt
                            string transferedFilesPathSave = path + folderForFileCopy + "\\_TransferedFilesPaths.txt";
                            using (StreamWriter w = File.AppendText(transferedFilesPathSave))
                            {
                                w.WriteLine(sourceFile);
                                Trace.WriteLine("SOURCE FILE: " + i.ToString() + ". " + sourceFile);
                            }

                            // Save file paths for conversion
                            if (sourceFile.Substring(sourceFile.Length - 4).ToLower() != ".txt" && !allConversionFilePaths.Contains(sourceFile))
                                allConversionFilePaths.Add(sourceFile);

                            if (sourceFile.Substring(sourceFile.Length - 4).ToLower() == ".pdf" && !pdfConversionFilePaths.Contains(sourceFile))
                                pdfConversionFilePaths.Add(sourceFile);
                            else if (sourceFile.Substring(sourceFile.Length - 4).ToLower() == ".doc" && !docConversionFilePaths.Contains(sourceFile))
                                docConversionFilePaths.Add(sourceFile);
                            else if (sourceFile.Substring(sourceFile.Length - 5).ToLower() == ".docx" && !docxConversionFilePaths.Contains(sourceFile))
                                docxConversionFilePaths.Add(sourceFile);
                        }
                        else
                        {
                            if (File.Exists(path + folderForFileCopy + "\\_TransferedFilesPaths.txt"))
                            {
                                int lineCount = File.ReadLines(path + folderForFileCopy + "\\_TransferedFilesPaths.txt").Count(line => !string.IsNullOrWhiteSpace(line));
                                updateProgress(lineCount, lineCount, "scanFinish", "scanFinish", false);
                            }                            
                        }
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
                        if(stopWork == false)
                        {
                            stopButtonReset();
                            if (autoScanConvert.IsChecked == true)
                            {
                                convert();
                            }
                        }
                    });
                }).Start();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Scan exception: " + ex);
            }
        }

        // Document conversion
        public void convert()
        {
            // Get file paths from log file for conversion
            if (wasScanned == false && File.Exists(path + folderForFileCopy + "\\_TransferedFilesPaths.txt"))
            {
                allConversionFilePaths.Clear();
                pdfConversionFilePaths.Clear();
                docConversionFilePaths.Clear();
                docxConversionFilePaths.Clear();
                string[] pathLogLines = File.ReadAllLines(path + folderForFileCopy + "\\_TransferedFilesPaths.txt");
                foreach (string line in pathLogLines)
                {
                    string lineFinal = line.Trim();
                    string lineFileExtension = helperMethods.extensionExtraction(line.Trim());
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
            else if (wasScanned == false && !File.Exists(path + folderForFileCopy + "\\_TransferedFilesPaths.txt"))
                MessageBox.Show("Log file containing scanned files doesn't exist! Please scan for files first.");
            ConvertWord convertWord = new ConvertWord();
            ConvertPdf convertPdf = new ConvertPdf();
            string targetD = path + folderForFileCopy;
            toogleElemets(false);
            stopButtonReset();
            bool ocrCheckValid = ocrOption.IsChecked == true ? true : false;

            // Start Conversion
            if (isMultiThreading == true)
            {
                // Multi Threaded Conversion
                Thread[] threads = new Thread[allConversionFilePaths.Count()];
                int br = 0;
                Enumerable.Range(0, allConversionFilePaths.Count()).ToList().ForEach(j =>
                {
                    threads[j] = new Thread(() =>
                    {
                        try
                        {
                            if (File.Exists(allConversionFilePaths[j]))
                            {
                                string targetFilePath = targetD + "\\" + helperMethods.fileNameExtraction(allConversionFilePaths[j].ToString()) + ".txt";

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
                                    if (File.Exists(targetFilePath))
                                        File.Delete(targetFilePath);
                                    using (StreamWriter w = File.AppendText(targetFilePath))
                                    {
                                        w.WriteLine(fileText);
                                    }
                                }
                            }
                            br++;
                            updateProgress(allConversionFilePaths.Count, br, allConversionFilePaths[j], "convertP", false);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("Task NO: " + j.ToString() + " multithread exception: " + ex);
                        }
                    });
                    threads[j].Start();
                });

                // Check in background if all task had finished, update UI
                new Thread(() =>
                {
                    Thread.Sleep(1000);
                    bool areThreadsRunning = true;
                    while (areThreadsRunning == true)
                    {
                        areThreadsRunning = false;
                        foreach (Thread th in threads)
                        {
                            if (th.IsAlive)
                                areThreadsRunning = true;
                        }
                    }
                    this.Dispatcher.Invoke(() =>
                    {
                        updateProgress(allConversionFilePaths.Count, allConversionFilePaths.Count, allConversionFilePaths[allConversionFilePaths.Count - 1], "convertFinish", true);
                        toogleElemets(true);
                        if (stopWork == false)
                            stopButtonReset();
                    });
                }).Start();
            }
            else
            {
                // Single Threaded Conversion
                Thread t = new Thread(() =>
                {
                    Enumerable.Range(0, allConversionFilePaths.Count()).ToList().ForEach(j =>
                    {
                        try
                        {
                            if (File.Exists(allConversionFilePaths[j]))
                            {
                                updateProgress(allConversionFilePaths.Count, j + 1, allConversionFilePaths[j], "convert", false);

                                string targetFilePath = targetD + "\\" + helperMethods.fileNameExtraction(allConversionFilePaths[j].ToString()) + ".txt";

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
                                    if (File.Exists(targetFilePath))
                                        File.Delete(targetFilePath);
                                    using (StreamWriter w = File.AppendText(targetFilePath))
                                    {
                                        w.WriteLine(fileText);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("Task NO: " + j.ToString() + " singlethread exception: " + ex);
                        }
                    });
                });
                t.Start();
                new Thread(() =>
                {
                    while (t.IsAlive)
                    {
                        //Trace.WriteLine("THREAD STATE: " + t.ThreadState);
                    }
                    this.Dispatcher.Invoke((Action)delegate ()
                    {
                        //Trace.WriteLine("THREAD STATE: " + t.ThreadState);
                        toogleElemets(true);
                        if (stopWork == false)
                            stopButtonReset();
                    });
                }).Start();
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
        public void stopButtonReset()
        {
            if(btnStopWork.Visibility == Visibility.Hidden)
            {
                btnStopWork.Visibility = Visibility.Visible;
                stopWork = false;
            }else
            {
                btnStopWork.Visibility = Visibility.Hidden;
                stopWork = false;
            }
        }
        private void stopClick(object sender, RoutedEventArgs e)
        {
            btnStopWork.Visibility = Visibility.Hidden;
            stopWork = true;
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

        public void updateProgress(int pMax, int counter, string filePath, string mode, bool isKill)
        {
            progressBar.Dispatcher.Invoke((Action)delegate ()
            {
                if (progressBar.Value == progressBar.Maximum && !isKill)
                    progressBar.Value = 0;
                if(progressBar.Value < progressBar.Maximum && !isKill)
                {
                    progressBar.Maximum = pMax;
                    progressBar.Value++;
                }
                if(mode == "scanFinish" || mode == "scanDrivesFinish")
                {
                    progressBar.Maximum = pMax;
                    progressBar.Value = pMax;
                }                    
            });

            string modeFinal = "Converting: ";
            string fileType = " files";
            string finalName = filePath;
            if (mode == "scan")
            {
                modeFinal = "Scanning: ";
                fileType = " files found";
            }
            else if (mode == "scanDrives" || mode == "scanDrivesFinish")
            {
                modeFinal = "Scanning: ";
            }
            else if (mode == "convert" || mode == "convertFinish" || mode == "convertP")
            {
                finalName = helperMethods.fileNameExtraction(filePath) + helperMethods.extensionExtraction(filePath);
                if(mode == "convertP")
                    modeFinal = "Converted: ";
            }
            progressStatus.Dispatcher.Invoke((Action)delegate ()
            {                
                progressStatus.Text = modeFinal + counter.ToString() + ". " + finalName;
                if (counter == pMax && mode != "scanDrives" && mode != "scanDrivesFinish" && mode != "convertP" && mode != "convertFinish")
                    progressStatus.Text = "Finished " + modeFinal + pMax.ToString() + fileType;
                if (counter == pMax && mode == "scanDrivesFinish")
                    progressStatus.Text = "Finished " + modeFinal + pMax.ToString() + " drives";
                if (counter == pMax && mode == "scanFinish")
                    progressStatus.Text = "Finished " + "Scanning: " + pMax.ToString() + " files found";
                if (counter == pMax && mode == "convertFinish")
                    progressStatus.Text = "Finished " + modeFinal + pMax.ToString() + fileType;
            });
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
        private void menuMultiThreading(object sender, RoutedEventArgs e)
        {
            isMultiThreading = !isMultiThreading;
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
            string originalFileSourceFolder = "";

            // Open original file from Source folder
            foreach (string file in allConversionFilePaths)
            {
                if (helperMethods.fileNameExtraction(file) == fileName && helperMethods.extensionExtraction(file) != ".txt")
                    originalFileSourceFolder = file;
            }
            if (originalFileSourceFolder != "")
            {
                Trace.WriteLine("Clicked File - Original File: " + originalFileSourceFolder);
                if (File.Exists(originalFileSourceFolder))
                    Process.Start(originalFileSourceFolder);
            }
            else
            {
                Trace.WriteLine("Clicked File - Original File: not found!");
            }

            // Open txt file from TransferedFiles folder
            if (File.Exists(filePathWithExtension))
                Process.Start(filePathWithExtension);
        }
        // Display all Drives on BottomStatusBar
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
    }
}
