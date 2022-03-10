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
                using (iTextSharp.text.pdf.PdfReader reader = new PdfReader(pdfFile))
                {
                    StringBuilder text = new StringBuilder();

                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        text.Append(PdfTextExtractor.GetTextFromPage(reader, i));
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
            try
            {
                using (var engine = new TesseractEngine(@"./testdata", "eng", EngineMode.Default))
                {
                    using (iTextSharp.text.pdf.PdfReader pdf = new PdfReader(pdfFile))
                    {

                        for (int pageNumber = 1; pageNumber <= pdf.NumberOfPages; pageNumber++)
                        {
                            text.Append(PdfTextExtractor.GetTextFromPage(pdf, pageNumber));
                            PdfDictionary pg = pdf.GetPageN(pageNumber);

                            IList<System.Drawing.Image> listImages = GetImagesFromPdfDict(pg, pdf);
                            if (listImages == null)
                            {
                                continue;
                            }
                            var imageNumber = 1;
                            foreach (var obj in listImages)
                            {
                                try
                                {
                                    var bmp = new Bitmap(obj);
                                    var img = PixConverter.ToPix(bmp);
                                    using (var image = engine.Process(img))
                                    {
                                        var textFromImage = image.GetText();
                                        text.Append(textFromImage);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Trace.WriteLine("Execption for image on page:" + pageNumber + "Image Number:" + imageNumber + " " + ex);
                                }
                                imageNumber++;
                            }

                        }
                    }
                }
            }
            catch (Exception)
            {
                Trace.WriteLine("PDF to Text Exception");
            }

            return text.ToString();
        }
        private IList<System.Drawing.Image> GetImagesFromPdfDict(PdfDictionary dict, PdfReader doc)
        {
            List<System.Drawing.Image> images = new List<System.Drawing.Image>();
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
                            int xrefIdx = ((PRIndirectReference)obj).Number;
                            PdfObject pdfObj = doc.GetPdfObject(xrefIdx);
                            PdfStream str = (PdfStream)(pdfObj);

                            iTextSharp.text.pdf.parser.PdfImageObject pdfImage =
                                new iTextSharp.text.pdf.parser.PdfImageObject((PRStream)str);
                            System.Drawing.Image img = pdfImage.GetDrawingImage();

                            images.Add(img);
                        }
                        else if (PdfName.FORM.Equals(subtype) || PdfName.GROUP.Equals(subtype))
                        {
                            images.AddRange(GetImagesFromPdfDict(tg, doc));
                        }
                    }
                }
            }
            return images;
        }
    }
}
