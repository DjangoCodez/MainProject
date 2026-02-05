using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO
{
    public class PriceListProductDTO : PriceListDTO
    {
        public string Number { get; set; }
        public string Name { get; set; }
        public decimal PurchasePrice { get; set; }

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
    public class PriceListDTO
    {
        public int PriceListId { get; set; }
        public int ProductId { get; set; }
        public int PriceListTypeId { get; set; }
        public string SysPriceListTypeName { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime StopDate { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    [TSInclude]
    public class ProductPriceRequestDTO
    {
        public int PriceListTypeId { get; set; }
        public int ProductId { get; set; }
        public int CustomerId { get; set; }
        public int CurrencyId { get; set; }
        public int WholesellerId { get; set; }
        public decimal Quantity { get; set; }
        public bool ReturnFormula { get; set; }
        public bool CopySysProduct { get; set; }
        public decimal? PurchasePrice { get; set; }
    }
    [TSInclude]
    public class ProductPricesRequestDTO
    {
        public List<ProductPricesRowRequestDTO> Products { get; set; }
        public int PriceListTypeId { get; set; }
        public int CustomerId { get; set; }
        public int CurrencyId { get; set; }
        public int WholesellerId { get; set; }
        public bool ReturnFormula { get; set; }
        public bool CopySysProduct { get; set; }
        public bool TimeRowIsLoadingProductPrice { get; set; }
        public bool? IncludeCustomerPrices { get; set; }
        public bool? CheckProduct { get; set; }
    }

    [TSInclude]
    public class ProductPricesRowRequestDTO
    {
        public int ProductId { get; set; }
        public string WholesellerName { get; set; }
        public int TempRowId { get; set; }
        public decimal Quantity { get; set; }
        public decimal? PurchasePrice { get; set; }
    }
}