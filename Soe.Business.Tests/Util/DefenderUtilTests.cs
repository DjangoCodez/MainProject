using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Soe.Business.Tests.Util
{
    [TestClass]
    public class DefenderUtilTests
    {
        [TestMethod]
        public void IsVirus_Returns_False_When_Defender_Not_Available()
        {
            // Arrange
            DefenderUtil.IsDefenderAvailable = false;
            var fileBytes = new byte[] { 0x41, 0x42, 0x43, 0x44 };

            // Act
            var result = DefenderUtil.IsVirus(fileBytes);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsVirusBase64_Returns_False_When_When_File_Not_Virus()
        {
            // Arrange
            var fileBytes = new byte[] { 0x41, 0x42, 0x43, 0x44 };
            var base64 = Convert.ToBase64String(fileBytes);
            // Act
            var result = DefenderUtil.IsVirusBase64(base64);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsVirus_Returns_False_When_File_Not_Virus()
        {
            // Arrange
            var fileBytes = new byte[] { 0x41, 0x42, 0x43, 0x44 }; // not a virus

            // Act
            var result = DefenderUtil.IsVirus(fileBytes);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsVirus_Returns_False_When_File_Not_Virus_Parallell()
        {
            // Arrange
            var fileBytes = new byte[] { 0x41, 0x42, 0x43, 0x44 }; // not a virus

            // Act
            Parallel.Invoke(() =>
            {
                var result1 = DefenderUtil.IsVirus(GenerateRandomBytes(1000));

                // Assert
                Assert.IsFalse(result1);
            },
            () =>
            {
                var result2 = DefenderUtil.IsVirus(GenerateRandomBytes(10000));

                // Assert
                Assert.IsFalse(result2);
            },
            () => {
                var result3 = DefenderUtil.IsVirus(GenerateRandomBytes(1000000));

                // Assert
                Assert.IsFalse(result3);
            });
        }

        private byte[] GenerateRandomBytes(int length)
        {
            byte[] buffer = new byte[length];
            new Random().NextBytes(buffer);
            return buffer;
        }

        //[TestMethod]
        //public void IsVirus_Returns_True_When_File_Is_Virus()
        //{
        //    // Arrange
        //    DefenderUtil.IsDefenderAvailable = true;
        //    var virusBytes = File.ReadAllBytes("virus.exe"); // replace with actual virus file
        //    var cancellationToken = CancellationToken.None;

        //    // Act
        //    var result = DefenderUtil.IsVirus(virusBytes, cancellationToken);

        //    // Assert
        //    Assert.IsTrue(result);
        //}
    }
}


