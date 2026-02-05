using System;
using System.Collections.Generic;
using System.Diagnostics;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Common.Util.Logger;
using Newtonsoft.Json;
using TypeLite;
using SoftOne.Soe.Common.Attributes;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class CustomerInvoiceDTO : InvoiceDTO
    {
        public OrderInvoiceRegistrationType RegistrationType { get; set; }
        public TermGroup_OrderType OrderType { get; set; }

        // Keys
        public int OriginateFrom { get; set; }
        public int? PaymentConditionId { get; set; }
        public int? DeliveryTypeId { get; set; }
        public int? DeliveryConditionId { get; set; }
        public int DeliveryAddressId { get; set; }
        public int BillingAddressId { get; set; }
        public int? PriceListTypeId { get; set; }
        public int? SysWholeSellerId { get; set; }


        // Text
        public string InvoiceText { get; set; }
        public string InvoiceHeadText { get; set; }
        public string InvoiceLabel { get; set; }
        public string InternalDescription { get; set; }
        public string ExternalDescription { get; set; }
        public string BillingAdressText { get; set; }
        public string DeliveryDateText { get; set; }

        // Dates
        public DateTime? OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

        // Amounts
        public decimal CentRounding { get; set; }
        public decimal FreightAmount { get; set; }
        public decimal FreightAmountCurrency { get; set; }
        public decimal FreightAmountEntCurrency { get; set; }
        public decimal FreightAmountLedgerCurrency { get; set; }
        public decimal InvoiceFee { get; set; }
        public decimal InvoiceFeeCurrency { get; set; }
        public decimal InvoiceFeeEntCurrency { get; set; }
        public decimal InvoiceFeeLedgerCurrency { get; set; }
        public decimal SumAmount { get; set; }
        public decimal SumAmountCurrency { get; set; }
        public decimal SumAmountEntCurrency { get; set; }
        public decimal SumAmountLedgerCurrency { get; set; }
        public decimal MarginalIncome { get; set; }
        public decimal MarginalIncomeCurrency { get; set; }
        public decimal MarginalIncomeEntCurrency { get; set; }
        public decimal MarginalIncomeLedgerCurrency { get; set; }
        public decimal? MarginalIncomeRatio { get; set; }

        // Flags
        public int NoOfReminders { get; set; }

        public bool HasHouseholdTaxDeduction { get; set; }
        public bool FixedPriceOrder { get; set; }
        public bool MultipleAssetRows { get; set; }
        public bool InsecureDebt { get; set; }
        public bool PrintTimeReport { get; set; }
        public bool BillingInvoicePrinted { get; set; }
        public bool IncludeOnlyInvoicedTime { get; set; }
        public bool CashSale { get; set; }
        public bool HasManuallyDeletedTimeProjectRows { get; set; }
        public bool AddAttachementsToEInvoice { get; set; }
        public bool AddSupplierInvoicesToEInvoice { get; set; }

        // Contract
        public int? ContractGroupId { get; set; }
        public int NextContractPeriodYear { get; set; }
        public int NextContractPeriodValue { get; set; }
        public DateTime? NextContractPeriodDate { get; set; }

        // Extensions
        public string CustomerBlockNote { get; set; }
        public List<CustomerInvoiceRowDTO> CustomerInvoiceRows { get; set; }

        public string WorkingDescription { get; set; }
        public bool IncludeOnInvoice { get; set; }
        public string CustomerNameFromInvoice { get; set; }

        // Order planning
        public int? ShiftTypeId { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedStopDate { get; set; }
        public int EstimatedTime { get; set; }
        public int RemainingTime { get; set; }
        public int? Priority { get; set; }

        public bool? HasOrder { get; set; }
        public int? InvoiceDeliveryType { get; set; }
        public int? InvoicePaymentService { get; set; }
        public int? CustomerInvoicePaymentService { get; set; }

        public string ExternalId { get; set; }
    }

    [DebuggerDisplay("RowNr = {rowNr}")]
    [Log]
    [TSInclude]
    public class CustomerInvoiceRowDTO
    {
        // Keys
        public int CustomerInvoiceRowId { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public int? ParentRowId { get; set; }
        public int? TargetRowId { get; set; }
        public int? AttestStateId { get; set; }
        public int? ProductId { get; set; }
        public int? ProductUnitId { get; set; }
        public int? VatCodeId { get; set; }
        public int? VatAccountId { get; set; }
        public int? CustomerInvoiceInterestId { get; set; }
        public int? CustomerInvoiceReminderId { get; set; }
        public int? EdiEntryId { get; set; }
        public int? StockId { get; set; }
        public int? SupplierInvoiceId { get; set; }
        public int? HouseholdDeductionType { get; set; }
        public int? IntrastatTransactionId { get; set; }

        public int RowNr { get; set; }
        public SoeInvoiceRowType Type { get; set; }
        public SoeOriginType OriginType { get; set; }

        [JsonProperty(PropertyName = "_quantity")]
        public decimal? Quantity { get; set; }
        public decimal? InvoiceQuantity { get; set; }
        public decimal? PreviouslyInvoicedQuantity { get; set; }
        public string Text { get; set; }
        public string DeliveryDateText { get; set; }

        public string SysWholesellerName { get; set; }

        // Amounts
        public decimal Amount { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal AmountEntCurrency { get; set; }
        public decimal AmountLedgerCurrency { get; set; }
        public int DiscountType { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountAmountCurrency { get; set; }
        public decimal DiscountAmountEntCurrency { get; set; }
        public decimal DiscountAmountLedgerCurrency { get; set; }
        public decimal VatRate { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal VatAmountEntCurrency { get; set; }
        public decimal VatAmountLedgerCurrency { get; set; }
        public decimal SumAmount { get; set; }
        public decimal SumAmountCurrency { get; set; }
        public decimal SumAmountEntCurrency { get; set; }
        public decimal SumAmountLedgerCurrency { get; set; }
        public decimal PurchasePrice { get; set; }
        [JsonProperty(PropertyName = "_purchasePriceCurrency")]
        public decimal PurchasePriceCurrency { get; set; }
        public decimal PurchasePriceEntCurrency { get; set; }
        public decimal PurchasePriceLedgerCurrency { get; set; }
        public decimal MarginalIncome { get; set; }
        public decimal MarginalIncomeCurrency { get; set; }
        public decimal MarginalIncomeEntCurrency { get; set; }
        public decimal MarginalIncomeLedgerCurrency { get; set; }
        public decimal MarginalIncomeRatio { get; set; }

        // Flags
        public bool IsFreightAmountRow { get; set; }
        public bool IsInvoiceFeeRow { get; set; }
        public bool IsCentRoundingRow { get; set; }
        public bool IsInterestRow { get; set; }
        public bool IsReminderRow { get; set; }
        public bool IsTimeProjectRow { get; set; }
        public bool IsStockRow { get; set; }
        public bool IsHouseholdTextRow { get; set; }
        public bool TimeManuallyChanged { get; set; }
        public string TimeManuallyChangedText { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? DateTo { get; set; }

        // Extensions
        public int TempRowId { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public string ProductUnitCode { get; set; }
        public string VatCodeCode { get; set; }
        public string VatAccountNr { get; set; }
        public string VatAccountName { get; set; }
        public bool IsSupplementChargeProduct { get; set; }
        public bool IsManuallyAdjusted { get; set; }
        public bool IsLocked { get; set; }
        public bool HasMultipleSalesRows { get; set; }
        public int ProjectId { get; set; }
        public List<AccountingRowDTO> AccountingRows { get; set; }
        public bool IsLiftProduct { get; set; }
        public bool IsClearingProduct { get; set; }
        public bool IsContractProduct { get; set; }
        public bool IsFixedPriceProduct { get; set; }
        public string StockCode { get; set; }
        public int? SysCountryId { get; set; }
        public int? IntrastatCodeId { get; set; }

        // HouseholdTaxDeduction
        public string HouseholdProperty { get; set; }
        [LogSocSec]
        public string HouseholdSocialSecNbr { get; set; }
        public string HouseholdName { get; set; }
        public decimal HouseholdAmount { get; set; }
        public decimal HouseholdAmountCurrency { get; set; }
        public string HouseholdApartmentNbr { get; set; }
        public string HouseholdCooperativeOrgNbr { get; set; }
        public bool HouseholdApplied { get; set; }
        public DateTime? HouseholdAppliedDate { get; set; }
        public bool HouseholdReceived { get; set; }
        public DateTime? HouseholdReceivedDate { get; set; }
        public int HouseHoldTaxDeductionType { get; set; }

        // GUI properties
        public string AmountFormula { get; set; }
        public decimal SupplementCharge { get; set; }
        public decimal SupplementChargePercent { get; set; }
        public bool IsSelected { get; set; }
        public bool IsSelectDisabled { get; set; }
        public bool IsModified { get; set; }
        public bool DetailVisible { get; set; }
        public bool VatAccountEnabled { get; set; }
        public string CurrencyCode { get; set; }
        public string EdiTextValue { get; set; }
        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }
        public decimal MarginalIncomeLimit { get; set; }

        //Admin values
        public int RowState { get; set; }
        public string RowStateName { get; set; }
    }

    public class CustomerInvoiceSmallExDTO : CustomerInvoiceSmallDTO
    {
        public int CurrencyId { get; set; }
        public int? SysWholesellerId { get; set; }
        public int? DeliveryAddressId { get; set; }
        public int? VoucherHeadId { get; set; }
        public bool CashSale { get; set; }
        public int? InvoiceDeliveryType { get; set; }
        public int? InvoiceDelieryProvider { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public decimal CurrencyRate { get; set; }
        public int OriginType { get; set; }
    }

    public class CustomerInvoiceSmallDTO: InvoiceTinyDTO
    {
        public int? ActorId { get; set; }
        public int? ProjectId { get; set; }
        public int? PriceListTypeId { get; set; }
    }

    public class CustomerInvoiceActorDTO : CustomerInvoiceSmallExDTO
    {
        public string CustomerNr { get; set; }
        public string CustomerName { get; set; }
    }

    public class CustomerInvoiceRowSmallDTO
    {
        public int CustomerInvoiceRowId { get; set; }
        public int? AttestStateId { get; set; }
        public int? EdiEntryId { get; set; }
        public int? ProductId { get; set; }
        public int RowNr { get; set; }
        public string Text { get; set; }
        public int Type { get; set; }
        public string DeliveryDateText { get; set; }
        public decimal VatRate { get; set; }

        public decimal Quantity { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal VATAmountCurrency { get; set; }
        public decimal SumAmountCurrency { get; set; }
    }

    public class CustomerInvoiceRowDistributionDTO
    {
        public int Type { get; set; }
        public InvoiceProductDistributionDTO Product { get; set; }
        public string Text { get; set; }
        public string Unit { get; set; }
        public decimal Quantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal VatRate { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal SumVATAmountCurrency { get; set; }
        public decimal SumAmountCurrency { get; set; }
        public decimal DiscountPercent { get; set; }
        public HouseholdTaxDeductionRowDTO HousholdDeductionRow { get; set; }

        public CustomerInvoiceRowDistributionDTO()
        {
        }
        public CustomerInvoiceRowDistributionDTO(string text) //TextRow
        {
            Product = null;
            Text = text;
        }

        public decimal VATAmountCurrency { 
            get {
                return this.Quantity == 0 ? 0 : this.SumVATAmountCurrency / this.Quantity;
            } 
        }
        public bool IsTextRow { 
            get {
                //For now. Might want to control the SoeInvoiceRowType
                return this.Product == null || Product.Number == null;
            } 
        }
        public decimal MarginalIncome { 
            get {
                return this.AmountCurrency - this.PurchasePrice;
            } 
        }
        public decimal MarginalIncomeRatio { 
            get {
                return this.PurchasePrice == 0 ? 0 : this.MarginalIncome / this.PurchasePrice;
            } 
        }
        public decimal PurchaseCost
        {
            get {
                return this.PurchasePrice * this.Quantity;
            }
        }
    }

    public class InvoiceProductDistributionDTO
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public int VatType { get; set; }
        public int HouseholdDeductionType { get; set; }
        public bool IsPayrollProduct
        {
            get
            {
                return this.VatType == (int)SoeProductType.PayrollProduct;
            }
        }
        public bool IsInvoiceProduct
        {
            get
            {
                return this.VatType == (int)SoeProductType.InvoiceProduct;
            }
        }
        public DateTime? Created { get; set; }
        public DateTime? Modified { get; set; }
        public DateTime LastUpdated() => Modified ?? Created ?? DateTime.MinValue;
    }

    [TSInclude]
    public class CustomerInvoiceRowDetailDTO
    {
        // Keys
        public int CustomerInvoiceRowId { get; set; }
        public int InvoiceId { get; set; }
        public int? AttestStateId { get; set; }
        public int? EdiEntryId { get; set; }
        public int? ProductId { get; set; }
        public int RowNr { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public SoeInvoiceRowType Type { get; set; }
        public string EdiTextValue { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public string Text { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? PreviouslyInvoicedQuantity { get; set; }
        public string ProductUnitCode { get; set; }
        public decimal AmountCurrency { get; set; }
        public decimal DiscountValue { get; set; }
        public string CurrencyCode { get; set; }
        public decimal SumAmountCurrency { get; set; }
        public decimal MarginalIncomeLimit { get; set; }
        public int DiscountType { get; set; }
        public string AttestStateName { get; set; }
        public string AttestStateColor { get; set; }
        public bool IsTimeProjectRow { get; set; }
        public bool IsExpenseRow { get; set; }
        public bool IsTimeBillingRow { get; set; }

    }

    public class CustomerInvoiceAmountDTO: InvoiceTinyDTO
    {
        public int ActorId { get; set; }
        public string ActorName { get; set; }
        public string ActorNr { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        public decimal PaidAmountCurrency { get; set; }
        public bool FullyPayed { get; set; }
        public int CurrencyId { get; set; }
    }

    public class CustomerInvoiceDistributionDTO : CustomerInvoiceAmountDTO
    {
        public string OCR { get; set; }
        public string ActorOrgNr { get; set; }
        public string ActorVatNr { get; set; }
        public string ActorSupplierNr { get; set; }
        public int SysCurrencyId { get; set; }
        public int VatType { get; set; }
        public int? ActorSysCountryId { get; set; }
        public int? ActorSysLanguageId { get; set; }
        public bool IncludeInvoicedTime { get; set; }
        public string CurrencyCode { get; set; }
        public int? ContactEComId { get; set; }
        public int? InvoiceDeliveryType { get; set; }
        public string ReferenceYour { get; set; }
        public string ReferenceOur { get; set; }
        public string WorkingDescription { get; set; }
        public bool ShowWorkingDescription { get; set; }
        public string InternalDescription { get; set; }
        public int? InvoiceDeliveryProvider { get; set; }
        public int BillingType { get; set; }
        public int? BillingAddressId { get; set; }
        public string InvoiceText { get; set; }
        public string InvoiceHeadText { get; set; }
        public string InvoiceLabel { get; set; }
        public string PaymentConditionCode { get; set; }
        public int PaymentConditionDays { get; set; }
        public decimal Freight { get; set; }
        public decimal InvoiceFee { get; set; }
        public int ExportStatus { get; set; }

        public bool IsEUCountryBased { get; set; }
        public string ActorCountryCode { get; set; }


        public bool IsContractorVat()
        {
            return this.VatType == (int)TermGroup_InvoiceVatType.Contractor;
        }
    }

    public class CustomerInvoiceSmallDialogDTO
    {
        public int CustomerInvoiceId { get; set; }
        public int SeqNr { get; set; }
        public string InvoiceNr { get; set; }
        public int ActorCustomerId { get; set; }
        public string ActorCustomerName { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        public decimal RemainingAmount { get; set; }
        public int SysCurrencyId { get; set; }
        public string CurrencyCode { get; set; }
    }
    public class CustomerInvoiceDistributionResultDTO
    {
        public ActionResult PermissionCheck { get; set; }
        public ActionResult EInvoiceResult { get; set; }
        public ActionResult EmailResult { get; set; }
        public ActionResult PrintResult { get; set; }
        public int UnhandledCount { get; set; }
        public int EmailedCount { get; set; }
        public int EInvoicedCount { get; set; }
        public int PrintedCount { get; set; }
    }
    public class CustomerInvoiceSummaryDTO
    {
        public int CustomerInvoiceId { get; set; }
        public decimal CostMaterial { get; set; }
        public decimal IncomeMaterial { get; set; }
        public decimal MarginalIncomeMaterial { get; set; }
        public decimal MarginalIncomeRatioMaterial { get; set; }
        public decimal CostPersonell { get; set; }
        public decimal IncomePersonell { get; set; }
        public decimal MarginalIncomePersonell { get; set; }
        public decimal MarginalIncomeRatioPersonell { get; set; }
        public decimal CostTotal { get; set; }
        public decimal IncomeTotal { get; set; }
        public decimal MarginalIncomeTotal { get; set; }
        public decimal MarginalIncomeRatioTotal { get; set; }

        public decimal WorkedMinutes { get; set; }
        public decimal BillableMinutes { get; set; }
    }
    [DebuggerDisplay("InvoiceNr = {InvoiceNr}")]
    public class CustomerInvoiceAnalysDTO : CustomerInvoiceGridDTO
    {
        public int? ContactId { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public int? PriceListTypeId { get; set; }

        public DateTime? OrderDate { get; set; }
        public DateTime? Modified { get; set; }
        public int VATTypeId { get; set; }
        public string VATType { get; set; }
        public int OriginUserCount { get; set; }
        public int OriginReadyUserCount { get; set; }
        public int OrderReadyStatePercent { get; set; }

    }

    public class CustomerInvoiceAccountRowTinyDTO
    {
        public int AccountId { get; set; }
        public bool VatRow { get; set; }
        public int CustomerInvoiceRowId { get; set; }
        public List<int> AccountInternal { get; set; }
    }

    public class CustomerInvoiceSequenceDTO
    {
        public int SeqNr { get; set; }
        public string EntityName { get; set; }
        public int NumberLength { get; set; }
    }

    #region Grid
    [TSInclude]
    public class CustomerInvoiceGridDTO
    {
        public int CustomerInvoiceId { get; set; }
        public int OriginType { get; set; }
        public int DeliveryType { get; set; }
        public string DeliveryTypeName { get; set; }
        public int InvoiceDeliveryProvider { get; set; }
        public string InvoiceDeliveryProviderName { get; set; }
        public int SeqNr { get; set; }
        public string InvoiceNr { get; set; }
        public string OCR { get; set; }
        public int CustomerPaymentId { get; set; }
        public int CustomerPaymentRowId { get; set; }
        public int PaymentSeqNr { get; set; }
        public string PaymentNr { get; set; }
        public int BillingTypeId { get; set; }
        public string BillingTypeName { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public int StatusIcon { get; set; }
        public int ExportStatus { get; set; }
        public string ExportStatusName { get; set; }
        public int ActorCustomerId { get; set; }
        public string ActorCustomerNr { get; set; }
        public string ActorCustomerName { get; set; }
        public string ActorCustomerNrName { get; set; }
        public string InternalText { get; set; }
        public string WorkDescription { get; set; }
        public string InvoiceLabel { get; set; }
        public int InvoicePaymentServiceId { get; set; }
        public string InvoicePaymentServiceName { get; set; }
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
        public string PaidAmountText { get; set; }
        public decimal PaidAmountCurrency { get; set; }
        public string PaidAmountCurrencyText { get; set; }
        public decimal RemainingAmount { get; set; }
        public string RemainingAmountText { get; set; }
        public decimal RemainingAmountExVat { get; set; }
        public string RemainingAmountExVatText { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal PaymentAmountCurrency { get; set; }
        public decimal PaymentAmountDiff { get; set; }
        public decimal ContractYearlyValue { get; set; }
        public decimal ContractYearlyValueExVat { get; set; }
        public decimal BankFee { get; set; }
        public decimal VatRate { get; set; }
        public int SysCurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrencyRate { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? PayDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public int OwnerActorId { get; set; }
        public bool FullyPaid { get; set; }
        public bool IsTotalAmountPaid { get; set; }
        public string InvoiceHeadText { get; set; }
        public int RegistrationType { get; set; }
        public int DeliveryAddressId { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryPostalCode { get; set; }
        public int BillingAddressId { get; set; }
        public string BillingAddress { get; set; }
        public int? ContactEComId { get; set; }
        public string ContactEComText { get; set; }
        public int? ReminderContactEComId { get; set; }
        public string ReminderContactEComText { get; set; }
        public bool BillingInvoicePrinted { get; set; }
        public bool HasHouseholdTaxDeduction { get; set; }
        public int HouseholdTaxDeductionType { get; set; }
        public bool HasVoucher { get; set; }
        public bool InsecureDebt { get; set; }
        public bool MultipleAssetRows { get; set; }
        public int NoOfReminders { get; set; }
        public int NoOfPrintedReminders { get; set; }
        public DateTime? LastCreatedReminder { get; set; }
        public string Categories { get; set; }
        public string CustomerCategories { get; set; }
        public string DeliverDateText { get; set; }
        public string OrderNumbers { get; set; }
        public int CustomerGracePeriodDays { get; set; }
        public string Users { get; set; }
        public string ProjectNr { get; set; }
        public string AttestStateNames { get; set; }
        public string ShiftTypeName { get; set; }
        public string ShiftTypeColor { get; set; }
        public string FixedPriceOrderName { get; set; }
        public bool FixedPriceOrder { get; set; }
        public int OrderType { get; set; }
        public string OrderTypeName { get; set; }
        public List<CustomerInvoiceRowAttestStateViewDTO> AttestStates { get; set; }
        public bool OnlyPayment { get; set; }
        public string NextContractPeriod { get; set; }
        public string ContractGroupName { get; set; }
        public DateTime? NextInvoiceDate { get; set; }
        public int? DefaultDim2AccountId { get; set; }
        public int? DefaultDim3AccountId { get; set; }
        public int? DefaultDim4AccountId { get; set; }
        public int? DefaultDim5AccountId { get; set; }
        public int? DefaultDim6AccountId { get; set; }
        public string DefaultDim2AccountName { get; set; }
        public string DefaultDim3AccountName { get; set; }
        public string DefaultDim4AccountName { get; set; }
        public string DefaultDim5AccountName { get; set; }
        public string DefaultDim6AccountName { get; set; }
        public string DefaultDimAccountNames { get; set; }
        public string ReferenceOur { get; set; }
        public string ReferenceYour { get; set; }
        public string MainUserName { get; set; }
        public string PriceListName { get; set; }
        public string ProjectName { get; set; }
        public int MyReadyState { get; set; }
        public int orderReadyStatePercent { get; set; }
        public string orderReadyStateText { get; set; }
        public string ExternalInvoiceNr { get; set; }
        public short EinvoiceDistStatus { get; set; }
        public int MyOriginUserStatus { get; set; }
        //GUI Properties
        public bool UseClosedStyle { get; set; }
        public bool IsSelectDisabled { get; set; }
        public bool IsOverdued { get; set; }
        public bool HasInterest { get; set; }
        public bool isCashSales { get; set; }
        public string isCashSalesText { get; set; }
        public Guid Guid { get; set; }
        public int InfoIcon { get; set; }
        public DateTime? Created { get; set; }
        public string MappedContractNr { get; set; }
    }

    [TSInclude]
    public class CustomerInvoiceSmallGridDTO
    {
        public int InvoiceId { get; set; }
        public int ProjectId { get; set; }
        public string InvoiceNr { get; set; }
        public string Customer { get; set; }
        public string CustomerInvoiceNumberName { get; set; }
        public string CustomerInvoiceNumberNameWithoutDescription { get; set; }
        public int PriceListTypeId { get; set; }
    }

    #endregion

    #region Search
    public class CustomerInvoiceSearchParamsDTO
    {
        public string Number { get; set; }
        public string ExternalNr { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerName { get; set; }
        public string InternalText { get; set; }
        public string ProjectNr { get; set; }
        public string ProjectName { get; set; }

        public int OriginType { get; set; }
        public int? CustomerId { get; set; }
        public int? ProjectId { get; set; }
        public int? UserId { get; set; }
        public int? IgnoreInvoiceId { get; set; }
        public int DeliveryAddressId { get; set; }
        public bool IgnoreChildren { get; set; }
        public bool? FullyPaid { get; set; }
        public DateTime? InvoiceDateFrom { get; set; }
        public DateTime? InvoiceDateTo { get; set; }
        public bool IncludePreliminary { get; set; }
        public bool IncludeClosed { get; set; }
        public bool IncludeVoucher { get; set; }
        public string InvoiceHeadText { get; set; }
    }

    [TSInclude]
    public class CustomerInvoiceSearchDTO
    {
        public int CustomerInvoiceId { get; set; }
        public int OriginType { get; set; }
        public string Number { get; set; }
        public string ExternalNr { get; set; }
        public string InternalText { get; set; }
        public int? CustomerId { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerName { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectNr { get; set; }
        public string ProjectName { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public int BillingType { get; set; }
        public int? SysCurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public int DeliveryAddressId { get; set; }
        public string InvoiceHeadText { get; set; }
    }

    [TSInclude]
    public class AgreementDetaílsOrderDTO
    {
        public int CustomerInvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public string InternalText { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountExVat { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
    }

    #endregion

}
