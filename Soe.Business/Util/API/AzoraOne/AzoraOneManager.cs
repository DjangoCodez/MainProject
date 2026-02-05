using SoftOne.Soe.Business.Util.API.AzoraOne.Connectors;
using SoftOne.Soe.Business.Util.API.AzoraOne.Models;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;

namespace SoftOne.Soe.Business.Util.API.AzoraOne
{
    public class AzoraOneManager
    {
        private Guid _companyApiKey { get; set; }
        public AzoraOneManager(string companyApiKey) : this(new Guid(companyApiKey)) { }
        public AzoraOneManager(Guid companyApiKey) 
        {
            if (companyApiKey == Guid.Empty)
                throw new ArgumentException("API key is required");

            _companyApiKey = companyApiKey;
        }

        public bool CompanyExists()
        {
            var connector = new CompanyConnector(_companyApiKey);
            var response = connector.GetCompany();
            return response.GetSucceeded();
        }

        public ActionResult SaveCompany(CompanyDTO company)
        {
            var connector = new CompanyConnector(_companyApiKey);
            var existingCompany = connector.GetCompany();
            var aoCompany = company.ToAzoraOneModel(_companyApiKey);

            return (
                    existingCompany.GetSucceeded() ?
                    connector.UpdateCompany(aoCompany) :
                    connector.AddCompany(aoCompany)
                ).ToActionResult();
        }
        public ActionResult DeactivateCompany()
        {
            var connector = new CompanyConnector(_companyApiKey);
            var response = connector.GetCompany();
            if (response.GetSucceeded())
            {
                var company = response.GetValue();
                company.Active = false;
                company.CorporateIdentityNumber = null;
                company.PlusGiroNumber = null;
                company.BankAccountNumber = null;
                company.Iban = null;
                return connector
                    .UpdateCompany(company)
                    .ToActionResult();
            }
            var result = new ActionResult();
            result.ObjectsAffected = 0;
            return result;
        }
        public ActionResult SyncSupplier(SupplierDistributionDTO supplierIn)
        {
            var connector = new AOSupplierConnector(_companyApiKey);
            var response = connector.GetSupplier(supplierIn.SupplierId);
            var supplier = supplierIn.ToAOSupplier();

            if (response.IsSuccess)
            {
                var existingSupplier = response.GetValue();
                if (supplier.Equals(existingSupplier))
                    return response.ToActionResult();
                else
                    return connector
                        .UpdateSupplier(supplier, true)
                        .ToActionResult();
            }

            return connector
                .AddSupplier(supplier, true)
                .ToActionResult();
        }

        public ActionResult SyncSuppliers(List<SupplierDistributionDTO> suppliersIn)
        {
            var connector = new AOSupplierConnector(_companyApiKey);
            var batchPost = connector.AddSuppliers(suppliersIn.ToAOSuppliers());

            var response = connector.GetSuppliers();
            if (!response.IsSuccess)
                return response.ToActionResult();

            var suppliers = suppliersIn.ToAOSuppliers();
            var existingAOSuppliers = response.GetValue().Suppliers;

            var suppliersToAdd = new List<AOSupplier>();
            var suppliersToUpdate = new List<AOSupplier>();
            foreach (var supplier in suppliers)
            {
                var existingSupplier = existingAOSuppliers.FirstOrDefault(s => s.SupplierID == supplier.SupplierID);
                
                if (existingSupplier != null && supplier.Equals(existingSupplier))
                    continue;

                if (!supplier.HasUniqueIdentifier())
                    continue;

                if (existingSupplier == null)
                    suppliersToAdd.Add(supplier);
                else
                    suppliersToUpdate.Add(supplier);
            }

            var batchResult = new ActionResult();
            batchResult.StrDict = new Dictionary<int, string>();
            foreach (var supplier in suppliersToAdd)
            {
                var addResult = connector.AddSupplier(supplier, true).ToActionResult();
                if (!addResult.Success)
                    batchResult.StrDict.Add(int.Parse(supplier.SupplierID), addResult.ErrorMessage);

                batchResult.ObjectsAffected += addResult.ObjectsAffected;
            }
            foreach (var supplier in suppliersToUpdate)
            {
                var updateResult = connector.UpdateSupplier(supplier, true).ToActionResult();
                if (!updateResult.Success)
                    batchResult.StrDict.Add(int.Parse(supplier.SupplierID), updateResult.ErrorMessage);
                
                batchResult.ObjectsAffected += updateResult.ObjectsAffected;
            }

            if (batchResult.StrDict.Count > 0)
            {
                batchResult.ErrorMessage = batchResult.StrDict
                    .Aggregate(new StringBuilder(), (sb, kvp) => sb.AppendLine($"{kvp.Key}: {kvp.Value}"))
                    .ToString();
                batchResult.Success = false;
            }

            return batchResult;
        }

        public ActionResult BookkeepInvoice(string documentId, SupplierInvoiceDTO invoice, List<AccountingRowDTO> accountingRows)
        {
            var aoInvoice = invoice.ToAOInvoice(accountingRows);
            var connector = new FileConnector(_companyApiKey);
            return connector.BookkeepSupplierInvoice(documentId, aoInvoice).ToActionResult();
        }

        public AOResponseWrapper<AOSupplierInvoice> ExtractInvoice(string fileId)
        {
            var connector = new FileConnector(_companyApiKey);
            return connector.ExtractSupplierInvoice(fileId);
        }
    }
}
