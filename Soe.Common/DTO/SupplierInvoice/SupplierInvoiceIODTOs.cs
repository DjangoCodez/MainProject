using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class SupplierInvoiceRowIODTO
    {
        public decimal? Amount;
        public decimal? AmountCurrency;
        public decimal? Quantity;
        public string AccountNr;
        public string AccountDim2Nr;
        public string AccountDim3Nr;
        public string AccountDim4Nr;
        public string AccountDim5Nr;
        public string AccountDim6Nr;
        public string AccountSieDim1;
        public string AccountSieDim6;
        public string Text;
    }
    public class SupplierInvoiceIODTO
    {
        public List<SupplierInvoiceRowIODTO> InvoiceRows = new List<SupplierInvoiceRowIODTO>();

        public string InvoiceNr { get; set; }
        public int? InvoiceId { get; set; }
        public int? SeqNr { get; set; }
        public int OriginStatus { get; set; }
        public string OriginStatusName { get; set; }

        public string BatchId { get; set; }

        public int? SupplierId { get; set; }
        public string SupplierNr { get; set; }
        public string SupplierExternalNr { get; set; }
        public string SupplierName { get; set; }
        public string SupplierOrgnr { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? VoucherDate { get; set; }
        public string ReferenceOur { get; set; }
        public string ReferenceYour { get; set; }
        public decimal? CurrencyRate { get; set; }
        public DateTime? CurrencyDate { get; set; }
        public string Currency { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalAmountCurrency { get; set; }
        public decimal? VATAmount { get; set; }
        public decimal? VATAmountCurrency { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? PaidAmountCurrency { get; set; }
        public decimal? RemainingAmount { get; set; }

        public decimal? CentRounding { get; set; }
        public bool? FullyPayed { get; set; }
        public string PaymentNr { get; set; }
        public string VoucherNr { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        public int VatType { get; set; }
        public string PaymentConditionCode { get; set; }
        public string VatAccountNr { get; set; }
        public string WorkingDescription { get; set; }
        public string InternalDescription { get; set; }
        public string ExternalDescription { get; set; }
        public string ProjectNr { get; set; }

        public int BillingType { get; set; }
        public string InvoiceLabel { get; set; }
        public string InvoiceHeadText { get; set; }

        public string ExternalId { get; set; }


        public string OCR { get; set; }
    }
    [TSInclude]
    public class SupplierInvoiceHeadIODTO
    {
        public int SupplierInvoiceHeadIOId { get; set; }
        public int ActorCompanyId { get; set; }
        public bool Import { get; set; }
        public TermGroup_IOType Type { get; set; }
        public TermGroup_IOStatus Status { get; set; }
        public TermGroup_IOSource Source { get; set; }
        public TermGroup_IOImportHeadType ImportHeadType { get; set; }
        public string BatchId { get; set; }
        public string ErrorMessage { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }

        public int? SupplierId { get; set; }
        public string SupplierNr { get; set; }

        public int? InvoiceId { get; set; }
        public string SupplierInvoiceNr { get; set; }
        public int? SeqNr { get; set; }
        public int? BillingType { get; set; }

        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? VoucherDate { get; set; }

        public string ReferenceOur { get; set; }
        public string ReferenceYour { get; set; }
        public string OCR { get; set; }

        public int? CurrencyId { get; set; }
        public string Currency { get; set; }
        public decimal? CurrencyRate { get; set; }
        public DateTime? CurrencyDate { get; set; }

        public decimal? TotalAmount { get; set; }
        public decimal? TotalAmountCurrency { get; set; }
        public decimal? VATAmount { get; set; }
        public decimal? VATAmountCurrency { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? PaidAmountCurrency { get; set; }
        public decimal? RemainingAmount { get; set; }
        public bool? FullyPayed { get; set; }
        public string PaymentNr { get; set; }
        public string VoucherNr { get; set; }
        public bool? CreateAccountingInXE { get; set; }

        public string Note { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public string BillingTypeName { get; set; }
        public string StatusName { get; set; }

        // Flags
        public bool IsSelected { get; set; }
        public bool IsModified { get; set; }
    }

    #region Filter
    public class SupplierInvoiceFilterIODTO
    {
        public int? InvoiceId { get; set; }
        public string Number { get; set; }

        public DateTime? InvoiceDateFrom { get; set; }
        public DateTime? InvoiceDateTo { get; set; }

        public bool IncludePreliminary { get; set; }

        public int PageNumber { get; set; }
        public int PageNrOfRecords { get; set; }

        public string SupplierNr { get; set; }
    }

    #endregion
}

public class SupplierInvoiceImageIODTO
{
    public int InvoiceId { get; set; }
    public int FileId { get; set; }
    public string Base64Data { get; set; }
    public string Filename { get; set; }
    public string MimeType { get; set; }
}
