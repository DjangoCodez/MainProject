using System;

namespace SoftOne.Soe.Business.DTO
{
    public class EmployeeGroupHolidayDTO
    {
        public int EmployeeGroupId { get; set; } //For future use
        public String Date { get; set; }
        public String Name { get; set; }
        public String DayTypeName { get; set; }
        public String Type { get; set; }
    }
}
