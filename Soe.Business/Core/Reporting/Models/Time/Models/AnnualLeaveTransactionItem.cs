

using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class AnnualLeaveTransactionItem
    {
        public string EmployeeNr { get; set; }
        public string EmployeeName { get; set; }
        public DateTime? DateEarned { get; set; }
        public DateTime? YearEarned { get; set; }
        public DateTime? DateSpent { get; set; }
        public decimal Accumulation { get; set; }
        public decimal Hours { get; set; }
        public decimal EarnedHours { get; set; }
        public decimal EarnedDays { get; set; }
        public decimal SpentHours { get; set; }
        public decimal SpentDays { get; set; }
        public decimal BalanceHours { get; set; }
        public decimal BalanceDays { get; set; }
        public string TypeName { get; set; }

    }
}
