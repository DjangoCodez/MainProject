using SoftOne.Soe.Common.Util;
using System;
using System.Linq;

namespace Soe.Business.Tests.Business.OrganizationStructure
{
    public static class DataGenerationHelper
    {
        public static int GenerateRandomInt(int min = 0, int maxExclusive = int.MaxValue)
        {
            var rnd = new Random();
            return rnd.Next(min, maxExclusive);
        }
        public static string GenerateRandomString(int maxLength)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable
                .Repeat(chars, maxLength)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());
        }
        public static string GenerateSwedishSSN(out TermGroup_Sex sex)
        {
            var rnd = new Random();

            // Random birth date between 1950-01-01 and 2005-12-31
            var startDate = new DateTime(1950, 1, 1);
            var endDate = new DateTime(2005, 12, 31);
            int range = (endDate - startDate).Days;
            var birthDate = startDate.AddDays(rnd.Next(range));
            string datePart = birthDate.ToString("yyyyMMdd");

            // Random 2-digit serial (00-99)
            int serialPrefix = rnd.Next(0, 100);
            string serialPrefixPart = serialPrefix.ToString("D2");

            // Sex digit: odd for male, even for female
            bool isMale = rnd.Next(2) == 0;
            int sexDigit = isMale ? 1 + 2 * rnd.Next(0, 5) : 2 * rnd.Next(0, 5);

            // Random digit (0-9) for checksum (not Luhn, just random for mock)
            int checksum = rnd.Next(0, 10);

            // Format: YYYYMMDD-XXXX
            string ssn = $"{datePart}-{serialPrefixPart}{sexDigit}{checksum}";
            sex = isMale ? TermGroup_Sex.Male : TermGroup_Sex.Female;
            return ssn;
        }
    }
}
