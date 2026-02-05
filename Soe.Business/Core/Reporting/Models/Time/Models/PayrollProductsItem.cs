
namespace SoftOne.Soe.Business.Core.Reporting.Models.Time.Models
{
    public class PayrollProductsItem
    {
        public string  Number { get; set; }
        public string Name { get; set; }
        public string  ShortName { get; set; }
        public string ExternalNumber { get; set; }
        public string ResultType { get; set; }
        public decimal ProductFactor { get; set; }
        public string syspayrolltypelevel1 { get; set; }
        public string syspayrolltypelevel2 { get; set; }
        public string syspayrolltypelevel3 { get; set; }
        public string syspayrolltypelevel4 { get; set; }
        public bool PayrollProductPayed { get; set; }
        public bool ExcludeInWorkTimeSummary { get; set; }
        public bool AverageCalculated { get; set; }
        public bool UseInPayroll{ get; set; }
        public bool DontUseFixedAccounting  { get; set; }
        public bool ProductExport  { get; set; }
        public bool IncludeAmountInExport { get; set; }
        public string Payrollgroup { get; set; }
        public string CentroundingType { get; set; }
        public string CentroundingLevel { get; set; }
        public string TaxCalculationType { get; set; }
        public string PensionCompany { get; set; }
        public string TimeUnit { get; set; }
        public string QuantityRoundingType { get; set; }
        public int QuantityRoundingMinutes { get; set; }
        public string ChildProduct { get; set; }
        public bool PrintOnSalaryspecification  { get; set; }
        public bool DontPrintOnSalarySpecificationWhenZeroAmount { get; set; }
        public bool ShowPrintDate { get; set; }
        public bool DontIncludeInRetroactivePayroll { get; set; }
        public bool VacationSalaryPromoted { get; set; }
        public bool UnionFeePromoted { get; set; }
        public bool WorkingTimePromoted { get; set; }
        public bool CalculateSupplementCharge { get; set; }
        public bool CalculateSicknessSalary { get; set; }
        public string Payrollpricetypes { get; set; }  
        public string Payrollpriceformulas { get; set; }
        public string AccountingPurchase { get; set; }
        public string AccountingPrioName { get; set; }
        public int PayrollProductId { get; set; }
    }
}
