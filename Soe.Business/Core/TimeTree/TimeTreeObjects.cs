using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Interfaces.Common;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeTree
{
    public class TimeTreeDayDTO
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public Guid Key { get; }

        public DateTime ScheduleIn { get; set; }
        public DateTime ScheduleOut { get; set; }
        public DateTime? StandbyIn { get; set; }
        public DateTime? StandbyOut { get; set; }
        public TimeSpan ScheduleTime { get; set; }
        public TimeSpan ScheduleBreakTime { get; set; }
        public TimeSpan StandbyTime { get; set; }
        public TimeSpan PresenceTime { get; set; }
        public TimeSpan PresenceBreakTime { get; set; }
        public TimeSpan AbsenceTime { get; set; }
        public bool HasSchedule
        {
            get
            {
                return this.ScheduleTime.TotalMinutes > 0;
            }
        }
        public bool HasScheduleWithoutTransactions
        {
            get
            {
                return this.HasSchedule && !this.HasTransactions && CalendarUtility.IsBeforeNow(this.Date, this.ScheduleOut);
            }
        }
        public bool HasPresenceTime
        {
            get
            {
                return this.PresenceTime.TotalMinutes > 0;
            }
        }
        public bool HasWorkedInsideSchedule { get; set; }
        public bool HasWorkedOutsideSchedule { get; set; }
        public bool HasTimeStampEntrys { get; set; }
        public bool HasTransactions { get; set; }

        public TimeTreeDayDTO(int employeeId, DateTime date)
        {
            this.Key = Guid.NewGuid();
            this.EmployeeId = employeeId;
            this.Date = date;

            this.ScheduleIn = CalendarUtility.DATETIME_DEFAULT;
            this.ScheduleOut = CalendarUtility.DATETIME_DEFAULT;
            this.StandbyIn = null;
            this.StandbyOut = null;
            this.ScheduleTime = new TimeSpan();
            this.ScheduleBreakTime = new TimeSpan();
            this.StandbyTime = new TimeSpan();
            this.PresenceTime = new TimeSpan();
            this.PresenceBreakTime = new TimeSpan();
            this.AbsenceTime = new TimeSpan();
        }
    }

    public class TimeTreeEmployeeInfoDTO
    {
        public Employee Employee { get; set; }
        public List<DateTime> EmploymentDates { get; set; }
        public Employment Employment { get; set; }
        public EmployeeGroup EmployeeGroup { get; set; }
        public List<AttestState> AttestStates { get; set; }

        public int EmployeeId
        {
            get
            {
                return this.Employee?.EmployeeId ?? 0;
            }
        }
        public bool AutogenTimeblocks
        {
            get
            {
                return this.EmployeeGroup?.AutogenTimeblocks ?? false;
            }
        }

        public TimeTreeEmployeeInfoDTO(Employee employee, List<EmployeeGroup> employeeGroups, DateTime startDate, DateTime stopDate)
        {
            this.Employee = employee;
            this.EmploymentDates = this.Employee?.GetEmploymentDates(startDate, stopDate) ?? new List<DateTime>();
            this.Employment = this.EmploymentDates.Any() ? employee?.GetEmployment(this.EmploymentDates.First()) : null;
            this.EmployeeGroup = this.Employment?.GetEmployeeGroup(startDate, employeeGroups);
            this.AttestStates = employee?.TimeTreeAttestStates;
        }

        public bool IsValid()
        {
            return this.Employee != null && this.Employment != null && this.EmployeeGroup != null;
        }
    }

    public class TimeTreeScheduleBlockDTO : IScheduleBlockObject, IScheduleBlockAccounting
    {
        public int? EmployeeId { get; set; }
        public int TimeScheduleTemplateBlockId { get; set; }
        public int? TimeScheduleTemplatePeriodId { get; set; }
        public int? TimeScheduleScenarioHeadId { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public int? AccountId { get; set; }
        public DateTime? Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public int Type { get; set; }
        public string Link { get; set; }
        public bool IsZeroBlock
        {
            get
            {
                return this.StartTime == this.StopTime;
            }
        }
        public bool IsPreliminary { get; set; }
        public bool IsBreak { get; set; }
        public int BreakType { get; set; }
        public int BreakNumber { get; set; }
        public int BreakMinutes { get; set; }

        //TimeCode
        public int TimeCodeId { get; set; }
        public string TimeCodeName { get; set; }
        public int TimeCodeType { get; set; }

        //ShiftType
        public int? ShiftTypeId { get; set; }
        public string ShiftTypeName { get; set; }
        public string ShiftTypeColor { get; set; }
        public string ShiftTypeDescription { get; set; }
        public int ShiftUserStatus { get; set; } //TermGroup_TimeScheduleTemplateBlockShiftUserStatus

        //TimeScheduleType
        public int? TimeScheduleTypeId { get; set; }
        public string TimeScheduleTypeName { get; set; }
        public string TimeScheduleTypeColor { get; set; }
        public bool IsNotScheduleTime { get; set; }

        //RecalculateTimeRecord
        public int? RecalculateTimeRecordId { get; set; }
        public int RecalculateTimeRecordStatus { get; set; } //TermGroup_RecalculateTimeRecordStatus

        //AbsenceType
        public int? AbsenceType { get; set; }
    }

    public class TimeTreeTimeBlockDTO
    {
        public int TimeBlockId { get; set; }
        public int TimeBlockDateId { get; set; }
        public int EmployeeId { get; set; }
        public int? TimeDeviationCauseStartId { get; set; }
        public int? TimeDeviationCauseStopId { get; set; }
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public IEnumerable<int> TimeCodeTypes { get; set; }
    }

    public class TimeTreeProjectTimeBlockDTO
    {
        public int ProjectTimeBlockId { get; set; }
        public int TimeBlockDateId { get; set; }
        public int EmployeeId { get; set; }
        public List<int> TimeCodeTransactionIds { get; set; }
        public int InvoiceQuantity { get; set; }
    }

    public class TimePayrollTransactionTreeDTO : IPayrollTransaction, IPayrollTransactionAccounting
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int ProductId { get; set; }
        public int AttestStateId { get; set; }
        public int TimeBlockDateId { get; set; }
        public int? TimeCodeTransactionId { get; set; }
        public int? TimePeriodId { get; set; }
        public int? UnionFeeId { get; set; }
        public int? EmployeeVehicleId { get; set; }
        public int? RetroactivePayrollOutcomeId { get; set; }
        public int? PayrollStartValueRowId { get; set; }        
        public decimal Quantity { get; set; }
        public decimal? Amount { get; set; }
        public DateTime Date { get; set; }
        public int? SysPayrollTypeLevel1 { get; set; }
        public int? SysPayrollTypeLevel2 { get; set; }
        public int? SysPayrollTypeLevel3 { get; set; }
        public int? SysPayrollTypeLevel4 { get; set; }
        public bool IsAdded { get; set; }
        public bool IsFixed { get; set; }
        public bool IsCentRounding { get; set; }
        public bool IsQuantityRounding { get; set; }
        public bool IsAdditionOrDeduction { get; set; }
        public bool PayrollProductUseInPayroll { get; set; }
        public List<int> AccountInternalIds { get; set; }
    }

    public static class TimeTreeExtensions
    {
        #region TimeTreeDayDTO

        public static TimeTreeDayDTO GetOrCreateDay(this List<TimeTreeDayDTO> l, int employeeId, DateTime date)
        {
            TimeTreeDayDTO e = l.FirstOrDefault(i => i.Date == date);
            if (e == null)
            {
                e = new TimeTreeDayDTO(employeeId, date);
                l.Add(e);
            }
            return e;
        }

        #endregion

        #region TimeTreeScheduleBlockDTO

        public static List<TimeTreeScheduleBlockDTO> GetWork(this List<TimeTreeScheduleBlockDTO> l)
        {
            return l?.Where(i => !i.IsBreak && i.StartTime <= i.StopTime).OrderBy(i => i.StartTime).ToList() ?? new List<TimeTreeScheduleBlockDTO>();
        }

        public static List<TimeTreeScheduleBlockDTO> GetBreaks(this List<TimeTreeScheduleBlockDTO> l)
        {
            return l?.Where(i => i.IsBreak && i.StartTime <= i.StopTime).OrderBy(i => i.StartTime).ToList() ?? new List<TimeTreeScheduleBlockDTO>();
        }

        public static bool IsSchedule(this TimeTreeScheduleBlockDTO e)
        {
            return e.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Schedule;
        }

        public static bool IsStandby(this TimeTreeScheduleBlockDTO e)
        {
            return e.Type == (int)TermGroup_TimeScheduleTemplateBlockType.Standby;
        }

        public static bool IsOnDuty(this TimeTreeScheduleBlockDTO e)
        {
            return e.Type == (int)TermGroup_TimeScheduleTemplateBlockType.OnDuty;
        }

        public static bool IsBreak(this TimeTreeScheduleBlockDTO e)
        {
            return
                (e.BreakType > (int)SoeTimeScheduleTemplateBlockBreakType.None) ||
                (e.TimeCodeType == (int)SoeTimeCodeType.Break) ||
                (e.BreakNumber > 0); //Should be removed later
        }

        public static List<WorkIntervalDTO> GetShiftsBasedOnScheduleHoles<T>(this List<T> l) where T: IScheduleBlockObject
        {
            if (l.IsNullOrEmpty())
                return new List<WorkIntervalDTO>();

            List<WorkIntervalDTO> shifts = new List<WorkIntervalDTO>();
            WorkIntervalDTO currentShift = null;
            WorkIntervalDTO GetCurrentShift() => currentShift;
            void AddCurrentShift(T shift = default(T))
            {
                if (currentShift != null)
                    shifts.Add(currentShift);
                currentShift = shift != null ? new WorkIntervalDTO(shift.TimeScheduleTemplateBlockId, shift.StartTime, shift.StopTime) : null;
            }
            void ExtendCurrentShift(T shift)
            {
                if (currentShift != null && shift != null)
                    currentShift.StopTime = shift.StopTime;
            }

            foreach (var shift in l.Where(i => !i.IsBreak && i.StartTime < i.StopTime).OrderBy(i => i.StartTime))
            {
                currentShift = GetCurrentShift();
                if (currentShift == null || currentShift.StopTime < shift.StartTime)
                    AddCurrentShift(shift);
                else
                    ExtendCurrentShift(shift);
                    
            }
            AddCurrentShift();

            return shifts;
        }

        public static void SetAccountOnZeroBlock(this List<TimeTreeScheduleBlockDTO> l, int accountId)
        {
            if (l.IsNullOrEmpty())
                return;

            //Temporary set zero-blocks to currentAccountId so wont be marked as lended. Zero-blocks belongs to all accounts.
            l.Where(b => b.StartTime == b.StopTime).ToList().ForEach(b => b.AccountId = accountId);
        }

        #endregion

        #region TimeTreeTimeBlockDTO

        public static List<DateTime> GetDatesWithTimeDeviationCause(this List<TimeTreeTimeBlockDTO> l, List<int> timeDeviationCauseIds)
        {
            if (l.IsNullOrEmpty() || timeDeviationCauseIds.IsNullOrEmpty())
                return null;
            return l.Where(e => e.TimeDeviationCauseStartId.HasValue && timeDeviationCauseIds.Contains(e.TimeDeviationCauseStartId.Value)).Select(e => e.Date).Distinct().ToList();
        }

        #endregion
    }
}
