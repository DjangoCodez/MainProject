using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    public class CustomerInvoiceSaveDTO
    {
        // Origin
        public int VoucherSeriesId { get; set; }
        public int? VoucherSeriesTypeId {  get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string OriginDescription { get; set; } // Internal text
        public string OrderNumbers { get; set; }

        // Invoice
        public int PrevInvoiceId { get; set; }
        public int InvoiceId { get; set; }
        public TermGroup_BillingType BillingType { get; set; }
        public TermGroup_InvoiceVatType VatType { get; set; }
        public int ActorId { get; set; }
        public TermGroup_OrderType OrderType { get; set; }

        public string InvoiceNr { get; set; }
        public string OCR { get; set; }
        public string ProjectNr { get; set; }
        public int ProjectId { get; set; }
        public int DeliveryCustomerId { get; set; }

        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? VoucherDate { get; set; }
        public int? VoucherHeadId { get; set; }

        public int CurrencyId { get; set; }
        public DateTime CurrencyDate { get; set; }
        public decimal CurrencyRate { get; set; }

        public string ReferenceOur { get; set; }
        public string ReferenceYour { get; set; }
        public int? ContactEComId { get; set; }
        public int? ContactGLNId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal RemainingAmount { get; set; }

        public bool ManuallyAdjustedAccounting { get; set; }

        public bool IsTemplate { get; set; }
        public bool? FullyPayed { get; set; }

        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeStatusIcon StatusIcon { get; set; }

        public string InternalDescription { get; set; }
        public string ExternalDescription { get; set; }
        public string ExternalId { get; set; }

        // CustomerInvoice
        public OrderInvoiceRegistrationType RegistrationType { get; set; }

        public DateTime? OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public int DeliveryAddressId { get; set; }
        public int BillingAddressId { get; set; }
        public int DeliveryTypeId { get; set; }
        public int DeliveryConditionId { get; set; }
        public int PaymentConditionId { get; set; }
        public int? InvoiceDeliveryType { get; set; }
        public int? InvoiceDeliveryProvider { get; set; }
        public int? InvoicePaymentService { get; set; }

        public decimal SumAmount { get; set; }
        public decimal SumAmountCurrency { get; set; }
        public decimal FreightAmount { get; set; }
        public decimal FreightAmountCurrency { get; set; }
        public decimal InvoiceFee { get; set; }
        public decimal InvoiceFeeCurrency { get; set; }
        public decimal CentRounding { get; set; }

        public string InvoiceText { get; set; }
        public string InvoiceHeadText { get; set; }
        public string InvoiceLabel { get; set; }
        public string ContractNr { get; set; }

        public decimal MarginalIncome { get; set; }
        public decimal MarginalIncomeCurrency { get; set; }
        public decimal MarginalIncomeRatio { get; set; }

        public bool FixedPriceOrder { get; set; }

        public int? SysWholeSellerId { get; set; }
        public int? PriceListTypeId { get; set; }

        public bool PrintTimeReport { get; set; }

        public bool ForceSave { get; set; }

        public string WorkingDescription { get; set; }
        public bool IncludeOnInvoice { get; set; }

        public bool IncludeOnlyInvoicedTime { get; set; }
        public bool CashSale { get; set; }
        public string BillingAdressText { get; set; }
        public string DeliveryDateText { get; set; }
        public bool AddAttachementsToEInvoice { get; set; }
        public bool AddSupplierInvoicesToEInvoices { get; set; }
        // Order planning
        public int? ShiftTypeId { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedStopDate { get; set; }
        public int EstimatedTime { get; set; }
        public int RemainingTime { get; set; }
        public int? Priority { get; set; }
        public bool KeepAsPlanned { get; set; }

        // Contract
        public int? ContractGroupId { get; set; }
        public int NextContractPeriodYear { get; set; }
        public int NextContractPeriodValue { get; set; }
        public DateTime? NextContractPeriodDate { get; set; }

        //Dims
        public int? Dim1AccountId { get; set; }
        public int? Dim2AccountId { get; set; }
        public int? Dim3AccountId { get; set; }
        public int? Dim4AccountId { get; set; }
        public int? Dim5AccountId { get; set; }
        public int? Dim6AccountId { get; set; }

        // Extended settings
        public bool CheckConflictsOnSave { get; set; }
    }

    public class CustomerLedgerSaveDTO
    {
        // Origin
        public int VoucherSeriesId { get; set; }
        public int? VoucherSeriesTypeId { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string OriginDescription { get; set; } // Internal text

        // Invoice
        public int InvoiceId { get; set; }
        public TermGroup_BillingType BillingType { get; set; }
        public TermGroup_InvoiceVatType VatType { get; set; }
        public int ActorId { get; set; }

        public int? SeqNr { get; set; }
        public string InvoiceNr { get; set; }
        public string OCR { get; set; }

        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? VoucherDate { get; set; }

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
        public decimal RemainingAmount { get; set; }

        public bool FullyPayed { get; set; }
        public int? SysPaymentTypeId { get; set; }
        public string PaymentNr { get; set; }
        public bool CashSale { get; set; }
        public int? VatCodeId { get; set; }

        // CustomerInvoice
        public int? PaymentConditionId { get; set; }
        public decimal SumAmount { get; set; }
        public decimal SumAmountCurrency { get; set; }
        public decimal FreightAmount { get; set; }
        public decimal FreightAmountCurrency { get; set; }
        public decimal InvoiceFee { get; set; }
        public decimal InvoiceFeeCurrency { get; set; }
        public decimal CentRounding { get; set; }

        public string InvoiceText { get; set; }

        public decimal MarginalIncome { get; set; }
        public decimal MarginalIncomeCurrency { get; set; }
        public decimal MarginalIncomeRatio { get; set; }
        public string InternalDescription { get; set; }
        public string ExternalId { get; set; }
    }
}
