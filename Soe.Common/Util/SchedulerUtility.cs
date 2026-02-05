using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Common.Util
{
    public class SchedulerUtility
    {
        #region Constants

        public const char CRONTAB_ALL_SELECTED = '*';
        public const char CRONTAB_ITEMTYPE_SEPARATOR = ' ';
        public const char CRONTAB_ITEM_SEPARATOR = ',';
        public const char CRONTAB_RANGE_SEPARATOR = '-';
        public const string CRONTAB_MINUTES = "minutes";
        public const int CRONTAB_MINUTES_LOWER = 0;
        public const int CRONTAB_MINUTES_UPPER = 59;
        public const string CRONTAB_HOURS = "hours";
        public const int CRONTAB_HOURS_LOWER = 0;
        public const int CRONTAB_HOURS_UPPER = 23;
        public const string CRONTAB_DAYS = "days";
        public const int CRONTAB_DAYS_LOWER = 1;
        public const int CRONTAB_DAYS_UPPER = 31;
        public const string CRONTAB_MONTHS = "months";
        public const int CRONTAB_MONTHS_LOWER = 1;
        public const int CRONTAB_MONTHS_UPPER = 12;
        public const string CRONTAB_WEEKDAYS = "weekdays";
        public const int CRONTAB_WEEKDAYS_LOWER = 1;
        public const int CRONTAB_WEEKDAYS_UPPER = 7;

        #endregion

        #region Get expression from selected items

        public static string GetCrontabExpression(List<int> minutes, List<int> hours, List<int> days, List<int> months, List<int> weekdays)
        {
            string expression = String.Empty;
            expression += GetCrontabMinutesExpression(minutes);
            expression += GetCrontabHoursExpression(hours);
            expression += GetCrontabDaysExpression(days);
            expression += GetCrontabMonthsExpression(months);
            expression += GetCrontabWeekdaysExpression(weekdays);

            return expression;
        }

        public static string GetCrontabMinutesExpression(List<int> minutes)
        {
            return GetCrontabIntegerExpression(minutes, CRONTAB_MINUTES_LOWER, CRONTAB_MINUTES_UPPER);
        }

        public static string GetCrontabHoursExpression(List<int> hours)
        {
            return GetCrontabIntegerExpression(hours, CRONTAB_HOURS_LOWER, CRONTAB_HOURS_UPPER);
        }

        public static string GetCrontabDaysExpression(List<int> days)
        {
            return GetCrontabIntegerExpression(days, CRONTAB_DAYS_LOWER, CRONTAB_DAYS_UPPER);
        }

        public static string GetCrontabMonthsExpression(List<int> months)
        {
            return GetCrontabIntegerExpression(months, CRONTAB_MONTHS_LOWER, CRONTAB_MONTHS_UPPER);
        }

        public static string GetCrontabWeekdaysExpression(List<int> weekdays)
        {
            return GetCrontabIntegerExpression(weekdays, CRONTAB_WEEKDAYS_LOWER, CRONTAB_WEEKDAYS_UPPER);
        }

        private static string GetCrontabIntegerExpression(List<int> ints, int lowerBound, int upperBound)
        {
            // All or no items selected
            if (ints.Count == upperBound - lowerBound + 1 || ints.Count == 0)
                return CRONTAB_ALL_SELECTED.ToString();

            string expression = String.Empty;
            int? prevInt = null;

            ints.Sort();

            foreach (int i in ints)
            {
                // Range
                if (prevInt.HasValue && prevInt + 1 == i)
                {
                    if (!expression.EndsWith(CRONTAB_RANGE_SEPARATOR.ToString()))
                        expression += CRONTAB_RANGE_SEPARATOR;  // Begin range
                }
                else if (prevInt.HasValue && expression.EndsWith(CRONTAB_RANGE_SEPARATOR.ToString()))
                {
                    expression += prevInt.Value;  // End range
                }

                // Single
                if (!expression.EndsWith(CRONTAB_RANGE_SEPARATOR.ToString()))
                {
                    if (expression.Length > 0)
                        expression += CRONTAB_ITEM_SEPARATOR;
                    expression += i;
                }

                prevInt = i;
            }

            // End range
            if (prevInt.HasValue && expression.EndsWith(CRONTAB_RANGE_SEPARATOR.ToString()))
                expression += prevInt.Value;

            return expression;
        }

        #endregion

        #region Parse expression into items

        public static Dictionary<string, List<int>> ParseCrontabExpression(string expression)
        {
            Dictionary<string, List<int>> dict = new Dictionary<string, List<int>>();

            if (String.IsNullOrEmpty(expression))
                expression = String.Format("{0} {1} {2} {3} {4}", CRONTAB_ALL_SELECTED, CRONTAB_ALL_SELECTED, CRONTAB_ALL_SELECTED, CRONTAB_ALL_SELECTED, CRONTAB_ALL_SELECTED);

            try
            {

                string[] types = expression.Split(CRONTAB_ITEMTYPE_SEPARATOR);
                if (types.Count() != 5)
                    throw new ArgumentException(String.Format("Felaktigt uttryck, innehåller {0} delar, måste vara 5", types.Count()));

                dict.Add(CRONTAB_MINUTES, ParseCrontabSingleExpression(types[0], CRONTAB_MINUTES_LOWER, CRONTAB_MINUTES_UPPER));
                dict.Add(CRONTAB_HOURS, ParseCrontabSingleExpression(types[1], CRONTAB_HOURS_LOWER, CRONTAB_HOURS_UPPER));
                dict.Add(CRONTAB_DAYS, ParseCrontabSingleExpression(types[2], CRONTAB_DAYS_LOWER, CRONTAB_DAYS_UPPER));
                dict.Add(CRONTAB_MONTHS, ParseCrontabSingleExpression(types[3], CRONTAB_MONTHS_LOWER, CRONTAB_MONTHS_UPPER));
                dict.Add(CRONTAB_WEEKDAYS, ParseCrontabSingleExpression(types[4], CRONTAB_WEEKDAYS_LOWER, CRONTAB_WEEKDAYS_UPPER));
            }
            catch (ArgumentException aeEx)
            {
                aeEx.ToString(); //prevent compiler warning
                throw;
            }

            return dict;
        }

        private static List<int> ParseCrontabSingleExpression(string expression, int lowerBound, int upperBound)
        {
            List<int> list = new List<int>();

            // None or all
            if (expression.Length == 0 || expression == CRONTAB_ALL_SELECTED.ToString())
                return list;

            try
            {
                string[] items = expression.Split(CRONTAB_ITEM_SEPARATOR);
                string[] range;
                int i;

                foreach (string item in items)
                {
                    i = -1;

                    // Range
                    if (item.Contains(CRONTAB_RANGE_SEPARATOR.ToString()))
                    {
                        range = item.Split(CRONTAB_RANGE_SEPARATOR);
                        if (range.Count() == 2)
                        {
                            // Get first part of range
                            int lowerBoundRange = -1;
                            Int32.TryParse(range[0], out lowerBoundRange);
                            if (lowerBoundRange == -1)
                                throw new ArgumentException(String.Format("Felaktig början på intervall ({0})", range[0]));

                            // Get last part of range
                            int upperBoundRange = -1;
                            Int32.TryParse(range[1], out upperBoundRange);
                            if (upperBoundRange == -1)
                                throw new ArgumentException(String.Format("Felaktigt slut på intervall ({0})", range[1]));

                            // Check that first part has a lower number than last part
                            // Otherwise switch places between first and last part
                            if (lowerBoundRange > upperBoundRange)
                            {
                                int tmp = lowerBoundRange;
                                lowerBoundRange = upperBoundRange;
                                upperBoundRange = tmp;
                            }

                            // Add all numbers in range
                            if (lowerBoundRange < lowerBound)
                                lowerBoundRange = lowerBound;
                            if (upperBoundRange > upperBound)
                                upperBoundRange = upperBound;
                            for (int j = lowerBoundRange; j <= upperBoundRange; j++)
                            {
                                if (!list.Contains(j))
                                    list.Add(j);
                            }
                        }
                        else
                            throw new ArgumentException(String.Format("Felaktigt intervall ({0})", item));
                    }
                    else
                    {
                        // Single integer
                        Int32.TryParse(item, out i);
                        if (i != -1)
                        {
                            if (i >= lowerBound && i <= upperBound && !list.Contains(i))
                                list.Add(i);
                        }
                        else
                            throw new ArgumentException(String.Format("Felaktigt nummer ({0})", item));
                    }
                }
            }
            catch (ArgumentException aeEx)
            {
                aeEx.ToString(); //prevent compiler warning
                throw;
            }

            list.Sort();

            return list;
        }

        #endregion

        public static DateTime GetNextExecutionTime(string expression, DateTime? currentDate = null)
        {
            if (!currentDate.HasValue)
                currentDate = DateTime.Now;

            // Parse crontab expression
            Dictionary<string, List<int>> dict = ParseCrontabExpression(expression);
            List<int> minutes = dict[CRONTAB_MINUTES];
            List<int> hours = dict[CRONTAB_HOURS];
            List<int> days = dict[CRONTAB_DAYS];
            List<int> months = dict[CRONTAB_MONTHS];
            List<int> weekdays = dict[CRONTAB_WEEKDAYS];

            // Add one minute until a matching time is reached
            bool match = false;
            while (!match)
            {
                currentDate = currentDate.Value.AddMinutes(1);
                match = ((minutes.Contains(currentDate.Value.Minute) || !minutes.Any()) &&
                         (hours.Contains(currentDate.Value.Hour) || !hours.Any()) &&
                         (days.Contains(currentDate.Value.Day) || !days.Any()) &&
                         (months.Contains(currentDate.Value.Month) || !months.Any()) &&
                         (weekdays.Contains(convertDayOfWeekToWeekday(currentDate.Value.DayOfWeek)) || !weekdays.Any()));
            }

            return currentDate.Value;
        }

        public static DateTime GetLastPreviousValidTime(string expression, DateTime? currentDate = null)
        {
            if (!currentDate.HasValue)
                currentDate = DateTime.Now;
            // Parse crontab expression
            Dictionary<string, List<int>> dict = ParseCrontabExpression(expression);
            List<int> minutes = dict[CRONTAB_MINUTES];
            List<int> hours = dict[CRONTAB_HOURS];
            List<int> days = dict[CRONTAB_DAYS];
            List<int> months = dict[CRONTAB_MONTHS];
            List<int> weekdays = dict[CRONTAB_WEEKDAYS];
            // Subtract one minute until a matching time is reached
            bool match = false;
            while (!match)
            {
                currentDate = currentDate.Value.AddMinutes(-1);
                match = ((minutes.Contains(currentDate.Value.Minute) || !minutes.Any()) &&
                         (hours.Contains(currentDate.Value.Hour) || !hours.Any()) &&
                         (days.Contains(currentDate.Value.Day) || !days.Any()) &&
                         (months.Contains(currentDate.Value.Month) || !months.Any()) &&
                         (weekdays.Contains(convertDayOfWeekToWeekday(currentDate.Value.DayOfWeek)) || !weekdays.Any()));
            }

            currentDate = CalendarUtility.ClearSeconds(currentDate.Value);

            return currentDate.Value;
        }

        #region Help-methods

        private static int convertDayOfWeekToWeekday(DayOfWeek dayOfWeek)
        {
            if (dayOfWeek == DayOfWeek.Sunday)
                return 7;
            else
                return (int)dayOfWeek;
        }

        private static DayOfWeek convertWeekdayToDayOfWeek(int weekday)
        {
            if (weekday == 7)
                return DayOfWeek.Sunday;
            else
                return (DayOfWeek)weekday;
        }

        #endregion
    }
}
