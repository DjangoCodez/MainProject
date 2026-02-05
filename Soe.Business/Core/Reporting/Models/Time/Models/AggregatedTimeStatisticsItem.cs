namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class AggregatedTimeStatisticsItem
    {
        public string AccountNr { get; set; }
        public string AccountName { get; set; }
        public string AccountDimName { get; set; }
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeePosition { get; set; }
        public decimal EmployeeWeekWorkHours { get; set; }
        public decimal EmployeeSalary { get; set; }
        public decimal WorkHoursTotal { get; set; }
        public decimal InconvinientWorkingHours { get; set; }
        public decimal InconvinientWorkingHoursLevel40 { get; set; }
        public decimal InconvinientWorkingHoursLevel57 { get; set; }
        public decimal InconvinientWorkingHoursLevel79 { get; set; }
        public decimal InconvinientWorkingHoursLevel113 { get; set; }
        public decimal InconvinientWorkingHoursLevel50 { get; set; }
        public decimal InconvinientWorkingHoursLevel70 { get; set; }
        public decimal InconvinientWorkingHoursLevel100 { get; set; }
        public decimal AddedTimeHours { get; set; }
        public decimal AddedTimeHoursLevel35 { get; set; }
        public decimal AddedTimeHoursLevel70 { get; set; }
        public decimal AddedTimeHoursLevel100 { get; set; }
        public decimal OverTimeHours { get; set; }
        public decimal OverTimeHoursLevel35 { get; set; }
        public decimal OverTimeHoursLevel50 { get; set; }
        public decimal OverTimeHoursLevel70 { get; set; }
        public decimal OverTimeHoursLevel100 { get; set; }
        public decimal SicknessHours { get; set; }
        public decimal VacationHours { get; set; }
        public decimal AbsenceHours { get; set; }
        public decimal CostTotal { get; set; }
        public decimal CostCalenderDayWeek { get; set; }
        public decimal CostCalenderDay { get; set; }
        public decimal CostNetHours { get; set; }
        public decimal InconvinientWorkingCost { get; set; }
        public decimal InconvinientWorkingCostLevel40 { get; set; }
        public decimal InconvinientWorkingCostLevel57 { get; set; }
        public decimal InconvinientWorkingCostLevel79 { get; set; }
        public decimal InconvinientWorkingCostLevel113 { get; set; }
        public decimal InconvinientWorkingCostLevel50 { get; set; }
        public decimal InconvinientWorkingCostLevel70 { get; set; }
        public decimal InconvinientWorkingCostLevel100 { get; set; }
        public decimal AddedTimeCost { get; set; }
        public decimal AddedTimeCostLevel35 { get; set; }
        public decimal AddedTimeCostLevel70 { get; set; }
        public decimal AddedTimeCostLevel100 { get; set; }
        public decimal OverTimeCost { get; set; }
        public decimal OverTimeCostLevel35 { get; set; }
        public decimal OverTimeCostLevel50 { get; set; }
        public decimal OverTimeCostLevel70 { get; set; }
        public decimal OverTimeCostLevel100 { get; set; }
        public decimal SicknessCost { get; set; }
        public decimal VacationCost { get; set; }
        public decimal AbsenceCost { get; set; }
        public decimal ScheduleNetQuantity { get; set; }
        public decimal ScheduleGrossQuantity { get; set; }
        public decimal ScheduleNetAmount { get; set; }
        public decimal ScheduleGrossAmount { get; set; }
    }
}
