using SoftOne.Soe.Common.DTO;
using System;

namespace SoftOne.Soe.Business.Util.API.AzoraOne.Models
{
    public class AOCompany
    {
        public string CompanyID { get; set; }
        public string CompanyName { get; set; }
        public string CompanyProxy { get; set; } //License
        public bool Active { get; set; }
        public AOPrecognition Precognition { get; set; }
        public string CorporateIdentityNumber { get; set; }
        public string VatNumber { get; set; }
        public string BankAccountNumber { get; set; }
        public string PlusGiroNumber { get; set; }
        public string Iban { get; set; }

        public bool Equals(AOCompany other)
        {
            return CompanyID == other.CompanyID &&
                CorporateIdentityNumber == other.CorporateIdentityNumber &&
                VatNumber == other.VatNumber;
        }
    }
    // Used for generating scanning suggestions without having a pretrained model.
    // I.e. when AzoraOne hasn't been trained to handle a specific document type (i.e. a specific supplier's invoice layout).
    public class AOPrecognition
    {
        public bool Active { get; set; }
    }

    public static class AOCompanyExtensions
    {
        public static AOCompany ToAzoraOneModel(this CompanyDTO company, Guid companyGuid)
        {
            return new AOCompany
            {
                CompanyID = companyGuid.ToString(),
                CompanyName = company.Name,
                CompanyProxy = company.LicenseNr,
                Active = true,
                Precognition = new AOPrecognition
                {
                    Active = true
                },
                CorporateIdentityNumber = AzoraOneHelper.ParseOrgNr(company.OrgNr),
                VatNumber = company.VatNr,
            };
        }
    }
}
