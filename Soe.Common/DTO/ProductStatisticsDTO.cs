using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class ProductStatisticsDTO
    {
        public SoeOriginType OriginType { get; set; }
        public string OriginTypeName { get; set; }
        public int ProductId { get; set; }
        public string ProductNr { get; set; }
        public string ProductName { get; set; }
        public string Year { get; set; }    
        public string Month { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? PurchaseDeliveryDate { get; set; }
        public int InvoiceId { get; set; }
        public string InvoiceNr { get; set; }
        public int OrderId { get; set; }
        public string OrderNr { get; set; }
        public int PurchaseId { get; set; }
        public string PurchaseNr { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerName { get; set; }
        public string SupplierNr { get; set; }
        public string SupplierName { get; set; }
        public decimal PurchaseQty { get; set; }
        public decimal CustomerInvoiceQty { get; set; }
        public decimal CustomerInvoiceAmount { get; set; }
        public decimal MarginalIncome { get; set; }
        public decimal MarginalRatio { get; set; }
        public TermGroup_InvoiceProductVatType VatType { get; set; }
    }
}
