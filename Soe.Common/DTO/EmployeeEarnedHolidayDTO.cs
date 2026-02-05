
using SoftOne.Soe.Common.Attributes;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class EmployeeEarnedHolidayDTO
    {
        public int EmployeeId { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public decimal EmployeePercent { get; set; }
        public bool Work5DaysPerWeek { get; set; }
        public string Work5DaysPerWeekString { get; set; }
        public bool HasTransaction { get; set; }
        public string HasTransactionString { get; set; }
        public bool? Suggestion { get; set; }
        public string SuggestionString { get; set; }
        public string SuggestionNote { get; set; }
    }
}
