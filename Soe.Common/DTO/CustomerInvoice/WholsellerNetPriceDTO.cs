using System;
using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Common.DTO.CustomerInvoice
{
    [TSInclude]
    public class WholsellerNetPriceRowDTO
    {
        public int WholsellerNetPriceId { get; set; }
        public int WholsellerNetPriceRowId { get; set; }
        public int SysWholesellerId { get; set; }
        public string WholesellerName { get; set; }
        public int PriceListTypeId { get; set; }
        public string PriceListTypeName { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public decimal? GNP { get; set; }
        public decimal NetPrice { get; set; }
        public int SysProductId { get; set; }
        public DateTime Date { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public ExternalProductType ProductType { get; set; }

    }
}
