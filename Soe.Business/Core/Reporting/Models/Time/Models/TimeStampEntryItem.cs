using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class TimeStampEntryItem
    {
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public DateTime Time { get; set; }
        public string AccountName { get; set; }
        public DateTime Date { get; set; }
        public bool IsBreak { get; set; }
        public bool IsPaidBreak { get; set; }
        public bool IsDistanceWork { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int ShiftTypeId { get; set; }
        public string TimeDeviationCauseName { get; set; }
        public string TimeScheduleTypeName { get; set; }
        public string TimeTerminalName { get; set; }
        public string TypeName { get; set; }
        public string Note { get; set; }
        public int OriginType { get; set; }
        public DateTime OriginalTime { get; set; }
        public int Status { get; set; }
        public int State { get; set; }

    }
}
