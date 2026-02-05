using System;
using System.Collections.Generic;
using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Common.DTO
{
    public class ContactDTO
    {
        public int ContactId { get; set; }
        public int? ActorId { get; set; }
        public TermGroup_SysContactType SysContactTypeId { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
        public SoeEntityState State { get; set; }

        // Extensions
        public List<ContactAddressDTO> ContactAddresses { get; set; }
        public List<ContactEComDTO> ContactEComs { get; set; }
    }

    public class ContactIODTO
    {

        public string SupplierNr { get; set; }
        public string CustomerNr { get; set; }
        public bool OnlyAddress { get; set; }
        public int ActorCompanyId { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string DistributionAddress { get; set; }
        public string DistributionCoAddress { get; set; }
        public string DistributionPostalCode { get; set; }
        public string DistributionPostalAddress { get; set; }
        public string DistributionCountry { get; set; }

        public string BillingAddress { get; set; }
        public string BillingCoAddress { get; set; }
        public string BillingPostalCode { get; set; }
        public string BillingPostalAddress { get; set; }
        public string BillingCountry { get; set; }

        public string BoardHQAddress { get; set; }
        public string BoardHQCountry { get; set; }

        public string VisitingAddress { get; set; }
        public string VisitingCoAddress { get; set; }
        public string VisitingPostalCode { get; set; }
        public string VisitingPostalAddress { get; set; }
        public string VisitingCountry { get; set; }

        public string DeliveryAddress { get; set; }
        public string DeliveryCoAddress { get; set; }
        public string DeliveryPostalCode { get; set; }
        public string DeliveryPostalAddress { get; set; }
        public string DeliveryCountry { get; set; }

        public string Email1 { get; set; }
        public string Email2 { get; set; }
        public string PhoneHome { get; set; }
        public string PhoneMobile { get; set; }
        public string PhoneJob { get; set; }
        public string Fax { get; set; }
        public string Webpage { get; set; }
    }

    [TSInclude]
    public class ContactAddressDTO
    {
        public int ContactAddressId { get; set; }
        public int ContactId { get; set; }
        public TermGroup_SysContactAddressType SysContactAddressTypeId { get; set; }

        public string Name { get; set; }
        public bool IsSecret { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }

        // Extensions
        public List<ContactAddressRowDTO> ContactAddressRows { get; set; }
        public string Address { get; set; }
    }

    [TSInclude]
    public class ContactAddressRowDTO
    {
        public int RowNr { get; set; }
        public int ContactAddressId { get; set; }
        public TermGroup_SysContactAddressRowType SysContactAddressRowTypeId { get; set; }

        public string Text { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }
    [TSInclude]
    public class ContactAdressIODTO
    {
        public int? ContactAddressId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string CoAddress { get; set; }
        public string PostalCode { get; set; }
        public string PostalAddress { get; set; }
        public string Country { get; set; }
    }
    [TSInclude]
    public class ContactEComIODTO
    {
        public int ContactEComId { get; set; }

        public string Name { get; set; }
        public string Text { get; set; }
    }

    public class ContactEComDTO
    {
        public int ContactEComId { get; set; }
        public int? ContactId { get; set; }
        public TermGroup_SysContactEComType SysContactEComTypeId { get; set; }

        public string Name { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }

        public bool IsSecret { get; set; }

        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? Modified { get; set; }
        public string ModifiedBy { get; set; }
    }
}
