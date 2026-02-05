namespace SoftOne.Soe.Business.Core.Reporting.Models.Economy.Models
{
    public class CustomerItem
    {
        public string CustomerName { get; set; }
        public string CustomerOrgNr { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerVatNr { get; set; }
        public int CustomerSupNr { get; set; }
        public string Country { get; set; }
        public string Currency { get; set; }
        public string PhoneJob { get; set; }
        public string Email { get; set; }
        public string Web { get; set; }
        public string Fax { get; set; }
        public string DeliveryAddressStreet { get; set; }
        public string DeliveryAddressCO { get; set; }
        public string DeliveryAddressPostalCode { get; set; }
        public string DeliveryAddressPostalAddress { get; set; }
        public string DistributionAddressStreet { get; set; }
        public string DistributionAddressCO { get; set; }
        public string DistributionAddressPostalCode { get; set; }
        public string DistributionAddressPostalAddress { get; set; }
        public string DistributionAddressCountry { get; set; }
        public string VisitingAddressStreet { get; set; }
        public string VisitingAddressCO { get; set; }
        public string VisitingAddressPostalCode { get; set; }
        public string VisitingAddressPostalAddress { get; set; }
        public string VisitingAddressCountry { get; set; }
        public string DeliveryAddress { get; set; }
        public string DistributionAddress { get; set; }
        public string VisitingAddress { get; set; }
        public bool IsActvie { get; set; }
        public string BillingAddress { get; set; }
        public string BillingAddressStreet { get; set; }
        public string BillingAddressCO { get; set; }
        public string BillingAddressPostalCode { get; set; }
        public string BillingAddressPostalAddress { get; set; }
        public string BillingAddressCountry { get; set; }
        public decimal DiscountMerchandise { get; set; }
        public string InvoiceReference { get; set; }
        public bool DisableInvoiceFee { get; set; }
        public int? InvoiceDeliveryType { get; set; }
        public string ContactGLN { get; set; }
        public string InvoiceLabel { get; set; }
        public string PaymentCondition { get; set; }
        public bool ImportInvoicesDetailed { get; set; }
    }
}
