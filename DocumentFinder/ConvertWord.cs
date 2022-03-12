using System;
using Microsoft.Office.Interop.Word;
using System.Diagnostics;
using Xceed.Words.NET;      // https://www.c-sharpcorner.com/article/generate-word-document-using-c-sharp/
using Application = Microsoft.Office.Interop.Word.Application;
using System.IO;

namespace DocumentFinder
{
    internal class ConvertWord
    {
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
    }
}
