using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Data
{
    public partial class TimeStampEntry : ICreatedModified, IState
    {
        public string TypeName { get; set; }
        public int OldStatus { get; set; }
        public bool Invalid { get; set; }
        public string DeviationCauseName { get; set; }
        public string AccountName { get; set; }
        public string TerminalName { get; set; }
    }

    public static partial class EntityExtensions
    {
        #region TimeStampEntry

        public static TimeStampEntryDTO ToDTO(this TimeStampEntry e)
        {
            if (e == null)
                return null;

            var dto = new TimeStampEntryDTO()
            {
                TimeStampEntryId = e.TimeStampEntryId,
                TimeTerminalId = e.TimeTerminalId,
                ActorCompanyId = e.ActorCompanyId,
                EmployeeId = e.EmployeeId,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                AccountId = e.AccountId,
                TimeTerminalAccountId = e.TimeTerminalAccountId,
                TimeScheduleTemplatePeriodId = e.TimeScheduleTemplatePeriodId,
                TimeBlockDateId = e.TimeBlockDateId,
                EmployeeChildId = e.EmployeeChildId,
                TimeScheduleTypeId = e.TimeScheduleTypeId,
                ShiftTypeId = e.ShiftTypeId,
                IsBreak = e.IsBreak,
                IsPaidBreak = e.IsPaidBreak,
                IsDistanceWork = e.IsDistanceWork,
                OriginType = (TermGroup_TimeStampEntryOriginType)e.OriginType,
                Type = (TimeStampEntryType)e.Type,
                TerminalStampData = e.TerminalStampData,
                Note = e.Note,
                Time = e.Time,
                OriginalTime = e.OriginalTime,
                ManuallyAdjusted = e.ManuallyAdjusted,
                EmployeeManuallyAdjusted = e.EmployeeManuallyAdjusted,
                Status = (TermGroup_TimeStampEntryStatus)e.Status,
                EmployeeNr = e.Employee?.EmployeeNr ?? string.Empty,
                EmployeeName = e.Employee?.Name ?? string.Empty,
                TimeDeviationCauseName = e.TimeDeviationCause?.Name ?? string.Empty,
                AccountNr = e.Account?.AccountNr ?? string.Empty,
                AccountName = e.Account?.Name ?? string.Empty,
                TimeBlockDateDate = e.TimeBlockDate?.Date,
                AdjustedTimeBlockDateDate = e.TimeBlockDate?.Date,
                TimeTerminalName = e.TimeTerminal?.Name ?? string.Empty,
                Date = e.Time,
                TypeName = e.TypeName,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (e.TimeStampEntryExtended != null)
                dto.Extended = e.TimeStampEntryExtended.Where(t => t.State == (int)SoeEntityState.Active).ToDTOs();

            return dto;
        }

        public static string GetInfo(this List<TimeStampEntry> l)
        {
            if (l.IsNullOrEmpty())
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            foreach (TimeStampEntry e in l)
            {
                sb.Append($"TimeStampEntryId={e.TimeStampEntryId},");
                sb.Append($"Time={e.Time},");
                sb.Append($"Type={e.Type},");
                sb.Append($"ManuallyAdjusted={e.ManuallyAdjusted},");
                sb.Append($"Status={e.Status},");
                sb.Append($"State={e.State},");
                sb.Append($"AutoStampOut={e.AutoStampOut},");
                sb.Append($"AccountId={e.AccountId},");
                sb.Append($"ShiftTypeId={e.ShiftTypeId},");
                sb.Append($"TimeDeviationCauseId={e.TimeDeviationCauseId},");
                sb.Append($"TimeScheduleTypeId={e.TimeScheduleTypeId},");
                sb.Append($"TimeBlockDateId={e.TimeBlockDateId}");
                sb.Append(";");
            }
            sb.Append("]");
            return sb.ToString();
        }

        public static TimeStampEntry GetPrevTimeStamp(this List<TimeStampEntry> l, TimeStampEntry entry, TimeStampEntryType type)
        {
            if (entry == null)
                return null;
            return l?.Where(i => !i.Invalid).OrderByDescending(i => i.Time).FirstOrDefault(i => i.Time < entry.Time && i.Type == (int)type);
        }

        public static int GetPrecenseMinutesAccordingToTimeStamps(this List<TimeStampEntry> l)
        {
            if (l.IsNullOrEmpty())
                return 0;

            int minutes = 0;

            TimeStampEntry prevIn = null;
            foreach (TimeStampEntry e in l.OrderBy(o => o.Time))
            {
                if (e.Type == (int)TimeStampEntryType.In)
                {
                    prevIn = e;
                }
                else if (e.Type == (int)TimeStampEntryType.Out && prevIn != null)
                {
                    minutes += (int)(e.Time - prevIn.Time).TotalMinutes;
                    prevIn = null;
                }
            }

            if (minutes == 0 && l.Count == 1)
                minutes = Convert.ToInt16((DateTime.Now - l[0].Time).TotalMinutes);

            return minutes;
        }

        public static IEnumerable<TimeStampEntryDTO> ToDTOs(this IEnumerable<TimeStampEntry> l)
        {
            var dtos = new List<TimeStampEntryDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static void SetTimeOnTimeBlocks(this TimeStampEntry e, DateTime time, DateTime newTime)
        {
            if (e.TimeBlock.IsNullOrEmpty())
                return;

            time = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, time);

            foreach (TimeBlock timeBlock in e.TimeBlock)
            {
                if (timeBlock.StartTime == time && time <= timeBlock.StopTime)
                    timeBlock.StartTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, newTime);
                if (timeBlock.StopTime == time && time >= timeBlock.StartTime)
                    timeBlock.StopTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, newTime);
            }

            e.TimeBlock.Where(w => w.StartTime >= w.StopTime).ToList().ForEach(f => f.State = (int)SoeEntityState.Deleted);
        }
        public static void SetTimeDeviationCauseAndTimeCodeOnTimeBlocks(this TimeStampEntry e, int timeDeviationCauseId, DateTime startInterval, DateTime stopInterval, TimeCode timeCode, bool startsWith = true)
        {
            if (e.TimeBlock.IsNullOrEmpty())
                return;

            foreach (TimeBlock timeBlock in e.TimeBlock.Where(w => w.State != (int)SoeEntityState.Deleted))
            {
                if ((startsWith && timeBlock.ActualStartTime >= startInterval && timeBlock.ActualStopTime <= stopInterval) || (!startsWith && timeBlock.ActualStopTime <= stopInterval && timeBlock.ActualStartTime >= startInterval))
                {
                    timeBlock.TimeDeviationCauseStartId = timeDeviationCauseId;
                    timeBlock.TimeDeviationCauseStopId = timeDeviationCauseId;
                    timeBlock.TimeCode.Clear();
                    timeBlock.TimeCode.Add(timeCode);
                }
            }
        }

        public static void SetTimeDeviationCauseAndTimeCodeOnTimeBlocks(this TimeStampEntry e, int timeDeviationCauseId, DateTime time, TimeCode timeCode)
        {
            if (e.TimeBlock.IsNullOrEmpty())
                return;

            int addDays = time.Date > e.TimeBlockDate.Date ? 1 : 0;
            time = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT.AddDays(addDays), time);

            foreach (TimeBlock timeBlock in e.TimeBlock.Where(w => w.State != (int)SoeEntityState.Deleted))
            {
                if (e.Type == (int)TimeStampEntryType.Out && timeBlock.StartTime == time)
                {
                    timeBlock.TimeDeviationCauseStartId = timeDeviationCauseId;
                    timeBlock.TimeDeviationCauseStopId = timeDeviationCauseId;
                    timeBlock.TimeCode.Clear();
                    timeBlock.TimeCode.Add(timeCode);
                }
                if (e.Type == (int)TimeStampEntryType.In && timeBlock.StopTime == time)
                {
                    timeBlock.TimeDeviationCauseStopId = timeDeviationCauseId;
                    timeBlock.TimeDeviationCauseStartId = timeDeviationCauseId;
                    timeBlock.TimeCode.Clear();
                    timeBlock.TimeCode.Add(timeCode);
                }
            }
        }

        public static bool HasAnyTimeStamp(this List<TimeStampEntry> l, DateTime from, DateTime to, TimeStampEntryType type)
        {
            foreach (var e in l)
            {
                if (e.Type != (int)type)
                    continue;

                DateTime time = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, e.Time);
                if (time >= from && time <= to)
                    return true;
            }

            return false;
        }

        public static bool IsPrevTimeStampOnOrBefore(this List<TimeStampEntry> l, TimeStampEntry entry, TimeStampEntryType type, DateTime time, out TimeStampEntry prevTimeStamp)
        {
            prevTimeStamp = l.GetPrevTimeStamp(entry, type);
            return prevTimeStamp != null && prevTimeStamp.Time <= time;
        }

        #endregion

        #region TimeStampEntryExtended

        public static TimeStampEntryExtendedDTO ToDTO(this TimeStampEntryExtended e)
        {
            if (e == null)
                return null;

            return new TimeStampEntryExtendedDTO()
            {
                TimeStampEntryExtendedId = e.TimeStampEntryExtendedId,
                TimeStampEntryId = e.TimeStampEntryId,
                TimeScheduleTypeId = e.TimeScheduleTypeId,
                TimeCodeId = e.TimeCodeId,
                AccountId = e.AccountId,
                Quantity = e.Quantity,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }

        public static List<TimeStampEntryExtendedDTO> ToDTOs(this IEnumerable<TimeStampEntryExtended> l)
        {
            var dtos = new List<TimeStampEntryExtendedDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region TimeStampEntryExtendedDTO

        public static TimeStampEntryExtendedDetailsDTO ToExtendedDetailsDTO(this TimeStampEntryExtended e)
        {
            if (e == null)
                return null;

            return new TimeStampEntryExtendedDetailsDTO()
            {
                TimeScheduleTypeId = e.TimeScheduleTypeId,
                AccountId = e.AccountId,
                TimeCodeId = e.TimeCodeId,
            };
        }

        public static List<TimeStampEntryExtendedDetailsDTO> ToExtendedDetailsDTOs(this IEnumerable<TimeStampEntryExtended> l)
        {
            var dtos = new List<TimeStampEntryExtendedDetailsDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToExtendedDetailsDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region TimeStampRounding

        public static bool HasRounding(this TimeStampRounding rounding)
        {
            if (rounding == null)
                return false;
            if (rounding.RoundInNeg == 0 && rounding.RoundOutNeg == 0 && rounding.RoundInPos == 0 && rounding.RoundOutPos == 0)
                return false;
            return true;
        }

        #endregion
    }
}
