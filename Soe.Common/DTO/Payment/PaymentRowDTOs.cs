using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;


namespace SoftOne.Soe.Common.DTO
{
    public class PaymentRowDTO
    {
        public int PaymentRowId { get; set; }
        public int PaymentId { get; set; }
        public int? InvoiceId { get; set; }
        public int? PaymentImportId { get; set; }
        public int? VoucherHeadId { get; set; }

        public int SysPaymentTypeId { get; set; }
        public int SeqNr { get; set; }

        public decimal CurrencyRate { get; set; }
        public DateTime CurrencyDate { get; set; }

        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal AmountDiff { get; set; }
        public decimal AmountDiffCurrency { get; set; }
        public decimal AmountDiffEntCurrency { get; set; }
        public decimal AmountDiffLedgerCurrency { get; set; }
        public decimal BankFee { get; set; }
        public decimal BankFeeCurrency { get; set; }
        public decimal BankFeeEntCurrency { get; set; }
        public decimal BankFeeLedgerCurrency { get; set; }

        public bool HasPendingAmountDiff { get; set; }
        public bool HasPendingBankFee { get; set; }
        public bool IsSuggestion { get; set; }

        public DateTime PayDate { get; set; }
        public string PaymentNr { get; set; }

        public int Status { get; set; }
        public string StatusName { get; set; }
        public string StatusMsg { get; set; }
        public string Text { get; set; }    

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public int VoucherSeriesId { get; set; }
        public int VoucherSeriesTypeId { get; set; }
        public int? PaymentMethodId { get; set; }
        public string Description { get; set; }
        public TermGroup_BillingType BillingType { get; set; }
        public int? ActorId { get; set; }
        public int CurrencyId { get; set; }
        public DateTime? VoucherDate { get; set; }
        public decimal InvoiceTotalAmount { get; set; }
        public decimal InvoiceTotalAmountCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PaidAmountCurrency { get; set; }
        public bool FullyPaid { get; set; }
        public List<AccountingRowDTO> PaymentAccountRows { get; set; }
        public int? OriginStatus { get; set; }
        public string OriginDescription { get; set; }
        public bool VoucherHasMultiplePayments { get; set; }
        public bool IsRestPayment { get; set; }
        public int TransferStatus { get; set; }
    }

    
    public class PaymentRowImportIODTO
    {
        public int Type { get; set; }
        public string InvoiceNr { get; set; }
        public int? InvoiceSeqNr { get; set; }
        public string InvoiceExternalId { get; set; }
        public string VoucherNr { get; set; }
        public string SupplierNr { get; set; }
        public string VoucherSeriesNr { get; set; }
        public int SysPaymentTypeId { get; set; }
        public int SeqNr { get; set; }
        public decimal CurrencyRate { get; set; }
        public DateTime CurrencyDate { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public DateTime PayDate { get; set; }
        public DateTime VoucherDate { get; set; }
        public string PaymentNr { get; set; }
        public string PaymentMethodCode { get; set; }
        public bool FullyPaid { get; set; }
        public string AccountNr { get; set; }
        public string AccountDim2Nr { get; set; }
        public string AccountDim3Nr { get; set; }
        public string AccountDim4Nr { get; set; }
        public string AccountDim5Nr { get; set; }
        public string AccountDim6Nr { get; set; }
        public int Status { get; set; }

        public bool ChangeFullyPaid { get; set; }

        public string Comment { get; set; }
    }

    public class PaymentRowSmallDTO
    {
        public int PaymentRowId { get; set; }
        public int Status { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public DateTime PayDate { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime? Created { get; set; }
    }

    public class PaymentRowInvoiceDTO
    {
        public int PaymentId { get; set; }
        public int PaymentRowId { get; set; }
        public int Status { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public DateTime PayDate { get; set; }
        public string PaymentNr { get; set; }
        public int SeqNr { get; set; }
        public int CurrencyId { get; set; }
        public int InvoiceId { get; set; }
        public int? InvoiceSeqNr { get; set; }
        public string InvoiceNr { get; set; }
        public string InvoiceActorName { get; set; }
        public int? InvoiceActorId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? InvoiceDueDate { get; set; }
        public decimal InvoiceTotalAmount { get; set; }
        public decimal InvoicePaidAmount { get; set; }
        public bool FullyPayed { get; set; }

        public DateTime CurrencyDate { get; set; }
        public decimal CurrencyRate { get; set; }
        public int SysPaymentTypeId { get; set; }
        public int PaymentMethodId { get; set; }
        public int BillingType { get; set; }
    }
    public class PaymentSearchDTO
    {
        public int? InvoiceId { get; set; }
        public DateTime? PayDateFrom { get; set; }
        public DateTime? PayDateTo { get; set; }
        public DateTime? ModifiedFrom { get; set; }
        public DateTime? ModifiedTo { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}
