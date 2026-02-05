using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class SupplierProductSmallDTO
    {
        public int SupplierProductId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        // Extensions
        public string NumberName
        {
            get
            {
                return String.Format("{0} {1}", StringUtility.NullToEmpty(Number).Trim(), StringUtility.NullToEmpty(Name).Trim());
            }
        }
    }

    [TSInclude]
    public class SupplierProductDTO
    {
        public int SupplierProductId { get; set; }
        public int SupplierId { get; set; }
        public int SupplierProductUnitId { get; set; }
        public string SupplierProductNr { get; set; }
        public string SupplierProductName { get; set; }
        public string SupplierProductCode { get; set; }

        public int ProductId { get; set; }
        public int? SysCountryId { get; set; }

        public decimal? PackSize { get; set; }
        public int? DeliveryLeadTimeDays { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        public int? IntrastatCodeId { get; set; }

        //Added to support migrated Angular pages
        public List<SupplierProductPriceDTO> PriceRows { get; set; }
    }

    public class SupplierProductExDTO : SupplierProductDTO
    {
        public string SupplierNr { get; set; }
        public string SupplierName { get; set; }
        public string SupplierProductUnitCode { get; set; }
    }

    [TSInclude]
    public class SupplierProductGridDTO
    {
        public int SupplierProductId { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierNr { get; set; }
        public string SupplierProductName { get; set; }
        public string SupplierProductNr { get; set; }
        public string SupplierProductCode { get; set; }
        public string SupplierProductUnitName { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductNr { get; set; }
    }

    [TSInclude]
    public class SupplierProductSearchDTO
    {
        public List<int> SupplierIds { get; set; }
        public string SupplierProduct { get; set; }
        public string SupplierProductName { get; set; }
        public string Product { get; set; }
        public string ProductName { get; set; }
        public int InvoiceProductId { get; set; }
    }

    [TSInclude]
    public class SupplierProductPriceDTO
    {
        public int SupplierProductPriceId { get; set; }
        public int? SupplierProductPriceListId { get; set; }
        public int SupplierProductId { get; set; }
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
        public int SysCurrencyId { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class SupplierProductSaveDTO
    {
        public SupplierProductDTO Product { get; set; }
        public List<SupplierProductPriceDTO> PriceRows  { get; set; }
    }

    /// <summary>
    /// As per the guidelines,
    /// added this new Grid DTO class to use in new Angular migration grid by removing unnecessary properties.
    /// </summary>
    [TSInclude]
    public class SupplierProductPriceListGridDTO
    {
        public int SupplierProductPriceListId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SupplierNr { get; set; }
        public string SupplierName { get; set; }
        public string SysWholeSellerName { get; set; }
        public string SysWholeSellerTypeName { get; set; }
        public string CurrencyCode { get; set; }
        public DateTime Created { get; set; }
    }

    [TSInclude]
    public class SupplierProductPricelistDTO
    {
        public int SupplierProductPriceListId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int SupplierId { get; set; }
        public string SupplierNr { get; set; }
        public string SupplierName { get; set; }
        public int? SysWholeSellerId { get; set; }
        public string SysWholeSellerName { get; set; }
        public int SysWholeSellerType { get; set; }
        public string SysWholeSellerTypeName { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public int SysCurrencyId { get; set; }
        public int CurrencyId { get; set; }
        public string CurrencyCode { get; set; }
    }

    [TSInclude]
    public class SupplierProductPriceSearchDTO
    {
        public int SupplierId { get; set; }
        public int currencyId { get; set; }
        public DateTime CompareDate { get; set; }
        public bool IncludePricelessProducts { get; set; }
    }

    [TSInclude]
    public class SupplierProductPriceListSaveDTO
    {
        public SupplierProductPricelistDTO PriceList { get; set; }
        public List<SupplierProductPriceDTO> PriceRows { get; set; }
    }

    [TSInclude]
    public class SupplierProductPriceComparisonDTO : SupplierProductPriceDTO
    {
        public int CompareSupplierProductPriceId { get; set; }
        public decimal CompareQuantity { get; set; }
        public decimal ComparePrice { get; set; }
        public DateTime? CompareStartDate { get; set; }
        public DateTime? CompareEndDate { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public string OurProductName { get; set; }
    }

    [TSInclude]
    public class SupplierProductImportDTO
    {
        public bool ImportToPriceList { get; set; }
        public bool ImportPrices { get; set; }
        public int? SupplierId { get; set; }
        public int? PriceListId { get; set; }
        public List<SupplierProductImportRawDTO> Rows { get; set; }
        public ImportOptionsDTO Options { get; set; }
    }
}