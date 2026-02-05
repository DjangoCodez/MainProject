namespace SoftOne.Soe.Business.Core.Reporting.Models.Economy
{
    public class SupplierItem
    {
        public string SupplierName { get; set; }
        public string SupplierNr { get; set; }
        public string SupplierOrgNr { get; set; }
        public string SupplierVatNr { get; set; }
        public string Country { get; set; }
        public string Currency { get; set; }
        public string OurCustomerNr { get; set; }
        public string Reference { get; set; }
        public int VatType { get; set; }
        public string PaymentCondition { get; set; }
        public string FactoringSupplier { get; set; }
        public string BIC { get; set; }
        public bool StopPayment { get; set; }
        public bool EDISupplier { get; set; }
        public string DefaultPaymentInformation { get; set; }
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
        public string InvoiceAddress { get; set; }
        public string InvoiceAddressStreet { get; set; }
        public string InvoiceAddressCO { get; set; }
        public string InvoiceAddressPostalCode { get; set; }
        public string InvoiceAddressPostalAddress { get; set; }
        public string InvoiceAddressCountry { get; set; }
        public bool IsActive { get; set; }
        public string Bankgiro { get; set; }
        public string Plusgiro { get; set; }
        public string Cfp { get; set; }
        public string Sepa { get; set; }
    }
}
