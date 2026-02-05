using System;

namespace SoftOne.Soe.Business.Util.API.AzoraOne.Connectors
{
    public class SupplierInvoiceConnector : AOBaseConnector
    {
        public SupplierInvoiceConnector(Guid companyApiKey) : base(companyApiKey) { }
        public string GetEndpoint()
        {
            return $"{CompanyEndpoint}/supplierinvoices";
        }
    }
}
