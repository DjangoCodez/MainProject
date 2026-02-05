namespace SoftOne.Soe.Common.DTO
{
    public class DrillDownReportDTO
    {
        public string GroupName { get; set; }
        public string HeaderName { get; set; }
        public string AccountNr { get; set; }
        public string AccountName { get; set; }
        public string VoucherDescription { get; set; }
        public decimal PeriodAmount { get; set; }
        public decimal YearAmount { get; set; }
        public decimal PreviousYearAmount { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public decimal PeriodVsYearDiff { get; set; }
        public decimal PeriodVsBudgetDiff { get; set; }
    }


}
