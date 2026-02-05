using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.POL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Util.SalaryAdapters.PreFlight.POL.Models.Tests
{
    [TestClass()]
    public class PolCompensationTransactionTests
    {
        [TestMethod()]
        public void ToPolString_ValidData_ReturnsCorrectFormat()
        {
            // Arrange
            var transaction = new PolCompensationTransaction
            {
                Foretag = 1,
                Period = new DateTime(2023, 03, 01),
                AnstNr = 1234567890,
                Loneart = 101,
                arbeteNr = "20230301",
                kostnadsstalle = "ITDept",
                Timmar = 8.5M,
                APris = 150.0M,
                Belopp = 1275.0M
            };

            // Act
            var result = transaction.ToPolString();

            // Assert
            Assert.AreEqual("0152303123456789010120230301             0085015000127500                                                                       ITDept\r\n", result);
        }

        [TestMethod()]
        public void IsValid_ValidData_ReturnsTrue()
        {
            // Arrange
            var transaction = new PolCompensationTransaction
            {
                Foretag = 1,
                Period = new DateTime(2023, 03, 01),
                AnstNr = 1234567890,
                Loneart = 101,
                arbeteNr = "20230301",
                kostnadsstalle = "ITDept",
                Timmar = 8.5M,
                APris = 150.0M,
                Belopp = 1275.0M
            };

            // Act
            var isValid = transaction.IsValid();

            // Assert
            Assert.IsTrue(isValid);
        }

        [TestMethod()]
        public void IsValid_InvalidForetag_ReturnsFalse()
        {
            // Arrange
            var transaction = new PolCompensationTransaction
            {
                Foretag = 0, // Invalid value
                Period = new DateTime(2023, 03, 01),
                AnstNr = 1234567890,
                Loneart = 101,
                arbeteNr = "20230301",
                kostnadsstalle = "ITDept",
                Timmar = 8.5M,
                APris = 150.0M,
                Belopp = 1275.0M
            };

            // Act
            var isValid = transaction.IsValid();

            // Assert
            Assert.IsFalse(isValid);
        }

        [TestMethod()]
        public void ToPolString_ValidData_ReturnsCorrectFormatedRow()
        {
            var expectedResult = "88524030000100259007                     10952                                                                                  4363\r\n";

            // Arrange
            var transaction = new PolCompensationTransaction
            {
                Foretag = 88,
                Period = new DateTime(2024, 03, 01),
                AnstNr = 100259,
                Loneart = 7,
                arbeteNr = "",
                kostnadsstalle = "4363",
                Timmar = new decimal(109.52),
                APris = 0,
                Belopp = 0,
            };

            // Act
            var result = transaction.ToPolString();

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod()]
        public void ToPolString_ValidData_ReturnsCorrectFormatedRow2()
        {
            var expectedResult = "88524030000100259430                     06125                                                                                  4363\r\n";

            // Arrange
            var transaction = new PolCompensationTransaction
            {
                Foretag = 88,
                Period = new DateTime(2024, 03, 01),
                AnstNr = 100259,
                Loneart = 430,
                arbeteNr = "",
                kostnadsstalle = "4363",
                Timmar = new decimal(61.25),
                APris = 0,
                Belopp = 0,
            };

            // Act
            var result = transaction.ToPolString();

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod()]
        //validate absence transaction to pol string 884540300001002592402050200430     240205
        public void ToPolString_ValidData_ReturnsCorrectFormatedRow3()
        {
            var expectedResult = "884540300001002592402050200430     240205\r\n";

            // Arrange
            var transaction = new PolAbsenceTransaction
            {
                Foretag = 88,
                Period = new DateTime(2024, 03, 01),
                AnstNr = 100259,
                TransTyp = 45,
                Begynnelsedatum = new DateTime(2024, 02, 05),
                OrsaksKod = "02",
                FranvaroTid = new decimal(4.30),
                KalenderDagar = null,
                Slutdatum = new DateTime(2024, 02, 05),
            };

            // Act
            var result = transaction.ToPolString();

            // Assert
            Assert.AreEqual(expectedResult, result);
        }
    }
}