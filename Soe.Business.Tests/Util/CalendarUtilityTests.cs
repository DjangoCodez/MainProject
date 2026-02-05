using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Common.Util.Tests
{
    [TestClass()]
    public class CalendarUtilityTests
    {
        [TestMethod()]
        public void GetBirthDateFromSecurityNumberTest()
        {
            var birthday = CalendarUtility.GetBirthDateFromSecurityNumber("19361128-9996");

            Assert.IsTrue(birthday.HasValue);
            Assert.IsTrue(birthday.Value.Year == 1936);

            birthday = CalendarUtility.GetBirthDateFromSecurityNumber("20060618-X000");
            Assert.IsTrue(birthday.HasValue);
            Assert.IsTrue(birthday.Value.Year == 2006);
        }



        [TestMethod]
        public void TestGetBirthDateFromSecurityNumber()
        {
            var test = StringUtility.SocialSecYYMMDDXXXX("19391231-****");

            // Test valid input with different formats
            Assert.AreEqual(new DateTime(1980, 5, 20), CalendarUtility.GetBirthDateFromSecurityNumber("8005201234"));
            Assert.AreEqual(new DateTime(1980, 5, 20), CalendarUtility.GetBirthDateFromSecurityNumber("800520-1234"));
            Assert.AreEqual(new DateTime(1980, 5, 20), CalendarUtility.GetBirthDateFromSecurityNumber("800520+1234"));
            Assert.AreEqual(new DateTime(1980, 5, 20), CalendarUtility.GetBirthDateFromSecurityNumber("800520_1234"));
            Assert.AreEqual(new DateTime(2000, 12, 31), CalendarUtility.GetBirthDateFromSecurityNumber("001231-1234"));
            Assert.AreEqual(new DateTime(1999, 12, 31), CalendarUtility.GetBirthDateFromSecurityNumber("991231-1234"));
            Assert.AreEqual(new DateTime(1930, 1, 1), CalendarUtility.GetBirthDateFromSecurityNumber("300101-1234"));
            Assert.AreEqual(new DateTime(1939, 12, 31), CalendarUtility.GetBirthDateFromSecurityNumber("391231-ABSC"));
            Assert.AreEqual(new DateTime(1939, 12, 31), CalendarUtility.GetBirthDateFromSecurityNumber("19391231-****"));

            // Test invalid input
            Assert.IsNull(CalendarUtility.GetBirthDateFromSecurityNumber(""));
            Assert.IsNull(CalendarUtility.GetBirthDateFromSecurityNumber("800520123"));
            Assert.IsNull(CalendarUtility.GetBirthDateFromSecurityNumber("80052012345"));
            Assert.IsNull(CalendarUtility.GetBirthDateFromSecurityNumber("800520-12345"));
            Assert.IsNull(CalendarUtility.GetBirthDateFromSecurityNumber("800520+12345"));
            Assert.IsNull(CalendarUtility.GetBirthDateFromSecurityNumber("800520_12345"));
            Assert.IsNull(CalendarUtility.GetBirthDateFromSecurityNumber("800230+abcd"));
        }

        [DataTestMethod]
        // Valid social security numbers with century and without dash
        [DataRow("191212121212", true, true, false, TermGroup_Sex.Unknown)]
        // Valid social security numbers with century and without dash
        [DataRow("19121212-1212", true, true, false, TermGroup_Sex.Unknown)]
        // Valid social security numbers with dash and without century
        [DataRow("121212-1212", true, false, true, TermGroup_Sex.Unknown)]
        [DataRow("1212121212", true, false, false, TermGroup_Sex.Unknown)]
        // Valid social security with X
        [DataRow("19770101-X111", true, false, true, TermGroup_Sex.Unknown)]

        public void TestIsValidSocialSecurityNumber(string source, bool checkValidDate, bool mustSpecifyCentury, bool mustSpecifyDash, TermGroup_Sex sex)
        {
            // Arrange

            // Act
            bool result = CalendarUtility.IsValidSocialSecurityNumber(source, checkValidDate, mustSpecifyCentury, mustSpecifyDash, sex);

            // Assert
            Assert.IsTrue(result);
        }

        [DataTestMethod]
        // Invalid social security numbers with different lengths
        [DataRow("123456789", true, false, true, TermGroup_Sex.Unknown)]
        [DataRow("1234567890", true, false, true, TermGroup_Sex.Unknown)]
        [DataRow("1234-56789", true, false, true, TermGroup_Sex.Unknown)]
        [DataRow("123456-789", true, false, true, TermGroup_Sex.Unknown)]
        // Invalid social security numbers with invalid dates
        [DataRow("000000-0000", true, false, false, TermGroup_Sex.Unknown)]
        [DataRow("200000-0000", true, false, false, TermGroup_Sex.Unknown)]
        [DataRow("300000-0000", true, false, false, TermGroup_Sex.Unknown)]
        [DataRow("991331-1234", true, false, false, TermGroup_Sex.Unknown)]
        [DataRow("20000230-0000", true, true, false, TermGroup_Sex.Unknown)]
        [DataRow("20000431-0000", true, true, false, TermGroup_Sex.Unknown)]
        [DataRow("20000229-0000", true, true, false, TermGroup_Sex.Unknown)]
        [DataRow("222222-2222", true, false, true, TermGroup_Sex.Unknown)]
        // Invalid social security numbers with invalid control digits
        [DataRow("111111-1119", true, false, true, TermGroup_Sex.Unknown)]
        [DataRow("222222-2229", true, false, true, TermGroup_Sex.Unknown)]
        [DataRow("19111111-1121", true, true, false, TermGroup_Sex.Unknown)]
        [DataRow("20121212-1221", true, true, false, TermGroup_Sex.Unknown)]
        // InValid social security with X
        [DataRow("19770101-111X", true, false, true, TermGroup_Sex.Unknown)]

        public void TestIsNotValidSocialSecurityNumber(string source, bool checkValidDate, bool mustSpecifyCentury, bool mustSpecifyDash, TermGroup_Sex sex)
        {

            // Act
            bool result = CalendarUtility.IsValidSocialSecurityNumber(source, checkValidDate, mustSpecifyCentury, mustSpecifyDash, sex);

            // Assert
            Assert.IsFalse(result);
        }
    }
}