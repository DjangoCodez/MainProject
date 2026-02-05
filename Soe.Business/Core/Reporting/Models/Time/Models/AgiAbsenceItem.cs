

using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class AgiAbsenceItem
    {
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string SocialSec { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime Date { get; set; }
        public String ProductNr { get; set; }
        public String ProductName { get; set; }
        public String Type { get; set; }
        public decimal Quantity { get; set; }

    }
}
