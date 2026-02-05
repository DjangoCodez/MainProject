using System;

namespace SoftOne.Soe.Business.Core.Reporting.Models.Billing.Models
{
    public class OrderItem
    {
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string OrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string ProjectNumber { get; set; }
        public string ProjectName { get; set; }
        public decimal AmountExVAT { get; set; }
        public decimal ToInvoiceExVAT { get; set; }
        public string AccountInternalName2 { get; set; }
        public string AccountInternalName3 { get; set; }
        public string AccountInternalName4 { get; set; }
        public string AccountInternalName5 { get; set; }
        public string AccountInternalName6 { get; set; }
        public string OurReference { get; set; }
        public string SalesPriceList { get; set; }
        public string AssignmentType { get; set; }
        public int ReadyStateMy { get; set; }
        public int ReadyStateAll { get; set; }
        public DateTime? Created { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? Changed { get; set; }
        public string ChangedBy { get; set; }
    }
}
