using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.API.AvionData.Models
{
    /// <summary>
    /// Represents the result of a company search.
    /// </summary>
    public class CompanyResult
    {
        /// <summary>
        /// Total number of results matching the search criteria.
        /// </summary>
        public int TotalResults { get; set; }

        /// <summary>
        /// List of companies returned in the result.
        /// </summary>
        public List<Company> Companies { get; set; }
    }

    /// <summary>
    /// Represents a company entry with all business details.
    /// </summary>
    public class Company
    {
        public CompanyIdentifier BusinessId { get; set; }
        public CompanyIdentifier EuId { get; set; }

        /// <summary>
        /// List of registered names of the company.
        /// </summary>
        public List<RegisterName> Names { get; set; }

        /// <summary>
        /// Main business line of the company.
        /// </summary>
        public RegisteredEntry MainBusinessLine { get; set; }

        /// <summary>
        /// Website details.
        /// </summary>
        public Website Website { get; set; }

        public List<CompanyForm> CompanyForms { get; set; }
        public List<CompanySituation> CompanySituations { get; set; }
        public List<RegisteredEntry> RegisteredEntries { get; set; }
        public List<Address> Addresses { get; set; }

        public string TradeRegisterStatus { get; set; }
        public string Status { get; set; }
        public string RegistrationDate { get; set; }
        public string EndDate { get; set; }
        public string LastModified { get; set; }
    }

    /// <summary>
    /// Identifier for a company (e.g., business ID or EU ID).
    /// </summary>
    public class CompanyIdentifier
    {
        public string Value { get; set; }
        public string RegistrationDate { get; set; }
        public string Source { get; set; }
    }

    /// <summary>
    /// A registered name entry for a company.
    /// </summary>
    public class RegisterName
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string RegistrationDate { get; set; }
        public string LanguageCode { get; set; }
        public string EndDate { get; set; }
        public int Version { get; set; }
        public string Source { get; set; }
    }

    /// <summary>
    /// Company form (legal structure) details.
    /// </summary>
    public class CompanyForm
    {
        public string Type { get; set; }
        public List<DescriptionEntry> Descriptions { get; set; }
        public string RegistrationDate { get; set; }
        public string EndDate { get; set; }
        public int Version { get; set; }
        public string Source { get; set; }
    }

    /// <summary>
    /// Describes the situation/status of a company.
    /// </summary>
    public class CompanySituation
    {
        public string Description { get; set; }
        public string Type  { get; set; }
        public string RegistrationDate { get; set; }
        public string EndDate { get; set; }
        public string Source { get; set; }
    }

    /// <summary>
    /// A registered entry such as a business line or other official registry entry.
    /// </summary>
    public class RegisteredEntry
    {
        public string Type { get; set; }
        public List<DescriptionEntry> Descriptions { get; set; }
        public string RegistrationDate { get; set; }
        public string EndDate { get; set; }
        public string Register { get; set; }
        public string Authority { get; set; }
    }

    /// <summary>
    /// Represents an address entry associated with the company.
    /// </summary>
    public class Address
    {
        public int Type { get; set; }
        public string Street { get; set; }
        public string PostCode { get; set; }
        public List<PostOffice> PostOffices { get; set; }
        public string PostOfficeBox { get; set; }
        public string BuildingNumber { get; set; }
        public string Entrance { get; set; }
        public string ApartmentNumber { get; set; }
        public string ApartmentIdSuffix { get; set; }
        public string Co { get; set; }
        public string Country { get; set; }
        public string FreeAddressLine { get; set; }
        public string RegistrationDate { get; set; }
        public string Source { get; set; }
    }

    /// <summary>
    /// Describes localized text.
    /// </summary>
    public class DescriptionEntry
    {
        public string LanguageCode { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Represents the website of the company.
    /// </summary>
    public class Website
    {
        public string Url { get; set; }
        public string RegistrationDate { get; set; }
        public string Source { get; set; }
    }

    /// <summary>
    /// Post office metadata.
    /// </summary>
    public class PostOffice
    {
        public string City { get; set; }
        public string LanguageCode { get; set; }
        public string MunicipalityCode { get; set; }
    }

    /// <summary>
    /// Standard API error response.
    /// </summary>
    public class ErrorResponse
    {
        public string Timestamp { get; set; }
        public string Message { get; set; }
        public string Errorcode { get; set; }
    }
}
