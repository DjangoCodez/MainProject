using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    #region Input

    public abstract class DayFunctionBase : TimeEngineInputDTO
    {
        public List<AttestEmployeeDaySmallDTO> Items { get; set; }
        public List<int> EmployeeIds
        {
            get
            {
                return this.Items.GetEmployeeIds();
            }
        }
        public string DateInterval
        {
            get
            {
                return this.Items.GetDateInterval();
            }
        }
        protected DayFunctionBase(List<AttestEmployeeDaySmallDTO> items)
        {
            this.Items = items;
        }
        public override int? GetIdCount()
        {
            return this.Items?.Select(i => i.EmployeeId).Distinct().Count();
        }
        public override int? GetIntervalCount()
        {
            return this.Items?.Select(i => i.Date).Distinct().Count();
        }
    }
    public class RestoreDaysToScheduleInputDTO : DayFunctionBase
    {
        public RestoreDaysToScheduleInputDTO(List<AttestEmployeeDaySmallDTO> items) : base(items) { }
    }
    public class RestoreDaysToScheduleDiscardDeviationsInputDTO : DayFunctionBase
    {
        public RestoreDaysToScheduleDiscardDeviationsInputDTO(List<AttestEmployeeDaySmallDTO> items) : base(items) { }
    }
    public class RestoreDaysToTemplateScheduleInputDTO : DayFunctionBase
    {
        public RestoreDaysToTemplateScheduleInputDTO(List<AttestEmployeeDaySmallDTO> items) : base(items) { }
    }
    public class ReGenerateTransactionsDiscardAttestInputDTO : TimeEngineInputDTO
    {
        public List<AttestEmployeeDaySmallDTO> Items { get; set; }
        public bool DoNotRecalculateAmounts { get; set; }
        public bool VacationOnly { get; set; }
        public bool VacationResetLeaveOfAbsence { get; set; }
        public bool VacationReset30000 { get; set; }
        public DateTime? LimitStartDate { get; set; }
        public DateTime? LimitStopDate { get; set; }
        public List<int> EmployeeIds { get { return this.Items.GetEmployeeIds(); } }
        public string DateInterval { get { return this.Items.GetDateInterval(); } }
        public ReGenerateTransactionsDiscardAttestInputDTO(List<AttestEmployeeDaySmallDTO> items, bool doNotRecalculateAmounts, bool vacationOnly, bool vacationResetLeaveOfAbsence, bool vacationReset30000, DateTime? limitStartDate, DateTime? limitStopDate)
        {
            this.Items = items;
            this.DoNotRecalculateAmounts = doNotRecalculateAmounts;
            this.VacationOnly = vacationOnly;
            this.VacationResetLeaveOfAbsence = vacationResetLeaveOfAbsence;
            this.VacationReset30000 = vacationReset30000;
            this.LimitStartDate = limitStartDate;
            this.LimitStopDate = limitStopDate;
        }
        public override int? GetIdCount()
        {
            return this.Items.GetNrOfEmployees();
        }
        public override int? GetIntervalCount()
        {
            return this.Items.GetNrOfDates();
        }
    }
    public class CleanDaysInputDTO : DayFunctionBase
    {
        public CleanDaysInputDTO(List<AttestEmployeeDaySmallDTO> items) : base(items) { }
    }
    public class SaveTimeCodeTransactionsInputDTO : TimeEngineInputDTO
    {
        public List<TimeCodeTransactionDTO> TimeCodeTransactionsInput { get; set; }
        public SaveTimeCodeTransactionsInputDTO(List<TimeCodeTransactionDTO> timeCodeTransactionInput)
        {
            this.TimeCodeTransactionsInput = timeCodeTransactionInput;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return TimeCodeTransactionsInput?.Count();
        }
    }
    public class CreateTransactionsForPlannedPeriodCalculationInputDTO : TimeEngineInputDTO
    {
        public int EmployeeId { get; set; }
        public int TimePeriodId { get; set; }
        public CreateTransactionsForPlannedPeriodCalculationInputDTO(int employeeId, int periodId)
        {
            this.EmployeeId = employeeId;
            this.TimePeriodId = periodId;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return 1;
        }
    }

    #endregion

    #region Output

    public class RestoreDaysToScheduleOutputDTO : TimeEngineOutputDTO { }
    public class RestoreDaysToScheduleDiscardDeviationsOutputDTO : TimeEngineOutputDTO { }
    public class RestoreDaysToTemplateScheduleOutputDTO : TimeEngineOutputDTO
    {
        public List<int> AutogenTimeBlockDateIds { get; set; }
        public List<int> StampingTimeBlockDateIds { get; set; }

        public RestoreDaysToTemplateScheduleOutputDTO()
        {
            this.AutogenTimeBlockDateIds = new List<int>();
            this.StampingTimeBlockDateIds = new List<int>();
        }
    }
    public class ReGenerateTransactionsDiscardAttestOutputDTO : TimeEngineOutputDTO
    {
        public List<DateTime> StampingDates { get; set; }
        public List<string> Logs { get; set; }
        public ReGenerateTransactionsDiscardAttestOutputDTO()
        {
            this.Logs = new List<string>();
        }
    }
    public class CleanDaysOutputDTO : TimeEngineOutputDTO { }
    public class SaveTimeCodeTransactionsOutputDTO : TimeEngineOutputDTO
    {
        public List<TimeCodeTransactionDTO> TimeCodeTransactionDTOs { get; set; }
    }
    public class CreateTransactionsForPlannedPeriodCalculationOutputDTO : TimeEngineOutputDTO
    {

    }

    #endregion
}
