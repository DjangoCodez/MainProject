using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class ShiftRequestItem
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public DateTime RequestCreated { get; set; }
        public string RequestCreatedBy { get; set; }
        public string Sender { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? ReadDate { get; set; }
        public string Answer { get; set; }
        public DateTime? AnswerDate { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public DateTime? ShiftDate { get; set; }
        public DateTime ShiftStartTime { get; set; }
        public DateTime ShiftStopTime { get; set; }
        public int? ShiftTypeId { get; set; }
        public string ShiftTypeName { get; set; }
        public DateTime? ShiftCreated { get; set; }
        public string ShiftCreatedBy { get; set; }
        public DateTime? ShiftModified { get; set; }
        public string ShiftModifiedBy { get; set; }
        public bool ShiftDeleted { get; set; }
        public string ShiftAccountNr { get; set; }
        public string ShiftAccountName { get; set; }
    }
}
