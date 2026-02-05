using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeScheduleTask : ICreatedModified, IState
    {
        public List<TimeScheduleTemplateBlockTask> ConnectedTimeScheduleTemplateBlockTasks { get; set; }

        public string RecurrencePatternDescription { get; set; }
        public string RecurrenceStartsOnDescription { get; set; }
        public string RecurrenceEndsOnDescription { get; set; }
        public DailyRecurrenceDatesOutput RecurringDates { get; set; }
        public bool IsFixed
        {
            get
            {
                return !this.AllowOverlapping && this.StartTime.HasValue && this.StopTime.HasValue && (int)this.StopTime.Value.Subtract(this.StartTime.Value).TotalMinutes == this.Length;
            }
        }
        public bool BreakFillsNeed
        {
            get
            {
                return this.DontAssignBreakLeftovers && IsFixed;
            }
        }
    }

    public partial class TimeScheduleTaskType : ICreatedModified, IState
    {

    }

    public partial class TimeScheduleTaskExcludedDate
    {
        public TimeScheduleTaskExcludedDate() { }

        public TimeScheduleTaskExcludedDate(DateTime date)
        {
            this.Date = date;
        }
    }

    public static partial class EntityExtensions
    {
        #region TimeScheduleTask

        public static TimeScheduleTaskDTO ToDTO(this TimeScheduleTask e, bool includeAccounts)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includeAccounts && !e.IsAdded())
                {
                    if (!e.AccountReference.IsLoaded)
                    {
                        e.AccountReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("TimeScheduleTask.cs e.AccountReference");
                    }
                    if (!e.AccountInternal.IsLoaded)
                    {
                        e.AccountInternal.Load();
                        foreach (var accInt in e.AccountInternal)
                        {
                            if (!accInt.AccountReference.IsLoaded)
                            {
                                accInt.AccountReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("TimeScheduleTask.cs accInt.AccountReference");
                            }
                            if (!accInt.Account.AccountDimReference.IsLoaded)
                            {
                                accInt.Account.AccountDimReference.Load();
                                DataProjectLogCollector.LogLoadedEntityInExtension("TimeScheduleTask.cs accInt.Account.AccountDimReference");
                            }
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            TimeScheduleTaskDTO dto = new TimeScheduleTaskDTO()
            {
                TimeScheduleTaskId = e.TimeScheduleTaskId,
                ShiftTypeId = e.ShiftTypeId,
                TimeScheduleTaskTypeId = e.TimeScheduleTaskTypeId,
                Name = e.Name,
                Description = e.Description,
                StartTime = e.StartTime,
                StopTime = e.StopTime,
                Length = e.Length,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                NbrOfOccurrences = e.NbrOfOccurrences,
                RecurrencePattern = e.RecurrencePattern,
                OnlyOneEmployee = e.OnlyOneEmployee,
                AllowOverlapping = e.AllowOverlapping,
                MinSplitLength = e.MinSplitLength,
                NbrOfPersons = e.NbrOfPersons,
                IsStaffingNeedsFrequency = e.IsStaffingNeedsFrequency,
                DontAssignBreakLeftovers = e.DontAssignBreakLeftovers,
                AccountId = e.AccountId,
                AccountName = e.Account?.Name ?? string.Empty,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            // Relations
            if (includeAccounts)
            {
                int position = 2;
                foreach (var accInt in e.AccountInternal.OrderBy(a => a.Account.AccountDim.AccountDimNr))
                {
                    dto.SetAccountId(position, accInt.AccountId);
                    position++;
                }
            }

            dto.RecurrencePatternDescription = e.RecurrencePatternDescription;
            dto.RecurrenceStartsOnDescription = e.RecurrenceStartsOnDescription;
            dto.RecurrenceEndsOnDescription = e.RecurrenceEndsOnDescription;
            dto.RecurringDates = e.RecurringDates;
            if (e.TimeScheduleTaskExcludedDate != null && e.TimeScheduleTaskExcludedDate.Count > 0)
                dto.ExcludedDates = e.TimeScheduleTaskExcludedDate.Select(d => d.Date).ToList();

            return dto;
        }

        public static IEnumerable<TimeScheduleTaskDTO> ToDTOs(this IEnumerable<TimeScheduleTask> l, bool includeAccounts)
        {
            var dtos = new List<TimeScheduleTaskDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeAccounts));
                }
            }
            return dtos;
        }

        public static TimeScheduleTaskGridDTO ToGridDTO(this TimeScheduleTask e)
        {
            if (e == null)
                return null;

            TimeScheduleTaskGridDTO dto = new TimeScheduleTaskGridDTO()
            {
                TimeScheduleTaskId = e.TimeScheduleTaskId,
                Name = e.Name,
                Description = e.Description,
                StartTime = e.StartTime,
                StopTime = e.StopTime,
                Length = e.Length,
                OnlyOneEmployee = e.OnlyOneEmployee,
                DontAssignBreakLeftovers = e.DontAssignBreakLeftovers,
                AllowOverlapping = e.AllowOverlapping,
                NbrOfPersons = e.NbrOfPersons,
                IsStaffingNeedsFrequency = e.IsStaffingNeedsFrequency,
                State = (SoeEntityState)e.State
            };

            dto.RecurrencePatternDescription = e.RecurrencePatternDescription;
            dto.RecurrenceStartsOnDescription = e.RecurrenceStartsOnDescription;
            dto.RecurrenceEndsOnDescription = e.RecurrenceEndsOnDescription;
            if (e.Account != null)
                dto.AccountName = e.Account.Name;
            if (e.ShiftType != null)
            {
                dto.ShiftTypeId = e.ShiftTypeId;
                dto.ShiftTypeName = e.ShiftType.Name;
            }
            if (e.TimeScheduleTaskType != null)
            {
                dto.TypeId = e.TimeScheduleTaskTypeId;
                dto.TypeName = e.TimeScheduleTaskType.Name;
            }

            return dto;
        }

        public static IEnumerable<TimeScheduleTaskGridDTO> ToGridDTOs(this IEnumerable<TimeScheduleTask> l)
        {
            var dtos = new List<TimeScheduleTaskGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static bool IsBaseNeed(this TimeScheduleTask e, DayOfWeek givenDayOfWeek)
        {
            return DailyRecurrencePatternDTO.IsBaseNeed(e.RecurrencePattern, givenDayOfWeek);
        }

        public static bool IsSpecificNeed(this TimeScheduleTask e)
        {
            return (e.StartDate == e.StopDate || e.NbrOfOccurrences == 1) || DailyRecurrencePatternDTO.IsAdditionalNeed(e.RecurrencePattern);
        }

        public static bool HasNoRecurrencePattern(this TimeScheduleTask e)
        {
            return DailyRecurrencePatternDTO.HasNoRecurrencePattern(e.RecurrencePattern);
        }

        public static StaffingNeedsTaskDTO CreateStaffingNeedsTask(this TimeScheduleTask e, DateTime date, DateTime? startTime, int lengthMinutes, bool isFixed, bool loadShiftType)
        {
            if (!startTime.HasValue)
                startTime = CalendarUtility.DATETIME_DEFAULT;

            if (loadShiftType && e.ShiftTypeId.HasValue && !e.ShiftTypeReference.IsLoaded)
            {
                e.ShiftTypeReference.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("TimeScheduleTask.cs e.ShiftTypeReference");
            }

            return new StaffingNeedsTaskDTO()
            {
                Type = SoeStaffingNeedsTaskType.Task,
                Id = e.TimeScheduleTaskId,
                Name = e.Name,
                Description = e.Description,
                StartTime = CalendarUtility.GetDateTime(date, startTime.Value),
                StopTime = CalendarUtility.GetDateTime(date, startTime.Value.AddMinutes(lengthMinutes)),
                Length = lengthMinutes,
                IsFixed = isFixed,
                RecurrencePattern = e.RecurrencePattern,
                ShiftTypeId = e.ShiftTypeId,
                ShiftTypeName = e.ShiftType?.Name ?? string.Empty,
                Color = e.ShiftType?.Color ?? string.Empty,
                AccountId = e.AccountId
            };
        }

        #endregion

        #region TimeScheduleTaskType

        public static TimeScheduleTaskTypeDTO ToDTO(this TimeScheduleTaskType e)
        {
            if (e == null)
                return null;

            return new TimeScheduleTaskTypeDTO()
            {
                TimeScheduleTaskTypeId = e.TimeScheduleTaskTypeId,
                Name = e.Name,
                Description = e.Description,
                AccountId = e.AccountId,
                AccountName = e.Account?.Name ?? string.Empty,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<TimeScheduleTaskTypeDTO> ToDTOs(this IEnumerable<TimeScheduleTaskType> l)
        {
            var dtos = new List<TimeScheduleTaskTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static TimeScheduleTaskTypeGridDTO ToGridDTO(this TimeScheduleTaskType e)
        {
            if (e == null)
                return null;

            return new TimeScheduleTaskTypeGridDTO()
            {
                TimeScheduleTaskTypeId = e.TimeScheduleTaskTypeId,
                Name = e.Name,
                Description = e.Description,
                AccountId = e.AccountId,
                AccountName = e.Account?.Name ?? string.Empty,
            };
        }

        public static IEnumerable<TimeScheduleTaskTypeGridDTO> ToGridDTOs(this IEnumerable<TimeScheduleTaskType> l)
        {
            var dtos = new List<TimeScheduleTaskTypeGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        #endregion
    }
}
