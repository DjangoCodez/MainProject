using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class ExpenseHeadDTO
    {
        public int ExpenseHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public int? ProjectId { get; set; }
        public int TimeBlockDateId { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? Stop { get; set; }
        public string Comment { get; set; }
        public string Accounting { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public List<ExpenseRowDTO> ExpenseRows { get; set; }
    }

    public class ExpenseRowDTO
    {
        public int ExpenseRowId { get; set; }
        public int ExpenseHeadId { get; set; }
        public int ActorCompanyId { get; set; }
        public int EmployeeId { get; set; }
        public int TimeCodeId { get; set; }
        public int ProjectId { get; set; }
        public int CustomerInvoiceId { get; set; }
        public string CustomerInvoiceNr { get; set; }
        public int CustomerInvoiceRowId { get; set; }
        public int? TimePeriodId { get; set; }
        public DateTime Start { get; set; }
        public DateTime Stop { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal Vat { get; set; }
        public decimal VatCurrency { get; set; }
        public decimal VatLedgerCurrency { get; set; }
        public decimal VatEntCurrency { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitPriceCurrency { get; set; }
        public decimal UnitPriceLedgerCurrency { get; set; }
        public decimal UnitPriceEntCurrency { get; set; }
        public string Comment { get; set; }
        public string ExternalComment { get; set; }
        public string Accounting { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public decimal InvoicedAmount { get; set; }
        public decimal InvoicedAmountCurrency { get; set; }
        public decimal InvoicedAmountLedgerCurrency { get; set; }
        public decimal InvoicedAmountEntCurrency { get; set; }
        public bool IsSpecifiedUnitPrice { get; set; }
        public DateTime? StandOnDate { get; set; }

        // Collections
        public List<FileUploadDTO> Files { get; set; }

        // Extensions
        public string TimeCodeName { get; set; }
        public int? CustomerInvoiceRowAttestStateId { get; set; }
        public int? TimePayrollAttestStateId { get; set; }
        public bool TransferToOrder { get; set; }
        public bool isTimeReadOnly { get; set; }
        public bool isReadOnly { get; set; }
        public bool isDeleted { get; set; }
        public int InvoiceRowAttestStateId { get; set; }
        public string InvoiceRowAttestStateName { get; set; }
        public string InvoiceRowAttestStateColor { get; set; }
        public int PayrollAttestStateId { get; set; }
        public string PayrollAttestStateName { get; set; }
        public string PayrollAttestStateColor { get; set; }
        public bool HasFiles { get; set; }
    }

    [TSInclude]
    public class ExpenseRowGridDTO
    {
        public int ExpenseRowId { get; set; }
        public int ExpenseHeadId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeNumber { get; set; }
        public string EmployeeName { get; set; }
        public int TimeCodeId { get; set; }
        public int TimeCodeRegistrationType { get; set; }
        public string TimeCodeName { get; set; }
        public DateTime From { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal Vat { get; set; }
        public decimal VatCurrency { get; set; }
        public decimal AmountExVat { get; set; }
        public decimal InvoicedAmount { get; set; }
        public decimal InvoicedAmountCurrency { get; set; }
        public int InvoiceRowAttestStateId { get; set; }
        public string InvoiceRowAttestStateName { get; set; }
        public string InvoiceRowAttestStateColor { get; set; }
        public int PayrollAttestStateId { get; set; }
        public string PayrollAttestStateName { get; set; }
        public string PayrollAttestStateColor { get; set; }
        public DateTime? PayrollTransactionDate { get; set; }
        public string Comment { get; set; }
        public string ExternalComment { get; set; }

        public int ProjectId { get; set; }
        public string ProjectNr { get; set; }
        public string ProjectName { get; set; }

        public int? OrderId { get; set; }
        public string OrderNr { get; set; }

        public int? ActorCustomerId { get; set; }
        public string CustomerName { get; set; }

        public bool IsSpecifiedUnitPrice { get; set; }
        public bool HasFiles { get; set; }

        public List<int> TimePayrollTransactionIds { get; set; }
    }

    public class ExpenseRowProjectOverviewDTO
    {
        public int ExpenseRowId { get; set; }
        public DateTime From { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public decimal Quantity { get; set; }
        public decimal Amount { get; set; }
        public decimal PayrollAmount { get; set; }
        public int TimeCodeRegistrationType { get; set; }
        public string TimeCodeName { get; set; }
        public bool IsSpecifiedUnitPrice { get; set; }
    }

    public class ExpenseRowForReportDTO: ExpenseRowProjectOverviewDTO
    {
        public int TimeCodeId { get; set; }
        public int TimePayrollTransactionId { get; set; }
        public string PayrollAttestStateName { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal InvoicedAmount { get; set; }
        public decimal PayRollAmount { get; set; }
        public string EmployeeNumber { get; set; }

    }

    public class SaveExpenseValidationDTO
    {
        public bool Success { get; set; }
        public bool CanOverride { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }

        public SaveExpenseValidationDTO()
        {
            this.Success = true;
        }

        public SaveExpenseValidationDTO(bool success, bool canOverride, string title, string message)
        {
            this.Success = success;
            this.CanOverride = canOverride;
            this.Title = title;
            this.Message = message;
        }
    }
}
