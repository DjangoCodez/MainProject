using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class ShiftHistoryItem
    {
        public int EmployeeId { get; set; }
        public int TimeScheduleTemplateBlockId { get; set; }
        public string TypeName { get; set; }
        public string FromShiftStatus { get; set; }
        public string ToShiftStatus { get; set; }
        public bool ShiftStatusChanged { get; set; }
        public string FromShiftUserStatus { get; set; }
        public string ToShiftUserStatus { get; set; }
        public bool ShiftUserStatusChanged { get; set; }
        public string FromEmployeeName { get; set; }
        public string ToEmployeeName { get; set; }
        public string FromEmployeeNr { get; set; }
        public string ToEmployeeNr { get; set; }
        public bool EmployeeChanged { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public bool TimeChanged { get; set; }
        public string FromDateAndTime { get; set; }
        public string ToDateAndTime { get; set; }
        public bool DateAndTimeChanged { get; set; }
        public string FromShiftType { get; set; }
        public string ToShiftType { get; set; }
        public bool ShiftTypeChanged { get; set; }
        public string FromTimeDeviationCause { get; set; }
        public string ToTimeDeviationCause { get; set; }
        public bool TimeDeviationCauseChanged { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public string AbsenceRequestApprovedText { get; set; }
        public string FromStart { get; set; }
        public string FromStop { get; set; }
        public string ToStart { get; set; }
        public string ToStop { get; set; }
        public string OriginEmployeeNr { get; set; }
        public string OriginEmployeeName { get; set; }
        public int? FromEmployeeId { get; set; }
        public int? ToEmployeeId { get; set; }
        public string FromExtraShift { get; set; }
        public string ToExtraShift { get; set; }
        public bool ExtraShiftChanged { get; set; }
    }
}
