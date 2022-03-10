using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace DocumentFinder
{
    internal class BuildFileList
    {
        public List<string> excludeDirs;
        public List<string> extensions;        
        //public Window1 progressWindow = new Window1();
        //public bool progressWindowIsRunning = false;

        // Constructor
        public BuildFileList(List<string> ExcludeDirs, List<string> Extensions)
        {
            this.excludeDirs = ExcludeDirs;
            this.extensions = Extensions;
        }

        public List<FileInfo> GetFiles()
        {
            // Find all Drives
            string[] drives = Directory.GetLogicalDrives();
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
                        if (!excludeDirs.Any(s => directoryInfo.FullName.ToString().Contains(s)))
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
            var fs = di.GetFiles("*.*", SearchOption.TopDirectoryOnly).Where(f => extensions.Contains(f.Extension.ToLower())).ToArray();
            files.AddRange(fs);
            var directories = di.GetDirectories();            
            foreach (var directoryInfo in directories)
            {
                try
                {
                    if (!excludeDirs.Any(s => directoryInfo.FullName.ToString().Contains(s)))
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
}
