using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmploymentHistoryItem
    {

        public string EmploymentNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string WorkingPlace { get; set; }
        public DateTime EmploymentStartDate { get; set; }
        public DateTime? EmploymentEndDate { get; set; }
        public string ReasonForEndingEmployment { get; set; }
        public string EmploymentType { get; set; }
        public int TotalEmploymentDays { get; set; }
        public decimal EmploymentPercentage { get; set; }
        public int LASDays { get; set; }
    }
}
