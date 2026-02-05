using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.Config;
using System;
using System.IO;

namespace Soe.Business.Tests.Util
{
    [TestClass]
    public class FileUtilTests
    {
        [TestMethod]
        public void DeleteOldFiles_RemovesOldFiles()
        {
            // Arrange
            string folderPath = ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL;
            DateTime cutoffDate = DateTime.Now.AddDays(-7);

            // Create some old files in the folder
            string filePath1 = $"{folderPath}\\oldfile1.txt";
            string filePath2 = $"{folderPath}\\oldfile2.txt";
            string filePath3 = $"{folderPath}\\subfolder\\oldfile3.txt";

            CreateFile(filePath1, "This is an old file.", cutoffDate.AddDays(-1));
            CreateFile(filePath2, "This is another old file.", cutoffDate.AddDays(-2));
            Directory.CreateDirectory($"{folderPath}\\subfolder");
            CreateFile(filePath3, "This is a file in a subfolder.", cutoffDate.AddDays(-3));

            // Act
           FileUtil.DeleteOldFiles(folderPath, cutoffDate);

            // Assert
            Assert.IsFalse(File.Exists(filePath1));
            Assert.IsFalse(File.Exists(filePath2));
            Assert.IsFalse(File.Exists(filePath3));
        }

        private void CreateFile(string filePath, string content, DateTime lastWriteTime)
        {
            using (StreamWriter sw = File.CreateText(filePath))
            {
                sw.WriteLine(content);
            }
            File.SetLastWriteTime(filePath, lastWriteTime);
        }
    }
}


