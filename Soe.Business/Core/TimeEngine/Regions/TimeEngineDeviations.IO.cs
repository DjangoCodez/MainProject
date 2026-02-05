using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoftOne.Soe.Business.Core.TimeEngine
{
    #region Input

    public class GenerateDeviationsFromTimeIntervalInputDTO : TimeEngineInputDTO
    {
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public DateTime? DisplayedDate { get; set; }//The date that is currently displayed in TimeDeviationEdit
        public String DeviationComment { get; set; }
        public int TimeScheduleTemplatePeriodId { get; set; }
        public int TimeDeviationCauseStartId { get; set; }
        public int TimeDeviationCauseStopId { get; set; }
        public int? EmployeeChildId { get; set; }
        public int? TimeScheduleTypeId { get; set; }
        public int? ShiftTypeId { get; set; }
        public int EmployeeId { get; set; }
        public TermGroup_TimeDeviationCauseType ChoosenDeviationCauseType { get; set; }//Depends on if user choose "Add presence" or "Add absence"       
        public GenerateDeviationsFromTimeIntervalInputDTO(DateTime start, DateTime stop, DateTime? displayedDate, String comment, int timescheduleTemplatePeriodId, int timeDeviationCauseStartId, int timeDeviationCauseStopId, int? employeeChildId, int employeeId, TermGroup_TimeDeviationCauseType devCauseType)
        {
            this.Start = start;
            this.Stop = stop;
            this.DeviationComment = comment;
            this.DisplayedDate = displayedDate;
            this.TimeScheduleTemplatePeriodId = timescheduleTemplatePeriodId;
            this.TimeDeviationCauseStartId = timeDeviationCauseStartId;
            this.TimeDeviationCauseStopId = timeDeviationCauseStopId;
            this.EmployeeChildId = employeeChildId;
            this.EmployeeId = employeeId;
            this.ChoosenDeviationCauseType = devCauseType;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return CalendarUtility.GetTotalDays(Start, Stop);
        }
    }
    public class SaveGeneratedDeviationsInputDTO : TimeEngineInputDTO
    {
        public List<AttestEmployeeDayTimeBlockDTO> TimeBlocks { get; set; }
        public List<AttestEmployeeDayTimeCodeTransactionDTO> TimeCodeTransactions { get; set; }
        public List<AttestPayrollTransactionDTO> TimePayrollTransactions { get; set; }
        public List<ApplyAbsenceDTO> ApplyAbsenceItems { get; set; }
        public int TimeBlockDateId { get; set; }
        public int TimeScheduleTemplatePeriodId { get; set; }
        public int EmployeeId { get; set; }
        public List<int> PayrollImportEmployeeTransactionIds { get; set; }
        public SaveGeneratedDeviationsInputDTO(List<AttestEmployeeDayTimeBlockDTO> inputTimeBlocks, List<AttestEmployeeDayTimeCodeTransactionDTO> timeCodeTransactions, List<AttestPayrollTransactionDTO> inputTimePayrollTransactions, List<ApplyAbsenceDTO> inputApplyAbsenceItems, int timeBlockDateId, int timeScheduleTemplatePeriodId, int employeeId, List<int> payrollImportEmployeeTransactionIds)
        {
            this.TimeBlocks = inputTimeBlocks;
            this.TimeCodeTransactions = timeCodeTransactions;
            this.TimePayrollTransactions = inputTimePayrollTransactions;
            this.ApplyAbsenceItems = inputApplyAbsenceItems;
            this.TimeBlockDateId = timeBlockDateId;
            this.TimeScheduleTemplatePeriodId = timeScheduleTemplatePeriodId;
            this.EmployeeId = employeeId;
            this.PayrollImportEmployeeTransactionIds = payrollImportEmployeeTransactionIds;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return TimeBlocks?.Select(i => i.TimeBlockDateId).Distinct().Count();
        }
    }
    public class SaveWholedayDeviationsInputDTO : TimeEngineInputDTO
    {
        public List<TimeBlockDTO> InputTimeBlocks { get; set; }
        public String DeviationComment { get; set; }
        public int TimeDeviationCauseStartId { get; set; }
        public int TimeDeviationCauseStopId { get; set; }
        public int EmployeeId { get; set; }
        public int? EmployeeChildId { get; set; }
        public SaveWholedayDeviationsInputDTO(List<TimeBlockDTO> inputTimeBlocks, String comment, int timeDeviationCauseStartId, int timeDeviationCauseStopId, int? employeeChildId, int employeeId)
        {
            this.InputTimeBlocks = inputTimeBlocks;
            this.DeviationComment = comment;
            this.TimeDeviationCauseStartId = timeDeviationCauseStartId;
            this.TimeDeviationCauseStopId = timeDeviationCauseStopId;
            this.EmployeeId = employeeId;
            this.EmployeeChildId = employeeChildId;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return InputTimeBlocks?.Select(i => i.TimeBlockDateId).Distinct().Count();
        }
    }
    public class ValidateDeviationChangeInputDTO : TimeEngineInputDTO
    {
        public int EmployeeId { get; set; }
        public int TimeBlockId { get; set; }
        public int TimeScheduleTemplatePeriodId { get; set; }
        public int? TimeScheduleTypeId { get; set; }
        public int? TimeDeviationCauseId { get; set; }
        public bool HasTimeDeviationCauseId
        {
            get
            {
                return this.TimeDeviationCauseId.HasValue && this.TimeDeviationCauseId.Value > 0;
            }
        }
        public int? ShiftTypeId { get; set; }
        public int? EmployeeChildId { get; set; }
        public string TimeBlockGuidId { get; set; }
        public SoeTimeBlockClientChange ClientChange { get; set; }
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public List<AttestEmployeeDayTimeBlockDTO> TimeBlocks { get; set; }
        public bool OnlyUseInTimeTerminal { get; set; }
        public string Comment { get; set; }
        public AccountingSettingsRowDTO AccountSetting { get; set; }

        public ValidateDeviationChangeInputDTO(int employeeId, int timeBlockId, int timeScheduleTemplatePeriodId, int? timeDeviationCauseId, int? employeeChildId, string timeBlockGuidId, SoeTimeBlockClientChange clientChange, DateTime date, DateTime startTime, DateTime stopTime, List<AttestEmployeeDayTimeBlockDTO> timeBlocks, bool onlyUseInTimeTerminal, string comment, AccountingSettingsRowDTO accountSetting)
        {
            this.EmployeeId = employeeId;
            this.TimeBlockId = timeBlockId;
            this.TimeBlockGuidId = timeBlockGuidId;
            this.TimeScheduleTemplatePeriodId = timeScheduleTemplatePeriodId;
            this.TimeDeviationCauseId = timeDeviationCauseId;
            this.EmployeeChildId = employeeChildId;
            this.ClientChange = clientChange;
            this.Date = date;
            this.StartTime = startTime;
            this.StopTime = stopTime;
            this.TimeBlocks = timeBlocks;
            this.OnlyUseInTimeTerminal = onlyUseInTimeTerminal;
            this.Comment = comment;
            this.AccountSetting = accountSetting;
        }
        public override int? GetIdCount()
        {
            return 1;
        }
        public override int? GetIntervalCount()
        {
            return TimeBlocks?.Select(i => i.TimeBlockDateId).Distinct().Count();
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"ClientChange={ClientChange};");
            sb.Append($"TimeBlockId={TimeBlockId};");
            sb.Append($"TimeBlockGuidId={TimeBlockGuidId};");
            sb.Append($"{StartTime.ToShortTimeString()}-{StopTime.ToShortTimeString()};");
            sb.Append($"TimeDeviationCauseId={TimeDeviationCauseId};");
            sb.Append("[");
            foreach (var timeBlock in TimeBlocks)
            {
                sb.Append($"TimeBlockId={timeBlock.TimeBlockId},");
                sb.Append($"{timeBlock.StartTime.ToShortTimeString()}-{timeBlock.StopTime.ToShortTimeString()},");
                sb.Append($"IsBreak{timeBlock.IsBreak},");
                sb.Append(";");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }    
    public class RecalculateUnhandledShiftChangesInputDTO : TimeEngineInputDTO
    {
        public List<TimeUnhandledShiftChangesEmployeeDTO> UnhandledShiftChanges { get; set; }
        public bool DoRecalculateShifts { get; set; }
        public bool DoRecalculateExtraShifts { get; set; }

        public RecalculateUnhandledShiftChangesInputDTO(List<TimeUnhandledShiftChangesEmployeeDTO> unhandledShiftChanges, bool doRecalculateShifts, bool doRecalculateExtraShifts)
        {
            this.UnhandledShiftChanges = unhandledShiftChanges;
            this.DoRecalculateShifts = doRecalculateShifts;
            this.DoRecalculateExtraShifts = doRecalculateExtraShifts;
        }
    }
    public class GetDayOfAbsenceNumberInputDTO : TimeEngineInputDTO
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public TermGroup_SysPayrollType SysPayrollTypeLevel3 { get; set; }
        public int MaxDays { get; set; }
        public int Interval { get; set; }
        public GetDayOfAbsenceNumberInputDTO(int employeeId, DateTime date, TermGroup_SysPayrollType sysPayrollTypeLevel3, int maxDays, int interval)
        {
            this.EmployeeId = employeeId;
            this.Date = date;
            this.SysPayrollTypeLevel3 = sysPayrollTypeLevel3;
            this.MaxDays = maxDays;
            this.Interval = interval;
        }
    }
    public class CreateAbsenceDetailsInputDTO : TimeEngineInputDTO
    {
        public int BatchInterval { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public int? EmployeeId { get; set; }
        public CreateAbsenceDetailsInputDTO(int batchInterval, DateTime startDate, DateTime stopDate, int? employeeId)
        {
            this.BatchInterval = batchInterval;
            this.StartDate = startDate;
            this.StopDate = stopDate;
            this.EmployeeId = employeeId;
        }
    }
    public class SaveAbsenceDetailsRatioInputDTO : TimeEngineInputDTO
    {
        public int EmployeeId { get; set; }
        public List<TimeAbsenceDetailDTO> TimeAbsenceDetails { get; set; }
        public SaveAbsenceDetailsRatioInputDTO(int employeeId, List<TimeAbsenceDetailDTO> timeAbsenceDetails)
        {
            this.EmployeeId = employeeId;
            this.TimeAbsenceDetails = timeAbsenceDetails;
        }
    }
    public class GetDeviationsAfterEmploymentInputDTO : TimeEngineInputDTO
    {
        public List<int> EmployeeIds { get; set; }
        public GetDeviationsAfterEmploymentInputDTO(List<int> employeeIds)
        {
            this.EmployeeIds = employeeIds;
        }
    }
    public class DeleteDeviationsDaysAfterEmploymentInputDTO : TimeEngineInputDTO
    {
        public List<EmployeeDeviationAfterEmploymentDTO> Deviations { get; set; }
        public DeleteDeviationsDaysAfterEmploymentInputDTO(List<EmployeeDeviationAfterEmploymentDTO> deviations)
        {
            this.Deviations = deviations;
        }
    }

    #endregion

    #region Output

    public class GenerateDeviationsFromTimeIntervalOutputDTO : TimeEngineOutputDTO { }
    public class SaveGeneratedDeviationsOutputDTO : TimeEngineOutputDTO { }
    public class SaveWholedayDeviationsOutputDTO : TimeEngineOutputDTO { }
    public class ValidateDeviationChangeOutputDTO : TimeEngineOutputDTO
    {
        public ValidateDeviationChangeResult ValidationResult { get; set; }
        public ValidateDeviationChangeOutputDTO() {}
        public ValidateDeviationChangeOutputDTO(ValidateDeviationChangeResult validationResult) : base() 
        { 
            this.ValidationResult = validationResult; 
        }
    }    
    public class TaskRecalculateUnhandledShiftChangesOutputDTO : TimeEngineOutputDTO { }
    public class GetDayOfAbsenceNumberOutputDTO : TimeEngineOutputDTO { }
    public class CreateAbsenceDetailsOutputDTO : TimeEngineOutputDTO
    {
        public List<CreateAbsenceDetailResultDTO> AbsenceResults { get; set; }
        public CreateAbsenceDetailsOutputDTO() : base()
        {
            this.AbsenceResults = new List<CreateAbsenceDetailResultDTO>();
        }
    }
    public class SaveAbsenceDetailsRatioOutputDTO : TimeEngineOutputDTO { }
    public class GetDeviationsAfterEmploymentOutputDTO : TimeEngineOutputDTO 
    {
        public List<EmployeeDeviationAfterEmploymentDTO> Deviations { get; set; }
        public GetDeviationsAfterEmploymentOutputDTO()
        {
            this.Deviations = new List<EmployeeDeviationAfterEmploymentDTO>();
        }
    }
    public class DeleteDeviationsDaysAfterEmploymentOutputDTO : TimeEngineOutputDTO { }

    #endregion
}
