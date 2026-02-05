using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.Util;

namespace Soe.Business.Tests.Util
{
    [TestClass]
    public class StringUtilityTests
    {
        #region Decimal
        [TestMethod]
        public void GetDecimal_ReturnsDefaultValue_WhenColumnIsNull()
        {
            // Arrange
            object column = null;
            decimal defaultValue = 0;

            // Act
            decimal result = StringUtility.GetDecimal(column, defaultValue);

            // Assert
            Assert.AreEqual(defaultValue, result);
        }

        [TestMethod]
        public void GetDecimal_ReturnsDefaultValue_WhenColumnIsEmptyString()
        {
            // Arrange
            object column = "";
            decimal defaultValue = 0;

            // Act
            decimal result = StringUtility.GetDecimal(column, defaultValue);

            // Assert
            Assert.AreEqual(defaultValue, result);
        }

        [TestMethod]
        public void GetDecimal_ReturnsDefaultValue_WhenColumnIsInvalidDecimalString()
        {
            // Arrange
            object column = "not a decimal";
            decimal defaultValue = 0;

            // Act
            decimal result = StringUtility.GetDecimal(column, defaultValue);

            // Assert
            Assert.AreEqual(defaultValue, result);
        }

        [TestMethod]
        public void GetDecimal_ReturnsDecimalValue_WhenColumnIsValidDecimalString()
        {
            // Arrange
            object column = "10.5";
            decimal defaultValue = 0;

            // Act
            decimal result = StringUtility.GetDecimal(column, defaultValue);

            // Assert
            Assert.AreEqual(10.5M, result);
        }

        [TestMethod]
        public void GetNullableDecimal_ReturnsNull_WhenColumnIsNull()
        {
            // Arrange
            object column = null;

            // Act
            decimal? result = StringUtility.GetNullableDecimal(column);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetNullableDecimal_ReturnsNull_WhenColumnIsEmptyString()
        {
            // Arrange
            object column = "";

            // Act
            decimal? result = StringUtility.GetNullableDecimal(column);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetNullableDecimal_ReturnsNull_WhenColumnIsInvalidDecimalString()
        {
            // Arrange
            object column = "not a decimal";

            // Act
            decimal? result = StringUtility.GetNullableDecimal(column);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetNullableDecimal_ReturnsDecimalValue_WhenColumnIsValidDecimalString()
        {
            // Arrange
            object column = "10.5";

            // Act
            decimal? result = StringUtility.GetNullableDecimal(column);

            // Assert
            Assert.AreEqual(10.5M, result);
        }

        #endregion

        #region String  

        [TestMethod]
        public void RemoveConsecutiveCharacters_ReturnsCorrectString()
        {
            // Act
            var newString = StringUtility.RemoveConsecutiveCharacters("tak----vaggtatning-flex-ultipro-gra-4000x1250-mm", '-');

            // Assert
            Assert.IsTrue(newString.Length == 45);
        }

        

        #endregion
    }

}


