using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using Tesseract;

namespace DocumentFinder
{
    internal class ConvertPdf
    {
        // PDF to Text
        public string ExtractTextFromPdf(string pdfFile)
        {
            try
            {
                using (PdfReader reader = new PdfReader(pdfFile))
                {
                    StringBuilder text = new StringBuilder();
                    for (int pageNumber = 1; pageNumber <= reader.NumberOfPages; pageNumber++)
                    {
                        if (MainWindow.main.stopWork == false)
                        {
                            Trace.WriteLine("| PDF: " + pdfFile + " | Current page: " + pageNumber.ToString());
                            text.Append(PdfTextExtractor.GetTextFromPage(reader, pageNumber));
                        }
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

        // PDF OCR
        public string ExtractTextFromPdfWithOCR(string pdfFile)
        {
            StringBuilder text = new StringBuilder();
            using (TesseractEngine engine = new TesseractEngine(@"./testdata/", "eng", EngineMode.Default))
            {
                using (PdfReader pdf = new PdfReader(pdfFile))
                {
                    for (int pageNumber = 1; pageNumber <= pdf.NumberOfPages; pageNumber++)
                    {
                        if (MainWindow.main.stopWork == false)
                        {
                            Trace.WriteLine("| PDF: " + pdfFile + " | Current page: " + pageNumber.ToString());
                            try
                            {
                                text.Append(PdfTextExtractor.GetTextFromPage(pdf, pageNumber));
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine("PDF to Text Exception: " + ex);
                            }
                            try
                            {
                                PdfDictionary pg = pdf.GetPageN(pageNumber);
                                IList<string> listImages = GetImagesFromPdfDict(pg, pdf, engine);
                                if (listImages != null)
                                    foreach (string s in listImages)
                                        text.Append(s);
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine("PDF Image OCR to text Exception: " + ex);
                            }
                        }
                    }
                }
            }
            if (text != null)
                return text.ToString();
            else
                return "";
        }

        // Get image text from PdfDict
        private IList<string> GetImagesFromPdfDict(PdfDictionary dict, PdfReader doc, TesseractEngine engine)
        {
            List<string> imagesText = new List<string>();
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
                            try
                            {
                                int xrefIdx = ((PRIndirectReference)obj).Number;
                                PdfObject pdfObj = doc.GetPdfObject(xrefIdx);
                                PdfStream str = (PdfStream)(pdfObj);
                                PdfImageObject pdfImage = new PdfImageObject((PRStream)str);
                                Pix img = PixConverter.ToPix(new Bitmap(pdfImage.GetDrawingImage()));
                                using (Page image = engine.Process(img))
                                {
                                    imagesText.Add(image.GetText());
                                    img.Dispose();
                                }
                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine(ex);
                            }
                        }
                        else if (PdfName.FORM.Equals(subtype) || PdfName.GROUP.Equals(subtype))
                        {
                            imagesText.AddRange(GetImagesFromPdfDict(tg, doc, engine));
                        }
                    }
                }
            }
            xobj.Clear();
            res.Clear();
            return imagesText;
        }
    }
}