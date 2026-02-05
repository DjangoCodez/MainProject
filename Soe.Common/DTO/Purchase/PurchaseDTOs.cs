using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class PurchaseGridDTO
    {
        public int PurchaseId { get; set; }
        public string PurchaseNr { get; set; }
        public string SupplierNr { get; set; }
        public string SupplierName { get; set; }
        public string ProjectNr { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string StatusName { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public PurchaseDeliveryStatus DeliveryStatus { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalAmountExVat { get; set; }
        public decimal TotalAmountExVatCurrency { get; set; }
        public string CurrencyCode { get; set; }
        public int SysCurrencyId { get; set; }
        public string Origindescription { get; set; }
        public int StatusIcon { get; set; }
    }

    [TSInclude]
    public class PurchaseDeliveryInvoiceSaveDTO
    {
        public bool Disconnect { get; set; }
        public List<PurchaseDeliveryInvoiceDTO> Rows { get; set; }
    }

    [TSInclude]
    public class PurchaseDeliveryInvoiceDTO
    {
        public int PurchaseDeliveryInvoiceId { get; set; }
        public int? SupplierinvoiceId { get; set; }
        public int? SupplierInvoiceSeqNr { get; set; }
        
        public int PurchaseRowId { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }

        public int PurchaseRowNr { get; set; }
        public decimal? DeliveredQuantity { get; set; }
        public decimal AskedPrice { get; set; }
        public string Text { get; set; }
        public string PurchaseRowDisplayName { get; set; }
        
        public int PurchaseId { get; set; }
        public string PurchaseNr { get; set; }
        public decimal? PurchaseQuantity { get; set; }

        public int? SupplierProductId { get; set; }
        public string SupplierProductNr { get; set; }
        public string SupplierProductName { get; set; }

        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductNumber { get; set; }

        public bool IsDeleted { get; set; }
        public bool LinkToInvoice { get; set; }
    }
    [TSInclude]
    public class PurchaseSmallDTO
    {
        public int PurchaseId { get; set; }
        public string PurchaseNr { get; set; }
        public string SupplierNr { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string OriginDescription { get; set; }
        public int Status { get; set; }
        public string DisplayName { get; set; }
        public string Name { 
            get {
                return $"{PurchaseNr} ({SupplierNr} - {SupplierName})";
            }
        }
        public void SetDisplayNameWithSupplier(string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                DisplayName = Name;
            }
            else
            {
                DisplayName = $"{PurchaseNr} ({SupplierNr} - {SupplierName} - {status})";
            }
        }
        public void SetDisplayNameWithDescription(string status)
        {
            status = status != null ? $"- {status}" : "";
            OriginDescription = OriginDescription.Length > 20 ? $"{OriginDescription.Substring(0, 20)}..." : $"{OriginDescription}";
            if (!string.IsNullOrEmpty(OriginDescription)) OriginDescription = $"({OriginDescription})";
            DisplayName = $"{PurchaseNr} {status} {OriginDescription}";
        }
    }
    [TSInclude]
    public class PurchaseDTO
    {
        public int PurchaseId { get; set; }
        public string PurchaseNr { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string ReferenceOur { get; set; }
        public string ReferenceYour { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierEmail { get; set; }
        public string SupplierCustomerNr { get; set; }
        public int? DefaultDim1AccountId { get; set; }
        public int? DefaultDim2AccountId { get; set; }
        public int? DefaultDim3AccountId { get; set; }
        public int? DefaultDim4AccountId { get; set; }
        public int? DefaultDim5AccountId { get; set; }
        public int? DefaultDim6AccountId { get; set; }
        public List<PurchaseRowDTO> PurchaseRows { get; set; }
        public List<OriginUserSmallDTO> OriginUsers { get; set; }
        public string PurchaseLabel { get; set; }
        public int? ContactEComId { get; set; }
        public int? DeliveryConditionId { get; set; }
        public int? DeliveryTypeId { get; set; }
        public int? PaymentConditionId { get; set; }
        public int? DeliveryAddressId { get; set; }
        public string DeliveryAddress { get; set; }
        public int VatType { get; set; }
        public string Origindescription { get; set; }
        public SoeOriginStatus OriginStatus { get; set; }
        public string StatusName { get; set; }
        public int? CurrencyId { get; set; }
        public decimal CurrencyRate { get; set; }
        public DateTime CurrencyDate { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectNr { get; set; }
        public int? OrderId { get; set; }
        public string OrderNr { get; set; }
        public int? StockId { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public decimal TotalAmountExVatCurrency { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public DateTime? WantedDeliveryDate { get; set; }
        public DateTime? ConfirmedDeliveryDate { get; set; }
    }
    [TSInclude]
    public class PurchaseReportDTO
    {
        public int PurchaseId { get; set; }
        public string PurchaseNr { get; set; }
        public int PurchaseNrInt { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string ReferenceOur { get; set; }
        public string ReferenceYour { get; set; }
        public string Attention { get; set; }
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierNr { get; set; }
        public string OurCustomerNr { get; set; }
        public int SupplierNumberInt { get; set; }
        public string SupplierOrgNr { get; set; }
        public string SupplierVatNr { get; set; }
        public string SupplierCountryCode { get; set; }
        public string SupplierCountryName { get; set; }
        public string SupplierEmail { get; set; }
        public int? SysLanguageId { get; set; }
        public int? DefaultDim1AccountId { get; set; }
        public int? DefaultDim2AccountId { get; set; }
        public int? DefaultDim3AccountId { get; set; }
        public int? DefaultDim4AccountId { get; set; }
        public int? DefaultDim5AccountId { get; set; }
        public int? DefaultDim6AccountId { get; set; }
        public List<PurchaseRowReportDTO> PurchaseRows { get; set; }
        public string PurchaseLabel { get; set; }
        public int? ContactEComId { get; set; }
        public int? DeliveryConditionId { get; set; }
        public string DeliveryConditionName { get; set; }
        public string DeliveryConditionCode { get; set; }
        public int? DeliveryTypeId { get; set; }
        public string DeliveryTypeName { get; set; }
        public string DeliveryTypeCode { get; set; }
        public int? PaymentConditionId { get; set; }
        public string PaymentConditionName { get; set; }
        public string PaymentConditioCode { get; set; }
        public int? DeliveryAddressId { get; set; }
        public string DeliveryAddress { get; set; }
        public DateTime? WantedDeliveryDate { get; set; }
        public int VatType { get; set; }
        public string InternalDescription { get; set; }
        public int OriginStatus { get; set; }
        public string StatusName { get; set; }
        public int? CurrencyId { get; set; }
        public decimal CurrencyRate { get; set; }
        public DateTime CurrencyDate { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectNr { get; set; }
        public int? OrderId { get; set; }
        public string OrderNr { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmountCurrency { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal SumAmount { get; set; }
        public decimal SumAmountCurrency { get; set; }
        public int State { get; set; }
    }
    [TSInclude]
    public class PurchaseRowOrderMappingDTO : IPurchaseStatusName
    {
        public int PurchaseRowId { get; set; }
        public int CustomerInvoiceRowId { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public string PurchaseNr { get; set; }
        public int PurchaseId { get; set; }
    }
    [TSInclude]
    public class PurchaseRowSmallDTO
    {
        public int PurchaseRowId { get; set; }
        public int PurchaseRowNr { get; set; }

        public int? SupplierProductId { get; set; }
        public string SupplierProductNr { get; set; }
        public string SupplierProductName { get; set; }

        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductNumber { get; set; }

        public decimal Price { get; set; }
        public decimal? DeliveredQuantity { get; set; }
        public string Text { get; set; }
        public string DisplayName { 
            get {
                if (SupplierProductId > 0) return $"{PurchaseRowNr} - {SupplierProductNr} - {SupplierProductName} ({Text})";
                else return $"{PurchaseRowNr} - {ProductNumber} - {ProductName} ({Text})";
            }
        }
    }
    public interface IPurchaseStatusName
    {
        int Status { get; set; }
        string StatusName { get; set; }
    }
    
    [TSInclude]
    public class PurchaseRowDTO : IPurchaseStatusName
    {
        public int TempRowId { get; set; }
        public int PurchaseRowId { get; set; }
        public int PurchaseId { get; set; }
        public int? ParentRowId { get; set; }
        public string PurchaseNr { get; set; }
        public int? ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductNr { get; set; }
        public int? StockId { get; set; }
        public string StockCode { get; set; }
        public int RowNr { get; set; }
        public int? PurchaseUnitId { get; set; }
        public decimal Quantity { get; set; }
        public decimal? DeliveredQuantity { get; set; }
        public DateTime? WantedDeliveryDate { get; set; }
        public DateTime? AccDeliveryDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Text { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal PurchasePriceCurrency { get; set; }
        public int DiscountType { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountAmountCurrency { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal VatRate { get; set; }
        public int? VatCodeId { get; set; }
        public string VatCodeName { get; set; }
        public string VatCodeCode { get; set; }
        public decimal SumAmount { get; set; }
        public decimal SumAmountCurrency { get; set; }
        public int? OrderId { get; set; }
        public string OrderNr { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public bool IsLocked
        {
            get
            {
                return Status == (int)SoeOriginStatus.PurchasePartlyDelivered || Status == (int)SoeOriginStatus.PurchaseDeliveryCompleted;
            }
        }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
        public PurchaseRowType Type { get; set; }

        public int? SupplierProductId { get; set; }
        public string SupplierProductNr { get; set; }

        public int? IntrastatCodeId { get; set; }
        public int? SysCountryId { get; set; }
        public int? IntrastatTransactionId { get; set; }

        public List<int> CustomerInvoiceRowIds { get; set; }
    }
    
    [TSInclude]
    public class PurchaseRowReportDTO : IPurchaseStatusName
    {
        public int TempRowId { get; set; }
        public int PurchaseRowId { get; set; }
        public int PurchaseId { get; set; }
        public string PurchaseNr { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductNr { get; set; }
        public int? StockId { get; set; }
        public string StockCode { get; set; }
        public int RowNr { get; set; }
        public int PurchaseUnitId { get; set; }
        public string PurchaseUnitName { get; set; }
        public string PurchaseUnitCode { get; set; }
        public decimal Quantity { get; set; }
        public decimal? DeliveredQuantity { get; set; }
        public DateTime? WantedDeliveryDate { get; set; }
        public DateTime? AccDeliveryDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string Text { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal PurchasePriceCurrency { get; set; }
        public int DiscountType { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountAmountCurrency { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatAmountCurrency { get; set; }
        public decimal VatRate { get; set; }
        public int? VatCodeId { get; set; }
        public decimal SumAmount { get; set; }
        public decimal SumAmountCurrency { get; set; }
        public int? OrderId { get; set; }
        public string OrderNr { get; set; }
        public int Status { get; set; }
        public string StatusName { get; set; }
        public bool IsLocked { get; set; }
        public string SupplierProductNr { get; set; }

        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public PurchaseRowType Type { get; set; }
    }
    [TSInclude]
    public class PurchaseFromOrderDTO
    {
        public bool CreateNewPurchase { get; set; }
        public int PurchaseId { get; set; }
        public int SupplierId { get; set; }
        public int OrderId { get; set; }
        public bool CopyProject { get; set; }
        public bool CopyInternalAccounts { get; set; }
        public List<PurchaseRowDTO> PurchaseRows { get; set; }
        public bool CopyDeliveryAddress { get; set; }
    }
    [TSInclude]
    public class PurchaseCustomerInvoiceRowDTO
    {
        public int PurchaseRowId { get; set; }
        public int PurchaseId { get; set; }
        public string ProductNr { get; set; }
        public string Text { get; set; }
        public string PurchaseNr { get; set; }
        public string SupplierName { get; set; }
        public string SupplierNr { get; set; }
        public string Supplier {
            get {
                return $"{SupplierNr} {SupplierName}";
            }
        }
        public string PurchaseRowStatusName { get; set; }
        public int PurchaseRowStatus { get; set; }
        public decimal PurchaseQuantity { get; set; }
        public decimal DeliveredQuantity { get; set; }
        public decimal RemainingQuantity { 
            get {
                return PurchaseQuantity - DeliveredQuantity;
            } 
        }
        public DateTime? RequestedDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime? DeliveryDate { get; set; }

        public int CustomerInvoiceRowId { get; set; }
        public string CustomerInvoiceRowAttestStatus { get; set; }
        public int CustomerInvoiceRowAttestId { get; set; }
        public decimal CustomerInvoiceQuantity { get; set; }
        public decimal CustomerInvoiceInvoiceQuantity { get; set; }
        public decimal CustomerInvoiceInvoicedQuantity { get; set; }
        public DateTime? CustomerInvoiceDeliveryDate { get; set; }
    }

    [TSInclude]
    public class CustomerInvoiceRowPurchaseDTO
    {
        public int InvoiceId { get; set; }
        public int ProductId { get; set; }
        public string ProductNr { get; set; }
        public string Text { get; set; }
        public int? InvoiceSeqNr { get; set; }
        public int InvoiceStatus { get; set; }
        public int CustomerInvoiceRowId { get; set; }
        public string AttestStatus { get; set; }
        public string Unit { get; set; }
        public int AttestStateId { get; set; }
        public string AttestStateColor { get; set; }
        public decimal Quantity { get; set; }
        public decimal InvoiceQuantity { get; set; }
        public decimal InvoicedQuantity { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public List<PurchaseRowGridDTO> PurchaseRows { get; set; }
        public decimal DeliveredPurchaseQuantity { get; set; }
        public int PurchaseRowCount
        {
            get
            {
                return PurchaseRows != null ? PurchaseRows.Count : 0;
            }
        }
    }

    [TSInclude]
    public class PurchaseRowGridDTO
    {
        public int PurchaseRowId { get; set; }
        public int PurchaseId { get; set; }
        public int PurchaseStatus { get; set; }
        public string PurchaseStatusName { get; set; }
        public int CustomerInvoiceRowCount { get; set; }
        public string ProductNr { get; set; }
        public string Text { get; set; }
        public string Unit { get; set; }
        public string PurchaseNr { get; set; }
        public string SupplierName { get; set; }
        public string SupplierNr { get; set; }
        public string Supplier
        {
            get
            {
                return $"{SupplierNr} {SupplierName}";
            }
        }
        public string RowStatusName { get; set; }
        public int RowStatus { get; set; }
        public decimal PurchaseQuantity { get; set; }
        public decimal DeliveredQuantity { get; set; }
        public decimal RemainingQuantity
        {
            get
            {
                return PurchaseQuantity - DeliveredQuantity;
            }
        }
        public DateTime? RequestedDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
    }

    [TSInclude]
    public class PurchaseRowFromStockDTO
    {
        public int TempId { get; set; }
        public int ProductId { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public string ProductUnitCode { get; set; }
        
        public int StockId { get; set; }
        public string StockName { get; set; }
        
        public decimal StockPurchaseTriggerQuantity { get; set; }
        public decimal StockPurchaseQuantity { get; set; }
        public decimal TotalStockQuantity { get; set; }
        public decimal ReservedStockQauntity { get; set; }
        public decimal AvailableStockQuantity { get; set; }
        public decimal PurchasedQuantity { get; set; }

        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierNr { get; set; }
        public int SupplierProductId { get; set; }
        public int SupplierUnitId { get; set; }
        public string SupplierUnitCode { get; set; }

        public bool MultipleSupplierMatches { get;set; }
        public int UnitId { get; set; }
        public string UnitCode { get; set; }
        public decimal? PackSize { get; set; }
        public int DeliveryLeadTimeDays { get; set; }
        
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Sum { get;set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal DiscountPercentage { get; set; }
        public DateTime RequestedDeliveryDate { get; set; }
        public string DeliveryAddress { get; set; }
        public bool ExclusivePurchase { get; set; }
        public string ReferenceOur { get; set; }

        public int VatCodeId { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatRate { get; set; }

        public int PurchaseId { get; set; }
        public string PurchaseNr { get; set; }
    }
}