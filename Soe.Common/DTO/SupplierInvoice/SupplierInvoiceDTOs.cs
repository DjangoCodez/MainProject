using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class SupplierInvoiceRowDTO
    {
        // Keys
        public int SupplierInvoiceRowId { get; set; }
        public int InvoiceId { get; set; }

        public decimal? Quantity { get; set; }

        // Amounts
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal VatAmountEntCurrency { get; set; }
        public decimal VatAmountLedgerCurrency { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public List<AccountingRowDTO> AccountingRows { get; set; }
    }

    public class SupplierInvoiceSearchDTO
    {
        public int InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public int? SeqNr { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public string SupplierNr { get; set; }
        public string SupplierName { get; set; }
    }

    [TSInclude]
    public class SupplierInvoiceGridDTO
    {
        public int SupplierInvoiceId { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public string SeqNr { get; set; }
        public string InvoiceNr { get; set; }
        public int BillingTypeId { get; set; }
        public string BillingTypeName { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierNr { get; set; }
        public string InternalText { get; set; }
        public decimal TotalAmount { get; set; }
        public string TotalAmountText { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        public string TotalAmountCurrencyText { get; set; }
        public decimal TotalAmountExVat { get; set; }
        public string TotalAmountExVatText { get; set; }
        public decimal TotalAmountExVatCurrency { get; set; }
        public string TotalAmountExVatCurrencyText { get; set; }
        public decimal VATAmount { get; set; }
        public decimal VATAmountCurrency { get; set; }
        public decimal PayAmount { get; set; }
        public string PayAmountText { get; set; }
        public decimal PayAmountCurrency { get; set; }
        public string PayAmountCurrencyText { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PaidAmountCurrency { get; set; }
        public int VatType { get; set; }
        public decimal VatRate { get; set; }
        public int SysCurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrencyRate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? PayDate { get; set; }
        public DateTime? VoucherDate { get; set; }
        public int? AttestStateId { get; set; }
        public string AttestStateName { get; set; }
        public string CurrentAttestUserName { get; set; }
        public int AttestGroupId { get; set; }
        public string AttestGroupName { get; set; }
        public bool isAttestRejected { get; set; }
        public int OwnerActorId { get; set; }
        public bool FullyPaid { get; set; }
        public string PaymentStatuses { get; set; }
        public DateTime? TimeDiscountDate { get; set; }
        public decimal? TimeDiscountPercent { get; set; }
        public int StatusIcon { get; set; }
        public bool MultipleDebtRows { get; set; }
        public bool HasVoucher { get; set; }
        public string Ocr { get; set; }
        public bool HasAttestComment { get; set; }
        public int NoOfPaymentRows { get; set; }
        public int NoOfCheckedPaymentRows { get; set; }
        public bool BlockPayment { get; set; }
        public string BlockReason { get; set; }

        //GUI Properties
        public bool UseClosedStyle { get; set; }
        public bool IsSelectDisabled { get; set; }
        public Guid Guid { get; set; }
        public bool IsOverdue { get; set; }
        public bool IsAboutToDue { get; set; }

        //EDi entrys
        public int EdiEntryId { get; set; }
        public decimal RoundedInterpretation { get; set; }
        public bool HasPDF { get; set; }
        public int EdiType { get; set; }
        public int ScanningEntryId { get; set; }
        public string OperatorMessage { get; set; }
        public int ErrorCode { get; set; }
        public int InvoiceStatus { get; set; }
        public string SourceTypeName { get; set; }
        public DateTime? Created { get; set; }
        public int EdiMessageType { get; set; }
        public string EdiMessageTypeName { get; set; }
        public string EdiMessageProviderName { get; set; }

        public string ErrorMessage { get; set; }
        public decimal ProjectAmount { get; set; }
        public decimal ProjectInvoicedAmount { get; set; }
        public decimal ProjectInvoicedSalesAmount { get; set; }
        public bool IsClosed { get; set; }
	}

    [TSInclude]
    public class SupplierInvoiceDTO : InvoiceDTO
    {
        // Keys
        public int? PaymentMethodId { get; set; }
        public int? AttestStateId { get; set; }
        public int? AttestGroupId { get; set; }

        // Flags
        public bool InterimInvoice { get; set; }
        public bool MultipleDebtRows { get; set; }
        public bool BlockPayment { get; set; }

        // VatDeduction 
        public TermGroup_VatDeductionType VatDeductionType { get; set; }
        public int? VatDeductionAccountId { get; set; }
        public decimal VatDeductionPercent { get; set; }


        // Relations
        public List<SupplierInvoiceRowDTO> SupplierInvoiceRows { get; set; }
        public List<SupplierInvoiceProjectRowDTO> SupplierInvoiceProjectRows { get; set; }
        public List<SupplierInvoiceOrderRowDTO> SupplierInvoiceOrderRows { get; set; }
        public List<SupplierInvoiceCostAllocationDTO> SupplierInvoiceCostAllocationRows { get; set; }

        public List<FileUploadDTO> SupplierInvoiceFiles { get; set; }

        // Block
        public int? BlockReasonTextId { get; set; }
        public string BlockReason { get; set; }

        // Order 
        public int? OrderCustomerInvoiceId { get; set; }
        public string OrderCustomerName { get; set; }
        public int? OrderProjectId { get; set; }

        // Extensions
        public int PrevInvoiceId { get; set; }
        public bool HasImage { get; set; }
        public string AttestStateName { get; set; }

        public GenericImageDTO Image { get; set; }

        public int EdiEntryId { get; set; }
        public int? OrderNr { get; set; }
        public GenericImageDTO ScanningImage { get; set; }
    }

    public class SupplierInvoiceSmallDTO : InvoiceTinyDTO
    {
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierNr { get; set; }
        public int? VoucherHeadId { get; set; }
    }

    [TSInclude]
    public class SupplierInvoiceIncomingHallGridDTO
    {
        public int InvoiceId { get; set; }
        public TermGroup_SupplierInvoiceSource InvoiceSource { get; set; }
        public string InvoiceSourceName { get; set; }
        public TermGroup_BillingType BillingTypeId { get; set; }
        public string InvoiceNr { get; set; }
        public int SupplierId { get; set; }
        public string SupplierNr { get; set; }
        public string SupplierName { get; set; }
        public string InternalText { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public TermGroup_SupplierInvoiceStatus InvoiceState { get; set; }
        public string InvoiceStateName { get; set; }
        public DateTime? Created { get; set; }
        public int EdiEntryId { get; set; } = 0;
        public bool HasPDF { get; set; }
        public int EdiType { get; set; }
        public int ScanningEntryId { get; set; } = 0;
        public int? SupplierInvoiceHeadIOId { get; set; }
        public bool? BlockPayment { get; set; }
        public bool? UnderInvestigation { get; set; }

        public decimal TotalAmountExcludingVat
        {
            get => TotalAmount - VatAmount;
        }

		public decimal TotalAmountCurrencyExcludingVat
		{
			get => TotalAmountCurrency - VatAmountCurrency;
		}

        public bool IsOverdue => DueDate.HasValue && DueDate.Value.Date < DateTime.Today.Date;
        public bool IsAboutToDue => DueDate.HasValue && DueDate.Value.Date <= DateTime.Today.AddDays(5).Date;

        public int? SysCurrencyId { get; set; }
        public string CurrencyCode { get; set; }

        public int? AttestGroupId { get; set; }
        public string AttestGroupName { get; set; }

        public SoeOriginStatus OriginStatus { get; set; }
        public TermGroup_SupplierInvoiceType SupplierInvoiceType { get; set; }

    }

	[TSInclude]
	public class SupplierInvoiceHistoryGridDTO
    {
        public int InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public string ApprovalGroup { get; set; } = "";
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        public decimal VATAmount { get; set; }
        public decimal VATAmountCurrency { get; set; }

		public decimal TotalAmountExcludingVAT
		{
			get => TotalAmount - VATAmount;
		}

		public decimal TotalAmountCurrencyExcludingVAT
		{
			get => TotalAmountCurrency - VATAmountCurrency;
		}
	}

	[TSInclude]
	public class SupplierInvoiceHistoryDetailsDTO
	{
		public int InvoiceId { get; set; }
		public string InvoiceNr { get; set; }
        public string PaymentNr { get; set; } = "";
        public DateTime InvoiceDate { get; set; }
		public DateTime DueDate { get; set; }
        public string InvoiceType { get; set; } = "";
        public string SupplierReference { get; set; } = "";
        public string VATType { get; set; } = "";
        public string VATCode { get; set; } = "";
        public string OurReference { get; set; } = "";
        public IEnumerable<AccountingRowDTO> Accounting { get; set; } = new List<AccountingRowDTO>();
        public decimal TotalAmount { get; set; }
		public decimal TotalAmountCurrency { get; set; }
		public decimal VATAmount { get; set; }
		public decimal VATAmountCurrency { get; set; }

		public decimal TotalAmountExcludingVAT
		{
			get => TotalAmount - VATAmount;
		}

		public decimal TotalAmountCurrencyExcludingVAT
		{
			get => TotalAmountCurrency - VATAmountCurrency;
		}
	}

	[TSInclude]
	public class SupplierInvoiceSummaryDTO
    {
        public int ActorCompanyId { get; set; }
        public string ActorCompanyName { get; set; }
        public long UnhandledInvoiceCount { get; set; }
        public long AttestingInvoiceCount { get; set; }
        public long PaymentReadyInvoiceCount { get; set; }
    }
}
