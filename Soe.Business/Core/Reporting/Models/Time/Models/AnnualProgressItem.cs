using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class AnnualProgressItem
    {
        public AnnualProgressItem()
        {
            AccountAnalysisFields = new List<AccountAnalysisField>();
        }

        public DateTime Date { get; set; }
        public decimal GoalPerWeek { get; set; }
        public decimal GoalPerMonth { get; set; }
        public decimal AveragePerWeekToDate { get; set; }
        public decimal AveragePerMonthToDate { get; set; }
        public decimal LastYearAveragePerWeek { get; set; }
        public decimal LastYearAveragePerMonth { get; set; }
        public decimal DifferenceAverageFromLastYearPerWeek { get; set; }
        public decimal DifferenceAverageFromLastYearPerMonth { get; set; }
        public decimal DifferenceAverageFromGoalPerWeek { get; set; }
        public decimal DifferenceAverageFromGoalPerMonth { get; set; }
        public decimal RemainingYearAveragePerWeek { get; set; }
        public decimal RemainingYearAveragePerMonth { get; set; }
        public decimal SalesToDate { get; set; }
        public decimal WorkingHoursToDate { get; set; }
        public decimal FPATGoal { get; set; }
        public decimal FPATToDate { get; set; }
        public List<AccountAnalysisField> AccountAnalysisFields { get; set; }
    }
}
