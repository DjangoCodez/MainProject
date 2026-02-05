using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using SoftOne.Soe.Common.DTO.ApiExternal;
using SoftOne.Soe.Common.Attributes;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class CustomerInvoiceIODTO
    {
        public List<CustomerInvoiceRowIODTO> InvoiceRows = new List<CustomerInvoiceRowIODTO>();

        #region Properties

        public int CustomerInvoiceHeadIOId { get; set; }
        public int? InvoiceId { get; set; }
        public string CustomerInvoiceNr { get; set; }
        public int? SeqNr { get; set; }
        public string OCR { get; set; }
        public int ActorCompanyId { get; set; }
        public bool Import { get; set; }
        public int Type { get; set; }
        public int Status { get; set; }
        public int Source { get; set; }
        public int OriginType { get; set; }
        public int OriginStatus { get; set; }
        public string BatchId { get; set; }

        public int? PriceListTypeId { get; set; }
        public string PaymentCondition { get; set; }
        public int? PaymentConditionId { get; set; }
        public string DeliveryCondition { get; set; }
        public int? DeliveryConditionId { get; set; }
        public string DeliveryType { get; set; }
        public int? DeliveryTypeId { get; set; }
        public bool? AutoCreateCustomer { get; set; }

        public int? CustomerId { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerExternalNr { get; set; }
        public string CustomerName { get; set; }
        public string CustomerOrgnr { get; set; }
        public int RegistrationType { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? VoucherDate { get; set; }
        public string ReferenceOur { get; set; }
        public string ReferenceYour { get; set; }
        public DateTime? OrderDate { get; set; }
        public decimal? CurrencyRate { get; set; }
        public DateTime? CurrencyDate { get; set; }
        public int CurrencyId { get; set; }
        public decimal? SumAmount { get; set; }
        public decimal? SumAmountCurrency { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalAmountCurrency { get; set; }
        public decimal? VATAmount { get; set; }
        public decimal? VATAmountCurrency { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? PaidAmountCurrency { get; set; }
        public decimal? RemainingAmount { get; set; }
        public decimal? FreightAmount { get; set; }
        public decimal? FreightAmountCurrency { get; set; }
        public decimal? InvoiceFee { get; set; }
        public decimal? InvoiceFeeCurrency { get; set; }
        public decimal? CentRounding { get; set; }
        public bool? FullyPayed { get; set; }
        public string PaymentNr { get; set; }
        public string VoucherNr { get; set; }
        public bool? CreateAccountingInXE { get; set; }
        public string Note { get; set; }
        public int? BillingType { get; set; }
        public string Currency { get; set; }
        public string TransferType { get; set; }
        public string ErrorMessage { get; set; }
        public int ImportHeadType { get; set; }
        public string BillingAddressAddress { get; set; }
        public string BillingAddressCO { get; set; }
        public string BillingAddressPostNr { get; set; }
        public string BillingAddressCity { get; set; }
        public string DeliveryAddressAddress { get; set; }
        public string DeliveryAddressPostNr { get; set; }
        public string DeliveryAddressCity { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public bool? UseFixedPriceArticle { get; set; }

        public decimal? VatRate1 { get; set; }
        public decimal? VatRate2 { get; set; }
        public decimal? VatRate3 { get; set; }

        public decimal? VatAmount1 { get; set; }
        public decimal? VatAmount2 { get; set; }
        public decimal? VatAmount3 { get; set; }

        public string BillingAddressCountry { get; set; }
        public string DeliveryAddressCO { get; set; }
        public string DeliveryAddressCountry { get; set; }
        public int InvoiceState { get; set; }
        public int VatType { get; set; }
        public string BillingAddressName { get; set; }
        public string DeliveryAddressName { get; set; }
        public string Language { get; set; }
        public string PaymentConditionCode { get; set; }
        public string SaleAccountNr { get; set; }
        public string SaleAccountNrDim2 { get; set; }
        public string SaleAccountNrDim3 { get; set; }
        public string SaleAccountNrDim4 { get; set; }
        public string SaleAccountNrDim5 { get; set; }
        public string SaleAccountNrDim6 { get; set; }
        public string SaleAccountNrSieDim1 { get; set; }
        public string SaleAccountNrSieDim6 { get; set; }
        public string ClaimAccountNr { get; set; }
        public string ClaimAccountNrDim2 { get; set; }
        public string ClaimAccountNrDim3 { get; set; }
        public string ClaimAccountNrDim4 { get; set; }
        public string ClaimAccountNrDim5 { get; set; }
        public string ClaimAccountNrDim6 { get; set; }
        public string ClaimAccountNrSieDim1 { get; set; }
        public string ClaimAccountNrSieDim6 { get; set; }
        public string VatAccountNr { get; set; }
        public string OfferNr { get; set; }
        public string OrderNr { get; set; }
        public string ContractNr { get; set; }
        public string WorkingDescription { get; set; }
        public string InternalDescription { get; set; }
        public string ExternalDescription { get; set; }
        public string ProjectNr { get; set; }
        public int ProjectId { get; set; }
        public int? NextContractPeriodYear { get; set; }
        public int? NextContractPeriodValue { get; set; }
        public DateTime? NextContractPeriodDate { get; set; }
        public int? ContractGroupId { get; set; }
        public int? ContractGroupInterval { get; set; }
        public int? ContractGroupDayInMonth { get; set; }
        public int? ContractGroupOrderTemplate { get; set; }
        public int? ContractGroupInvoiceTemplate { get; set; }
        public string ContractGroupDecription { get; set; }
        public string ContractGroupName { get; set; }
        public string ContractGroupPeriod { get; set; }
        public string ContractGroupPriceManagementName { get; set; }
        public string ContractGroupInvoiceText { get; set; }
        public string ContractGroupInvoiceRowText { get; set; }

        public int ContractEndYear { get; set; }
        public int ContractEndDay { get; set; }
        public int ContractEndMonth { get; set; }

        public string InvoiceLabel { get; set; }
        public string InvoiceHeadText { get; set; }
        public bool? CreateDeliveryAddressAsTextOnly { get; set; }
        public string ExternalId { get; set; }

        public string Email { get; set; }
        public string CustomerGlnNr { get; set; }
        public int? InvoiceDeliveryType { get; set; }
        public int? OrderType { get; set; }

        public string CustomerContractNr { get; set; }

        #endregion

        #region Partial comp

        public string StatusName { get; set; }
        public string BillingTypeName { get; set; }

        public List<int> AttestStates { get; set; }
        public List<string> AttestStateNames { get; set; }

        public string DebetInvoiceNr { get; set; }

        #endregion

        #region Extensions

        public int NrOfProductRows { get; set; }
        public bool IsSelected { get; set; }
        public bool IsModified { get; set; }
        public bool IsClosed { get; set; }

        #endregion
    }
    [TSInclude]
    public class CustomerInvoiceRowIODTO
    {
        #region Propertys

        public int CustomerInvoiceRowIOId { get; set; }
        public int? CustomerInvoiceHeadIOId { get; set; }
        public int InvoiceId { get; set; }
        public int InvoiceRowId { get; set; }
        public string InvoiceNr { get; set; }
        public int ActorCompanyId { get; set; }
        public bool Import { get; set; }
        public TermGroup_IOType Type { get; set; }
        public int Status { get; set; }
        public int Source { get; set; }
        public string BatchId { get; set; }
        public SoeInvoiceRowType CustomerRowType { get; set; }
        public string ProductNr { get; set; }
        public int? ProductId { get; set; }
        public int? ProductGroupId { get; set; }
        public string ProductName { get; set; }
        public string ProductName2 { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Discount { get; set; }
        public string Unit { get; set; }
        public int? ProductUnitId { get; set; }
        public decimal? VatRate { get; set; }

        public string AccountNr { get; set; }
        public string AccountDim2Nr { get; set; }
        public string AccountDim3Nr { get; set; }
        public string AccountDim4Nr { get; set; }
        public string AccountDim5Nr { get; set; }
        public string AccountDim6Nr { get; set; }
        public string AccountSieDim1 { get; set; }
        public string AccountSieDim6 { get; set; }

        public decimal? PurchasePrice { get; set; }
        public decimal? PurchasePriceCurrency { get; set; }
        public decimal? Amount { get; set; }
        public decimal? AmountCurrency { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal? VatAmountCurrency { get; set; }
        public decimal? DiscountAmount { get; set; }
        public decimal? DiscountAmountCurrency { get; set; }
        public decimal? MarginalIncome { get; set; }
        public decimal? MarginalIncomeCurrency { get; set; }
        public decimal? SumAmount { get; set; }
        public decimal? SumAmountCurrency { get; set; }

        public string Text { get; set; }
        public string ErrorMessage { get; set; }
        public int ImportHeadType { get; set; }
        public int? RowNr { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public int State { get; set; }
        public int? StockId { get; set; }
        public string RowStatus { get; set; }
        public string VatCode { get; set; }

        public int VatCodeId { get; set; }
        public decimal InvoiceQuantity { get; set; }
        public decimal PreviouslyInvoicedQuantity { get; set; }
        public string Stock { get; set; }
        public DateTime RowDate { get; set; }
        public string DeliveryDateText { get; set; }
        public string ClaimAccountNr { get; set; }
        public string ClaimAccountNrDim2 { get; set; }
        public string ClaimAccountNrDim3 { get; set; }
        public string ClaimAccountNrDim4 { get; set; }
        public string ClaimAccountNrDim5 { get; set; }
        public string ClaimAccountNrDim6 { get; set; }
        public string ClaimAccountNrSieDim1 { get; set; }
        public string ClaimAccountNrSieDim6 { get; set; }
        public string VatAccountnr { get; set; }

        public string ExternalId { get; set; }

        public List<int> CategoryIds { get; set; }

        #endregion

        #region Partial comp

        public string StatusName { get; set; }

        public int AttestState { get; set; }
        public string AttestStateName { get; set; }

        #endregion

        #region Extensions

        public bool IsSelected { get; set; }
        public bool IsModified { get; set; }
        public bool IsReadonly { get; set; }

        #endregion
    }

    public class CustomerInvoiceSmallIODTO
    {
        public List<CustomerInvoiceRowSmallIODTO> InvoiceRows = new List<CustomerInvoiceRowSmallIODTO>();
        public int InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerName { get; set; }
        public string CustomerOrgNr { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public int? BillingType { get; set; }
        public string Currency { get; set; }
        public int OriginStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        public decimal VATAmount { get; set; }
        public decimal VATAmountCurrency { get; set; }
    }

    public class CustomerInvoiceRowSmallIODTO
    {
        public string ProductNr { get; set; }
        public decimal Quantity { get; set; }
        public string Text { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? UnitPriceCurrency { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal? VatAmountCurrency { get; set; }
        public decimal? SumAmount { get; set; }
        public decimal? SumAmountCurrency { get; set; }
        public List<AccountInfoIODTO> Accounts { get; set; }

        public string AttestStateName { get; set; }
        public int? AttestState { get; set; }
    }

    #region Filter
    public class CustomerInvoiceFilterIODTO
    {
        public int? InvoiceId { get; set; }
        public string Number { get; set; }

        public DateTime? InvoiceDateFrom { get; set; }
        public DateTime? InvoiceDateTo { get; set; }

        public bool IncludePreliminary { get; set; }
        public bool IncludeVoucher { get; set; }

        public int PageNumber { get; set; }
        public int PageNrOfRecords { get; set; }

        public bool IncludeRowAccountInfo { get; set; }
    }

    #endregion


    #region Search
    public class CustomerInvoiceSearchIODTO
    {
        public string Number { get; set; }

        public string CustomerNr { get; set; }
        public DateTime? InvoiceDateFrom { get; set; }
        public DateTime? InvoiceDateTo { get; set; }
        public bool? FullyPaid { get; set; }
        public bool? IncludePreliminary { get; set; }
        public bool? IncludeVoucherStatus { get; set; }
    }

    public class CustomerInvoiceSearchResultIODTO
    {
        public int CustomerInvoiceId { get; set; }
        public string Number { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerName { get; set; }
        public int BillingType { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal PaidAmount { get; set; }
    }

    public class CustomerOrderSearchIODTO
    {
        public string Number { get; set; }

        public string CustomerNr { get; set; }
        public DateTime? OrderDateFrom { get; set; }
        public DateTime? OrderDateTo { get; set; }
        public bool? IncludeClosed { get; set; }
        public int PageNumber { get; set; }
        public int PageNrOfRecords { get; set; }
    }

    public class CustomerOrderSearchResultIODTO
    {
        public int CustomerInvoiceId { get; set; }
        public string Number { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerName { get; set; }

        public DateTime? OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal VatAmount { get; set; }
    }

    #endregion

    #region Update
    public class CustomerInvoiceOrderUpdateIODTO
    {
        public int InvoiceId { get; set; }
        public string Number { get; set; }
        public DateTime? Date { get; set; }
        public string ReferenceOur { get; set; }
        public string ReferenceYour { get; set; }
        public ContactAdressIODTO DeliveryAddress { get; set; }
        public List<CustomerInvoiceOrderRowUpdateIODTO> Rows { get; set; }
    }

    public class CustomerInvoiceOrderRowUpdateIODTO
    {
        public int InvoiceRowId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Price { get; set; }
        public string ProductNr { get; set; }
        public bool? Delete { get; set; }
        public string Text { get; set; }
        public bool? IsReady { get; set; }
    }

    #endregion
}
