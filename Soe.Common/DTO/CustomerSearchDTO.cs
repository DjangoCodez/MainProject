
namespace SoftOne.Soe.Common.DTO
{
    public class CustomerSearchDTO
    {
        public int ActorCustomerId { get; set; }
        public string Name { get; set; }
        public string CustomerNr { get; set; }
        public string NameOrCustomerNrOrAddress { get; set; }
        public string Note { get; set; }
        public string BillingAddress { get; set; }
        public string DeliveryAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
    }

    public class CustomerSearchIODTO
    {
        
        public string Number { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
    }

    public class CustomerSearchResultIODTO
    {
        public int CustomerId { get; set; }
        public string CustomerNr { get; set; }
        public string CustomerName { get; set; }
        public string OrgNr { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryPostalCode { get; set; }
        public string DeliveryCity { get; set; }
        
    }
}
