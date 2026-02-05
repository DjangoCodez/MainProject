using System;

namespace SoftOne.Soe.Common.DTO
{
    public class EmploymentCalenderDTO
    {
        public string Key
        {
            get
            {
                return $"{EmployeeId}";
            }
        }
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public DateTime Date { get; set; }
        public int EmploymentId { get; set; }
        public int? PayrollGroupId { get; set; }
        public int? EmployeeGroupId { get; set; }
        public int? VacationGroupId { get; set; }
        public int? AnnualLeaveGroupId { get; set; }
        public decimal Percent { get; set; }
        public int? DayTypeId { get; set; }
        public DateTime CacheValidTo { get; set; }
        public string DayTypeName { get; set; }
        public int EmploymentTypeId { get; set; }
        public int EmploymentType { get; set; }
        public string EmploymentTypeName { get; set; }

        public bool IsValid()
        {
            return this.CacheValidTo >= DateTime.UtcNow;
        }
    }
}
