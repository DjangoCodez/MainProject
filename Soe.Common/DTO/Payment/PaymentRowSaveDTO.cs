using SoftOne.Soe.Common.Util;
using System;
using System.Runtime.Serialization;

namespace SoftOne.Soe.Common.DTO
{
    [KnownType(typeof(PaymentRowSaveDTO))]
    public class PaymentRowSaveBaseDTO
    {
        // Origin
        public int OriginId { get; set; }

        // Invoice
        public DateTime? InvoiceDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? VoucherDate { get; set; }
        public int ActorId { get; set; }
        public decimal CurrencyRate { get; set; }
    }

    public class PaymentRowSaveDTO : PaymentRowSaveBaseDTO
    {
        // Origin
        public SoeOriginType OriginType { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string OriginDescription { get; set; }
        public int VoucherSeriesId { get; set; }
        public int? VoucherSeriesTypeId { get; set; }
        public int AccountYearId { get; set; }

        // Invoice
        public int InvoiceId { get; set; }
        public SoeInvoiceType InvoiceType { get; set; }
        public bool OnlyPayment { get; set; }
        public TermGroup_BillingType BillingType { get; set; }
        public string InvoiceNr { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PaidAmountCurrency { get; set; }
        public int CurrencyId { get; set; }
        public DateTime CurrencyDate { get; set; }
        public bool FullyPayed { get; set; }

        // PaymentRow
        public int PaymentMethodId { get; set; }
        public int SeqNr { get; set; }
        public int SysPaymentTypeId { get; set; }
        public string PaymentNr { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountDiff { get; set; }
        public decimal AmountDiffCurrency { get; set; }
        public bool HasPendingAmountDiff { get; set; }
        public bool HasPendingBankFee { get; set; }
        public SoeEntityState State { get; set; }
        public int? VoucherHeadId { get; set; }
        public SoePaymentStatus? PaymentStatus { get; set; }

        public int? paymentRowId { get; set; }
        public bool IsRestPayment { get; set; }
        public string Text { get; set; }

        // PaymentImport
        public DateTime? ImportDate { get; set; }
        public string ImportFilename { get; set; }
        

        // Super support
        public bool IsSuperSupportSave { get; set; }

    }
}
