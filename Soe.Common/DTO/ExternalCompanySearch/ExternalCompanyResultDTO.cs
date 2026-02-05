using SoftOne.Soe.Common.Attributes;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class ExternalCompanyResultDTO
    {
        public string RegistrationNr { get; set; }
        public string Name { get; set; }
        public ExternalCompanyAddressDTO StreetAddress { get; set; }
        public ExternalCompanyAddressDTO PostalAddress { get; set; }
        public string WebUrl { get; set; }
    }

    [TSInclude]
    public class ExternalCompanyAddressDTO
    {
        public string Street { get; set; }
        public string PostOfficeBox { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string CO { get; set; }
        public string AddressLine1
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Street)
                        ? Street
                        : (!string.IsNullOrWhiteSpace(PostOfficeBox)
                            ? PostOfficeBox
                            : string.Empty);
            }
        }
    }
}
