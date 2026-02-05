using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class PriceListTypeDTO
    {
        public int PriceListTypeId { get; set; }
        public int CurrencyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public bool InclusiveVat { get; set; }
        public bool IsProjectPriceList { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }
    }

    public class PriceListTypeIODTO
    {
        public int PriceListTypeId { get; set; }
        public string Currency { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal DiscountPercent { get; set; }
        public bool InclusiveVat { get; set; }
    }

    [TSInclude]
    public class PriceListTypeGridDTO
    {
        public int PriceListTypeId { get; set; }
        public int CurrencyId { get; set; }
        public int? SysCurrencyId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool InclusiveVat { get; set; }
        public string Currency { get; set; }
        public bool IsProjectPriceList { get; set; }
    }

}
