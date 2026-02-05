using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Data
{
    public partial class TimeBlockDate
    {
        private const char ROWDELIMETER = ';';
        private const char KEYVALUEDELIMETER = ':';

        public bool IsNew { get; set; }
        public bool HasSetToCompletedWithErrorsStatus { get; set; }
        public bool? CalculatedAutogenTimeblocks { get; set; }
        public bool IsLocked 
        {
            get { return this.Status == (int)SoeTimeBlockDateStatus.Locked; } 
        }

        public void SetDiscardedBreakEvaluation(bool value, int? key = null, bool forceDetails = false)
        {
            Dictionary<int, bool> details = GetDiscardedBreakEvaluationDetails(key);
            if (key.HasValue && (!details.IsNullOrEmpty() || forceDetails))
            {
                if (details.ContainsKey(key.Value))
                    details[key.Value] = value;
                else
                    details.Add(key.Value, value);

                StringBuilder sb = new StringBuilder();
                foreach (var detail in details)
                {
                    if (sb.Length > 0)
                        sb.Append(ROWDELIMETER);
                    sb.Append($"{detail.Key}{KEYVALUEDELIMETER}{detail.Value}");
                }
                this.DiscardedBreakEvaluationDetails = sb.ToString();
                this.DiscardedBreakEvaluation = details.Any(i => i.Value);
            }
            else
            {
                this.DiscardedBreakEvaluation = value;
            }
        }
        public bool DoDiscardedBreakEvaluation(int? key = null)
        {
            Dictionary<int, bool> details = GetDiscardedBreakEvaluationDetails(key);
            if (!details.IsNullOrEmpty())
                return details.ContainsKey(key.Value) && details[key.Value];
            else
                return this.DiscardedBreakEvaluation;
        }
        private Dictionary<int, bool> GetDiscardedBreakEvaluationDetails(int? key)
        {
            Dictionary<int, bool> details = new Dictionary<int, bool>();

            if (key.HasValue && !string.IsNullOrEmpty(this.DiscardedBreakEvaluationDetails))
            {
                string[] parts = this.DiscardedBreakEvaluationDetails.Split(ROWDELIMETER);
                foreach (var part in parts)
                {
                    string[] keyValue = part?.Split(KEYVALUEDELIMETER);
                    if (keyValue?.Length == 2 && int.TryParse(keyValue[0], out int k) && !details.ContainsKey(k))
                        details.Add(k, StringUtility.GetBool(keyValue[1]));
                }
            }

            return details;
        }
    }

    public static partial class EntityExtensions
    {
        #region TimeBlockDate

        public static TimeBlockDateDTO ToDTO(this TimeBlockDate e, int accountId = 0)
        {
            if (e == null)
                return null;

            return new TimeBlockDateDTO
            {
                TimeBlockDateId = e.TimeBlockDateId,
                EmployeeId = e.EmployeeId,
                Date = e.Date,
                Status = (SoeTimeBlockDateStatus)e.Status,
                StampingStatus = (TermGroup_TimeBlockDateStampingStatus)e.StampingStatus,
                DiscardedBreakEvaluation = e.DoDiscardedBreakEvaluation(accountId)
            };
        }

        public static IEnumerable<TimeBlockDateDTO> ToDTOs(this IEnumerable<TimeBlockDate> l)
        {
            var dtos = new List<TimeBlockDateDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static Dictionary<int, DateTime> GetDict(this IEnumerable<TimeBlockDate> l)
        {
            var dict = new Dictionary<int, DateTime>();
            foreach (var e in l)
            {
                if (!dict.ContainsKey(e.TimeBlockDateId))
                    dict.Add(e.TimeBlockDateId, e.Date);
            }
            return dict;
        }

        public static List<TimeBlockDate> Filter(this List<TimeBlockDate> l, int employeeId, List<DateTime> dates)
        {
            return l.Where(i => i.EmployeeId == employeeId && dates.Contains(i.Date)).ToList();
        }

        public static List<TimeBlockDate> Filter(this List<TimeBlockDate> l, int employeeId, DateTime dateFrom, DateTime dateTo)
        {
            return l.Where(i => i.EmployeeId == employeeId && CalendarUtility.GetDates(dateFrom, dateTo).Contains(i.Date)).ToList();
        }

        public static List<TimeBlockDate> Filter(this List<TimeBlockDate> l, DateTime dateFrom, DateTime dateTo)
        {
            return l?.Where(tbd => CalendarUtility.IsDateInRange(tbd.Date, dateFrom, dateTo)).ToList() ?? new List<TimeBlockDate>();
        }

        public static List<TimeBlockDate> Filter(this List<TimeBlockDate> l, (DateTime StartDate, DateTime StopDate)? week = null)
        {
            return week.HasValue ? l?.Filter(week.Value.StartDate, week.Value.StopDate) : l;
        }

        public static List<TimeBlockDate> Exclude(this List<TimeBlockDate> l, List<DateTime> exclude)
        {
            if (exclude == null)
                return l;
            return l?.Where(e => !exclude.Contains(e.Date)).Distinct().ToList() ?? new List<TimeBlockDate>();
        }

        public static List<int> GetTimeBlockDateIds(this List<TimeBlockDate> l)
        {
            return l?.Where(i => i.TimeBlockDateId > 0).Select(i => i.TimeBlockDateId).Distinct().ToList() ?? new List<int>();
        }

        public static DateTime GetFirstDate(this List<TimeBlockDate> l)
        {
            return l?.OrderBy(i => i.Date).FirstOrDefault()?.Date ?? CalendarUtility.DATETIME_DEFAULT;
        }

        public static DateTime GetLastDate(this List<TimeBlockDate> l)
        {
            return l?.OrderByDescending(i => i.Date).FirstOrDefault()?.Date ?? CalendarUtility.DATETIME_DEFAULT;
        }

        public static List<DateTime> GetDates(this List<TimeBlockDate> l)
        {
            return l?.Select(e => e.Date).ToList() ?? new List<DateTime>();
        }

        public static bool HasUnhandledShiftChanges(this List<TimeBlockDate> l, out List<TimeBlockDate> unhandled, (DateTime StartDate, DateTime StopDate)? week = null)
        {
            unhandled = l?.Filter(week).Where(e => e.HasUnhandledShiftChanges).ToList();
            return !unhandled.IsNullOrEmpty();
        }

        public static bool HasUnhandledExtraShiftChanges(this List<TimeBlockDate> l, out List<TimeBlockDate> unhandled, (DateTime StartDate, DateTime StopDate)? week = null)
        {
            unhandled = l?.Filter(week).Where(e => e.HasUnhandledExtraShiftChanges).ToList();
            return !unhandled.IsNullOrEmpty();
        }

        public static List<TimeBlockDateDTO> GetUniqueById(this List<TimeBlockDate> l)
        {
            List<TimeBlockDateDTO> unique = new List<TimeBlockDateDTO>();
            foreach (var group in l.GroupBy(i => i.TimeBlockDateId))
                unique.Add(group.First().ToDTO());
            return unique;
        }

        #endregion

        #region TimeBlockDateDetail

        public static decimal? GetTimeBlockDateDetailRatioFromPayrollType(this TimeBlockDate e, int sysPayrollTypeLevel3)
        {
            if (e == null || sysPayrollTypeLevel3 == 0)
                return null;

            try
            {
                if (!e.TimeBlockDateDetail.IsLoaded)
                {
                    e.TimeBlockDateDetail.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("TimeBlockDate.cs e.TimeBlockDateDetail");
                }
                return e.TimeBlockDateDetail?.FirstOrDefault(t => t.State == (int)SoeEntityState.Active && t.OutcomeId == sysPayrollTypeLevel3)?.Ratio;
            }
            catch
            {
                return null;
            }
        }

        public static List<TimeBlockDateDetail> GetTimeBlockDateDetailsToDelete(this TimeBlockDate e, SoeTimeBlockDateDetailType type, int? outcomeId)
        {
            return e?.TimeBlockDateDetail?.Where(d => d.Type == (int)type && d.State == (int)SoeEntityState.Active && (!outcomeId.HasValue || outcomeId.Value == d.OutcomeId)).ToList() ?? new List<TimeBlockDateDetail>();
        }

        public static bool ContainsOutcomeWithRatio(this TimeBlockDate e, SoeTimeBlockDateDetailType type, int outcomeId)
        {
            return e?.TimeBlockDateDetail?.Any(d => d.Type == (int)type && d.State == (int)SoeEntityState.Active && d.OutcomeId == outcomeId && d.Ratio.HasValue) ?? false;
        }

        #endregion
    }
}
