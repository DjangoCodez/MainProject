using Newtonsoft.Json;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data.Util;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace SoftOne.Soe.Data
{
    public partial class TimeBlock : ITimeBlockObject, ICreatedModified, IModifiedWithNoCheckes, IState, ITask
    {
        public Guid? GuidId { get; set; }
        public Guid GuidTemplateBlock { get; set; }

        public bool IsAttested { get; set; }
        public bool IsTransferedToSalary { get; set; }
        public bool CreateAsBlank { get; set; }
        public bool IsNew { get; set; }
        public bool CopyCommentToExcessBlockIfCreated { get; set; }
        public string EmployeeChildName { get; set; }
        public bool TimeChanged { get; set; }
        public bool IsFromOtherProjectTimeBlock { get; set; }
        public string DebugInfo { get; set; }

        //Midnight secure
        public DateTime? ActualStartTime
        {
            get { return this.TimeBlockDate != null ? CalendarUtility.MergeDateAndTime(this.TimeBlockDate.Date.AddDays((this.StartTime.Date - CalendarUtility.DATETIME_DEFAULT).Days), this.StartTime) : (DateTime?)null; }
        }
        //Midnight secure
        public DateTime? ActualStopTime
        {
            get { return ActualStartTime.HasValue ? ActualStartTime.Value.AddMinutes((this.StopTime - this.StartTime).TotalMinutes) : (DateTime?)null; }
        }
        public bool IsGeneratedFromBreak
        {
            get { return this.TimeScheduleTemplateBlockBreakId.HasValue; }
        }
        public bool IsBreakOrGeneratedFromBreak
        {
            get { return this.IsBreak || IsGeneratedFromBreak; }
        }
        public bool IsGeneratedFromBreakButNotBreak
        {
            get { return IsGeneratedFromBreak && !IsBreak; }
        }
        public int TotalMinutes
        {
            get { return (int)this.StopTime.Subtract(this.StartTime).TotalMinutes; }
        }
        public string TimeString
        {
            get
            {
                return this.StartTime.ToShortTimeString() + "-" + this.StopTime.ToShortTimeString();
            }
        }
        public bool IsPayed
        {
            get
            {
                return this.TimeCode?.Any(i => i.Payed) ?? false;
            }
        }
        public bool IsSchedulePreliminaryTimeBlock { get; set; } //Type1
        public bool IsScheduleAbsenceTimeBlock { get; set; } //Type2
        public bool IsSickDuringIwhTimeBlock { get; set; }
        public bool IsSickDuringStandbyTimeBlock { get; set; }
        public bool IsSickDuringIwhOrStandbyTimeBlock
        {
            get
            {
                return this.IsSickDuringIwhTimeBlock || this.IsSickDuringStandbyTimeBlock;
            }
        }
        public bool TransactionsResulted { get; set; }

        public int? PayrollImportEmployeeTransactionId { get; set; }
        public List<int> TransactionTimeCodeIds { get; set; }
        
        public List<AccountInternal> DeviationAccounts { get; set; }
        public bool HasDeviationAccounts => !this.DeviationAccounts.IsNullOrEmpty();
        public bool DeviationAccountsNotLoaded => !this.DeviationAccountIds.IsNullOrEmpty() && this.DeviationAccounts.IsNullOrEmpty();
        public List<int> GetDeviationAccountIds()
        {
            return !string.IsNullOrEmpty(this.DeviationAccountIds) ? this.DeviationAccountIds?.Split(',').Select(int.Parse).ToList() ?? new List<int>() : new List<int>();
        }

        public void SetDeviationAccounts(List<AccountInternal> accountInternals)
        {
            this.DeviationAccounts = accountInternals;
            this.DeviationAccountIds = accountInternals?.Select(a => a.AccountId).ToCommaSeparated();
        }
        public void SetDeviationAccounts(TimeBlock other)
        {
            if (other != null && !other.DeviationAccounts.IsNullOrEmpty())
                SetDeviationAccounts(other.DeviationAccounts);
        }

        public int? CalculatedShiftTypeId { get; set; }
        public int? CalculatedTimeScheduleTypeId { get; set; }
        public int? CalculatedTimeScheduleTypeIdFromShift { get; set; }
        public int? CalculatedTimeScheduleTypeIdFromShiftType { get; set; }
        public List<int> CalculatedTimeScheduleTypeIdsFromEmployee { get; set; }
        public List<int> CalculatedTimeScheduleTypeIdsFromTimeStamp { get; set; }
        public List<int> CalculatedTimeScheduleTypeIdsFromTimeLeisureCodes { get; set; }
        public bool? CalculatedAsPresence { get; set; }
        public bool? CalculatedAsAbsence { get; set; }
        public bool? CalculatedAsAbsenceAndPresence { get; set; }
        public bool? CalculatedAsStandby { get; set; }
        public bool? CalculatedAsExcludeFromPresenceWorkRules { get; set; }
        public bool? CalculatedAsExcludeFromPaddingRules { get; set; }
        public bool? CalculatedOutsideBreakWindow { get; set; }
        public SoeTimeRuleType? CalculatedTimeRuleType { get; set; }

        public void UpdateStartTime(DateTime time, bool setToZeroIfInvalid = false)
        {
            if (time > this.StopTime)
            {
                if (setToZeroIfInvalid)
                {
                    this.StartTime = this.StopTime;
                    this.State = (int)SoeEntityState.Deleted;
                    return;
                }
                else
                    return;
            }
            this.StartTime = time;
        }
        public void UpdateStopTime(DateTime time, bool setToZeroIfInvalid = false)
        {
            if (time < this.StartTime)
            {
                if (setToZeroIfInvalid)
                {
                    this.StopTime = this.StartTime;
                    this.State = (int)SoeEntityState.Deleted;
                    return;
                }
                else
                    return;
            }

            this.StopTime = time;
        }
        public int GetMinutes() => (int)this.StopTime.Subtract(this.StartTime).TotalMinutes;
        public void CopyFrom(TimeBlock fromBlock, bool copyScheduleTemplatedAndTimeBlockDate)
        {
            EmployeeId = fromBlock.EmployeeId;
            IsBreak = fromBlock.IsBreak();
            Comment = fromBlock.TimeStampEntry != null && fromBlock.TimeStampEntry.Any(w => !w.Note.IsNullOrEmpty()) ? string.Empty : fromBlock.Comment;
            IsPreliminary = fromBlock.IsPreliminary;
            CalculatedAsAbsence = fromBlock.CalculatedAsAbsence;

            //Set FK
            TimeDeviationCauseStopId = fromBlock.TimeDeviationCauseStopId;
            TimeDeviationCauseStartId = fromBlock.TimeDeviationCauseStartId;
            EmployeeChildId = fromBlock.EmployeeChildId;
            ShiftTypeId = fromBlock.ShiftTypeId;
            TimeScheduleTypeId = fromBlock.TimeScheduleTypeId;
            TimeScheduleTemplateBlockBreakId = fromBlock.TimeScheduleTemplateBlockBreakId;
            ProjectTimeBlockId = fromBlock.ProjectTimeBlockId;
            PayrollImportEmployeeTransactionId = fromBlock.PayrollImportEmployeeTransactionId;

            if (copyScheduleTemplatedAndTimeBlockDate)
            {
                TimeScheduleTemplatePeriodId = fromBlock.TimeScheduleTemplatePeriodId;
                TimeScheduleTemplatePeriod = fromBlock.TimeScheduleTemplatePeriod;
                TimeBlockDateId = fromBlock.TimeBlockDateId;
            }

            //Add TimeCode's
            foreach (TimeCode timeCode in fromBlock.TimeCode)
            {
                TimeCode.Add(timeCode);
            }
        }
    }

    public class DuplicateTimeBlock
    {
        public int? TaskId { get; set; }
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public int TimeBlockDateId { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public List<int> TimeBlockIds { get; set; }
        public List<string> Comments { get; set; }

        private DuplicateTimeBlock()
        {

        }

        public static List<DuplicateTimeBlock> Create(int actorCompanyId, int employeeId, int timeBlockDateId, DateTime? date, Dictionary<string, List<int>> duplicateTimes, int? taskId = null, List<string> comments = null)
        {
            List<DuplicateTimeBlock> duplicates = new List<DuplicateTimeBlock>();
            foreach (var duplicateTime in duplicateTimes)
            {
                duplicates.Add(Create(actorCompanyId, employeeId, timeBlockDateId, date, duplicateTime.Key, duplicateTime.Value, taskId, comments));
            }
            return duplicates;
        }

        public static DuplicateTimeBlock Create(int actorCompanyId, int employeeId, int timeBlockDateId, DateTime? date, string time, List<int> timeBlockIds, int? taskId = null, List<string> comments = null)
        {
            return new DuplicateTimeBlock
            {
                TaskId = taskId,
                ActorCompanyId = actorCompanyId,
                EmployeeId = employeeId,
                TimeBlockDateId = timeBlockDateId,
                Date = date?.ToShortDateString(),
                Time = time,
                TimeBlockIds = timeBlockIds,
                Comments = comments,
            };
        }
    }

    public static partial class EntityExtensions
    {
        #region TimeBlock

        public static TimeBlockDTO ToDTO(this TimeBlock e, bool includeTransactions, bool includeChild)
        {
            if (e == null)
                return null;

            #region Try load

            try
            {
                if (!e.IsAdded())
                {
                    if (includeTransactions)
                    {
                        if (!e.TimeCodeTransaction.IsLoaded)
                        {
                            e.TimeCodeTransaction.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("TimeBlock.cs e.TimeCodeTransaction");
                        }
                        if (!e.TimeInvoiceTransaction.IsLoaded)
                        {
                            e.TimeInvoiceTransaction.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("TimeBlock.cs e.TimeInvoiceTransaction");
                        }
                        if (!e.TimePayrollTransaction.IsLoaded)
                        {
                            e.TimePayrollTransaction.Load();
                            DataProjectLogCollector.LogLoadedEntityInExtension("TimeBlock.cs e.TimePayrollTransaction");
                        }
                    }
                    if (includeChild && !e.EmployeeChildReference.IsLoaded)
                    {
                        e.EmployeeChildReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("TimeBlock.cs e.EmployeeChildReference");
                    }
                }
            }
            catch (InvalidOperationException ex) { ex.ToString(); }

            #endregion

            TimeBlockDTO dto = new TimeBlockDTO()
            {
                Guid = e.GuidId,
                TimeBlockId = e.TimeBlockId,
                EmployeeId = e.EmployeeId,
                TimeBlockDateId = e.TimeBlockDateId,
                Date = e.TimeBlockDate?.Date ?? null,
                TimeDeviationCauseStartId = e.TimeDeviationCauseStartId,
                TimeDeviationCauseStopId = e.TimeDeviationCauseStopId,
                TimeScheduleTemplatePeriodId = e.TimeScheduleTemplatePeriodId,
                TimeScheduleTemplateBlockBreakId = e.TimeScheduleTemplateBlockBreakId,
                AccountId = e.AccountStdId,
                EmployeeChildId = e.EmployeeChildId,
                EmployeeChildName = e.EmployeeChild?.Name ?? string.Empty,
                ShiftTypeId = e.ShiftTypeId,
                TimeScheduleTypeId = e.TimeScheduleTypeId,
                ManuallyAdjusted = e.ManuallyAdjusted,
                StartTime = e.StartTime,
                StopTime = e.StopTime,
                IsBreak = e.IsBreak,
                IsPreliminary = e.IsPreliminary,
                IsAttested = e.IsAttested,
                IsTransferedToSalary = e.IsTransferedToSalary,
                CopyCommentToExcessBlockIfCreated = e.CopyCommentToExcessBlockIfCreated,
                Comment = e.Comment,
                Created = e.Created,
                CreatedBy = e.CreatedBy,
                Modified = e.Modified,
                ModifiedBy = e.ModifiedBy,
                State = (SoeEntityState)e.State,
            };

            dto.TimeCodes = e.TimeCode?.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs(false).ToList() ?? new List<TimeCodeDTO>();
            dto.TimeDeviationCauseStart = e.TimeDeviationCauseStart?.ToDTO();
            dto.TimeDeviationCauseStop = e.TimeDeviationCauseStop?.ToDTO();
            if (includeTransactions)
            {
                dto.TimeCodeTransactions = e.TimeCodeTransaction?.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<TimeCodeTransactionDTO>();
                dto.TimeInvoiceTransactions = e.TimeInvoiceTransaction?.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs().ToList() ?? new List<TimeInvoiceTransactionDTO>();
                dto.TimePayrollTransactions = e.TimePayrollTransaction?.Where(r => r.State == (int)SoeEntityState.Active).ToDTOs(false, false).ToList() ?? new List<TimePayrollTransactionDTO>();
                dto.TransactionTimeCodeIds = e.TransactionTimeCodeIds;
            }

            return dto;
        }

        public static IEnumerable<TimeBlockDTO> ToDTOs(this IEnumerable<TimeBlock> l, bool includeTransactions, bool includeChild)
        {
            var dtos = new List<TimeBlockDTO>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.ToDTO(includeTransactions, includeChild));
                }
            }
            return dtos;
        }

        public static TimeBlock FromDTO(this TimeBlockDTO dto)
        {
            if (dto == null)
                return null;

            TimeBlock e = new TimeBlock()
            {
                TimeBlockId = dto.TimeBlockId,
                EmployeeId = dto.EmployeeId,
                TimeBlockDateId = dto.TimeBlockDateId,
                TimeDeviationCauseStartId = dto.TimeDeviationCauseStartId,
                TimeDeviationCauseStopId = dto.TimeDeviationCauseStopId,
                TimeScheduleTemplatePeriodId = dto.TimeScheduleTemplatePeriodId,
                TimeScheduleTemplateBlockBreakId = dto.TimeScheduleTemplateBlockBreakId,
                AccountStdId = dto.AccountId ?? 0,
                ManuallyAdjusted = dto.ManuallyAdjusted,
                StartTime = dto.StartTime,
                StopTime = dto.StopTime,
                IsBreak = dto.IsBreak,
                IsPreliminary = dto.IsPreliminary,
                Comment = dto.Comment,
                Created = dto.Created,
                CreatedBy = dto.CreatedBy,
                Modified = dto.Modified,
                ModifiedBy = dto.ModifiedBy,
                State = (int)dto.State,
                GuidId = dto.Guid,
                EmployeeChildId = dto.EmployeeChildId,
                EmployeeChildName = dto.EmployeeChildName,
                ShiftTypeId = dto.ShiftTypeId,
                TimeScheduleTypeId = dto.TimeScheduleTypeId,
            };

            if (dto.TimeCodes != null)
            {
                foreach (var timeCodeDto in dto.TimeCodes)
                {
                    e.TimeCode.Add(timeCodeDto.FromDTO());
                }
            }

            return e;
        }

        public static IEnumerable<TimeBlock> FromDTOs(this IEnumerable<TimeBlockDTO> l)
        {
            var dtos = new List<TimeBlock>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.FromDTO());
                }
            }
            return dtos;
        }

        public static TimeBlock FromDTO(this AttestEmployeeDayTimeBlockDTO dto, DateTime date)
        {
            if (dto == null)
                return null;

            int minutes = (int)dto.StopTime.Subtract(dto.StartTime).TotalMinutes;
            int daysOffset = (dto.StartTime.Date - date).Days;
            DateTime startTime = CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT.AddDays(daysOffset), dto.StartTime);
            DateTime stopTime = startTime.AddMinutes(minutes);

            TimeBlock e = new TimeBlock()
            {
                GuidId = !String.IsNullOrEmpty(dto.GuidId) ? Guid.Parse(dto.GuidId) : (Guid?)null,
                TimeBlockId = dto.TimeBlockId,

                AccountStdId = dto.AccountId ?? 0,
                EmployeeId = dto.EmployeeId,
                EmployeeChildId = dto.EmployeeChildId,
                ShiftTypeId = dto.ShiftTypeId,
                TimeBlockDateId = dto.TimeBlockDateId,
                TimeDeviationCauseStartId = dto.TimeDeviationCauseStartId,
                TimeDeviationCauseStopId = dto.TimeDeviationCauseStopId,
                TimeScheduleTemplatePeriodId = dto.TimeScheduleTemplatePeriodId,
                TimeScheduleTemplateBlockBreakId = dto.TimeScheduleTemplateBlockBreakId,
                TimeScheduleTypeId = dto.TimeScheduleTypeId,

                ManuallyAdjusted = dto.ManuallyAdjusted,
                StartTime = startTime,
                StopTime = stopTime,
                IsBreak = dto.IsBreak,
                IsPreliminary = dto.IsPreliminary,
                Comment = dto.Comment,
            };

            if (!dto.DeviationAccounts.IsNullOrEmpty())
                e.DeviationAccounts = dto.DeviationAccounts.Select(a => a.FromDTO()).ToList(); //Added in partial, null as default
            if (!dto.TimeCodes.IsNullOrEmpty())
                e.TimeCode.AddRange(dto.TimeCodes.Select(t => t.FromDTO())); //Added by model, empty as default

            return e;
        }

        public static IEnumerable<TimeBlock> FromDTOs(this IEnumerable<AttestEmployeeDayTimeBlockDTO> l, DateTime date)
        {
            var dtos = new List<TimeBlock>();
            if (l != null)
            {
                foreach (var e in l)
                {
                    dtos.Add(e.FromDTO(date));
                }
            }
            return dtos.OrderBy(i => i.StartTime).ThenBy(i => i.StopTime);
        }

        public static List<AttestEmployeeDayTimeBlockDTO> ToAttestEmployeeTimeBlockDTOs(this List<TimeBlock> l, TimeBlockDate timeBlockDate, DateTime scheduleIn, DateTime scheduleOut, List<AttestPayrollTransactionDTO> transactions = null, List<TimeDeviationCause> timeDeviationCauses = null, List<AccountDimDTO> accountDims = null)
        {
            if (l == null || timeBlockDate == null)
                return null;

            var dtos = new List<AttestEmployeeDayTimeBlockDTO>();

            foreach (var e in l)
            {
                var startTime = CalendarUtility.MergeDateAndTime(timeBlockDate.Date, e.StartTime).AddDays((e.StartTime.Date - CalendarUtility.DATETIME_DEFAULT).TotalDays);
                var timeDeviationCauseName = e.TimeDeviationCauseStart?.Name ?? (e.TimeDeviationCauseStartId.HasValue ? timeDeviationCauses?.FirstOrDefault(tdc => tdc.TimeDeviationCauseId == e.TimeDeviationCauseStartId)?.Name ?? string.Empty : string.Empty);

                var dto = new AttestEmployeeDayTimeBlockDTO()
                {
                    TimeBlockId = e.TimeBlockId,
                    
                    AccountId = e.AccountStdId,
                    EmployeeChildId = e.EmployeeChildId,
                    EmployeeId = e.EmployeeId,
                    ShiftTypeId = e.ShiftTypeId,
                    TimeBlockDateId = e.TimeBlockDateId,
                    TimeDeviationCauseStartId = e.TimeDeviationCauseStartId,
                    TimeDeviationCauseStopId = e.TimeDeviationCauseStopId,
                    TimeScheduleTemplatePeriodId = e.TimeScheduleTemplatePeriodId,
                    TimeScheduleTemplateBlockBreakId = e.TimeScheduleTemplateBlockBreakId,
                    TimeScheduleTypeId = e.TimeScheduleTypeId,

                    DeviationAccounts = e.DeviationAccounts.ToDTOs(accountDims),
                    TimeCodes = e.TimeCode?.ToDTOs(false).ToList(),

                    StartTime = startTime,
                    StopTime = startTime.Add((e.StopTime - e.StartTime)),
                    IsBreak = e.IsBreak,
                    IsGeneratedFromBreak = e.IsGeneratedFromBreak,
                    IsPreliminary = e.IsPreliminary,
                    ManuallyAdjusted = e.ManuallyAdjusted,
                    TimeDeviationCauseName = timeDeviationCauseName,
                    Comment = e.Comment,
                    IsReadonlyLeft = e.IsAttested || e.HasBreakAdjacentBreakToLeft(l),
                    IsReadonlyRight = e.IsAttested || e.HasBreakAdjacentBreakToRight(l),
                };

                if (e.GuidId.HasValue)
                    dto.GuidId = e.GuidId.Value.ToString();

                if (transactions != null)
                {
                    List<AttestPayrollTransactionDTO> transactionsForTimeBlock = transactions.Where(i => i.TimeBlockId > 0 ? i.TimeBlockId == dto.TimeBlockId : i.GuidIdTimeBlock == dto.GuidId).ToList();
                    foreach (AttestPayrollTransactionDTO transaction in transactionsForTimeBlock)
                    {
                        transaction.GuidIdTimeBlock = dto.GuidId;
                    }

                    dto.IsPresence = transactionsForTimeBlock.Any(i => i.IsWorkTime() || i.IsGrossSalaryTimeHourMonthly());
                    dto.IsAbsence = transactionsForTimeBlock.Any(i => i.IsAbsence() || i.IsTimeAccumulatorMinusTime());
                    dto.IsOvertime = transactionsForTimeBlock.Any(i => i.IsAddedTime() || i.IsOvertimeCompensation() || i.IsOvertimeAddition() || i.IsTimeAccumulatorAddedTime() || i.IsTimeAccumulatorOverTime());
                    dto.IsOutsideScheduleNotOvertime = !dto.IsOvertime && (dto.StartTime < scheduleIn || dto.StopTime > scheduleOut);
                }

                dtos.Add(dto);
            }

            return dtos;
        }

        public static List<TimeBlock> Filter(this List<TimeBlock> l, List<int> timeBlockDateIds)
        {
            return l?.Where(e => timeBlockDateIds.Contains(e.TimeBlockDateId)).ToList() ?? new List<TimeBlock>();
        }

        public static List<TimeBlock> Filter(this List<TimeBlock> l, DateTime date)
        {
            return l?.Where(b => b.TimeBlockDate?.Date == date).ToList() ?? new List<TimeBlock>();
        }

        public static List<TimeBlock> Filter(this List<TimeBlock> l, DateTime dateFrom, DateTime dateTo)
        {
            return l?.Where(b => b.TimeBlockDate != null && CalendarUtility.IsDateInRange(b.TimeBlockDate.Date, dateFrom, dateTo)).ToList() ?? new List<TimeBlock>();
        }

        public static List<TimeBlock> GetNew(this List<TimeBlock> l, bool excludeBreak)
        {
            return l?.Where(i => i.TimeBlockId == 0 && (!excludeBreak || !i.IsBreak) && (i.State == (int)SoeEntityState.Active || i.State == (int)SoeEntityState.Temporary)).ToList() ?? new List<TimeBlock>();
        }

        public static List<TimeBlock> GetWork(this List<TimeBlock> l, bool excludeGeneratedFromBreak)
        {
            return l?.Where(i => !i.IsBreak && (!excludeGeneratedFromBreak || !i.IsGeneratedFromBreak) && i.State == (int)SoeEntityState.Active).OrderBy(i => i.StartTime).ToList() ?? new List<TimeBlock>();
        }

        public static List<TimeBlock> GetBreaks(this List<TimeBlock> l)
        {
            return l?.Where(i => (i.IsBreak || i.IsGeneratedFromBreak) && i.State == (int)SoeEntityState.Active).OrderBy(i => i.StartTime).ToList() ?? new List<TimeBlock>();
        }

        public static List<TimeBlock> GetAllBefore(this List<TimeBlock> l, DateTime time)
        {
            return l?.Where(i => i.StopTime <= time && i.State == (int)SoeEntityState.Active).ToList() ?? new List<TimeBlock>();
        }

        public static List<TimeBlock> GetAllAfter(this List<TimeBlock> l, DateTime time)
        {
            return l?.Where(i => i.StartTime >= time && i.State == (int)SoeEntityState.Active).ToList() ?? new List<TimeBlock>();
        }

        public static List<TimeBlock> GetOutsideBreakWindow(this List<TimeBlock> l, DateTime startTime, DateTime stopTime)
        {
            return l?.Where(i => i.StopTime <= startTime || i.StartTime >= stopTime).ToList() ?? new List<TimeBlock>();
        }

        public static List<TimeBlock> GetInsideBreakWindow(this List<TimeBlock> l, DateTime startTime, DateTime stopTime)
        {
            return l?.Where(i => (i.StartTime >= startTime && i.StopTime <= stopTime) || i.StartTime == startTime && i.StopTime > stopTime).ToList() ?? new List<TimeBlock>();
        }

        public static List<TimeBlock> GetPresence(this List<TimeBlock> l)
        {
            return l?.Where(i => i.IsPresence()).ToList() ?? new List<TimeBlock>();
        }

        public static List<TimeBlock> FilterOvertime(this List<TimeBlock> l, List<int> timeDeviationCauseIds)
        {
            if (timeDeviationCauseIds.IsNullOrEmpty())
                return new List<TimeBlock>();
            return l?.Where(e => e.TimeDeviationCauseStartId.HasValue && timeDeviationCauseIds.Contains(e.TimeDeviationCauseStartId.Value)).ToList() ?? new List<TimeBlock>();
        }

        public static List<TimeBlock> ExcludeZero(this List<TimeBlock> l)
        {
            return l?.Where(t => t.StartTime < t.StopTime).ToList() ?? new List<TimeBlock>();
        }

        public static List<TimeBlock> SortByStart(this List<TimeBlock> l)
        {
            return l?.OrderBy(t => t.StartTime).ThenBy(t => t.StopTime).ToList() ?? new List<TimeBlock>();
        }

        public static void DecideTimeBlockStandby(this List<TimeBlock> l, List<TimeScheduleTemplateBlock> scheduleBlocks, EmployeeGroup employeeGroup, List<TimeDeviationCause> timeDeviationCauses = null)
        {
            if (!l.DoDecideTimeBlockStandby(scheduleBlocks))
                return;

            foreach (TimeBlock e in l)
            {
                TimeScheduleTemplateBlock matchingTemplateBlock = scheduleBlocks.GetMatchingScheduleBlock(e, false);
                TimeDeviationCause timeDeviationCause = e.TimeDeviationCauseStartId.HasValue ? timeDeviationCauses?.FirstOrDefault(i => i.TimeDeviationCauseId == e.TimeDeviationCauseStartId.Value) : null;
                e.CalculatedAsStandby = (matchingTemplateBlock?.IsStandby() ?? false) && (timeDeviationCause?.IsStandby(employeeGroup) ?? false);
            }
        }

        public static bool DoDecideTimeBlockStandby(this List<TimeBlock> l, List<TimeScheduleTemplateBlock> scheduleBlocks)
        {
            if (l.IsNullOrEmpty() || l.All(i => i.CalculatedAsStandby.HasValue) || !scheduleBlocks.ContainsStandby())
                return false;
            return true;
        }

        public static List<TimePayrollTransaction> GetTimePayrollTransactions(this List<TimeBlock> l, DateTime date, bool discardTimeBlockState = false)
        {
            return l?.Filter(date).GetTimePayrollTransactions(discardTimeBlockState) ?? new List<TimePayrollTransaction>();
        }

        public static List<TimePayrollTransaction> GetTimePayrollTransactions(this List<TimeBlock> l, bool discardTimeBlockState = false)
        {
            return l
                .Where(e => discardTimeBlockState || e.State == (int)SoeEntityState.Active)
                .SelectMany(e => e.TimePayrollTransaction.Where(t => t.State == (int)SoeEntityState.Active)).ToList();
        }

        public static List<int> GetTimePayrollTransactionLevel3Ids(this List<TimeBlock> l)
        {
            return l?.SelectMany(e => e.TimePayrollTransaction.GetLevel3Ids()).Distinct().ToList() ?? new List<int>();
        }

        public static TimeBlock GetFirstPresence(this List<TimeBlock> l)
        {
            if (l.IsNullOrEmpty())
                return null;

            return l.Where(i => i.IsPresence()).OrderBy(o => o.StartTime).FirstOrDefault();
        }

        public static TimeBlock GetLastPresence(this List<TimeBlock> l)
        {
            if (l.IsNullOrEmpty())
                return null;

            return l.Where(i => i.IsPresence()).OrderBy(o => o.StartTime).LastOrDefault();
        }

        public static TimeBlock GetFirst(this List<TimeBlock> l)
        {
            return (from tb in l
                    orderby tb.StartTime ascending
                    select tb).FirstOrDefault();
        }

        public static TimeBlock GetLast(this List<TimeBlock> l)
        {
            return (from tb in l
                    orderby tb.StopTime descending
                    select tb).FirstOrDefault();
        }

        public static TimeBlock GetNearest(this List<TimeBlock> l, TimeBlock e, bool excludeNew, bool excludeBreaks)
        {
            TimeBlock nearestTimeBlock = null;
            if (e != null)
            {
                l = l.Where(i => i.StartTime != e.StartTime && i.StopTime != e.StopTime).ToList();
                if (excludeNew && e.State != (int)SoeEntityState.Temporary)
                    l = l.Where(i => i.TimeBlockId > 0).ToList();
                if (excludeBreaks)
                    l = l.Where(i => !i.IsBreak).ToList();
                
                if (l.Any())
                {
                    l = l.OrderBy(i => i.StartTime).ToList();
                    nearestTimeBlock = l.FirstOrDefault(i => i.StartTime == e.StopTime); //TimeBlock is at start of day
                    if (nearestTimeBlock == null)
                        nearestTimeBlock = l.FirstOrDefault(i => i.StopTime == e.StartTime); //TimeBlock is at end of day
                    if (nearestTimeBlock == null)
                        nearestTimeBlock = l.FirstOrDefault(i => i.StartTime > e.StartTime); //Take nearest TimeBlock later on day
                    if (nearestTimeBlock == null)
                        nearestTimeBlock = l.FirstOrDefault(i => i.StartTime < e.StartTime); //Take nearest TimeBlock earlier on day
                }
            }
            return nearestTimeBlock;
        }

        public static TimeBlock GetPrev(this List<TimeBlock> l, TimeBlock e)
        {
            return l.Where(i => i.StopTime <= e.StartTime).OrderByDescending(i => i.StopTime).FirstOrDefault();
        }

        public static TimeBlock GetNext(this List<TimeBlock> l, TimeBlock e)
        {
            return l.Where(i => i.StartTime >= e.StopTime).OrderBy(i => i.StartTime).FirstOrDefault();
        }

        public static TimeBlock Get(this List<TimeBlock> l, int timeBlockId, string guid)
        {
            TimeBlock e = null;
            if (timeBlockId > 0)
                e = l.FirstOrDefault(i => i.TimeBlockId == timeBlockId);
            else if (!String.IsNullOrEmpty(guid))
                e = l.FirstOrDefault(i => i.GuidId.HasValue && i.GuidId.Value.ToString() == guid);
            return e;
        }

        public static TimeBlock Get(this List<TimeBlock> l, DateTime time)
        {
            return l?.Where(i => i.StartTime <= time && i.StopTime >= time).OrderBy(i => i.StartTime).FirstOrDefault();
        }

        public static TimeBlock Get(this List<TimeBlock> l, DateTime startTime, DateTime stopTime)
        {
            return l?.FirstOrDefault(i => i.StartTime == startTime && i.StopTime == stopTime);
        }

        public static TimeBlock GetBasedOnStartTime(this List<TimeBlock> l, DateTime startTime)
        {
            return l?.FirstOrDefault(b => b.StartTime == startTime);
        }

        public static TimeBlock GetBasedOnStopTime(this List<TimeBlock> l, DateTime stopTime)
        {
            return l?.FirstOrDefault(b => b.StopTime == stopTime);
        }

        public static (TimeCodeTransaction, TimeBlock) GetFirstTimeBlock(this List<TimeBlock> l, List<TimeCodeTransaction> timeCodeTransactions)
        {
            foreach (TimeCodeTransaction timeCodeTransaction in timeCodeTransactions.Where(i => i.TimeBlock != null))
            {
                TimeBlock timeBlock = l.Get(timeCodeTransaction.TimeBlock.StartTime, timeCodeTransaction.TimeBlock.StopTime);
                if (timeBlock != null)
                    return (timeCodeTransaction, timeBlock);
            }

            return (null, null);
        }

        public static AccountInternal GetAccountInternal(this TimeBlock e, int accountDimId)
        {
            if (e.AccountInternal.IsNullOrEmpty())
                return null;

            try
            {
                foreach (var accountInternal in e.AccountInternal)
                {
                    if (!accountInternal.AccountReference.IsLoaded)
                    {
                        accountInternal.AccountReference.Load();
                        DataProjectLogCollector.LogLoadedEntityInExtension("TimeBlock.cs accountInternal.AccountReference");
                    }
                }
            }
            catch (InvalidOperationException ioExc)
            {
                ioExc.ToString(); //prevent compiler warning (entity cannot be loaded)
            }

            return e.AccountInternal?.FirstOrDefault(i => i.Account?.AccountDimId == accountDimId);
        }

        public static List<DateTime> GetDates(this IEnumerable<TimeBlock> l)
        {
            return l?.Where(e => e.TimeBlockDate != null).Select(e => e.TimeBlockDate.Date).Distinct().ToList() ?? new List<DateTime>();
        }

        public static DateTime GetFirstTimeStampTime(this TimeBlock e)
        {
            if (e.TimeStampEntry.IsNullOrEmpty() || !e.TimeStampEntry.Any(a => CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, a.Time) >= e.StartTime && CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, a.Time) <= e.StopTime))
                return e.StartTime;

            return CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, e.TimeStampEntry.First(w => CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, w.Time) >= e.StartTime && CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, w.Time) <= e.StopTime).Time);
        }

        public static DateTime GetLastTimeStampTime(this TimeBlock e)
        {
            if (e.TimeStampEntry.IsNullOrEmpty() || !e.TimeStampEntry.Any(a => CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, a.Time) >= e.StartTime && CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, a.Time) <= e.StopTime))
                return e.StartTime;

            return CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, e.TimeStampEntry.Last(w => CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, w.Time) >= e.StartTime && CalendarUtility.MergeDateAndTime(CalendarUtility.DATETIME_DEFAULT, w.Time) <= e.StopTime).Time);
        }

        public static DateTime GetStartTime(this List<TimeBlock> l)
        {
            if (l.IsNullOrEmpty())
                return CalendarUtility.DATETIME_DEFAULT;

            return l.GetFirst().StartTime;
        }

        public static DateTime GetStopTime(this List<TimeBlock> l)
        {
            if (l.IsNullOrEmpty())
                return CalendarUtility.DATETIME_DEFAULT;

            return l.GetLast().StopTime;
        }

        public static DateTime? GetLastUnlockedDate(this List<TimeBlock> l, DateTime dateFrom, DateTime? dateTo, List<int> lockedAttestStateIds)
        {
            if (l.IsNullOrEmpty())
                return dateFrom;

            List<TimePayrollTransaction> timePayrollTransactions = l.GetTimePayrollTransactions();
            if (timePayrollTransactions.IsNullOrEmpty())
                return dateFrom;

            return timePayrollTransactions.GetLastUnlockedDate(dateFrom, dateTo ?? l.Max(i => i.TimeBlockDate.Date), lockedAttestStateIds);
        }

        public static int? GetWholedayDeviationTimeDeviationCauseId(this List<TimeBlock> l)
        {
            int? timeDeviationCauseId = null;

            if (!l.IsNullOrEmpty())
            {
                List<int> timeDeviationCauseIds = l.GetWork(false).Where(i => i.TimeDeviationCauseStartId.HasValue).Select(i => i.TimeDeviationCauseStartId.Value).Distinct().ToList();
                if (timeDeviationCauseIds.Count == 1)
                    timeDeviationCauseId = timeDeviationCauseIds.First();
            }

            return timeDeviationCauseId;
        }

        public static int GetWorkMinutes(this List<TimeBlock> l, bool excludeGeneratedFromBreak)
        {
            return l.GetWork(excludeGeneratedFromBreak).GetMinutes();
        }

        public static int GetBreakMinutes(this List<TimeBlock> l)
        {
            return l.GetBreaks().GetMinutes();
        }

        public static int GetMinutes(this List<TimeBlock> l)
        {
            if (l.IsNullOrEmpty())
                return 0;

            return l.Sum(e => e.GetMinutes());
        }

        public static int GetMinutes(this TimeBlock e)
        {
            return (int)e.StopTime.Subtract(e.StartTime).TotalMinutes;
        }

        public static SoeTimeBlockDeviationChange GetDeviationChange(this TimeBlock e, DateTime startTime, DateTime stopTime, SoeTimeBlockClientChange clientChange = SoeTimeBlockClientChange.None)
        {
            if (e.IsNew)
                return SoeTimeBlockDeviationChange.NewTimeBlock;
            if (e.StartTime == startTime && startTime == stopTime)
                return SoeTimeBlockDeviationChange.DeleteTimeBlock;
            if (startTime > stopTime)
                return clientChange == SoeTimeBlockClientChange.Left ? SoeTimeBlockDeviationChange.DeleteTimeBlockAdvancedFromLeft : SoeTimeBlockDeviationChange.DeleteTimeBlockAdvancedFromRight;
            if (e.StartTime != startTime && e.StopTime != stopTime)
                return SoeTimeBlockDeviationChange.ResizeBothSides;
            if (e.StartTime > startTime)
                return SoeTimeBlockDeviationChange.ResizeStartLeftDragLeft;
            else if (e.StartTime < startTime)
                return SoeTimeBlockDeviationChange.ResizeStartLeftDragRight;
            else if (e.StopTime > stopTime)
                return SoeTimeBlockDeviationChange.ResizeStartRightDragLeft;
            else if (e.StopTime < stopTime)
                return SoeTimeBlockDeviationChange.ResizeStartRightDragRight;
            else
                return SoeTimeBlockDeviationChange.None;
        }

        public static SoeTimeRuleType GetDeviationRuleTypeForStartChange(this TimeBlock e, TimeDeviationCause timeDeviationCause, DateTime scheduleIn, DateTime newStartTime)
        {
            if (timeDeviationCause != null)
            {
                if (timeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.Absence)
                    return SoeTimeRuleType.Absence;
                else if (timeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.Presence)
                    return SoeTimeRuleType.Presence;
                else if (timeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence && e.StartTime == scheduleIn && newStartTime > scheduleIn)
                    return SoeTimeRuleType.Absence;
            }
            return SoeTimeRuleType.Presence;
        }

        public static SoeTimeRuleType GetDeviationRuleTypeForStopChange(this TimeBlock e, TimeDeviationCause timeDeviationCause, DateTime scheduleOut, DateTime newStopTime)
        {
            if (timeDeviationCause != null)
            {
                if (timeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.Absence)
                    return SoeTimeRuleType.Absence;
                else if (timeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.Presence)
                    return SoeTimeRuleType.Presence;
                else if (timeDeviationCause.Type == (int)TermGroup_TimeDeviationCauseType.PresenceAndAbsence && e.StopTime == scheduleOut && newStopTime < scheduleOut)
                    return SoeTimeRuleType.Absence;
            }
            return SoeTimeRuleType.Presence;
        }

        public static void SetIsAttested(this List<TimeBlock> l, AttestStateDTO attestStateInitialPayroll, AttestStateDTO attestStateIntitialInvoice)
        {
            if (l == null)
                return;

            l.SetIsAttestedPayroll(attestStateInitialPayroll);
            l.SetIsAttestedInvoice(attestStateIntitialInvoice);
        }

        public static void SetIsAttestedPayroll(this List<TimeBlock> l, AttestStateDTO attestStateInitial, List<AttestPayrollTransactionDTO> transactions = null)
        {
            if (l == null || attestStateInitial == null)
                return;

            l.SetIsAttestedPayroll(attestStateInitial.AttestStateId, transactions);
        }

        public static void SetIsAttestedPayroll(this List<TimeBlock> l, int attestStateIdInitial, List<AttestPayrollTransactionDTO> transactions = null)
        {
            if (l == null)
                return;

            foreach (TimeBlock timeBlock in l)
            {
                timeBlock.SetIsAttestedPayroll(attestStateIdInitial, transactions);
            }
        }

        public static void SetIsAttestedInvoice(this List<TimeBlock> l, AttestStateDTO attestStateInitial)
        {
            if (l == null || attestStateInitial == null)
                return;

            foreach (TimeBlock timeBlock in l)
            {
                timeBlock.SetIsAttestedInvoice(attestStateInitial);
            }
        }

        public static bool SetIsAttestedPayroll(this TimeBlock e, AttestStateDTO attestStateInitial, List<AttestPayrollTransactionDTO> transactions = null)
        {
            if (e == null || attestStateInitial == null)
                return false;
            return e.SetIsAttestedPayroll(attestStateInitial.AttestStateId, transactions);
        }

        public static bool SetIsAttestedPayroll(this TimeBlock e, int attestStateIdInitial, List<AttestPayrollTransactionDTO> transactions = null)
        {
            if (e == null)
                return false;

            if (e.TimePayrollTransaction != null && e.TimePayrollTransaction.IsLoaded)
            {
                if (e.TimePayrollTransaction.Any(i => i.AttestStateId != attestStateIdInitial && i.State == (int)SoeEntityState.Active))
                    e.IsAttested = true;
            }
            else if (transactions != null && transactions.Any(i => i.AttestStateId != attestStateIdInitial && i.TimeBlockId == e.TimeBlockId))
            {
                e.IsAttested = true;
            }

            return e.IsAttested;
        }

        public static bool SetIsAttestedInvoice(this TimeBlock e, AttestStateDTO attestStateInitial)
        {
            if (e == null || attestStateInitial == null)
                return false;
            return e.SetIsAttestedInvoice(attestStateInitial.AttestStateId);
        }

        public static bool SetIsAttestedInvoice(this TimeBlock e, int attestStateIdInitial)
        {
            if (e == null)
                return false;

            if (e.TimeInvoiceTransaction != null && e.TimeInvoiceTransaction.Any(i => i.AttestStateId != attestStateIdInitial && i.State == (int)SoeEntityState.Active))
                e.IsAttested = true;

            return e.IsAttested;
        }

        public static bool HasAnyToTheLeft(this List<TimeBlock> l, DateTime time, int? discardTimeBlockId = null, bool discardStandby = false)
        {
            return l.Any(i => i.StopTime <= time && (!discardTimeBlockId.HasValue || discardTimeBlockId.Value != i.TimeBlockId) && (!discardStandby || i.CalculatedAsStandby == false));
        }

        public static bool HasAnyToTheRight(this List<TimeBlock> l, DateTime time, int? discardTimeBlockId = null, bool discardStandby = false)
        {
            return l.Any(i => i.StartTime >= time && (!discardTimeBlockId.HasValue || discardTimeBlockId.Value != i.TimeBlockId) && (!discardStandby || i.CalculatedAsStandby == false));
        }

        public static bool HasBreakAdjacentBreakToLeft(this TimeBlock e, List<TimeBlock> l)
        {
            if (l == null || e == null || !e.IsBreakOrGeneratedFromBreak)
                return false;
            return l.Any(i => i.IsBreakOrGeneratedFromBreak && i.StopTime == e.StartTime && i.StartTime < i.StopTime);
        }

        public static bool HasBreakAdjacentBreakToRight(this TimeBlock e, List<TimeBlock> l)
        {
            if (l == null || e == null || !e.IsBreakOrGeneratedFromBreak)
                return false;
            return l.Any(i => i.IsBreakOrGeneratedFromBreak && i.StartTime == e.StopTime && i.StartTime < i.StopTime);
        }

        public static bool HasAccountStd(this TimeBlock e)
        {
            return (e.AccountStdId.HasValue && e.AccountStdId.Value > 0) || e.AccountStd != null;
        }

        public static bool HasAccountInternals(this TimeBlock e)
        {
            return e.AccountInternal != null && e.AccountInternal.Count > 0;
        }

        public static bool IsValidDeviationChange(this List<TimeBlock> l, DateTime startTime, DateTime stopTime, SoeTimeBlockDeviationChange deviationChange)
        {
            switch (deviationChange)
            {
                case SoeTimeBlockDeviationChange.ResizeStartLeftDragRight:
                    if (startTime > l.GetStopTime())
                        return false;
                    break;
                case SoeTimeBlockDeviationChange.ResizeStartRightDragLeft:
                    if (stopTime < l.GetStartTime())
                        return false;
                    break;
                case SoeTimeBlockDeviationChange.ResizeStartLeftDragLeft:
                case SoeTimeBlockDeviationChange.ResizeStartRightDragRight:
                    if (startTime > stopTime)
                        return false;
                    break;
                case SoeTimeBlockDeviationChange.DeleteTimeBlockAdvancedFromLeft:
                case SoeTimeBlockDeviationChange.DeleteTimeBlockAdvancedFromRight:
                    if (startTime <= stopTime)
                        return false;
                    break;
            }

            return true;
        }

        public static bool IsNewOverlappedByCurrent(this List<TimeBlock> l, DateTime newStartTime, DateTime newStopTime)
        {
            if (l.IsNullOrEmpty())
                return false;

            foreach (TimeBlock e in l)
            {
                if (CalendarUtility.IsNewOverlappedByCurrent(newStartTime, newStopTime, e.StartTime, e.StopTime))
                    return true;
            }

            return false;
        }

        public static bool IsOverlappedByDeviationChange(this TimeBlock e, TimeBlock currentTimeBlock)
        {
            if (currentTimeBlock == null)
                return false;
            return CalendarUtility.IsNewOverlappedByCurrent(e.StartTime, e.StopTime, currentTimeBlock.StartTime, currentTimeBlock.StopTime);
        }

        public static bool IsInvolvedInDeviationChange(this TimeBlock e, TimeBlock currentTimeBlock, SoeTimeBlockDeviationChange deviationChange)
        {
            if (currentTimeBlock == null)
                return false;

            switch (deviationChange)
            {
                case SoeTimeBlockDeviationChange.ResizeStartLeftDragLeft:
                case SoeTimeBlockDeviationChange.ResizeStartLeftDragRight:
                    if (e.StartTime >= currentTimeBlock.StopTime)
                        return false;
                    break;
                case SoeTimeBlockDeviationChange.ResizeStartRightDragLeft:
                case SoeTimeBlockDeviationChange.ResizeStartRightDragRight:
                    if (e.StartTime <= currentTimeBlock.StartTime)
                        return false;
                    break;
            }

            return true;
        }

        public static bool IsZeroBlock(this TimeBlock e)
        {
            return e.StartTime == e.StopTime && e.StartTime.Hour == 0 && e.StartTime.Minute == 0;
        }

        public static bool IsScheduleTime(this TimeBlock e, List<int> timeScheduleTypeIdsIsNotScheduleTime)
        {
            if (e == null || !e.CalculatedTimeScheduleTypeId.HasValue || timeScheduleTypeIdsIsNotScheduleTime.IsNullOrEmpty())
                return true;

            return !timeScheduleTypeIdsIsNotScheduleTime.Contains(e.CalculatedTimeScheduleTypeId.Value);
        }

        public static bool IsPresence(this TimeBlock e)
        {
            return
                (e.TimeCode != null && e.TimeCode.Any(t => t.Type == (int)SoeTimeCodeType.Work))
                ||
                e.CalculatedAsPresence == true;
        }

        public static bool IsAllAbsence(this List<TimeBlock> l)
        {
            if (l.IsNullOrEmpty())
                return false;

            foreach (var e in l)
            {
                if (!e.IsAbsence())
                    return false;
            }

            return true;
        }

        public static bool IsAbsence(this TimeBlock e)
        {
            return
                (e.TimeCode != null && e.TimeCode.Any(t => t.Type == (int)SoeTimeCodeType.Absense))
                ||
                e.CalculatedAsAbsence == true;
        }

        public static bool IsBreak(this TimeBlock timeBlock)
        {
            if (timeBlock == null)
                return false;
            if (timeBlock.IsBreak || (timeBlock.TimeCode?.Any(i => i.Type == (int)SoeTimeCodeType.Break) ?? false))
                return true;
            return false;
        }

        public static bool IsScheduleBlockOverlappedTimeBlock(this TimeScheduleTemplateBlock scheduleBlock, TimeBlock currentTimeBlock)
        {
            return CalendarUtility.IsNewOverlappedByCurrent(scheduleBlock.StartTime, scheduleBlock.StopTime, currentTimeBlock.StartTime, currentTimeBlock.StopTime);
        }

        public static bool IsScheduleBlockOverlappedTimeBlock(this TimeScheduleTemplateBlockDTO scheduleBlock, TimeBlock currentTimeBlock)
        {
            return CalendarUtility.IsNewOverlappedByCurrent(scheduleBlock.StartTime, scheduleBlock.StopTime, currentTimeBlock.StartTime, currentTimeBlock.StopTime);
        }

        public static bool IsAnyAbsenceTimeBlockOverlappingScheduleBlock(this TimeScheduleTemplateBlockDTO scheduleBlock, List<TimeBlock> absenceTimeBlocks)
        {
            return scheduleBlock.GetAbsenceTimeBlockOverlappingScheduleBlock(absenceTimeBlocks) != null;
        }

        public static bool IsBreakSurroundedByAbsence(this TimeBlock e, List<TimeBlock> l)
        {
            if (e == null || !e.IsBreak)
                return false;

            var work = l.GetPresence();
            var before = work.Where(i => i.StartTime <= e.StartTime).OrderByDescending(i => i.StartTime).FirstOrDefault();
            var after = work.Where(i => i.StopTime >= e.StopTime).OrderBy(i => i.StopTime).FirstOrDefault();
            if (before == null && after == null)
                return false;
            return (before == null || before.IsAbsence()) && (after == null || after.IsAbsence());
        }

        public static bool ContainsVacationFiveDaysPerWeek(this List<TimeBlock> l)
        {
            foreach (TimeBlock e in l)
            {
                foreach (TimePayrollTransaction timePayrollTransaction in e.TimePayrollTransaction)
                {
                    if (timePayrollTransaction.IsVacationFiveDaysPerWeek || timePayrollTransaction.IsAbsenceVacationNoVacationDaysDeducted())
                        return true;
                }
            }
            return false;
        }

        public static bool IsFromProjectTimeBlock(this List<TimeBlock> l)
        {
            return l?.Any(x => x.ProjectTimeBlockId.HasValue && x.ProjectTimeBlockId.Value > 0) ?? false;
        }

        public static bool IsNoneOverlapping(this List<TimeBlock> l, DateTime startTime, DateTime stopTime)
        {
            if (l.IsNullOrEmpty())
                return false;

            foreach (var e in l)
            {
                if (CalendarUtility.IsDatesOverlapping(e.StartTime, e.StopTime, startTime, stopTime))
                    return false;
            }

            return true;
        }

        public static bool IsOnDate(this TimeBlock e, DateTime date)
        {
            return e.TimeBlockDate?.Date == date;
        }

        public static bool ContainsDuplicateTimeBlocks(this List<TimeBlock> l)
        {
            return l.TryGetDuplicateTimeBlocks(out _);
        }

        public static bool ContainsTimeDeviationCause(this List<TimeBlock> l, List<int> timeDeviationCauseIds)
        {
            if (l.IsNullOrEmpty() || timeDeviationCauseIds.IsNullOrEmpty())
                return false;
            return l.Any(e => e.TimeDeviationCauseStartId.HasValue && timeDeviationCauseIds.Contains(e.TimeDeviationCauseStartId.Value));
        }

        public static bool TryAddTimeCode(this TimeBlock e, TimeCode timeCode)
        {
            if (e?.TimeCode == null || timeCode == null)
                return false;

            e.TimeCode.Add(timeCode);
            return true;
        }

        public static bool TryAdjustAfterCurrent(this TimeBlock e, TimeBlock currentTimeBlock, SoeTimeBlockDeviationChange deviationChange)
        {
            switch (deviationChange)
            {
                case SoeTimeBlockDeviationChange.ResizeStartLeftDragLeft:
                case SoeTimeBlockDeviationChange.ResizeStartRightDragLeft:
                    if (CalendarUtility.IsNewStopInCurrent(e.StartTime, e.StopTime, currentTimeBlock.StartTime, currentTimeBlock.StopTime))
                        e.StopTime = currentTimeBlock.StartTime;
                    break;
                case SoeTimeBlockDeviationChange.ResizeStartLeftDragRight:
                case SoeTimeBlockDeviationChange.ResizeStartRightDragRight:
                    if (CalendarUtility.IsNewStartInCurrent(e.StartTime, e.StopTime, currentTimeBlock.StartTime, currentTimeBlock.StopTime))
                        e.StartTime = currentTimeBlock.StopTime;
                    break;
            }

            return e.StartTime < e.StopTime;
        }

        public static bool TryGetAbsenceSysPayrollTypeLevel3s(this List<TimeBlock> l, out List<int> sysPayrollTypeLevel3s)
        {
            sysPayrollTypeLevel3s = new List<int>();
            foreach (var e in l)
            {
                foreach (TimePayrollTransaction timePayrollTransaction in e.TimePayrollTransaction)
                {
                    if (!timePayrollTransaction.SysPayrollTypeLevel2.HasValue || timePayrollTransaction.SysPayrollTypeLevel2.Value != (int)TermGroup_SysPayrollType.SE_GrossSalary_Absence)
                        continue;
                    if (!timePayrollTransaction.SysPayrollTypeLevel3.HasValue)
                        continue;
                    if (!sysPayrollTypeLevel3s.Contains(timePayrollTransaction.SysPayrollTypeLevel3.Value))
                        sysPayrollTypeLevel3s.Add(timePayrollTransaction.SysPayrollTypeLevel3.Value);
                }
            }
            return sysPayrollTypeLevel3s.Count > 0;
        }

        public static bool TryGetDuplicateTimeBlocks(this List<TimeBlock> l, out Dictionary<string, List<int>> duplicateTimes)
        {
            duplicateTimes = new Dictionary<string, List<int>>();

            if (!l.IsNullOrEmpty())
            {
                foreach (var timeBlocksGroupedByTime in l.Where(i => i.State == (int)SoeEntityState.Active && i.StartTime < i.StopTime).GroupBy(i => i.TimeString))
                {
                    if (timeBlocksGroupedByTime.Count() > 1)
                        duplicateTimes.Add(timeBlocksGroupedByTime.Key, timeBlocksGroupedByTime.Select(i => i.TimeBlockId).ToList());
                }
            }

            return !duplicateTimes.IsNullOrEmpty();
        }

        public static bool TryGetDuplicateJson(this List<TimeBlock> timeBlocks, out string json, Employee employee, TimeBlockDate timeBlockDate, int? taskId, params string[] comments)
        {
            json = "";
            if (timeBlocks.IsNullOrEmpty() || employee == null || timeBlockDate == null)
                return false;
            if (!timeBlocks.TryGetDuplicateTimeBlocks(out Dictionary<string, List<int>> duplicateTimes))
                return false;

            dynamic d = new ExpandoObject();
            d.DuplicateTimeBlocks = DuplicateTimeBlock.Create(employee.ActorCompanyId, employee.EmployeeId, timeBlockDate.TimeBlockDateId, timeBlockDate.Date, duplicateTimes, taskId, comments.ToList());
            d.TimeBlocks = timeBlocks.GetInfoObjects();
            json = JsonConvert.SerializeObject(d);

            return true;
        }

        public static bool TryGetDuplicateJson(this List<TimeBlock> timeBlocks, out string json, TimeBlock timeBlock, int actorCompanyId, int? taskId, params string[] comments)
        {
            json = "";
            if (timeBlocks.IsNullOrEmpty() || timeBlock == null)
                return false;

            dynamic d = new ExpandoObject();
            d.DuplicateTimeBlock = DuplicateTimeBlock.Create(actorCompanyId, timeBlock.EmployeeId, timeBlock.TimeBlockDateId, timeBlock.TimeBlockDate?.Date, timeBlock.TimeString, timeBlock.TimeBlockId.ObjToList(), taskId, comments.ToList());
            d.TimeBlock = timeBlock.GetInfoObject();
            d.TimeBlocks = timeBlocks.GetInfoObjects();
            json = JsonConvert.SerializeObject(d);

            return true;
        }

        public static List<dynamic> GetInfoObjects(this List<TimeBlock> l)
        {
            var d = new List<dynamic>();
            l.ForEach(tb => d.Add(tb.GetInfoObject()));
            return d;
        }

        public static dynamic GetInfoObject(this TimeBlock e)
        {
            dynamic d = new ExpandoObject();
            if (e != null)
            {
                d.TimeBlockId = e.TimeBlockId;
                d.TimeBlockDateId = e.TimeBlockDateId;
                d.TimeString = e.TimeString;
                d.IsBreak = e.IsBreak;
                d.State = e.State;
                d.DebugInfo = e.DebugInfo;
            }
            return d;
        }

        public static string ToJson(this List<TimeBlock> l)
        {
            return JsonConvert.SerializeObject(l.GetInfoObjects());
        }

        public static void AddTimeDeviationCauseOrDefaultIfMissing(this List<TimeBlock> l, TimeDeviationCause deviationStartCause, TermGroup_TimeDeviationCauseType type, TimeCode defaultCompanyTimeCode)
        {
            if (l != null)
            {
                foreach (var e in l)
                {
                    if (e.TimeCode != null && e.TimeCode.Any())
                        continue;
                    e.AddTimeDeviationCauseOrDefault(deviationStartCause, type, defaultCompanyTimeCode);
                }
            }
        }

        public static void AddTimeDeviationCauseOrDefault(this TimeBlock e, TimeDeviationCause deviationStartCause, TermGroup_TimeDeviationCauseType type, TimeCode defaultCompanyTimeCode)
        {
            if (e == null || deviationStartCause == null)
                return;

            if (deviationStartCause.TimeCode != null)
                e.TimeCode.Add(deviationStartCause.TimeCode);
            else if (type == TermGroup_TimeDeviationCauseType.Presence && defaultCompanyTimeCode != null)
                e.TimeCode.Add(defaultCompanyTimeCode);
        }

        #endregion
    }
}
