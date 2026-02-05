using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class ChangeStatusGridViewBalanceDTO
    {
        public SoeOriginStatusClassification Classification { get; set; }
        public int Count { get; set; }
        public decimal BalanceTotal { get; set; }
        public decimal BalanceExVat { get; set; }

        public ChangeStatusGridViewBalanceDTO()
        {
        }

        public ChangeStatusGridViewBalanceDTO(SoeOriginStatusClassification classification, int count, decimal balance, decimal balanceExVat)
        {
            Classification = classification;
            Count = count;
            BalanceTotal = balance;
            BalanceExVat = balanceExVat;
        }
    }

    public class ChangeStatusGridViewDTO
    {
        #region Columns from View

        //Company
        public int OwnerActorId { get; set; }
        //Actor
        public int ActorId { get; set; }
        public string ActorNr { get; set; }
        public string ActorName { get; set; }
        //Supplier
        public bool SupplierBlockPayment { get; set; }
        //Customer
        public int CustomerGracePeriodDays { get; set; }
        public int? DefaultBillingReportTemplate { get; set; }
        public int CustomerPriceListTypeId { get; set; }
        //Origin
        public int OriginId { get; set; }
        public int OriginType { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string InternalText { get; set; }
        //Invoice
        public int InvoiceId { get; set; }
        public int InvoiceType { get; set; }
        public string InvoiceNr { get; set; }
        public int? InvoiceSeqNr { get; set; }
        public int BillingTypeId { get; set; }
        public string BillingTypeName { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal RemainingAmountExVat { get; set; }
        public SoeStatusIcon StatusIcon { get; set; }
        public string ReferenceYour { get; set; }
        //Supplier invoice
        public bool BlockPayment { get; set; }
        public bool MultipleDebtRows { get; set; }
        public int? SupplierInvoiceAttestStateId { get; set; }
        public int? SupplierInvoicePaymentMethodId { get; set; }
        public int SupplierInvoicePaymentMethodType { get; set; }
        public DateTime? TimeDiscountDate { get; set; }
        public decimal? TimeDiscountPercent { get; set; }
        public string CurrentAttestUsers { get; set; }
        //Customer invoice
        public string InvoiceHeadText { get; set; }
        public int RegistrationType { get; set; }
        public int DeliveryAddressId { get; set; }
        public int BillingAddressId { get; set; }
        public bool BillingInvoicePrinted { get; set; }
        public bool HasHouseholdTaxDeduction { get; set; }
        public bool InsecureDebt { get; set; }
        public bool MultipleAssetRows { get; set; }
        public int NoOfReminders { get; set; }
        public int NoOfPrintedReminders { get; set; }
        public string Categories { get; set; }
        public string DeliverDateText { get; set; }
        public string InvoicePaymentServiceName { get; set; }
        //Shift Type
        public string ShiftTypeName { get; set; }
        public string ShiftTypeColor { get; set; }
        //Contract
        public string ContractGroupName { get; set; }
        public int ContractPeriod { get; set; }
        public int ContractInterval { get; set; }
        public DateTime? NextContractPeriodDate { get; set; }
        public int NextContractPeriodYear { get; set; }
        public int NextContractPeriodValue { get; set; }
        //Payment method
        public int SysPaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; }
        //Payment
        public int PaymentRowId { get; set; }
        public string PaymentNr { get; set; }
        public string PaymentNrString { get; set; }
        public int PaymentSeqNr { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal PaymentAmountCurrency { get; set; }
        public decimal PaymentAmountDiff { get; set; }
        public decimal BankFee { get; set; }
        //Sequence number
        public int SequenceNumber { get; set; }
        public int SequenceNumberRecordId { get; set; }
        //Supplier payment
        public bool PaymentIsSuggestion { get; set; }
        //Customer payment
        //Common
        public string CurrencyCode { get; set; }
        public decimal CurrencyRate { get; set; }
        public int SysCurrencyId { get; set; }
        public int AccountYearId { get; set; }
        public int VoucherSeriesId { get; set; }
        public bool HasVoucher { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? VoucherDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? PayDate { get; set; }
        public decimal InvoiceTotalAmount { get; set; }
        public decimal InvoiceTotalAmountCurrency { get; set; }
        public decimal InvoiceTotalAmountExVat { get; set; }
        public decimal InvoiceTotalAmountExVatCurrency { get; set; }
        public decimal InvoiceSumAmount { get; set; }
        public decimal InvoiceSumAmountCurrency { get; set; }
        public decimal InvoiceSumAmountExVat { get; set; }
        public decimal InvoiceSumAmountCurrencyExVat { get; set; }
        public int VatType { get; set; }
        public decimal VATAmount { get; set; }
        public decimal VATAmountCurrency { get; set; }
        public decimal InvoicePaidAmount { get; set; }
        public decimal InvoicePaidAmountCurrency { get; set; }
        public decimal InvoicePayAmount { get; set; }
        public decimal InvoicePayAmountCurrency { get; set; }
        public bool FullyPayed { get; set; }
        public string PaymentStatuses { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string ProjectNumber { get; set; }
        public string OrderNumbers { get; set; }
        public int? DeliveryType { get; set; }
        public int ExportStatus { get; set; }
        public int ProjectId { get; set; }
        public string FixedPriceOrderName { get; set; }
        public OrderContractType FixedPriceOrder { get; set; }
        public int? OrderNr { get; set; }

        #endregion
    }
}
