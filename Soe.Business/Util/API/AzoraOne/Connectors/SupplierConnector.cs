
using Microsoft.Rest.TransientFaultHandling;
using RestSharp;
using SoftOne.Soe.Business.Util.API.AzoraOne.Models;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.API.AzoraOne.Connectors
{
    public class AOSupplierConnector : AOBaseConnector
    {
        public AOSupplierConnector(Guid companyApiKey) : base(companyApiKey) { }
        public string GetEndpoint()
        {
            return $"{CompanyEndpoint}/suppliers";
        }
        public AOResponseWrapper<AOSupplier> AddSupplier(AOSupplier supplier, bool shouldRetry)
        {
            var result = Post<AOSupplier>(GetEndpoint(), supplier);

            if (result.IsSuccess || !shouldRetry)
                return result;

            if (result.Error.Data == null)
                return result;

            if (TryAdjustForSupplierErrors(supplier, result.Error.Data))
                return Post<AOSupplier>(GetEndpoint(), supplier);

            return result;
        }
        public AOResponseWrapper<bool> AddSuppliers(List<AOSupplier> suppliers)
        {
            var request = new RestRequest($"{GetEndpoint()}/multiple", Method.Post);
            request.RequestFormat = DataFormat.Json;

            var suppliersObject = new AOSuppliers{ Suppliers = suppliers };
            AddBody(request, suppliersObject);
            var response = this.Client.Execute(request);

            if ((int)response.StatusCode == 200)
            {
                return AOResponseWrapper<bool>.Success(new AOResponse<bool>() { Data = true }, false, response.Content);
            }
            else
            {
                return AOResponseWrapper<bool>.Success(new AOResponse<bool>() { Data = false }, false, response.Content);
            }
        }
        public AOResponseWrapper<AOSupplier> UpdateSupplier(AOSupplier supplier, bool shouldRetry)
        {
            var result = Put<AOSupplier>($"{GetEndpoint()}/{supplier.SupplierID}", supplier);

            if (result.IsSuccess || !shouldRetry)
                return result;

            if (result.Error.Data == null)
                return result;

            if (TryAdjustForSupplierErrors(supplier, result.Error.Data))
                return Put<AOSupplier>($"{GetEndpoint()}/{supplier.SupplierID}", supplier);

            return result;
        }
        public AOResponseWrapper<AOSupplier> GetSupplier(string supplierId)
        {
            return Get<AOSupplier>($"{GetEndpoint()}/{supplierId}");
        }
        public AOResponseWrapper<AOSupplier> GetSupplier(int supplierId)
        {
            return GetSupplier(supplierId.ToString());
        }
        public AOResponseWrapper<AOSuppliers> GetSuppliers()
        {
            return Get<AOSuppliers>(GetEndpoint());
        }

        public bool TryAdjustForSupplierErrors(AOSupplier supplier, List<AOErrorDetails> errors)
        {
            foreach (var error in errors)
            {
                switch ((ConnectorError) error.Code)
                {
                    case ConnectorError.Supplier_IdAlreadyExists:
                    case ConnectorError.Supplier_IdIsMissing:
                    case ConnectorError.Supplier_IdIsNotValid:
                        //Cases which we cannot jump back from
                        return false;
                    case ConnectorError.Supplier_OrgNrAndBGAndPGAndIbanAreMissing:
                        supplier.BankAccountNumber = "000-00000";
                        break;
                    case ConnectorError.Supplier_NameIsNotValid:
                        supplier.SupplierName = "InvalidName";
                        break;
                    case ConnectorError.Supplier_OrgNrIsNotValid:
                        supplier.CorporateIdentityNumber = null;
                        break;
                    case ConnectorError.Supplier_BGNumberIsNotValid:
                        supplier.BankAccountNumber = null;
                        break;
                    case ConnectorError.Supplier_PGNumberIsNotValid:
                        supplier.PlusGiroNumber = null;
                        break;
                    case ConnectorError.Supplier_IbanIsNotValid:
                        supplier.Iban = null;
                        break;
                }
            }
            return true;
        }
    }
}
