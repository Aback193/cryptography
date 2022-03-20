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
    internal class Convert
    {
        public List<string> allConversionFilePaths;
        public int length;
        public string targetD;
        public bool ocrCheckValid;
        public bool isMultiThreading;

        ConvertWord convertWord = new ConvertWord();
        ConvertPdf convertPdf = new ConvertPdf();
        HelperMethods helperMethods = new HelperMethods();

        public Convert(List<string> AllConversionFilePaths, int Length, string TargetD, bool OcrCheckValid, bool IsMultiThreading)
        {
            this.allConversionFilePaths = AllConversionFilePaths;
            this.length = Length;
            this.targetD = TargetD;
            this.ocrCheckValid = OcrCheckValid;
            this.isMultiThreading = IsMultiThreading;
        }

        // Start Conversion
        public async void startConversion()
        {
            if (isMultiThreading == true)
            {
                // Multi Threaded Conversion                
                Task[] tasks = new Task[length];
                int currFileCounter = 0;
                Enumerable.Range(0, length).ToList().ForEach(j =>
                {
                    tasks[j] = Task.Run(() =>
                    {
                        try
                        {
                            Trace.WriteLine("Task Concurency task start <<<= " + j.ToString());
                            string currFile = allConversionFilePaths[j];

                            conversion(currFile);

                            currFileCounter++;
                            MainWindow.main.updateProgress(length, currFileCounter, currFile, "convertP", false);
                            Trace.WriteLine("Task Concurency task end =>>> " + j.ToString());
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("Task NO: " + j.ToString() + " multithread exception: " + ex);
                        }
                        return j + 1;
                    });
                });

                await Task.WhenAll(tasks.ToArray());
                foreach (var task in tasks) task.Dispose();

                MainWindow.main.updateProgress(length, length, "convertFinish", "convertFinish", true);
                MainWindow.main.toogleElemets(true);
                if (MainWindow.main.stopWork == false && MainWindow.main.btnStopWork.Visibility == Visibility.Visible)
                    MainWindow.main.stopButtonReset();
            }
            else
            {
                // Single Threaded Conversion
                Task t = Task.Run(() =>
                {
                    Enumerable.Range(0, length).ToList().ForEach(j =>
                    {
                        try
                        {
                            string currFile = allConversionFilePaths[j];
                            conversion(currFile, j);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("Task NO: " + j.ToString() + " singlethread exception: " + ex);
                        }
                    });
                    return 1;
                });

                await Task.WhenAll(t);
                t.Dispose();

                MainWindow.main.toogleElemets(true);
                if (MainWindow.main.stopWork == false && MainWindow.main.btnStopWork.Visibility == Visibility.Visible)
                    MainWindow.main.stopButtonReset();
            }
        }

        // Call Conversion Methods
        private void conversion(string currFile, int index = 0)
        {
            if (File.Exists(currFile))
            {
                if (isMultiThreading == false)
                    MainWindow.main.updateProgress(length, index + 1, currFile, "convert", false);

                string targetFilePath = targetD + "\\" + helperMethods.fileNameExtraction(currFile.ToString()) + ".txt";
                string lineFileExtension = helperMethods.extensionExtraction(currFile.Trim());
                string fileText = "";

                if (lineFileExtension == ".pdf")
                    fileText = ocrCheckValid == true ? convertPdf.ExtractTextFromPdfWithOCR(currFile) : convertPdf.ExtractTextFromPdf(currFile);                    
                else if (lineFileExtension == ".doc")
                    fileText = convertWord.ExtractTextFromDoc(currFile.ToString());
                else if (lineFileExtension == ".docx")
                    fileText = convertWord.ExtractTextFromDocxXml(currFile.ToString());


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
    }
}
