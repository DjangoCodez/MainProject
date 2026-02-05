using Newtonsoft.Json;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeScheduleTemplateBlock : IScheduleBlockObject, IScheduleBlockAccounting, ICreatedModified, IState
    {
        // Used sometimes when cloning
        public int OldTimeScheduleTemplateBlockId { get; set; }

        private bool guidSet = false;
        private Guid guid;
        public Guid Guid
        {
            get
            {
                if (!guidSet)
                {
                    guid = Guid.NewGuid();
                    guidSet = true;
                }
                return guid;
            }
        }
        public bool Unique { get; set; }
        public int TimeCodeType { get; set; }

        private bool? isBreak { get; set; }

        public bool IsBreak
        {
            get
            {
                if (isBreak.HasValue)
                    return isBreak.Value;
                isBreak = this.IsBreak();
                return isBreak.Value;
            }
        }

        //Midnight secure
        public DateTime? ActualStartTime
        {
            get { return Date.HasValue ? CalendarUtility.MergeDateAndTime(this.Date.Value.AddDays((this.StartTime.Date - CalendarUtility.DATETIME_DEFAULT).Days), this.StartTime) : (DateTime?)null; }
        }
        //Midnight secure
        public DateTime? ActualStopTime
        {
            get { return ActualStartTime.HasValue ? ActualStartTime.Value.AddMinutes((this.StopTime - this.StartTime).TotalMinutes) : (DateTime?)null; }
        }
        public int TotalMinutes
        {
            get { return (int)StopTime.Subtract(StartTime).TotalMinutes; }
        }
        public bool BelongsToPreviousDay
        {
            get { return StartTime.Date.AddDays(-1) == CalendarUtility.DATETIME_DEFAULT; }
        }
        public bool BelongsToNextDay
        {
            get { return StartTime.Date.AddDays(1) == CalendarUtility.DATETIME_DEFAULT; }
        }
        public bool IsSchedule()
        {
            return this.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Schedule;
        }
        public bool IsOrder()
        {
            // A shift with a connected order
            return this.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Order && this.CustomerInvoiceId.HasValue;
        }
        public bool IsBooking()
        {
            return this.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Booking;
        }
        public bool IsStandby()
        {
            return this.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Standby;
        }

        public bool IsOnDuty()
        {
            return this.Type == (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty;
        }

        public bool ChangesAffectsRestore()
        {
            return this.IsSchedule() || this.IsStandby() || this.IsOnDuty();
        }

        public bool IsZero
        {
            get { return TotalMinutes == 0; }
        }
        public string TimeScheduleTypeName { get; set; }

        public bool TempUsed { get; set; }
        public List<int> AccountIdsIncludingAccountIdOnBlock { get; set; }
        public string AccountIdsIncludingAccountIdOnBlockString { get; set; }
        public bool BreakIsOverlappedByNotScheduleTimeShift { get; set; }
        public bool IsNotScheduleTimeShift { get; set; }
        public bool DateHasBeenAltered { get; set; }

        public int GetMinutes() => (int)this.StopTime.Subtract(this.StartTime).TotalMinutes;

        public (DateTime start, DateTime stop) GetStartAndStopTime()
        {
            var startTime = this.StartTime;
            var stopTime = this.StopTime;
            if (this.IsBooking() && startTime.Year != 1900)
            {
                stopTime = CalendarUtility.GetScheduleTime(stopTime, startTime.Date, stopTime.Date);
                startTime = CalendarUtility.GetScheduleTime(startTime);
            }
            return (startTime, stopTime);
        }

        public (DateTime start, DateTime stop) GetNewActualStartAndStopTime(DateTime newDate)
        {
            var (startTime, stopTime) = this.GetStartAndStopTime();

            DateTime start = CalendarUtility.MergeDateAndTime(newDate.AddDays((startTime.Date - CalendarUtility.DATETIME_DEFAULT.Date).Days), startTime);
            return (start, start.Add(stopTime - startTime));
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this.GetInfoObject());
        }
    }

    public partial class TimeScheduleTemplateBlockHistory : ICreated, IState
    {

    }

    public partial class TimeScheduleTemplateBlockTask : ICreatedModified, IState
    {
        public TimeScheduleTemplatePeriod CalculatedTemplatePeriod { get; set; }
    }

    public partial class GetTimeScheduleTemplateBlockAccountsForEmployeeResult : IAccountId
    {

    }

    public static partial class EntityExtensions
    {
        #region TimeScheduleTemplateBlock

        public static TimeScheduleTemplateBlockDTO ToDTO(this TimeScheduleTemplateBlock e, bool includeAccounts = false, bool includeTasks = false)
        {
            if (e == null)
                return null;

            TimeScheduleTemplateBlockDTO dto = new TimeScheduleTemplateBlockDTO()
            {
                EmployeeId = e.EmployeeId,
                TimeScheduleTemplateBlockId = e.TimeScheduleTemplateBlockId,
                TimeScheduleTemplatePeriodId = e.TimeScheduleTemplatePeriodId,
                TimeScheduleEmployeePeriodId = e.TimeScheduleEmployeePeriodId,
                TimeCodeId = e.TimeCodeId,
                TimeScheduleTypeId = e.TimeScheduleTypeId,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                EmployeeChildId = e.EmployeeChildId,
                ShiftTypeId = e.ShiftTypeId,
                AccountId = e.AccountId,
                StaffingNeedsRowPeriodId = e.StaffingNeedsRowPeriodId,
                StaffingNeedsRowId = e.StaffingNeedsRowId,
                CustomerInvoiceId = e.CustomerInvoiceId,
                ProjectId = e.ProjectId,

                Date = e.Date,
                StartTime = e.StartTime,
                StopTime = e.StopTime,
                IsBreak = e.IsBreak,
                BreakType = (SoeTimeScheduleTemplateBlockBreakType)e.BreakType,
                Type = (TermGroup_TimeScheduleTemplateBlockType)e.Type,
                Description = e.Description,
                Link = !String.IsNullOrEmpty(e.Link) ? new Guid(e.Link) : (Guid?)null,
                RecalculateTimeRecordId = e.RecalculateTimeRecordId,
                RecalculateTimeRecordStatus = e.RecalculateTimeRecord != null ? (TermGroup_RecalculateTimeRecordStatus)e.RecalculateTimeRecord.Status : TermGroup_RecalculateTimeRecordStatus.Waiting,
                TimeDeviationCauseStatus = (SoeTimeScheduleDeviationCauseStatus)e.TimeDeviationCauseStatus,
                TimeScheduleTypeName = e.TimeScheduleTypeName,
                ShiftStatus = (TermGroup_TimeScheduleTemplateBlockShiftStatus)e.ShiftStatus,
                ShiftUserStatus = (TermGroup_TimeScheduleTemplateBlockShiftUserStatus)e.ShiftUserStatus,
                State = (SoeEntityState)e.State,
            };

            if (e.TimeScheduleTemplatePeriod != null)
            {
                dto.DayNumber = e.TimeScheduleTemplatePeriod.DayNumber;
                dto.DayName = e.TimeScheduleTemplatePeriod.DayName;
                dto.IsHoliday = e.TimeScheduleTemplatePeriod.IsHoliday;
                dto.HasAttestedTransactions = e.TimeScheduleTemplatePeriod.HasAttestedTransactions;
            }
            if (e.ShiftType != null)
            {
                dto.ShiftTypeName = e.ShiftType.Name;
                dto.ShiftTypeDescription = e.ShiftType.Description;
                dto.ShiftTypeTimeScheduleTypeId = e.ShiftType.TimeScheduleTypeId;
            }
            if (e.Account != null)
                dto.AccountName = e.Account.Name;
            if (e.TimeCode != null)
                dto.TimeCode = e.TimeCode.ToDTO();
            if (includeAccounts)
                dto.AccountInternals = e.AccountInternal?.Select(a => a.Account).ToDTOs(includeAccountDim: true) ?? new List<AccountDTO>();
            if (e.AccountInternal != null)
                dto.AccountInternalIds = e.AccountInternal.Select(a => a.AccountId).ToList();
            if (includeTasks && !e.IsBreak)
                dto.Tasks = e.TimeScheduleTemplateBlockTask?.ToDTOs().ToList() ?? new List<TimeScheduleTemplateBlockTaskDTO>();

            return dto;
        }

        public static List<TimeScheduleTemplateBlockDTO> ToDTOs(this IEnumerable<TimeScheduleTemplateBlock> l, bool includeAccounts = false, bool includeTasks = false)
        {
            var dtos = new List<TimeScheduleTemplateBlockDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeAccounts, includeTasks));
                }
            }
            return dtos;
        }

        public static List<Tuple<string, string>> GetLinkMappings(this List<TimeScheduleTemplateBlock> l)
        {
            List<Tuple<string, string>> linkMappings = new List<Tuple<string, string>>();
            foreach (var linkGroup in l.Where(b => b.Link.HasValue()).GroupBy(g => g.Link))
            {
                linkMappings.Add(Tuple.Create(linkGroup.Key, Guid.NewGuid().ToString()));
            }

            return linkMappings;
        }

        public static List<TimeSchedulePlanningDayDTO> ToTimeSchedulePlanningDayDTOs(this List<TimeScheduleTemplateBlock> l, bool isHiddenEmployee = false)
        {
            var dtos = new List<TimeSchedulePlanningDayDTO>();

            if (l.IsNullOrEmpty())
                return dtos;

            foreach (var e in l.Where(i => !i.IsBreak))
            {
                var dto = e.ToTimeSchedulePlanningDayDTO();
                if (dto != null)
                {
                    #region Breaks

                    if (e.TimeScheduleEmployeePeriodId.HasValue)
                    {
                        var breaks = l.Where(tb => tb.IsBreak && tb.TimeScheduleEmployeePeriodId.HasValue && tb.TimeScheduleEmployeePeriodId.Value == e.TimeScheduleEmployeePeriodId.Value).ToList();
                        if (isHiddenEmployee)
                            breaks = breaks.Where(x => x.Link == e.Link).ToList();

                        int breakNr = 1;
                        foreach (TimeScheduleTemplateBlock b in breaks)
                        {
                            int id = b.TimeScheduleTemplateBlockId;
                            int timeCodeId = b.TimeCodeId;
                            DateTime startTime = b.ActualStartTime.HasValue ? b.ActualStartTime.Value : b.StartTime;
                            int length = (int)b.StopTime.Subtract(b.StartTime).TotalMinutes;
                            Guid? link = !String.IsNullOrEmpty(b.Link) ? new Guid(b.Link) : (Guid?)null;
                            bool isPreliminary = b.IsPreliminary;

                            switch (breakNr)
                            {
                                case 1:
                                    dto.Break1Id = id;
                                    dto.Break1TimeCodeId = timeCodeId;
                                    dto.Break1StartTime = startTime;
                                    dto.Break1Minutes = length;
                                    dto.Break1Link = link;
                                    dto.Break1IsPreliminary = isPreliminary;
                                    break;
                                case 2:
                                    dto.Break2Id = id;
                                    dto.Break2TimeCodeId = timeCodeId;
                                    dto.Break2StartTime = startTime;
                                    dto.Break2Minutes = length;
                                    dto.Break2Link = link;
                                    dto.Break2IsPreliminary = isPreliminary;
                                    break;
                                case 3:
                                    dto.Break3Id = id;
                                    dto.Break3TimeCodeId = timeCodeId;
                                    dto.Break3StartTime = startTime;
                                    dto.Break3Minutes = length;
                                    dto.Break3Link = link;
                                    dto.Break3IsPreliminary = isPreliminary;
                                    break;
                                case 4:
                                    dto.Break4Id = id;
                                    dto.Break4TimeCodeId = timeCodeId;
                                    dto.Break4StartTime = startTime;
                                    dto.Break4Minutes = length;
                                    dto.Break4Link = link;
                                    dto.Break4IsPreliminary = isPreliminary;
                                    break;
                            }
                            breakNr++;
                        }
                    }

                    #endregion

                    dtos.Add(dto);
                }
            }

            return dtos;
        }

        public static TimeSchedulePlanningDayDTO ToTimeSchedulePlanningDayDTO(this TimeScheduleTemplateBlock e)
        {
            if (e == null || !e.Date.HasValue || e.IsBreak)
                return null;

            // Handle both 1900-01-01 and correct date (like 2025-01-01)
            DateTime startTime = e.StartTime;
            if (e.StartTime.Date < CalendarUtility.DATETIME_DEFAULT.Date.AddDays(2))
                startTime = CalendarUtility.MergeDateAndTime(e.Date.Value.AddDays((e.StartTime.Date - CalendarUtility.DATETIME_DEFAULT.Date).Days), e.StartTime);
            DateTime stopTime = startTime.AddMinutes(e.TotalMinutes);

            TimeSchedulePlanningDayDTO dayDTO = new TimeSchedulePlanningDayDTO()
            {
                UniqueId = Guid.NewGuid().ToString(),
                Type = (TermGroup_TimeScheduleTemplateBlockType)e.Type,
                StartTime = startTime,
                StopTime = stopTime,
                BelongsToPreviousDay = e.BelongsToPreviousDay,
                BelongsToNextDay = e.BelongsToNextDay,
                Description = e.Description,
                IsPreliminary = e.IsPreliminary,

                //Set FK
                TimeScheduleTemplateBlockId = e.TimeScheduleTemplateBlockId,
                TimeScheduleTemplatePeriodId = e.TimeScheduleTemplatePeriodId,
                TimeScheduleEmployeePeriodId = e.TimeScheduleEmployeePeriodId ?? 0,
                TimeScheduleScenarioHeadId = e.TimeScheduleScenarioHeadId,
                TimeCodeId = e.TimeCodeId,
                EmployeeId = e.EmployeeId ?? 0,
                TimeScheduleTypeId = e.TimeScheduleTypeId ?? 0,
                TimeScheduleTypeName = e.TimeScheduleType?.Name,
                ShiftTypeId = e.ShiftTypeId ?? 0,
                ShiftTypeName = e.ShiftType?.Name,
                ShiftTypeColor = e.ShiftType?.Color,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                TimeDeviationCauseName = e.TimeDeviationCause?.Name,
                EmployeeChildId = e.EmployeeChildId,
                ShiftStatus = (TermGroup_TimeScheduleTemplateBlockShiftStatus)e.ShiftStatus,
                ShiftUserStatus = (TermGroup_TimeScheduleTemplateBlockShiftUserStatus)e.ShiftUserStatus,
                Link = (!String.IsNullOrEmpty(e.Link)) ? new Guid(e.Link) : (Guid?)null,
                ExtraShift = e.ExtraShift,
                SubstituteShift = e.SubstituteShift,
                StaffingNeedsRowId = e.StaffingNeedsRowId,
                StaffingNeedsRowPeriodId = e.StaffingNeedsRowPeriodId,
                AccountId = e.AccountId,
                PlannedTime = e.PlannedTime,
            };

            // Order planning
            if (e.CustomerInvoiceId.HasValue)
            {
                dayDTO.Order = new OrderListDTO()
                {
                    OrderId = e.CustomerInvoiceId.Value,
                    ProjectId = e.ProjectId
                };
            }

            return dayDTO;
        }

        public static ShiftDTO ToShiftDTO(this TimeSchedulePlanningDayDTO e)
        {
            if (e == null)
                return null;

            ShiftDTO shiftDTO = new ShiftDTO()
            {
                Type = e.Type,
                TimeScheduleTemplateBlockId = e.TimeScheduleTemplateBlockId,
                TimeScheduleEmployeePeriodId = e.TimeScheduleEmployeePeriodId,
                TimeCodeId = e.TimeCodeId,
                Description = e.Description,

                StartTime = e.StartTime,
                StopTime = e.StopTime,
                //AbsenceStartTime = e.AbsenceStartTime,
                //AbsenceStopTime = e.AbsenceStopTime,
                BelongsToPreviousDay = e.BelongsToPreviousDay,
                BelongsToNextDay = e.BelongsToNextDay,

                TimeScheduleTypeId = e.TimeScheduleTypeId,
                TimeScheduleTypeCode = e.TimeScheduleTypeCode,
                TimeScheduleTypeName = e.TimeScheduleTypeName,
                TimeScheduleTypeIsNotScheduleTime = e.TimeScheduleTypeIsNotScheduleTime,
                TimeScheduleTypeFactors = e.TimeScheduleTypeFactors,
                ShiftTypeTimeScheduleTypeId = e.ShiftTypeTimeScheduleTypeId,
                ShiftTypeTimeScheduleTypeCode = e.ShiftTypeTimeScheduleTypeCode,
                ShiftTypeTimeScheduleTypeName = e.ShiftTypeTimeScheduleTypeName,

                EmployeeId = e.EmployeeId,
                EmployeeName = e.EmployeeName,
                //EmployeePostId = e.EmployeePostId,
                //EmployeeChildId = e.EmployeeChildId,

                GrossTime = e.GrossTime,
                TotalCost = e.TotalCost,
                TotalCostIncEmpTaxAndSuppCharge = e.TotalCostIncEmpTaxAndSuppCharge,

                Breaks = new List<ShiftBreakDTO>(),

                TimeDeviationCauseId = e.TimeDeviationCauseId,
                TimeDeviationCauseName = e.TimeDeviationCauseName,

                ShiftTypeId = e.ShiftTypeId,
                ShiftTypeCode = e.ShiftTypeCode,
                ShiftTypeName = e.ShiftTypeName,
                ShiftTypeDesc = e.ShiftTypeDesc,
                ShiftTypeColor = e.ShiftTypeColor,

                ShiftStatus = e.ShiftStatus,
                //ShiftStatusName = e.ShiftStatusName,
                ShiftUserStatus = e.ShiftUserStatus,
                //ShiftUserStatusName = e.ShiftUserStatusName,

                IsPreliminary = e.IsPreliminary,
                ExtraShift = e.ExtraShift,
                SubstituteShift = e.SubstituteShift,
                IsDeleted = e.IsDeleted,

                AccountId = e.AccountId,
                AccountName = e.AccountName,
                //HasMultipleEmployeeAccountsOnDate = e.HasMultipleEmployeeAccountsOnDate,

                NbrOfWantedInQueue = e.NbrOfWantedInQueue,
                //IamInQueue = e.IamInQueue,

                //HasSwapRequest = e.HasSwapRequest,
                //SwapShiftInfo = e.SwapShiftInfo,

                //HasShiftRequest = e.HasShiftRequest,
                //ShiftRequestAnswerType = e.ShiftRequestAnswerType,
                //ApprovalTypeId = e.ApprovalTypeId,

                TimeScheduleTemplateHeadId = e.TimeScheduleTemplateHeadId,
                //TimeScheduleTemplatePeriodId = e.TimeScheduleTemplatePeriodId,
                NbrOfWeeks = e.NbrOfWeeks,
                DayNumber = e.DayNumber,
                OriginalBlockId = e.OriginalBlockId,

                Link = e.Link,

                IsAbsenceRequest = e.IsAbsenceRequest,

                TimeLeisureCodeId = e.TimeLeisureCodeId,

                //StaffingNeedsRowId = e.StaffingNeedsRowId,

                //TimeScheduleEmployeePeriodId = e.TimeScheduleEmployeePeriodId,
                //Order = e.Order,
                //PlannedTime = e.PlannedTime,

                //TimeScheduleScenarioHeadId = e.TimeScheduleScenarioHeadId,
            };

            return shiftDTO;
        }

        public static List<ShiftDTO> ToShiftDTOs(this IEnumerable<TimeSchedulePlanningDayDTO> l)
        {
            var dtos = new List<ShiftDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToShiftDTO());
                }
            }
            return dtos;
        }

        public static TimeSchedulePlanningDayDTO ToTimeSchedulePlanningDayDTO(this ShiftDTO e)
        {
            if (e == null)
                return null;

            TimeSchedulePlanningDayDTO dayDTO = new TimeSchedulePlanningDayDTO()
            {
                Type = e.Type,
                TimeScheduleTemplateBlockId = e.TimeScheduleTemplateBlockId,
                TimeCodeId = e.TimeCodeId,
                Description = e.Description,

                StartTime = e.StartTime,
                StopTime = e.StopTime,
                AbsenceStartTime = e.AbsenceStartTime,
                AbsenceStopTime = e.AbsenceStopTime,
                BelongsToPreviousDay = e.BelongsToPreviousDay,
                BelongsToNextDay = e.BelongsToNextDay,

                //TimeScheduleTypeId = e.TimeScheduleTypeId,
                //TimeScheduleTypeCode = e.TimeScheduleTypeCode,
                //TimeScheduleTypeName = e.TimeScheduleTypeName,
                //TimeScheduleTypeIsNotScheduleTime = e.TimeScheduleTypeIsNotScheduleTime,
                //TimeScheduleTypeFactors = e.TimeScheduleTypeFactors,
                ShiftTypeTimeScheduleTypeId = e.ShiftTypeTimeScheduleTypeId,
                //ShiftTypeTimeScheduleTypeCode = e.ShiftTypeTimeScheduleTypeCode,
                ShiftTypeTimeScheduleTypeName = e.ShiftTypeTimeScheduleTypeName,

                EmployeeId = e.EmployeeId,
                EmployeeName = e.EmployeeName,
                //EmployeePostId = e.EmployeePostId,
                //EmployeeChildId = e.EmployeeChildId,

                GrossTime = e.GrossTime,
                TotalCost = e.TotalCost,
                TotalCostIncEmpTaxAndSuppCharge = e.TotalCostIncEmpTaxAndSuppCharge,

                TimeDeviationCauseId = e.TimeDeviationCauseId,
                TimeDeviationCauseName = e.TimeDeviationCauseName,

                ShiftTypeId = e.ShiftTypeId,
                ShiftTypeCode = e.ShiftTypeCode,
                ShiftTypeName = e.ShiftTypeName,
                ShiftTypeDesc = e.ShiftTypeDesc,
                ShiftTypeColor = e.ShiftTypeColor,

                ShiftStatus = e.ShiftStatus,
                //ShiftStatusName = e.ShiftStatusName,
                ShiftUserStatus = e.ShiftUserStatus,
                //ShiftUserStatusName = e.ShiftUserStatusName,

                //IsPreliminary = e.IsPreliminary,
                //ExtraShift = e.ExtraShift,
                //SubstituteShift = e.SubstituteShift,

                AccountId = e.AccountId,
                AccountName = e.AccountName,
                //HasMultipleEmployeeAccountsOnDate = e.HasMultipleEmployeeAccountsOnDate,

                //NbrOfWantedInQueue = e.NbrOfWantedInQueue,
                //IamInQueue = e.IamInQueue,

                //HasSwapRequest = e.HasSwapRequest,
                //SwapShiftInfo = e.SwapShiftInfo,

                //HasShiftRequest = e.HasShiftRequest,
                //ShiftRequestAnswerType = e.ShiftRequestAnswerType,
                ApprovalTypeId = e.ApprovalTypeId,

                TimeScheduleTemplateHeadId = e.TimeScheduleTemplateHeadId,
                //TimeScheduleTemplatePeriodId = e.TimeScheduleTemplatePeriodId,
                //NbrOfWeeks = e.NbrOfWeeks,
                //DayNumber = e.DayNumber,
                //OriginalBlockId = e.OriginalBlockId,

                Link = e.Link,

                IsAbsenceRequest = e.IsAbsenceRequest,

                TimeLeisureCodeId = e.TimeLeisureCodeId,

                //StaffingNeedsRowId = e.StaffingNeedsRowId,

                //TimeScheduleEmployeePeriodId = e.TimeScheduleEmployeePeriodId,
                //Order = e.Order,
                //PlannedTime = e.PlannedTime,

                //TimeScheduleScenarioHeadId = e.TimeScheduleScenarioHeadId,
            };

            if (e?.Breaks?.Any() == true)
            {
                int breakNbr = 1;
                foreach (ShiftBreakDTO b in e.Breaks)
                {
                    switch (breakNbr)
                    {
                        case 1:
                            dayDTO.Break1Id = b.BreakId;
                            dayDTO.Break1TimeCodeId = b.TimeCodeId;
                            dayDTO.Break1StartTime = b.StartTime ?? CalendarUtility.DATETIME_0VALUE;
                            dayDTO.Break1Minutes = b.Minutes;
                            dayDTO.Break1Link = b.Link;
                            dayDTO.Break1IsPreliminary = b.IsPreliminary;
                            break;
                        case 2:
                            dayDTO.Break2Id = b.BreakId;
                            dayDTO.Break2TimeCodeId = b.TimeCodeId;
                            dayDTO.Break2StartTime = b.StartTime ?? CalendarUtility.DATETIME_0VALUE;
                            dayDTO.Break2Minutes = b.Minutes;
                            dayDTO.Break2Link = b.Link;
                            dayDTO.Break2IsPreliminary = b.IsPreliminary;
                            break;
                        case 3:
                            dayDTO.Break3Id = b.BreakId;
                            dayDTO.Break3TimeCodeId = b.TimeCodeId;
                            dayDTO.Break3StartTime = b.StartTime ?? CalendarUtility.DATETIME_0VALUE;
                            dayDTO.Break3Minutes = b.Minutes;
                            dayDTO.Break3Link = b.Link;
                            dayDTO.Break3IsPreliminary = b.IsPreliminary;
                            break;
                        case 4:
                            dayDTO.Break4Id = b.BreakId;
                            dayDTO.Break4TimeCodeId = b.TimeCodeId;
                            dayDTO.Break4StartTime = b.StartTime ?? CalendarUtility.DATETIME_0VALUE;
                            dayDTO.Break4Minutes = b.Minutes;
                            dayDTO.Break4Link = b.Link;
                            dayDTO.Break4IsPreliminary = b.IsPreliminary;
                            break;
                    }
                    breakNbr++;
                }

            }

            return dayDTO;
        }

        public static List<TimeSchedulePlanningDayDTO> ToTimeSchedulePlanningDayDTOs(this IEnumerable<ShiftDTO> l)
        {
            var dtos = new List<TimeSchedulePlanningDayDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToTimeSchedulePlanningDayDTO());
                }
            }
            return dtos;
        }

        public static List<BreakDTO> ToBreakDTOs(this List<TimeScheduleTemplateBlock> l, bool actualDate, bool includeGaps)
        {
            List<BreakDTO> breaks = new List<BreakDTO>();

            List<TimeScheduleTemplateBlock> breakBlocks = l.GetBreaks();
            foreach (TimeScheduleTemplateBlock breakBlock in breakBlocks)
            {
                breaks.Add(new BreakDTO()
                {
                    Id = breakBlock.TimeScheduleTemplateBlockId,
                    TimeCodeId = breakBlock.TimeCodeId,
                    StartTime = actualDate && breakBlock.Date.HasValue ? breakBlock.ActualStartTime.Value : breakBlock.StartTime,
                    BreakMinutes = Convert.ToInt32(breakBlock.StopTime.Subtract(breakBlock.StartTime).TotalMinutes),
                    Link = !String.IsNullOrEmpty(breakBlock.Link) ? Guid.Parse(breakBlock.Link) : (Guid?)null
                });
            }

            if (includeGaps)
            {
                List<BreakDTO> gaps = new List<BreakDTO>();

                List<TimeScheduleTemplateBlock> workBlocks = l.GetWork();
                if (workBlocks.Count > 1)
                {
                    for (int shiftNr = 1; shiftNr <= workBlocks.Count; shiftNr++)
                    {
                        TimeScheduleTemplateBlock block = l[shiftNr - 1];
                        TimeScheduleTemplateBlock prevBlock = shiftNr >= 2 ? l[shiftNr - 2] : null;
                        if (block != null && prevBlock != null)
                        {
                            int gapMinutes = Convert.ToInt32(block.StartTime.Subtract(prevBlock.StopTime).TotalMinutes);
                            if (gapMinutes > 1)
                            {
                                gaps.Add(new BreakDTO()
                                {
                                    Id = 0,
                                    TimeCodeId = 0,
                                    StartTime = actualDate && prevBlock.Date.HasValue ? prevBlock.ActualStopTime.Value : prevBlock.StopTime,
                                    BreakMinutes = gapMinutes,
                                    Link = null,
                                });
                            }
                        }
                    }
                }

                breaks.AddRange(gaps);
            }

            return breaks;
        }

        public static List<TimeScheduleTemplateBlock> Filter(this List<TimeScheduleTemplateBlock> l, DateTime date)
        {
            return l?.Where(e => e.Date == date).ToList() ?? new List<TimeScheduleTemplateBlock>();
        }

        public static List<TimeScheduleTemplateBlock> Filter(this List<TimeScheduleTemplateBlock> l, List<DateTime> dates)
        {
            return l?.Where(e => e.Date.HasValue && dates.Contains(e.Date.Value)).ToList() ?? new List<TimeScheduleTemplateBlock>();
        }

        public static List<TimeScheduleTemplateBlock> Filter(this List<TimeScheduleTemplateBlock> l, DateTime dateFrom, DateTime dateTo, int? timeDeviationCauseId = null)
        {
            return l?.Where(b => (!timeDeviationCauseId.HasValue || timeDeviationCauseId.Value == b.TimeDeviationCauseId) && CalendarUtility.IsDateInRange(b.Date.Value, dateFrom, dateTo)).ToList() ?? new List<TimeScheduleTemplateBlock>();
        }

        public static List<TimeScheduleTemplateBlock> Filter(this List<TimeScheduleTemplateBlock> l, List<TimeScheduleType> excludeTimeScheduleTypes)
        {
            if (excludeTimeScheduleTypes == null)
                return l;

            return l
                .Where(b =>
                    !b.TimeScheduleTypeId.HasValue ||
                    !excludeTimeScheduleTypes.Any(tst => tst.TimeScheduleTypeId == b.TimeScheduleTypeId))
                .ToList();
        }

        public static List<TimeScheduleTemplateBlock> FilterScenario(this List<TimeScheduleTemplateBlock> l, int? timeScheduleScenarioHeadId)
        {
            if (l.IsNullOrEmpty())
                return new List<TimeScheduleTemplateBlock>();

            if (timeScheduleScenarioHeadId.HasValue)
                return l.Where(i => i.TimeScheduleScenarioHeadId == timeScheduleScenarioHeadId.Value).ToList();
            else
                return l.Where(i => !i.TimeScheduleScenarioHeadId.HasValue).ToList();
        }

        public static List<TimeScheduleTemplateBlock> FilterScheduleType(this List<TimeScheduleTemplateBlock> l, bool includeStandBy, bool includeOnDuty)
        {
            if (l.IsNullOrEmpty())
                return new List<TimeScheduleTemplateBlock>();

            if (includeStandBy && includeOnDuty)
                return l.Where(i => i.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Schedule || i.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Standby || i.Type == (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty).ToList();
            else if (includeStandBy)
                return l.Where(i => i.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Schedule || i.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Standby).ToList();
            else if (includeOnDuty)
                return l.Where(i => i.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Schedule || i.Type == (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty).ToList();
            else
                return l.Where(i => i.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Schedule).ToList();
        }

        public static List<TimeScheduleTemplateBlock> GetScheduleBreaks(this IEnumerable<TimeScheduleTemplateBlock> l, int timeScheduleTemplatePeriodId, int employeeId, DateTime date)
        {
            return l?.Where(i => i.TimeScheduleTemplatePeriodId == timeScheduleTemplatePeriodId && i.IsBreak && i.EmployeeId == employeeId && i.Date == date).ToList() ?? new List<TimeScheduleTemplateBlock>();
        }

        public static List<TimeScheduleTemplateBlock> GetEmployeePostBreaks(this IEnumerable<TimeScheduleTemplateBlock> l, int timeScheduleTemplatePeriodId)
        {
            return l?.Where(i => i.TimeScheduleTemplatePeriodId == timeScheduleTemplatePeriodId && i.IsBreak && !i.EmployeeId.HasValue && !i.Date.HasValue && i.TimeScheduleTemplatePeriod.TimeScheduleTemplateHead.EmployeePostId.HasValue).ToList() ?? new List<TimeScheduleTemplateBlock>();
        }

        public static List<TimeScheduleTemplateBlock> GetTemplateBreaks(this IEnumerable<TimeScheduleTemplateBlock> l, int timeScheduleTemplatePeriodId)
        {
            return l?.Where(i => i.TimeScheduleTemplatePeriodId == timeScheduleTemplatePeriodId && i.IsBreak && !i.EmployeeId.HasValue && !i.Date.HasValue && !i.TimeScheduleTemplatePeriod.TimeScheduleTemplateHead.EmployeePostId.HasValue).ToList() ?? new List<TimeScheduleTemplateBlock>();
        }

        public static List<TimeScheduleTemplateBlock> GetWork(this IEnumerable<TimeScheduleTemplateBlock> l, bool onlyStandby = false, bool skipZero = false)
        {
            return l?.Where(i => !i.IsBreak && (!onlyStandby || i.IsStandby()) && (!skipZero || i.StartTime < i.StopTime)).OrderBy(i => i.StartTime).ToList() ?? new List<TimeScheduleTemplateBlock>();
        }

        public static List<TimeScheduleTemplateBlock> GetWorkWithScheduleType(this List<TimeScheduleTemplateBlock> l)
        {
            return l?.Where(i => !i.IsBreak && i.TimeScheduleTypeId.HasValue).OrderBy(i => i.StartTime).ToList() ?? new List<TimeScheduleTemplateBlock>();
        }

        public static List<TimeScheduleTemplateBlock> GetBreaks(this List<TimeScheduleTemplateBlock> l, TimeScheduleTemplateBlock shift, bool skipZero = false)
        {
            return l.GetBreaks(shift.StartTime, shift.StopTime, skipZero);
        }

        public static List<TimeScheduleTemplateBlock> GetBreaks(this List<TimeScheduleTemplateBlock> l, DateTime? startTime = null, DateTime? stopTime = null, bool skipZero = false)
        {
            var breaks = l?.Where(i => i.IsBreak && (!skipZero || i.StartTime < i.StopTime)).OrderBy(i => i.StartTime).ToList() ?? new List<TimeScheduleTemplateBlock>();
            if (startTime.HasValue)
                breaks = breaks.Where(i => i.StartTime >= startTime.Value).ToList();
            if (startTime.HasValue)
                breaks = breaks.Where(i => i.StopTime <= stopTime.Value).ToList();
            return breaks.OrderBy(i => i.StartTime).ToList();
        }

        public static List<TimeScheduleTemplateBlock> GetStandby(this List<TimeScheduleTemplateBlock> l)
        {
            return l?.Where(i => i.IsStandby()).OrderBy(i => i.StartTime).ToList() ?? new List<TimeScheduleTemplateBlock>();
        }

        public static List<TimeScheduleTemplateBlock> ExcludeStandby(this List<TimeScheduleTemplateBlock> l)
        {
            return l?.Where(i => !i.IsStandby()).OrderBy(i => i.StartTime).ToList() ?? new List<TimeScheduleTemplateBlock>();
        }

        public static List<TimeScheduleTemplateBlock> ExcludeEmployeeIds(this List<TimeScheduleTemplateBlock> l, List<int> excludeEmployeeIds)
        {
            return l?.Where(i => i.EmployeeId.HasValue && !excludeEmployeeIds.Contains(i.EmployeeId.Value)).ToList();
        }

        public static List<TimeScheduleTemplateBlock> ExcludeEmployeeId(this List<TimeScheduleTemplateBlock> l, int excludeEmployeeId)
        {
            return l?.Where(i => i.EmployeeId.HasValue && i.EmployeeId.Value != excludeEmployeeId).ToList();
        }

        public static List<TimeScheduleTemplateBlock> ExcludeHandlingMoney(this List<TimeScheduleTemplateBlock> l, List<int> shiftTypeIdsHandlingMoney)
        {
            return l?.Where(i => (i.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.NormalBreak) || (i.ShiftTypeId.HasValue && shiftTypeIdsHandlingMoney.Contains(i.ShiftTypeId.Value))).ToList();
        }

        public static List<TimeScheduleTemplateBlock> DiscardZero(this List<TimeScheduleTemplateBlock> l)
        {
            return l?.Where(i => i.StartTime < i.StopTime).ToList() ?? new List<TimeScheduleTemplateBlock>();
        }

        public static List<TimeScheduleTemplateBlock> FilterScheduleOrStandby(this List<TimeScheduleTemplateBlock> l)
        {
            return l?.Where(e => e.IsScheduleOrStandby()).ToList() ?? new List<TimeScheduleTemplateBlock>();
        }

        public static List<TimeScheduleTemplateBlock> SortByStartTime(this List<TimeScheduleTemplateBlock> l)
        {
            return l?.OrderBy(e => e.Date).ThenBy(e => e.StartTime).ThenBy(e => e.StopTime).ToList() ?? new List<TimeScheduleTemplateBlock>();
        }

        public static List<TimeScheduleTemplateBlockDTO> SplitOnBreaks(this List<TimeScheduleTemplateBlock> l, bool removeBreaks = false)
        {
            Dictionary<int?, List<TimeScheduleTemplateBlockDTO>> splitted = new Dictionary<int?, List<TimeScheduleTemplateBlockDTO>>();
            Dictionary<int?, List<TimeScheduleTemplateBlockDTO>> dict = l.ToDTOs().GroupBy(g => g.EmployeeId).ToDictionary(k => k.Key, v => v.ToList());

            foreach (var pair in dict)
            {
                var overlappingSchedule = pair.Value.GroupBy(x => x.Date.ToString() + x.StartTime.ToString() + x.StopTime.ToString()).Where(x => x.Count() > 1);
                if (overlappingSchedule.Any())
                {
                    foreach (var item in overlappingSchedule)
                    {
                        var scheduleBreaks = item.Where(x => x.IsBreak).ToList();
                        var schedules = item.Where(x => !x.IsBreak).ToList();
                        foreach (var schedule in schedules)
                        {
                            foreach (var scheduleBreak in scheduleBreaks)
                            {
                                var breakMinutes = CalendarUtility.GetOverlappingMinutes(schedule.StartTime, schedule.StopTime, scheduleBreak.StartTime, scheduleBreak.StopTime);
                                if (breakMinutes == schedule.TotalMinutes)
                                    pair.Value.Remove(schedule); //remove schedule if it is totaly overlapped by a break
                            }
                        }
                    }
                }

                var breaks = pair.Value.Where(w => w.IsBreak).ToList();
                if (breaks.Any())
                {
                    var blocks = pair.Value.Where(w => !w.IsBreak).ToList();

                    foreach (var breakBlock in breaks)
                    {
                        var overlappningBlocks = blocks.Where(w => w.Date == breakBlock.Date && CalendarUtility.GetOverlappingMinutes(w.StartTime, w.StopTime, breakBlock.StartTime, breakBlock.StopTime) > 0).ToList();

                        for (int i = 0; i < overlappningBlocks.Count; i++)
                        {
                            var block = overlappningBlocks[i];
                            var clone = block.CloneDTO();

                            if (breakBlock.StartTime > block.StartTime)
                            {
                                block.StopTime = breakBlock.StartTime;

                                if (clone.StopTime > breakBlock.StopTime)
                                {
                                    clone.StartTime = breakBlock.StopTime;
                                    blocks.Add(clone);
                                }
                            }
                            else if (breakBlock.StopTime < block.StopTime)
                            {
                                block.StartTime = breakBlock.StopTime;
                            }

                        }

                    }

                    pair.Value.Clear();
                    pair.Value.AddRange(blocks);
                    pair.Value.AddRange(breaks);
                }

                if (removeBreaks)
                {
                    var values = pair.Value.Where(w => !w.IsBreak).ToList();
                    pair.Value.Clear();
                    pair.Value.AddRange(values);
                }

                splitted.Add(pair.Key, pair.Value);
            }

            return splitted.SelectMany(s => s.Value).ToList();
        }

        public static List<TimeEmployeeScheduleDataSmallDTO> GetOverlappedBreaks(this List<TimeEmployeeScheduleDataSmallDTO> l, List<TimeEmployeeScheduleDataSmallDTO> breakitems, bool includeBreaksThatOnlyStartInScheduleBlock = false)
        {
            List<TimeEmployeeScheduleDataSmallDTO> breaks = new List<TimeEmployeeScheduleDataSmallDTO>();
            foreach (var shift in l)
            {
                breaks.AddRange(shift.GetOverlappedBreaks(breakitems, includeBreaksThatOnlyStartInScheduleBlock));
            }

            return breaks;
        }

        public static List<TimeEmployeeScheduleDataSmallDTO> GetOverlappedBreaks(this TimeEmployeeScheduleDataSmallDTO e, List<TimeEmployeeScheduleDataSmallDTO> breaks, bool includeBreaksThatOnlyStartInScheduleBlock = false)
        {
            List<TimeEmployeeScheduleDataSmallDTO> overlappedBreaks = new List<TimeEmployeeScheduleDataSmallDTO>();

            foreach (var brk in breaks.Where(x => x.IsBreak && x.EmployeeId == e.EmployeeId && x.Date == e.Date).ToList())
            {
                if (brk.StartTime >= e.StartTime && brk.StopTime <= e.StopTime)
                    overlappedBreaks.Add(brk);
                else if (includeBreaksThatOnlyStartInScheduleBlock && brk.StartTime >= e.StartTime && brk.StartTime < e.StopTime)
                    overlappedBreaks.Add(brk);
            }

            return overlappedBreaks;
        }

        public static List<PlannedAbsenceIntervalDTO> GetPlannedAbsenceIntervals(this List<TimeScheduleTemplateBlock> l)
        {
            List<PlannedAbsenceIntervalDTO> intervals = new List<PlannedAbsenceIntervalDTO>();

            foreach (var block in l.Where(w => w.Date.HasValue).OrderBy(o => o.StartTime))
            {
                if (block.TimeDeviationCauseId.HasValue)
                {
                    intervals.Add(new PlannedAbsenceIntervalDTO()
                    {
                        ActualStartTime = block.ActualStartTime.Value,
                        ActualStopTime = block.ActualStopTime.Value,
                        TimeDeviationCauseId = block.TimeDeviationCauseId.Value,
                        TimeScheduleTemplateBlocks = new List<TimeScheduleTemplateBlockDTO>() { block.ToDTO() },
                        Date = block.Date.Value,
                        EmployeeId = block.EmployeeId.Value
                    });
                }
            }

            if (intervals.Any())
            {
                foreach (var interval in intervals.OrderBy(o => o.EmployeeId).ThenBy(t => t.Date))
                {
                    if (interval.Delete)
                        continue;

                    if (intervals.Any(f => f.Date == interval.Date && f.EmployeeId == interval.EmployeeId && f.ActualStartTime == interval.ActualStopTime && f.TimeDeviationCauseId == interval.TimeDeviationCauseId))
                    {
                        var afters = intervals.Where(f => f.Date == interval.Date && f.EmployeeId == interval.EmployeeId && f.ActualStartTime >= interval.ActualStopTime && f.TimeDeviationCauseId == interval.TimeDeviationCauseId).ToList();

                        foreach (var after in afters)
                        {
                            var next = intervals.FirstOrDefault(f => f.Date == interval.Date && f.EmployeeId == interval.EmployeeId && f.ActualStartTime == interval.ActualStopTime && f.TimeDeviationCauseId == after.TimeDeviationCauseId);

                            if (next != null)
                            {
                                if (next.Delete)
                                    continue;

                                interval.ActualStopTime = next.ActualStopTime;
                                next.Delete = true;
                                interval.TimeScheduleTemplateBlocks.AddRange(next.TimeScheduleTemplateBlocks);
                            }
                        }
                    }
                }
            }

            return intervals.Where(w => !w.Delete).ToList();
        }

        public static Dictionary<DateTime, List<TimeScheduleTemplateBlock>> ByDate(this List<TimeScheduleTemplateBlock> l)
        {
            return l?
                .Where(b => b.Date.HasValue)
                .GroupBy(b => b.Date.Value)
                .ToDictionary(k => k.Key, v => v.ToList()) ?? new Dictionary<DateTime, List<TimeScheduleTemplateBlock>>();
        }

        public static Dictionary<DateTime, List<TimeSchedulePlanningDayDTO>> ByDate(this List<TimeSchedulePlanningDayDTO> l)
        {
            return l?
                .GroupBy(b => b.ActualDate)
                .ToDictionary(k => k.Key, v => v.ToList()) ?? new Dictionary<DateTime, List<TimeSchedulePlanningDayDTO>>();
        }

        public static List<WorkIntervalDTO> GetWorkIntervals(this List<TimeScheduleTemplateBlock> l, int hiddenEmployeeId, bool loadGrossNetCost = false)
        {
            List<WorkIntervalDTO> workIntervals = new List<WorkIntervalDTO>();

            if (l.IsNullOrEmpty())
                return workIntervals;

            foreach (var employeeGrouping in l.Where(i => i.Date.HasValue).GroupBy(i => i.EmployeeId))
            {
                int employeeId = employeeGrouping.Key ?? 0;
                bool isHiddenEmployee = (employeeId == hiddenEmployeeId);

                foreach (var employeeDateGrouping in employeeGrouping.GroupBy(i => i.Date))
                {
                    DateTime date = employeeDateGrouping.Key.Value;
                    List<TimeScheduleTemplateBlock> templateBlocksByEmployeeAndDate = employeeDateGrouping.ToList();
                    List<BreakDTO> breaksByEmployeeAndDate = templateBlocksByEmployeeAndDate.ToBreakDTOs(true, false);

                    foreach (TimeScheduleTemplateBlock shift in templateBlocksByEmployeeAndDate.Where(b => b.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None && b.StartTime != b.StopTime).ToList())
                    {
                        DateTime start = shift.ActualStartTime.Value;
                        DateTime stop = shift.ActualStopTime.Value;

                        bool restart;
                        do
                        {
                            restart = false;
                            foreach (BreakDTO brk in isHiddenEmployee ? breaksByEmployeeAndDate.Where(b => b.Link.ToString() == shift.Link).ToList() : breaksByEmployeeAndDate.ToList())
                            {
                                if (CalendarUtility.GetOverlappingMinutes(brk.StartTime, brk.StopTime, start, stop) > 0)
                                {
                                    if (brk.StartTime >= start)
                                    {
                                        // Break starts inside presence
                                        WorkIntervalDTO workInterval = new WorkIntervalDTO(employeeId, start, brk.StartTime, grossNetCost: loadGrossNetCost && !shift.IsOnDuty() ? GrossNetCostDTO.Create(employeeId, shift.TimeScheduleTemplateBlockId, shift.TimeScheduleTypeId, shift.TimeDeviationCauseId, date) : null);
                                        if (workInterval.TotalMinutes != 0)
                                            workIntervals.Add(workInterval);
                                        start = brk.StopTime;
                                        restart = true;
                                        break;
                                    }
                                    else if (brk.StopTime < stop)
                                    {
                                        // Break ends inside presence
                                        WorkIntervalDTO workInterval = new WorkIntervalDTO(employeeId, brk.StopTime, stop, grossNetCost: loadGrossNetCost && !shift.IsOnDuty() ? GrossNetCostDTO.Create(employeeId, shift.TimeScheduleTemplateBlockId, shift.TimeScheduleTypeId, shift.TimeDeviationCauseId, date) : null);
                                        if (workInterval.TotalMinutes != 0)
                                            workIntervals.Add(workInterval);
                                        start = stop;
                                    }
                                    else
                                    {
                                        // Break compleately overlaps presence
                                        start = stop;
                                    }
                                }
                            }
                        } while (restart);

                        if (stop > start)
                        {
                            WorkIntervalDTO workInterval = new WorkIntervalDTO(employeeId, start, stop, grossNetCost: loadGrossNetCost && !shift.IsOnDuty() ? GrossNetCostDTO.Create(employeeId, shift.TimeScheduleTemplateBlockId, shift.TimeScheduleTypeId, shift.TimeDeviationCauseId, date) : null);
                            if (workInterval.TotalMinutes != 0)
                                workIntervals.Add(workInterval);
                        }
                    }
                }
            }

            return workIntervals.OrderBy(i => i.StartTime).ThenBy(i => i.StopTime).ToList();
        }

        public static List<BreakDTO> GetGaps(this List<TimeScheduleTemplateBlock> l, bool useActualDate = false)
        {
            List<BreakDTO> gaps = new List<BreakDTO>();

            if (l.IsNullOrEmpty())
                return gaps;

            var work = l.GetWork();
            if (work.Count > 1)
            {
                for (int blockNr = 1; blockNr < work.Count; blockNr++)
                {
                    TimeScheduleTemplateBlock block = work[blockNr];
                    TimeScheduleTemplateBlock prevBlock = work[blockNr - 1];
                    int gapMinutes = Convert.ToInt32(block.StartTime.Subtract(prevBlock.StopTime).TotalMinutes);
                    if (gapMinutes > 1)
                    {
                        gaps.Add(new BreakDTO()
                        {
                            Id = 0,
                            TimeCodeId = 0,
                            StartTime = useActualDate ? CalendarUtility.MergeDateAndDefaultTime(prevBlock.Date.Value, prevBlock.StopTime) : prevBlock.StopTime,
                            BreakMinutes = gapMinutes,
                            Link = null,
                        });
                    }
                }
            }

            return gaps;
        }

        public static (DateTime scheduleIn, DateTime scheduleOut, bool isWholedayAbsence) GetScheduleInfo(this List<TimeScheduleTemplateBlock> l, DateTime date, int? timeDeviationCauseId = null)
        {
            List<TimeScheduleTemplateBlock> scheduleBlocksForDate = l.Filter(date);
            DateTime scheduleIn = scheduleBlocksForDate.GetScheduleIn();
            DateTime scheduleOut = scheduleBlocksForDate.GetScheduleOut();
            bool isWholedayAbsence = timeDeviationCauseId.HasValue && scheduleBlocksForDate.All(b => b.TimeDeviationCauseId == timeDeviationCauseId.Value);
            return (scheduleIn, scheduleOut, isWholedayAbsence);
        }

        public static TimeScheduleTemplateBlock GetTemplateBlockFromTimeBlock(this List<TimeScheduleTemplateBlock> l, TimeBlock timeBlock)
        {
            return l?.Where(i => CalendarUtility.IsNewOverlappedByCurrent(timeBlock.StartTime, timeBlock.StopTime, i.StartTime, i.StopTime)).OrderBy(i => i.StartTime).LastOrDefault();
        }

        public static TimeScheduleTemplateBlock GetScheduleInTemplateBlock(this List<TimeScheduleTemplateBlock> l, List<int> timeScheduleTypeIdsIsNotScheduleTime = null)
        {
            return l?.Where(e => !e.IsBreak && e.IsScheduleTime(timeScheduleTypeIdsIsNotScheduleTime)).OrderBy(e => e.StartTime).FirstOrDefault();
        }

        public static TimeScheduleTemplateBlock GetMatchingScheduleBlock(this List<TimeScheduleTemplateBlock> l, TimeBlock timeBlock, bool acceptConnectedOutsideSchedule)
        {
            if (l.IsNullOrEmpty() || timeBlock == null)
                return null;

            bool isMultipleDates = l.Select(b => b.Date).Distinct().Count() > 1;
            List<TimeScheduleTemplateBlock> scheduleBlocksForDate = isMultipleDates ? l.Where(scheduleBlock => scheduleBlock.Date == timeBlock.TimeBlockDate?.Date).ToList() : l.ToList();
            if (scheduleBlocksForDate.IsNullOrEmpty())
                return null;

            TimeScheduleTemplateBlock matchingScheduleBlock =
                scheduleBlocksForDate.FirstOrDefault(scheduleBlock => scheduleBlock.StartTime == timeBlock.StartTime && scheduleBlock.StopTime == timeBlock.StopTime) ??
                scheduleBlocksForDate.FirstOrDefault(scheduleBlock => scheduleBlock.StartTime <= timeBlock.StartTime && scheduleBlock.StopTime >= timeBlock.StopTime); //TimeScheduleTemplateBlock that overlaps TimeBlock

            if (matchingScheduleBlock == null && acceptConnectedOutsideSchedule)
            {
                //TimeBlocks before schedule that stops at schedule in
                DateTime scheduleIn = scheduleBlocksForDate.GetScheduleIn();
                if (timeBlock.StopTime == scheduleIn)
                    matchingScheduleBlock = scheduleBlocksForDate.FirstOrDefault(scheduleBlock => scheduleBlock.StartTime == scheduleIn);

                if (matchingScheduleBlock == null)
                {
                    //TimeBlocks after schedule that starts at schedule out
                    DateTime scheduleOut = scheduleBlocksForDate.GetScheduleOut();
                    if (timeBlock.StartTime == scheduleOut)
                        matchingScheduleBlock = scheduleBlocksForDate.FirstOrDefault(scheduleBlock => scheduleBlock.StopTime == scheduleOut);
                }
            }
            return matchingScheduleBlock;
        }

        public static TimeScheduleTemplateBlock GetClosest(this List<TimeScheduleTemplateBlock> l, DateTime start, DateTime stop)
        {
            if (l.IsNullOrEmpty())
                return null;

            // First check if this block is surrounded by a schedule block
            TimeScheduleTemplateBlock timeScheduleTemplateBlock = l.FirstOrDefault(s => !s.IsBreak && s.StartTime <= start && s.StopTime >= stop);

            // If no schedule block found, check forward
            if (timeScheduleTemplateBlock == null)
                timeScheduleTemplateBlock = l.Where(s => !s.IsBreak && s.StartTime >= start).OrderBy(s => s.StartTime).FirstOrDefault();

            // If no schedule block found, check backwards
            if (timeScheduleTemplateBlock == null)
                timeScheduleTemplateBlock = l.Where(s => !s.IsBreak && s.StartTime < start).OrderByDescending(s => s.StartTime).FirstOrDefault();

            return timeScheduleTemplateBlock;
        }

        public static TimeScheduleTemplateBlock GetScheduleOutTemplateBlock(this List<TimeScheduleTemplateBlock> l, List<int> timeScheduleTypeIdsIsNotScheduleTime = null)
        {
            return l?.Where(e => !e.IsBreak && e.IsScheduleTime(timeScheduleTypeIdsIsNotScheduleTime)).OrderByDescending(e => e.StopTime).FirstOrDefault();
        }

        public static TimeScheduleTemplateBlock GetFirst(this IEnumerable<TimeScheduleTemplateBlock> l)
        {
            return l?.OrderBy(i => i.Date).ThenBy(i => i.StartTime).FirstOrDefault();
        }

        public static TimeScheduleTemplateBlock GetFirstFromTime(this IEnumerable<TimeScheduleTemplateBlock> l, DateTime time)
        {
            return l?.Where(e => e.StartTime <= time && e.StopTime >= time).OrderBy(e => e.StartTime).FirstOrDefault();
        }

        public static TimeScheduleTemplateBlock GetLast(this IEnumerable<TimeScheduleTemplateBlock> l)
        {
            return l?.OrderBy(i => i.Date).ThenBy(i => i.StartTime).LastOrDefault();
        }

        public static TimeScheduleTemplateBlock GetPrev(this IEnumerable<TimeScheduleTemplateBlock> l, TimeScheduleTemplateBlock e)
        {
            return l?.Where(i => i.StopTime <= e.StartTime && !i.IsBreak).OrderByDescending(i => i.StopTime).FirstOrDefault();
        }

        public static TimeScheduleTemplateBlock GetPrev(this IEnumerable<TimeScheduleTemplateBlock> l, DateTime time)
        {
            return l?.Where(i => i.StopTime <= time && !i.IsBreak).OrderByDescending(i => i.StopTime).FirstOrDefault();
        }

        public static TimeScheduleTemplateBlock GetNext(this IEnumerable<TimeScheduleTemplateBlock> l, TimeScheduleTemplateBlock e)
        {
            return l?.Where(i => i.StartTime >= e.StopTime && !i.IsBreak).OrderBy(i => i.StartTime).FirstOrDefault();
        }

        public static TimeScheduleTemplateBlock GetNext(this IEnumerable<TimeScheduleTemplateBlock> l, DateTime time)
        {
            return l?.Where(i => i.StartTime >= time && !i.IsBreak).OrderBy(i => i.StartTime).FirstOrDefault();
        }

        public static TimeScheduleTemplateBlock Clone(this TimeScheduleTemplateBlock e, string username)
        {
            var newBlock = new TimeScheduleTemplateBlock()
            {
                OldTimeScheduleTemplateBlockId = e.TimeScheduleTemplateBlockId,
                StartTime = e.StartTime,
                StopTime = e.StopTime,
                Date = e.Date,
                Description = e.Description,
                BreakNumber = e.BreakNumber,
                BreakType = e.BreakType,
                IsPreliminary = e.IsPreliminary,
                TimeDeviationCauseStatus = e.TimeDeviationCauseStatus,
                ShiftStatus = e.ShiftStatus,
                ShiftUserStatus = e.ShiftUserStatus,
                NbrOfWantedInQueue = e.NbrOfWantedInQueue,
                NbrOfSuggestionsInQueue = e.NbrOfSuggestionsInQueue,
                Link = e.Link,

                //Set FK
                EmployeeId = e.EmployeeId,
                TimeCodeId = e.TimeCodeId,
                TimeHalfdayId = e.TimeHalfdayId,
                TimeScheduleTypeId = e.TimeScheduleTypeId,
                ShiftTypeId = e.ShiftTypeId,
                AccountId = e.AccountId,
                EmployeeScheduleId = e.EmployeeScheduleId,
                TimeDeviationCauseId = e.TimeDeviationCauseId,
                EmployeeChildId = e.EmployeeChildId,
                RecalculateTimeRecordId = e.RecalculateTimeRecordId,

                //Set references
                TimeScheduleEmployeePeriodId = e.TimeScheduleEmployeePeriodId,
                TimeScheduleTemplatePeriodId = e.TimeScheduleTemplatePeriodId,
                ExtraShift = e.ExtraShift,

                //Created
                Created = DateTime.Now,
                CreatedBy = username,
            };

            if (e.AccountInternal != null)
            {
                newBlock.Account = null;
                foreach (var ai in e.AccountInternal)
                {
                    newBlock.AccountInternal.Add(ai);
                }
            }

            return newBlock;
        }

        public static List<int> GetAccountInternalIds(this List<TimeScheduleTemplateBlock> l)
        {
            List<int> accountIds = new List<int>();
            foreach (TimeScheduleTemplateBlock e in l)
            {
                accountIds.AddRange(e.AccountInternal.Select(i => i.AccountId));
            }
            return accountIds.Distinct().ToList();
        }

        public static List<int> GetAccountIdForResetingAccounting(this TimeScheduleTemplateBlock block, List<EmployeeAccount> employeeAccounts, ShiftType shiftType, int defaultEmployeeAccountDimEmployeeAccountDimId, List<Account> accounts, List<AccountDim> accountDims)
        {
            return block.GetAccountAccountInternalForResetingAccounting(employeeAccounts, shiftType, defaultEmployeeAccountDimEmployeeAccountDimId, accounts, accountDims).Select(s => s.AccountId).ToList();
        }

        public static string GetAccountIdsIncludingAccountIdOnBlockString(this TimeScheduleTemplateBlock block)
        {
            if (block.IsBreak)
                return "";

            if (block.AccountIdsIncludingAccountIdOnBlockString != null)
                return block.AccountIdsIncludingAccountIdOnBlockString;

            var ids = block.GetAccountIdsIncludingAccountIdOnBlock();

            block.AccountIdsIncludingAccountIdOnBlockString = ids.JoinToString("_");

            return block.AccountIdsIncludingAccountIdOnBlockString;

        }

        public static List<int> GetAccountIdsIncludingAccountIdOnBlock(this TimeScheduleTemplateBlock block)
        {
            if (block.AccountIdsIncludingAccountIdOnBlock != null)
                return block.AccountIdsIncludingAccountIdOnBlock;

            List<int> resultAccountIds = new List<int>();

            if (block.AccountInternal != null)
                resultAccountIds.AddRange(block.AccountInternal.Select(s => s.AccountId));

            if (block.AccountId.HasValue)
                resultAccountIds.Add(block.AccountId.Value);

            block.AccountIdsIncludingAccountIdOnBlock = resultAccountIds.Distinct().OrderBy(o => o).ToList();

            return block.AccountIdsIncludingAccountIdOnBlock;
        }

        public static List<AccountInternal> GetAccountAccountInternalForResetingAccounting(this TimeScheduleTemplateBlock block, List<EmployeeAccount> employeeAccounts, ShiftType shiftType, int defaultEmployeeAccountDimEmployeeAccountDimId, List<Account> accounts, List<AccountDim> accountDims)
        {
            List<AccountInternal> accountInternals = new List<AccountInternal>();

            if (block == null)
                return accountInternals;

            var account = block.AccountId.HasValue && accounts != null ? accounts.FirstOrDefault(w => w.AccountId == block.AccountId) : null;
            if (account != null)
            {
                int dimId = account.AccountDimId;
                accountInternals.Add(account.AccountInternal);

                if (!shiftType.AccountInternal.IsLoaded)
                {
                    shiftType.AccountInternal.Load();
                    Util.DataProjectLogCollector.LogLoadedEntityInExtension("TimeScheduletemplateBlock.cs shiftType.AccountInternal");
                }

                foreach (var ai in shiftType.AccountInternal)
                {
                    if (ai.Account.AccountDimId != dimId)
                        accountInternals.Add(ai);
                }
            }
            else if (employeeAccounts != null)
            {
                var accountsForDate = employeeAccounts.GetEmployeeAccounts(block.Date);
                if (!accountsForDate.IsNullOrEmpty())
                {
                    bool addedFromAccountsForDate = false;
                    foreach (var a in accountsForDate)
                    {
                        if (a.Account.AccountDimId == defaultEmployeeAccountDimEmployeeAccountDimId)
                        {
                            addedFromAccountsForDate = true;
                            accountInternals.Add(a.Account.AccountInternal);
                        }
                    }

                    if (!shiftType.AccountInternal.IsLoaded)
                    {
                        shiftType.AccountInternal.Load();
                        Util.DataProjectLogCollector.LogLoadedEntityInExtension("TimeScheduletemplateBlock.cs shiftType.AccountInternal");
                    }

                    foreach (var ai in shiftType.AccountInternal)
                    {
                        if (!addedFromAccountsForDate || ai.Account.AccountDimId != defaultEmployeeAccountDimEmployeeAccountDimId)
                            accountInternals.Add(ai);
                    }
                }
            }

            if (!accountInternals.Any())
                accountInternals = shiftType.AccountInternal.ToList();

            int count = 2;
            int i = 0;

            while (i < count && accountInternals.Count < accountDims.Count(w => w.AccountDimNr != 1))
            {
                foreach (var dim in accountDims.Where(w => w.AccountDimNr != 1))
                {
                    if (!accountInternals.Select(a => a.Account).Any(a => a.AccountDimId == dim.AccountDimId))
                    {
                        List<AccountInternal> parentAccountInternals = new List<AccountInternal>();

                        var accOnDim = accounts.Where(w => w.AccountDimId == dim.AccountDimId);
                        foreach (var acc in accountInternals.Select(s => s.Account).Where(w => w.ParentAccountId.HasValue))
                        {
                            var parentOnDim = accOnDim.FirstOrDefault(f => f.AccountId == acc.ParentAccountId);
                            if (parentOnDim != null && !accountInternals.Any(a => a.AccountId == parentOnDim.AccountId) && !parentAccountInternals.Any(a => a.AccountId == parentOnDim.AccountId))
                                parentAccountInternals.Add(parentOnDim.AccountInternal);
                        }
                        accountInternals.AddRange(parentAccountInternals);
                    }
                }
                i++;
            }
            return accountInternals;
        }

        public static TimeScheduleTypeDTO GetTimeScheduleType(this TimeScheduleTemplateBlock e, List<TimeScheduleTypeDTO> timeScheduleTypes)
        {
            if (e?.TimeScheduleTypeId == null)
                return null;
            return timeScheduleTypes?.FirstOrDefault(tst => tst.TimeScheduleTypeId == e.TimeScheduleTypeId.Value);
        }

        public static TimeBlock GetAbsenceTimeBlockOverlappingScheduleBlock(this TimeScheduleTemplateBlockDTO e, IEnumerable<TimeBlock> absenceTimeBlocks)
        {
            if (e == null || absenceTimeBlocks.IsNullOrEmpty())
                return null;

            foreach (TimeBlock absenceTimeBlock in absenceTimeBlocks)
            {
                if (e.IsScheduleBlockOverlappedTimeBlock(absenceTimeBlock))
                    return absenceTimeBlock;
            }

            return null;
        }

        public static List<DateTime> GetDates(this IEnumerable<TimeScheduleTemplateBlock> l)
        {
            return l?.Where(e => e.Date.HasValue).Select(e => e.Date.Value).Distinct().ToList() ?? new List<DateTime>();
        }

        public static DateTime GetScheduleIn(this List<TimeScheduleTemplateBlock> l, List<int> timeScheduleTypeIdsIsNotScheduleTime = null, bool actualDate = false)
        {
            return l.GetScheduleIn(out int _, timeScheduleTypeIdsIsNotScheduleTime, actualDate);
        }

        public static DateTime GetScheduleIn(this List<TimeScheduleTemplateBlock> l, out int timeScheduleTemplatePeriodId, List<int> timeScheduleTypeIdsIsNotScheduleTime = null, bool actualDate = false)
        {
            timeScheduleTemplatePeriodId = 0;

            TimeScheduleTemplateBlock scheduleInBlock = l.GetScheduleInTemplateBlock(timeScheduleTypeIdsIsNotScheduleTime);
            if (scheduleInBlock == null)
                return CalendarUtility.DATETIME_DEFAULT;

            if (scheduleInBlock.TimeScheduleTemplatePeriodId.HasValue)
                timeScheduleTemplatePeriodId = scheduleInBlock.TimeScheduleTemplatePeriodId.Value;

            return actualDate && scheduleInBlock.Date.HasValue ? scheduleInBlock.ActualStartTime.Value : scheduleInBlock.StartTime;
        }

        public static DateTime? GetScheduleInNullable(this List<TimeScheduleTemplateBlock> l, List<int> timeScheduleTypeIdsIsNotScheduleTime = null)
        {
            TimeScheduleTemplateBlock scheduleInBlock = l.GetScheduleInTemplateBlock(timeScheduleTypeIdsIsNotScheduleTime);
            if (scheduleInBlock == null || scheduleInBlock.IsZero)
                return null;
            return scheduleInBlock.StartTime;
        }

        public static DateTime GetScheduleOut(this List<TimeScheduleTemplateBlock> l, List<int> timeScheduleTypeIdsIsNotScheduleTime = null, bool actualDate = false)
        {
            return l.GetScheduleOut(out int _, timeScheduleTypeIdsIsNotScheduleTime, actualDate);
        }

        public static DateTime GetScheduleOut(this List<TimeScheduleTemplateBlock> l, out int timeScheduleTemplatePeriodId, List<int> timeScheduleTypeIdsIsNotScheduleTime = null, bool actualDate = false)
        {
            timeScheduleTemplatePeriodId = 0;

            TimeScheduleTemplateBlock scheduleOutBlock = l.GetScheduleOutTemplateBlock(timeScheduleTypeIdsIsNotScheduleTime);
            if (scheduleOutBlock == null)
                return CalendarUtility.DATETIME_DEFAULT;

            if (scheduleOutBlock.TimeScheduleTemplatePeriodId.HasValue)
                timeScheduleTemplatePeriodId = scheduleOutBlock.TimeScheduleTemplatePeriodId.Value;

            return actualDate && scheduleOutBlock.Date.HasValue ? scheduleOutBlock.ActualStopTime.Value : scheduleOutBlock.StopTime;
        }

        public static DateTime? GetScheduleOutNullable(this List<TimeScheduleTemplateBlock> l, List<int> timeScheduleTypeIdsIsNotScheduleTime = null)
        {
            TimeScheduleTemplateBlock scheduleOutBlock = l.GetScheduleOutTemplateBlock(timeScheduleTypeIdsIsNotScheduleTime);
            if (scheduleOutBlock == null || scheduleOutBlock.IsZero)
                return null;
            return scheduleOutBlock.StopTime;
        }

        public static DateTime? GetStartDate(this List<TimeScheduleTemplateBlock> l)
        {
            return l?.Where(i => i.Date.HasValue).Min(i => i.Date.Value);
        }

        public static DateTime? GetStopDate(this List<TimeScheduleTemplateBlock> l)
        {
            return l?.Where(i => i.Date.HasValue).Max(i => i.Date.Value);
        }

        public static DateTime GetTemplateBlockScheduleTypeStopTime(this List<TimeScheduleTemplateBlock> l, DateTime timeBlockStartTime, DateTime timeBlockStopTime, DateTime scheduleOut, bool useStartIfLastBlock)
        {
            if (l.IsNullOrEmpty())
                return timeBlockStopTime;

            DateTime scheduleTypeStopTime = timeBlockStopTime;

            foreach (TimeScheduleTemplateBlock templateBlock in l.GetWorkWithScheduleType())
            {
                if (CalendarUtility.IsDatesOverlapping(templateBlock.StartTime, templateBlock.StopTime, timeBlockStartTime, timeBlockStopTime))
                {
                    if (useStartIfLastBlock && timeBlockStopTime == scheduleOut)
                    {
                        if (timeBlockStartTime < templateBlock.StartTime)
                            scheduleTypeStopTime = templateBlock.StartTime;
                    }
                    else
                    {
                        if (timeBlockStopTime > templateBlock.StopTime)
                            scheduleTypeStopTime = templateBlock.StopTime;
                    }
                    break;
                }
            }

            return scheduleTypeStopTime;
        }

        public static int GetMinutes(this List<TimeScheduleTemplateBlock> l)
        {
            var timeSpan = new TimeSpan();
            foreach (var item in l)
            {
                timeSpan = timeSpan.Add(item.StopTime.Subtract(item.StartTime));
            }
            return (int)timeSpan.TotalMinutes;
        }

        public static int GetWorkTimeForMultipleDates(this List<TimeScheduleTemplateBlock> l)
        {
            int workMinutes = 0;
            foreach (var employeeGroup in l.Where(w => !w.IsBreak).GroupBy(g => g.EmployeeId))
            {
                foreach (var dateGroup in employeeGroup.GroupBy(g => g.Date))
                {
                    workMinutes += dateGroup.ToList().GetMinutes();
                }
            }

            workMinutes -= l.GetBreakMinutesForMultipleDates();

            return workMinutes;
        }

        public static int GetBreakMinutesForMultipleDates(this List<TimeScheduleTemplateBlock> l)
        {
            int breakMinutes = 0;
            foreach (var employeeGroup in l.GroupBy(g => g.EmployeeId))
            {
                foreach (var dateGroup in employeeGroup.GroupBy(g => g.Date))
                {
                    breakMinutes += dateGroup.ToList().GetBreakMinutes();
                }
            }

            return breakMinutes;
        }

        public static int GetBreakMinutes(this List<TimeScheduleTemplateBlock> l)
        {
            return l.GetBreaks().GetMinutes();
        }

        public static int GetBreakTimeWithinShift(this List<TimeScheduleTemplateBlock> l, DateTime scheduleStart, DateTime scheduleStop)
        {
            int breakMinutes = 0;
            foreach (var brk in l.Where(brk => brk.StartTime < brk.StopTime))
            {
                breakMinutes += (int)CalendarUtility.GetNewTimeInCurrent(scheduleStart, scheduleStop, brk.StartTime, brk.StopTime).TotalMinutes;
            }
            return breakMinutes;
        }

        public static int GetWorkMinutes(this List<TimeScheduleTemplateBlock> l)
        {
            return l.GetWork().GetMinutes() - l.GetBreakMinutes();
        }

        public static int GetWorkMinutes(this List<TimeScheduleTemplateBlock> l, List<TimeScheduleTypeDTO> scheduleTypes)
        {
            int totalWorkAndBreakMinutes = 0;
            int totalBreakMinutes = 0;
            foreach (TimeScheduleTemplateBlock templateBlock in l.Where(x => !x.IsBreak))
            {
                if (templateBlock.TimeScheduleTypeId.HasValue)
                {
                    TimeScheduleTypeDTO scheduleType = scheduleTypes.FirstOrDefault(st => st.TimeScheduleTypeId == templateBlock.TimeScheduleTypeId.Value);
                    if (scheduleType != null && scheduleType.IsNotScheduleTime)
                        continue;
                }

                int workMinutes = (int)(templateBlock.StopTime - templateBlock.StartTime).TotalMinutes;
                totalWorkAndBreakMinutes += workMinutes;

                int dayBreakMinutes = 0;
                List<TimeScheduleTemplateBlock> templateBlockBreaks = new List<TimeScheduleTemplateBlock>();
                if (templateBlock.TimeScheduleEmployeePeriodId.HasValue)
                {
                    templateBlockBreaks = l.Where(tb => tb.IsBreak && tb.TimeScheduleEmployeePeriodId.HasValue && tb.TimeScheduleEmployeePeriodId.Value == templateBlock.TimeScheduleEmployeePeriodId.Value).ToList();
                    foreach (TimeScheduleTemplateBlock templateBlockBreak in templateBlockBreaks)
                    {
                        int breakMinutes = (int)CalendarUtility.GetNewTimeInCurrent(templateBlock.StartTime, templateBlock.StopTime, templateBlockBreak.StartTime, templateBlockBreak.StopTime).TotalMinutes;
                        dayBreakMinutes += breakMinutes;
                        totalBreakMinutes += breakMinutes;
                    }
                }

                int factorMinutes = templateBlock.GetTimeScheduleTypeFactorsWithinShift(templateBlockBreaks, scheduleTypes);
                totalWorkAndBreakMinutes += factorMinutes;
            }

            return totalWorkAndBreakMinutes - totalBreakMinutes;
        }

        public static int GetTotalMinutes(this TimeScheduleTemplateBlock e, List<TimeScheduleTemplateBlock> blocksOnDate, List<TimeScheduleTypeDTO> timeScheduleTypes, bool addFactorMinutes = false)
        {
            List<TimeScheduleTemplateBlock> breaksOnDate = blocksOnDate.GetBreaks(e.StartTime, e.StopTime);
            int totalMinutes = e.TotalMinutes - breaksOnDate.GetMinutes();
            if (addFactorMinutes)
                totalMinutes += e.GetTimeScheduleTypeFactorsWithinShift(breaksOnDate, timeScheduleTypes);
            return totalMinutes;
        }

        public static int GetTimeScheduleTypeFactorsWithinShift(this TimeScheduleTemplateBlock e, List<TimeScheduleTemplateBlock> breaksOnDate, List<TimeScheduleTypeDTO> scheduleTypes)
        {
            int minutes = 0;

            TimeSpan factorTime = new TimeSpan();
            if (e.TimeScheduleTypeId.HasValue)
            {
                List<TimeScheduleTypeFactorDTO> factors = scheduleTypes?.FirstOrDefault(tst => tst.TimeScheduleTypeId == e.TimeScheduleTypeId.Value)?.Factors;
                if (factors != null)
                {
                    foreach (TimeScheduleTypeFactorDTO factor in factors)
                    {
                        double factorMinutes = 0;

                        if (CalendarUtility.IsNewOverlappedByCurrent(factor.FromTime, factor.ToTime, e.StartTime, e.StopTime))
                        {
                            // Factor is completely overlapped by a presence shift
                            factorMinutes = (factor.ToTime - factor.FromTime).TotalMinutes;
                            // Reduce time with the break time that overlaps
                            foreach (TimeScheduleTemplateBlock brk in breaksOnDate)
                            {
                                factorMinutes -= (int)CalendarUtility.GetNewTimeInCurrent(factor.FromTime, factor.ToTime, brk.StartTime, brk.StopTime).TotalMinutes;
                            }
                        }
                        else if (CalendarUtility.IsNewOverlappingCurrentStart(factor.FromTime, factor.ToTime, e.StartTime, e.StopTime))
                        {
                            // Factor end intersects with a presence shift
                            factorMinutes = (factor.ToTime - e.StartTime).TotalMinutes;
                            // Reduce time with the break time that overlaps
                            foreach (TimeScheduleTemplateBlock brk in breaksOnDate)
                            {
                                factorMinutes -= (int)CalendarUtility.GetNewTimeInCurrent(e.StartTime, factor.ToTime, brk.StartTime, brk.StopTime).TotalMinutes;
                            }
                        }
                        else if (CalendarUtility.IsNewOverlappingCurrentStop(factor.FromTime, factor.ToTime, e.StartTime, e.StopTime))
                        {
                            // Factor start intersects with a presence shift
                            factorMinutes = (e.StopTime - factor.FromTime).TotalMinutes;
                            // Reduce time with the break time that overlaps
                            foreach (TimeScheduleTemplateBlock brk in breaksOnDate)
                            {
                                factorMinutes -= (int)CalendarUtility.GetNewTimeInCurrent(factor.FromTime, e.StopTime, brk.StartTime, brk.StopTime).TotalMinutes;
                            }
                        }

                        factorTime = factorTime.Add(TimeSpan.FromMinutes(factorMinutes * (double)(factor.Factor - 1)));
                    }
                }
            }

            minutes += (int)factorTime.TotalMinutes;
            return minutes;
        }

        public static int GetScheduleInMinutes(this List<TimeScheduleTemplateBlock> l, List<int> timeScheduleTypeIdsIsNotScheduleTime = null)
        {
            TimeScheduleTemplateBlock e = l.GetScheduleInTemplateBlock(timeScheduleTypeIdsIsNotScheduleTime);
            return e != null ? CalendarUtility.TimeToMinutes(e.StartTime) : 0;
        }

        public static int GetScheduleOutMinutes(this List<TimeScheduleTemplateBlock> l, List<int> timeScheduleTypeIdsIsNotScheduleTime = null)
        {
            TimeScheduleTemplateBlock e = l.GetScheduleOutTemplateBlock(timeScheduleTypeIdsIsNotScheduleTime);
            return e != null ? CalendarUtility.TimeToMinutes(e.StopTime) : 0;
        }

        public static bool IsScheduleBlockSurroundedByAbsenceTimeBlock(this TimeScheduleTemplateBlock e, IEnumerable<TimeBlock> absenceTimeBlocks)
        {
            if (absenceTimeBlocks.IsNullOrEmpty() || e == null)
                return false;

            //Check if overlapped completely by absence TimeBlock
            foreach (TimeBlock absenceTimeBlock in absenceTimeBlocks)
            {
                if (e.IsScheduleBlockOverlappedTimeBlock(absenceTimeBlock))
                    return true;
            }

            //Check if break is in hole between two absence Timeblocks
            if (absenceTimeBlocks.Any(i => i.StopTime == e.StartTime) && absenceTimeBlocks.Any(i => i.StartTime == e.StopTime))
                return true;

            return false;
        }

        public static bool IsBetweenSchedule(this List<TimeScheduleTemplateBlock> l, DateTime time)
        {
            List<BreakDTO> gaps = l.GetGaps();
            return gaps.Any(x => x.StartTime <= time && time <= x.StopTime);
        }

        public static bool IsStartAfterScheduleHole(this TimeScheduleTemplateBlock e, List<TimeScheduleTemplateBlock> l)
        {
            var prev = l.GetPrev(e);
            return prev != null && e.StartTime.Subtract(prev.StopTime).TotalMinutes > 0;
        }

        public static bool IsStartAfterScheduleHole(this List<TimeScheduleTemplateBlock> l, DateTime time, out TimeScheduleTemplateBlock prevScheduleBlock)
        {
            prevScheduleBlock = l.GetPrev(time);
            TimeScheduleTemplateBlock nextScheduleBlock = prevScheduleBlock != null ? l.GetNext(prevScheduleBlock) : null;
            if (prevScheduleBlock != null && nextScheduleBlock != null && prevScheduleBlock.StopTime < nextScheduleBlock.StartTime)
            {
                return time.Subtract(prevScheduleBlock.StopTime).TotalMinutes > 0;
            }
            else
            {
                prevScheduleBlock = null;
                return false;
            }
        }

        public static bool IsStopBeforeScheduleHole(this TimeScheduleTemplateBlock e, List<TimeScheduleTemplateBlock> l)
        {
            var next = l.GetNext(e);
            return next != null && next.StartTime.Subtract(e.StopTime).TotalMinutes > 0;
        }

        public static bool IsStopBeforeScheduleHole(this List<TimeScheduleTemplateBlock> l, DateTime time, out TimeScheduleTemplateBlock nextScheduleBlock)
        {
            nextScheduleBlock = l.GetNext(time);
            TimeScheduleTemplateBlock prevScheduleBlock = nextScheduleBlock != null ? l.GetPrev(nextScheduleBlock) : null;
            if (prevScheduleBlock != null && prevScheduleBlock.StopTime < nextScheduleBlock.StartTime)
            {
                return nextScheduleBlock.StartTime.Subtract(time).TotalMinutes > 0;
            }
            else
            {
                nextScheduleBlock = null;
                return false;
            }
        }

        public static bool IsBreakSurroundedByAbsence(this TimeScheduleTemplateBlock e, List<TimeScheduleTemplateBlock> l)
        {
            if (e == null || !e.IsBreak)
                return false;

            var work = l.GetWork();
            var before = work.Where(i => i.StartTime <= e.StartTime).OrderByDescending(i => i.StartTime).FirstOrDefault();
            var after = work.Where(i => i.StopTime >= e.StopTime).OrderBy(i => i.StopTime).FirstOrDefault();
            if (before == null && after == null)
                return false;
            return (before == null || before.TimeDeviationCauseId.HasValue) && (after == null || after.TimeDeviationCauseId.HasValue);
        }

        public static bool IsZeroBlock(this TimeScheduleTemplateBlock e)
        {
            return e.StartTime == e.StopTime && e.StartTime.Hour == 0 && e.StartTime.Minute == 0;
        }

        public static bool IsBreak(this TimeScheduleTemplateBlock e)
        {
            return
                (e.BreakType > (int)SoeTimeScheduleTemplateBlockBreakType.None) ||
                (e.TimeCode != null && e.TimeCode.Type == (int)SoeTimeCodeType.Break) ||
                (e.BreakNumber > 0); //Should be removed later
        }

        public static bool IsScheduledAbsenceAroundBreak(this TimeScheduleTemplateBlock scheduleAbsence, TimeScheduleTemplateBlock scheduleBreak, DateTime scheduleIn, DateTime scheduleOut)
        {
            return
                scheduleAbsence != null &&
                scheduleBreak != null &&
                scheduleAbsence.TimeDeviationCauseId.HasValue &&
                scheduleAbsence.StartTime != scheduleIn &&
                scheduleAbsence.StartTime < scheduleBreak.StartTime &&
                scheduleAbsence.StopTime != scheduleOut &&
                scheduleAbsence.StopTime > scheduleBreak.StopTime;
        }

        public static bool IsScheduleTime(this TimeScheduleTemplateBlock e, List<int> timeScheduleTypeIdsIsNotScheduleTime)
        {
            if (!e.IsScheduleOrStandby())
                return false;
            if (!e.TimeScheduleTypeId.HasValue)
                return true;
            if (timeScheduleTypeIdsIsNotScheduleTime.IsNullOrEmpty())
                return true;
            if (!timeScheduleTypeIdsIsNotScheduleTime.Contains(e.TimeScheduleTypeId.Value))
                return true;

            return false;
        }

        public static bool IsNotScheduleTime(this TimeScheduleTemplateBlock e, List<TimeScheduleTypeDTO> timeScheduleTypes)
        {
            return e.GetTimeScheduleType(timeScheduleTypes)?.IsNotScheduleTime ?? false;
        }

        public static bool IsScheduleOrStandby(this TimeScheduleTemplateBlock e)
        {
            return e != null && (e.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Schedule || e.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Standby);
        }

        public static bool IsPartTimeAbsence(this List<TimeScheduleTemplateBlock> l)
        {
            return l.ContainsAbsence() && l.Count(x => x.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None && x.TimeDeviationCauseId.HasValue) < l.Count(x => x.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None);
        }

        public static bool IsWholeDayAbsence(this List<TimeScheduleTemplateBlock> l)
        {
            return l != null && l.Count(x => x.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None && x.TimeDeviationCauseId.HasValue) == l.Count(x => x.BreakType == (int)SoeTimeScheduleTemplateBlockBreakType.None);
        }

        public static bool HasAccountInternals(this TimeScheduleTemplateBlock e)
        {
            return e.AccountInternal != null && e.AccountInternal.Count > 0;
        }

        public static bool ContainsStandby(this List<TimeScheduleTemplateBlock> l)
        {
            return l?.Any(i => i.IsStandby()) ?? false;
        }

        public static bool ContainsAbsence(this List<TimeScheduleTemplateBlock> l)
        {
            return l?.Any(i => i.TimeDeviationCauseId.HasValue) ?? false;
        }

        public static bool TrySplitByAccount(this List<TimeScheduleTemplateBlock> l, bool onlyBreakEvaluationCompliant, out Dictionary<int, List<TimeScheduleTemplateBlock>> groupDict)
        {
            groupDict = null;

            if (l.IsNullOrEmpty())
                return false;

            //all shift must have account
            var work = l.GetWork();
            if (work.Any(i => !i.AccountId.HasValue))
                return false;

            //must be more than one account on day
            var workByAccount = work.GroupBy(i => i.AccountId.Value).ToList();
            if (workByAccount.Count <= 1)
                return false;

            //split
            groupDict = new Dictionary<int, List<TimeScheduleTemplateBlock>>();
            foreach (var workByAccountGroup in workByAccount)
            {
                var workByAccountGroupSorted = workByAccountGroup.OrderBy(i => i.StartTime);
                DateTime? workByAcountStartTime = workByAccountGroupSorted.FirstOrDefault()?.StartTime;
                DateTime? workByAcountStopTime = workByAccountGroupSorted.LastOrDefault()?.StopTime;
                var breaksByAccount = l.GetBreaks(workByAcountStartTime, workByAcountStopTime).ToList();

                //validate that total break minutes can be fitted after last break, but before schedule for group ends
                if (onlyBreakEvaluationCompliant && breaksByAccount.Any())
                {
                    DateTime? lastBreakStopTime = breaksByAccount.LastOrDefault()?.StopTime;
                    if (lastBreakStopTime.HasValue && lastBreakStopTime.Value.AddMinutes(breaksByAccount.GetMinutes()) > workByAcountStopTime)
                    {
                        groupDict = null;
                        return false;
                    }
                }

                if (!groupDict.ContainsKey(workByAccountGroup.Key))
                    groupDict.Add(workByAccountGroup.Key, workByAccountGroupSorted.Concat(breaksByAccount).OrderBy(i => i.StartTime).ToList());
            }

            return !groupDict.IsNullOrEmpty();
        }

        public static List<dynamic> GetInfoObjects(this List<TimeScheduleTemplateBlock> l)
        {
            var d = new List<dynamic>();
            l.ForEach(tb => d.Add(tb.GetInfoObject()));
            return d;
        }

        public static dynamic GetInfoObject(this TimeScheduleTemplateBlock e)
        {
            dynamic d = new ExpandoObject();
            if (e != null)
            {
                d.TimeScheduleTemplateBlockId = e.TimeScheduleTemplateBlockId;
                d.Date = e.Date;
                d.StartTime = e.StartTime.ToShortTimeString();
                d.StopTime = e.StopTime.ToShortTimeString();
                d.IsBreak = e.IsBreak;
                d.IsBreak = e.IsBreak;
                d.AccountId = e.AccountId;
                d.TimeDeviationCauseId = e.TimeDeviationCauseId;
            }
            return d;
        }
        public static List<dynamic> GetInfoObjects(this List<TimePayrollTransaction> l)
        {
            var d = new List<dynamic>();
            l.ForEach(t => d.Add(t.GetInfoObject()));
            return d;
        }
        public static dynamic GetInfoObject(this TimePayrollTransaction e)
        {
            dynamic d = new ExpandoObject();
            if (e != null)
            {
                d.TimePayrollTransactionId = e.TimePayrollTransactionId;
                d.ProductId = e.ProductId;
                d.Date = e.TimeBlockDate?.Date;
                d.TimeBlockDateId = e.TimeBlockDateId;
                d.AccountId = e.AccountStdId;
                d.InternalAccountIds = e.AccountInternalIds.ToCommaSeparated();
            }
            return d;
        }

        public static string ToJson(this List<TimeScheduleTemplateBlock> l)
        {
            return JsonConvert.SerializeObject(l.GetInfoObjects());
        }

        public static string ToJson(this List<TimePayrollTransaction> l)
        {
            return JsonConvert.SerializeObject(l.GetInfoObjects());
        }
        public static string ToJson(this List<string> l)
        {
            return JsonConvert.SerializeObject(l);
        }

        #endregion

        #region IScheduleBlockObject

        public static List<T> GetOverlappedBreaks<T>(this IScheduleBlockObject e, IEnumerable<T> breaks, bool includeBreaksThatOnlyStartInScheduleBlock = false, bool includeBreaksThatOnlyEndInScheduleBlock = false, bool includeBreaksThatCouldBothStartAndEndInScheduleBlock = false, bool includeBreaksThatCouldBothStartAndEndOutsideScheduleBlock = false) where T : IScheduleBlockObject
        {
            List<T> overlappedBreaks = new List<T>();

            if (e == null || breaks.IsNullOrEmpty())
                return overlappedBreaks;

            foreach (var brk in breaks)
            {
                if (brk.StartTime >= e.StartTime && brk.StopTime <= e.StopTime)
                {
                    overlappedBreaks.Add(brk);
                }
                else
                {
                    if (includeBreaksThatCouldBothStartAndEndOutsideScheduleBlock)
                    {
                        if (brk.StartTime <= e.StartTime && brk.StopTime >= e.StopTime)
                        {
                            overlappedBreaks.Add(brk);
                            continue;
                        }
                    }

                    if (includeBreaksThatCouldBothStartAndEndInScheduleBlock)
                    {
                        if ((brk.StartTime >= e.StartTime && brk.StartTime <= e.StopTime) || (brk.StopTime >= e.StartTime && brk.StopTime <= e.StopTime))
                            overlappedBreaks.Add(brk);
                    }
                    else
                    {
                        if (includeBreaksThatOnlyStartInScheduleBlock && brk.StartTime >= e.StartTime && brk.StartTime < e.StopTime)
                            overlappedBreaks.Add(brk);
                        else if (includeBreaksThatOnlyEndInScheduleBlock && brk.StopTime >= e.StartTime && brk.StopTime < e.StopTime)
                            overlappedBreaks.Add(brk);
                    }
                }
            }

            return overlappedBreaks;
        }

        public static List<T> FilterOnScheduleType<T>(this List<T> l, List<int> shiftTypeIds, bool isHidden, bool allowNullValues) where T : IScheduleBlockObject
        {
            List<T> valid = new List<T>();

            if (l.IsNullOrEmpty())
                return valid;
            if (shiftTypeIds.IsNullOrEmpty())
                return l;

            valid.AddRange(l.Where(i => !i.IsBreak && (i.ShiftTypeId.HasValue && shiftTypeIds.Contains(i.ShiftTypeId.Value) || allowNullValues && i.ShiftTypeId.IsNullOrEmpty())));
            if (valid.Any())
            {
                if (isHidden)
                {
                    List<string> links = valid.Where(e => !string.IsNullOrEmpty(e.Link)).Select(e => e.Link).Distinct().ToList();
                    valid.AddRange(l.Where(e => e.IsBreak && links.Contains(e.Link)));
                }
                else
                {
                    foreach (var brk in l.Where(e => e.IsBreak))
                    {
                        if (valid.Any(e => !e.IsBreak && CalendarUtility.IsDatesOverlapping(e.StartTime, e.StopTime, brk.StartTime, brk.StopTime)))
                            valid.Add(brk);
                    }
                }
            }

            return valid;
        }

        #endregion

        #region TimeScheduleTemplateBlockTask

        public static TimeScheduleTemplateBlockTaskDTO ToDTO(this TimeScheduleTemplateBlockTask e, DateTime? startTime = null, int? length = null)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (e.TimeScheduleTaskId.HasValue && !e.TimeScheduleTaskReference.IsLoaded)
                    {
                        e.TimeScheduleTaskReference.Load();
                        Util.DataProjectLogCollector.LogLoadedEntityInExtension("TimeScheduletemplateBlock.cs e.TimeScheduleTaskReference");
                    }
                    if (e.IncomingDeliveryRowId.HasValue && !e.IncomingDeliveryRowReference.IsLoaded)
                    {
                        e.IncomingDeliveryRowReference.Load();
                        Util.DataProjectLogCollector.LogLoadedEntityInExtension("TimeScheduletemplateBlock.cs e.IncomingDeliveryRowReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            TimeScheduleTemplateBlockTaskDTO dto = new TimeScheduleTemplateBlockTaskDTO()
            {
                TimeScheduleTemplateBlockTaskId = e.TimeScheduleTemplateBlockTaskId,
                TimeScheduleTemplateBlockId = e.TimeScheduleTemplateBlockId,
                TimeScheduleTaskId = e.TimeScheduleTaskId,
                IncomingDeliveryRowId = e.IncomingDeliveryRowId,
                StartTime = startTime ?? e.StartTime,
                StopTime = startTime.HasValue && length.HasValue ? startTime.Value.AddMinutes(length.Value) : e.StopTime,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State
            };

            if (e.TimeScheduleTask != null)
            {
                dto.Name = e.TimeScheduleTask.Name;
                dto.Description = e.TimeScheduleTask.Description;
            }
            else if (e.IncomingDeliveryRow != null)
            {
                dto.Name = e.IncomingDeliveryRow.Name;
                dto.Description = e.IncomingDeliveryRow.Description;
            }

            return dto;
        }

        public static IEnumerable<TimeScheduleTemplateBlockTaskDTO> ToDTOs(this IEnumerable<TimeScheduleTemplateBlockTask> l)
        {
            var dtos = new List<TimeScheduleTemplateBlockTaskDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static List<TimeScheduleTemplateBlockTask> GetOverlappingTask(this List<TimeScheduleTemplateBlockTask> tasks, DateTime time)
        {
            return tasks.Where(x => x.IsOverlapped(time)).ToList();
        }

        public static List<TimeScheduleTemplateBlockTask> GetTaskThatEndsBeforeGivenTime(this List<TimeScheduleTemplateBlockTask> l, DateTime time)
        {
            return l.Where(x => x.EndsBeforeGivenTime(time)).ToList();
        }

        public static List<TimeScheduleTemplateBlockTask> GetTaskThatStartsAfterGivenTime(this List<TimeScheduleTemplateBlockTask> l, DateTime time)
        {
            return l.Where(x => x.StartsAfterGivenTime(time)).ToList();
        }

        public static List<TimeScheduleTemplateBlockTask> GetScheduledBlockTasks(this IEnumerable<TimeScheduleTemplateBlockTask> l, DateTime date, SoeStaffingNeedType type, List<TimeScheduleTemplateHead> templateHeads)
        {
            List<TimeScheduleTemplateBlockTask> scheduleBlockTasks = new List<TimeScheduleTemplateBlockTask>();
            if (templateHeads == null)
                return scheduleBlockTasks;

            List<TimeScheduleTemplateBlockTask> validTasks = (from e in l
                                                              where e.StartTime < e.StopTime &&
                                                              e.State == (int)SoeEntityState.Active &&
                                                              e.TimeScheduleTemplateBlockId.HasValue &&
                                                              e.TimeScheduleTemplateBlock != null &&
                                                              e.TimeScheduleTemplateBlock.State == (int)SoeEntityState.Active &&
                                                              e.TimeScheduleTemplateBlock.StartTime < e.TimeScheduleTemplateBlock.StopTime &&
                                                              (
                                                              (type == SoeStaffingNeedType.EmployeePost && e.StartTime.Year == CalendarUtility.DATETIME_DEFAULT.Year)
                                                                ||
                                                              (type == SoeStaffingNeedType.Template && !e.TimeScheduleTemplateBlock.Date.HasValue && e.TimeScheduleTemplateBlock.EmployeeId.HasValue)
                                                                ||
                                                              (type == SoeStaffingNeedType.Employee && e.TimeScheduleTemplateBlock.Date == date && e.TimeScheduleTemplateBlock.EmployeeId.HasValue)
                                                              )// &&
                                                              //e.TimeScheduleTemplateBlock.TimeScheduleEmployeePeriodId.HasValue
                                                              select e).ToList();

            #region Template

            foreach (TimeScheduleTemplateBlockTask task in validTasks)
            {
                TimeScheduleTemplateHead templateHead = templateHeads.FirstOrDefault(i => i.StartDate.HasValue && i.TimeScheduleTemplatePeriodIds.Contains(task.TimeScheduleTemplateBlock.TimeScheduleTemplatePeriodId.Value));
                if (templateHead == null)
                    continue;

                TimeScheduleTemplatePeriod templatePeriod = templateHead.TimeScheduleTemplatePeriod.FirstOrDefault(i => i.TimeScheduleTemplatePeriodId == task.TimeScheduleTemplateBlock.TimeScheduleTemplatePeriodId);
                if (templatePeriod == null)
                    continue;

                if (type == SoeStaffingNeedType.EmployeePost || type == SoeStaffingNeedType.Template)
                {
                    int rowNr = CalendarUtility.GetScheduleDayNumber(date, templateHead.StartDate.Value, 1, templateHead.NoOfDays);
                    if (rowNr != templatePeriod.DayNumber)
                        continue;
                }

                if (!templatePeriod.TimeScheduleTemplateBlock.IsLoaded)
                {
                    templatePeriod.TimeScheduleTemplateBlock.Load();
                    Util.DataProjectLogCollector.LogLoadedEntityInExtension("TimeScheduletemplateBlock.cs templatePeriod.TimeScheduleTemplateBlock");
                }
                task.CalculatedTemplatePeriod = templatePeriod;
                scheduleBlockTasks.Add(task);
            }

            #endregion

            return scheduleBlockTasks.OrderBy(i => i.StartTime).ToList();
        }

        public static List<TimeScheduleTemplateBlockTaskDTO> SplitBreaks(this TimeScheduleTemplateBlockTask e, List<TimeScheduleTemplateBlock> scheduleBreaks)
        {
            List<TimeScheduleTemplateBlockTaskDTO> blockTasksAfterBreaks = new List<TimeScheduleTemplateBlockTaskDTO>();

            if (!scheduleBreaks.IsNullOrEmpty())
            {
                DateTime currentStart = e.StartTime;
                for (int breakNr = 1; breakNr <= scheduleBreaks.Count; breakNr++)
                {
                    TimeScheduleTemplateBlock scheduleBreak = scheduleBreaks[breakNr - 1];
                    int minutes = (int)scheduleBreak.StartTime.Subtract(currentStart).TotalMinutes;
                    if (minutes > 0)
                        blockTasksAfterBreaks.Add(e.ToDTO(currentStart, minutes));

                    if (breakNr == scheduleBreaks.Count)
                    {
                        minutes = (int)e.StopTime.Subtract(scheduleBreak.StopTime).TotalMinutes;
                        if (minutes > 0)
                            blockTasksAfterBreaks.Add(e.ToDTO(scheduleBreak.StopTime, minutes));
                    }

                    currentStart = scheduleBreak.StopTime;
                }
            }
            else
            {
                blockTasksAfterBreaks.Add(e.ToDTO());
            }

            return blockTasksAfterBreaks;
        }

        public static List<TimeScheduleTemplateBlock> GetBreaks(this TimeScheduleTemplateBlockTask e, SoeStaffingNeedType type)
        {
            List<TimeScheduleTemplateBlock> templateBlockBreaks = null;
            if (type == SoeStaffingNeedType.EmployeePost)
                templateBlockBreaks = e.CalculatedTemplatePeriod.TimeScheduleTemplateBlock.GetEmployeePostBreaks(e.CalculatedTemplatePeriod.TimeScheduleTemplatePeriodId);
            else if (type == SoeStaffingNeedType.Template)
                templateBlockBreaks = e.CalculatedTemplatePeriod.TimeScheduleTemplateBlock.GetTemplateBreaks(e.CalculatedTemplatePeriod.TimeScheduleTemplatePeriodId);
            else if (type == SoeStaffingNeedType.Employee && e.TimeScheduleTemplateBlock.EmployeeId.HasValue && e.TimeScheduleTemplateBlock.Date.HasValue)
                templateBlockBreaks = e.CalculatedTemplatePeriod.TimeScheduleTemplateBlock.GetScheduleBreaks(e.CalculatedTemplatePeriod.TimeScheduleTemplatePeriodId, e.TimeScheduleTemplateBlock.EmployeeId.Value, e.TimeScheduleTemplateBlock.Date.Value);

            List<TimeScheduleTemplateBlock> validTemplateBlockBreaks = new List<TimeScheduleTemplateBlock>();
            foreach (TimeScheduleTemplateBlock templateBlockBreak in templateBlockBreaks.Where(i => i.State == (int)SoeEntityState.Active))
            {
                if (CalendarUtility.IsNewInsideCurrent(templateBlockBreak.StartTime, templateBlockBreak.StopTime, e.StartTime, e.StopTime))
                    validTemplateBlockBreaks.Add(templateBlockBreak);
            }

            return validTemplateBlockBreaks;
        }

        public static List<StaffingNeedsTaskDTO> GetRemainingBlockTasks(this List<TimeScheduleTemplateBlockTask> l, TimeScheduleTask timeScheduleTask, DateTime date, SoeStaffingNeedType type, bool loadShiftType)
        {
            List<StaffingNeedsTaskDTO> remainingTasks = new List<StaffingNeedsTaskDTO>();
            if (timeScheduleTask != null)
            {
                if (l == null)
                    l = new List<TimeScheduleTemplateBlockTask>();

                List<Tuple<DateTime, DateTime, int, bool>> tuples = l.GetRemainingBlockTasks(date, timeScheduleTask.StartTime ?? CalendarUtility.DATETIME_DEFAULT, timeScheduleTask.StopTime, timeScheduleTask.Length, timeScheduleTask.IsFixed, type, timeScheduleTask.BreakFillsNeed);
                foreach (Tuple<DateTime, DateTime, int, bool> tuple in tuples)
                {
                    if (tuple.Item3 > 0)
                        remainingTasks.Add(timeScheduleTask.CreateStaffingNeedsTask(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, loadShiftType));
                }
            }
            return remainingTasks;
        }

        public static List<StaffingNeedsTaskDTO> GetRemainingBlockTasks(this List<TimeScheduleTemplateBlockTask> l, TimeScheduleTask timeScheduleTask, List<StaffingNeedsRowPeriodDTO> periods, DateTime date, SoeStaffingNeedType type, bool loadShiftType, int interval = 15)
        {
            List<StaffingNeedsTaskDTO> remainingTasks = new List<StaffingNeedsTaskDTO>();
            if (timeScheduleTask != null)
            {
                if (l == null)
                    l = new List<TimeScheduleTemplateBlockTask>();

                List<Tuple<DateTime, DateTime, int, bool>> tuples = l.GetRemainingBlockTasks(date, periods, type, interval, timeScheduleTask.BreakFillsNeed);
                foreach (Tuple<DateTime, DateTime, int, bool> tuple in tuples)
                {
                    if (tuple.Item3 > 0)
                        remainingTasks.Add(timeScheduleTask.CreateStaffingNeedsTask(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, loadShiftType));
                }
            }
            return remainingTasks;
        }

        public static List<StaffingNeedsTaskDTO> GetRemainingBlockTasks(this List<TimeScheduleTemplateBlockTask> l, IncomingDeliveryRow incominingDeliveryRow, DateTime date, SoeStaffingNeedType type, bool loadShiftType)
        {
            List<StaffingNeedsTaskDTO> remainingTasks = new List<StaffingNeedsTaskDTO>();
            if (incominingDeliveryRow != null)
            {
                if (l == null)
                    l = new List<TimeScheduleTemplateBlockTask>();

                List<Tuple<DateTime, DateTime, int, bool>> tuples = l.GetRemainingBlockTasks(date, incominingDeliveryRow.StartTime ?? CalendarUtility.DATETIME_DEFAULT, incominingDeliveryRow.StopTime, incominingDeliveryRow.NbrOfPersons > 1 ? incominingDeliveryRow.Length * incominingDeliveryRow.NbrOfPersons : incominingDeliveryRow.Length, incominingDeliveryRow.IsFixed, type);
                foreach (Tuple<DateTime, DateTime, int, bool> tuple in tuples)
                {
                    if (tuple.Item3 > 0)
                        remainingTasks.Add(incominingDeliveryRow.CreateStaffingNeedsTask(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, loadShiftType));
                }
            }
            return remainingTasks;
        }

        public static List<Tuple<DateTime, DateTime, int, bool>> GetRemainingBlockTasks(this List<TimeScheduleTemplateBlockTask> l, DateTime date, DateTime startTime, DateTime? stopTime, int needMinutes, bool isFixed, SoeStaffingNeedType type, bool breakFillsNeed = false)
        {
            //Date, StartTime, length, isFixed
            List<Tuple<DateTime, DateTime, int, bool>> remainingTasks = new List<Tuple<DateTime, DateTime, int, bool>>();

            if (type == SoeStaffingNeedType.Employee)
            {
                int days = stopTime.HasValue ? (stopTime.Value - startTime).Days : 0;
                startTime = CalendarUtility.GetDateTime(date, startTime);
                if (stopTime.HasValue)
                    stopTime = CalendarUtility.GetDateTime(date, stopTime.Value).AddDays(days);
            }

            if (l == null)
                l = new List<TimeScheduleTemplateBlockTask>();
            l = l.OrderBy(i => i.StartTime).ThenBy(i => i.StopTime).ToList();
            if (isFixed && stopTime.HasValue && !l.IsAnyOverlapping())
            {
                #region Fixed times

                if (l.Any())
                {
                    #region Split after breaks

                    List<TimeScheduleTemplateBlockTaskDTO> blockTasksAfterBreaks = new List<TimeScheduleTemplateBlockTaskDTO>();

                    for (int taskNr = 1; taskNr <= l.Count; taskNr++)
                    {
                        TimeScheduleTemplateBlockTask blockTask = l[taskNr - 1];
                        List<TimeScheduleTemplateBlock> scheduleBreaks = blockTask.GetBreaks(type);
                        if (breakFillsNeed)
                            scheduleBreaks = new List<TimeScheduleTemplateBlock>();

                        blockTasksAfterBreaks.AddRange(blockTask.SplitBreaks(scheduleBreaks));
                    }


                    #endregion

                    #region Calculate remaining need

                    if (blockTasksAfterBreaks.Count > 0)
                    {
                        DateTime currentStart = type == SoeStaffingNeedType.Employee ? CalendarUtility.GetDateTime(date, startTime) : startTime;
                        for (int taskNr = 1; taskNr <= blockTasksAfterBreaks.Count; taskNr++)
                        {
                            TimeScheduleTemplateBlockTaskDTO blockTask = blockTasksAfterBreaks[taskNr - 1];

                            int minutes = (int)blockTask.StartTime.Subtract(currentStart).TotalMinutes;
                            if (minutes > 0)
                                remainingTasks.Add(Tuple.Create(date, currentStart, minutes, true));

                            if (taskNr == blockTasksAfterBreaks.Count)
                            {
                                minutes = (int)stopTime.Value.Subtract(blockTask.StopTime).TotalMinutes;
                                if (minutes > 0)
                                    remainingTasks.Add(Tuple.Create(date, blockTask.StopTime, minutes, true));
                            }

                            currentStart = blockTask.StopTime;
                        }
                    }

                    #endregion
                }
                else
                {
                    remainingTasks.Add(Tuple.Create(date, startTime, needMinutes, true));
                }

                #endregion
            }
            else
            {
                #region Not fixed times

                int breakMinutes = l.GetBreakMinutes(type);
                int scheduledMinutes = (int)l.Sum(i => i.StopTime.Subtract(i.StartTime).TotalMinutes) - breakMinutes;
                int remainingMinutes = needMinutes - scheduledMinutes;
                while (remainingMinutes > 0)
                {
                    DateTime currentStop = CalendarUtility.GetEarliestDate(startTime.AddMinutes(remainingMinutes), stopTime ?? DateTime.MaxValue);
                    int length = (int)currentStop.Subtract(startTime).TotalMinutes;
                    if (length <= 0)
                        break;

                    remainingTasks.Add(Tuple.Create(date, startTime, length, false));
                    remainingMinutes -= length;
                }

                #endregion
            }

            return remainingTasks;
        }

        public static List<Tuple<DateTime, DateTime, int, bool>> GetRemainingBlockTasks(this List<TimeScheduleTemplateBlockTask> l, DateTime date, List<StaffingNeedsRowPeriodDTO> periods, SoeStaffingNeedType type, int interval = 15, bool breakFillsNeed = false)
        {
            //Date, StartTime, length, isFixed
            List<Tuple<DateTime, DateTime, int, bool>> remainingTasks = new List<Tuple<DateTime, DateTime, int, bool>>();
            if (periods != null && periods.Count > 0)
            {
                if (l == null)
                    l = new List<TimeScheduleTemplateBlockTask>();

                #region Split after breaks

                List<TimeScheduleTemplateBlockTaskDTO> blockTasksAfterBreaks = new List<TimeScheduleTemplateBlockTaskDTO>();

                for (int taskNr = 1; taskNr <= l.Count; taskNr++)
                {
                    TimeScheduleTemplateBlockTask blockTask = l[taskNr - 1];
                    List<TimeScheduleTemplateBlock> scheduleBreaks = blockTask.GetBreaks(type);
                    if (!breakFillsNeed)
                        scheduleBreaks = new List<TimeScheduleTemplateBlock>();

                    blockTasksAfterBreaks.AddRange(blockTask.SplitBreaks(scheduleBreaks));
                }

                #endregion

                Dictionary<DateTime, int> timeNeedDict = new Dictionary<DateTime, int>();
                DateTime start = periods.Min(i => i.TimeSlot.From);
                DateTime stop = periods.Max(i => i.TimeSlot.To);
                DateTime current = start;
                while (current.AddMinutes(interval) <= stop)
                {
                    int currentNeed = periods.Count(period => CalendarUtility.IsNewOverlappedByCurrent(current, current.AddMinutes(interval), period.TimeSlot.From, period.TimeSlot.To));
                    int currentSchedule = blockTasksAfterBreaks.Count(blockTask => CalendarUtility.IsNewOverlappedByCurrent(current, current.AddMinutes(interval), blockTask.StartTime, blockTask.StopTime));
                    int remainingNeed = currentNeed - currentSchedule;
                    if (remainingNeed > 0)
                        timeNeedDict.Add(current, remainingNeed);

                    current = current.AddMinutes(interval);
                }

                int completedIterations = 0;
                Dictionary<DateTime, int> remainingTimeNeedDict = new Dictionary<DateTime, int>();
                remainingTimeNeedDict.AddRange(timeNeedDict.ToList());
                while (remainingTimeNeedDict.Count > 0)
                {
                    List<Tuple<DateTime, DateTime>> ranges = remainingTimeNeedDict.Select(i => i.Key).GetCoherentTimeRanges(interval);
                    foreach (Tuple<DateTime, DateTime> range in ranges)
                    {
                        remainingTasks.Add(Tuple.Create(date, range.Item1, (int)range.Item2.Subtract(range.Item1).TotalMinutes, true));
                    }
                    completedIterations++;
                    remainingTimeNeedDict = new Dictionary<DateTime, int>();
                    remainingTimeNeedDict.AddRange(timeNeedDict.Where(i => i.Value > completedIterations).ToList());
                }
            }
            return remainingTasks;
        }

        public static int GetBreakMinutes(this IEnumerable<TimeScheduleTemplateBlockTask> l, SoeStaffingNeedType type)
        {
            int breakMinutes = 0;
            foreach (TimeScheduleTemplateBlockTask blockTask in l)
            {
                List<TimeScheduleTemplateBlock> scheduleBreaks = blockTask.GetBreaks(type);
                foreach (TimeScheduleTemplateBlock scheduleBreak in scheduleBreaks)
                {
                    breakMinutes += CalendarUtility.GetOverlappingMinutes(blockTask.StartTime, blockTask.StopTime, scheduleBreak.StartTime, scheduleBreak.StopTime);
                }
            }
            return breakMinutes;
        }

        public static bool IsAnyOverlapping(this IEnumerable<TimeScheduleTemplateBlockTask> l)
        {
            bool isOverlapping = false;
            foreach (var blockTaskOuter in l)
            {
                foreach (var blockTaskInner in l)
                {
                    if (blockTaskOuter.GetHashCode() != blockTaskInner.GetHashCode() && CalendarUtility.IsDatesOverlapping(blockTaskOuter.StartTime, blockTaskOuter.StopTime, blockTaskInner.StartTime, blockTaskInner.StopTime))
                        isOverlapping = true;
                }
            }
            return isOverlapping;
        }

        public static bool IsTaskActive(this TimeScheduleTemplateBlockTask e)
        {
            //Must also check TimeScheduleTemplateBlock beacuse we cant be sure that TimeScheduleTemplateBlockTask always is set to delete when TimeScheduleTemplateBlock is
            //Cannot delete if task and block is active
            return e.State == (int)SoeEntityState.Active && (e.TimeScheduleTemplateBlock == null || e.TimeScheduleTemplateBlock.State == (int)SoeEntityState.Active);
        }

        public static bool IsOverlapped(this TimeScheduleTemplateBlockTask e, DateTime time)
        {
            return e.StartTime <= time && e.StopTime >= time;
        }

        public static bool EndsBeforeGivenTime(this TimeScheduleTemplateBlockTask e, DateTime time)
        {
            return e.StopTime <= time;
        }

        public static bool StartsAfterGivenTime(this TimeScheduleTemplateBlockTask e, DateTime time)
        {
            return e.StartTime >= time;
        }

        #endregion

        #region TimeScheduleTemplateBlockHistory

        public static TimeScheduleTemplateBlockHistoryDTO ToDTO(this TimeScheduleTemplateBlockHistory e)
        {
            TimeScheduleTemplateBlockHistoryDTO dto = new TimeScheduleTemplateBlockHistoryDTO()
            {
                //Keys
                TimeScheduleTemplateBlockHistoryId = e.TimeScheduleTemplateBlockHistoryId,
                TimeScheduleTemplateBlockId = e.TimeScheduleTemplateBlockId,
                ActorCompanyId = e.ActorCompanyId,
                CurrentEmployeeId = e.CurrentEmployeeId,

                //Common
                BatchId = e.BatchId,
                Note = String.Empty,
                Type = (TermGroup_ShiftHistoryType)e.Type,
                IsBreak = e.IsBreak,
                RecordId = e.RecordId,
                Created = e.Created,
                CreatedBy = e.CreatedBy,

                //Dates
                FromStart = e.FromStart,
                ToStart = e.ToStart,
                FromStop = e.FromStop,
                ToStop = e.ToStop,
                OriginDate = e.OriginDate,

                //Employee
                FromEmployeeId = e.FromEmployeeId,
                FromEmployeeNr = String.Empty, //Cannot be set in DTO, sets later
                ToEmployeeId = e.ToEmployeeId,
                ToEmployeeNr = String.Empty, //Cannot be set in DTO, sets later
                OriginEmployeeId = e.OriginEmployeeId,
                OriginEmployeeNr = String.Empty, //Cannot be set in DTO, sets later

                //Shift
                FromShiftTypeId = e.FromShiftTypeId,
                FromShiftStatus = e.FromShiftStatus != null ? (TermGroup_TimeScheduleTemplateBlockShiftStatus)e.FromShiftStatus : (TermGroup_TimeScheduleTemplateBlockShiftStatus?)null,
                FromShiftUserStatus = e.FromShiftUserStatus != null ? (TermGroup_TimeScheduleTemplateBlockShiftUserStatus)e.FromShiftUserStatus : (TermGroup_TimeScheduleTemplateBlockShiftUserStatus?)null,
                FromShiftTypeExtCode = String.Empty, //Cannot be set in DTO, sets later
                ToShiftTypeId = e.ToShiftTypeId,
                ToShiftStatus = e.ToShiftStatus != null ? (TermGroup_TimeScheduleTemplateBlockShiftStatus)e.ToShiftStatus : (TermGroup_TimeScheduleTemplateBlockShiftStatus?)null,
                ToShiftUserStatus = e.ToShiftUserStatus != null ? (TermGroup_TimeScheduleTemplateBlockShiftUserStatus)e.ToShiftUserStatus : (TermGroup_TimeScheduleTemplateBlockShiftUserStatus?)null,
                ToShiftTypeExtCode = String.Empty, //Cannot be set in DTO, sets later

                FromTimeDeviationCauseId = e.FromTimeDeviationCauseId,
                FromTimeDeviationCauseName = String.Empty, //Cannot be set in DTO, sets later
                ToTimeDeviationCauseId = e.ToTimeDeviationCauseId,
                ToTimeDeviationCauseName = String.Empty, //Cannot be set in DTO, sets later
            };

            dto.IsNew = e.IsNew();
            dto.IsChanged = e.IsChanged();
            dto.IsDeleted = e.IsDeleted();

            return dto;
        }

        public static IEnumerable<TimeScheduleTemplateBlockHistoryDTO> ToDTOs(this IEnumerable<TimeScheduleTemplateBlockHistory> l)
        {
            var dtos = new List<TimeScheduleTemplateBlockHistoryDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO());
                }
            }
            return dtos;
        }

        public static bool IsChanged(this TimeScheduleTemplateBlockHistory e)
        {
            if (e.IsDeleted())
                return false;
            if (e.FromEmployeeId.HasValue && e.ToEmployeeId.HasValue)
                return true;
            return false;
        }

        public static bool HasChangedEmployee(this TimeScheduleTemplateBlockHistory e)
        {
            if (e.FromEmployeeId.HasValue && e.ToEmployeeId.HasValue)
                return e.FromEmployeeId.Value != e.ToEmployeeId.Value;

            return false;
        }

        public static bool IsNew(this TimeScheduleTemplateBlockHistory e)
        {
            if (!e.FromEmployeeId.HasValue && e.ToEmployeeId.HasValue)
                return true;
            return false;
        }

        public static bool IsOriginatedFromAnotherShift(this TimeScheduleTemplateBlockHistory e)
        {
            if (e.OriginTimeScheduleTemplateBlockId.HasValue)
                return true;
            return false;
        }

        public static bool IsTimeDeviationCauseChanged(this TimeScheduleTemplateBlockHistory e)
        {
            if (!e.FromTimeDeviationCauseId.HasValue && e.ToTimeDeviationCauseId.HasValue)
                return true;
            return false;
        }

        public static bool IsDeleted(this TimeScheduleTemplateBlockHistory e)
        {
            if (e.ToStart.HasValue && e.ToStop.HasValue && CalendarUtility.IsTimeZero(e.ToStart.Value) && CalendarUtility.IsTimeZero(e.ToStop.Value))
                return true;
            if (e.FromEmployeeId.HasValue && !e.ToEmployeeId.HasValue)
                return true;
            return false;
        }

        #endregion
    }
}
