using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class EmployeeSkillItem
    {
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CategoryName { get; set; }
        public int id { get; set; }
        public string SkillTypeName { get; set; }
        public int SkillTypeId { get; set; }
        public string SkillDescription { get; set; }
        public string SkillTypeDescription { get; set; }
        public string SkillName { get; set; }
        public DateTime? SkillDate { get; set; }
        public int? SkillLevel { get; set; }
        public int? BirthYear { get; set; }
        public string SSYKCode { get; set; }
        public string PositionName { get; set; }
        public string Gender { get; set; }
        public string EmploymentTypeName { get; set; }

    }
}
