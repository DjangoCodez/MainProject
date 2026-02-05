using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.Kernel.Geom;

namespace SoftOne.Soe.Util
{
    public class PDFUtility
    {
        public static bool MergeFiles(DirectoryInfo directoryWithPdfsToMerge, string destinationFileName, string searchPattern = "")
        {
            if (!directoryWithPdfsToMerge.Exists)
                return false;

            List<string> sourceFiles = new List<string>();

            FileInfo[] fileInfos = null;
            if (!String.IsNullOrEmpty(searchPattern))
                fileInfos = directoryWithPdfsToMerge.GetFiles(searchPattern, SearchOption.TopDirectoryOnly);
            else
                fileInfos = directoryWithPdfsToMerge.GetFiles();

            foreach (FileInfo file in fileInfos)
            {
                if (file.Extension.ToLower() == ".pdf")
                    sourceFiles.Add(file.FullName);
            }

            return MergeFiles(destinationFileName, sourceFiles);
        }

        private static void WritePdfContent(PdfDocument targetPdf, PdfDocument sourcePdf)
        {
            PdfMerger merger = new PdfMerger(targetPdf);

            for (int i = 1; i <= sourcePdf.GetNumberOfPages(); i++)
            {
                merger.Merge(sourcePdf, i, i);
            }
        }

        public static List<KeyValuePair<string, byte[]>> MergeDictionary(string mainFileName, List<KeyValuePair<string, byte[]>> attachments)
        {
            var mainFile = attachments.FindLast(x=> x.Key == mainFileName);

            if (mainFile.Value == null)
                throw new Exception("MergeDictionary is missing main file content");

            var result = new List<KeyValuePair<string, byte[]>>();

            using (var mainPdfStream = new MemoryStream(mainFile.Value))
            using (var targetStream = new MemoryStream())
            {
                var targetPdf = new PdfDocument(new PdfWriter(targetStream));
                var mainPdf = new PdfDocument(new PdfReader(mainPdfStream));

                WritePdfContent(targetPdf, mainPdf);
                attachments.Remove(mainFile);

                foreach (var fileEntry in attachments)
                {
                    if (fileEntry.Key.ToLower().EndsWith(".pdf"))
                    {
                        using (var sourcePdfStream = new MemoryStream(fileEntry.Value))
                        {
                            var sourcePdf = new PdfDocument(new PdfReader(sourcePdfStream));
                            WritePdfContent(targetPdf, sourcePdf);
                        }
                    }
                    else
                    {
                        result.Add(new KeyValuePair<string, byte[]>(fileEntry.Key, fileEntry.Value));
                    }
                }

                targetPdf.Close();
                result.Add(new KeyValuePair<string, byte[]>(mainFileName, targetStream.ToArray()));
            }

            return result;
        }

        public static bool MergeFiles(string destinationFile, List<string> sourceFiles)
        {
            try
            {
                if (sourceFiles.Count == 0)
                    return false;

                using (var targetPdf = new PdfDocument(new PdfWriter(destinationFile)))
                {
                    foreach (var sourceFile in sourceFiles)
                    {
                        using (var sourcePdf = new PdfDocument(new PdfReader(sourceFile)))
                        {
                            WritePdfContent(targetPdf, sourcePdf);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public static byte[] CreatePdfFromTif(byte[] tifImage, string destinationFileName, bool deleteTempFile)
        {
            byte[] pdf = null;

            try
            {
                using (var targetStream = new FileStream(destinationFileName, FileMode.Create))
                using (var document = new PdfDocument(new PdfWriter(targetStream)))
                {
                    var pdfDoc = new Document(document);
                    var bitmap = new Bitmap(new MemoryStream(tifImage));
                    int noOfPages = bitmap.GetFrameCount(FrameDimension.Page);

                    for (int page = 0; page < noOfPages; ++page)
                    {
                        bitmap.SelectActiveFrame(FrameDimension.Page, page);

                        using (var ms = new MemoryStream())
                        {
                            bitmap.Save(ms, ImageFormat.Bmp);
                            var imageData = ImageDataFactory.Create(ms.ToArray());
                            var image = new iText.Layout.Element.Image(imageData)
                                .ScaleToFit(PageSize.A4.GetWidth(), PageSize.A4.GetHeight())
                                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

                            pdfDoc.Add(image);
                            if (page < noOfPages - 1)
                            {
                                pdfDoc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                            }
                        }
                    }

                    pdfDoc.Close();
                    pdf = File.ReadAllBytes(destinationFileName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (deleteTempFile)
                {
                    if (!string.IsNullOrEmpty(destinationFileName))
                        File.Delete(destinationFileName);
                }
            }

            return pdf;
        }

        public static byte[] CreatePdfFromTifInMemory(byte[] tifImage)
        {
            using (var output = new MemoryStream())
            {
                try
                {
                    var document = new PdfDocument(new PdfWriter(output));
                    var pdfDoc = new Document(document);
                    var bitmap = new Bitmap(new MemoryStream(tifImage));
                    int noOfPages = bitmap.GetFrameCount(FrameDimension.Page);

                    for (int page = 0; page < noOfPages; ++page)
                    {
                        bitmap.SelectActiveFrame(FrameDimension.Page, page);

                        using (var ms = new MemoryStream())
                        {
                            bitmap.Save(ms, ImageFormat.Bmp);
                            var imageData = ImageDataFactory.Create(ms.ToArray());
                            var image = new iText.Layout.Element.Image(imageData)
                                .ScaleToFit(PageSize.A4.GetWidth(), PageSize.A4.GetHeight())
                                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

                            pdfDoc.Add(image);
                            if (page < noOfPages - 1)
                            {
                                pdfDoc.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                            }
                        }
                    }

                    pdfDoc.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                return output.ToArray();
            }
        }

        #region Help methods

        public static byte[] GetDataFromStream(Stream stream)
        {
            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, (int)stream.Length);
            return data;
        }

        #endregion
    }
}
