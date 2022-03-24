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
        public static List<string> excludeDirs = new List<string> { "C:\\Windows", "C:\\Recovery", "C:\\Program Files", "C:\\ProgramData", "C:\\$Recycle.Bin" };       
        const string folderForFileCopy = "\\TransferedFiles";
        const string pathLogFile = "\\_TransferedFilesPaths.txt";
        const string helpFile = "DocFInder.pdf";
        string path = "C:";

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
            searchTb.Focusable = isEnabled;
            tb1.Focusable = isEnabled;
            OptionsBtn.IsEnabled = isEnabled;
            HelpBtn.IsEnabled = isEnabled;
        }

        private void btnAutoFindConvertClick(object sender, RoutedEventArgs e)
        {
            scanAsync();
        }
        private void btnFindClick(object sender, RoutedEventArgs e)
        {
            scanAsync();            
        }
        private void btnConvertClick(object sender, RoutedEventArgs e)
        {            
            convertAsync();            
        }
        private void btnSearchClick(object sender, RoutedEventArgs e)
        {
            search();
        }        

        // Scan Folders and Files
        public async Task scanAsync()
        {
            try
            {
                tb1.Clear();                
                setBottomStatusBar();
                toogleElemets(false);
                stopButtonReset();
                bool copyFilesCheckValid = cbxCopy.IsChecked == true ? true : false;
                BuildFileList buildFileList = new BuildFileList(excludeDirs, extensions);

                // Get current directory & make new directory for file transfer if non existent
                string targetD = path + folderForFileCopy;
                if (!Directory.Exists(targetD))                
                    Directory.CreateDirectory(targetD);
                else if (File.Exists(path + folderForFileCopy + pathLogFile))
                    File.Delete(path + folderForFileCopy + pathLogFile);

                Task search = Task.Run(() =>
                {
                    var files = buildFileList.GetFiles();
                    int filesCount = files.Count;
                    for (int i = 0; i < filesCount; i++)
                        if (stopWork == false)
                        {
                            string sourceFile = Path.Combine(files[i].DirectoryName.ToString(), files[i].ToString());
                            string destFile = Path.Combine(targetD, files[i].ToString());

                            // Update UI thread TextBox with paths.
                            tb1.Dispatcher.Invoke((Action)delegate ()
                            {
                                tb1.Text = tb1.Text + files[i].DirectoryName.ToString() + "\\" + files[i].ToString() + "\n";
                            });

                            updateProgress(filesCount, i + 1, files[i].DirectoryName.ToString() + "\\" + files[i].ToString(), "scan");
                            if(filesCount == i + 1)
                                updateProgress(filesCount, filesCount, "scanFinish", "scanFinish");

                            // Copy original files to destination if checkbox is checked
                            try
                            {
                                string ext = helperMethods.extensionExtraction(sourceFile);
                                if (copyFilesCheckValid == true && File.Exists(sourceFile))
                                    File.Copy(sourceFile, destFile, true);
                                else if (ext == ".txt" && File.Exists(sourceFile))
                                    File.Copy(sourceFile, destFile, true);
                                
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine("Exception, file copy: " + ex);
                            }

                            // Save all file paths to log file
                            string transferedFilesPathSave = path + folderForFileCopy + pathLogFile;
                            using (StreamWriter w = File.AppendText(transferedFilesPathSave))
                                w.WriteLine(sourceFile);

                            // Save file paths for conversion
                            if (sourceFile.Substring(sourceFile.Length - 4).ToLower() != ".txt" && !allConversionFilePaths.Contains(sourceFile))
                                allConversionFilePaths.Add(sourceFile);

                            if (sourceFile.Substring(sourceFile.Length - 4).ToLower() == ".pdf" && !pdfConversionFilePaths.Contains(sourceFile))
                                pdfConversionFilePaths.Add(sourceFile);
                            else if (sourceFile.Substring(sourceFile.Length - 4).ToLower() == ".doc" && !docConversionFilePaths.Contains(sourceFile))
                                docConversionFilePaths.Add(sourceFile);
                            else if (sourceFile.Substring(sourceFile.Length - 5).ToLower() == ".docx" && !docxConversionFilePaths.Contains(sourceFile))
                                docxConversionFilePaths.Add(sourceFile);

                            wasScanned = true;
                        }
                        else
                            // Incomplete interrupted scan. Update progress status only with file paths written in LogFile.
                            if (File.Exists(path + folderForFileCopy + pathLogFile))
                            {
                                int lineCount = File.ReadLines(path + folderForFileCopy + pathLogFile).Count(line => !string.IsNullOrWhiteSpace(line));
                                updateProgress(lineCount, lineCount, "scanFinish", "scanFinish");
                            }
                });

                await Task.WhenAll(search);
                search.Dispose();

                toogleElemets(true);
                if (stopWork == false)
                {
                    stopButtonReset();
                    if (autoScanConvert.IsChecked == true)
                        convertAsync();                    
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Scan exception: " + ex);
            }
        }

        // Document conversion
        public async Task convertAsync()
        {            
            toogleElemets(false);
            bool startConversion = true;

            // Get file paths from log file for conversion
            if (wasScanned == false && File.Exists(path + folderForFileCopy + pathLogFile))
            {
                stopButtonReset();
                loadLogFile();                
            }
            else if (wasScanned == false && !File.Exists(path + folderForFileCopy + pathLogFile))
            {
                startConversion = false;
                MessageBox.Show("Log file containing scanned files doesn't exist! Please scan for files first.");
                toogleElemets(true);
                if (btnStopWork.Visibility == Visibility.Visible)
                    stopButtonReset();
            }
            else if (wasScanned == true && File.Exists(path + folderForFileCopy + pathLogFile))
                if (btnStopWork.Visibility == Visibility.Hidden)
                    stopButtonReset();

            if (startConversion && allConversionFilePaths.Count() > 0)
            {
                Convert convert = new Convert(allConversionFilePaths, allConversionFilePaths.Count(), path + folderForFileCopy, ocrOption.IsChecked == true ? true : false, isMultiThreading);
                convert.startConversion();
            }
        }

        // Searching for text inside all files within TransferedFiles folder
        public async Task search()
        {
            try
            {
                toogleElemets(false);
                stopButtonReset();

                string targetD = path + folderForFileCopy;

                if (Directory.Exists(targetD))
                {
                    string workDir = path + folderForFileCopy + "\\";
                    string searchTerm = "";

                    if (searchTb.Text.ToString() != "")
                    {
                        searchTerm = searchTb.Text.ToString();
                    }
                    searchTerm = searchTerm.Trim();

                    var files = new List<FileInfo>();
                    var resultList = new List<SearchResults>();

                    Task keyWordSearch = Task.Run(()=>
                    {
                        var di = new DirectoryInfo(workDir);
                        var extensions = new List<string> { ".txt" };
                        var fs = di.GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(f => extensions.Contains(f.Extension.ToLower())).ToArray();
                        files.AddRange(fs);
                        files.RemoveAll(x => x.Name == "_TransferedFilesPaths.txt");

                        if (searchTerm != "" && !searchTerm.All(char.IsWhiteSpace))
                        {
                            bool caseSensitive = false;
                            this.Dispatcher.Invoke((Action)delegate ()
                            {
                                if (cbxCS.IsChecked == true)
                                    caseSensitive = true;
                                progressBar.Value = 0;
                                progressBar.Maximum = 100;
                            });

                            List<string> SearchWords = new List<string>();
                            SearchWords = searchTerm.Split(' ').ToList();
                            SearchWords.RemoveAll(x => x == "");
                            int counter = 0;

                            //search for term
                            foreach (var file in files)
                            {
                                if (stopWork == false)
                                {
                                    counter++;
                                    this.Dispatcher.Invoke((Action)delegate ()
                                    {
                                        updateProgress(files.Count(), counter, file.Name, "search");
                                    });

                                    if (File.Exists(workDir + file.Name) && SearchWords.Count() > 0)
                                    {
                                        var lines = File.ReadAllLines(workDir + file.Name);

                                        SearchWords.ForEach(x =>
                                        {
                                            x = x.Trim();
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
                                else
                                {
                                    updateProgress(files.IndexOf(file), files.IndexOf(file), "searchFinish", "searchFinish");
                                    break;
                                }
                            }

                            //sort results
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
                                    progressStatus.Text = "Finished search for: " + searchTerm + ". Found " + resultList.Count().ToString() + " files.";
                                });
                            }

                            if(resultList.Count() == 0)
                            {
                                this.Dispatcher.Invoke((Action)delegate ()
                                {
                                    progressStatus.Text = "Finished search for: " + searchTerm + ". Found " + resultList.Count().ToString() + " files.";
                                });
                            }
                        }
                        else
                        {
                            MessageBox.Show("Search term can't be empty");
                        }
                    }
                    );

                    await Task.WhenAll(keyWordSearch);
                    keyWordSearch.Dispose();

                    toogleElemets(true);

                    if (stopWork == false)
                    {
                        stopButtonReset();
                    }
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
        // Reset STOP button state.
        public void stopButtonReset()
        {
            if (btnStopWork.Visibility == Visibility.Hidden)
            {
                btnStopWork.Visibility = Visibility.Visible;
                stopWork = false;
            }
            else
            {
                btnStopWork.Visibility = Visibility.Hidden;
                stopWork = false;
            }
        }
        // Click on STOP button.
        private void stopClick(object sender, RoutedEventArgs e)
        {
            btnStopWork.Visibility = Visibility.Hidden;
            stopWork = true;
        }
        // Pressing Enter key when in search term text box initiates the search. No need to press the button.
        private void searchTbEnterListener(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && searchTb.IsFocused)
            {
                search();
            }
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

        public void updateProgress(int pMax, int counter, string filePath, string mode)
        {
            // Update UI progress bar % visualization here.
            progressBar.Dispatcher.Invoke((Action)delegate ()
            {
                if(progressBar.Value < progressBar.Maximum)
                {
                    progressBar.Maximum = pMax;
                    progressBar.Value++;
                } else
                        progressBar.Value = 0;
                if (mode.Contains("Finish"))
                    progressBar.Maximum = progressBar.Value = pMax;
            });

            // Set variables needed for progress status text UI upate, following next.
            string modeFinal = "Searching: ";
            string finalName = filePath;
            string endingText = " files";

            if (mode.Contains("scan"))
                if(mode.Contains("Drives"))
                    endingText = " drives";
                else
                    endingText = " files found";
            else if (mode.Contains("convert"))
            {
                finalName = helperMethods.fileNameExtraction(filePath) + helperMethods.extensionExtraction(filePath);
                if (mode.Equals("convertMultiThread"))
                    modeFinal = "Converted: ";
                else
                    modeFinal = "Converting: ";
            }

            // Update UI progress status text here.
            progressStatus.Dispatcher.Invoke((Action)delegate ()
            {
                // Update text after action finish, if progress bar maximum was reached, else update text of currently running action
                if (counter == pMax && mode.Contains("Finish"))
                    progressStatus.Text = "Finished " + modeFinal + pMax.ToString() + endingText;
                else
                    progressStatus.Text = modeFinal + counter.ToString() + "/" + pMax.ToString() + " " + finalName;
            });
        }

        private void menuScanClick(object sender, RoutedEventArgs e)
        {
            scanAsync();
        }
        private void menuConvertClick(object sender, RoutedEventArgs e)
        {
            convertAsync();
        }
        private void menuAutoScanConvertClick(object sender, RoutedEventArgs e)
        {
            scanAsync();
        }
        private void menuMultiThreading(object sender, RoutedEventArgs e)
        {
            isMultiThreading = !isMultiThreading;
        }
        // Write PDF Help file from app's Resources folder to app's Temp folder, and launch!
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string locationToSavePdf = Path.Combine(Path.GetTempPath(), helpFile);
                File.WriteAllBytes(locationToSavePdf, Properties.Resources.DocFInder);
                Process.Start(locationToSavePdf);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                Trace.WriteLine(ex.ToString());
            }            
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
        // Load Log file
        public void loadLogFile()
        {            
            allConversionFilePaths.Clear();
            pdfConversionFilePaths.Clear();
            docConversionFilePaths.Clear();
            docxConversionFilePaths.Clear();
            string[] pathLogLines = File.ReadAllLines(path + folderForFileCopy + pathLogFile);
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
        // Open files when clicked on search result
        private void OpenOriginalFile(string filePathWithExtension)
        {
            string fileName = helperMethods.fileNameExtraction(filePathWithExtension);
            string originalFileSourceFolder = "";

            // Open original file from Source folder
            foreach (string file in allConversionFilePaths)            
                if (helperMethods.fileNameExtraction(file) == fileName && helperMethods.extensionExtraction(file) != ".txt")
                    originalFileSourceFolder = file;
            
            if (originalFileSourceFolder != "")
            {
                Trace.WriteLine("Clicked File - Original File: " + originalFileSourceFolder);
                if (File.Exists(originalFileSourceFolder))
                    Process.Start(originalFileSourceFolder);
            }
            else            
                Trace.WriteLine("Clicked File - Original File: not found!");
            
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
                outText = outText + "  [ " + drive.VolumeLabel + " " + drive.Name + " ]";            
            statusBar.Text = "Drives found: " + outText;
        }
    }
}
