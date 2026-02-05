using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using TypeLite;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class ProductSmallDTO
    {
        public int ProductId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }

        // Extensions
        [TsIgnore]
        public string NumberName
        {
            get
            {
                return String.Format("{0} {1}", StringUtility.NullToEmpty(Number).Trim(), StringUtility.NullToEmpty(Name).Trim());
            }
        }
        [TSIgnore]
        
        public string NumberSort
        {
            get
            {
                return StringUtility.IsNumeric(Number) ? this.Number.PadLeft(100, '0') : this.Number;
            }
        }
    }

    [TSInclude]
    public class ProductPriceListDTO : ProductSmallDTO
    {
        public int PriceListId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
    }

    [TSInclude]
    public class ProductComparisonDTO : ProductSmallDTO
    {
        public decimal PurchasePrice { get; set; }
        public decimal ComparisonPrice { get; set; }
        public decimal Price { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
    }

    [TSInclude]
    public class ProductDTO : ProductSmallDTO
    {
        public int? ProductUnitId { get; set; }
        public int? ProductGroupId { get; set; }
        public int Type { get; set; }

        public string Description { get; set; }
        public string AccountingPrio { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public int State { get; set; }

        // Extensions
        public string ProductUnitCode { get; set; }
    }

    [TSInclude]
    public class ProductCleanupDTO
    {         
        public int ProductId { get; set; }
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public bool IsExternal { get; set; }
        public DateTime LastUsedDate { get; set; }
        public bool IsActive { get; set; }
    }

    [TSInclude]
    public class ProductTimeCodeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TermGroup_InvoiceProductVatType? VatType { get; set; }
        public int State { get; set; }
    }
}
