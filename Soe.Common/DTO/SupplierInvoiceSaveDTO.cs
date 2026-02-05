using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    public class SupplierInvoiceSaveDTO
    {
        // Origin
        public int VoucherSeriesId { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string OriginDescription { get; set; } // Internal text

        // Invoice
        public int InvoiceId { get; set; }
        public TermGroup_BillingType BillingType { get; set; }
        public TermGroup_InvoiceVatType VatType { get; set; }
        public int ActorId { get; set; }
        public int? VatCodeId { get; set; }

        public int? SeqNr { get; set; }
        public string InvoiceNr { get; set; }
        public string OCR { get; set; }

        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? VoucherDate { get; set; }

        public DateTime? TimeDiscountDate { get; set; }
        public Decimal? TimeDiscountPercent { get; set; }

        public int CurrencyId { get; set; }
        public DateTime CurrencyDate { get; set; }
        public decimal CurrencyRate { get; set; }

        public string ReferenceOur { get; set; }
        public string ReferenceYour { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PaidAmountCurrency { get; set; }

        public bool FullyPayed { get; set; }
        public int? SysPaymentTypeId { get; set; }
        public string PaymentNr { get; set; }
        public SoeStatusIcon StatusIcon { get; set; }
        public int? ProjectId { get; set; }

        // SupplierInvoice
        public bool InterimInvoice { get; set; }
        public bool BlockPayment { get; set; }
        public int? PaymentMethodId { get; set; }
        public int? AttestStateId { get; set; }
        public int? AttestGroupId { get; set; }

        public int EdiEntryId { get; set; }

        //Dims
        public int? Dim1AccountId { get; set; }
        public int? Dim2AccountId { get; set; }
        public int? Dim3AccountId { get; set; }
        public int? Dim4AccountId { get; set; }
        public int? Dim5AccountId { get; set; }
        public int? Dim6AccountId { get; set; }
    }
}
