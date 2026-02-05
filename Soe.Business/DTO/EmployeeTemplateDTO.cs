using System;

namespace SoftOne.Soe.Business.DTO
{
    public class EmployeeTemplateHeadDTO
    {
        public int EmployeeId { get; set; }
        public int TimeScheduleTemplateHeadId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? StopDate { get; set; }
    }
}
