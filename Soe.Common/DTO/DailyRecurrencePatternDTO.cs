using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Common.DTO
{
    // https://msdn.microsoft.com/office/office365/APi/complex-types-for-mail-contacts-calendar#RecurrencePattern

    [TSInclude]
    public class DailyRecurrencePatternDTO : DailyRecurrenceBase
    {
        #region Variables

        public DailyRecurrencePatternType Type { get; set; }
        public int Interval { get; set; }
        public int DayOfMonth { get; set; }                 // 1-31
        public int Month { get; set; }                      // 1-12
        public List<DayOfWeek> DaysOfWeek { get; set; }     // Sunday = 0, Monday = 1, Tuesday = 2, Wednesday = 3, Thursday = 4, Friday = 5, Saturday = 6
        public DayOfWeek FirstDayOfWeek { get; set; }       // Sunday = 0, Monday = 1, Tuesday = 2, Wednesday = 3, Thursday = 4, Friday = 5, Saturday = 6
        public DailyRecurrencePatternWeekIndex WeekIndex { get; set; }
        public List<int> SysHolidayTypeIds { get; set; }

        #endregion

        #region Ctor

        public DailyRecurrencePatternDTO() : base()
        {
            this.Type = DailyRecurrencePatternType.Daily;
            this.Interval = 1;
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            List<int> days = new List<int>();
            if (this.DaysOfWeek != null)
            {
                foreach (DayOfWeek day in this.DaysOfWeek)
                {
                    days.Add((int)day);
                }
            }

            return String.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}",
                (int)this.Type,
                this.Interval,
                this.DayOfMonth,
                this.Month,
                days.Count > 0 ? String.Join(",", days) : String.Empty,
                (int)this.FirstDayOfWeek,
                (int)this.WeekIndex,
                this.SysHolidayTypeIds != null && this.SysHolidayTypeIds.Count > 0 ? String.Join(",", this.SysHolidayTypeIds) : String.Empty);
        }

        public bool IsBaseNeed(DayOfWeek givenDayOfWeek)
        {
            if (this.Type == DailyRecurrencePatternType.Daily)
                return true;
            if (this.Type == DailyRecurrencePatternType.Weekly && this.DaysOfWeek.Contains(givenDayOfWeek))
                return true;
            return false;
        }

        public bool IsAdditionalNeed()
        {
            if (this.Type == DailyRecurrencePatternType.Weekly && this.Interval > 1)
                return true;
            if (this.Type == DailyRecurrencePatternType.RelativeMonthly)
                return true;
            if (this.Type == DailyRecurrencePatternType.RelativeYearly)
                return true;
            if (this.Type == DailyRecurrencePatternType.AbsoluteMonthly)
                return true;
            if (this.Type == DailyRecurrencePatternType.AbsoluteYearly)
                return true;
            return false;
        }

        #endregion

        #region Static methods

        public static bool IsBaseNeed(string pattern, DayOfWeek givenDayOfWeek)
        {
            DailyRecurrencePatternDTO dto = DailyRecurrencePatternDTO.Parse(pattern);
            return dto != null && dto.IsBaseNeed(givenDayOfWeek);
        }

        public static bool IsAdditionalNeed(string pattern)
        {
            DailyRecurrencePatternDTO dto = DailyRecurrencePatternDTO.Parse(pattern);
            return dto != null && dto.IsAdditionalNeed();
        }

        public static bool HasNoRecurrencePattern(string pattern)
        {
            DailyRecurrencePatternDTO dto = DailyRecurrencePatternDTO.Parse(pattern);
            return dto != null && dto.Type == DailyRecurrencePatternType.None;
        }

        public static bool IsSysHolidayTypePattern(string pattern)
        {
            string[] parts = pattern.Split('_');
            DailyRecurrencePatternType type = (DailyRecurrencePatternType)StringUtility.GetInt(parts[0]);

            return type == DailyRecurrencePatternType.SysHoliday;
        }

        public static DailyRecurrencePatternDTO Parse(string pattern)
        {
            string[] parts = pattern.Split('_');

            if (parts.Length != 7 && parts.Length != 8)
                return null;

            DailyRecurrencePatternDTO dto = new DailyRecurrencePatternDTO();
            dto.Type = (DailyRecurrencePatternType)StringUtility.GetInt(parts[0]);
            if (dto.Type != DailyRecurrencePatternType.None)
            {
                dto.Interval = StringUtility.GetInt(parts[1]);
                dto.DayOfMonth = StringUtility.GetInt(parts[2]);
                dto.Month = StringUtility.GetInt(parts[3]);

                // Days are comma separated
                string[] strDays = parts[4].Split(',');
                dto.DaysOfWeek = new List<DayOfWeek>();
                foreach (var strDay in strDays)
                {
                    if (strDay.Length > 0)
                        dto.DaysOfWeek.Add((DayOfWeek)StringUtility.GetInt(strDay));
                }

                dto.FirstDayOfWeek = (DayOfWeek)StringUtility.GetInt(parts[5]);
                dto.WeekIndex = (DailyRecurrencePatternWeekIndex)StringUtility.GetInt(parts[6]);

                if (parts.Length > 7)
                {
                    // Sys holidays are comma separated
                    string[] strSysHolidayTypeIds = parts[7].Split(',');
                    dto.SysHolidayTypeIds = new List<int>();
                    foreach (var sysHolidayTypeId in strSysHolidayTypeIds)
                    {
                        if (sysHolidayTypeId.Length > 0)
                            dto.SysHolidayTypeIds.Add(StringUtility.GetInt(sysHolidayTypeId));
                    }
                }
            }

            return dto;
        }

        public static DailyRecurrenceDatesOutput GetDatesFromPattern(string pattern, DateTime startDate, DateTime visibleDateFrom, DateTime visibleDateTo, int? nbrOfOccurrences, List<DateTime> excludeDates = null, List<HolidayDTO> holidays = null)
        {
            List<DateTime> dates = new List<DateTime>();

            if (nbrOfOccurrences.HasValue && nbrOfOccurrences.Value == 0)
                nbrOfOccurrences = null;

            int occurrences = 0;
            if (startDate <= visibleDateTo && pattern != null && pattern.Length > 0)
            {
                // Get pattern
                DailyRecurrencePatternDTO dto = Parse(pattern);
                if (dto != null && dto.Type != DailyRecurrencePatternType.None)
                {
                    if (dto.Type == DailyRecurrencePatternType.SysHoliday)
                    {
                        #region Sys holidays

                        if (holidays != null)
                        {
                            foreach (int sysHolidayTypeId in dto.SysHolidayTypeIds)
                            {
                                var holidaysOfType = holidays.Where(h => h.SysHolidayTypeId == sysHolidayTypeId).ToList();
                                foreach (var holidayOfType in holidaysOfType)
                                {
                                    if (holidayOfType.Date >= visibleDateFrom && holidayOfType.Date <= visibleDateTo)
                                        dates.Add(holidayOfType.Date);
                                }
                            }
                        }

                        #endregion
                    }
                    else
                    {
                        // Get interval
                        int interval = dto.Interval > 0 ? dto.Interval : 1;

                        // Always start loop at start date of pattern to get correct interval
                        DateTime currentDate = startDate;

                        // Loop until end of visible range
                        while (currentDate <= CalendarUtility.GetLastDateOfWeek(visibleDateTo) && (!nbrOfOccurrences.HasValue || occurrences < nbrOfOccurrences.Value))
                        {
                            switch (dto.Type)
                            {
                                case DailyRecurrencePatternType.Daily:
                                    #region Every day

                                    // Add every day with specified interval
                                    if (currentDate >= startDate && currentDate >= visibleDateFrom && currentDate <= visibleDateTo)
                                        dates.Add(currentDate);

                                    // Always increase occurrences, even outside visible range
                                    occurrences++;

                                    // Increase date with specified number of days in interval
                                    currentDate = currentDate.AddDays(interval);
                                    break;
                                #endregion
                                case DailyRecurrencePatternType.Weekly:
                                    #region Every week

                                    // Must make extra check on date range since GetDatesInWeek() check a whole week
                                    List<DateTime> datesInWeek = GetDatesInWeek(currentDate, dto.DaysOfWeek).Where(d => d >= startDate && d >= visibleDateFrom && d <= visibleDateTo).ToList();

                                    // Add all days in specified DaysOfWeek
                                    if (datesInWeek.Count > 0)
                                        dates.AddRange(datesInWeek.Where(d => d >= startDate && d >= visibleDateFrom && d <= visibleDateTo));

                                    // Always increase occurrences, even outside visible range
                                    occurrences += datesInWeek.Count;

                                    // Increase date with specified number of weeks in interval
                                    currentDate = currentDate.AddDays(7 * interval);
                                    break;
                                #endregion
                                case DailyRecurrencePatternType.AbsoluteMonthly:
                                    #region Same day every month

                                    // Get actual date for current month
                                    DateTime currentMonthDate = GetDateForDayOfMonth(currentDate, dto.DayOfMonth);
                                    if (currentMonthDate >= startDate && currentMonthDate >= visibleDateFrom && currentMonthDate <= visibleDateTo)
                                        dates.Add(currentMonthDate);

                                    // Always increase occurrences, even outside visible range
                                    occurrences++;

                                    // Increase date with specified number of months in interval
                                    // Use first day of month to make sure specified day exists in next loop
                                    currentDate = CalendarUtility.GetFirstDateOfMonth(currentDate).AddMonths(interval);
                                    break;
                                #endregion
                                case DailyRecurrencePatternType.RelativeMonthly:
                                    #region Same week every month

                                    // Get this months first occurrence of specified day of week (eg. first friday of month)
                                    currentDate = CalendarUtility.GetFirstDateOfMonth(currentDate);
                                    while (currentDate.DayOfWeek != dto.FirstDayOfWeek)
                                    {
                                        currentDate = currentDate.AddDays(1);
                                    }

                                    if (dto.WeekIndex == DailyRecurrencePatternWeekIndex.Last)
                                    {
                                        // Get last day of week for specified month
                                        currentDate = CalendarUtility.GetLastDateOfMonth(currentDate);
                                        while (currentDate.DayOfWeek != dto.FirstDayOfWeek)
                                        {
                                            currentDate = currentDate.AddDays(-1);
                                        }
                                    }
                                    else
                                    {
                                        // Get date for specified week index (eg. second friday)
                                        currentDate = currentDate.AddDays((int)dto.WeekIndex * 7);
                                    }
                                    if (currentDate >= startDate && currentDate >= visibleDateFrom && currentDate <= visibleDateTo)
                                        dates.Add(currentDate);

                                    // Always increase occurrences, even outside visible range
                                    occurrences++;

                                    // Increase date with specified number of months in interval
                                    // Use first day of month to make sure specified day exists in next loop
                                    currentDate = CalendarUtility.GetFirstDateOfMonth(currentDate).AddMonths(interval);
                                    break;
                                #endregion
                                case DailyRecurrencePatternType.AbsoluteYearly:
                                    #region Same day every year

                                    // Get actual date for current month
                                    DateTime currentYearDate = GetDateForDayOfYear(currentDate, dto.Month, dto.DayOfMonth);
                                    if (currentYearDate >= startDate && currentYearDate >= visibleDateFrom && currentYearDate <= visibleDateTo)
                                        dates.Add(currentYearDate);

                                    // Always increase occurrences, even outside visible range
                                    occurrences++;

                                    // Increase date with one year
                                    // Use first day of month to make sure specified day exists in next loop
                                    currentDate = CalendarUtility.GetFirstDateOfMonth(currentYearDate).AddYears(1);
                                    break;
                                #endregion
                                case DailyRecurrencePatternType.RelativeYearly:
                                    #region Same week every year

                                    // Get next occurrence of specified month
                                    currentDate = CalendarUtility.GetFirstDateOfMonth(currentDate);
                                    while (currentDate.Month != dto.Month)
                                    {
                                        currentDate = currentDate.AddMonths(1);
                                    }

                                    // Get this months first occurrence of specified day of week (eg. first friday of month)
                                    while (currentDate.DayOfWeek != dto.FirstDayOfWeek)
                                    {
                                        currentDate = currentDate.AddDays(1);
                                    }

                                    if (dto.WeekIndex == DailyRecurrencePatternWeekIndex.Last)
                                    {
                                        // Get last day of week for specified month
                                        currentDate = CalendarUtility.GetLastDateOfMonth(currentDate);
                                        while (currentDate.DayOfWeek != dto.FirstDayOfWeek)
                                        {
                                            currentDate = currentDate.AddDays(-1);
                                        }
                                    }
                                    else
                                    {
                                        // Get date for specified week index (eg. second friday)
                                        currentDate = currentDate.AddDays((int)dto.WeekIndex * 7);
                                    }
                                    if (currentDate >= startDate && currentDate >= visibleDateFrom && currentDate <= visibleDateTo)
                                        dates.Add(currentDate);

                                    // Always increase occurrences, even outside visible range
                                    occurrences++;

                                    // Increase date with one year
                                    // Use first day of month to make sure specified day exists in next loop
                                    currentDate = CalendarUtility.GetFirstDateOfMonth(currentDate).AddYears(1);
                                    break;
                                    #endregion
                            }
                        }
                    }
                }
            }

            return new DailyRecurrenceDatesOutput(dates, excludeDates);
        }

        public static string GetRecurrenceDescription(string pattern, List<GenericType> terms, int langId, List<SysHolidayTypeDTO> sysHolidayTypes)
        {
            if (terms == null)
                return String.Empty;

            var dto = DailyRecurrencePatternDTO.Parse(pattern);
            if (dto == null)
                return String.Empty;
            if (dto.Type == DailyRecurrencePatternType.None)
                return GetText(1, "Ingen upprepning", terms, langId);

            string intervalExt = langId == (int)TermGroup_Languages.Swedish ? CalendarUtility.GetSwedishDayExtension(dto.Interval) : string.Empty;
            string dayOfOfMonthExt = langId == (int)TermGroup_Languages.Swedish ? CalendarUtility.GetSwedishDayExtension(dto.DayOfMonth) : string.Empty;
            string typeDescription = "";

            if (dto.Type == DailyRecurrencePatternType.Daily)
            {
                #region Daily

                typeDescription += GetIntervalDayDescription(dto.Interval, intervalExt, terms, langId);

                #endregion
            }
            else if (dto.Type == DailyRecurrencePatternType.Weekly)
            {
                #region Weekly

                typeDescription += GetIntervalWeekDescription(dto.Interval, intervalExt, terms, langId);
                typeDescription += " ";
                typeDescription += GetText(18, "på en", terms, langId);
                typeDescription += " ";
                typeDescription += GetDaysOfWeekName(dto.DaysOfWeek, terms, langId);

                #endregion
            }
            else if (dto.Type == DailyRecurrencePatternType.AbsoluteMonthly)
            {
                #region AbsoluteMonthly

                typeDescription += GetText(17, "den", terms, langId);
                typeDescription += " ";
                typeDescription += String.Format("{0}{1}", dto.DayOfMonth, dayOfOfMonthExt);
                typeDescription += " ";
                typeDescription += GetIntervalMonthDescription(dto.Interval, intervalExt, terms, langId);

                #endregion
            }
            else if (dto.Type == DailyRecurrencePatternType.RelativeMonthly)
            {
                #region RelativeMonthly

                typeDescription += GetText(17, "den", terms, langId);
                typeDescription += " ";
                typeDescription += GetWeekIndexName(dto.WeekIndex, terms, langId);
                typeDescription += " ";
                typeDescription += GetDayOfWeekName(dto.FirstDayOfWeek, terms, langId, definedForm: true);
                typeDescription += " ";
                typeDescription += GetIntervalMonthDescription(dto.Interval, intervalExt, terms, langId);

                #endregion
            }
            else if (dto.Type == DailyRecurrencePatternType.AbsoluteYearly)
            {
                #region AbsoluteYearly

                typeDescription += GetText(17, "den", terms, langId);
                typeDescription += " ";
                typeDescription += String.Format("{0}{1}", dto.DayOfMonth, dayOfOfMonthExt);
                typeDescription += " ";
                typeDescription += GetMonthName(dto.Month, terms, langId);
                typeDescription += " ";
                typeDescription += String.Format(GetText(11, "varje år", terms, langId));

                #endregion
            }
            else if (dto.Type == DailyRecurrencePatternType.RelativeYearly)
            {
                #region RelativeYearly

                typeDescription += GetText(17, "den", terms, langId);
                typeDescription += " ";
                typeDescription += GetWeekIndexName(dto.WeekIndex, terms, langId);
                typeDescription += " ";
                typeDescription += GetDayOfWeekName(dto.FirstDayOfWeek, terms, langId, definedForm: true);
                typeDescription += " ";
                typeDescription += GetText(19, "i", terms, langId);
                typeDescription += " ";
                typeDescription += GetMonthName(dto.Month, terms, langId);

                #endregion
            }
            else if (dto.Type == DailyRecurrencePatternType.SysHoliday)
            {
                #region SysHoliday

                foreach (int sysHolidayTypeId in dto.SysHolidayTypeIds)
                {
                    SysHolidayTypeDTO type = sysHolidayTypes.FirstOrDefault(t => t.SysHolidayTypeId == sysHolidayTypeId);
                    if (type != null)
                    {
                        if (!String.IsNullOrEmpty(typeDescription))
                            typeDescription += ", ";
                        typeDescription += type.Name;
                    }
                }

                #endregion
            }

            return String.Format("{0} {1}", GetText(41, "Inträffar", terms, langId), typeDescription);
        }

        #endregion

        #region Help-methods


        #endregion
    }

    [TSInclude]
    public class DailyRecurrenceRangeDTO : DailyRecurrenceBase
    {
        #region Variables

        public DailyRecurrenceRangeType Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int NumberOfOccurrences { get; set; }

        #endregion

        #region Ctor

        public DailyRecurrenceRangeDTO() : base()
        {
            this.Type = DailyRecurrenceRangeType.NoEnd;
        }

        #endregion

        #region Public methods

        public override string ToString()
        {
            return String.Format("{0}_{1}_{2}_{3}", (int)this.Type, this.StartDate.HasValue ? this.StartDate.Value.ToString("yyyy-MM-dd") : String.Empty, this.EndDate.HasValue ? this.EndDate.Value.ToString("yyyy-MM-dd") : String.Empty, this.NumberOfOccurrences);
        }

        #endregion

        #region Static methods

        public static DailyRecurrenceRangeDTO Parse(string str)
        {
            string[] parts = str.Split('_');

            if (parts.Length != 4)
                return null;

            DailyRecurrenceRangeDTO dto = new DailyRecurrenceRangeDTO();
            dto.Type = (DailyRecurrenceRangeType)int.Parse(parts[0]);

            DateTime startDate;
            if (parts[1].Length > 0)
            {
                if (DateTime.TryParseExact(parts[1], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate))
                    dto.StartDate = startDate;
            }
            DateTime endDate;
            if (parts[1].Length > 0)
            {
                if (DateTime.TryParseExact(parts[2], "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                    dto.EndDate = endDate;
            }

            dto.NumberOfOccurrences = int.Parse(parts[3]);

            return dto;
        }

        public static string GetRecurrenceRangeStartsOnDescription(DateTime? startDate)
        {
            return startDate.HasValue ? startDate.Value.ToString("yyyy-MM-dd") : String.Empty;
        }

        public static string GetRecurrenceRangeEndsOnDescription(DateTime? endDate, int? numberOfOccurrences, List<GenericType> terms, int langId)
        {
            if (endDate.HasValue)
                return endDate.Value.ToString("yyyy-MM-dd");
            else if (numberOfOccurrences.HasValue && numberOfOccurrences.Value > 0)
                return String.Format(GetText(40, "Slutar efter {0} gånger", terms, langId), numberOfOccurrences);
            else
                return String.Empty;
        }

        #endregion
    }

    public class DailyRecurrenceBase
    {
        #region Ctor

        protected DailyRecurrenceBase()
        {

        }

        #endregion

        #region Static methods

        protected static List<DateTime> GetDatesInWeek(DateTime date, List<DayOfWeek> daysOfWeek)
        {
            List<DateTime> dates = new List<DateTime>();

            // Get first date of specified week
            date = CalendarUtility.GetFirstDateOfWeek(date);

            // Loop through week and add dates where day of week is specified
            for (int i = 0; i < 7; i++)
            {
                if (daysOfWeek.Contains(date.AddDays(i).DayOfWeek))
                    dates.Add(date.AddDays(i));
            }

            return dates;
        }

        protected static DateTime GetDateForDayOfMonth(DateTime date, int day)
        {
            // Get date for specified months day
            // If day does not exist in current month (eg. 31) use last day of month
            DateTime monthDate;
            try
            {
                monthDate = new DateTime(date.Year, date.Month, day);
            }
            catch (ArgumentOutOfRangeException)
            {
                monthDate = new DateTime(date.Year, date.Month, 1);
                monthDate = CalendarUtility.GetEndOfMonth(monthDate);
            }

            return monthDate;
        }

        protected static DateTime GetDateForDayOfYear(DateTime date, int month, int day)
        {
            // Get date for specified month and day
            // If day does not exist in current month (eg. 29/2) use last day of month
            DateTime monthDate;
            try
            {
                monthDate = new DateTime(date.Year, month, day);
            }
            catch (ArgumentOutOfRangeException)
            {
                monthDate = new DateTime(date.Year, month, 1);
                monthDate = CalendarUtility.GetEndOfMonth(monthDate);
            }

            return monthDate;
        }

        protected static string GetDayOfWeekName(DayOfWeek dayOfWeek, List<GenericType> terms, int langId, bool definedForm = false)
        {
            string dayOfWeekName = "";

            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    dayOfWeekName = GetText(21, "måndag", terms, langId);
                    break;
                case DayOfWeek.Tuesday:
                    dayOfWeekName = GetText(22, "tisdag", terms, langId);
                    break;
                case DayOfWeek.Wednesday:
                    dayOfWeekName = GetText(23, "onsdag", terms, langId);
                    break;
                case DayOfWeek.Thursday:
                    dayOfWeekName = GetText(24, "torsdag", terms, langId);
                    break;
                case DayOfWeek.Friday:
                    dayOfWeekName = GetText(25, "fredag", terms, langId);
                    break;
                case DayOfWeek.Saturday:
                    dayOfWeekName = GetText(26, "lördag", terms, langId);
                    break;
                case DayOfWeek.Sunday:
                    dayOfWeekName = GetText(27, "söndag", terms, langId);
                    break;
            }

            if (!string.IsNullOrEmpty(dayOfWeekName) && langId == (int)TermGroup_Languages.Swedish && definedForm)
                dayOfWeekName += "en";

            return dayOfWeekName;
        }

        protected static string GetDaysOfWeekName(List<DayOfWeek> daysOfWeek, List<GenericType> terms, int langId)
        {
            if (daysOfWeek == null || daysOfWeek.Count == 0)
                return String.Format("({0})", GetText(20, "inga veckodagar valda", terms, langId));

            StringBuilder sb = new StringBuilder();
            foreach (DayOfWeek dayOfWeek in daysOfWeek)
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(GetDayOfWeekName(dayOfWeek, terms, langId));
            }
            return sb.ToString();
        }

        protected static string GetWeekIndexName(DailyRecurrencePatternWeekIndex weekIndex, List<GenericType> terms, int langId)
        {
            string weekIndexName = "";
            switch (weekIndex)
            {
                case DailyRecurrencePatternWeekIndex.First:
                    weekIndexName = GetText(12, "första", terms, langId);
                    break;
                case DailyRecurrencePatternWeekIndex.Second:
                    weekIndexName = GetText(13, "andra", terms, langId);
                    break;
                case DailyRecurrencePatternWeekIndex.Third:
                    weekIndexName = GetText(14, "tredje", terms, langId);
                    break;
                case DailyRecurrencePatternWeekIndex.Fourth:
                    weekIndexName = GetText(15, "fjärde", terms, langId);
                    break;
                case DailyRecurrencePatternWeekIndex.Last:
                    weekIndexName = GetText(16, "sista", terms, langId);
                    break;
            }
            return weekIndexName;
        }

        protected static string GetMonthName(int month, List<GenericType> terms, int langId)
        {
            string monthName = "";

            switch (month)
            {
                case 1:
                    monthName += GetText(28, "januari", terms, langId);
                    break;
                case 2:
                    monthName += GetText(29, "februari", terms, langId);
                    break;
                case 3:
                    monthName += GetText(30, "mars", terms, langId);
                    break;
                case 4:
                    monthName += GetText(31, "april", terms, langId);
                    break;
                case 5:
                    monthName += GetText(32, "maj", terms, langId);
                    break;
                case 6:
                    monthName += GetText(33, "juni", terms, langId);
                    break;
                case 7:
                    monthName += GetText(34, "juli", terms, langId);
                    break;
                case 8:
                    monthName += GetText(35, "augusti", terms, langId);
                    break;
                case 9:
                    monthName += GetText(36, "september", terms, langId);
                    break;
                case 10:
                    monthName += GetText(37, "oktober", terms, langId);
                    break;
                case 11:
                    monthName += GetText(38, "november", terms, langId);
                    break;
                case 12:
                    monthName += GetText(39, "december", terms, langId);
                    break;
            }

            return monthName;
        }

        protected static string GetIntervalDayDescription(int interval, string intervalExt, List<GenericType> terms, int langId)
        {
            if (interval <= 0)
                return String.Empty;

            if (interval == 1)
                return GetText(2, "varje dag", terms, langId);
            else if (interval == 2)
                return GetText(3, "varannan dag", terms, langId);
            else
                return String.Format(GetText(4, "var {0}{1} dag", terms, langId), interval, intervalExt);
        }

        protected static string GetIntervalWeekDescription(int interval, string intervalExt, List<GenericType> terms, int langId)
        {
            if (interval <= 0)
                return String.Empty;

            if (interval == 1)
                return GetText(5, "varje vecka", terms, langId);
            else if (interval == 2)
                return GetText(6, "varannan vecka", terms, langId);
            else
                return String.Format(GetText(7, "var {0}{1} vecka", terms, langId), interval, intervalExt);
        }

        protected static string GetIntervalMonthDescription(int interval, string intervalExt, List<GenericType> terms, int langId)
        {
            if (interval <= 0)
                return String.Empty;

            if (interval == 1)
                return GetText(8, "varje månad", terms, langId);
            else if (interval == 2)
                return GetText(9, "varannan månad", terms, langId);
            else
                return String.Format(GetText(10, "var {0}{1} månad", terms, langId), interval, intervalExt);
        }

        protected static string GetText(int sysTermId, string name, List<GenericType> terms, int langId)
        {
            GenericType term = terms != null ? terms.FirstOrDefault(i => i.Id == sysTermId) : null;
            return term != null ? term.Name : String.Empty;
        }

        #endregion
    }

    [TSInclude]
    public class DailyRecurrenceDatesOutput
    {
        #region Variables

        // Needs to be public to make them visible in Angular
        // Should use the methods below when accessing them here on the server side
        public List<DateTime> RecurrenceDates { get; set; }
        public List<DateTime> RemovedDates { get; set; }

        #endregion

        #region Ctor

        public DailyRecurrenceDatesOutput()
        {

        }

        public DailyRecurrenceDatesOutput(List<DateTime> recurrenceDates, List<DateTime> removedDates)
        {
            this.RecurrenceDates = recurrenceDates != null ? recurrenceDates : new List<DateTime>();
            this.RemovedDates = removedDates != null ? removedDates : new List<DateTime>();
        }

        #endregion

        #region Public methods

        public List<DateTime> GetValidDates(bool doIncludeRemovedDates = false)
        {
            return doIncludeRemovedDates ? this.RecurrenceDates : this.RecurrenceDates.Where(i => !this.RemovedDates.Contains(i.Date)).ToList();
        }

        public bool DoRecurOnDate(DateTime date, bool doIncludeRemovedDates = false)
        {
            return this.GetValidDates(doIncludeRemovedDates).Contains(date);
        }

        public bool DoRecurOnDateButIsRemoved(DateTime date)
        {
            return this.RecurrenceDates.Contains(date) && this.RemovedDates.Contains(date);
        }

        public bool HasRecurringDates(bool doIncludeRemovedDates = false)
        {
            return this.GetValidDates(doIncludeRemovedDates).Count > 0;
        }

        public void AddRecurringDate(DateTime date)
        {
            this.RecurrenceDates.Add(date);
        }

        public bool DayOfWeekValid(DayOfWeek dayOfWeek)
        {
            return GetValidDates().Any(w => w.DayOfWeek == dayOfWeek);
        }

        #endregion
    }
}
