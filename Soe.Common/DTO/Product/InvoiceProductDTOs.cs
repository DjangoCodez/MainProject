using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using TypeLite;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class InvoiceProductSmallDTO : ProductSmallDTO
    {
        public int CalculationType { get; set; }
        public int? ProductUnitId { get; set; }
        public int? ProductGroupId { get; set; }
        public decimal? GuaranteePercentage { get; set; }
        public bool? UseCalculatedCost { get; set; }
        public decimal PurchasePrice { get; set; }
    }
    [TSInclude]
    public class InvoiceProductDTO : ProductDTO
    {
        public int? SysProductId { get; set; }
        public int? SysPriceListHeadId { get; set; }
        public TermGroup_InvoiceProductVatType VatType { get; set; }
        public bool VatFree { get; set; }
        public string EAN { get; set; }
        public decimal PurchasePrice { get; set; }
        public string SysWholesellerName { get; set; }
        public TermGroup_InvoiceProductCalculationType CalculationType { get; set; }
        public decimal? GuaranteePercentage { get; set; }
        public int? TimeCodeId { get; set; }
        public int PriceListOrigin { get; set; }
        public bool ShowDescriptionAsTextRow { get; set; }
        public bool ShowDescrAsTextRowOnPurchase { get; set; }
        public bool DontUseDiscountPercent { get; set; }
        public bool UseCalculatedCost { get; set; }
        public int? VatCodeId { get; set; }
        public int? HouseholdDeductionType { get; set; }
        public decimal? HouseholdDeductionPercentage { get; set; }
        public bool IsStockProduct { get; set; }
        public decimal? Weight { get; set; }
        public int? IntrastatCodeId { get; set; }
        public int? SysCountryId { get; set; }
        public int? DefaultGrossMarginCalculationType { get; set; }

        // Relations
        [TSIgnore]
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> PurchaseAccounts { get; set; }
        [TSIgnore]
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> SalesAccounts { get; set; }
        [TSIgnore]
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> VatAccounts { get; set; }
        [TSIgnore]
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> SalesNoVatAccounts { get; set; }
        [TSIgnore]
        [TsIgnore]
        public Dictionary<int, AccountSmallDTO> SalesContractorAccounts { get; set; }

        // Extensions
        public bool? IsExternal { get; set; }

        public decimal SalesPrice { get; set; }
        public bool IsSupplementCharge
        {
            get { return (CalculationType == TermGroup_InvoiceProductCalculationType.SupplementCharge); }
        }

        public List<PriceListDTO> PriceLists { get; set; }
        public List<int> CategoryIds { get; set; }
        public List<AccountingSettingsRowDTO> AccountingSettings { get; set; }
        public int? SysProductType { get; set; }
    }

    [TSInclude]
    public class InvoiceProductGridDTO
    {
        public int ProductId { get; set; }
        public string Number { get; set; }
        public string NumberSort { get; set; }
        public string Name { get; set; }
        public int? ProductGroupId { get; set; }
        public string ProductGroupCode { get; set; }
        public string ProductGroupName { get; set; }
        public string ProductCategories { get; set; }
        public int? SysProductId { get; set; }
        public bool External { get; set; }
        public TermGroup_InvoiceProductVatType VatType { get; set; }
        public SoeEntityState State { get; set; }
        public string EanCode { get; set; }
        public int? TimeCodeId { get; set; }
        public string TimeCodeName { get; set; }
        public bool? IsActive
        {
            get { return this.State == SoeEntityState.Active; }
            set { this.State = value.HasValue && value.Value ? SoeEntityState.Active : SoeEntityState.Inactive; }
        }
    }

    public class InvoiceProductSearchDTO
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public string EAN { get; set; }
        public bool External { get; set; }
        public DateTime? ModifiedSince { get; set; }
        public bool IncludeInactive { get; set; }
        public List<int> ProductGroupIds { get; set; }
    }

    [TSInclude]
    public class ProductRowsProductDTO
    {
        // Use only in ProductRowsDirective!
        // Performance optimized, do not add unneccessary properties!

        public int ProductId { get; set; }
        public int? SysProductId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }
        public bool ShowDescriptionAsTextRow { get; set; }
        public bool ShowDescrAsTextRowOnPurchase { get; set; }

        public int? ProductUnitId { get; set; }
        public string ProductUnitCode { get; set; }

        public TermGroup_InvoiceProductVatType VatType { get; set; }
        public TermGroup_InvoiceProductCalculationType CalculationType { get; set; }

        public decimal? GuaranteePercentage { get; set; }
        public string SysWholesellerName { get; set; }

        public bool DontUseDiscountPercent { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SalesPrice { get; set; }
        public int? VatCodeId { get; set; }

        public decimal? Weight { get; set; }
        public bool IsExternal { get; set; }

        public bool IsStockProduct { get; set; }
        public bool IsSupplementCharge { get; set; }
        public int? HouseholdDeductionType { get; set; }
        public decimal? HouseholdDeductionPercentage { get; set; }
        public bool IsLiftProduct { get; set; }
        public bool IsInactive { get; set; }
        public int? IntrastatCodeId { get; set; }
        public int? SysCountryId { get; set; }
        public TermGroup_GrossMarginCalculationType GrossMarginCalculationType { get; set; }
    }

    public class InvoiceProductStatisticsDTO
    {
        public int ProductId { get; set; }
        public decimal SalesQuantity { get; set; }
        public decimal SalesAmount { get; set; }
    }

    public class InvoiceProductCopyDTO
    {
        public int ProductId {  set; get; }
        public string Number {  get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string EAN { get; set; }
        public string AccountingPrio { get; set; }
        public int CalculationType { get; set; }
        public int VatType { get; set; }
        public int Type { get; set; }
        public int? VatCodeId { get; set; }
        public decimal PurchasePrice { get; set; }
        public int? ExternalProductId { get; set; }
        public int? ExternalPriceListHeadId { get; set; }
        public string SysWholesellerName { get; set; }
        public string ProductUnitCode { get; set; }
        public string ProductUnitName { get; set; }
        public string ProductGroupCode { get; set; }
        public string ProductGroupName { get; set; }
        public int? TimeCodeId { get; set; }
        public bool ShowDescriptionAsTextRow { get; set; }
        public bool ShowDescrAsTextRowOnPurchase { get; set; }

        public List<ProductAccountStdDTO> ProductAccounts { get; set; }

    }
}
