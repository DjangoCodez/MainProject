using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class SwapShiftItem
    {
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public string HasShift { get; set; }
        public string ShiftType { get; set; }
        public string AcceptorEmployeeNr { get; set; }
        public string AcceptorEmployeeName { get; set; }
        public DateTime? AcceptedDate { get; set; }
        public string SwappedToEmployeeNr { get; set; }
        public string SwappedToEmployeeName { get; set; }
        public string InitiatorEmployeeNr { get; set; }
        public string InitiatorEmployeeName { get; set; }
        public DateTime? InitiatedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string ApprovedBy { get; set; }
        public int ShiftLengthInMinutes { get; set; }

    }
}
