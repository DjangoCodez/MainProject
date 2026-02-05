using SoftOne.Soe.Common.DTO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Common.Util
{
    public static class CalendarUtility
    {
        #region Properties

        public static readonly DateTime DATETIME_MINVALUE = DateTime.Parse("1753-01-01", CultureInfo.InvariantCulture);
        public static readonly DateTime DATETIME_MAXVALUE = DateTime.Parse("9999-12-31", CultureInfo.InvariantCulture);
        public static readonly DateTime DATETIME_0VALUE = DateTime.Parse("0001-01-01", CultureInfo.InvariantCulture);
        public static readonly DateTime DATETIME_DEFAULT = DateTime.Parse("1900-01-01", CultureInfo.InvariantCulture);   
        public static readonly string URL_FRIENDLY_DATETIME_DEFAULT = DATETIME_DEFAULT.ToString("yyyyMMdd");
        public static readonly string SHORTDATEMASK = "yyMMdd";

        #endregion

        #region Culture

        public static CultureInfo GetValidCultureInfo(string cultureCode)
        {
            CultureInfo culture = null;

            try
            {
                culture = new CultureInfo(cultureCode);
            }
            catch (Exception ex)
            {
                ex.ToString(); //prevent compiler warning
                culture = null;
            }

            return culture;
        }

        #endregion

        #region Date

        #region SQL Server DateTime

        /// <summary>
        /// Check that the DateTime has a valid SQL Server range
        /// </summary>
        /// <param name="source">The DateTime</param>
        /// <param name="preserveTime">If true, keep the time part of the date when converting to a valid date</param>
        /// <returns>1900-01-01 if Date is invalid</returns>
        public static DateTime GetValidSqlServerDateTime(DateTime source, bool preserveTime)
        {
            if (IsDateTimeSqlServerValid(source))
                return source;

            if (preserveTime)
                return MergeDateAndTime(DATETIME_DEFAULT, source);

            return DATETIME_DEFAULT;
        }

        /// <summary>
        /// Check that the DateTime has a valid SQL Server range
        /// </summary>
        /// <param name="source">The DateTime</param>
        /// <param name="defaultDateTime">The default DateTime</param>
        /// <returns>The default DateTime if Date is invalid</returns>
        public static DateTime GetValidSqlServerDateTime(DateTime source, DateTime defaultDateTime)
        {
            if (!IsDateTimeSqlServerValid(source))
            {
                if (IsDateTimeSqlServerValid(defaultDateTime))
                    return defaultDateTime;
                return DATETIME_DEFAULT;
            }
            return source;
        }

        public static bool IsDateTimeSqlServerValid(DateTime source)
        {
            // SQL Server can only store dates between 1753-01-01 and 9999-12-31
            if (source < DATETIME_MINVALUE || source > DATETIME_MAXVALUE)
                return false;
            return true;
        }

        #endregion

        #region DateTime

        public static List<DateTime> GetDates(DateTime? startDate, DateTime? stopDate, DateTime? defaultDateIfEmpty = null)
        {
            List<DateTime> dates = new List<DateTime>();

            if (startDate.HasValue && stopDate.HasValue)
                dates.AddRange(GetDates(startDate.Value, stopDate.Value));
            else if (startDate.HasValue)
                dates.Add(startDate.Value);
            else if (stopDate.HasValue)
                dates.Add(stopDate.Value);

            if (dates.Count == 0 && defaultDateIfEmpty.HasValue)
                dates.Add(defaultDateIfEmpty.Value);

            return dates;
        }

        public static List<DateTime> GetDates(DateTime startDate, DateTime stopDate)
        {
            List<DateTime> dates = new List<DateTime>();
            DateTime currentDate = startDate;
            while (currentDate <= stopDate)
            {
                dates.Add(currentDate);
                currentDate = currentDate.AddDays(1);
            }
            return dates;
        }

        public static List<DateTime> GetIntersectedDates(List<DateRangeDTO> ranges, DateTime b1, DateTime b2)
        {
            List<DateTime> intersectsDates = new List<DateTime>();
            foreach (var range in ranges)
            {
                intersectsDates.AddRange(GetIntersectedDates(range.Start.Date, range.Stop.Date, b1, b2));
            }

            return intersectsDates;
        }

        public static List<DateTime> GetIntersectedDates(DateTime a1, DateTime a2, DateTime b1, DateTime b2)
        {
            List<DateTime> dates = GetDates(a1, a2);
            List<DateTime> intersectsDates = new List<DateTime>();

            foreach (var date in dates)
            {
                if (b1 <= date && b2 >= date)
                    intersectsDates.Add(date.Date);
            }
            return intersectsDates;
        }

        public static List<DateTime> GetDates(DayOfWeek dayOfWeek, DateTime startDate, DateTime stopDate)
        {
            List<DateTime> dates = GetDates(startDate, stopDate);

            return dates.Where(d => d.DayOfWeek == dayOfWeek).ToList();
        }

        public static List<DateTime> GetDatesBack(DateTime date, int days)
        {
            return GetSurroundingDates(date, days, true);
        }

        public static List<DateTime> GetDatesForward(DateTime date, int days)
        {
            return GetSurroundingDates(date, days, false);
        }

        public static List<DateTime> GetSurroundingDates(DateTime date, int daysBack, int daysForward)
        {
            List<DateTime> datesToRecalculate = new List<DateTime>();
            datesToRecalculate.AddRange(GetDatesBack(date, daysBack));
            datesToRecalculate.AddRange(GetDatesForward(date, daysForward));
            return datesToRecalculate.OrderBy(i => i.Date).ToList();
        }

        public static List<DateTime> GetSurroundingDates(DateTime date, int days, bool back)
        {
            List<DateTime> dates = new List<DateTime>();

            if (days > 0)
            {
                for (int day = 1; day <= days; day++)
                {
                    dates.Add(date.AddDays(back ? -day : day));
                }
            }

            return dates.OrderBy(i => i.Date).ToList();
        }

        public static List<DateTime> GetDatesInWeek(DateTime from)
        {
            return GetDatesInInterval(AdjustDateToBeginningOfWeek(from), AdjustDateToBeginningOfWeek(from).AddDays(6));
        }

        public static List<DateTime> GetDatesInInterval(DateTime from, DateTime to)
        {
            List<DateTime> dates = new List<DateTime>();

            if (from > to)
                return new List<DateTime>();
            if (from.Date == to.Date)
                return new List<DateTime>() { from.Date };

            to = GetEndOfDay(to);

            double days = to.Subtract(from).TotalDays;
            for (int i = 0; i < days; i++)
            {
                dates.Add(from.AddDays(i));
            }

            return dates.OrderBy(i => i.Date).ToList();
        }

        public static List<DateTime> MergeDateTimes(List<DateTime> dateTimes1, List<DateTime> dateTimes2)
        {
            List<DateTime> dateTimes = new List<DateTime>();
            dateTimes.AddRange(dateTimes1);
            foreach (DateTime dateTime in dateTimes2)
            {
                if (!dateTimes.Contains(dateTime))
                    dateTimes.Add(dateTime);
            }
            return dateTimes.OrderBy(i => i.Date).ToList();
        }

        public static List<WorkIntervalDTO> MergeIntervals(List<WorkIntervalDTO> tuples1, List<WorkIntervalDTO> tuples2, bool checkEndsInInterval = false)
        {
            var tuples = new List<WorkIntervalDTO>();

            var allTuples = new List<WorkIntervalDTO>();
            allTuples.AddRange(tuples1);
            allTuples.AddRange(tuples2);

            foreach (var idGrouping in allTuples.GroupBy(i => i.Id))
            {
                foreach (var tuple in idGrouping.OrderBy(i => i.StartTime).ThenBy(i => i.StopTime))
                {
                    if (tuples.Any(i => i.StartTime == tuple.StartTime && i.StopTime == tuple.StopTime))
                        continue;

                    var adjacentTuple = tuples.FirstOrDefault(i => i.StopTime == tuple.StartTime);
                    var endsInIntervalTuple = tuples.FirstOrDefault(i => i.StopTime > tuple.StartTime && i.StopTime <= tuple.StopTime);
                    if (adjacentTuple != null)
                    {
                        tuples.Remove(adjacentTuple);
                        var newInterval = new WorkIntervalDTO(idGrouping.Key, adjacentTuple.StartTime, tuple.StopTime);
                        newInterval.HasBilagaJ = adjacentTuple.HasBilagaJ || tuple.HasBilagaJ;
                        tuples.Add(newInterval);
                    }else if (checkEndsInInterval && endsInIntervalTuple != null)
                    {
                        tuples.Remove(endsInIntervalTuple);
                        var newInterval = new WorkIntervalDTO(idGrouping.Key, endsInIntervalTuple.StartTime, tuple.StopTime);
                        newInterval.HasBilagaJ = endsInIntervalTuple.HasBilagaJ || tuple.HasBilagaJ;
                        tuples.Add(newInterval);
                    }
                    else
                    {
                        tuples.Add(tuple);
                    }
                }
            }

            return tuples.Where(i => i.StartTime < i.StopTime).OrderBy(i => i.StartTime).ThenBy(i => i.StopTime).ToList();
        }

        public static void MinAndMaxToNull(ref DateTime? dateFrom, ref DateTime? dateTo)
        {
            if (dateFrom.HasValue && dateFrom.Value == DATETIME_MINVALUE)
                dateFrom = null;
            if (dateTo.HasValue && dateTo.Value == DATETIME_MAXVALUE)
                dateTo = null;
        }

        public static void NullToToday(ref DateTime? dateFrom, ref DateTime? dateTo)
        {
            if (!dateFrom.HasValue)
                dateFrom = DateTime.Today;
            if (!dateTo.HasValue)
                dateTo = DateTime.Today;
        }

        public static (DateTime DateFrom, DateTime DateTo) GetWeek(DateTime? date)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            return (CalendarUtility.GetBeginningOfWeek(date), CalendarUtility.GetEndOfWeek(date));
        }

        public static DateTime GetBeginningOfDay(DateTime? date = null, int addMinutes = 0)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            return new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 0, 0, 0).AddMinutes(addMinutes);
        }

        public static DateTime GetBeginningOfWeek(DateTime? date = null, DayOfWeek startOfWeek = DayOfWeek.Monday, int addMinutes = 0)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            DateTime firstDayInWeek = date.Value.Date;
            while (firstDayInWeek.DayOfWeek != startOfWeek)
            {
                firstDayInWeek = firstDayInWeek.AddDays(-1);
            }

            return firstDayInWeek.AddMinutes(addMinutes);
        }

        public static DateTime GetBeginningOfMonth(DateTime? date = null, int addMinutes = 0)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            return new DateTime(date.Value.Year, date.Value.Month, 1, 0, 0, 0).AddMinutes(addMinutes);
        }

        public static DateTime GetBeginningOfQuarter(DateTime? date = null, int addMinutes = 0)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            int month = date.Value.Month;
            List<int> quarterMonths = new List<int>() { 1, 4, 7, 10 };

            while (!quarterMonths.Contains(month))
            {
                month--;
            }

            return new DateTime(date.Value.Year, month, 1, 0, 0, 0).AddMinutes(addMinutes);
        }

        public static DateTime GetBeginningOfHalfYear(DateTime? date = null, int addMinutes = 0)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            int month = date.Value.Month;
            List<int> quarterMonths = new List<int>() { 1, 7 };

            while (!quarterMonths.Contains(month))
            {
                month--;
            }

            return new DateTime(date.Value.Year, month, 1, 0, 0, 0).AddMinutes(addMinutes);
        }

        public static DateTime GetBeginningOfYear(DateTime? date = null, int addMinutes = 0)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            return new DateTime(date.Value.Year, 1, 1, 0, 0, 0).AddMinutes(addMinutes);
        }

        public static DateTime GetBeginningOfNextYear(DateTime? date = null, int addMinutes = 0)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            return new DateTime(date.Value.AddYears(1).Year, 1, 1, 0, 0, 0).AddMinutes(addMinutes);
        }

        public static DateTime GetEndOfDay(DateTime? date = null, int addMinutes = 0)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            if (date == DATETIME_MAXVALUE)
                return date.Value;

            return new DateTime(date.Value.Year, date.Value.Month, date.Value.Day, 23, 59, 59).AddMinutes(addMinutes);
        }

        public static DateTime GetEndOfWeek(DateTime? date, DayOfWeek startOfWeek = DayOfWeek.Monday, int addMinutes = 0)
        {
            DateTime firstDayInWeek = GetBeginningOfWeek(date, startOfWeek);
            return firstDayInWeek.AddDays(7).AddSeconds(-1).AddMinutes(addMinutes);
        }

        public static DateTime GetEndOfMonth(DateTime? date = null, int addMinutes = 0)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            if (date == DATETIME_MAXVALUE)
                return date.Value;

            return new DateTime(date.Value.Year, date.Value.Month, DateTime.DaysInMonth(date.Value.Year, date.Value.Month), 23, 59, 59).AddMinutes(addMinutes);
        }

        public static DateTime GetEndOfQuarter(DateTime? date = null, int addMinutes = 0)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            int month = date.Value.Month;
            List<int> quarterMonths = new List<int>() { 3, 6, 9, 12 };

            while (!quarterMonths.Contains(month))
            {
                month++;
            }

            return new DateTime(date.Value.Year, month, DateTime.DaysInMonth(date.Value.Year, month), 23, 59, 59).AddMinutes(addMinutes);
        }

        public static DateTime GetEndOfHalfYear(DateTime? date = null, int addMinutes = 0)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            int month = date.Value.Month;
            List<int> quarterMonths = new List<int>() { 6, 12 };

            while (!quarterMonths.Contains(month))
            {
                month++;
            }

            return new DateTime(date.Value.Year, month, DateTime.DaysInMonth(date.Value.Year, month), 23, 59, 59).AddMinutes(addMinutes);
        }

        public static DateTime GetEndOfYear(DateTime? date = null, int addMinutes = 0)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            return new DateTime(date.Value.Year, 12, 31, 23, 59, 59).AddMinutes(addMinutes);
        }

        public static DateTime GetEndOfYear(int year)
        {
            return new DateTime(year, 12, 31);
        }

        public static DateTime GetEarliestDate(params DateTime?[] dates)
        {
            return dates.Any(i => i.HasValue) ? dates.Where(i => i.HasValue).OrderBy(i => i.Value).FirstOrDefault().Value : DATETIME_DEFAULT;
        }

        public static DateTime? GetEarliestDate(DateTime? date1, DateTime? date2)
        {
            if (!date1.HasValue && !date2.HasValue)
                return null;
            if (!date1.HasValue)
                return date2;
            if (!date2.HasValue)
                return date1;

            return date1 <= date2 ? date1 : date2;
        }

        public static DateTime GetEarliestDate(DateTime date1, DateTime? date2)
        {
            if (!date2.HasValue)
                return date1;

            return date1 <= date2.Value ? date1 : date2.Value;
        }

        public static DateTime GetLatestDate(params DateTime?[] dates)
        {
            return dates.Any(i => i.HasValue) ? dates.Where(i => i.HasValue).OrderByDescending(i => i.Value).FirstOrDefault().Value : DATETIME_DEFAULT;
        }

        public static DateTime? GetLatestDate(DateTime? date1, DateTime? date2)
        {
            if (!date1.HasValue && !date2.HasValue)
                return null;
            if (!date1.HasValue)
                return date2;
            if (!date2.HasValue)
                return date1;

            return date1 >= date2 ? date1 : date2;
        }

        public static DateTime GetLatestDate(DateTime date1, DateTime? date2)
        {
            if (!date2.HasValue)
                return date1;

            return date1 >= date2.Value ? date1 : date2.Value;
        }

        public static DateTime ApplyTimeOnDateTime(DateTime date, string time)
        {
            //Set default time to 00:00:00
            date = date.Date;

            char[] splitter = { ':' };
            string strTime = time;

            strTime = strTime.Replace(".", ":").Replace(",", ":").Replace(";", ":");
            if (!strTime.IsNullOrEmpty())
            {
                if (strTime.StartsWith("-"))
                    strTime = strTime.Substring(1, strTime.Length - 1);

                string[] timeParts = strTime.Split(splitter);
                if (timeParts.Length >= 1)
                {
                    int hour = GetValidHour(Convert.ToInt32(timeParts[0]));
                    int minute = 0;

                    if (timeParts.Length >= 2)
                        minute = GetValidMinute(Convert.ToInt32(timeParts[1]));

                    date = new DateTime(date.Year, date.Month, date.Day, hour, minute, 0, DateTimeKind.Local);
                }
            }

            return date;
        }

        public static DateTime? GetBreakTime(DateTime date)
        {
            DateTime validDate = GetValidSqlServerDateTime(date, true);
            if (validDate.Date == DATETIME_DEFAULT && validDate.Hour == 0 && validDate.Minute == 0 && validDate.Second == 0)
                return null;

            return MergeDateAndTime(DATETIME_DEFAULT, validDate);
        }

        public static DateTime? GetNullableDateTime(object column)
        {
            if (!StringUtility.HasValue(column))
                return null;

            return GetNullableDateTime(column.ToString());
        }

        public static DateTime? GetNullableDateTime(string source)
        {
            DateTime? value = null;
            if (!String.IsNullOrEmpty(source))
            {
                source = source.Trim();
                string[] formats = new[]
                {
                    "yyyyMMdd",
                    "yyyy-MM-dd",
                    "yyyy-MM-dd HH:mm",
                    "yyyy-MM-dd HH:mm:ss",
                    "yyyy-MM-ddTHH:mm:ss",
                    "d.M.yyyy",
                    "d.M.yyyy HH:mm",
                    "d.M.yyyy H:mm",
                    "d.M.yyyy HH:mm:ss",
                };

                if (DateTime.TryParseExact(source, formats, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out DateTime date))
                    value = date;
            }
            return value;
        }

        public static DateTime? GetNullableDateTime(string source, string format)
        {
            DateTime? value = null;
            if (!string.IsNullOrEmpty(source) && DateTime.TryParseExact(source, format, null, DateTimeStyles.None, out DateTime date))
                value = date;
            return value;
        }

        public static DateTime? ToNullableDateTime(string source)
        {
            if (!string.IsNullOrEmpty(source))
            {
                DateTime.TryParse(source, out DateTime date);
                return date;
            }
            else
                return null;
        }

        public static DateTime GetDateTime(string source, string format = "")
        {
            return GetDateTime(source, DATETIME_DEFAULT, format);
        }

        public static DateTime GetDateTime(string source, DateTime defaultValue, string format = "")
        {
            DateTime value;

            if (!string.IsNullOrEmpty(source))
            {
                if (string.IsNullOrEmpty(format))
                {
                    if (DateTime.TryParse(source, out value))
                        return value;
                }
                else
                {
                    if (DateTime.TryParseExact(source, format, null, DateTimeStyles.None, out value))
                        return value;
                }
            }

            return defaultValue;
        }

        public static DateTime GetDateTime(DateTime time)
        {
            return GetDateTime(time.Hour, time.Minute, time.Second);
        }

        public static DateTime GetDateTime(int hour, int minute, int second = 0, int daysOffset = 0)
        {
            return GetDateTime(DATETIME_DEFAULT.AddDays(daysOffset), hour, minute, second);
        }

        public static DateTime GetDateTime(DateTime date, DateTime time)
        {
            return GetDateTime(date, time.Hour, time.Minute, time.Second);
        }

        public static DateTime GetDateTime(DateTime date, int hour, int minute, int second = 0)
        {
            return new DateTime(date.Year, date.Month, date.Day, hour, minute, second);
        }

        public static DateTime GetDateTime(TimeSpan timeSpan)
        {
            return new DateTime(DATETIME_DEFAULT.Year, DATETIME_DEFAULT.Month, DATETIME_DEFAULT.Day).Add(timeSpan);
        }

        public static DateTime GetMiddleTime(DateTime start, DateTime stop)
        {
            if (start > stop)
            {
                // Swap start and stop
                DateTime tmp = start;
                start = stop;
                stop = tmp;
            }

            // Calculate time span between the dates
            double lenght = (stop - start).TotalMinutes;
            // Get time in the middle of the time span
            lenght /= 2;
            return start.AddMinutes(lenght);
        }

        public static DateTime GetScheduleTime(DateTime scheduledTime)
        {
            DateTime date = DATETIME_DEFAULT;
            return new DateTime(date.Year, date.Month, date.Day, scheduledTime.Hour, scheduledTime.Minute, scheduledTime.Second);
        }

        public static DateTime GetScheduleTime(DateTime time, DateTime startDate, DateTime stopDate)
        {
            return GetDateTime(time).AddDays((stopDate.Date - startDate.Date).Days);
        }

        public static DateTime GetActualDateTime(DateTime date, DateTime time)
        {
            return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
        }

        public static DateTime MergeDateAndDefaultTime(DateTime date, DateTime? time, bool adjust1899 = false)
        {
            if (!time.HasValue)
                time = DATETIME_DEFAULT;

            if (time.Value.Date == DATETIME_DEFAULT.AddDays(1))
                date = date.AddDays(1);

            if (adjust1899 && time.Value.Date == DATETIME_DEFAULT.AddDays(-1))
                date = date.AddDays(-1);

            return MergeDateAndTime(date, time.Value);
        }

        public static DateTime MergeDateAndTime(DateTime date, DateTime? time, bool ignoreSqlServerDateTime = false)
        {
            if (time == null)
                time = DATETIME_DEFAULT;

            return MergeDateAndTime(date, time.Value, ignoreSqlServerDateTime);
        }

        public static DateTime MergeDateAndTime(DateTime date, DateTime time, bool ignoreSqlServerDateTime = false, bool handleDefaultTimeAfterMidnight = false)
        {
            if (!ignoreSqlServerDateTime)
                date = GetValidSqlServerDateTime(date, false);

            if (handleDefaultTimeAfterMidnight && time.Date == DATETIME_DEFAULT.AddDays(1))
                return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second).AddDays(1);
            else
                return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
        }

        public static DateTime MergeDateAndTime(DateTime date, TimeSpan time)
        {
            if (time.TotalSeconds < 0)
                time = TimeSpan.FromSeconds(time.TotalSeconds * -1);

            date = GetValidSqlServerDateTime(date, false);
            return new DateTime(date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds);
        }

        public static DateTime? GetDateTimeInInterval(DateTime date, DateTime? dateFrom, DateTime? dateTo)
        {
            //Case 1: if the date is within range, use date passed
            //Case 2: if the date is after dateTo, use dateTo
            //Case 3: if the date is before datefrom, use datefrom    

            DateTime? validDate = null;

            if (dateFrom.HasValue && dateTo.HasValue)
            {
                if (dateFrom.Value.Date <= date && dateTo.Value.Date >= date) //Case 1 
                    validDate = date;
                else if (dateTo.Value.Date <= date) //Case 2
                    validDate = dateTo.Value.Date;
                else if (dateFrom.Value.Date >= date) //Case 3
                    validDate = dateFrom.Value.Date;
            }
            else if (!dateFrom.HasValue && dateTo.HasValue)
            {
                if (dateTo.Value.Date >= date) //Case 1
                    validDate = date;
                else if (dateTo.Value.Date <= date) //Case 2
                    validDate = dateTo.Value.Date;
            }
            else if (dateFrom.HasValue)
            {
                if (dateFrom.Value.Date <= date) //Case 1
                    validDate = date;
                else if (dateFrom.Value.Date >= date)  //Case 3
                    validDate = dateFrom.Value.Date;
            }
            else
            {
                validDate = date; //Case 1
            }

            return validDate;
        }

        public static DateTime AdjustDateTimeByPercent(DateTime dateTime, double percentalAdjustment)
        {
            percentalAdjustment = 1 - (percentalAdjustment / 100);
            int minutes = dateTime.Minute + (dateTime.Hour * 60);
            int diff = (int)Math.Floor(minutes * percentalAdjustment);
            dateTime = dateTime.Subtract(new TimeSpan(0, minutes - diff, 0));

            return dateTime;
        }

        public static DateTime AdjustToEndOfInterval(DateTime time, int interval)
        {
            // Use AdjustAccordingToInterval to get the interval start
            DateTime adjusted = CalendarUtility.AdjustAccordingToInterval(time, 0, interval, true);

            // Add the interval to the start to get the end
            DateTime intervalEnd = adjusted.AddMinutes(interval);

            // If the time is after the adjusted time, return the interval end
            if (time > adjusted)
                return intervalEnd;

            // Otherwise, return the adjusted start
            return adjusted;
        }


        public static DateTime AdjustAccordingToInterval(DateTime time, int minutes, int interval, bool alwaysReduce = false)
        {
            int minutesFromMidnight = Convert.ToInt32((time - time.Date).TotalMinutes);

            if (minutesFromMidnight == 0)
                return time;

            minutesFromMidnight = AdjustAccordingToInterval(minutesFromMidnight, interval, alwaysReduce);

            return time.Date.AddMinutes(minutesFromMidnight);
        }

        public static DateTime IncreaseDateTimeByPercent(DateTime startTime, int totalWorkMinutes, decimal percentalWorkTime)
        {
            percentalWorkTime = 1 - (percentalWorkTime / 100);
            int percentInMinutes = (int)Math.Floor(Convert.ToDouble(totalWorkMinutes * percentalWorkTime));
            startTime = startTime.AddMinutes(percentInMinutes);

            return startTime;
        }

        public static DateTime DecreaseDateTimeByPercent(DateTime stopTime, int totalWorkMinutes, decimal percentalWorkTime)
        {
            percentalWorkTime = 1 - (percentalWorkTime / 100);
            int percentInMinutes = (int)Math.Floor(Convert.ToDouble(totalWorkMinutes * percentalWorkTime));
            stopTime = stopTime.Subtract(new TimeSpan(0, percentInMinutes, 0));

            return stopTime;
        }

        public static DateTime GetDateFromMinutes(int timeInMinutes, bool handlePreviousNextDay = false)
        {
            if (handlePreviousNextDay)
            {
                #region Handle Previous/Next day

                bool isPreviousDay = false;
                bool isNextDay = false;

                //Previous day
                if (timeInMinutes < 0)
                {
                    timeInMinutes = (24 * 60) + timeInMinutes;
                    isPreviousDay = true;
                }

                //Hours
                int hours = timeInMinutes / 60;

                //Next day
                if (hours > 23)
                {
                    if ((hours - 24) < 24)
                    {
                        hours -= 24;
                        isNextDay = true;
                    }
                    else if (hours >= 48)
                    {
                        hours = 23;
                        isNextDay = true;
                        timeInMinutes = (60 * 48) - 1;
                    }
                }

                //Minutes
                int minutes = (timeInMinutes - (hours * 60));
                if (isNextDay)
                    minutes -= (24 * 60);

                //Date
                DateTime date = DATETIME_DEFAULT;
                if (isPreviousDay)
                    date = date.AddDays(-1);
                else if (isNextDay)
                    date = date.AddDays(1);

                AdjustToValidTime(ref hours, ref minutes);

                return new DateTime(date.Year, date.Month, date.Day, hours, minutes, 0);

                #endregion
            }
            else
            {
                #region Do not handle Previous/Next day

                int days = 1;
                int hours = timeInMinutes / 60;
                if (hours > 23)
                {
                    days += hours / 24;
                    hours = hours - ((days - 1) * 24);
                }
                if (hours < 0)
                    hours = 0;

                //Convert to min, (minutes - days * minutesPerDay - hour * minutesPerHour)
                int minutes = timeInMinutes - ((days - 1) * 1440) - (hours * 60);
                if (minutes < 0)
                    minutes = 0;

                AdjustToValidTime(ref hours, ref minutes);

                return new DateTime(DATETIME_DEFAULT.Year, DATETIME_DEFAULT.Month, days, hours, minutes, 0); //Date is static and we only look at the time part when using this datetime

                #endregion
            }
        }

        public static DateTime GetDateFromMinutes(DateTime dateTime, int minutes)
        {
            DateTime date = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day);
            date = date.AddMinutes(minutes);
            return date;
        }

        public static DateTime ClearSeconds(DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
        }

        public static TimeSpan GetNewTimeInCurrentAndRule(DateTime scheduleStart, DateTime scheduleStop, DateTime ruleStart, DateTime ruleStop, DateTime currentStart, DateTime currentStop)
        {
            DateTime startDate = GetLatestDate(scheduleStart, ruleStart);
            DateTime stopDate = GetEarliestDate(scheduleStop, ruleStop);
            return GetNewTimeInCurrent(startDate, stopDate, currentStart, currentStop);
        }

        public static TimeSpan GetNewTimeInCurrent(DateTime scheduleStart, DateTime scheduleStop, DateTime currentStart, DateTime currentStop)
        {
            TimeSpan time = new TimeSpan();

            //Calculation not needed if current time is outside range scheduled time
            if (!IsDatesOverlapping(scheduleStart, scheduleStop, currentStart, currentStop))
                return time;

            if (IsNewOverlappedByCurrent(scheduleStart, scheduleStop, currentStart, currentStop))
            {
                // Schedule is completely overlapped by current
                time = time.Add((scheduleStop - scheduleStart).Duration());
            }
            else if (IsCurrentOverlappedByNew(scheduleStart, scheduleStop, currentStart, currentStop))
            {
                // Current is completely overlapped schedule
                time = time.Add((currentStop - currentStart).Duration());
            }
            else if (IsNewOverlappingCurrentStart(scheduleStart, scheduleStop, currentStart, currentStop))
            {
                // Schedule end intersects with current
                time = time.Add((scheduleStop - currentStart).Duration());
            }
            else if (IsNewOverlappingCurrentStop(scheduleStart, scheduleStop, currentStart, currentStop))
            {
                // Schedule time start intersects with current
                time = time.Add((currentStop - scheduleStart).Duration());
            }

            return time;
        }

        public static int GetLength(DateTime? start, DateTime? stop)
        {
            int length = 0;
            if (start.HasValue && stop.HasValue)
                length = Convert.ToInt32((stop.Value - start.Value).TotalMinutes);
            return length;
        }

        public static int GetTotalDays(DateTime? from, DateTime? to)
        {
            if (!from.HasValue || !to.HasValue || from > to)
                return 0;
            return (int)to.Value.Subtract(from.Value).TotalDays;
        }

        public static int GetOverlappingMinutes(DateTime newStart, DateTime newStop, DateTime rangeStart, DateTime rangeStop)
        {
            DateTime start = GetLatestDate(newStart, rangeStart);
            DateTime stop = GetEarliestDate(newStop, rangeStop);
            if (start > stop)
                return 0;

            int minutes = (int)stop.Subtract(start).TotalMinutes;
            return minutes;
        }

        public static int AdjustAccordingToInterval(int minutes, int interval, bool alwaysReduce = false)
        {
            if (interval == 1 || interval == 0)
                return minutes;

            var rest = minutes % interval;

            if (rest > 7 && !alwaysReduce)
                minutes = minutes + (interval - rest);
            else
                minutes = minutes - rest;

            return minutes;
        }

        public static DateTime GetDateAfterCoherentRangeOrFirstGap(List<DateTime> dates)
        {
            bool isCoherentPeriod = IsCoherentDateRange(dates, out DateTime firstGapDate);
            return isCoherentPeriod ? dates.Last().AddDays(1) : firstGapDate;
        }

        public static bool IsCoherentDateRange(List<DateTime> dates, out DateTime firstGapDate)
        {
            firstGapDate = DATETIME_DEFAULT;
            if (!dates.IsNullOrEmpty())
            {
                DateTime stopDate = dates.Last();
                DateTime currentDate = dates.First();
                while (currentDate <= stopDate)
                {
                    if (!dates.Contains(currentDate))
                    {
                        firstGapDate = currentDate;
                        return false;
                    }
                    currentDate = currentDate.AddDays(1);
                }
            }
            return true;
        }

        public static bool AccordingToInterval(int interval, int minutes)
        {
            return (minutes % interval) == 0;
        }

        public static bool IsBeforeNow(DateTime date, DateTime time)
        {
            if (date > DateTime.Today)
                return false;
            else if (date < DateTime.Today)
                return true;
            else
                return GetDateTime(date, time) < DateTime.Now;
        }

        public static bool IsDateInRange(DateTime date, DateTime? dateFrom, DateTime? dateTo)
        {
            if (!dateFrom.HasValue)
                dateFrom = DateTime.MinValue;
            if (!dateTo.HasValue)
                dateTo = DateTime.MaxValue;

            return (dateFrom.Value <= date && dateTo.Value >= date);
        }

        public static bool IsEndTimeInRange(DateTime endTime, DateTime? dateFrom, DateTime? dateTo)
        {
            if (!dateFrom.HasValue)
                dateFrom = DateTime.MinValue;
            if (!dateTo.HasValue)
                dateTo = DateTime.MaxValue;

            return (dateFrom.Value < endTime && dateTo.Value >= endTime);
        }

        public static bool IsDatesOverlappingNullable(DateTime? newStart, DateTime? newStop, List<Tuple<DateTime?, DateTime?>> dateRanges, bool validateDatesAreTouching = false)
        {
            if (dateRanges != null)
            {
                foreach (var dateRange in dateRanges)
                {
                    if (IsDatesOverlappingNullable(newStart, newStop, dateRange.Item1, dateRange.Item2, validateDatesAreTouching))
                        return true;
                }
            }

            return false;
        }

        public static bool IsDatesOverlappingNullable(DateTime? newStart, DateTime? newStop, DateTime? currentStart, DateTime? currentStop, bool validateDatesAreTouching = false)
        {
            newStart = newStart ?? DateTime.MinValue;
            newStop = newStop ?? DateTime.MaxValue;
            currentStart = currentStart ?? DateTime.MinValue;
            currentStop = currentStop ?? DateTime.MaxValue;

            return IsDatesOverlapping(newStart.Value, newStop.Value, currentStart.Value, currentStop.Value, validateDatesAreTouching);
        }

        public static bool IsTimesOverlappingNew(DateTime newStart, DateTime newStop, DateTime currentStart, DateTime currentStop)
        {
            return newStart < currentStop && currentStart < newStop;
        }

        public static bool IsDatesOverlapping(TimeSpan newStart, TimeSpan newStop, TimeSpan currentStart, TimeSpan currentStop, bool validateDatesAreTouching = false)
        {
            return IsDatesOverlapping(GetDateTime(newStart), GetDateTime(newStop), GetDateTime(currentStart), GetDateTime(currentStop), validateDatesAreTouching);
        }

        public static bool IsDatesOverlapping(DateTime newStart, DateTime newStop, DateTime currentStart, DateTime currentStop, bool validateDatesAreTouching = false)
        {
            bool overlapping = IsNewOverlappedByCurrent(newStart, newStop, currentStart, currentStop);
            if (!overlapping)
                overlapping = IsCurrentOverlappedByNew(newStart, newStop, currentStart, currentStop);
            if (!overlapping)
                overlapping = IsNewStopInCurrent(newStart, newStop, currentStart, currentStop);
            if (!overlapping)
                overlapping = IsNewStartInCurrent(newStart, newStop, currentStart, currentStop);
            if (!overlapping && validateDatesAreTouching)
                overlapping = IsDateRangesTouching(newStart, newStop, currentStart, currentStop);
            return overlapping;
        }

        public static bool GetOverlappingDates(DateTime newStart, DateTime newStop, DateTime? rangeStart, DateTime? rangeStop, out DateTime start, out DateTime stop)
        {
            start = GetLatestDate(newStart, rangeStart ?? DateTime.MinValue);
            stop = GetEarliestDate(newStop, rangeStop ?? DateTime.MaxValue);
            if (start > stop)
                return false;
            else
                return true;
        }

        public static bool HasOverlappingDays(DateTime newStart, DateTime newStop, DateTime rangeStart, DateTime rangeStop)
        {
            return GetOverlappingDates(newStart, newStop, rangeStart, rangeStop, out _, out _);
        }

        public static bool IsDatesInIntervalNullable(DateTime? newStart, DateTime? newStop, DateTime? currentStart, DateTime? currentStop)
        {
            newStart = newStart ?? DateTime.MinValue;
            newStop = newStop ?? DateTime.MaxValue;
            currentStart = currentStart ?? DateTime.MinValue;
            currentStop = currentStop ?? DateTime.MaxValue;

            return IsDatesInInterval(newStart.Value, newStop.Value, currentStart.Value, currentStop.Value);
        }

        public static bool IsDatesInInterval(DateTime newStart, DateTime newStop, DateTime currentStart, DateTime currentStop)
        {
            bool valid = true;

            if (!IsDatesValid(newStart, newStop))
                valid = false;
            else if (newStart < currentStart)
                valid = false;
            else if (newStop > currentStop)
                valid = false;

            return valid;
        }

        public static bool IsDatesValid(DateTime? start, DateTime? stop)
        {
            start = start ?? DateTime.MinValue;
            stop = stop ?? DateTime.MaxValue;

            return start <= stop;
        }

        public static bool IsDatesEqual(DateTime? date1, DateTime? date2)
        {
            if (!date1.HasValue && !date2.HasValue)
                return true;
            if (date1.HasValue && date2.HasValue && date1.Value.Trim(TimeSpan.TicksPerSecond).Equals(date2.Value.Trim(TimeSpan.TicksPerSecond)))
                return true;
            return false;
        }

        public static bool IsNewDateBeforeOldDate(DateTime? newDate, DateTime? oldDate)
        {
            //Case 1: Changed from null to date
            if (newDate.HasValue && !oldDate.HasValue)
                return true;
            //Case 2: Changed from date to lesser date
            if (newDate.HasValue && newDate.Value < oldDate.Value)
                return true;
            return false;
        }

        public static bool IsNewDateAfterOldDate(DateTime? newDate, DateTime? oldDate)
        {
            //Case 1: Changed from date to null
            if (!newDate.HasValue && oldDate.HasValue)
                return true;
            //Case 2: Changed from date to greater date
            if (newDate.HasValue && oldDate.HasValue && newDate.Value > oldDate.Value)
                return true;
            return false;
        }

        public static bool IsPreviousDay(DateTime date1, DateTime date2)
        {
            return date1 == date2.AddDays(-1);
        }

        public static bool IsNextDay(DateTime date1, DateTime date2)
        {
            return date2 == date2.AddDays(1);
        }

        /// <summary>
        /// Example:  
        ///         current: |       |
        ///         new    : |       |
        ///         new    :  |    |
        /// </summary>        
        public static bool IsNewOverlappedByCurrent(DateTime newStart, DateTime newStop, DateTime currentStart, DateTime currentStop)
        {
            return currentStart <= newStart && currentStop >= newStop;
        }

        /// <summary>
        /// Example:  
        ///         current: |       |        
        ///         new    :  |    |
        /// </summary>        
        public static bool IsNewInsideCurrent(DateTime newStart, DateTime newStop, DateTime currentStart, DateTime currentStop)
        {
            return currentStart < newStart && currentStop > newStop;
        }

        /// <summary>
        /// Example:  
        ///         current:|       |
        ///         new     |       |
        /// </summary>        
        public static bool IsNewSameAsCurrent(DateTime newStart, DateTime newStop, DateTime currentStart, DateTime currentStop)
        {
            return currentStart == newStart && currentStop == newStop;
        }

        /// <summary>
        /// Example:  
        ///         current:  |       |
        ///         new    :  |       |
        ///         new    :|           |
        /// </summary>        
        public static bool IsCurrentOverlappedByNew(DateTime newStart, DateTime newStop, DateTime currentStart, DateTime currentStop)
        {
            return currentStart >= newStart && currentStop <= newStop;
        }

        /// <summary>
        /// Example:  
        ///         current:  |       |        
        ///         new    :|           |
        /// </summary>        
        public static bool IsCurrentInsideNew(DateTime newStart, DateTime newStop, DateTime currentStart, DateTime currentStop)
        {
            return currentStart > newStart && currentStop < newStop;
        }

        /// <summary>
        /// Example:  
        ///         current:  |       |        
        ///         new    :  |    |
        ///         new    :|      |
        /// </summary>        
        public static bool IsNewStopInCurrent(DateTime newStart, DateTime newStop, DateTime currentStart, DateTime currentStop)
        {
            return currentStart >= newStart && currentStart < newStop && currentStop > newStop;
        }

        /// <summary>
        /// Example:  
        ///         current: |       |        
        ///         new    :     |   |
        ///         new    :     |       |
        /// </summary>        
        public static bool IsNewStartInCurrent(DateTime newStart, DateTime newStop, DateTime currentStart, DateTime currentStop)
        {
            return currentStart < newStart && currentStop > newStart && currentStop <= newStop;
        }

        public static bool IsDateRangesTouching(DateTime newStart, DateTime newStop, DateTime currentStart, DateTime currentStop)
        {
            return newStart == currentStop || newStop == currentStart;
        }

        public static bool IsNewBeforeCurrentStart(DateTime newStart, DateTime newStop, DateTime currentStart, DateTime currentStop)
        {
            return newStart < currentStart;
        }

        public static bool IsNewAfterCurrentStop(DateTime newStart, DateTime newStop, DateTime currentStart, DateTime currentStop)
        {
            return newStart >= currentStop;
        }

        public static bool IsNewOverlappingCurrentStart(DateTime newStart, DateTime newStop, DateTime currentStart, DateTime currentStop)
        {
            return newStart < currentStart && newStop > currentStart && newStop <= currentStop;
        }

        public static bool IsNewOverlappingCurrentStop(DateTime newStart, DateTime newStop, DateTime currentStart, DateTime currentStop)
        {
            return newStart >= currentStart && newStart < currentStop && newStop > currentStop;
        }

        public static bool IsAnyDateInRange(DateTime startDate, DateTime stopDate, params DateTime?[] dates)
        {
            foreach (var date in dates)
            {
                if (!date.HasValue)
                    continue;
                if (date >= startDate && date <= stopDate)
                    return true;
            }

            return false;
        }

        public static bool IsTimeZero(DateTime source)
        {
            return source.Hour == 0 && source.Minute == 0 && source.Second == 0;
        }

        public static string ToTime(DateTime date)
        {
            return date.ToString("HH:mm");
        }

        public static string ToSqlFriendlyDateTime(DateTime? date)
        {
            return ToUrlFriendlyDateTime(date).Replace(".", ":");
        }

        public static string ToUrlFriendlyDateTime(DateTime? date)
        {
            if (!date.HasValue)
                return String.Empty;
            return date.Value.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string ToShortDateTimeString(DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm");
        }

        public static string ToMonthAndDay(DateTime date)
        {
            return date.ToString("MMdd");
        }

        public static string ToFileFriendlyDateTime(DateTime date)
        {
            return date.ToString("yyyyMMddHHmmss" + date.Millisecond.ToString());
        }

        public static string ToDateTime(DateTime? date, string format)
        {
            if (!date.HasValue)
                return String.Empty;
            return date.Value.ToString(format);
        }

        public static string ToShortDateString(DateTime? date)
        {
            if (!date.HasValue)
                return String.Empty;
            return date.Value.ToShortDateString();
        }

        public static string GetSwedishDayExtension(int dayNumber)
        {
            if (dayNumber % 10 == 1 || dayNumber % 10 == 2)
                return ":a";
            else
                return ":e";
        }

        public static void GetTimeInMiddle(int length, int startTimeMinutes, int stopTimeMinutes, out DateTime startTime, out DateTime stopTime)
        {
            //Take the intersection of start and stop as the starttime
            int timeInMinutes = startTimeMinutes + stopTimeMinutes;
            if (timeInMinutes > 0)
                timeInMinutes /= 2;

            //Place break in the middle
            timeInMinutes -= (length / 2);
            startTime = GetDateFromMinutes(timeInMinutes);
            timeInMinutes += length;
            stopTime = GetDateFromMinutes(timeInMinutes);
        }

        public static void AdjustToValidTime(ref int hours, ref int minutes)
        {
            if (hours < 0)
                hours = 0;
            if (hours > 23)
                hours = 23;
            if (minutes < 0)
                minutes = 0;
            if (minutes > 59)
                minutes = 59;
        }

        #endregion

        #region Year

        public static List<GenericType> GetYears(int yearsBack)
        {
            int year = DateTime.Now.Year;

            List<GenericType> years = new List<GenericType>();
            if (yearsBack > 0)
            {
                for (int i = 1; i <= yearsBack; i++)
                {
                    int prevYear = DateTime.Now.AddYears(-i).Year;
                    years.Add(new GenericType()
                    {
                        Id = prevYear,
                        Name = prevYear.ToString(),
                    });
                }
            }

            years.Add(new GenericType()
            {
                Id = year,
                Name = year.ToString(),
            });

            return years;
        }

        /// <summary>
        /// Get the first day of the year the specified Date represents
        /// </summary>
        /// <param name="date">The Date</param>
        /// <returns>The first day for the specified Year</returns>
        public static DateTime GetFirstDateOfYear(DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;
            
            return new DateTime(date.Value.Year, 1, 1);
        }

        /// <summary>
        /// Get the last day of the year the specified Date represents
        /// </summary>
        /// <param name="date">The Date</param>
        /// <returns>The last day for the specified Year</returns>
        public static DateTime GetLastDateOfYear(DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            return new DateTime(date.Value.Year, 12, 31);
        }

        public static int GetNoOfDaysForAYear(int year)
        {
            int days = 0;
            for (int i = 1; i <= 12; i++)
            {
                days += DateTime.DaysInMonth(year, i);
            }
            return days;
        }

        public static int GetYearsBetweenDates(DateTime from, DateTime to)
        {
            int years = to.Year - from.Year;
            if (to.Month < from.Month || (to.Month == from.Month && to.Day < from.Day))
                years--;

            return years;
        }

        #endregion

        #region Quarter

        public static int GetQuarterNr(DateTime date)
        {
            return date.Month / 3 + 1;
        }

        public static bool IsFirstDayOfQuarter(DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            return (date.Value.Month == 1 || date.Value.Month == 4 || date.Value.Month == 7 || date.Value.Month == 10) && IsFirstDayOfMonth(date.Value);
        }

        #endregion

        #region Month

        /// <summary>
        /// Get a Month and Year string for the given date.
        /// </summary>
        /// <param name="date">The Date</param>
        /// <returns>Month and Year string. Ex: januari 2009</returns>
        public static string GetMonthInfo(DateTime date)
        {
            string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(date.Month);
            if (!String.IsNullOrEmpty(monthName))
                monthName += " " + date.Year;
            return monthName;
        }

        /// <summary>
        /// Get the first day of the month the given Date represents
        /// </summary>
        /// <param name="date">The Date</param>
        /// <returns>The DateTime for the last day of the month</returns>
        public static DateTime GetFirstDateOfMonth(DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            return new DateTime(date.Value.Year, date.Value.Month, 1, 0, 0, 0);
        }

        public static DateTime GetFirstDateOfNextMonth(DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            return new DateTime(date.Value.Year, date.Value.Month, 1, 0, 0, 0).AddMonths(1).Date;
        }

        /// <summary>
        /// Get the last day of the month the given Date represents
        /// </summary>
        /// <param name="date">The Date</param>
        /// <returns>The DateTime for the last day of the month</returns>
        public static DateTime GetLastDateOfMonth(DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            return new DateTime(date.Value.Year, date.Value.Month, DateTime.DaysInMonth(date.Value.Year, date.Value.Month), 0, 0, 0);
        }

        /// <summary>
        /// Check if specified date is first date of month
        /// </summary>
        /// <param name="date">Date to check, or null for today</param>
        /// <returns>True if first day of month else false</returns>
        public static bool IsFirstDayOfMonth(DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            return date.Value.Day == 1;
        }

        public static List<Tuple<DateTime, DateTime>> GetMonths(DateTime dateFrom, DateTime dateTo)
        {
            List<Tuple<DateTime, DateTime>> months = new List<Tuple<DateTime, DateTime>>();

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                DateTime firstDayInMonth = GetFirstDateOfMonth(date);
                DateTime lastDayInMonth = GetEndOfDay(GetLastDateOfMonth(date));

                if (months.Count == 0)
                    months.Add(Tuple.Create<DateTime, DateTime>(firstDayInMonth, lastDayInMonth));
                else if (!months.Any(x => x.Item1 == firstDayInMonth && x.Item2 == lastDayInMonth))
                    months.Add(Tuple.Create<DateTime, DateTime>(firstDayInMonth, lastDayInMonth));

                date = date.AddDays(1);
            }

            return months;
        }

        #endregion

        #region Week

        public static List<(DateTime WeekStart, DateTime WeekStop)> GetWeeks(DateTime dateFrom, DateTime dateTo)
        {
            List<(DateTime WeekStart, DateTime WeekStop)> weeks = new List<(DateTime weekStart, DateTime weekStop)>();

            DateTime date = dateFrom;
            while (date <= dateTo)
            {
                DateTime dateWeekFirst = GetFirstDateOfWeek(date);
                DateTime dateWeekLast = GetEndOfDay(GetLastDateOfWeek(date));
                if (!weeks.Any(x => x.WeekStart == dateWeekFirst && x.WeekStop == dateWeekLast))
                    weeks.Add((dateWeekFirst, dateWeekLast));

                date = date.AddDays(1);
            }

            return weeks;
        }

        public static List<(DateTime WeekStart, DateTime WeekStop)> GetSchoolWeeks(DateTime dateFrom, DateTime dateTo, int minorsSchoolDayStartMinutes, int minorsSchoolDayStopMinutes, List<Tuple<DateTime, DateTime>> schoolHolidayIntervals)
        {
            var weeks = GetWeeks(dateFrom, dateTo);
            foreach (var week in weeks)
            {
                DateTime? dateWeekFirst = null;
                DateTime? dateWeekLast = null;

                DateTime date = week.WeekStop;
                while (date >= week.WeekStart)
                {
                    switch (date.Date.DayOfWeek)
                    {
                        case DayOfWeek.Monday:
                        case DayOfWeek.Tuesday:
                        case DayOfWeek.Wednesday:
                        case DayOfWeek.Thursday:
                            //Valid start if day is schoolholiday, adjust to schoolday stop
                            Tuple<DateTime, DateTime> schoolHolidayInterval = schoolHolidayIntervals.FirstOrDefault(i => i.Item1 <= date && i.Item2 >= date);
                            if (schoolHolidayInterval != null)
                                dateWeekFirst = date.Date.AddMinutes(minorsSchoolDayStopMinutes);
                            break;
                        case DayOfWeek.Friday:
                            //Valid start, adjust to schoolday stop
                            dateWeekFirst = date.Date.AddMinutes(minorsSchoolDayStopMinutes);
                            break;
                        case DayOfWeek.Saturday:
                            break;
                        case DayOfWeek.Sunday:
                            //Valid stop, adjust to monday morning 0800
                            dateWeekLast = date.Date.AddDays(1).AddMinutes(minorsSchoolDayStartMinutes);
                            break;
                    }

                    date = date.AddDays(-1);
                }

                //Remove original week (Remove() doesnt seem to work)
                weeks = weeks.Where(i => i.WeekStart != week.WeekStart && i.WeekStop != week.WeekStop).ToList();

                //Add new week if valid
                if (dateWeekFirst.HasValue && dateWeekLast.HasValue)
                    weeks.Add((dateWeekFirst.Value, dateWeekLast.Value));
            }

            return weeks;
        }

        public static DateTime GetFirstDateOfWeek(int year, int week)
        {
            DateTime firstDateOfYear = new DateTime(year, 1, 1);

            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            int firstDateOfYearWeekNr = CultureInfo.CurrentCulture.DateTimeFormat.Calendar.GetWeekOfYear(firstDateOfYear, dfi.CalendarWeekRule, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
            int days = 0;
            if (firstDateOfYearWeekNr == 53)
                days = week * 7;
            else
                days = (week - 1) * 7;

            int daysOffset = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - (int)firstDateOfYear.DayOfWeek;

            return firstDateOfYear.AddDays(days + daysOffset);
        }

        public static DateTime GetFirstDateOfWeek(DateTime? date = null, CultureInfo culture = null, DayOfWeek? offset = null)
        {
            return GetFirstDateOfWeek(date.HasValue ? date.Value : DateTime.Today, culture, offset);
        }

        public static DateTime GetFirstDateOfWeek(DateTime date, CultureInfo culture = null, DayOfWeek? offset = null)
        {
            if (culture == null)
                culture = CultureInfo.CurrentCulture;

            DayOfWeek firstDay = offset ?? culture.DateTimeFormat.FirstDayOfWeek;
            while (date.DayOfWeek != firstDay)
            {
                date = date.AddDays(-1);
            }

            return date.Date;
        }

        public static DateTime GetPrevDayOfWeek(DayOfWeek day)
        {
            DateTime date = DateTime.Today;
            while (date.DayOfWeek != day)
            {
                date = date.AddDays(-1);
            }

            return date;
        }

        public static DateTime GetNextDayOfWeek(DayOfWeek day)
        {
            DateTime date = DateTime.Today;
            while (date.DayOfWeek != day)
            {
                date = date.AddDays(1);
            }

            return date;
        }

        public static DateTime GetLastDateOfWeek(int year, int week)
        {
            DateTime firstDateOfWeek = GetFirstDateOfWeek(year, week);
            return firstDateOfWeek.AddDays(7);
        }

        /// <summary>
        /// Get the last day of the week the given Date represents
        /// </summary>
        /// <param name="date">The Date</param>
        /// <param name="getEndOfDay">Sets time to end of day for the last day of week</param>
        /// <param name="culture">Sets which CultureInfo to use, used to determine first day of week</param>
        /// <returns>The DateTime for the last day of the week</returns>
        public static DateTime GetLastDateOfWeek(DateTime? date = null, bool getEndOfDay = false, CultureInfo culture = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            // Get first day of week and increase it by one
            CultureInfo info = culture ?? CultureInfo.CurrentCulture;
            int dayNumber = (int)info.DateTimeFormat.FirstDayOfWeek - 1;
            if (dayNumber < (int)DayOfWeek.Sunday)
                dayNumber = (int)DayOfWeek.Saturday;

            DayOfWeek lastDay = (DayOfWeek)dayNumber;
            while (date.Value.DayOfWeek != lastDay)
            {
                date = date.Value.AddDays(1);
            }

            return getEndOfDay ? GetEndOfDay(date) : date.Value;
        }

        /// <summary>
        /// Get the first week of the month the given Date represents
        /// </summary>
        /// <param name="date">The Date</param>
        /// <returns>The DateTime for the last day of the month</returns>
        public static DateTime GetFirstWeekOfMonth(DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            return GetFirstDateOfWeek(new DateTime(date.Value.Year, date.Value.Month, 1));
        }

        public static int GetWeekNr(DateTime date)
        {
            return GetWeekNr(date, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Get Number of weeks in year from a given date
        /// </summary>
        /// <param name="date"></param>
        /// <param name="culture"></param>
        /// <returns>number of weeks</returns>
        public static decimal GetNumberOfWeeksToDate(DateTime date)
        {
            decimal dayOfYear = (decimal)date.DayOfYear;
            return decimal.Round(dayOfYear / 7, 2);
        }

        /// <summary>
        /// Get Number of weeks remaining in year from a given date
        /// </summary>
        /// <param name="date"></param>
        /// <returns>number of weeks</returns>
        public static decimal GetNumberOfWeeksRemainingInYear(DateTime date)
        {
            int daysInYear = DateTime.IsLeapYear(date.Year) ? 366 : 365;
            decimal daysRemainingInYear = daysInYear - date.DayOfYear;

            return decimal.Round(daysRemainingInYear / 7, 2);
        }


        public static int GetWeekNr(DateTime date, CultureInfo culture)
        {
            int weekNr = 0;

            if (culture != null)
            {
                // Workaround for error in GetWeekOfYear function.
                // If its Monday, Tuesday or Wednesday, then it'll be the same week number as whatever Thursday, Friday or Saturday are,
                // and we always get those right.
                if (culture.Calendar.GetDayOfWeek(date) == DayOfWeek.Monday || culture.Calendar.GetDayOfWeek(date) == DayOfWeek.Tuesday || culture.Calendar.GetDayOfWeek(date) == DayOfWeek.Wednesday)
                    date = date.AddDays(3);

                weekNr = culture.Calendar.GetWeekOfYear(date, culture.DateTimeFormat.CalendarWeekRule, culture.DateTimeFormat.FirstDayOfWeek);
            }

            return weekNr;
        }

        public static List<string> GetWeekNrs(DateTime startDate, DateTime stopDate, CultureInfo culture)
        {
            List<string> weekNrs = new List<string>();
            List<string> weeks = new List<string>();

            if (stopDate < startDate)
                return weekNrs;

#if DEBUG
            culture = new CultureInfo(Constants.SYSLANGUAGE_LANGCODE_SWEDISH);
#endif

            string weekNrStop = $"{stopDate.Year}_{GetWeekNr(stopDate, culture)}_{GetFirstDateOfWeek(stopDate, culture: culture)}";

            #region 

            DateTime currentDate = GetFirstDateOfWeek(startDate, culture: culture);
            while (currentDate <= stopDate)
            {
                int weekNr = GetWeekNr(currentDate, culture);
                if (!weeks.Contains($"{currentDate.Year}_{weekNr}_{GetFirstDateOfWeek(currentDate, culture: culture)}"))
                    weeks.Add($"{currentDate.Year}_{weekNr}_{GetFirstDateOfWeek(currentDate, culture: culture)}");

                currentDate = currentDate.AddDays(7);
            }

            //Handle if stopDate is middle of week
            if (!weeks.Contains(weekNrStop))
                weeks.Add(weekNrStop);

            #endregion


            return weeks;
        }

        public static int GetWeekNrInMonth(DateTime date)
        {
            return GetWeekNrInMonth(date, CultureInfo.CurrentCulture);
        }

        public static int GetWeekNrInMonth(DateTime date, CultureInfo culture)
        {
            return GetWeekNr(date, culture) - GetWeekNr(new DateTime(date.Year, date.Month, 1), culture) + 1;
        }

        public static int GetNoOfWeeksInYear(int year)
        {        
            // ISO 8601: Week starts on Monday, first week has at least 4 days
            var calendar = CultureInfo.InvariantCulture.Calendar;
            var date = new DateTime(year, 12, 31);
            var weekRule = CalendarWeekRule.FirstFourDayWeek;
            var firstDayOfWeek = DayOfWeek.Monday;

            return calendar.GetWeekOfYear(date, weekRule, firstDayOfWeek);
        }

        public static int GetWeekNr(int dayNumber)
        {
            if (dayNumber < 1)
                dayNumber = 1;

            return ((int)Decimal.Divide(dayNumber - 1, 7) + 1);
        }

        /// <summary>
        /// Adjust the date to the beginning of the week
        /// </summary>
        /// <param name="date">The date to adjust</param>
        /// <returns>A new date, which is a Monday</returns>
        public static DateTime AdjustDateToBeginningOfWeek(DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Tuesday:
                    return date.AddDays(-1);
                case DayOfWeek.Wednesday:
                    return date.AddDays(-2);
                case DayOfWeek.Thursday:
                    return date.AddDays(-3);
                case DayOfWeek.Friday:
                    return date.AddDays(-4);
                case DayOfWeek.Saturday:
                    return date.AddDays(-5);
                case DayOfWeek.Sunday:
                    return date.AddDays(-6);
                default:
                    return date;
            }
        }

        /// <summary>
        /// Adjust the date to the end of the week
        /// </summary>
        /// <param name="date">The date to adjust</param>
        /// <returns>A new date, which is a Sunday</returns>
        public static DateTime AdjustDateToEndOfWeek(DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return date.AddDays(6);

                case DayOfWeek.Tuesday:
                    return date.AddDays(5);

                case DayOfWeek.Wednesday:
                    return date.AddDays(4);

                case DayOfWeek.Thursday:
                    return date.AddDays(3);

                case DayOfWeek.Friday:
                    return date.AddDays(2);

                case DayOfWeek.Saturday:
                    return date.AddDays(1);

                default:
                    return date;
            }
        }

        #endregion

        #region Day

        /// <summary>
        /// Get the day name of specified date
        /// </summary>
        /// <param name="date">The Date</param>
        /// <param name="camelCase">If true, first letter will be in upper case</param>
        /// <returns>The day name</returns>
        public static string GetDayNameFromCulture(DateTime date, bool camelCase = false)
        {
            return GetDayName(date, CultureInfo.CurrentCulture, camelCase);
        }

        /// <summary>
        /// Get the day name of specified date
        /// </summary>
        /// <param name="date">The Date</param>
        /// <param name="culture">The Culture</param>
        /// <param name="camelCase">If true, first letter will be in upper case</param>
        /// <param name="abbreviated">If true, the abbreviated (short) name for the day is returned</param>
        /// <returns>The day name</returns>
        public static string GetDayName(DateTime date, CultureInfo culture, bool camelCase = false, bool abbreviated = false)
        {
            return GetDayName(date.DayOfWeek, culture, camelCase, abbreviated);
        }

        /// <summary>
        /// Get the day name of specified day of week
        /// </summary>
        /// <param name="dayOfWeek"></param>
        /// <param name="culture"></param>
        /// <param name="camelCase"></param>
        /// <param name="abbreviated"></param>
        /// <returns></returns>
        public static string GetDayName(DayOfWeek dayOfWeek, CultureInfo culture, bool camelCase = false, bool abbreviated = false)
        {
            string dayName = abbreviated ? culture.DateTimeFormat.GetAbbreviatedDayName(dayOfWeek) : culture.DateTimeFormat.GetDayName(dayOfWeek);
            if (camelCase)
                dayName = StringUtility.CamelCaseWord(dayName);

            return dayName;
        }

        /// <summary>
        /// Get first letter of the day name of specified date
        /// </summary>
        /// <param name="date">The Date</param>
        /// <returns>The day name</returns>
        public static string GetShortDayName(DateTime date)
        {
            string name = GetDayName(date, CultureInfo.CurrentCulture);
            return name.Substring(0, 1);
        }

        /// <summary>
        /// Get a given Dates name
        /// </summary>
        /// <param name="date">The Date</param>
        /// <returns>The day name</returns>
        public static string GetDayOfMonth(DateTime date)
        {
            return (date.Day <= 9 ? "0" + date.Day.ToString() : date.Day.ToString());
        }

        public static int GetDayNr(DateTime date)
        {
            int dayNumber = (int)date.DayOfWeek;
            if (dayNumber == 0)
                dayNumber = 7;

            return dayNumber;
        }

        public static int GetDayNrFromCulture(DateTime date)
        {
            return (int)GetDayOfWeek(date);
        }

        public static DayOfWeek GetDayOfWeek(DateTime date)
        {
            Calendar calendar = CultureInfo.CurrentCulture.Calendar;
            return calendar.GetDayOfWeek(date);
        }

        public static Dictionary<int, string> GetDayOfWeekNames(bool camelCase = true)
        {
            Dictionary<int, string> dict = new Dictionary<int, string>();

            // Start on monday
            for (int i = 1; i < 7; i++)
            {
                dict.Add(i, GetDayOfWeekName((DayOfWeek)i, camelCase));
            }
            dict.Add(0, GetDayOfWeekName((DayOfWeek)0, camelCase));

            return dict;
        }

        public static string GetDayOfWeekName(DayOfWeek dayOfWeek, bool camelCase = true)
        {
            string name = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)dayOfWeek];
            if (camelCase)
                name = StringUtility.CamelCaseWord(name);

            return name;
        }

        public static DateTime GetDateFromDayOfWeek(DateTime weekstart, DayOfWeek dayOfWeek)
        {
            DateTime currentDate = weekstart;
            while (currentDate < weekstart.AddDays(8))
            {
                if (GetDayOfWeek(currentDate) == dayOfWeek)
                    return currentDate;

                currentDate = currentDate.AddDays(1);
            }

            return DATETIME_DEFAULT;
        }

        /// <summary>
        /// Get day number from date where Monday = 1 and Sunday = 7
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>Day number</returns>
        public static int GetDayNumberStartOnMonday(DateTime date)
        {
            int dayNumber = GetDayNrFromCulture(date);
            if (dayNumber == 0) // Sunday
                dayNumber = 7;

            return dayNumber;
        }

        /// <summary>
        /// Get day number from day of week where Monday = 1 and Sunday = 7
        /// </summary>
        /// <param name="dayOfWeek">Day of week</param>
        /// <returns>Day number</returns>
        public static int GetDayNumberStartOnMonday(DayOfWeek dayOfWeek)
        {
            int dayNumber = (int)dayOfWeek;
            if (dayNumber == 0) // Sunday
                dayNumber = 7;

            return dayNumber;
        }

        public static int GetScheduleDayNumber(DateTime currentDate, DateTime scheduleStartDate, int scheduleStartDayNumber, int scheduleNoOfDays)
        {

            if (scheduleNoOfDays == 0)
                return 0;

            currentDate = currentDate.Date;
            scheduleStartDate = scheduleStartDate.Date;

            // Get number of days that has passed from schedule start to specified date
            int daysPassed = (currentDate - scheduleStartDate).Days;

            // Add schedule's start day (Employees schedule might not start with the templates first period)
            daysPassed += scheduleStartDayNumber;

            // If daysPassed is larger than total number of days in template, decrease it with one complete period length until it gets below.
            while (daysPassed > scheduleNoOfDays)
            {
                daysPassed -= scheduleNoOfDays;
            }

            return daysPassed;
        }

        public static List<Tuple<DateTime, DateTime>> GetWeekIntervalsFromDate(DateTime start, DateTime stop)
        {
            List<Tuple<DateTime, DateTime>> weekIntervals = new List<Tuple<DateTime, DateTime>>();

            var currentDate = start;
            var firstDayInWeek = currentDate;

            while (currentDate <= stop)
            {
                if (currentDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    weekIntervals.Add(Tuple.Create(firstDayInWeek, currentDate));
                    firstDayInWeek = currentDate.AddDays(1);
                }
                currentDate = currentDate.AddDays(1);
            }
            currentDate = currentDate.AddDays(-1);
            if (currentDate.DayOfWeek != DayOfWeek.Sunday)
                weekIntervals.Add(Tuple.Create(firstDayInWeek, currentDate));

            return weekIntervals;
        }

        public static ObservableCollection<GenericType<DayOfWeek, string>> GetWeekDays()
        {
            ObservableCollection<GenericType<DayOfWeek, string>> coll = new ObservableCollection<GenericType<DayOfWeek, string>>();

            coll.Add(new GenericType<DayOfWeek, string>() { Field1 = DayOfWeek.Monday, Field2 = GetDayName(DayOfWeek.Monday, CultureInfo.CurrentCulture, true) });
            coll.Add(new GenericType<DayOfWeek, string>() { Field1 = DayOfWeek.Tuesday, Field2 = GetDayName(DayOfWeek.Tuesday, CultureInfo.CurrentCulture, true) });
            coll.Add(new GenericType<DayOfWeek, string>() { Field1 = DayOfWeek.Wednesday, Field2 = GetDayName(DayOfWeek.Wednesday, CultureInfo.CurrentCulture, true) });
            coll.Add(new GenericType<DayOfWeek, string>() { Field1 = DayOfWeek.Thursday, Field2 = GetDayName(DayOfWeek.Thursday, CultureInfo.CurrentCulture, true) });
            coll.Add(new GenericType<DayOfWeek, string>() { Field1 = DayOfWeek.Friday, Field2 = GetDayName(DayOfWeek.Friday, CultureInfo.CurrentCulture, true) });
            coll.Add(new GenericType<DayOfWeek, string>() { Field1 = DayOfWeek.Saturday, Field2 = GetDayName(DayOfWeek.Saturday, CultureInfo.CurrentCulture, true) });
            coll.Add(new GenericType<DayOfWeek, string>() { Field1 = DayOfWeek.Sunday, Field2 = GetDayName(DayOfWeek.Sunday, CultureInfo.CurrentCulture, true) });

            return coll;
        }

        public static Dictionary<DayOfWeek, string> GetWeekDaysDict()
        {
            Dictionary<DayOfWeek, string> dict = new Dictionary<DayOfWeek, string>();

            dict.Add(DayOfWeek.Monday, GetDayName(DayOfWeek.Monday, CultureInfo.CurrentCulture, true));
            dict.Add(DayOfWeek.Tuesday, GetDayName(DayOfWeek.Tuesday, CultureInfo.CurrentCulture, true));
            dict.Add(DayOfWeek.Wednesday, GetDayName(DayOfWeek.Wednesday, CultureInfo.CurrentCulture, true));
            dict.Add(DayOfWeek.Thursday, GetDayName(DayOfWeek.Thursday, CultureInfo.CurrentCulture, true));
            dict.Add(DayOfWeek.Friday, GetDayName(DayOfWeek.Friday, CultureInfo.CurrentCulture, true));
            dict.Add(DayOfWeek.Saturday, GetDayName(DayOfWeek.Saturday, CultureInfo.CurrentCulture, true));
            dict.Add(DayOfWeek.Sunday, GetDayName(DayOfWeek.Sunday, CultureInfo.CurrentCulture, true));

            return dict;
        }

        public static List<DayOfWeek> GetWeekDaysList()
        {
            List<DayOfWeek> list = new List<DayOfWeek>();

            list.Add(DayOfWeek.Monday);
            list.Add(DayOfWeek.Tuesday);
            list.Add(DayOfWeek.Wednesday);
            list.Add(DayOfWeek.Thursday);
            list.Add(DayOfWeek.Friday);
            list.Add(DayOfWeek.Saturday);
            list.Add(DayOfWeek.Sunday);

            return list;
        }

        public static bool IsMonday(DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Today;

            return date.Value.DayOfWeek == DayOfWeek.Monday;
        }

        public static bool IsEqual(DayOfWeek? value1, DayOfWeek? value2)
        {
            return value1.HasValue && value1.HasValue == value2.HasValue && value1.Value == value2.Value;
        }

        #endregion

        #region Age

        public static bool IsAgeYoungerThan18(DateTime birthDate, DateTime? date = null)
        {

            DateTime refDate = date ?? DateTime.Today;
            return refDate < birthDate.AddYears(18);
        }

        public static bool IsAgeBetween16To18(DateTime birthDate, DateTime? date = null)
        {
            DateTime refDate = date ?? DateTime.Today;
            return refDate >= birthDate.AddYears(16) && refDate < birthDate.AddYears(18);
        }

        public static bool IsAgeBetween13To15(DateTime birthDate, DateTime? date = null)
        {
            DateTime refDate = date ?? DateTime.Today;
            return refDate >= birthDate.AddYears(13) && refDate < birthDate.AddYears(16);
        }

        public static bool IsAgeYoungerThan13(DateTime birthDate, DateTime? date = null)
        {
            DateTime refDate = date ?? DateTime.Today;
            return refDate < birthDate.AddYears(13);
        }


        public static int AgeYears(DateTime birthDate, DateTime date)
        {
            int age = date.Year - birthDate.Year;

            if (date.Month < birthDate.Month || (date.Month == birthDate.Month && date.Day < birthDate.Day))
                age--;

            return age;
        }

        public static int AgeMonths(DateTime birthDate, DateTime date)
        {
            int age = AgeYears(birthDate, date);
            DateTime lastBirthDay = birthDate.AddYears(age);
            int months = 0;
            while (lastBirthDay.AddMonths(months) < date)
                months++;

            return Convert.ToInt32((decimal.Multiply(12, age) + months));
        }

        #endregion

        #endregion

        #region Time

        #region TimeSpan

        public static TimeSpan GetTimeSpanFromDateTime(DateTime date)
        {
            return new TimeSpan(0, GetTotalMinutesFromDateTimeAsTime(date), 0);
        }

        public static TimeSpan GetTimeSpanFromMinutes(decimal minutes)
        {
            return TimeSpan.FromMinutes((double)Decimal.Floor(minutes));
        }

        public static TimeSpan GetTimeSpanFromMinutes(int minutes)
        {
            return new TimeSpan(0, minutes, 0);
        }

        public static string FormatTimeSpan(decimal minutes, string prefix = "")
        {
            return prefix + FormatTimeSpan(MinutesToTimeSpan(Convert.ToInt32(minutes)), false, false);
        }

        public static string FormatTimeSpan(TimeSpan time, bool showDays, bool showSeconds, bool clearZero = false, bool clearZeroHours = false)
        {
            string value = String.Empty;
            bool isNegative = (time < new TimeSpan());

            if (clearZero && ((showSeconds && time.TotalSeconds == 0) || (!showSeconds && time.TotalMinutes == 0)))
                return value;

            int hours = Math.Abs(time.Hours);
            int minutes = Math.Abs(time.Minutes);
            int seconds = Math.Abs(time.Seconds);
            int days = Math.Abs(time.Days);

            // Reset
            time = new TimeSpan(hours, minutes, seconds);

            // Add days
            if (days != 0)
            {
                if (showDays)
                    value += days + ".";
                else
                    hours += (Math.Abs(days) * 24);
            }

            // Add time
            value += String.Format("{0}:{1}", showDays && days > 0 ? hours.ToString().PadLeft(2, '0') : hours.ToString(), minutes.ToString().PadLeft(2, '0'));
            if (showSeconds)
                value += ":" + seconds.ToString().PadLeft(2, '0');

            if (isNegative)
                value = "-" + value;

            // Fix ex 08:00 --> 8:00
            if (clearZeroHours && !String.IsNullOrEmpty(value) && value.StartsWith("0") && value.IndexOf(':') == 2)
                value = value.SubstringToLengthOfString(1);

            return value;
        }

        public static TimeSpan TextToTimeSpan(string str, bool useSeconds = false)
        {
            bool isNegative = false;

            if (!String.IsNullOrEmpty(str))
            {
                str = str.Replace(',', ':');
                // Mac sometimes specifies time with a .
                str = str.Replace('.', ':');

                // Check if negative
                if (str.Substring(0, 1) == "-")
                {
                    isNegative = true;
                    str = str.Substring(1);
                }

                if (str.IndexOf(':') == -1)
                {
                    // No separator entered, check length to see if more than hour is specified
                    if (str.Length > 4 && useSeconds)
                        str = String.Format("{0}:{1}:{2}", str.Left(str.Length - 4), str.Substring(str.Length - 4, 2), str.Right(2));
                    else if (str.Length > 2)
                        str = String.Format("{0}:{1}", str.Left(str.Length - 2), str.Right(2));
                }
            }

            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            if (!String.IsNullOrEmpty(str))
            {
                string[] parts = str.Split(':');
                if (parts.Length > 0)
                    Int32.TryParse(parts[0], out hours);
                if (parts.Length > 1)
                    Int32.TryParse(parts[1], out minutes);
                if (parts.Length > 2)
                    Int32.TryParse(parts[2], out seconds);
            }

            TimeSpan span = new TimeSpan(hours, minutes, seconds);
            if (isNegative)
                span = -span;

            return span;
        }

        public static DateTime? GetTime(string time)
        {
            DateTime? dateTime = null;
            if (!String.IsNullOrEmpty(time) && DateTime.TryParse(time, out DateTime d))
                dateTime = new DateTime(DATETIME_DEFAULT.Year, DATETIME_DEFAULT.Month, DATETIME_DEFAULT.Day, d.Hour, d.Minute, 0);
            return dateTime;
        }

        public static string FormatTime(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return String.Empty;

            return dateTime.Value.ToString("HH:mm");
        }

        /// <summary>
        /// Format given hour and minutes to HH:MM format
        /// </summary>
        /// <param name="hour">Hour</param>
        /// <param name="minute">Minutes</param>
        /// <returns>HH:MM</returns>
        public static string FormatTime(int hour, int minute)
        {
            DateTime time = DATETIME_DEFAULT.Add(new TimeSpan(hour, minute, 0));

            bool isNegative = hour < 0 || minute < 0;
            string value = time.ToString("HH:mm");
            if (isNegative)
                value = "-" + value;

            return value;
        }

        #endregion

        #region Hour

        /// <summary>
        /// Validates given hour
        /// </summary>
        /// <param name="hour">The hour to validate</param>
        /// <returns>0 if hour not between 0-24</returns>
        public static int GetValidHour(int hour)
        {
            return (hour < 0 || hour > 24 ? 0 : hour);
        }

        public static int RoundMinutesToNextHalfHour(decimal minutes)
        {
            if (minutes == 0)
                return 0;

            TimeSpan time = MinutesToTimeSpan((int)minutes);
            if (time.Minutes > 0)
            {
                if (time.Minutes < 30)
                    time = new TimeSpan((int)time.TotalHours, 30, 0);
                else if (time.Minutes < 60)
                    time = new TimeSpan((int)time.TotalHours + 1, 0, 0);
            }
            return (int)time.TotalMinutes;
        }

        #endregion

        #region Minute

        public static int GetMinutes(string time)
        {
            int mpt;
            char[] splitter = { ':' };
            string strTime = time;

            if (!string.IsNullOrEmpty(time))
                strTime = strTime.Replace(".", ":").Replace(",", ":").Replace(";", ":");

            if (!String.IsNullOrEmpty(time))
            {
                if (strTime.StartsWith("-"))
                {
                    strTime = strTime.Substring(1, strTime.Length - 1);
                    mpt = -1;
                }
                else
                {
                    mpt = 1;
                }

                string[] timeParts = strTime.Split(splitter);
                if (timeParts.GetUpperBound(0) == 0)
                    return Convert.ToInt32(timeParts[0]) * 60 * mpt;
                else
                    return (Convert.ToInt32(timeParts[0]) * 60 + (Convert.ToInt32(timeParts[1]))) * mpt;
            }
            else
            {
                return 0;
            }
        }

        public static string FormatMinutes(int minutes)
        {
            int tempMin, tempTim;
            string chr, chrMin;

            if (minutes.ToString().Length > 0)
            {
                if (minutes < 0)
                {
                    chr = "-";
                    minutes = minutes * -1;
                }
                else
                {
                    chr = "";
                }

                tempMin = minutes % 60;
                tempTim = (minutes - tempMin) / 60;

                if (tempMin.ToString().Length < 2)
                    chrMin = "0" + tempMin.ToString();
                else
                    chrMin = tempMin.ToString();

                string times = (tempTim.ToString().Length < 2) ? "0" + tempTim.ToString() : tempTim.ToString();

                return chr + times + ":" + chrMin;
            }
            else
            {
                return minutes.ToString();
            }
        }

        public static int GetTotalMinutesFromDateTimeAsTime(DateTime time, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DATETIME_DEFAULT;
            return (int)(time - date.Value).TotalMinutes;
        }

        public static DateTime GetTimeWithDefaultDateTime(DateTime datetime, DateTime belongstoDate)
        {
            if (datetime > DATETIME_DEFAULT.AddDays(-1) && datetime < DATETIME_DEFAULT.AddDays(1))
            {
                return datetime;
            }
            else
            {
                DateTime date = DATETIME_DEFAULT.AddMinutes((int)(datetime - GetBeginningOfDay(datetime)).TotalMinutes);
                if (GetBeginningOfDay(datetime).AddDays(1) == GetBeginningOfDay(belongstoDate))
                    date = date.AddDays(1);
                if (GetBeginningOfDay(datetime).AddDays(-1) == GetBeginningOfDay(belongstoDate))
                    date = date.AddDays(-1);
                return date;
            }
        }

        public static int TimeSpanToMinutes(string time, bool useSeconds)
        {
            return (int)TextToTimeSpan(time, useSeconds).TotalMinutes;
        }

        /// <summary>
        /// Get difference between two dates in minutes
        /// </summary>
        /// <param name="stopDate">The stop date</param>
        /// <param name="startDate">The start date</param>
        /// <returns>Difference in minutes</returns>
        public static int TimeSpanToMinutes(DateTime stopDate, DateTime startDate)
        {
            TimeSpan difference = stopDate - startDate;
            return (int)difference.TotalMinutes;
        }

        /// <summary>
        /// Get difference between two dates in milliseconds
        /// </summary>
        /// <param name="stopDate">The stop date</param>
        /// <param name="startDate">The start date</param>
        /// <returns>Difference in milliseconds</returns>
        public static int TimeSpanToMilliSeconds(DateTime stopDate, DateTime startDate)
        {
            TimeSpan difference = stopDate - startDate;
            return (int)difference.TotalMilliseconds;
        }

        public static int TimeSpanToMinutesStartAndStopUnknown(DateTime date1, DateTime date2)
        {
            if (date1 < date2)
                return TimeSpanToMinutes(date2, date1);
            else
                return TimeSpanToMinutes(date1, date2);
        }

        /// <summary>
        /// Create a TimeSpan from a range of minutes
        /// </summary>
        /// <param name="startMinutes">Number of minutes from midnight, start</param>
        /// <param name="stopMinutes">Number of minutes from midnight, stop</param>
        /// <returns>TimeSpan</returns>
        public static TimeSpan MinutesToTimeSpan(int startMinutes, int stopMinutes)
        {
            return TimeSpan.FromMinutes(stopMinutes - startMinutes);
        }

        /// <summary>
        /// Create a TimeSpan from minutes
        /// </summary>
        /// <param name="minutes">Number of minutes</param>
        /// <returns>TimeSpan</returns>
        public static TimeSpan MinutesToTimeSpan(int minutes)
        {
            return TimeSpan.FromMinutes(minutes);
        }

        public static DateTime MinutesToDateTime(DateTime date, int minutes)
        {
            return date.AddMinutes(minutes);
        }

        /// <summary>
        /// Create a TimeSpan from seconds
        /// </summary>
        /// <param name="seconds">Number of seconds</param>
        /// <returns>TimeSpan</returns>
        public static TimeSpan SecondsToTimeSpan(int seconds)
        {
            return TimeSpan.FromSeconds(seconds);
        }

        /// <summary>
        /// Converts minutes to HH:MM string
        /// </summary>
        /// <param name="minutes">The minutes to convert</param>
        /// <param name="leadingHourZero">Add 0 to begining</param>
        /// <returns>HH:MM</returns>
        public static string GetHoursAndMinutesString(int minutes, bool leadingHourZero = true)
        {
            bool isNegative = false;
            if (minutes < 0)
            {
                isNegative = true;
                minutes = -minutes;
            }

            var ts = new TimeSpan(0, minutes, 0);
            var value = GetHoursAndMinutesString(ts, leadingHourZero);

            if (isNegative)
                value = "-" + value;

            return value;
        }

        public static string GetHoursAndMinutesString(TimeSpan source, bool leadingHourZero = true)
        {
            int hours = Convert.ToInt32(Math.Floor(source.TotalHours));
            int minutes = source.Minutes;

            return (hours > 9 ? hours.ToString() : (leadingHourZero ? "0" : "") + hours) + ":" + (minutes > 9 ? minutes.ToString() : "0" + minutes);
        }

        public static int? GetMinutesFromString(object obj)
        {
            if (obj == null)
                return null;

            string value = obj.ToString();
            if (String.IsNullOrEmpty(value) || value.Length > 5)
                return null;

            string[] parts = null;
            if (value.Contains(","))
                parts = value.Split(',');
            if (value.Contains("."))
                parts = value.Split('.');
            if (value.Contains(":"))
                parts = value.Split(':');

            if (parts == null || parts.Count() != 2)
                return null;

            Int32.TryParse(parts[0], out int hours);
            Int32.TryParse(parts[1], out int minutes);
            TimeSpan ts = new TimeSpan(hours, minutes, 0);
            return (int)ts.TotalMinutes;
        }

        public static string GetHoursAndMinutesString(DateTime source)
        {
            int hour = source.Hour;
            int minute = source.Minute;

            return (hour > 9 ? hour.ToString() : "0" + hour) + ":" + (minute > 9 ? minute.ToString() : "0" + minute);
        }

        /// <summary>
        /// Converts the time part of a date to number of minutes from DATETIME_DEFAULT (if not other date is passed as parameter)
        /// </summary>
        /// <param name="date">Date</param>
        /// <param name="compareDate">Date to compare against. DATETIME_DEFAULT will be used if not this is set</param>
        /// <returns>Number of elapsed minutes since DATETIME_DEFAULT</returns>
        public static int TimeToMinutes(DateTime date, DateTime? compareDate = null)
        {
            if (!compareDate.HasValue)
                compareDate = DATETIME_DEFAULT;
            return (int)(date - compareDate.Value).TotalMinutes;
        }

        public static int GetOneDayInMinutes()
        {
            return 24 * 60;
        }

        public static int GetTimeInMinutes(int timeCodeBreakTimeType, int minutes, int schemaIn, int schemaOut)
        {
            switch (timeCodeBreakTimeType)
            {
                case (int)SoeTimeCodeBreakTimeType.ScheduleIn:
                    minutes += schemaIn;
                    break;
                case (int)SoeTimeCodeBreakTimeType.ScheduleOut:
                    minutes += schemaOut;
                    break;
            }
            return minutes;
        }

        public static int GetTimeDiscrepence(DateTime dateTimeToCompare, DateTime dateTimeToCompareAgainst)
        {
            int result = (int)(dateTimeToCompare - dateTimeToCompareAgainst).TotalMinutes;
            if (result < 0)
            {
                //negate
                result *= -1;
            }
            return result;
        }

        /// <summary>
        /// Validates given minute
        /// </summary>
        /// <param name="minute">The minute to validate</param>
        /// <returns>0 if minute not between 0-60</returns>
        public static int GetValidMinute(int minute)
        {
            return (minute < 0 || minute > 60 ? 0 : minute);
        }

        #endregion

        #endregion

        #region TimeInterval

        public static DateRangeDTO GetTimeInterval(TermGroup_TimeIntervalPeriod period, TermGroup_TimeIntervalStart start, int startOffset, TermGroup_TimeIntervalStop stop, int stopOffset, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Now;

            DateTime startDate = date.Value;
            DateTime stopDate = date.Value;

            if (start == TermGroup_TimeIntervalStart.CurrentTime)
                startDate = date.Value;
            if (stop == TermGroup_TimeIntervalStop.CurrentTime)
                stopDate = date.Value;

            switch (period)
            {
                case TermGroup_TimeIntervalPeriod.Day:
                    #region Day

                    startDate = startDate.AddDays(startOffset);
                    if (start == TermGroup_TimeIntervalStart.BeginningOfPeriod)
                        startDate = GetBeginningOfDay(startDate);

                    stopDate = stopDate.AddDays(stopOffset);
                    if (stop == TermGroup_TimeIntervalStop.EndOfPeriod)
                        stopDate = GetEndOfDay(stopDate);

                    #endregion
                    break;
                case TermGroup_TimeIntervalPeriod.Week:
                    #region Week

                    startDate = startDate.AddDays(startOffset * 7);
                    if (start == TermGroup_TimeIntervalStart.BeginningOfPeriod)
                        startDate = GetBeginningOfWeek(startDate);

                    stopDate = stopDate.AddDays(stopOffset * 7);
                    if (stop == TermGroup_TimeIntervalStop.EndOfPeriod)
                        stopDate = GetEndOfWeek(stopDate);

                    #endregion
                    break;
                case TermGroup_TimeIntervalPeriod.Month:
                    #region Month

                    startDate = startDate.AddMonths(startOffset);
                    if (start == TermGroup_TimeIntervalStart.BeginningOfPeriod)
                        startDate = GetBeginningOfMonth(startDate);

                    stopDate = stopDate.AddMonths(stopOffset);
                    if (stop == TermGroup_TimeIntervalStop.EndOfPeriod)
                        stopDate = GetEndOfMonth(stopDate);

                    #endregion
                    break;
                case TermGroup_TimeIntervalPeriod.Quarter:
                    #region Quarter

                    startDate = startDate.AddMonths(startOffset * 3);
                    if (start == TermGroup_TimeIntervalStart.BeginningOfPeriod)
                        startDate = GetBeginningOfQuarter(startDate);

                    stopDate = stopDate.AddMonths(stopOffset * 3);
                    if (stop == TermGroup_TimeIntervalStop.EndOfPeriod)
                        stopDate = GetEndOfQuarter(stopDate);

                    #endregion
                    break;
                case TermGroup_TimeIntervalPeriod.HalfYear:
                    #region HalfYear

                    startDate = startDate.AddMonths(startOffset * 6);
                    if (start == TermGroup_TimeIntervalStart.BeginningOfPeriod)
                        startDate = GetBeginningOfHalfYear(startDate);

                    stopDate = stopDate.AddMonths(stopOffset * 6);
                    if (stop == TermGroup_TimeIntervalStop.EndOfPeriod)
                        stopDate = GetEndOfHalfYear(stopDate);

                    #endregion
                    break;
                case TermGroup_TimeIntervalPeriod.Year:
                    #region Year

                    startDate = startDate.AddYears(startOffset);
                    if (start == TermGroup_TimeIntervalStart.BeginningOfPeriod)
                        startDate = GetBeginningOfYear(startDate);

                    stopDate = stopDate.AddYears(stopOffset);
                    if (stop == TermGroup_TimeIntervalStop.EndOfPeriod)
                        stopDate = GetEndOfYear(stopDate);

                    #endregion
                    break;
            }

            return new DateRangeDTO(startDate, stopDate);
        }

        #endregion

        #region Holiday

        public static bool IsSaturday(DateTime date)
        {
            return IsDayOfWeek(date, DayOfWeek.Saturday);
        }

        public static bool IsSunday(DateTime date)
        {
            return IsDayOfWeek(date, DayOfWeek.Sunday);
        }

        public static bool IsDayOfWeek(DateTime date, DayOfWeek day)
        {
            return (date.DayOfWeek == day);
        }

        public static bool IsWeekdayRangeValid(int from, int to)
        {
            /*
             * Week ends on sunday.
             * 
             * Valid example's:
             * Monday-Friday
             * Monday-Saturday
             * Monday-Sunday
             * Sunday-Sunday
             * 
             * Invalid examples':
             * Saturday-Monday
             * Sunday-Monday 
             */

            bool valid =
                IsSunday(to)
                ||
                from <= to && !IsSunday(from);

            return valid;
        }

        public static bool IsDayTypeInRange(int? from, int? to, int dayOfWeek)
        {
            if (!from.HasValue || !to.HasValue)
                return false;
            return IsDayTypeInRange(from.Value, to.Value, dayOfWeek);
        }

        public static bool IsDayTypeInRange(int from, int to, int dayOfWeek)
        {
            bool found = false;

            //Only valid in chronologic order, thus: week always ends on sunday. Ex: Saturday-Monday or Sunday-Wednesday is not valid
            switch (dayOfWeek)
            {
                case (int)DayOfWeek.Monday:
                case (int)DayOfWeek.Tuesday:
                case (int)DayOfWeek.Wednesday:
                case (int)DayOfWeek.Thursday:
                case (int)DayOfWeek.Friday:
                case (int)DayOfWeek.Saturday:
                    found =
                        //From  (Monday-Saturday and earlier)
                        (IsWeekdayOrSaturday(from) && IsTodayOrEarlier(from, dayOfWeek))
                        &&
                        //To    (Monday-Saturday/Sunday and later)
                        ((IsWeekdayOrSaturday(to) && IsTodayOrLater(to, dayOfWeek))
                        ||
                        (IsSunday(to)));

                    break;
                case (int)DayOfWeek.Sunday:
                    found =
                        //From  (Monday-Saturday/Sunday)
                        (IsWeekdayOrSaturday(from) || IsSunday(from))
                        &&
                        //To    (Sunday)
                        (IsSunday(to));
                    break;
            }

            return found;
        }

        public static bool IsWeekdayOrSaturday(int dayNr)
        {
            return dayNr >= (int)DayOfWeek.Monday && dayNr <= (int)DayOfWeek.Saturday;
        }

        public static bool IsSunday(int dayNr)
        {
            return dayNr == (int)DayOfWeek.Sunday;
        }

        public static bool IsTodayOrEarlier(int dayNr, int today)
        {
            return dayNr <= today;
        }

        public static bool IsTodayOrLater(int dayNr, int today)
        {
            return dayNr >= today;
        }

        #endregion

        #region Social security number

        public static TermGroup_Sex GetSexFromSocialSecNr(string socialSecNr)
        {
            if (!String.IsNullOrEmpty(socialSecNr))
            {
                // Näst sista siffran är udda för män och jämn för kvinnor
                var charArray = socialSecNr.ToCharArray();
                if (charArray.Length > 2 && int.TryParse(charArray[charArray.Length - 2].ToString(), out int genderNumber))
                {
                    bool isEven = genderNumber % 2 == 0;
                    return isEven ? TermGroup_Sex.Female : TermGroup_Sex.Male;
                }
            }

            return TermGroup_Sex.Unknown;
        }

        public static int GetBirthYearFromSecurityNumber(string socSecNr)
        {
            var first = GetBirthDateFromSecurityNumber(socSecNr);

            if (first != null)
                return first.Value.Year;

            return DATETIME_DEFAULT.Year;
        }

        public static DateTime? GetBirthDateFromSecurityNumber(string socSecNr)
        {
            DateTime? date = null;

            if (string.IsNullOrEmpty(socSecNr))
                return DateTime.Today;

            if (socSecNr.ToLower().Contains("x") && socSecNr[socSecNr.Length - 4].ToString().ToLower().Equals("x", StringComparison.OrdinalIgnoreCase))
                socSecNr = socSecNr.ToLower().Replace("x", "0");

            // Supported formats:
            // YYYYMMDD-NNNN
            // YYYYMMDDNNNN
            // YYMMDD-NNNN
            // YYMMDDNNNN
            // YYYYMMDD
            // YYMMDD
            // YYYYMMDD-**** where the stars are any character

            // Remove all but digits
            socSecNr = socSecNr.ToNumeric();

            // Possible formats left:
            // YYYYMMDDNNNN
            // YYMMDDNNNN
            // YYYYMMDD
            // YYMMDD

            if (socSecNr.Length < 6 || socSecNr.Length > 12)
                return null;

            // Remove four last digits
            if (socSecNr.Length == 10 || socSecNr.Length == 12)
                socSecNr = socSecNr.Left(socSecNr.Length - 4);

            // Possible formats left:
            // YYYYMMDD
            // YYMMDD

            if (socSecNr.Length != 8 && socSecNr.Length != 6)
                return null;

            int day = NumberUtility.ToInteger(socSecNr.Right(2));
            if (day > 60)   // Samordningsnummer
                day -= 60;
            socSecNr = socSecNr.Left(socSecNr.Length - 2);

            // Possible formats left:
            // YYYYMM
            // YYMM

            int month = NumberUtility.ToInteger(socSecNr.Right(2));
            socSecNr = socSecNr.Left(socSecNr.Length - 2);

            // Possible formats left:
            // YYYY
            // YY

            int year = 0;

            if (socSecNr.Length == 4)
                year = NumberUtility.ToInteger(socSecNr);
            else
            {
                // Use Windows default two-digit year intepretation
                // 00-29 => 2000-2029
                // 30-99 => 1930-1999
                year = NumberUtility.ToInteger(socSecNr.Left(2));
                year += year < 30 ? 2000 : 1900;
            }

            try
            {
                date = new DateTime(year, month, day);
            }
            catch
            {
                date = null;
            }

            return date;
        }

        public static bool IsValidSocialSecurityNumber(string source, bool checkValidDate, bool mustSpecifyCentury, bool mustSpecifyDash, TermGroup_Sex sex = TermGroup_Sex.Unknown)
        {
            return IsValidSwedishSocialSecurityNumber(source, checkValidDate, mustSpecifyCentury, mustSpecifyDash, sex);
        }

        public static bool IsValidSocialSecurityNumber(TermGroup_Country companyCountry, string source, bool checkValidDate, bool mustSpecifyCentury, bool mustSpecifyDash, TermGroup_Sex sex = TermGroup_Sex.Unknown)
        {
            switch (companyCountry)
            {
                case TermGroup_Country.FI:
                    return IsValidFinnishSocialSecurityNumber(source);
                case TermGroup_Country.SE:
                default:
                    bool valid = IsValidSwedishSocialSecurityNumber(source, checkValidDate, mustSpecifyCentury, mustSpecifyDash, sex);
                    if (valid)
                        return valid;

                    //If not valid in Sweden try if it's valid in Finland (Same in SL side)
                    return CalendarUtility.IsValidFinnishSocialSecurityNumber(source);
            }
        }

        public static bool IsValidSwedishSocialSecurityNumber(string source, bool checkValidDate, bool mustSpecifyCentury, bool mustSpecifyDash, TermGroup_Sex sex = TermGroup_Sex.Unknown)
        {
            // Check length
            int length = 10;
            if (mustSpecifyDash)
                length++;
            else
                source = source.Trim().Replace("-", String.Empty);

            if (mustSpecifyCentury)
                length += 2;
            else if (source.Length >= 12 && (source.StartsWith("19") || source.StartsWith("20")))
                source = source.Remove(0, 2);

            if (String.IsNullOrEmpty(source) || source.Length != length)
                return false;

            if (checkValidDate)
            {
                // First six or eight chars must be a valid date
                try
                {
                    int year = Convert.ToInt32(source.Substring(0, mustSpecifyCentury ? 4 : 2));
                    // Year 2000 is special case since datetime must be greater then 0
                    if (year == 0)
                        year = 2000;

                    int month = Convert.ToInt32(source.Substring(mustSpecifyCentury ? 4 : 2, 2));
                    int day = Convert.ToInt32(source.Substring(mustSpecifyCentury ? 6 : 4, 2));
                    // Samordningsnummer
                    if (day > 60)
                        day -= 60;

                    DateTime date = new DateTime(year, month, day);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                    return false;
                }
            }

            // Validate sex if not unknown
            if (sex != TermGroup_Sex.Unknown && sex != GetSexFromSocialSecNr(source))
                return false;

            // This statement validates if the provided string is a valid Swedish social security number
            // in the format "YYYYMMDD-XNNN", where X is a identifier character (either a dash or plus sign), and NNN
            // is a three-digit number that identifies the individual. Exemple "19770101-X002"
            if (source.ToLower().Contains("x") && source[source.Length - 4].ToString().ToLower().Equals("x", StringComparison.OrdinalIgnoreCase))
                return true;

            // Remove century before control digit check
            if (mustSpecifyCentury)
                source = source.Substring(2);

            // Remove dash before control digit check
            source = source.Trim().Replace("-", String.Empty);

            // Check control digit
            char[] chars = source.ToLower().ToCharArray();
            int sum = 0;
            for (var i = source.Length - 1; i >= 0; i--)
            {
                if (!Int32.TryParse(chars[i].ToString(), out int val))
                    return false;

                val = val * (i % 2 - 2) * -1;
                if (val > 9)
                    val -= 9;
                sum += val;
            }

            return sum % 10 == 0;
        }

        public static bool IsValidFinnishSocialSecurityNumber(string source)
        {
            bool result = false;

            // Length must be 11
            if (source.Length == 11)
            {
                string birthDate = source.Substring(0, 6);
                string separator = source.Substring(6, 1);
                string personNo = source.Substring(7, 3);
                string checkSum = source.Substring(10, 1);

                // Check that numbers are valid
                Double.TryParse(birthDate, NumberStyles.Any, CultureInfo.CurrentCulture, out double birthDateNumber);
                Double.TryParse(personNo, NumberStyles.Any, CultureInfo.CurrentCulture, out double personNoNumber);

                // BirthDate is numbers only. Checksum too, prepare to do checking.
                if (birthDateNumber > 0 && personNoNumber > 0 && (CheckCharFinland(birthDate + personNo) == checkSum) && (BirthCenturyFinland(separator) >= 1800))
                    result = true;
            }

            return result;
        }

        public static string CheckCharFinland(string source)
        {
            // In Finland format is DDMMYY + NNN where NNN is serial number of the person
            // (Note! Some characters are not used!                                                   10   11   12   13   14   15   16   17   18   19   20   21   22   23   24   25   26   27   28   29   30
            string[] checkchars = new string[31] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "H", "J", "K", "L", "M", "N", "P", "R", "S", "T", "U", "V", "W", "X", "Y" };
            int checkvalue;
            string result = "";

            int nro = int.Parse(source);
            checkvalue = nro % 31;
            result = checkchars[checkvalue];

            return result; // returns alphabetic checkmark
        }

        public static int BirthCenturyFinland(string separator)
        {
            // In Finland separator specifies which century you were born

            int result = 0;

            if (separator?.Length == 1)
            {
                if ("ABCDEF".Contains(separator))
                    result = 2000;
                else if ("-YXWVU".Contains(separator))
                    result = 1900;
                else if (separator == "+")
                    result = 1800;
            }
            
            return result;
        }

        #endregion

        #region IBAN
        /// <summary>
        /// Should we need to check validity of any IBAN account, we can use this function no matter on which country or bank account is. 
        /// </summary>
        /// <param name="IBAN"></param>
        /// <returns></returns>
        public static bool IsValidIban(string IBAN)
        {

            bool retvalue = true;  // True by default

            // Let's check it's at least 15 characters (Shortest possible IBAN is 15 in Norway). 
            if (IBAN.Length < 15)
            {
                retvalue = false;
            }
            else
            {   // Length ok, now we need to check formula

                char first = IBAN[0];
                char second = IBAN[1];
                char third = IBAN[2];
                char fourth = IBAN[3];

                // let's see if first to characters are valid
                if (first <= 'A' || first >= 'Z' || second <= 'A' || second >= 'Z') retvalue = false;

                // let's see third & fourth are numbers
                if (third <= '0' || third >= '9' || fourth <= '0' || fourth >= '9') retvalue = false;
            }

            if (retvalue)  // if already false, no reason to check further. 
            {
                // Converting countrycode to numbers
                string cCountry = IBAN.Substring(0, 2);
                string checknumber = IBAN.Substring(2, 2);

                // Translate IBAN to checkable 
                string chkIBAN = IBAN.Substring(4, IBAN.Length - 4) + IBAN.Substring(0, 4);

                // Replace letters with numbers if exist
                chkIBAN = Regex.Replace(chkIBAN, @"\D", m => ((int)m.Value[0] - 55).ToString());

                // Let's calculate checksum with modulo 97
                int leftover = 0;
                while (chkIBAN.Length >= 7)
                {
                    leftover = int.Parse(leftover + chkIBAN.Substring(0, 7)) % 97;
                    chkIBAN = chkIBAN.Substring(7);
                }

                leftover = int.Parse(leftover + chkIBAN) % 97;

                if (leftover != 1)
                {
                    retvalue = false;
                }
                else
                {
                    retvalue = true;
                }
            }

            return retvalue;
        }

        #endregion

        #region Contract

        public static Tuple<int, int> CalculateNextPeriod(TermGroup_ContractGroupPeriod period, int interval, int currentYear, int currentValue)
        {
            // Increase currentValue with interval
            // Handle year transitions

            if (currentYear == 0)
                currentYear = DateTime.Today.Year;

            switch (period)
            {
                case TermGroup_ContractGroupPeriod.Week:
                    #region Week

                    if (currentValue == 0)
                        currentValue = GetWeekNr(DateTime.Today);

                    int nbrOfWeeks = 0;
                    for (int i = 1; i <= interval; i++)
                    {
                        // Check number of weeks in current year
                        nbrOfWeeks = GetNoOfWeeksInYear(currentYear);
                        currentValue++;
                        if (currentValue > nbrOfWeeks)
                        {
                            // If week number passed number of avaliable weeks, move to next year
                            currentYear++;
                            currentValue = 1;
                        }
                    }

                    #endregion
                    break;
                case TermGroup_ContractGroupPeriod.Month:
                    #region Month

                    if (currentValue == 0)
                        currentValue = DateTime.Today.Month;

                    currentValue += interval;
                    while (currentValue > 12)
                    {
                        // If month passed number of available months, move to next year
                        currentValue -= 12;
                        currentYear++;
                    }

                    #endregion
                    break;
                case TermGroup_ContractGroupPeriod.Quarter:
                    #region Quarter

                    if (currentValue == 0)
                        currentValue = GetQuarterNr(DateTime.Today);

                    currentValue += interval;
                    while (currentValue > 4)
                    {
                        // If quarter passed number of available quarters, move to next year
                        currentValue -= 4;
                        currentYear++;
                    }

                    #endregion
                    break;
                case TermGroup_ContractGroupPeriod.Year:
                    #region Year

                    if (currentValue == 0)
                        currentValue = 1;
                    currentYear += interval;

                    #endregion
                    break;
                case TermGroup_ContractGroupPeriod.CalendarYear:
                    #region CalendarYear

                    // Move to next year
                    currentYear++;
                    currentValue = 1;

                    #endregion
                    break;
            }

            return new Tuple<int, int>(currentYear, currentValue);
        }

        public static Tuple<int, int> CalculateCurrentPeriod(TermGroup_ContractGroupPeriod period, DateTime date)
        {
            int currentYear = date.Year;
            int currentValue = 0;

            switch (period)
            {
                case TermGroup_ContractGroupPeriod.Week:
                    #region Week

                    currentValue = GetWeekNr(date);

                    #endregion
                    break;
                case TermGroup_ContractGroupPeriod.Month:
                    #region Month

                    currentValue = date.Month;

                    #endregion
                    break;
                case TermGroup_ContractGroupPeriod.Quarter:
                    #region Quarter

                    currentValue = GetQuarterNr(date);

                    #endregion
                    break;
                case TermGroup_ContractGroupPeriod.Year:
                case TermGroup_ContractGroupPeriod.CalendarYear:
                    #region CalendarYear

                    currentValue = 1;

                    #endregion
                    break;
            }

            return new Tuple<int, int>(currentYear, currentValue);
        }
        public static Tuple<int, int> CalculatePreviousPeriod(TermGroup_ContractGroupPeriod period, int interval, int currentYear, int currentValue)
        {
            if (currentYear == 0)
                currentYear = DateTime.Today.Year;

            switch (period)
            {
                case TermGroup_ContractGroupPeriod.Week:
                    #region Week
                    if (currentValue == 0)
                        currentValue = GetWeekNr(DateTime.Today);

                    int nbrOfWeeks;
                    for (int i = 1; i <= interval; i++)
                    {
                        if (currentValue == 1)
                        {
                            currentYear--;
                            nbrOfWeeks = GetNoOfWeeksInYear(currentYear);
                            currentValue = nbrOfWeeks;
                        }
                        else
                        {
                            currentValue--;
                        }
                    }
                    #endregion
                    break;
                case TermGroup_ContractGroupPeriod.Month:
                    #region Month
                    if (currentValue == 0)
                        currentValue = DateTime.Today.Month;

                    currentValue -= interval;
                    while (currentValue < 1)
                    {
                        currentValue += 12;
                        currentYear--;
                    }
                    #endregion
                    break;
                case TermGroup_ContractGroupPeriod.Quarter:
                    #region Quarter
                    if (currentValue == 0)
                        currentValue = GetQuarterNr(DateTime.Today);

                    currentValue -= interval;
                    while (currentValue < 1)
                    {
                        currentValue += 4;
                        currentYear--;
                    }
                    #endregion
                    break;
                case TermGroup_ContractGroupPeriod.Year:
                    #region Year
                    if (currentValue == 0)
                        currentValue = 1;
                    currentYear -= interval;
                    #endregion
                    break;
                case TermGroup_ContractGroupPeriod.CalendarYear:
                    #region CalendarYear
                    currentYear--;
                    currentValue = 1;
                    #endregion
                    break;
            }

            return new Tuple<int, int>(currentYear, currentValue);
        }

        public static DateTime ConvertContractPeriodToDate(TermGroup_ContractGroupPeriod period, DateTime startDate, int year, int value, int dayInMonth)
        {
            DateTime date = startDate;

            if (year == 0)
                year = DateTime.Today.Year;

            switch (period)
            {
                case TermGroup_ContractGroupPeriod.Week:
                    #region Week

                    // Get monday in specified week
                    date = GetFirstDateOfWeek(year, value);

                    #endregion
                    break;
                case TermGroup_ContractGroupPeriod.Month:
                    #region Month

                    // Month must be 1-12
                    if (value < 1 || value > 12)
                        break;

                    // Default is first day in specified month
                    date = new DateTime(year, value, 1);
                    if (dayInMonth > 0)
                    {
                        DateTime lastDayInMonth = GetLastDateOfMonth(date);
                        if (dayInMonth > lastDayInMonth.Day)
                            date = new DateTime(year, value, lastDayInMonth.Day);
                        else
                            date = new DateTime(year, value, dayInMonth);
                    }

                    #endregion
                    break;
                case TermGroup_ContractGroupPeriod.Quarter:
                    #region Quarter

                    // Quarter must be 1-4
                    if (value < 1 || value > 4)
                        break;
                    // Always the first day in specified quarter
                    date = new DateTime(year, (value * 3) - 2, 1);

                    #endregion
                    break;
                case TermGroup_ContractGroupPeriod.Year:
                    #region Year

                    // Same date as start date, just changing the year
                    date = new DateTime(year, startDate.Month, startDate.Day);

                    #endregion
                    break;
                case TermGroup_ContractGroupPeriod.CalendarYear:
                    #region CalendarYear

                    // Always 1:st of january, just changing the year
                    date = new DateTime(year, 1, 1);

                    #endregion
                    break;
            }

            return date;
        }

        #endregion
    }
}
