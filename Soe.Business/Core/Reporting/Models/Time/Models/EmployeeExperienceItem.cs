using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmployeeExperienceItem
    {
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string SSN { get; set; }
        public int? Age { get; set; }
        public int? ExperienceIn { get; set; }
        public int? ExperienceTot { get; set; }
        public string ExperienceType { get; set; }
        public String SalaryType { get; set; }
        public DateTime? SalaryDate { get; set; }
        public decimal Salary { get; set; }
        public string SalaryTypeName { get;  set; }
    }
   

}
