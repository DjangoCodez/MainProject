
using SoftOne.Soe.Common.Attributes;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class ProjectWeekTotal
    {
        public int WorkTimeInMinutes { get; set; }
        public int InvoiceTimeInMinutes { get; set; }
        public int Year { get; set; }
        public int WeekNumber { get; set; }
        public int InvoiceProductId { get; set; }
        public int EmployeeId { get; set; }
        public int TimeCodeId { get; set; }
    }
}
