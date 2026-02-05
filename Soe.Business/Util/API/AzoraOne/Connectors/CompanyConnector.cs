using SoftOne.Soe.Business.Util.API.AzoraOne.Models;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.API.AzoraOne.Connectors
{
    public class CompanyConnector : AOBaseConnector
    {
        public CompanyConnector(Guid companyGuid) : base(companyGuid) { }
        public string GetEndpoint()
        {
            return $"/companies";
        }
        public string SingleCompanyEndpoint()
        {
            return CompanyEndpoint;
        }
        public AOResponseWrapper<AOCompany> GetCompany()
        {
            return Get<AOCompany>(SingleCompanyEndpoint());
        }
        public AOResponseWrapper<AOCompany> UpdateCompany(AOCompany company)
        {
            var result = Put<AOCompany>(SingleCompanyEndpoint(), company);

            if (result.IsSuccess)
                return result;

            if (TryAdjustForCompanyErrors(company, result.Error.Data))
                return Put<AOCompany>(SingleCompanyEndpoint(), company);

            return result;
        }
        public AOResponseWrapper<AOCompany> AddCompany(AOCompany company)
        {
            var result = Post<AOCompany>(GetEndpoint(), company);

            if (result.IsSuccess)
                return result;

            if (TryAdjustForCompanyErrors(company, result.Error.Data))
                return Post<AOCompany>(GetEndpoint(), company);

            return result;
        }

        public bool TryAdjustForCompanyErrors(AOCompany company, List<AOErrorDetails> errors)
        { 
            foreach (var error in errors)
            {
                switch ((ConnectorError) error.Code)
                {
                    case ConnectorError.Company_OrgNrIsNotValid:
                        company.CorporateIdentityNumber = null;
                        break;
                    case ConnectorError.Company_VatNrIsNotValid:
                        company.VatNumber = null;
                        break;
                }
            }
            return true;
        }
    }
}
