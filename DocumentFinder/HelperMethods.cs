using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DocumentFinder
{
    internal class HelperMethods
    {
        // Extract just extension from Path
        public string extensionExtraction(string path)
        {
            string extension = "";
            try
            {
                extension = Path.GetExtension(path);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            return extension;
        }
        // Extract just filename from Path
        public string fileNameExtraction(string path)
        {
            string fileName = "";
            try
            {
                fileName = Path.GetFileNameWithoutExtension(path);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            return fileName;
        }
        // Return FilePath without extension
        public string filePathStripExtension(string path)
        {
            string filePathWithoutExtension = "";
            try
            {                
                filePathWithoutExtension = Path.ChangeExtension(path, null);
                //Trace.WriteLine("FILE CLICKED: " + filePathWithoutExtension);                
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            return filePathWithoutExtension;
        }
        // Return Directory in which the file resides
        public string fileDirectory(string path)
        {
            string dirPath = "";
            try
            {
                dirPath = Path.GetDirectoryName(path);
                //Trace.WriteLine("DIR PATH: " + dirPath);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            return dirPath;
        }
        // Return File path of the originalFile with the same name and .PDF .DOC or .DOCX extension
        public string originalFile(string fileName, string dir)
        {
            string founfFilePath = "";
            var files = new List<FileInfo>();
            var di = new DirectoryInfo(dir);
            List<string> extensions = new List<string> {".pdf", ".docx", ".doc" };            
            try
            {
                var fs = di.GetFiles(fileName+".*", SearchOption.TopDirectoryOnly).Where(f => extensions.Contains(f.Extension.ToLower())).ToArray();
                files.AddRange(fs);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            foreach (var f in files)
            {
                founfFilePath = f.FullName;
                //Trace.WriteLine("ORIGINAL FILES FOUND: " + f.FullName);
            }
            return founfFilePath;
        }
    }
}
