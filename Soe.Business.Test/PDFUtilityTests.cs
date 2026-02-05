using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using System.IO;

namespace SoftOne.Soe.Util.Tests
{
    [TestClass]
    public class PDFUtilityTests
    {
        private string testDirectory;
        private string destinationFileName;
        private byte[] sampleTiffImage;
        private byte[] samplePdfFile;
        private List<KeyValuePair<string, byte[]>> sampleAttachments;

        [TestInitialize]
        public void SetUp()
        {
            testDirectory = Path.Combine(Path.GetTempPath(), "PdfMergeTest");
            Directory.CreateDirectory(testDirectory);

            destinationFileName = Path.Combine(testDirectory, "merged.pdf");

            // Sample TIFF image byte array
            using (Bitmap bitmap = new Bitmap(100, 100))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.White);
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Tiff);
                    sampleTiffImage = ms.ToArray();
                }
            }

            // Sample PDF file byte array
            samplePdfFile = CreateSamplePdf();

            // Sample attachments dictionary
            sampleAttachments = new List<KeyValuePair<string, byte[]>>
            {
               new KeyValuePair<string, byte[]>( "main.pdf", samplePdfFile),
                new KeyValuePair < string, byte[] >("attachment1.pdf", samplePdfFile)
            };
        }

        [TestCleanup]
        public void TearDown()
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
        }

        [TestMethod]
        public void MergeFiles_DirectoryWithPdfsToMerge_ReturnsTrue()
        {
            // Arrange
            var pdfFile1 = Path.Combine(testDirectory, "file1.pdf");
            var pdfFile2 = Path.Combine(testDirectory, "file2.pdf");

            File.WriteAllBytes(pdfFile1, samplePdfFile);
            File.WriteAllBytes(pdfFile2, samplePdfFile);

            // Act
            bool result = PDFUtility.MergeFiles(new DirectoryInfo(testDirectory), destinationFileName, "*.pdf");

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(destinationFileName));
        }

        [TestMethod]
        public void MergeDictionary_ValidAttachments_ReturnsMergedPdf()
        {
            // Act
            var result = PDFUtility.MergeDictionary("main.pdf", sampleAttachments);

            // Assert
            var file = result.Find(x => x.Key == "main.pdf");
            Assert.IsTrue(file.Key == "main.pdf");
            Assert.IsNotNull(file.Value);
        }

        [TestMethod]
        public void MergeFiles_ValidSourceFiles_ReturnsTrue()
        {
            // Arrange
            var sourceFiles = new List<string>
            {
                Path.Combine(testDirectory, "file1.pdf"),
                Path.Combine(testDirectory, "file2.pdf")
            };

            foreach (var file in sourceFiles)
            {
                File.WriteAllBytes(file, samplePdfFile);
            }

            // Act
            bool result = PDFUtility.MergeFiles(destinationFileName, sourceFiles);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(destinationFileName));
        }

        [TestMethod]
        public void CreatePdfFromTif_ValidTifImage_ReturnsPdf()
        {
            // Act
            byte[] result = PDFUtility.CreatePdfFromTif(sampleTiffImage, destinationFileName, deleteTempFile: true);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        public void CreatePdfFromTifInMemory_ValidTifImage_ReturnsPdf()
        {
            // Act
            byte[] result = PDFUtility.CreatePdfFromTifInMemory(sampleTiffImage);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod]
        public void GetDataFromStream_ValidStream_ReturnsByteArray()
        {
            // Arrange
            byte[] expectedData = new byte[] { 1, 2, 3, 4, 5 };
            using (MemoryStream stream = new MemoryStream(expectedData))

            // Act
            {
                byte[] result = PDFUtility.GetDataFromStream(stream);

                // Assert
                CollectionAssert.AreEqual(expectedData, result);
            }
        }

        private byte[] CreateSamplePdf()
        {
            using (var output = new MemoryStream())
            {
                using (var writer = new PdfWriter(output))
                using (var pdfDoc = new PdfDocument(writer))
                using (var doc = new Document(pdfDoc))
                {
                    doc.Add(new Paragraph("Sample PDF"));
                }

                return output.ToArray();
            }
        }
    }
}
