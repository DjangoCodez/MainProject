using Soe.Sys.Common.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Soe.Sys.Common.Enumerations;

namespace SoftOne.Soe.Business.Util.Tests
{
    [TestClass()]
    public class ConfigurationSetupUtilTests
    {

        private List<SysCompDBDTO> sysCompDBDTOs;

        [TestInitialize]
        public void Initialize()
        {
            // Initialize the sysCompDBDTOs list with the provided data
            sysCompDBDTOs = new List<SysCompDBDTO>
                {
                    new SysCompDBDTO { SysCompDbId = 1, ApiUrl = "https://s1s1d1.softone.se/apix/", Type = SysCompDBType.Production},
                    new SysCompDBDTO { SysCompDbId = 2, ApiUrl = "https://s2s1d2.softone.se/apix/", Type = SysCompDBType.Production},
                    new SysCompDBDTO { SysCompDbId = 4, Name = "Demomall", ApiUrl = "https://demomall.softone.se/apix/",Type = SysCompDBType.Demo },
                    new SysCompDBDTO { SysCompDbId = 5, Name = "Demo", ApiUrl = "https://demo.softone.se/apix/", Type = SysCompDBType.Demo},
                    new SysCompDBDTO { SysCompDbId = 7, ApiUrl = "https://s1s1d7.softone.se/apix/",Type = SysCompDBType.Production },
                    new SysCompDBDTO { SysCompDbId = 8, ApiUrl = "https://s4s1d8.softone.se/apix/",Type = SysCompDBType.Production },
                    new SysCompDBDTO { SysCompDbId = 9, ApiUrl = "https://development.softone.se/apix/",Type = SysCompDBType.Test },
                    new SysCompDBDTO { SysCompDbId = 18, ApiUrl = "https://s1s1d18.softone.se/apix",Type = SysCompDBType.Production },
                    new SysCompDBDTO { SysCompDbId = 19, ApiUrl = "https://s1s1d19.softone.se/apix",Type = SysCompDBType.Production }
                };
        }

        [TestMethod]
        public void GetCurrentSysCompDBId_Should_Return_Correct_SysCompDBId_For_Bridge_Folder()
        {
            // Arrange
            string currentFolder = @"E:\Sites\Bridge\";
            int expectedSysCompDBId = -1; // Update with the expected SysCompDBId for the Bridge folder

            // Act
            int actualSysCompDBId = ConfigurationSetupUtil.GetCurrentSysCompDbId(sysCompDBDTOs, currentFolder, out _);

            // Assert
            Assert.AreEqual(expectedSysCompDBId, actualSysCompDBId, $"SysCompDBId mismatch for folder: {currentFolder}");
        }

        [TestMethod]
        public void GetCurrentSysCompDBId_Should_Return_Correct_SysCompDBId_For_Demomall_Folder()
        {
            // Arrange
            string currentFolder = @"E:\Sites\Demomall\";
            int expectedSysCompDBId = 4; // The expected SysCompDBId for the Demomall folder

            // Act
            int actualSysCompDBId = ConfigurationSetupUtil.GetCurrentSysCompDbId(sysCompDBDTOs, currentFolder, out _);

            // Assert
            Assert.AreEqual(expectedSysCompDBId, actualSysCompDBId, $"SysCompDBId mismatch for folder: {currentFolder}");
        }


        [TestMethod]
        public void GetCurrentSysCompDBId_Should_Return_Correct_SysCompDBId_For_ProductionAxf_Folder()
        {
            // Arrange
            string currentFolder = @"c:\xe\Productionaxf\";
            int expectedSysCompDBId = 16; // The expected SysCompDBId for the productionaxf folder

            // Act
            int actualSysCompDBId = ConfigurationSetupUtil.GetCurrentSysCompDbId(sysCompDBDTOs, currentFolder, out _);

            // Assert
            Assert.AreEqual(expectedSysCompDBId, actualSysCompDBId, $"SysCompDBId mismatch for folder: {currentFolder}");
        }

        // Add similar test methods for the remaining folders

        // Example test method for s1d18 folder
        [TestMethod]
        public void GetCurrentSysCompDBId_Should_Return_Correct_SysCompDBId_For_s1d18_Folder()
        {
            // Arrange
            string currentFolder = @"E:\Sites\s1d18\";
            int expectedSysCompDBId = 18; // The expected SysCompDBId for the s1d2 folder

            // Act
            int actualSysCompDBId = ConfigurationSetupUtil.GetCurrentSysCompDbId(sysCompDBDTOs, currentFolder, out _);

            // Assert
            Assert.AreEqual(expectedSysCompDBId, actualSysCompDBId, $"SysCompDBId mismatch for folder: {currentFolder}");
        }

        // Example test method for s1d18 folder
        [TestMethod]
        public void GetCurrentSysCompDBId_Should_Return_Correct_SysCompDBId_For_s1d2_Folder()
        {
            // Arrange
            string currentFolder = @"E:\Sites\s1d2\";
            int expectedSysCompDBId = 2; // The expected SysCompDBId for the s1d18 folder

            // Act
            int actualSysCompDBId = ConfigurationSetupUtil.GetCurrentSysCompDbId(sysCompDBDTOs, currentFolder, out _);

            // Assert
            Assert.AreEqual(expectedSysCompDBId, actualSysCompDBId, $"SysCompDBId mismatch for folder: {currentFolder}");
        }

        [TestMethod]
        public void GetCurrentSysCompDBId_Should_Return_Correct_SysCompDBId_For_s1d51_Folder()
        {
            // Arrange
            string currentFolder = @"E:\Sites\s1d51\";
            int expectedSysCompDBId = 51; // The expected SysCompDBId for the s1d18 folder

            // Act
            int actualSysCompDBId = ConfigurationSetupUtil.GetCurrentSysCompDbId(sysCompDBDTOs, currentFolder, out _);

            // Assert
            Assert.AreEqual(expectedSysCompDBId, actualSysCompDBId, $"SysCompDBId mismatch for folder: {currentFolder}");
        }


        // Example test method for Devs1d18 folder
        [TestMethod]
        public void GetCurrentSysCompDBId_Should_Return_Correct_SysCompDBId_For_Devs1d18_Folder()
        {
            // Arrange
            string currentFolder = @"E:\Sites\Dev\Devs1d18\";
            int expectedSysCompDBId = 37; // The expected SysCompDBId for the s1d18 folder

            // Act
            int actualSysCompDBId = ConfigurationSetupUtil.GetCurrentSysCompDbId(sysCompDBDTOs, currentFolder, out string folder);

            // Assert
            Assert.AreEqual("devs1d18", folder.ToLower(), $"Folder mismatch for folder: {folder} and devs1d18");
            Assert.AreEqual(expectedSysCompDBId, actualSysCompDBId, $"SysCompDBId mismatch for folder: {currentFolder}");
        }

        // Example test method for Devs1d18 folder
        [TestMethod]
        public void GetCurrentSysCompDBId_Should_Return_Correct_SysCompDBId_For_Stages1d18_Folder()
        {
            // Arrange
            string currentFolder = @"E:\Sites\Stage\Stages1d18\";
            int expectedSysCompDBId = 31; // The expected SysCompDBId for the s1d18 folder

            // Act
            int actualSysCompDBId = ConfigurationSetupUtil.GetCurrentSysCompDbId(sysCompDBDTOs, currentFolder, out string folder);

            // Assert
            Assert.AreEqual("stages1d18", folder.ToLower(), $"Folder mismatch for folder: {folder} and stages1d18");
            Assert.AreEqual(expectedSysCompDBId, actualSysCompDBId, $"SysCompDBId mismatch for folder: {currentFolder}");
        }

        // Example test method for Devs1d18 folder
        [TestMethod]
        public void GetCurrentSysCompDBId_Should_Return_Correct_SysCompDBId_For_Stages1d8_Folder()
        {
            // Arrange
            string currentFolder = @"E:\Sites\s101s1d8\WSX\";
            int expectedSysCompDBId = 8; // The expected SysCompDBId for the s101s1d8folder

            // Act
            int actualSysCompDBId = ConfigurationSetupUtil.GetCurrentSysCompDbId(sysCompDBDTOs, currentFolder, out string folder);

            // Assert
            Assert.AreEqual("stages1d8", folder.ToLower(), $"Folder mismatch for folder: {folder} and stages1d8");
            Assert.AreEqual(expectedSysCompDBId, actualSysCompDBId, $"SysCompDBId mismatch for folder: {currentFolder}");
        }

        // Example test method for s1d18 folder
        [TestMethod]
        public void GetCurrentSysCompDBId_Should_Return_Correct_SysCompDBId_For_s5s1d18_Folder()
        {
            // Arrange
            string currentFolder = @"E:\Sites\s5s1d18\";
            int expectedSysCompDBId = 18; // The expected SysCompDBId for the s1d18 folder

            // Act
            int actualSysCompDBId = ConfigurationSetupUtil.GetCurrentSysCompDbId(sysCompDBDTOs, currentFolder, out _);

            // Assert
            Assert.AreEqual(expectedSysCompDBId, actualSysCompDBId, $"SysCompDBId mismatch for folder: {currentFolder}");
        }

        // Example test method for development folder
        [TestMethod]
        public void GetCurrentSysCompDBId_Should_Return_Correct_SysCompDBId_For_DevelopmentFolder()
        {
            // Arrange
            string currentFolder = @"C:\XE\Development";
            int expectedSysCompDBId = 9; // The expected SysCompDBId for the development folder

            // Act
            int actualSysCompDBId = ConfigurationSetupUtil.GetCurrentSysCompDbId(sysCompDBDTOs, currentFolder, out string folder);

            // Assert
            Assert.AreEqual("development", folder.ToLower(), $"Folder mismatch for folder: {folder} and Development");
            Assert.AreEqual(expectedSysCompDBId, actualSysCompDBId, $"SysCompDBId mismatch for folder: {currentFolder}");
        }


        // Example test method for development folder from bin
        [TestMethod]
        public void GetCurrentSysCompDBId_Should_Return_Correct_SysCompDBId_For_Development_BinFolder()
        {
            // Arrange
            string currentFolder = @"C:\XE\Development\Web\Bin\";
            int expectedSysCompDBId = 9; // The expected SysCompDBId for the development folder

            // Act
            int actualSysCompDBId = ConfigurationSetupUtil.GetCurrentSysCompDbId(sysCompDBDTOs, currentFolder, out string folder);

            // Assert
            Assert.AreEqual("development", folder.ToLower(), $"Folder mismatch for folder: {folder} and Development");
            Assert.AreEqual(expectedSysCompDBId, actualSysCompDBId, $"SysCompDBId mismatch for folder: {currentFolder}");
        }

        [TestMethod]
        public void GetCurrentSysCompDBId_Should_Return_Correct_SysCompDBId_For_s4_demomall()
        {
            // Arrange
            string currentFolder = @"E:\Sites\s1d4\";
            int expectedSysCompDBId = 4; // The expected SysCompDBId for the development folder

            // Act
            int actualSysCompDBId = ConfigurationSetupUtil.GetCurrentSysCompDbId(sysCompDBDTOs, currentFolder, out string folder);
           var url = ConfigurationSetupUtil.GetCurrentUrl();

            // Assert
            Assert.AreEqual("development", folder.ToLower(), $"Folder mismatch for folder: {folder} and Development");
            Assert.AreEqual(expectedSysCompDBId, actualSysCompDBId, $"SysCompDBId mismatch for folder: {currentFolder}");
        }

        [TestMethod]
        public void TestGetCurrentUrlForProductionEnvironment()
        {
            // Arrange
            var expectedUrl = "https://main.softone.se/";

            // Act
            var actualUrl = ConfigurationSetupUtil.GetCurrentUrl();

            // Assert
            Assert.AreEqual(expectedUrl, actualUrl);
        }
    }
}