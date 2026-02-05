using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Business.Core.Reporting.Matrix.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Reporting.Matrix.Models.Tests
{
    [TestClass()]
    public class FileExportMatrixTests
    {

        #region GetDate
        [TestMethod]
        public void GetDate_ReturnsNullForNullOrEmptyString()
        {
            var result = FileExportMatrix.GetDate(null);
            Assert.IsNull(result);

            result = FileExportMatrix.GetDate("");
            Assert.IsNull(result);

            result = FileExportMatrix.GetDate("   ");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetDate_ReturnsDateTimeValueForValidString()
        {
            var result = FileExportMatrix.GetDate("2023-03-19 07:55");
            Assert.AreEqual(new DateTime(2023, 3, 19, 7, 55, 0), result);

            result = FileExportMatrix.GetDate("2023-03-19");
            Assert.AreEqual(new DateTime(2023, 3, 19), result);
        }

        [TestMethod]
        public void GetDate_ReturnsDateTimeNowForDateTimeNowString()
        {
            var result = FileExportMatrix.GetDate(nameof(DateTime.Now));
            Assert.AreEqual(DateTime.Now.Date, result.Value.Date);
            Assert.AreEqual(DateTime.Now.Hour, result.Value.Hour);
            Assert.AreEqual(DateTime.Now.Minute, result.Value.Minute);
        }

        [TestMethod]
        public void GetDate_ReturnsDateTimeTodayForDateTimeTodayString()
        {
            var result = FileExportMatrix.GetDate(nameof(DateTime.Today));
            Assert.AreEqual(DateTime.Today, result);
        }

        [TestMethod]
        public void GetDate_ReturnsDateTimeTodayWithAddDayForAddDayString()
        {
            var result = FileExportMatrix.GetDate(nameof(DateTime.Today) + ".AddDays(1)");
            Assert.AreEqual(DateTime.Today.AddDays(1), result);

            result = FileExportMatrix.GetDate(nameof(DateTime.Today) + ".AddDays(-1)");
            Assert.AreEqual(DateTime.Today.AddDays(-1), result);
        }

        [TestMethod]
        public void GetDate_ReturnsDateTimeNowWithAddDayForAddDayString()
        {
            var result = FileExportMatrix.GetDate(nameof(DateTime.Now) + ".AddDays(1)");
            Assert.AreEqual(DateTime.Today.AddDays(1).Date, result.Value.Date);

            result = FileExportMatrix.GetDate(nameof(DateTime.Now) + ".AddDays(-1)");
            Assert.AreEqual(DateTime.Today.AddDays(-1).Date, result.Value.Date);
        }

        [TestMethod]
        public void GetDate_ReturnsDateTimeTodayWithAddMonthForAddMonthString()
        {
            var result = FileExportMatrix.GetDate(nameof(DateTime.Today) + ".AddMonths(1)");
            Assert.AreEqual(DateTime.Today.AddMonths(1), result);

            result = FileExportMatrix.GetDate(nameof(DateTime.Today) + ".AddMonths(-1)");
            Assert.AreEqual(DateTime.Today.AddMonths(-1), result);
        }

        #endregion

        #region GetStringValue

        [TestMethod]
        public void GetStringValue_WithReplace_ReturnsExpected()
        {
            // Arrange
            var value = "Hello, world!";
            var convert = "Replace|,|!";

            // Act
            var result = FileExportMatrix.GetStringValue(value, convert);

            // Assert
            Assert.AreEqual("Hello! world!", result);
        }

        [TestMethod]
        public void GetStringValue_WithRemoveBeginning_ReturnsExpected()
        {
            // Arrange
            var value = "1234567890";
            var convert = "RemoveBeginning|5|";

            // Act
            var result = FileExportMatrix.GetStringValue(value, convert);

            // Assert
            Assert.AreEqual("67890", result);
        }

        [TestMethod]
        public void GetStringValue_WithTruncateAt_ReturnsExpected()
        {
            // Arrange
            var value = "Hello, world!";
            var convert = "TruncateAt|5|left|";

            // Act
            var result = FileExportMatrix.GetStringValue(value, convert);

            // Assert
            Assert.AreEqual("Hello", result);
        }

        [TestMethod]
        public void GetStringValue_WithFillWithChar_ReturnsExpected()
        {
            // Arrange
            var value = "123";
            var convert = "FillWithChar|_|5|left|";

            // Act
            var result = FileExportMatrix.GetStringValue(value, convert);

            // Assert
            Assert.AreEqual("__123", result);
        }

        [TestMethod]
        public void GetStringValue_WithWhenContains_ReturnsExpected()
        {
            // Arrange
            var value = "Hello, world!";
            var convert = "WhenContains|wor|1|0|";

            // Act
            var result = FileExportMatrix.GetStringValue(value, convert);

            // Assert
            Assert.AreEqual("1", result);
        }

        [TestMethod]
        public void GetStringValue_WithWhenStartsWith_ReturnsExpected()
        {
            // Arrange
            var value = "Hello, world!";
            var convert = "WhenStartsWith|Hel|1|0|";

            // Act
            var result = FileExportMatrix.GetStringValue(value, convert);

            // Assert
            Assert.AreEqual("1", result);
        }

        [TestMethod]
        public void GetStringValue_WithFillBeginning_ReturnsExpected()
        {
            // Arrange
            var value = "world!";
            var convert = "FillBeginning|Hello, |";

            // Act
            var result = FileExportMatrix.GetStringValue(value, convert);

            // Assert
            Assert.AreEqual("Hello, world!", result);
        }
        #endregion

        [TestMethod]
        public void ConvertDecimal_BasicWithoutSeparator_ReturnsFormattedString()
        {
            // Arrange
            var value = "3.00";
            var convert = "ConvertDecimal|IIII";

            // Act
            var result = FileExportMatrix.GetStringValue(value, convert);

            // Assert
            Assert.AreEqual("0003", result);
        }

        [TestMethod]
        public void ConvertDecimal_WithColonSeparator_ReturnsFormattedString()
        {
            // Arrange
            var value = "12.34";
            var convert = "ConvertDecimal|II:DD";

            // Act
            var result = FileExportMatrix.GetStringValue(value, convert);

            // Assert
            Assert.AreEqual("12:34", result);
        }

        [TestMethod]
        public void ConvertDecimal_WithDashSeparator_ReturnsFormattedString()
        {
            // Arrange
            var value = "56.78";
            var convert = "ConvertDecimal|II-DD";

            // Act
            var result = FileExportMatrix.GetStringValue(value, convert);

            // Assert
            Assert.AreEqual("56-78", result);
        }

        [TestMethod]
        public void ConvertDecimal_IncorrectFormatString_HandlesGracefully()
        {
            // Arrange
            var value = "90.12";
            var convert = "ConvertDecimal|WrongFormat";

            // Act
            var result = FileExportMatrix.GetStringValue(value, convert);

            // Assert
            Assert.AreEqual("IncorrectFormat", result); // Replace with expected result
        }

        [TestMethod]
        public void ConvertDecimal_LargeDecimalNumber_ReturnsFormattedString()
        {
            // Arrange
            var value = "1234.5678";
            var convert = "ConvertDecimal|IIIIII.DD";

            // Act
            var result = FileExportMatrix.GetStringValue(value, convert);

            // Assert
            Assert.AreEqual("001234.56", result); 
        }


        #region FillWithChar

        [TestMethod]
        public void FillWithChar_ReturnsOriginValue_WhenTargetSizeEqualsOriginValueLength()
        {
            // Arrange
            string character = "*";
            int targetSize = 5;
            string originValue = "hello";

            // Act
            string result = FileExportMatrix.FillWithChar(character, targetSize, originValue);

            // Assert
            Assert.AreEqual(originValue, result);
        }

        [TestMethod]
        public void FillWithChar_ReturnsOriginValueWithPadding_WhenTargetSizeIsGreaterThanOriginValueLengthAndBeginningIsFalse()
        {
            // Arrange
            string character = "*";
            int targetSize = 10;
            string originValue = "hello";

            // Act
            string result = FileExportMatrix.FillWithChar(character, targetSize, originValue);

            // Assert
            Assert.AreEqual("hello*****", result);
        }

        [TestMethod]
        public void FillWithChar_ReturnsOriginValueWithPaddingAtBeginning_WhenTargetSizeIsGreaterThanOriginValueLengthAndBeginningIsTrue()
        {
            // Arrange
            string character = "*";
            int targetSize = 10;
            string originValue = "hello";

            // Act
            string result = FileExportMatrix.FillWithChar(character, targetSize, originValue, beginning: true);

            // Assert
            Assert.AreEqual("*****hello", result);
        }

        [TestMethod]
        public void FillWithChar_TruncatesOriginValue_WhenTargetSizeIsLessThanOriginValueLengthAndTruncateIsTrue()
        {
            // Arrange
            string character = "*";
            int targetSize = 3;
            string originValue = "hello";

            // Act
            string result = FileExportMatrix.FillWithChar(character, targetSize, originValue, truncate: true);

            // Assert
            Assert.AreEqual("hel", result);
        }

        [TestMethod]
        public void FillWithChar_ReturnsOriginValue_WhenTargetSizeIsLessThanOriginValueLengthAndTruncateIsFalse()
        {
            // Arrange
            string character = "*";
            int targetSize = 3;
            string originValue = "hello";

            // Act
            string result = FileExportMatrix.FillWithChar(character, targetSize, originValue);

            // Assert
            Assert.AreEqual(originValue, result);
        }

        [TestMethod]
        public void FillWithChar_ReturnsOriginValue_WhenCharacterIsNull()
        {
            // Arrange
            string character = null;
            int targetSize = 5;
            string originValue = "hello";

            // Act
            string result = FileExportMatrix.FillWithChar(character, targetSize, originValue);

            // Assert
            Assert.AreEqual(originValue, result);
        }

        [TestMethod]
        public void FillWithChar_ReturnsOriginValue_WhenCharacterIsEmptyString()
        {
            // Arrange
            string character = "";
            int targetSize = 5;
            string originValue = "hello";

            // Act
            string result = FileExportMatrix.FillWithChar(character, targetSize, originValue);

            // Assert
            Assert.AreEqual(originValue, result);
        }

        #endregion

        #region SkipRow

        [TestMethod]
        public void SkipRowBasedOnColumn_NoRules_ReturnsFalse()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO { ConvertValue = "" };
            string value = "some value";

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_SkipRowIfEmptyOrZero_EmptyValue_ReturnsTrue()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO { ConvertValue = "SkipRowIfEmptyOrZero" };
            string value = "";

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_SkipRowIfEmptyOrZero_ZeroIntValue_ReturnsTrue()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO { ConvertValue = "SkipRowIfEmptyOrZero" };
            string value = "0";

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_SkipRowIfEmptyOrZero_ZeroDecimalValue_ReturnsTrue()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO { ConvertValue = "SkipRowIfEmptyOrZero" };
            string value = "0.0";

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_SkipRowIfEmptyOrZero_NonEmptyValue_ReturnsFalse()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO { ConvertValue = "SkipRowIfEmptyOrZero" };
            string value = "some value";

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_SkipRowIfNowEmpty_NonEmptyValue_ReturnsTrue()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO { ConvertValue = "SkipRowIfNowEmpty" };
            string value = "some value";

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_SkipRowIfNowEmpty_EmptyValue_ReturnsFalse()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO { ConvertValue = "SkipRowIfNowEmpty" };
            string value = "";

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_SkipRowIfValue_ValueMatches_ReturnsTrue()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO { ConvertValue = "SkipRowIfValue|some value" };
            string value = "some value";

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsTrue(result);

        }

        [TestMethod]
        public void SkipRowBasedOnColumn_ReturnsTrue_WhenValueIsBeforeLimitDate()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO
            {
                ConvertValue = "SkipRowIfDateOlder|2022-01-01"
            };
            var value = "2021-12-31";

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_ReturnsTrue_WhenValueIsBeforeLimitDateWithToDayAddDays()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO
            {
                ConvertValue = "SkipRowIfDateOlder|Today.AddDays(-11)"
            };
            var value = DateTime.Today.AddDays(-20).ToShortDateString();

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_ReturnsFalse_WhenValueIsAfterLimitDateWithToDayAddMonths()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO
            {
                ConvertValue = "SkipRowIfDateOlder|Today.AddMonths(-11)"
            };
            var value = DateTime.Today.AddMonths(-10).ToShortDateString();

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_ReturnsFalse_WhenValueIsAfterLimitDate()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO
            {
                ConvertValue = "SkipRowIfDateOlder|2022-01-01"
            };
            var value = "2022-01-02";

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_ReturnsFalse_WhenValueIsNotADate()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO
            {
                ConvertValue = "SkipRowIfDateOlder|2022-01-01"
            };
            var value = "not a date";

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_ReturnsFalse_WhenValueIsEmpty()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO
            {
                ConvertValue = "SkipRowIfDateOlder|2022-01-01"
            };
            var value = "";

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_ReturnsFalse_WhenValueIsNull()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO
            {
                ConvertValue = "SkipRowIfDateOlder|2022-01-01"
            };
            string value = null;

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_ReturnsFalse_WhenRuleHasInvalidDate()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO
            {
                ConvertValue = "SkipRowIfDateOlder|2022-02-31"
            };
            string value = "2022-02-01";

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_ShouldReturnTrue_WhenValueContainsSkipRowIfContainsRule()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO
            {
                ConvertValue = "SkipRowIfContains|abc^SkipRowIfContains|xyz"
            };
            var value = "some abc value";

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_ShouldReturnFalse_WhenValueDoesNotContainSkipRowIfContainsRule()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO
            {
                ConvertValue = "SkipRowIfContains|abc^SkipRowIfContains|xyz"
            };
            var value = "some other value";

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_ShouldReturnFalse_WhenColumnDoesNotHaveSkipRowIfContainsRule()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO
            {
                ConvertValue = "SomeOtherRule"
            };
            var value = "some abc value";

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_ShouldReturnFalse_WhenValueIsEmptyString()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO
            {
                ConvertValue = "SkipRowIfContains|abc^SkipRowIfContains|xyz"
            };
            var value = "";

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void SkipRowBasedOnColumn_ShouldReturnFalse_WhenValueIsNull()
        {
            // Arrange
            var column = new ExportDefinitionLevelColumnDTO
            {
                ConvertValue = "SkipRowIfContains|abc^SkipRowIfContains|xyz"
            };
            string value = null;

            // Act
            var result = FileExportMatrix.SkipRowBasedOnColumn(column, value);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion
    }
}
