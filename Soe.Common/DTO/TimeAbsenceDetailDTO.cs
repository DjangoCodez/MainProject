using System;

namespace SoftOne.Soe.Common.DTO
{
    public class TimeAbsenceDetailDTO
    {
        public int TimeBlockDateDetailId { get; set; }
        public int TimeBlockDateId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNrAndName { get; set; }
        public DateTime Date { get; set; }
        public string DayName { get; set; }
        public int WeekNr { get; set; }
        public string WeekInfo { get; set; }
        public int DayOfWeekNr { get; set; }
        public int? HolidayId { get; set; }
        public string HolidayName { get; set; }
        public bool IsHoliday { get; set; }
        public int? DayTypeId { get; set; }
        public string DayTypeName { get; set; }
        public string HolidayAndDayTypeName
        {
            get
            {
                if (!String.IsNullOrEmpty(HolidayName))
                    return $"{DayTypeName} ({HolidayName})";
                else
                    return $"{DayTypeName}";
            }
        }
        public int? TimeDeviationCauseId { get; set; }
        public string TimeDeviationCauseName { get; set; }
        public int SysPayrollTypeLevel3 { get; set; }
        public string SysPayrollTypeLevel3Name { get; set; }
        public decimal? Ratio { get; set; }
        public string RatioText { get; set; }
        public bool ManuallyAdjusted { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }
}
