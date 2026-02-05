using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace SoftOne.Soe.Data
{
    public partial class StaffingNeedsFrequency
    {
        public StaffingNeedsFrequencyIO TempStaffingNeedsFrequencyIO { get; set; }

        private string compareKey { get; set; }
        public string CompareKey
        {
            get
            {
                if (string.IsNullOrEmpty(compareKey))
                    compareKey = $"{this.ActorCompanyId}#{this.AccountId}#{this.ParentAccountId}#{this.TimeFrom}#{this.TimeTo}#{this.ExternalCode}#{this.FrequencyType}#{ParentExternalCode}";

                return compareKey;
            }
        }

        private string compareKeyWithValues { get; set; }

        public string CompareKeyWithValues
        {
            get
            {
                if (string.IsNullOrEmpty(compareKeyWithValues))
                    compareKeyWithValues = CompareKey + $"{Convert.ToInt32(this.Amount)}#{Convert.ToInt32(this.NbrOfCustomers)}#{Convert.ToInt32(this.NbrOfItems)}#{Convert.ToInt32(this.NbrOfMinutes)}#{Convert.ToInt32(this.Cost)}";

                return compareKeyWithValues;
            }
        }
    }

    public partial class StaffingNeedsRule : ICreatedModified, IState
    {
        public StaffingNeedsFrequencyType FrequencyType
        {
            get
            {
                StaffingNeedsFrequencyType freqType = StaffingNeedsFrequencyType.Unknown;
                switch ((TermGroup_StaffingNeedsRuleUnit)this.Unit)
                {
                    case TermGroup_StaffingNeedsRuleUnit.ItemsPerMinute:
                        freqType = StaffingNeedsFrequencyType.NbrOfItems;
                        break;
                    case TermGroup_StaffingNeedsRuleUnit.CustomersPerMinute:
                        freqType = StaffingNeedsFrequencyType.NbrOfCustomers;
                        break;
                    case TermGroup_StaffingNeedsRuleUnit.AmountPerMinute:
                        freqType = StaffingNeedsFrequencyType.Amount;
                        break;
                }

                return freqType;
            }
        }
    }

    public partial class StaffingNeedsHead : ICreatedModified, IState
    {

    }

    public partial class StaffingNeedsHeadUser : ICreatedModified, IState
    {

    }

    public partial class StaffingNeedsLocation : ICreatedModified, IState
    {

    }

    public partial class StaffingNeedsLocationGroup : ICreatedModified, IState
    {

    }

    public partial class StaffingNeedsRow : ICreatedModified, IState
    {

    }

    public partial class StaffingNeedsRowPeriod : ICreatedModified, IState
    {

    }

    public partial class StaffingNeedsRowTask : ICreatedModified, IState
    {

    }

    public partial class StaffingNeedsRowFrequency : ICreatedModified, IState
    {

    }

    public partial class StaffingNeedsRule : ICreatedModified, IState
    {

    }

    public static partial class EntityExtensions
    {
        #region StaffingNeedsHead

        public static StaffingNeedsHeadDTO ToDTO(this StaffingNeedsHead e, bool includeRows, bool includePeriods, bool includeTasks, bool includeRowFrequencys)
        {
            if (e == null)
                return null;

            StaffingNeedsHeadDTO dto = new StaffingNeedsHeadDTO()
            {
                StaffingNeedsHeadId = e.StaffingNeedsHeadId,
                Type = (StaffingNeedsHeadType)e.Type,
                Interval = e.Interval,
                Name = e.Name,
                Description = e.Description,
                FromDate = e.FromDate,
                DayTypeId = e.DayTypeId,
                Weekday = (DayOfWeek?)e.Weekday,
                Date = e.Date,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Status = (TermGroup_StaffingNeedsHeadStatus)e.Status,
                AccountId = e.AccountId,
                ParentId = e.ParentId
            };

            dto.StaffingNeedsHeadUsers = new List<StaffingNeedsHeadUserDTO>();

            if (e.StaffingNeedsHeadUser != null)
            {
                foreach (var user in e.StaffingNeedsHeadUser.OrderByDescending(u => u.Main).ThenBy(u => u.User.Name))
                {
                    dto.StaffingNeedsHeadUsers.Add(user.ToDTO());
                }
            }
            if (includeRows)
                dto.Rows = e.StaffingNeedsRow?.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs(includePeriods, includeTasks, includeRowFrequencys).ToList() ?? new List<StaffingNeedsRowDTO>();
            if (includeRowFrequencys)
                dto.Rows = e.StaffingNeedsRow?.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs(includePeriods, includeTasks, includeRowFrequencys).ToList() ?? new List<StaffingNeedsRowDTO>();

            return dto;
        }

        public static IEnumerable<StaffingNeedsHeadDTO> ToDTOs(this IEnumerable<StaffingNeedsHead> l, bool includeRows, bool includePeriods, bool includeTasks, bool includeRowFrequencys)
        {
            var dtos = new List<StaffingNeedsHeadDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeRows, includePeriods, includeTasks, includeRowFrequencys));
                }
            }
            return dtos;
        }

        public static List<StaffingNeedsHead> Filter(this IEnumerable<StaffingNeedsHead> l, DayOfWeek dayOfWeek, DateTime fromDate)
        {
            return l?.Where(h => h.Weekday == (int)dayOfWeek && (!h.FromDate.HasValue || h.FromDate.Value <= fromDate)).ToList() ?? new List<StaffingNeedsHead>();
        }

        public static void SetDateOnRowFrequencys(this List<StaffingNeedsHeadDTO> l)
        {
            foreach (var e in l)
                e.SetDateOnRowFrequencys();
        }

        public static void SetDateOnRowFrequencys(this StaffingNeedsHeadDTO e)
        {
            if (e?.Rows != null && e.Date.HasValue)
            {
                foreach (var row in e.Rows.Where(r => r.RowFrequencys != null))
                {
                    foreach (var freq in row.RowFrequencys)
                    {
                        if (freq.Date < DateTime.Now.AddYears(-10))
                        {
                            freq.Date = e.Date.Value.Date;
                            if (e.Weekday.HasValue)
                                freq.StartTime = CalendarUtility.MergeDateAndTime(freq.Date, freq.ActualStartTime);
                        }
                    }
                }
            }
        }

        public static List<StaffingNeedsRowFrequencyDTO> GetRowFrequencys(this StaffingNeedsHeadDTO e)
        {
            List<StaffingNeedsRowFrequencyDTO> staffingNeedsRowFrequencyDTOs = new List<StaffingNeedsRowFrequencyDTO>();

            if (e?.Rows != null)
            {
                foreach (var row in e.Rows.Where(r => r.RowFrequencys != null))
                {
                    foreach (var freq in row.RowFrequencys)
                    {
                        staffingNeedsRowFrequencyDTOs.Add(freq);
                    }
                }
            }

            return staffingNeedsRowFrequencyDTOs;
        }

        public static List<StaffingNeedsRowDTO> GetValidRows(this StaffingNeedsHeadDTO e, TermGroup_StaffingNeedHeadsFilterType filterType, List<TimeScheduleTaskDTO> timeScheduleTasks, List<IncomingDeliveryHeadDTO> incomingDeliveryHeads, DateTime date, bool isChart)
        {
            List<StaffingNeedsRowDTO> rows = new List<StaffingNeedsRowDTO>();
            if (e.Rows != null)
            {
                foreach (StaffingNeedsRowDTO row in e.Rows)
                {
                    row.Weekday = e.Weekday;
                    row.DayTypeId = e.DayTypeId;
                    row.Date = e.Date;

                    List<StaffingNeedsRowPeriodDTO> periods = new List<StaffingNeedsRowPeriodDTO>();
                    foreach (StaffingNeedsRowPeriodDTO period in row.Periods)
                    {
                        TimeScheduleTaskDTO timeScheduleTask = period.TimeScheduleTaskId.HasValue ? timeScheduleTasks.FirstOrDefault(i => i.TimeScheduleTaskId == period.TimeScheduleTaskId.Value && i.State == SoeEntityState.Active) : null;
                        if (timeScheduleTask != null && timeScheduleTask.StopDate.HasValue && timeScheduleTask.StopDate.Value < date)
                            continue;

                        IncomingDeliveryHeadDTO incomingDeliveryHead = period.IncomingDeliveryRowId.HasValue ? incomingDeliveryHeads.FirstOrDefault(i => i.IncomingDeliveryHeadId == period.IncomingDeliveryRowId.Value && i.State == SoeEntityState.Active) : null;
                        if (incomingDeliveryHead != null && incomingDeliveryHead.StopDate.HasValue && incomingDeliveryHead.StopDate.Value < date)
                            continue;

                        if (isChart)
                        {
                            period.IsSpecificNeed = e.Weekday == null;
                        }
                        else
                        {
                            if (timeScheduleTask != null)
                            {
                                period.IsSpecificNeed = timeScheduleTask.IsSpecificNeed();
                                period.IsRemovedNeed = timeScheduleTask.RecurringDates != null && timeScheduleTask.RecurringDates.DoRecurOnDateButIsRemoved(date);
                            }
                            else if (incomingDeliveryHead != null)
                            {
                                period.IsSpecificNeed = incomingDeliveryHead.IsSpecificNeed();
                                period.IsRemovedNeed = incomingDeliveryHead.RecurringDates != null && incomingDeliveryHead.RecurringDates.DoRecurOnDateButIsRemoved(date);
                            }
                        }

                        if (period.IsValid(filterType))
                            periods.Add(period);
                    }

                    row.Periods = new List<StaffingNeedsRowPeriodDTO>();
                    if (periods.Any())
                    {
                        row.Periods.AddRange(periods);
                        rows.Add(row);
                    }
                    else if (row.RowFrequencys != null)
                    {
                        rows.Add(row);
                    }
                }
            }

            return rows;
        }

        public static List<StaffingNeedsHeadDTO> Filter(this IEnumerable<StaffingNeedsHeadDTO> e, DayOfWeek dayOfWeek, DateTime fromDate)
        {
            return e.Where(h => h.Weekday == dayOfWeek && (!h.FromDate.HasValue || h.FromDate.Value <= fromDate)).ToList();
        }

        public static bool IsValid(this StaffingNeedsRowPeriodDTO e, TermGroup_StaffingNeedHeadsFilterType filterType)
        {
            bool valid = false;
            switch (filterType)
            {
                case TermGroup_StaffingNeedHeadsFilterType.ActualNeed:
                    valid = (e.IsBaseNeed || e.IsSpecificNeed) && !e.IsRemovedNeed;
                    break;
                case TermGroup_StaffingNeedHeadsFilterType.BaseNeed:
                    valid = e.IsBaseNeed;
                    break;
                case TermGroup_StaffingNeedHeadsFilterType.SpecificNeed:
                    valid = e.IsSpecificNeed && !e.IsRemovedNeed;
                    break;
                case TermGroup_StaffingNeedHeadsFilterType.None:
                    valid = true; //Return all (used from gui)
                    break;
            }
            return valid;
        }

        public static StaffingNeedsHeadSmallDTO ToSmallDTO(this StaffingNeedsHead e)
        {
            if (e == null)
                return null;

            return new StaffingNeedsHeadSmallDTO()
            {
                StaffingNeedsHeadId = e.StaffingNeedsHeadId,
                Type = (StaffingNeedsHeadType)e.Type,
                Interval = e.Interval,
                Name = e.Name,
                DayTypeId = e.DayTypeId,
                Weekday = (DayOfWeek?)e.Weekday,
                Date = e.Date,
                Status = (TermGroup_StaffingNeedsHeadStatus)e.Status
            };
        }

        public static IEnumerable<StaffingNeedsHeadSmallDTO> ToSmallDTOs(this IEnumerable<StaffingNeedsHead> l, bool addEmpty)
        {
            var dtos = new List<StaffingNeedsHeadSmallDTO>();
            if (l != null)
            {
                if (addEmpty)
                {
                    dtos.Add(new StaffingNeedsHeadSmallDTO()
                    {
                        StaffingNeedsHeadId = 0,
                        Interval = 0,
                        Name = " ",
                    });
                }

                foreach (var e in l)
                {
                    dtos.Add(e.ToSmallDTO());
                }
            }
            return dtos;
        }

        public static List<StaffingNeedsHead> GetStaffingNeedsHeadsForDate(this List<StaffingNeedsHead> l, DateTime date)
        {
            var heads = l.Where(i => i.State == (int)SoeEntityState.Active && ((!i.FromDate.HasValue || i.FromDate.Value <= date) && (i.Date.HasValue && i.Date.Value == date))).OrderByDescending(i => i.FromDate).ThenByDescending(i => i.Created).ToList();

            var groups = heads.GroupBy(g => $"{g.Name}#{g.FromDate}#{g.Date}");

            List<StaffingNeedsHead> filtered = new List<StaffingNeedsHead>();

            foreach (var item in groups)
                filtered.Add(item.OrderByDescending(c => c.Created).First());

            return filtered;
        }

        public static List<StaffingNeedsHead> GetStaffingNeedsHeadsForDayOfWeek(this List<StaffingNeedsHead> l, DateTime date, List<StaffingNeedsHead> excludeHeads)
        {
            List<int> excludeIds = excludeHeads?.Select(s => s.StaffingNeedsHeadId).ToList() ?? new List<int>();
            List<StaffingNeedsHead> heads = l
                .Where(i => !excludeIds.Contains(i.StaffingNeedsHeadId) && (!i.FromDate.HasValue || i.FromDate.Value <= date) && (i.Weekday.HasValue && i.Weekday == (int)date.DayOfWeek))
                .OrderByDescending(i => i.FromDate)
                .ThenByDescending(i => i.Created)
                .ToList();

            List<StaffingNeedsHead> filteredNeeds = new List<StaffingNeedsHead>();

            foreach (StaffingNeedsHead head in heads.OrderByDescending(o => o.FromDate))
            {
                head.Date = date;

                if (!filteredNeeds.Any(a => a.Name == head.Name))
                    filteredNeeds.Add(head);
            }

            return filteredNeeds;
        }

        #endregion

        #region StaffingNeedsHeadUser

        public static StaffingNeedsHeadUserDTO ToDTO(this StaffingNeedsHeadUser e)
        {
            if (e == null)
                return null;

            return new StaffingNeedsHeadUserDTO()
            {
                StaffingNeedsHeadUserId = e.StaffingNeedsHeadUserId,
                StaffingNeedsHeadId = e.StaffingNeedsHeadId,
                UserId = e.UserId,
                Main = e.Main,
                LoginName = e.User?.LoginName ?? string.Empty,
                Name = e.User?.Name ?? string.Empty,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }
        #endregion

        #region StaffingNeedsRow

        public static StaffingNeedsRowDTO ToDTO(this StaffingNeedsRow e, bool includePeriods, bool includeTasks, bool includeRowFrequencys)
        {
            if (e == null)
                return null;

            StaffingNeedsRowDTO dto = new StaffingNeedsRowDTO()
            {
                StaffingNeedsRowId = e.StaffingNeedsRowId,
                StaffingNeedsHeadId = e.StaffingNeedsHeadId,
                ShiftTypeId = e.ShiftTypeId,
                Name = e.Name,
                Type = (StaffingNeedsRowType)e.Type,
                Weekday = (DayOfWeek?)e.StaffingNeedsHead.Weekday,
                DayTypeId = e.StaffingNeedsHead.DayTypeId,
                Date = e.StaffingNeedsHead.Date,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            // Relations
            if (includePeriods)
            {
                dto.Periods = e.StaffingNeedsRowPeriod?.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<StaffingNeedsRowPeriodDTO>();
                if (dto.Periods.Any())
                {
                    dto.DayStart = dto.Periods.Min(p => p.StartTime).TimeOfDay;
                    dto.DayEnd = dto.Periods.Max(p => p.StartTime).TimeOfDay.Add(TimeSpan.FromMinutes(dto.Periods.First().Interval));
                }
            }

            if (includeRowFrequencys)
            {
                dto.RowFrequencys = e.StaffingNeedsRowFrequency?.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<StaffingNeedsRowFrequencyDTO>();
                if (dto.RowFrequencys.Count > 0)
                {
                    dto.DayStart = dto.RowFrequencys.Min(p => p.StartTime).TimeOfDay;
                    dto.DayEnd = dto.RowFrequencys.Max(p => p.StartTime).TimeOfDay.Add(TimeSpan.FromMinutes(dto.RowFrequencys.First().Interval));
                }
            }

            if (includeTasks)
                dto.Tasks = e.StaffingNeedsRowTask?.Where(x => x.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<StaffingNeedsRowTaskDTO>();

            if (dto.ShiftTypeId.HasValue)
            {
                if (e.ShiftType != null)
                {
                    dto.ShiftTypeName = e.ShiftType.Name;
                    dto.ShiftTypeColor = e.ShiftType.Color;
                }
                else if (e.StaffingNeedsRowPeriod != null && e.StaffingNeedsRowPeriod.Count > 0 && e.StaffingNeedsRowPeriod.Select(p => p.ShiftTypeId).Distinct().Count() == 1)
                {
                    ShiftType shiftType = e.StaffingNeedsRowPeriod.Select(p => p.ShiftType).FirstOrDefault();
                    if (shiftType != null)
                    {
                        dto.ShiftTypeName = shiftType.Name;
                        dto.ShiftTypeColor = shiftType.Color;
                    }
                }
                else
                {
                    dto.ShiftTypeName = String.Empty;
                    dto.ShiftTypeColor = "#FF808080";   // Colors.Gray
                }
            }
            else
            {
                dto.ShiftTypeName = String.Empty;
                dto.ShiftTypeColor = "#FF808080";   // Colors.Gray
            }

            return dto;
        }

        public static IEnumerable<StaffingNeedsRowDTO> ToDTOs(this IEnumerable<StaffingNeedsRow> l, bool includePeriods, bool includeTasks, bool includeRowFrequencys)
        {
            var dtos = new List<StaffingNeedsRowDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includePeriods, includeTasks, includeRowFrequencys));
                }
            }
            return dtos;
        }

        #endregion

        #region StaffingNeedsRowPeriod

        public static StaffingNeedsRowPeriodDTO ToDTO(this StaffingNeedsRowPeriod e)
        {
            if (e == null)
                return null;

            return new StaffingNeedsRowPeriodDTO()
            {
                StaffingNeedsRowPeriodId = e.StaffingNeedsRowPeriodId,
                StaffingNeedsRowId = e.StaffingNeedsRowId,
                ShiftTypeId = e.ShiftTypeId,
                TimeScheduleTaskId = e.TimeScheduleTaskId,
                IncomingDeliveryRowId = e.IncomingDeliveryRowId,
                Interval = e.Interval,
                StartTime = e.StartTime,
                Value = e.Value,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                Length = e.Lenght,
                IsBreak = e.IsBreak,
                ParentId = e.ParentId,
                ShiftTypeName = e.ShiftType?.Name ?? string.Empty,
                ShiftTypeNeedsCode = e.ShiftType?.NeedsCode ?? string.Empty,
                ShiftTypeColor = e.ShiftType?.Color ?? string.Empty,
            };
        }

        public static IEnumerable<StaffingNeedsRowPeriodDTO> ToDTOs(this IEnumerable<StaffingNeedsRowPeriod> l)
        {
            var dtos = new List<StaffingNeedsRowPeriodDTO>();
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

        #region StaffingNeedsRowFrequency

        public static StaffingNeedsRowFrequencyDTO ToDTO(this StaffingNeedsRowFrequency e)
        {
            if (e == null)
                return null;

            return new StaffingNeedsRowFrequencyDTO()
            {
                StaffingNeedsRowFrequencyId = e.StaffingNeedsRowFrequencyId,
                StaffingNeedsRowId = e.StaffingNeedsRowId,
                ShiftTypeId = e.ShiftTypeId,
                Interval = e.Interval,
                StartTime = e.StartTime,
                Value = e.Value,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };
        }

        public static IEnumerable<StaffingNeedsRowFrequencyDTO> ToDTOs(this IEnumerable<StaffingNeedsRowFrequency> l)
        {
            var dtos = new List<StaffingNeedsRowFrequencyDTO>();
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

        #region StaffingNeedsRowTask

        public static StaffingNeedsRowTaskDTO ToDTO(this StaffingNeedsRowTask e)
        {
            if (e == null)
                return null;

            return new StaffingNeedsRowTaskDTO()
            {
                StaffingNeedsRowTaskId = e.StaffingNeedsRowTaskId,
                StaffingNeedsRowId = e.StaffingNeedsRowId,
                Task = e.Task,
                StartTime = e.StartTime,
                StopTime = e.StopTime,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }

        public static IEnumerable<StaffingNeedsRowTaskDTO> ToDTOs(this IEnumerable<StaffingNeedsRowTask> l)
        {
            var dtos = new List<StaffingNeedsRowTaskDTO>();
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

        #region StaffingNeedsFrequency

        public static StaffingNeedsFrequencyDTO ToDTO(this StaffingNeedsFrequency e, bool includeInternalAccounts)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (includeInternalAccounts && !e.IsAdded())
                {
                    if (!e.AccountInternal.IsLoaded)
                    {
                        e.AccountInternal.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("StaffingNeed.cs e.AccountInternal");
                    }
                    foreach (AccountInternal accountInternal in e.AccountInternal)
                    {
                        if (!accountInternal.AccountReference.IsLoaded)
                        {
                            accountInternal.AccountReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("StaffingNeed.cs accountInternal.AccountReference");
                        }
                        if (accountInternal.Account != null && !accountInternal.Account.AccountDimReference.IsLoaded)
                        {
                            accountInternal.Account.AccountDimReference.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("StaffingNeed.cs accountInternal.Account.AccountDimReference");
                        }
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            StaffingNeedsFrequencyDTO dto = new StaffingNeedsFrequencyDTO()
            {
                StaffingNeedsFrequencyId = e.StaffingNeedsFrequencyId,
                ActorCompanyId = e.ActorCompanyId,
                TimeFrom = e.TimeFrom,
                TimeTo = e.TimeTo,
                NbrOfItems = e.NbrOfItems,
                NbrOfCustomers = e.NbrOfCustomers,
                Amount = e.Amount,
                ExternalCode = e.ExternalCode,
                AccountId = e.AccountId,
                FrequencyType = (FrequencyType)e.FrequencyType,
                ParentAccountId = e.ParentAccountId,
                NbrOfMinutes = e.NbrOfMinutes,
                Cost = e.Cost
            };

            if (includeInternalAccounts)
            {
                foreach (AccountInternal accountInternal in e.AccountInternal)
                {
                    Account accInt = accountInternal.Account;

                    if (accInt != null && accInt.AccountDim != null)
                    {
                        switch (accInt.AccountDim.AccountDimNr)
                        {
                            case 2:
                                dto.Dim2Id = accInt.AccountId;
                                dto.Dim2Nr = accInt.AccountNr;
                                dto.Dim2Name = accInt.Name;
                                break;
                            case 3:
                                dto.Dim3Id = accInt.AccountId;
                                dto.Dim3Nr = accInt.AccountNr;
                                dto.Dim3Name = accInt.Name;
                                break;
                            case 4:
                                dto.Dim4Id = accInt.AccountId;
                                dto.Dim4Nr = accInt.AccountNr;
                                dto.Dim4Name = accInt.Name;
                                break;
                            case 5:
                                dto.Dim5Id = accInt.AccountId;
                                dto.Dim5Nr = accInt.AccountNr;
                                dto.Dim5Name = accInt.Name;
                                break;
                            case 6:
                                dto.Dim6Id = accInt.AccountId;
                                dto.Dim6Nr = accInt.AccountNr;
                                dto.Dim6Name = accInt.Name;
                                break;
                        }
                    }
                }
            }

            return dto;
        }

        public static IEnumerable<StaffingNeedsFrequencyDTO> ToDTOs(this IEnumerable<StaffingNeedsFrequency> l, bool includeInternalAccounts)
        {
            var dtos = new List<StaffingNeedsFrequencyDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeInternalAccounts));
                }
            }
            return dtos;
        }

        public static decimal ItemsPerMinute(this StaffingNeedsFrequency e, StaffingNeedsFrequencyType freqType, int freqInterval)
        {
            // Get items per minute based on frequence type
            decimal itemsPerMinute = 0;
            switch (freqType)
            {
                case StaffingNeedsFrequencyType.NbrOfItems:
                    itemsPerMinute = Decimal.Divide(e.NbrOfItems, freqInterval);
                    break;
                case StaffingNeedsFrequencyType.NbrOfCustomers:
                    itemsPerMinute = Decimal.Divide(e.NbrOfCustomers, freqInterval);
                    break;
                case StaffingNeedsFrequencyType.Amount:
                    itemsPerMinute = Decimal.Divide(e.Amount, freqInterval);
                    break;
            }

            return itemsPerMinute;
        }

        public static int? GetGroupId(this StaffingNeedsFrequency e, List<StaffingNeedsLocation> locations, List<StaffingNeedsLocation> allLocations)
        {
            int? groupId = null;

            var location = e.AccountId.HasValue ? locations.FirstOrDefault(g => !string.IsNullOrEmpty(g.ExternalCode) && g.ExternalCode.ToLower().Equals(e.ExternalCode.ToLower()) &&
                           g.StaffingNeedsLocationGroup != null && g.StaffingNeedsLocationGroup.AccountId.HasValue && g.StaffingNeedsLocationGroup.AccountId == e.AccountId) : null;

            if (location == null)
                location = e.ParentAccountId.HasValue ? locations.FirstOrDefault(g => !string.IsNullOrEmpty(g.ExternalCode) && g.ExternalCode.ToLower().Equals(e.ExternalCode.ToLower()) &&
                            g.StaffingNeedsLocationGroup != null && g.StaffingNeedsLocationGroup.AccountId.HasValue && g.StaffingNeedsLocationGroup.AccountId == e.ParentAccountId) : null;

            if (location == null)
                location = locations.FirstOrDefault(g => !string.IsNullOrEmpty(g.ExternalCode) && g.ExternalCode.ToLower().Equals(e.ExternalCode.ToLower()));

            if (location != null)
                groupId = location.StaffingNeedsLocationGroupId;

            if (!groupId.HasValue && allLocations.Count == 1)
                groupId = allLocations.FirstOrDefault()?.StaffingNeedsLocationGroupId;

            return groupId;
        }

        #endregion

        #region StaffingNeedsLocation

        public static StaffingNeedsLocationDTO ToDTO(this StaffingNeedsLocation e)
        {
            if (e == null)
                return null;

            return new StaffingNeedsLocationDTO()
            {
                StaffingNeedsLocationId = e.StaffingNeedsLocationId,
                StaffingNeedsLocationGroupId = e.StaffingNeedsLocationGroupId,
                Name = e.Name,
                Description = e.Description,
                ExternalCode = e.ExternalCode,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };
        }

        public static StaffingNeedsLocationGridDTO ToGridDTO(this StaffingNeedsLocation e, bool includeGroupName, bool includeGroupAccount = false)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && includeGroupName && !e.StaffingNeedsLocationGroupReference.IsLoaded)
                {
                    e.StaffingNeedsLocationGroupReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("StaffingNeed.cs e.StaffingNeedsLocationGroupReference");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            StaffingNeedsLocationGridDTO dto = new StaffingNeedsLocationGridDTO()
            {
                StaffingNeedsLocationId = e.StaffingNeedsLocationId,
                Name = e.Name,
                Description = e.Description,
                ExternalCode = e.ExternalCode,
            };

            if (includeGroupName && e.StaffingNeedsLocationGroup != null)
            {
                dto.GroupId = e.StaffingNeedsLocationGroup.StaffingNeedsLocationGroupId;
                dto.GroupName = e.StaffingNeedsLocationGroup.Name;

                if (includeGroupAccount)
                {
                    if (!e.StaffingNeedsLocationGroup.AccountReference.IsLoaded)
                    {
                        e.StaffingNeedsLocationGroup.AccountReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("StaffingNeed.cs e.StaffingNeedsLocationGroup.AccountReference");
                    }

                    if (e.StaffingNeedsLocationGroup.Account != null)
                    {
                        dto.GroupAccountId = e.StaffingNeedsLocationGroup.Account.AccountId;
                        dto.GroupAccountName = e.StaffingNeedsLocationGroup.Account.Name;
                    }
                }
            }

            return dto;
        }

        public static IEnumerable<StaffingNeedsLocationDTO> ToDTOs(this IEnumerable<StaffingNeedsLocation> l)
        {
            var dtos = new List<StaffingNeedsLocationDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static IEnumerable<StaffingNeedsLocationGridDTO> ToGridDTOs(this IEnumerable<StaffingNeedsLocation> l, bool includeGroupName, bool includeGroupAccount = false)
        {
            var dtos = new List<StaffingNeedsLocationGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO(includeGroupName, includeGroupAccount));
                }
            }
            return dtos;
        }

        #endregion

        #region StaffingNeedsLocationGroup

        public static StaffingNeedsLocationGroupDTO ToDTO(this StaffingNeedsLocationGroup e, bool includeShiftTypes)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && includeShiftTypes && !e.ShiftType.IsLoaded)
                {
                    e.ShiftType.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("StaffingNeed.cs e.ShiftType");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            StaffingNeedsLocationGroupDTO dto = new StaffingNeedsLocationGroupDTO()
            {
                StaffingNeedsLocationGroupId = e.StaffingNeedsLocationGroupId,
                ActorCompanyId = e.ActorCompanyId,
                Name = e.Name,
                Description = e.Description,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
                TimeScheduleTaskId = e.TimeScheduleTaskId,
                accountId = e.AccountId,
                AccountName = e.Account?.Name ?? string.Empty,
            };

            dto.StaffingNeedsLocations = e.StaffingNeedsLocation?.ToDTOs().ToList() ?? new List<StaffingNeedsLocationDTO>();
            if (includeShiftTypes)
            {
                dto.ShiftTypeIds = e.ShiftType?.Select(i => i.ShiftTypeId).ToList() ?? new List<int>();
                dto.SelectedShiftTypeNames = e.ShiftType?.Select(i => i.Name).Distinct().ToCommaSeparated();
            }

            return dto;
        }

        public static StaffingNeedsLocationGroupGridDTO ToGridDTO(this StaffingNeedsLocationGroup e)
        {
            if (e == null)
                return null;

            StaffingNeedsLocationGroupGridDTO dto = new StaffingNeedsLocationGroupGridDTO()
            {
                StaffingNeedsLocationGroupId = e.StaffingNeedsLocationGroupId,
                Name = e.Name,
                Description = e.Description,
                TimeScheduleTaskId = e.TimeScheduleTaskId,
                TimeScheduleTaskName = e.TimeScheduleTask?.Name ?? string.Empty,
                AccountId = e.AccountId,
                AccountName = e.Account?.Name ?? string.Empty,
            };

            dto.SelectedShiftTypeNames = e.ShiftType?.Select(i => i.Name).Distinct().ToCommaSeparated();

            return dto;
        }

        public static IEnumerable<StaffingNeedsLocationGroupDTO> ToDTOs(this IEnumerable<StaffingNeedsLocationGroup> l, bool includeShiftTypes)
        {
            var dtos = new List<StaffingNeedsLocationGroupDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeShiftTypes));
                }
            }
            return dtos;
        }

        public static IEnumerable<StaffingNeedsLocationGroupGridDTO> ToGridDTOs(this IEnumerable<StaffingNeedsLocationGroup> l)
        {
            var dtos = new List<StaffingNeedsLocationGroupGridDTO>();
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

        #region StaffingNeedsRule

        public static StaffingNeedsRuleDTO ToDTO(this StaffingNeedsRule e, bool includeRows)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && includeRows && !e.StaffingNeedsRuleRow.IsLoaded)
                {
                    e.StaffingNeedsRuleRow.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("StaffingNeed.cs e.StaffingNeedsRuleRow");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            StaffingNeedsRuleDTO dto = new StaffingNeedsRuleDTO()
            {
                StaffingNeedsRuleId = e.StaffingNeedsRuleId,
                StaffingNeedsLocationGroupId = e.StaffingNeedsLocationGroupId,
                Name = e.Name,
                Unit = (TermGroup_StaffingNeedsRuleUnit)e.Unit,
                MaxQuantity = e.MaxQuantity,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                AccountId = e.AccountId,
                State = (SoeEntityState)e.State
            };

            if (includeRows)
                dto.Rows = e.StaffingNeedsRuleRow?.ToDTOs().ToList() ?? new List<StaffingNeedsRuleRowDTO>();

            return dto;
        }

        public static StaffingNeedsRuleGridDTO ToGridDTO(this StaffingNeedsRule e, bool includeGroupName)
        {
            if (e == null)
                return null;

            StaffingNeedsRuleGridDTO dto = new StaffingNeedsRuleGridDTO()
            {
                StaffingNeedsRuleId = e.StaffingNeedsRuleId,
                Name = e.Name,
                MaxQuantity = e.MaxQuantity,
                AccountId = e.AccountId,
                AccountName = e.Account?.Name ?? string.Empty,
            };

            if (includeGroupName && e.StaffingNeedsLocationGroup != null)
                dto.GroupName = e.StaffingNeedsLocationGroup.Name;

            return dto;
        }

        public static IEnumerable<StaffingNeedsRuleDTO> ToDTOs(this IEnumerable<StaffingNeedsRule> l, bool includeRows)
        {
            var dtos = new List<StaffingNeedsRuleDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeRows));
                }
            }
            return dtos;
        }

        public static IEnumerable<StaffingNeedsRuleGridDTO> ToGridDTOs(this IEnumerable<StaffingNeedsRule> l, bool includeGroupName)
        {
            var dtos = new List<StaffingNeedsRuleGridDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToGridDTO(includeGroupName));
                }
            }
            return dtos.OrderBy(e => e.Name).ThenBy(e => e.AccountName).ThenBy(e => e.GroupName).ToList();
        }

        #endregion

        #region StaffingNeedsRuleRow

        public static StaffingNeedsRuleRowDTO ToDTO(this StaffingNeedsRuleRow e)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded() && !e.DayTypeReference.IsLoaded)
                {
                    e.DayTypeReference.Load();
                    DataProjectLogCollector.LogLoadedEntityInExtension("StaffingNeed.cs e.DayTypeReference");
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            StaffingNeedsRuleRowDTO dto = new StaffingNeedsRuleRowDTO()
            {
                StaffingNeedsRuleRowId = e.StaffingNeedsRuleRowId,
                StaffingNeedsRuleId = e.StaffingNeedsRuleId,
                Sort = e.Sort,
                DayTypeId = e.DayTypeId,
                Weekday = (DayOfWeek?)e.Weekday,
                Value = e.Value
            };

            if (e.DayTypeId.HasValue)
                dto.DayName = e.DayType?.Name ?? string.Empty;
            else if (e.Weekday.HasValue)
                dto.DayName = CalendarUtility.GetDayName((DayOfWeek)e.Weekday, Thread.CurrentThread.CurrentCulture, true);

            return dto;
        }

        public static IEnumerable<StaffingNeedsRuleRowDTO> ToDTOs(this IEnumerable<StaffingNeedsRuleRow> l)
        {
            var dtos = new List<StaffingNeedsRuleRowDTO>();
            if (l != null)
            {
                foreach (var e in l.OrderBy(o => o.Sort).ToList())
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        #endregion

        #region StaffingNeedsCalculationTimeSlot

        public static StaffingNeedsCalculationTimeSlot AsFixedSlot(this StaffingNeedsCalculationTimeSlot e, DateTime start, int interval)
        {
            e.From = start;
            e.MinFrom = start;
            e.To = start.AddMinutes(interval);
            e.MaxTo = start.AddMinutes(interval);
            return e;
        }

        #endregion 
    }
}
