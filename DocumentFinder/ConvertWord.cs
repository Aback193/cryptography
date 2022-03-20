using System;
using Microsoft.Office.Interop.Word;
using System.Diagnostics;
using Xceed.Words.NET;      // https://www.c-sharpcorner.com/article/generate-word-document-using-c-sharp/
using Application = Microsoft.Office.Interop.Word.Application;
using System.IO;
using System.IO.Compression;
using System.Xml;

namespace DocumentFinder
{
    internal class ConvertWord
    {
        // DOCX to Text using extraction and XML
        public string ExtractTextFromDocxXml(string filePath)
        {
            try
            {
                string folder = Path.GetDirectoryName(filePath);
                string extractionFolder = folder + "\\extraction__DOCX";

                if (Directory.Exists(extractionFolder))
                    Directory.Delete(extractionFolder, true);

                ZipFile.ExtractToDirectory(filePath, extractionFolder);
                string xmlFilepath = extractionFolder + "\\word\\document.xml";

                var xmldoc = new XmlDocument();
                xmldoc.Load(xmlFilepath);

                if (Directory.Exists(extractionFolder))
                    Directory.Delete(extractionFolder, true);

                return xmldoc.DocumentElement.InnerText;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("DOCX to Text Exception XML: " + ex);
                return "";
            }
        }
        // DOC to Text using Microsoft.Office.Interop.Word
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
            catch (Exception ex)
            {
                Trace.WriteLine("DOC to Text Exception: " + ex);
                return "";
            }
        }

        // DOCX to Text with DocX library
        /*public string ExtractTextFromDocX(string filePath)
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
        }*/
    }
}
