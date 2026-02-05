using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class IncomingDeliveryHead : ICreatedModified, IState
    {
        public string RecurrencePatternDescription { get; set; }
        public string RecurrenceStartsOnDescription { get; set; }
        public string RecurrenceEndsOnDescription { get; set; }
        public DailyRecurrenceDatesOutput RecurringDates { get; set; }
    }

    public partial class IncomingDeliveryHeadExcludedDate
    {
        public IncomingDeliveryHeadExcludedDate()
        { }
        public IncomingDeliveryHeadExcludedDate(DateTime date)
        {
            this.Date = date;
        }
    }

    public partial class IncomingDeliveryRow : ICreatedModified, IState
    {
        public List<TimeScheduleTemplateBlockTask> ConnectedTimeScheduleTemplateBlockTasks { get; set; }

        public bool IsFixed
        {
            get
            {
                return !this.AllowOverlapping && this.StartTime.HasValue && this.StopTime.HasValue && (int)this.StopTime.Value.Subtract(this.StartTime.Value).TotalMinutes == this.Length;
            }
        }
    }

    public static partial class EntityExtensions
    {
        #region IncomingDelivery

        public static IncomingDeliveryHeadDTO ToDTO(this IncomingDeliveryHead e, bool includeRows, bool includeAccounts)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && includeRows && !e.IncomingDeliveryRow.IsLoaded)
                {
                    e.IncomingDeliveryRow.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("IncomingDelivery.cs e.IncomingDeliveryRow");
                }
                if (!e.IsAdded() && includeAccounts && !e.AccountReference.IsLoaded)
                {
                    e.AccountReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("IncomingDelivery.cs e.AccountReference");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            IncomingDeliveryHeadDTO dto = new IncomingDeliveryHeadDTO()
            {
                IncomingDeliveryHeadId = e.IncomingDeliveryHeadId,
                Name = e.Name,
                Description = e.Description,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                AccountId = e.AccountId,
                AccountName = e.Account?.Name ?? string.Empty,
                NbrOfOccurrences = e.NbrOfOccurrences,
                RecurrencePattern = e.RecurrencePattern,
                RecurrencePatternDescription = e.RecurrencePatternDescription,
                RecurrenceStartsOnDescription = e.RecurrenceStartsOnDescription,
                RecurrenceEndsOnDescription = e.RecurrenceEndsOnDescription,
                RecurringDates = e.RecurringDates,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            // Relations
            if (includeRows)
                dto.Rows = e.IncomingDeliveryRow?.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs(includeAccounts).ToList() ?? new List<IncomingDeliveryRowDTO>();
            if (e.IncomingDeliveryHeadExcludedDate != null && e.IncomingDeliveryHeadExcludedDate.Count > 0)
                dto.ExcludedDates = e.IncomingDeliveryHeadExcludedDate.Select(d => d.Date).ToList();

            return dto;
        }

        public static IEnumerable<IncomingDeliveryHeadDTO> ToDTOs(this IEnumerable<IncomingDeliveryHead> l, bool includeRows, bool includeAccounts)
        {
            var dtos = new List<IncomingDeliveryHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeRows, includeAccounts));
                }
            }
            return dtos;
        }

        public static IncomingDeliveryGridDTO ToGridDTO(this IncomingDeliveryHead e)
        {
            if (e == null)
                return null;

            IncomingDeliveryGridDTO dto = new IncomingDeliveryGridDTO()
            {
                IncomingDeliveryHeadId = e.IncomingDeliveryHeadId,
                Name = e.Name,
                Description = e.Description,
                StartDate = e.StartDate,
                StopDate = e.StopDate,
                NbrOfOccurrences = e.NbrOfOccurrences,
                AccountId = e.AccountId,
                AccountName = e.Account?.Name ?? string.Empty,
            };

            dto.RecurrencePatternDescription = e.RecurrencePatternDescription;
            dto.RecurrenceStartsOnDescription = e.RecurrenceStartsOnDescription;
            dto.RecurrenceEndsOnDescription = e.RecurrenceEndsOnDescription;

            if (e.IncomingDeliveryRow != null)
                dto.HasRows = e.IncomingDeliveryRow.Any(r => r.State == (int)SoeEntityState.Active);

            return dto;
        }

        public static IEnumerable<IncomingDeliveryGridDTO> ToGridDTOs(this IEnumerable<IncomingDeliveryHead> l)
        {
            var dtos = new List<IncomingDeliveryGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO());
                }
            }
            return dtos;
        }

        public static IncomingDeliveryRowDTO ToDTO(this IncomingDeliveryRow e, bool includeAccounts)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includeAccounts && !e.IsAdded())
                {
                    if (!e.AccountInternal.IsLoaded)
                    {
                        e.AccountInternal.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("IncomingDelivery.cs e.AccountInternal");
                    }
                    foreach (var accInt in e.AccountInternal)
                    {
                        if (!accInt.AccountReference.IsLoaded)
                        {
                            accInt.AccountReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("IncomingDelivery.cs accInt.AccountReference");
                        }
                        if (!accInt.Account.AccountDimReference.IsLoaded)
                        {
                            accInt.Account.AccountDimReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("IncomingDelivery.cs accInt.Account.AccountDimReference");
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            IncomingDeliveryRowDTO dto = new IncomingDeliveryRowDTO()
            {
                IncomingDeliveryRowId = e.IncomingDeliveryRowId,
                IncomingDeliveryHeadId = e.IncomingDeliveryHeadId,
                IncomingDeliveryTypeId = e.IncomingDeliveryTypeId,
                ShiftTypeId = e.ShiftTypeId,
                Name = e.Name,
                Description = e.Description,
                NbrOfPackages = e.NbrOfPackages,
                NbrOfPersons = e.NbrOfPersons,
                StartTime = e.StartTime,
                StopTime = e.StopTime,
                Length = e.Length,
                OffsetDays = e.StartTime.HasValue ? e.StartTime.Value.Date.Subtract(CalendarUtility.DATETIME_DEFAULT).Days : 0,
                MinSplitLength = e.MinSplitLength,
                OnlyOneEmployee = e.OnlyOneEmployee,
                AllowOverlapping = e.AllowOverlapping,
                DontAssignBreakLeftovers = e.DontAssignBreakLeftovers,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            if (e.IncomingDeliveryTypeId != 0 && e.IncomingDeliveryTypeReference.IsLoaded)
            {
                dto.IncomingDeliveryTypeDTO = e.IncomingDeliveryType.ToDTO();
                dto.TypeName = e.IncomingDeliveryType.Name;
            }

            if (e.ShiftType != null)
                dto.ShiftTypeName = e.ShiftType.Name;

            if (includeAccounts)
            {
                int i = 2;
                foreach (var accInt in e.AccountInternal.OrderBy(a => a.Account.AccountDim.AccountDimNr))
                {
                    if (i == 2)
                        dto.Account2Id = accInt.AccountId;
                    else if (i == 3)
                        dto.Account3Id = accInt.AccountId;
                    else if (i == 4)
                        dto.Account4Id = accInt.AccountId;
                    else if (i == 5)
                        dto.Account5Id = accInt.AccountId;
                    else if (i == 6)
                        dto.Account6Id = accInt.AccountId;
                    i++;
                }
            }

            if (e.IncomingDeliveryHead != null)
            {
                dto.HeadAccountId = e.IncomingDeliveryHead.AccountId;
                if (e.IncomingDeliveryHead.Account != null)
                    dto.HeadAccountName = e.IncomingDeliveryHead.Account.Name;
            }

            return dto;
        }

        public static IEnumerable<IncomingDeliveryRowDTO> ToDTOs(this IEnumerable<IncomingDeliveryRow> l, bool includeAccounts)
        {
            var dtos = new List<IncomingDeliveryRowDTO>();
            if (l != null)
            {
                foreach (var e in l.OrderBy(i => i.StartTime).ThenBy(i => i.Length))
                {
                    dtos.Add(e.ToDTO(includeAccounts));
                }
            }
            return dtos;
        }

        public static bool IsBaseNeed(this IncomingDeliveryHead e, DayOfWeek givenDayOfWeek)
        {
            return DailyRecurrencePatternDTO.IsBaseNeed(e.RecurrencePattern, givenDayOfWeek);
        }

        public static bool IsSpecificNeed(this IncomingDeliveryHead e)
        {
            return (e.StartDate == e.StopDate || e.NbrOfOccurrences == 1) || DailyRecurrencePatternDTO.IsAdditionalNeed(e.RecurrencePattern);
        }

        public static bool HasNoRecurrencePattern(this IncomingDeliveryHead e)
        {
            return DailyRecurrencePatternDTO.HasNoRecurrencePattern(e.RecurrencePattern);
        }

        #endregion

        #region IncomingDeliveryRow

        public static StaffingNeedsTaskDTO CreateStaffingNeedsTask(this IncomingDeliveryRow deliveryRow, DateTime date, DateTime? startTime, int lengthMinutes, bool isFixed, bool loadShiftType)
        {
            if (!startTime.HasValue)
                startTime = CalendarUtility.DATETIME_DEFAULT;

            if (loadShiftType && deliveryRow.ShiftTypeId.HasValue && !deliveryRow.ShiftTypeReference.IsLoaded)
            {
                deliveryRow.ShiftTypeReference.Load();
                DataProjectLogCollector.LogLoadedEntityInExtension("IncomingDelivery.cs deliveryRow.ShiftTypeReference");
            }

            return new StaffingNeedsTaskDTO()
            {
                Type = SoeStaffingNeedsTaskType.Delivery,
                Id = deliveryRow.IncomingDeliveryRowId,
                Name = String.Format("{0} - {1}", deliveryRow.IncomingDeliveryHead.Name, deliveryRow.Name),
                Description = deliveryRow.Description,
                StartTime = CalendarUtility.GetDateTime(date, startTime.Value),
                StopTime = CalendarUtility.GetDateTime(date, startTime.Value.AddMinutes(lengthMinutes)),
                Length = lengthMinutes,
                IsFixed = isFixed,
                RecurrencePattern = deliveryRow.IncomingDeliveryHead.RecurrencePattern,
                ShiftTypeId = deliveryRow.ShiftTypeId,
                ShiftTypeName = deliveryRow.ShiftType?.Name ?? string.Empty,
                Color = deliveryRow.ShiftType?.Color ?? string.Empty,
            };
        }

        #endregion

        #region IncomingDeliveryType

        public static IncomingDeliveryTypeDTO ToDTO(this IncomingDeliveryType e)
        {
            if (e == null)
                return null;

            return new IncomingDeliveryTypeDTO()
            {
                IncomingDeliveryTypeId = e.IncomingDeliveryTypeId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                Description = e.Description,
                Length = e.Length,
                NbrOfPersons = e.NbrOfPersons,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                AccountId = e.AccountId,
                AccountName = e.Account?.Name ?? string.Empty
            };
        }

        public static IEnumerable<IncomingDeliveryTypeDTO> ToDTOs(this IEnumerable<IncomingDeliveryType> l)
        {
            var dtos = new List<IncomingDeliveryTypeDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static IncomingDeliveryTypeSmallDTO ToSmallDTO(this IncomingDeliveryType e)
        {
            if (e == null)
                return null;

            return new IncomingDeliveryTypeSmallDTO()
            {
                IncomingDeliveryTypeId = e.IncomingDeliveryTypeId,
                Name = e.Name,
                Length = e.Length,
                NbrOfPersons = e.NbrOfPersons,
            };
        }

        public static IEnumerable<IncomingDeliveryTypeSmallDTO> ToSmallDTOs(this IEnumerable<IncomingDeliveryType> l)
        {
            var dtos = new List<IncomingDeliveryTypeSmallDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        public static IncomingDeliveryTypeGridDTO ToGridDTO(this IncomingDeliveryType e)
        {
            if (e == null)
                return null;

            return new IncomingDeliveryTypeGridDTO()
            {
                IncomingDeliveryTypeId = e.IncomingDeliveryTypeId,
                Name = e.Name,
                Description = e.Description,
                NbrOfPersons = e.NbrOfPersons,
                Length = e.Length,
                AccountName = e.Account?.Name ?? string.Empty,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<IncomingDeliveryTypeGridDTO> ToGridDTOs(this IEnumerable<IncomingDeliveryType> l)
        {
            var dtos = new List<IncomingDeliveryTypeGridDTO>();
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
