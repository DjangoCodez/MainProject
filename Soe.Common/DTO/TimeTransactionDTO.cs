using System;

namespace SoftOne.Soe.Common.DTO
{
    public class TimePayrollTransactionReportDTO
    {
        public int EmployeeId { get; set; }
        public int TimeBlockDateId { get; set; }
        public int TimeCodeId { get; set; }
        public int TimePayrollTransactionState { get; set; }
        public int TimeCodeTransactionState { get; set; }
        public int ProductId { get; set; }
        public int TimePayrollTransactionId { get; set; }
        public int? TimeCodeTransactionId { get; set; }
        public DateTime Date { get; set; }
        public string PayrollProductNumber { get; set; }
        public string PayrollProductName { get; set; }
        public decimal PayrollProductMinutes { get; set; }
        public decimal PayrollProductFactor { get; set; }
        public int? PayrollProductType { get; set; }
        public int? PayrollProductTypeLevel1 { get; set; }
        public int? PayrollProductTypeLevel2 { get; set; }
        public int? PayrollProductTypeLevel3 { get; set; }
        public int? PayrollProductTypeLevel4 { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? UnitPriceCurrency { get; set; }
        public decimal? UnitPriceEntCurrency { get; set; }
        public decimal? Amount { get; set; }
        public decimal? AmountCurrency { get; set; }
        public decimal? AmountEntCurrency { get; set; }
        public decimal? Vatamount { get; set; }
        public decimal? VatAmountCurrency { get; set; }
        public decimal? VatAmountEntCurrency { get; set; }
        public decimal? Quantity { get; set; }
        public int? PayrollTypeLevel1 { get; set; }
        public int? PayrollTypeLevel2 { get; set; }
        public int? PayrollTypeLevel3 { get; set; }
        public int? PayrollTypeLevel4 { get; set; }
        public bool PayedTime { get; set; }
        public string Formula { get; set; }
        public string FormulaExtracted { get; set; }
        public string FormulaNames { get; set; }
        public string FormulaOrigin { get; set; }
        public string FormulaPlain { get; set; }
        public string Note { get; set; }
        public string AccountNr { get; set; }
        public string AccountName { get; set; }
        public string AccountDim2Nr { get; set; }
        public string AccountDim2Name { get; set; }
        public string AccountDim3Nr { get; set; }
        public string AccountDim3Name { get; set; }
        public string AccountDim4Nr { get; set; }
        public string AccountDim4Name { get; set; }
        public string AccountDim5Nr { get; set; }
        public string AccountDim5Name { get; set; }
        public string AccountDim6Nr { get; set; }
        public string AccountDim6Name { get; set; }
        public string AttestStateName { get; set; }

    }
}
