using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class ScheduledTimeSummaryItem
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public int Time { get; set; }
    }
}
