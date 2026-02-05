using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmploymentDaysItem
    {
        public string EmploymentNumber { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CurrentWorkingPlace { get; set; }
        public DateTime EmploymentStartDate { get; set; }
        public DateTime? EmploymentEndDate { get; set; }
        public string CurrentEmploymentType { get; set; }
        public string CurrentTimeAgreement { get; set; }
        public Dictionary<int, int> EmploymentTypesDays { get; set; }
        public int TotalEmploymentDays { get; set; }
        public int LASTypeAvaDays { get; set; }
        public int LASTypeSvaDays { get; set; }
        public int LASTypeVikDays { get; set; }
        public int LASTypeOtherDays { get; set; }
        public int IsFixedButNotSubstituteDays { get; set; }
    }
}
