using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class AbsenceRequestItem
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public DateTime Created { get; set; }
        public string Creator { get; set; }
        public string Modifier { get; set; }
    }
}
