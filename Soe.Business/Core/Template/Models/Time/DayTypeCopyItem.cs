using System;

namespace SoftOne.Soe.Business.Core.Template.Models.Time
{
    public class DayTypeCopyItem
    {
        public int DayTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? StandardWeekdayFrom { get; set; }
        public int? StandardWeekdayTo { get; set; }
        public int Type { get; set; }
        public int State { get; set; }
        public int? SysDayTypeId { get; set; }
    }

    public class TimeHalfDayCopyItem
    {
        public int TimeHalfDayId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Type { get; set; }
        public decimal Value { get; set; }
        public int State { get; set; }
    }

    public class HolidayCopyItem
    {
        public int DayTypeId { get; set; }
        public int HolidayId { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int State { get; set; }
        public int? SysHolidayId { get; set; }
        public int? SysHolidayTypeId { get; set; }
        public bool IsRedDay { get; set; }
    }
}
