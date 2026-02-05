using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class PurchaseStatisticsDTO
    {
        public string SupplierNr { get; set; }
        public string SupplierName { get; set; }
        public string SupplierNumberName { get { return $"{SupplierNr} {SupplierName}"; } }
        public string PurchaseNr { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public int Status { get; set; }
        public int SysCurrencyId { get; set; }
        public string ProductUnitCode { get; set; }

        public string StatusName { get; set; }
        public string CurrencyCode { get; set; }
        public string Code { get; set; }
        public string ProjectNumber { get; set; }
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }
        public string SupplierItemNumber { get; set; }
        public string SupplierItemName { get; set; }
        public string SupplierItemCode { get; set; }
        public string StockPlace { get; set; }
        public string CustomerOrderNumber { get; set; }
        public decimal Quantity { get; set; }
        public decimal? DeliveredQuantity { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal PurchasePriceCurrency { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountAmountCurrency { get; set; }
        public decimal SumAmount { get; set; }
        public decimal SumAmountCurrency { get; set; }
        public DateTime? WantedDeliveryDate { get; set; }
        public DateTime? AcknowledgeDeliveryDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public int RowStatus { get; set; }
        public string RowStatusName { get; set; }
        public string Unit { get; set; }
    }
}

