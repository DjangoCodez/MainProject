using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmployeePayrollAdditionsItem
    {
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string Group { get; set; }
        public string Type { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
