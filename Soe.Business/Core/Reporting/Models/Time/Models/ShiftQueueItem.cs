using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class ShiftQueueItem
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string TypeName { get; set; }
        public DateTime Created { get; set; }
        public string Creator { get; set; }
        public string Modifier { get; set; }
        public DateTime? Date { get; set; }
        public decimal QueueTimeBeforeShiftStartInHours { get; set; }
        public decimal QueueTimeBeforeQueueWasHandledInHours { get; set; }
        public decimal QueueTimeSinceShiftCreatedInHours { get; set; }
        public decimal QueueTimeHandledBeforeShiftStartInHours { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public string CurrentEmployee { get; set; }
        public bool CurrentEmployeeIsHidden { get; set; }
        public DateTime? DateHandled { get; set; }
    }
}
