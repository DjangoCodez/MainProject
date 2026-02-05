using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Billing.Models
{
    public class InvoiceItem
    {
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string InvoiceType { get; set; }
        public string Status { get; set; }
        public string ProjectNumber { get; set; }
        public string ProjectName { get; set; }
        public decimal? AmountExVAT { get; set; }
        public decimal? ToInvoiceExVAT { get; set; }
        public string AccountInternalName1 { get; set; }
        public string AccountInternalName2 { get; set; }
        public string AccountInternalName3 { get; set; }
        public string AccountInternalName4 { get; set; }
        public string AccountInternalName5 { get; set; }
        public string AccountInternalName6 { get; set; }
        public string OurReference { get; set; }
        public string OriginDescription { get; set; }
        public string InvoiceLabel { get; set; }
        public string SalesPriceList { get; set; }
        public string VATType { get; set; }
        public string Currency { get; set; }
        public string InvoiceAddress { get; set; }
        public string DeliveryAddress { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Changed { get; set; }
        public string ChangedBy { get; set; }
    }
}
