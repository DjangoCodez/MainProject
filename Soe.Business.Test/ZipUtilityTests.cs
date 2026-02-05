using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Util.Tests
{
    [TestClass()]
    public class ZipUtilityTests
    {
        [TestMethod]
        public void TestUnzipFilesInZipFile()
        {
            // Arrange
            var files = new Dictionary<string, byte[]>
        {
            { "file1.txt", System.Text.Encoding.UTF8.GetBytes("This is the content of file1.") },
            { "file2.txt", System.Text.Encoding.UTF8.GetBytes("This is the content of file2.") },
            { "file3.txt", System.Text.Encoding.UTF8.GetBytes("This is the content of file3.") }
        };

            byte[] zipFile;
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        var entry = archive.CreateEntry(file.Key);
                        using (var entryStream = entry.Open())
                        {
                            entryStream.Write(file.Value, 0, file.Value.Length);
                        }
                    }
                }
                zipFile = memoryStream.ToArray();
            }

            // Act
            var extractedFiles = ZipUtility.UnzipFilesInZipFile(zipFile);

            // Assert
            Assert.AreEqual(files.Count, extractedFiles.Count, "The number of extracted files should match the original number of files.");

            foreach (var file in files)
            {
                Assert.IsTrue(extractedFiles.ContainsKey(file.Key), $"Extracted files should contain {file.Key}.");
                CollectionAssert.AreEqual(file.Value, extractedFiles[file.Key], $"The content of {file.Key} should match.");
            }
        }
    }
}