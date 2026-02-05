using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Soe.WebApi.Util
{
    public class CalendarUtility
    {
        public static DateTime? BuildDateTimeFromString(string dateString, bool clearTime, DateTime? defaultDateTime = null)
        {
            if (String.IsNullOrEmpty(dateString) || dateString == "null")
                return defaultDateTime.HasValue ? defaultDateTime.Value : (DateTime?)null;

            if (dateString.EndsWith("Z"))
                dateString = dateString.Substring(0, dateString.Length - 1);

            Regex r = new Regex(@"^\d{4}\d{2}\d{2}T\d{2}\d{2}\d{2}$");
            if (!r.IsMatch(dateString))
                throw new FormatException(string.Format("{0} is not the correct format. Should be yyyyMMddTHHmmss", dateString));

            DateTime date = DateTime.ParseExact(dateString, "yyyyMMddTHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal);

            if (clearTime)
                date = date.Date;

            return date;
        }
    }
}