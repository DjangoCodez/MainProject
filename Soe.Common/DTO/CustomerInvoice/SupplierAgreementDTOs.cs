using System;
using SoftOne.Soe.Common.Attributes;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class SupplierAgreementDTO
    {
        public int RebateListId { get; set; }
        public int SysWholesellerId { get; set; }
        public string WholesellerName { get; set; }
        public int? PriceListTypeId { get; set; }
        public string PriceListTypeName { get; set; }
        public decimal DiscountPercent { get; set; }
        public string Code { get; set; }
        public int CodeType { get; set; }
        public DateTime? Date { get; set; }
        public int? CategoryId { get; set; }
        public int State { get; set; }
    }

}
