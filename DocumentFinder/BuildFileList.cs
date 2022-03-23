using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace DocumentFinder
{
    internal class BuildFileList
    {
        public List<string> excludeDirs;
        public List<string> extensions;
        string[] drives;

        public BuildFileList(List<string> ExcludeDirs, List<string> Extensions)
        {
            this.excludeDirs = ExcludeDirs;
            this.extensions = Extensions;
        }

        public List<FileInfo> GetFiles()
        {
            // Find all Drives
            drives = Directory.GetLogicalDrives();
            var files = new List<FileInfo>();
            foreach (string drive in drives)
            {
                var di = new DirectoryInfo(drive);
                var directories = di.GetDirectories();
                foreach (var directoryInfo in directories)
                {
                    try
                    {
                        if (!excludeDirs.Any(s => directoryInfo.FullName.ToString().Contains(s)) && MainWindow.main.stopWork == false)                        
                            GetFilesFromDirectory(directoryInfo.FullName, files);                        
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                }
            }
            MainWindow.main.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                MainWindow.main.updateProgress(drives.Length, drives.Length, "scanDrivesFinish", "scanDrivesFinish");
            }));
            Thread.Sleep(1000);
            return files;
        }

        private void GetFilesFromDirectory(string directory, List<FileInfo> files)
        {
            var di = new DirectoryInfo(directory);
            var fs = di.GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(f => extensions.Contains(f.Extension.ToLower())).ToArray();
            files.AddRange(fs);
            var directories = di.GetDirectories();
            int counter = 0;
            foreach (var directoryInfo in directories)
            {
                try
                {
                    if (MainWindow.main.stopWork == false)
                    {
                        counter++;
                        MainWindow.main.Dispatcher.BeginInvoke(new Action(delegate ()
                        {
                            MainWindow.main.updateProgress(directories.Length, counter, directoryInfo.Root.ToString(), "scanDrives");
                        }));
                        if (!excludeDirs.Any(s => directoryInfo.FullName.ToString().Contains(s)))                        
                            GetFilesFromDirectory(directoryInfo.FullName, files);                        
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                }
            }
        }
    }
}
