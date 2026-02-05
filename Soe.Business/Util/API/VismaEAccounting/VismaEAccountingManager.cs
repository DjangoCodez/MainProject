using SoftOne.Soe.Business.Util.API.Shared;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util.API.VismaEAccounting
{
    public class VismaEAccountingIntegrationManager : IExternalInvoiceSystem
    {
        public string Name { get; } = "Visma";
        private VismaEAccountingConnector _connector;
        private VismaCompanySettings _settings { get; set; }
        private List<VismaUnit> _units { get; set; }
        private HashSet<string> _updatedProductNumbers = new HashSet<string>();
        private List<VismaArticleAccountCoding> _accountCodings { get; set; }
        private List<VismaTermsOfPayment> _termsOfPayments { get; set; }
        private List<VismaArticle> _articles { get; set; }
        public ExternalInvoiceSystemParameters Params { get; }
        public VismaEAccountingIntegrationManager()
        {
            _connector = new VismaEAccountingConnector();
            Params = new ExternalInvoiceSystemParameters()
            {
                Feature = Feature.Billing_Invoice_Invoices_Edit_ExportVismaEAccounting,
                DistributionStatusType = TermGroup_EDistributionType.VismaEAccounting,
                RefreshTokenStoragePoint = CompanySettingType.BillingVismaEAccountingRefreshToken,
                LastSyncStoragePoint = CompanySettingType.BillingVismaEAccountingLastSync,
            };
        }
        public string GetActivationUrl(string finalCallbackDestination)
        {
            return VismaEAccountingConnector.GetActivationUrl(finalCallbackDestination);
        } 
        public void SetAuthFromRefreshToken(string refreshToken)
        {
            _connector.SetAuthToken(refreshToken);
        }
        public void SetAuthFromCode(string code)
        {
            _connector.SetAuthToken(code, true);
        }
        public string GetRefreshToken()
        {
            return _connector.GetRefreshToken();
        }
        public ActionResult AddInvoice(CustomerInvoiceDistributionDTO invoiceDto, CustomerDistributionDTO customerDto, List<CustomerInvoiceRowDistributionDTO> invoiceRows)
        {
            var result = new ActionResult();

            if (!TrySetupCache())
                return new ActionResult("Could not load cache value");

            result = CheckArticles(invoiceRows, invoiceDto.IsContractorVat());
            if (!result.Success)
                return result;

            result = CheckCustomer(invoiceDto, customerDto, out VismaCustomer customer);
            if (!result.Success)
                return result;

            result = CreateInvoice(invoiceDto, customerDto, invoiceRows, out VismaCustomerInvoice invoice);
            if (!result.Success)
                return result;

            invoice.SetCustomer(customer);
            var response = _connector.CreateNewInvoice(invoice);
            if (!response.Success)
                return new ActionResult($"Fel vid skapande av faktura: {response.Error.UserErrorMessage}");
            else
                result.StringValue = response.Data.InvoiceNumber.ToString();

            return result;
        }

        private ActionResult CreateInvoice(CustomerInvoiceDistributionDTO invoiceDto, CustomerDistributionDTO customerDto, List<CustomerInvoiceRowDistributionDTO> invoiceRowDtos, out VismaCustomerInvoice invoice)
        {
            ActionResult result;
            invoice = new VismaCustomerInvoice(invoiceDto, customerDto);

            if (!SetTermsOfPaymentId(invoice, invoiceDto, out result))
                return result;

            result = SetHouseholdDeductionValues(invoice, invoiceRowDtos);
            if (!result.Success)
                return result;

            result = CreateInvoiceRows(invoiceRowDtos, customerDto.IsPrivatePerson, out List<VismaCustomerInvoiceRow> invoiceRows);
            if (!result.Success)
                return result;
            invoice.SetInvoiceRows(invoiceRows);

            result = CreateHouseholdDeductionPersons(invoiceRowDtos, out List<VismaHouseholdDeductionPerson> persons);
            if (!result.Success)
                return result;
            invoice.SetPersons(persons);

            return new ActionResult();
        }
        private bool TrySetupCache()
        {
            if (_settings == null)
            {
                var response = _connector.GetSettings();
                if (!response.Success)
                    return false;
                _settings = response.Data;
            }
            if (_units == null)
                _units = _connector.GetAllUnits();
            if (_termsOfPayments == null)
                _termsOfPayments = _connector.GetAllTermsOfPayment();
            if (_accountCodings == null)
                _accountCodings = _connector.GetAllAccountCodings();
            if (_articles == null)
                _articles = _connector.GetAllArticles();

            return true;
        }
        private VismaResponseWrapper<VismaArticle> SaveArticle(VismaArticle article)
        {
            var newArticle = article.Id == null;
            var response = newArticle ?
                _connector.CreateNewArticle(article) :
                _connector.UpdateArticle(article);

            if (!response.Success)
                return response;

            var idx = _articles.FindIndex(a => a.Id == response.Data.Id);
            if (idx != -1)
                _articles[idx] = response.Data;
            else
                _articles.Add(response.Data);

            return response;
        }

        public void AddCustomer(CustomerDistributionDTO customer, CustomerDTO customerInput)
        {
            try
            {
                var referenceCustomer = new VismaCustomer(customer, null, customerInput);
                var paymentConditionDays = 30; //default 'Terms of payment' as 'Net 30 days'
                var existing = _connector.GetCustomerByNumber(customerInput.CustomerNr);
                _termsOfPayments = _connector.GetAllTermsOfPayment();
                var termsOfPayment = _termsOfPayments.FirstOrDefault(t => t.NumberOfDays == paymentConditionDays && t.TermsOfPaymentTypeId == 0);

                if (existing != null)
                    referenceCustomer.SetId(existing);
                referenceCustomer.TermsOfPaymentId = referenceCustomer.Id == null ? termsOfPayment.Id : existing.TermsOfPaymentId;
                SaveCustomer(referenceCustomer);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Visma customer sync failed: {ex.Message}");
            }
        }

        private VismaResponseWrapper<VismaCustomer> SaveCustomer(VismaCustomer customer)
        {
                var newCustomer = customer.Id == null;
                return newCustomer ?
                    _connector.CreateNewCustomer(customer) :
                    _connector.UpdateCustomer(customer);
            }
        private bool SetTermsOfPaymentId(VismaCustomer customer, CustomerInvoiceDistributionDTO invoice, out ActionResult result)
        {
            result = new ActionResult();
            var termsOfPayment = _termsOfPayments.FirstOrDefault(t => t.NumberOfDays == invoice.PaymentConditionDays && t.TermsOfPaymentTypeId == 0);

            if (termsOfPayment == null)
            {
                result = new ActionResult($"Betalningsvillkor med {invoice.PaymentConditionDays} dagar kunde inte hittas.");
                return false;
            }
            customer.SetTermsOfPayment(termsOfPayment);
            return true;
        }
        private bool SetTermsOfPaymentId(VismaCustomerInvoice invoice, CustomerInvoiceDistributionDTO invoiceDto, out ActionResult result)
        {
            result = new ActionResult();
            var termsOfPayment = _termsOfPayments.FirstOrDefault(t => t.NumberOfDays == invoiceDto.PaymentConditionDays && t.TermsOfPaymentTypeId == 0);

            if (termsOfPayment == null)
            {
                result = new ActionResult($"Betalningsvillkor med {invoiceDto.PaymentConditionDays} dagar kunde inte hittas.");
                return false;
            }
            invoice.SetTermsOfPayment(termsOfPayment);
            return true;
        }

        private bool SetUnitId(VismaArticle article, CustomerInvoiceRowDistributionDTO productRow, out ActionResult result)
        {
            result = new ActionResult();
            var unitAbbreviation = productRow.Unit.ToLower();
            var unit = _units.FirstOrDefault(t =>
                unitAbbreviation == t.Name.ToLower() ||
                unitAbbreviation == t.Abbreviation.ToLower() ||
                unitAbbreviation == t.AbbreviationEnglish.ToLower() ||
                unitAbbreviation == t.Code.ToLower());

            // Default to pieces.
            if (unit == null)
                unit = _units.FirstOrDefault(t => t.Abbreviation.ToLower() == "st" || t.AbbreviationEnglish.ToLower() == "pcs");

            if (unit == null)
            {
                result = new ActionResult($"Enheten {unitAbbreviation} kunde inte hittas.");
                return false;
            }
            article.SetUnit(unit);
            return true;
        }
        //private bool SetArticleId(VismaCustomerInvoiceRow row)

        private bool SetAccountCodingId(VismaArticle article, CustomerInvoiceRowDistributionDTO product, bool isReverseVat, out ActionResult result)
        {
            VismaArticleAccountCoding accountCoding = FindCoding(product, isReverseVat);

            if (accountCoding == null)
            {
                result = new ActionResult($"Artikelkontering för artikel med nummer {product.Product.Number} kunde inte hittas.");
                return false;
            }

            article.SetAccountCoding(accountCoding);
            result = new ActionResult();
            return true;
        }

        private VismaArticleAccountCoding FindCoding(CustomerInvoiceRowDistributionDTO product, bool isReverseVat)
        {
            // VatType 1 => Material
            // VatType 2 => Service
            // Special handling for contractor/reverse vat since you cannot set that type of Account on codings with 0% vat. In Visma the VAT% is linked to the product.
            if (isReverseVat)
            {
                var vatRate = 0.25m;
                return product.Product.VatType == 2 ?
                    _accountCodings.FirstOrDefault(a => a.Type == "Service" && a.VatRatePercent == vatRate && a.DomesticSalesSubjectToReversedConstructionVatAccountNumber.HasValue) :
                    _accountCodings.FirstOrDefault(a => a.Type == "Goods" && a.VatRatePercent == vatRate && a.NameEnglish.ToLower().StartsWith("goods") && a.DomesticSalesSubjectToReversedConstructionVatAccountNumber.HasValue);
            }
            else
            {
                var vatRate = product.VatRate / 100;
                return product.Product.VatType == 2 ?
                    _accountCodings.FirstOrDefault(a => a.Type == "Service" && a.VatRatePercent == vatRate) :
                    _accountCodings.FirstOrDefault(a => a.Type == "Goods" && a.VatRatePercent == vatRate && a.NameEnglish.ToLower().StartsWith("goods"));
            }
        }

        private ActionResult CheckArticles(List<CustomerInvoiceRowDistributionDTO> rows, bool isReverseVat)
        {
            ActionResult result;

            foreach (var row in rows.Where(r => !r.IsTextRow))
            {
                if (_updatedProductNumbers.Contains(row.Product.Number) && !isReverseVat)
                    continue;

                var existingProduct = _articles.FirstOrDefault(a => a.Number == row.Product.Number);

                var referenceArticle = new VismaArticle(row);

                if (existingProduct != null)
                    referenceArticle.SetId(existingProduct);

                if (!SetAccountCodingId(referenceArticle, row, isReverseVat, out result))
                    return result;

                if (!SetUnitId(referenceArticle, row, out result))
                    return result;

                if (existingProduct == null || !existingProduct.Equals(referenceArticle))
                {
                    var response = SaveArticle(referenceArticle);
                    if (!response.Success)
                        return new ActionResult($"Fel vid uppdatering av artikel med nummer {row.Product.Number}: {response.Error.UserErrorMessage}");
                    _updatedProductNumbers.Add(row.Product.Number);
                }
            }

            return new ActionResult();
        }

        private ActionResult CheckCustomer(CustomerInvoiceDistributionDTO invoiceDto, CustomerDistributionDTO customerDto, out VismaCustomer customer)
        {
            //Out
            customer = null;

            var existing = _connector.GetCustomerByNumber(invoiceDto.ActorNr);
            var referenceCustomer = new VismaCustomer(customerDto, invoiceDto, null);

            if (existing != null)
            {
                referenceCustomer.SetId(existing);
                customer = existing;
            }

            if (!SetTermsOfPaymentId(referenceCustomer, invoiceDto, out var result))
                return result;

            if (existing == null || !referenceCustomer.Equals(existing))
            {
                var response = SaveCustomer(referenceCustomer);
                if (!response.Success)
                    return new ActionResult($"Fel vid uppdatering av kund med nummer {invoiceDto.ActorNr}: {response.Error.UserErrorMessage}");
                customer = response.Data;
            }

            return new ActionResult();
        }

        private ActionResult CreateInvoiceRows(List<CustomerInvoiceRowDistributionDTO> invoiceRowDtos, bool isPrivatePerson, out List<VismaCustomerInvoiceRow> invoiceRows)
        {
            var result = new ActionResult();
            invoiceRows = new List<VismaCustomerInvoiceRow>();

            bool invoiceHasHouseHoldDeduction = invoiceRowDtos.Any(r => r.HousholdDeductionRow != null);

            foreach (var rowDto in invoiceRowDtos)
            {
                if (InvoiceIntegrationUtility.IsDeductionTextRow(rowDto)) continue;
                if (rowDto.HousholdDeductionRow != null) continue;

                var newRow = new VismaCustomerInvoiceRow(rowDto, invoiceHasHouseHoldDeduction, isPrivatePerson);
                //ROT value!
                //Set ArticleId
                if (!rowDto.IsTextRow)
                {
                    var article = _articles.FirstOrDefault(a => a.Number == rowDto.Product.Number);
                    if (article == null) return new ActionResult($"Artikel med nummer {rowDto.Product.Number} kunde inte hittas.");
                    newRow.SetArticle(article);
                }
                invoiceRows.Add(newRow);
            }

            return result;
        }

        private ActionResult CreateHouseholdDeductionPersons(List<CustomerInvoiceRowDistributionDTO> invoiceRows, out List<VismaHouseholdDeductionPerson> persons)
        {
            persons = new List<VismaHouseholdDeductionPerson>();
            foreach (var item in invoiceRows.Where(p => p.HousholdDeductionRow != null))
            {
                persons.Add(new VismaHouseholdDeductionPerson(item.HousholdDeductionRow));
            }
            return new ActionResult();
        }

        private ActionResult SetHouseholdDeductionValues(VismaCustomerInvoice invoice, List<CustomerInvoiceRowDistributionDTO> rowDtos)
        {
            var houseHoldDeductionRows = rowDtos.Where(r => r.HousholdDeductionRow != null).ToList();

            if (!houseHoldDeductionRows.Any())
                return new ActionResult();

            var houseHoldDeductionRow = houseHoldDeductionRows.First();
            var houseHoldInformation = houseHoldDeductionRow.HousholdDeductionRow;
            var houseHoldDeductionType = houseHoldDeductionRow.Product.HouseholdDeductionType;

            if (houseHoldDeductionType == 0)
            {
                var otherRow = rowDtos.FirstOrDefault(r => r.Product?.HouseholdDeductionType != 0);
                houseHoldDeductionType = otherRow.Product.HouseholdDeductionType;
            }

            if (VismaUtility.IsGreenWork(houseHoldDeductionType))
            {
                //Set green work values
                invoice.SetGreenWorkValues(
                    InvoiceIntegrationUtility.GetPropertyDesignation(houseHoldInformation),
                    houseHoldInformation.CooperativeOrgNr);
                invoice.SetOtherCosts(rowDtos);
            }
            else
            {
                //Set rot/rut values
                var type = VismaUtility.GetHouseworkDeductionType(houseHoldDeductionType);
                invoice.SetRotRutValues(
                    type,
                    InvoiceIntegrationUtility.GetPropertyDesignation(houseHoldInformation),
                    houseHoldInformation.CooperativeOrgNr);
            }

            return new ActionResult();
        }
    }
}
