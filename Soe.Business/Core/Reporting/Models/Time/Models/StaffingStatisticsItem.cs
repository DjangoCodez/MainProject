using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class StaffingStatisticsItem
    {
        public StaffingStatisticsItem()
        {
            AccountAnalysisFields = new List<AccountAnalysisField>();
        }

        public DateTime Date { get; set; }
        public string AccountNr { get; set; }
        public string AccountName { get; set; }
        public decimal ForecastSales { get; set; }
        public decimal ForecastHours { get; set; }
        public decimal ForecastPersonelCost { get; set; }
        public decimal ForecastSalaryPercent { get; set; }
        public decimal ForecastLPAT { get; set; }
        public decimal ForecastFPAT { get; set; }
        public decimal ForecastBPAT { get; set; }

        public decimal BudgetSales { get; set; }
        public decimal BudgetHours { get; set; }
        public decimal BudgetPersonelCost { get; set; }
        public decimal BudgetSalaryPercent { get; set; }
        public decimal BudgetLPAT { get; set; }
        public decimal BudgetFPAT { get; set; }
        public decimal BudgetBPAT { get; set; }

        public decimal TemplateScheduleSales { get; set; }
        public decimal TemplateScheduleHours { get; set; }
        public decimal TemplateSchedulePersonelCost { get; set; }
        public decimal TemplateScheduleSalaryPercent { get; set; }
        public decimal TemplateScheduleLPAT { get; set; }
        public decimal TemplateScheduleFPAT { get; set; }
        public decimal TemplateScheduleBPAT { get; set; }

        public decimal ScheduleSales { get; set; }
        public decimal ScheduleHours { get; set; }
        public decimal SchedulePersonelCost { get; set; }
        public decimal ScheduleSalaryPercent { get; set; }
        public decimal ScheduleLPAT { get; set; }
        public decimal ScheduleFPAT { get; set; }
        public decimal ScheduleBPAT { get; set; }

        public decimal TimeSales { get; set; }
        public decimal TimeHours { get; set; }
        public decimal TimePersonelCost { get; set; }
        public decimal TimeSalaryPercent { get; set; }
        public decimal TimeLPAT { get; set; }
        public decimal TimeFPAT { get; set; }
        public decimal TimeBPAT { get; set; }

        public decimal ScheduleAndTimeHours { get; set; }
        public decimal ScheduleAndTimePersonalCost { get; set; }

        public List<AccountAnalysisField> AccountAnalysisFields { get; set; }
        public string EmployeeName { get; internal set; }
    }
}
