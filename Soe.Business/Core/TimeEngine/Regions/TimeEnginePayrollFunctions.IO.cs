using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    public partial class TimeEngine : ManagerBase
    {
        #region Input

        public class LockUnlockPayrollPeriodInputDTO : TimeEngineInputDTO
        {
            public List<int> EmployeeIds { get; set; }
            public List<int> TimePeriodIds { get; set; }
            public int RoleId { get; set; }
            public bool IgnoreResultingAttestStateId { get; set; }
            public LockUnlockPayrollPeriodInputDTO(List<int> employeeIds, int timePeriodId, int roleId, bool ignoreResultingAttestStateId)
            {
                this.EmployeeIds = employeeIds;
                this.TimePeriodIds = timePeriodId.ObjToList();
                this.RoleId = roleId;
                this.IgnoreResultingAttestStateId = ignoreResultingAttestStateId;
            }
            public LockUnlockPayrollPeriodInputDTO(int employeeId, int timePeriodId, int roleId, bool ignoreResultingAttestStateId)
            {
                this.EmployeeIds = employeeId.ObjToList();
                this.TimePeriodIds = timePeriodId.ObjToList();
                this.RoleId = roleId;
                this.IgnoreResultingAttestStateId = ignoreResultingAttestStateId;
            }
            public LockUnlockPayrollPeriodInputDTO(List<int> employeeIds, List<int> timePeriodIds, int roleId, bool ignoreResultingAttestStateId)
            {
                this.EmployeeIds = employeeIds;
                this.TimePeriodIds = timePeriodIds;
                this.RoleId = roleId;
                this.IgnoreResultingAttestStateId = ignoreResultingAttestStateId;
            }
            public override int? GetIdCount()
            {
                return this.EmployeeIds?.Count;
            }
            public override int? GetIntervalCount()
            {
                return this.TimePeriodIds?.Count;
            }
        }
        public class RecalculatePayrollPeriodInputDTO : TimeEngineInputDTO
        {
            public List<int> EmployeeIds { get; set; }
            public List<int> TimePeriodIds { get; set; }
            public bool IncludeScheduleTransactions { get; set; }
            public bool IgnoreEmploymentHasEnded { get; set; }
            public Guid Key { get; set; }
            public SoeProgressInfo Info { get; set; }
            public SoeMonitor Monitor { get; set; }
            public bool DoLogProgressInfo
            {
                get
                {
                    return Info != null && Monitor != null;
                }
            }
            public RecalculatePayrollPeriodInputDTO(Guid key, int employeeId, int timePeriodId, bool includeScheduleTransactions, bool ignoreEmploymentHasEnded, SoeProgressInfo info = null, SoeMonitor monitor = null)
            {
                this.Key = key;
                this.Info = info;
                this.Monitor = monitor;
                this.EmployeeIds = employeeId.ObjToList();
                this.TimePeriodIds = timePeriodId.ObjToList();
                this.IncludeScheduleTransactions = includeScheduleTransactions;
                this.IgnoreEmploymentHasEnded = ignoreEmploymentHasEnded;
            }
            public RecalculatePayrollPeriodInputDTO(Guid key, List<int> employeeIds, int timePeriodId, bool includeScheduleTransactions, bool ignoreEmploymentHasEnded, SoeProgressInfo info = null, SoeMonitor monitor = null)
            {
                this.Key = key;
                this.Info = info;
                this.Monitor = monitor;
                this.EmployeeIds = employeeIds;
                this.TimePeriodIds = timePeriodId.ObjToList();
                this.IncludeScheduleTransactions = includeScheduleTransactions;
                this.IgnoreEmploymentHasEnded = ignoreEmploymentHasEnded;
            }
            public RecalculatePayrollPeriodInputDTO(Guid key, List<int> employeeIds, List<int> timePeriodIds, bool includeScheduleTransactions, bool ignoreEmploymentHasEnded, SoeProgressInfo info = null, SoeMonitor monitor = null)
            {
                this.Key = key;
                this.Info = info;
                this.Monitor = monitor;
                this.EmployeeIds = employeeIds;
                this.TimePeriodIds = timePeriodIds;
                this.IncludeScheduleTransactions = includeScheduleTransactions;
                this.IgnoreEmploymentHasEnded = ignoreEmploymentHasEnded;
            }
            public override int? GetIdCount()
            {
                return this.EmployeeIds?.Count;
            }
            public override int? GetIntervalCount()
            {
                return this.TimePeriodIds?.Count;
            }
        }
        public class RecalculateExportedEmploymentTaxJOBInputDTO : TimeEngineInputDTO
        {
            public List<int> EmployeeIds { get; set; }
            public List<int> TimePeriodIds { get; set; }
            public RecalculateExportedEmploymentTaxJOBInputDTO(List<int> employeeIds, int timePeriodId)
            {
                this.EmployeeIds = employeeIds;
                this.TimePeriodIds = timePeriodId.ObjToList();
            }
            public RecalculateExportedEmploymentTaxJOBInputDTO(List<int> employeeIds, List<int> timePeriodIds)
            {
                this.EmployeeIds = employeeIds;
                this.TimePeriodIds = timePeriodIds;
            }
            public override int? GetIdCount()
            {
                return this.EmployeeIds?.Count;
            }
            public override int? GetIntervalCount()
            {
                return this.TimePeriodIds?.Count;
            }
        }
        public class SavePayrollTransactionAmountsInputDTO : TimeEngineInputDTO
        {
            public int EmployeeId { get; set; }
            public int TimeBlockDateId { get; set; }
            public DateTime? Date { get; set; }
            public SavePayrollTransactionAmountsInputDTO(int employeeId, int timeBlockDateId)
            {
                this.EmployeeId = employeeId;
                this.TimeBlockDateId = timeBlockDateId;
                this.Date = null;
            }
            public SavePayrollTransactionAmountsInputDTO(int employeeId, DateTime date)
            {
                EmployeeId = employeeId;
                TimeBlockDateId = 0;
                Date = date;
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
        public class GetUnhandledPayrollTransactionsInputDTO : TimeEngineInputDTO
        {
            public int EmployeeId { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime StopDate { get; set; }
            public bool IsBackwards { get; set; }
            public GetUnhandledPayrollTransactionsInputDTO(int employeeId, DateTime startDate, DateTime stopDate, bool isBackwards)
            {
                this.EmployeeId = employeeId;
                this.StartDate = startDate;
                this.StopDate = stopDate;
                this.IsBackwards = isBackwards;
            }
            public override int? GetIdCount()
            {
                return 1;
            }
            public override int? GetIntervalCount()
            {
                return CalendarUtility.GetTotalDays(this.StartDate, this.StopDate);
            }
        }
        public class AssignPayrollTransactionsToTimePeriodInputDTO : TimeEngineInputDTO
        {
            public List<AttestPayrollTransactionDTO> TransactionItems { get; set; }
            public List<AttestPayrollTransactionDTO> ScheduleTransactionItems { get; set; }
            public TimePeriodDTO TimePeriodItem { get; set; }
            public TermGroup_TimePeriodType PeriodType { get; set; }
            public int EmployeeId { get; set; }
            public AssignPayrollTransactionsToTimePeriodInputDTO(List<AttestPayrollTransactionDTO> transactionItems, List<AttestPayrollTransactionDTO> scheduleTransactionItems, TimePeriodDTO timePeriodItem, TermGroup_TimePeriodType periodType, int employeeId)
            {
                this.TransactionItems = transactionItems ?? new List<AttestPayrollTransactionDTO>();                
                this.ScheduleTransactionItems = scheduleTransactionItems ?? new List<AttestPayrollTransactionDTO>();
                this.TimePeriodItem = timePeriodItem;
                this.PeriodType = periodType;
                this.EmployeeId = employeeId;
            }
            public override int? GetIdCount()
            {
                return 1;
            }
            public override int? GetIntervalCount()
            {
                return TransactionItems?.Select(i => i.TimeBlockDateId).Distinct().Count();
            }
        }
        public class ReverseTransactionsValidationInputDTO : TimeEngineInputDTO
        {
            public int EmployeeId { get; set; }
            public List<DateTime> Dates { get; set; }
            public ReverseTransactionsValidationInputDTO(int employeeId, List<DateTime> dates)
            {
                this.Dates = dates;
                this.EmployeeId = employeeId;
            }
            public override int? GetIdCount()
            {
                return 1;
            }
            public override int? GetIntervalCount()
            {
                return Dates?.Count;
            }
        }
        public class ReverseTransactionsAngularInputDTO : TimeEngineInputDTO
        {
            public int EmployeeId { get; set; }
            public int? TimeDeviationCauseId { get; set; }
            public int? TimePeriodId { get; set; }
            public int? EmployeeChildId { get; set; }
            public List<DateTime> Dates { get; set; }
            public ReverseTransactionsAngularInputDTO(int employeeId, List<DateTime> dates, int? timeDeviationCauseId, int? timePeriodId, int? employeeChildId)
            {
                this.Dates = dates;
                this.EmployeeId = employeeId;
                this.EmployeeChildId = employeeChildId;
                this.TimeDeviationCauseId = timeDeviationCauseId;
                this.TimePeriodId = timePeriodId;
            }
            public override int? GetIdCount()
            {
                return 1;
            }
            public override int? GetIntervalCount()
            {
                return Dates?.Count;
            }
        }
        public class ReverseTransactionsInputDTO : TimeEngineInputDTO
        {
            public List<AttestEmployeeDaySmallDTO> Items { get; set; }
            public List<int> EmployeeIds { get { return this.Items.GetEmployeeIds(); } }
            public string DateInterval { get { return this.Items.GetDateInterval(); } }
            public ReverseTransactionsInputDTO() : base() { }
            public override int? GetIdCount()
            {
                return this.Items.GetNrOfEmployees();
            }
            public override int? GetIntervalCount()
            {
                return this.Items.GetNrOfDates();
            }
        }
        public class SaveFixedPayrollRowsInputDTO : TimeEngineInputDTO
        {
            public List<FixedPayrollRowDTO> InputItems { get; set; }
            public int EmployeeId { get; set; }
            public SaveFixedPayrollRowsInputDTO(List<FixedPayrollRowDTO> inputItems, int employeeId)
            {
                this.InputItems = inputItems;
                this.EmployeeId = employeeId;
            }
            public override int? GetIdCount()
            {
                return 1;
            }
        }
        public class SaveAddedTransactionInputDTO : TimeEngineInputDTO
        {
            public AttestPayrollTransactionDTO InputItem { get; set; }
            public List<AccountingSettingDTO> AccountingSettings { get; set; }
            public List<AccountingSettingsRowDTO> AccountingSettingsAngular { get; set; }
            public int EmployeeId { get; set; }
            public int? TimePeriodId { get; set; }
            public bool IgnoreEmploymentHasEnded { get; set; }
            public SaveAddedTransactionInputDTO(AttestPayrollTransactionDTO inputItem, List<AccountingSettingDTO> accountSettings, List<AccountingSettingsRowDTO> accountingSettingsAngular, int employeeId, int? timePeriodId, bool ignoreEmploymentHasEnded)
            {
                this.InputItem = inputItem;
                this.AccountingSettings = accountSettings;
                this.AccountingSettingsAngular = accountingSettingsAngular;
                this.EmployeeId = employeeId;
                this.TimePeriodId = timePeriodId;
                this.IgnoreEmploymentHasEnded = ignoreEmploymentHasEnded;
            }
            public override int? GetIdCount()
            {
                return 1;
            }
        }
        public class CreateAddedTransactionsFromTemplateInputDTO : TimeEngineInputDTO
        {
            public MassRegistrationTemplateHeadDTO Template { get; set; }
            public CreateAddedTransactionsFromTemplateInputDTO(MassRegistrationTemplateHeadDTO template)
            {
                this.Template = template;
            }
        }
        public class SavePayrollScheduleTransactionsInputDTO : TimeEngineInputDTO
        {
            public Dictionary<int, List<DateTime>> EmployeeDates { get; set; }
            public SavePayrollScheduleTransactionsInputDTO(Dictionary<int, List<DateTime>> employeeDates)
            {
                this.EmployeeDates = employeeDates;
            }
        }    
        public class RecalculatePayrollControllInputDTO : TimeEngineInputDTO
        {
            public List<int> EmployeeIds { get; set; }
            public int TimePeriodId { get; set; }
            public RecalculatePayrollControllInputDTO(List<int> employeeIds, int timePeriod)
            {
                this.EmployeeIds = employeeIds;
                this.TimePeriodId = timePeriod;
            }
        }
        public class RecalculatePayrollControllOutputDTO : TimeEngineOutputDTO { }
        
        #endregion

        #region Output

        public class LockUnlockPayrollPeriodOutputDTO : TimeEngineOutputDTO { }
        public class ClearPayrollCalculationOutputDTO : TimeEngineOutputDTO { }
        public class RecalculatePayrollPeriodOutputDTO : TimeEngineOutputDTO { }
        public class RecalculateExportedEmploymentTaxJOBOutputDTO : TimeEngineOutputDTO { }
        public class SavePayrollTransactionAmountsOutputDTO : TimeEngineOutputDTO { }
        public class GetUnhandledPayrollTransactionsOutputDTO : TimeEngineOutputDTO
        {
            public List<AttestPayrollTransactionDTO> TimePayrollTransactionItems { get; set; }
            public GetUnhandledPayrollTransactionsOutputDTO()
            {
                this.TimePayrollTransactionItems = new List<AttestPayrollTransactionDTO>();
            }
        }
        public class AssignPayrollTransactionsToTimePeriodOutputDTO : TimeEngineOutputDTO { }
        public class ReverseTransactionsValidationOutputDTO : TimeEngineOutputDTO
        {
            public ReverseTransactionsValidationOutputDTO() : base() { }
            public ReverseTransactionsValidationOutputDTO(string errorMessage) : base()
            {
                this.Result = new ActionResult(false);
                this.ValidationOutput = new ReverseTransactionsValidationDTO()
                {
                    Success = false,
                    CanContinue = false,
                    Message = !string.IsNullOrEmpty(errorMessage) ? errorMessage : "",
                };
            }
            public ReverseTransactionsValidationDTO ValidationOutput { get; set; }
        }
        public class ReverseTransactionsOutputDTO : TimeEngineOutputDTO { }
        public class SaveFixedPayrollRowsOutputDTO : TimeEngineOutputDTO { }
        public class SaveAddedTransactionOutputDTO : TimeEngineOutputDTO { }
        public class CreateAddedTransactionsFromTemplateOutputDTO : TimeEngineOutputDTO { }
        public class SavePayrollScheduleTransactionsOutputDTO : TimeEngineOutputDTO { }

        #endregion
    }
}
