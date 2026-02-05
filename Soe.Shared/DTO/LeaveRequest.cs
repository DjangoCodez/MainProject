using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Common.Util.Logger;

namespace SoftOne.Soe.Shared.DTO
{
    /// <summary>
    /// Represents an employee's leave request, including details about the employee, leave period, status, and affected periods or shifts.
    /// </summary>
    [Log]
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    public class LeaveRequest
    {
        /// <summary>
        /// Id of the leave request
        /// </summary>
        public int EmployeeRequestId { get; set; }

        /// <summary>
        /// Id of the employee
        /// </summary>
        [LogEmployeeId]
        public int EmployeeId { get; set; }

        /// <summary>
        /// Name of the employee
        /// </summary>
        public string EmployeeName { get; set; }

        /// <summary>
        /// Employee's number
        /// </summary>
        public string EmployeeNumber { get; set; }

        /// <summary>
        /// External code for the employee
        /// </summary>
        public string EmployeeExternalCode { get; set; }

        /// <summary>
        /// Identifier for the deviation cause
        /// </summary>
        public int? TimeDeviationCauseId { get; set; }

        /// <summary>
        /// Name of the deviation cause.
        /// </summary>
        public string TimeDeviationCauseName { get; set; }

        /// <summary>
        /// External code for the deviation cause
        /// </summary>
        public string TimeDeviationCauseExternalCode { get; set; }

        /// <summary>
        /// Name of the employee's child, if applicable
        /// </summary>
        public string EmployeeChildName { get; set; }

        /// <summary>
        /// Start date and time of the leave
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// End date and time of the leave
        /// </summary>
        public DateTime Stop { get; set; }

        /// <summary>
        /// Comment associated with the leave request
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Date and time when the leave request was created
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// User who created the leave request
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Date and time when the leave request was last modified
        /// </summary>
        public DateTime? Modified { get; set; }

        /// <summary>
        /// User who last modified the leave request
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Status of the leave request
        /// </summary>
        public TermGroup_EmployeeRequestStatus Status { get; set; }

        /// <summary>
        /// Name of the leave request status
        /// </summary>
        public string StatusName { get { return this.Status.ToString(); } }

        /// <summary>
        /// Entity state of the leave request
        /// </summary>
        public SoeEntityState State { get; set; }

        /// <summary>
        /// Name of the entity state
        /// </summary>
        public string StateName { get { return this.State.ToString(); } }

        /// <summary>
        /// Result status of the leave request
        /// </summary>
        public TermGroup_EmployeeRequestResultStatus ResultStatus { get; set; }

        /// <summary>
        /// Name of the result status
        /// </summary>
        public string ResultStatusName { get { return this.ResultStatus.ToString(); } }

        /// <summary>
        /// Extended absence settings for the leave request
        /// </summary>
        public LeaveRequestExtendedAbsenceSettings ExtendedAbsenceSettings { get; set; }

        /// <summary>
        /// List of affected shifts for the leave request
        /// </summary>
        public List<LeaveRequestAffectedShift> AffectedShifts { get; set; }

        /// <summary>
        /// List of affected periods for the leave request
        /// </summary>
        public List<LeaveRequestAffectedPeriod> AffectedPeriods { get; set; }
    }

    /// <summary>
    /// Extended absence settings for a leave request, including day-specific and percental absence options
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    public class LeaveRequestExtendedAbsenceSettings
    {
        /// <summary>
        /// Indicates whether the first and last day of the absence have separate settings
        /// </summary>
        public bool AbsenceFirstAndLastDay { get; set; }

        /// <summary>
        /// Indicates whether the whole first day is absent
        /// </summary>
        public bool? AbsenceWholeFirstDay { get; set; }

        /// <summary>
        /// Start time for the first day of absence
        /// </summary>
        public string AbsenceFirstDayStart { get; set; }

        /// <summary>
        /// Indicates whether the whole last day is absent
        /// </summary>
        public bool? AbsenceWholeLastDay { get; set; }

        /// <summary>
        /// Stop time for the last day of absence
        /// </summary>
        public string AbsenceLastDayStop { get; set; }

        /// <summary>
        /// Indicates whether the absence is percental
        /// </summary>
        public bool PercentalAbsence { get; set; }

        /// <summary>
        /// Percental value of the absence
        /// </summary>
        public decimal? PercentalValue { get; set; }

        /// <summary>
        /// Indicates whether percental absence occurs at the start of the day
        /// </summary>
        public bool? PercentalAbsenceOccursStartOfDay { get; set; }

        /// <summary>
        /// Indicates whether percental absence occurs at the end of the day
        /// </summary>
        public bool? PercentalAbsenceOccursEndOfDay { get; set; }

        /// <summary>
        /// Indicates whether absence is adjusted per weekday
        /// </summary>
        public bool AdjustAbsencePerWeekDay { get; set; }

        /// <summary>
        /// Start time for absence on all days
        /// </summary>
        public string AdjustAbsenceAllDaysStart { get; set; }

        /// <summary>
        /// Stop time for absence on all days
        /// </summary>
        public string AdjustAbsenceAllDaysStop { get; set; }

        /// <summary>
        /// Start time for absence on Monday
        /// </summary>
        public string AdjustAbsenceMonStart { get; set; }

        /// <summary>
        /// Stop time for absence on Monday
        /// </summary>
        public string AdjustAbsenceMonStop { get; set; }

        /// <summary>
        /// Start time for absence on Tuesday
        /// </summary>
        public string AdjustAbsenceTueStart { get; set; }

        /// <summary>
        /// Stop time for absence on Tuesday
        /// </summary>
        public string AdjustAbsenceTueStop { get; set; }

        /// <summary>
        /// Start time for absence on Wednesday
        /// </summary>
        public string AdjustAbsenceWedStart { get; set; }

        /// <summary>
        /// Stop time for absence on Wednesday
        /// </summary>
        public string AdjustAbsenceWedStop { get; set; }

        /// <summary>
        /// Start time for absence on Thursday
        /// </summary>
        public string AdjustAbsenceThuStart { get; set; }

        /// <summary>
        /// Stop time for absence on Thursday
        /// </summary>
        public string AdjustAbsenceThuStop { get; set; }

        /// <summary>
        /// Start time for absence on Friday
        /// </summary>
        public string AdjustAbsenceFriStart { get; set; }

        /// <summary>
        /// Stop time for absence on Friday
        /// </summary>
        public string AdjustAbsenceFriStop { get; set; }

        /// <summary>
        /// Start time for absence on Saturday
        /// </summary>
        public string AdjustAbsenceSatStart { get; set; }

        /// <summary>
        /// Stop time for absence on Saturday
        /// </summary>
        public string AdjustAbsenceSatStop { get; set; }

        /// <summary>
        /// Start time for absence on Sunday
        /// </summary>
        public string AdjustAbsenceSunStart { get; set; }

        /// <summary>
        /// Stop time for absence on Sunday
        /// </summary>
        public string AdjustAbsenceSunStop { get; set; }
    }

    /// <summary>
    /// Represents a shift affected by a leave request, including reference Id, date, shift times, and absence times
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    public class LeaveRequestAffectedShift
    {
        /// <summary>
        /// Reference Id for the affected shift
        /// </summary>
        public string PeriodReferenceId { get; set; }

        /// <summary>
        /// Actual date of the affected shift
        /// </summary>
        public string ActualDate { get; set; }

        /// <summary>
        /// Start time of the shift
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Stop time of the shift
        /// </summary>
        public DateTime StopTime { get; set; }

        /// <summary>
        /// Start time of the absence within the shift
        /// </summary>
        public DateTime AbsenceStartTime { get; set; }

        /// <summary>
        /// Stop time of the absence within the shift
        /// </summary>
        public DateTime AbsenceStopTime { get; set; }
    }

    /// <summary>
    /// Represents a period affected by a leave request, including reference Id, date, and start/stop times
    /// </summary>
    [JsonObject(NamingStrategyType = typeof(CamelCaseNamingStrategy), ItemNullValueHandling = NullValueHandling.Ignore)]
    public class LeaveRequestAffectedPeriod
    {
        /// <summary>
        /// Reference Id for the affected period
        /// </summary>
        public string PeriodReferenceId { get; set; }

        /// <summary>
        /// Actual date of the affected period
        /// </summary>
        public string ActualDate { get; set; }

        /// <summary>
        /// Start time of the affected period
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Stop time of the affected period
        /// </summary>
        public DateTime StopTime { get; set; }
    }

}
