using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    #region Input

    public class SaveAttestForEmployeeInputDTO : TimeEngineInputDTO
    {
        public List<SaveAttestEmployeeDayDTO> InputItems { get; set; }
        public int EmployeeId { get; set; }
        public int AttestStateId { get; set; }
        public bool IsMySelf { get; set; }
        public bool IsPayrollAttest { get; set; }
        public bool ForceWholeDay { get; set; }
        public SaveAttestForEmployeeInputDTO(List<SaveAttestEmployeeDayDTO> inputItems, int employeeId, int attestStateId, bool isMySelf, bool isPayrollAttest, bool forceWholeDay)
        {
            this.InputItems = inputItems;
            this.EmployeeId = employeeId;
            this.AttestStateId = attestStateId;
            this.IsMySelf = isMySelf;
            this.IsPayrollAttest = isPayrollAttest;
            this.ForceWholeDay = forceWholeDay;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return InputItems?.Select(i => i.TimeBlockDateId).Distinct().Count();
        }
    }
    public class SaveAttestForEmployeesInputDTO : TimeEngineInputDTO
    {
        public int CurrentEmployeeId { get; set; }
        public List<int> EmployeeIds { get; set; }
        public int AttestStateId { get; set; }
        public int? TimePeriodId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? StopDate { get; set; }
        public bool IsPayrollAttest { get; set; }
        public SaveAttestForEmployeesInputDTO(int currentEmployeeId, List<int> employeeIds, int attestStateId, int timePeriodId, bool isPayrollAttest)
        {
            this.CurrentEmployeeId = currentEmployeeId;
            this.EmployeeIds = employeeIds;
            this.AttestStateId = attestStateId;
            this.TimePeriodId = timePeriodId;
            this.StartDate = null;
            this.StopDate = null;
            this.IsPayrollAttest = isPayrollAttest;
        }
        public SaveAttestForEmployeesInputDTO(int currentEmployeeId, List<int> employeeIds, int attestStateId, DateTime startDate, DateTime stopDate, bool isPayrollAttest)
        {
            this.CurrentEmployeeId = currentEmployeeId;
            this.EmployeeIds = employeeIds;
            this.AttestStateId = attestStateId;
            this.TimePeriodId = null;
            this.StartDate = startDate;
            this.StopDate = stopDate;
            this.IsPayrollAttest = isPayrollAttest;
        }
        public override int? GetIdCount()
        {
            return EmployeeIds.Count;
        }
        public override int? GetIntervalCount()
        {
            return CalendarUtility.GetTotalDays(this.StartDate, this.StopDate);
        }
    }
    public class SaveAttestForTransactionsInputDTO : TimeEngineInputDTO
    {
        public List<SaveAttestTransactionDTO> InputItems { get; set; }
        public int AttestStateId { get; set; }
        public bool IsMySelf { get; set; }
        public SaveAttestForTransactionsInputDTO(List<SaveAttestTransactionDTO> inputItems, int attestStateId, bool isMySelf)
        {
            this.InputItems = inputItems;
            this.AttestStateId = attestStateId;
            this.IsMySelf = isMySelf;
        }
        public override int? GetIdCount()
        {
            return InputItems?.Select(i => i.EmployeeId).Distinct().Count();
        }
        public override int? GetIntervalCount()
        {
            return InputItems?.Select(i => i.Date).Distinct().Count();
        }
    }
    public class SaveAttestForExternalTransactionsInputDTO : TimeEngineInputDTO
    {
        public List<int> TimeCodeTransactionIds { get; set; }
        public int AttestStateId { get; set; }
        public int EmployeeId { get; set; }
        public bool IsMySelf { get; set; }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return TimeCodeTransactionIds?.Count();
        }
    }
    public class SaveAttestForAccountProvisionInputDTO : TimeEngineInputDTO
    {
        public List<AccountProvisionTransactionGridDTO> InputTransactions { get; set; }
        public SaveAttestForAccountProvisionInputDTO(List<AccountProvisionTransactionGridDTO> inputTransactions) : base()
        {
            this.InputTransactions = inputTransactions;
        }
    }
    public class RunAutoAttestInputDTO : TimeEngineInputDTO
    {
        public List<int> EmployeeIds { get; set; }
        public List<int> ScheduleJobHeadIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public bool AutoAttestJob { get; set; }

        public RunAutoAttestInputDTO(List<int> employeeIds, List<int> scheduleJobHeadIds, DateTime startDate, DateTime stopDate, bool autoAttestJob = false)
        {
            this.EmployeeIds = employeeIds;
            this.ScheduleJobHeadIds = scheduleJobHeadIds;
            this.StartDate = startDate;
            this.StopDate = stopDate;
            this.AutoAttestJob = autoAttestJob;

            if (this.StartDate > this.StopDate)
                this.StartDate = this.StopDate;
        }
        public override int? GetIdCount()
        {
            return EmployeeIds.Count;
        }
        public override int? GetIntervalCount()
        {
            return CalendarUtility.GetTotalDays(this.StartDate, this.StopDate);
        }
    }
    public class SendAttestReminderInputDTO : TimeEngineInputDTO
    {
        public List<int> EmployeeIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public bool DoSendToExecutive { get; set; }
        public bool DoSendToEmployee { get; set; }
        public SendAttestReminderInputDTO(List<int> employeeIds, DateTime startDate, DateTime stopDate, bool doSendToExecutive, bool doSendToEmployee)
        {
            this.EmployeeIds = employeeIds;
            this.StartDate = startDate;
            this.StopDate = stopDate;
            this.DoSendToExecutive = doSendToExecutive;
            this.DoSendToEmployee = doSendToEmployee;
        }
        public override int? GetIdCount()
        {
            return EmployeeIds.Count;
        }
        public override int? GetIntervalCount()
        {
            return CalendarUtility.GetTotalDays(this.StartDate, this.StopDate);
        }
    }

    #endregion

    #region Output

    public class SaveAttestForEmployeeOutputDTO : TimeEngineOutputDTO { }
    public class SaveAttestForEmployeesOutputDTO : TimeEngineOutputDTO { }
    public class SaveAttestForTransactionsOutputDTO : TimeEngineOutputDTO { }
    public class SaveAttestForAccountProvisionOutputDTO : TimeEngineOutputDTO { }
    public class RunAutoAttestOutputDTO : TimeEngineOutputDTO { }
    public class SendAttestReminderOutputDTO : TimeEngineOutputDTO { }

    #endregion
}
