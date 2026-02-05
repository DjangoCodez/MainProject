using Newtonsoft.Json;
using RestSharp;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SoftOne.Soe.Business.Util.API.Utility;
using SoftOne.Soe.Business.Util.API.Shared;
using System.Text.RegularExpressions;

namespace SoftOne.Soe.Business.Util.API.Fortnox
{
    public class FortnoxConnector : IExternalInvoiceSystem
    {
        public string Name { get; } = "Fortnox";
        private const string ProdUrl = "https://api.fortnox.se/3/";
        private const string AuthUrl = "https://apps.fortnox.se/";
        private const string CallbackHandler = "https://devbridge.softone.se/api/fortnox/callback";

        private const string ClientId = "fddA51DNmIxT";
        private const string ClientSecret = "JUcfDYUZxW";
        private string _credentials
        {
            get
            {
                var plainTextBytes = Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}");
                return Convert.ToBase64String(plainTextBytes);
            }
        }

        private RateLimitChecker _checker = new RateLimitChecker(5, 25); //25 requests per 5 seconds
        private FortnoxAccessToken _token { get; set; }
        private ArticlesCache _articlesCache { get; set; }
        private HashSet<string> _updatedProducts { get; set; } = new HashSet<string>();
        public ExternalInvoiceSystemParameters Params { get; }
        public FortnoxConnector() {
            Params = new ExternalInvoiceSystemParameters()
            {
                Feature = Feature.Billing_Invoice_Invoices_Edit_ExportFortnox,
                DistributionStatusType = TermGroup_EDistributionType.Fortnox,
                RefreshTokenStoragePoint = CompanySettingType.BillingFortnoxRefreshToken,
                LastSyncStoragePoint = CompanySettingType.BillingFortnoxLastSync,
            };
            _articlesCache = new ArticlesCache(this);
        }

        public static string GetActivationUrl(string finalCallbackDestination)
        {
            //Since Fortnox only allows pre-defined callback URL:s, we use Bridge as their point of contact.
            //Bridge decodes the state parameter and redirects the user there with the Fortnox auth code appended as a query param.
            var plainTextBytes = Encoding.UTF8.GetBytes(finalCallbackDestination);
            var base64Encoded = Convert.ToBase64String(plainTextBytes);

            return $"https://apps.fortnox.se/oauth-v1/auth?client_id={ClientId}&redirect_uri={CallbackHandler}&scope=invoice%20customer%20article&state={base64Encoded}&access_type=offline&response_type=code";
        }

        private void SetLoginToken(string refreshToken, bool isCode = false)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return;
            }

            var client = new GoRestClient(AuthUrl);
            var request = new RestRequest("/oauth-v1/token", Method.Post);
            request.AddHeader("Authorization", $"Basic {_credentials}");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");

            if (isCode)
            {
                request.AddParameter("application/x-www-form-urlencoded", $"grant_type=authorization_code&code={refreshToken}&redirect_uri={CallbackHandler}", ParameterType.RequestBody);
            }
            else
            {
                request.AddParameter("application/x-www-form-urlencoded", $"grant_type=refresh_token&refresh_token={refreshToken}", ParameterType.RequestBody);
            }

            var response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var token = JsonConvert.DeserializeObject<FortnoxAccessToken>(response.Content);
                this._token = token;
            }
        }
        public void SetAuthFromRefreshToken(string refreshToken)
        {
            SetLoginToken(refreshToken);
        }
        public void SetAuthFromCode(string code)
        {
            SetLoginToken(code, true);
        }
        public string GetRefreshToken()
        {
            return _token?.Refresh_token;
        }
        public T MakeRequest<T>(string endpoint, Method method = Method.Get, object body = null)
        {
            if (_checker.WaitBeforeRequest(out int waitTime))
                Thread.Sleep(waitTime);

            var client = new GoRestClient(ProdUrl);
            var request = new RestRequest(endpoint, method);

            request.AddHeader("Authorization", $"Bearer {_token.Access_token}");
            request.AddHeader("Client-Secret", ClientSecret);
            request.AddHeader("Accept", "application/json");
            request.RequestFormat = DataFormat.Json;

            if (body != null)
            {
                request.AddHeader("Content-type", "application/json; charset=utf-8");
                request.AddJsonBody(JsonConvert.SerializeObject(body));
            }

            try
            {
                var response = client.Execute(request);
                // Our rate limit checker is not enough.
                // I have not been able to understand why it's not working,
                // But I suppose this is at least as good.
                if ((int)response.StatusCode == 429)
                {
                    var header = response.Headers.FirstOrDefault(h => h.Name.ToLower() == "retry-after");
                    if (int.TryParse(header?.Value?.ToString(), out int retryAfter))
                    {
                        Thread.Sleep((retryAfter + 1) * 1000);
                        response = client.Execute(request); //Retry the request
                    }
                }

                var result = JsonConvert.DeserializeObject<T>(response.Content);
                if (result == null)
                {
                    throw new Exception($"Unexpected null response");
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during request. Endpoint: {endpoint}, method: {method}, body: {body ?? string.Empty}, ex: {ex.Message}");
            }
        }
        public ActionResult AddInvoice(CustomerInvoiceDistributionDTO invoice, CustomerDistributionDTO customer, List<CustomerInvoiceRowDistributionDTO> invoiceRows)
        {
            var result = new ActionResult();

            result = _articlesCache.Init();
            if (!result.Success)
                return result;

            result = CheckForUpdatedArticles(invoiceRows);
            if (!result.Success)
                return result;


            result = SaveCustomer(invoice, customer);
            if (!result.Success)
                return result;
            
            return TryCreateInvoice(invoice, customer, invoiceRows);
        }

        public ActionResult CheckForUpdatedArticles(List<CustomerInvoiceRowDistributionDTO> rows)
        {
            var changedRows = new List<CustomerInvoiceRowDistributionDTO>();

            foreach (var row in rows.Where(r => r.Product != null))
            {
                var product = row.Product;
                if (product.Number == null)
                    continue;
                product.Number = FortnoxUtility.CleanInvalidChars(product.Number);
                product.Name = FortnoxUtility.CleanInvalidChars(product.Name);

                var actionResult = _articlesCache.GetArticle(product.Number, out FortnoxArticle existingProduct);
                //Removing this for now. Not sure what happens when we request a product that does not exist.
                //if (!actionResult.Success)
                //    return actionResult;
                
                if (existingProduct == null) continue;

                if (!existingProduct.Equals(row))
                {
                    changedRows.Add(row);
                }
            }

            return TryUpdateArticles(changedRows);
        }
        private ActionResult TryUpdateArticles(List<CustomerInvoiceRowDistributionDTO> rows)
        {
            var result = new ActionResult();

            foreach (var productRow in rows)
            {
                var article = new FortnoxArticleWrapper()
                {
                    Article = new FortnoxArticle(productRow),
                };

                var wrapper = MakeRequest<FortnoxResponseWrapper>($"articles/{productRow.Product.Number}", Method.Put, article);

                if (IsError(wrapper, out result))
                    return result;

                _updatedProducts.Add(productRow.Product.Number);
                _articlesCache.TryUpdateArticle(wrapper.Article);
            }

            return result;
        }

        public void AddCustomer(CustomerDistributionDTO customer, CustomerDTO customerInput)
        {
            var customerWrapper = MakeRequest<FortnoxResponseWrapper>($"customers/{customerInput.CustomerNr}");
            ActionResult result = null;
            var fnCustomer = new FortnoxCustomerWrapper()
            {
                Customer = new FortnoxCustomer(customer, null, customerInput),
            };

            if (customerWrapper.Customer == null)
            {
                //New customer
                result = TryPostCustomer(fnCustomer);
            }
            else if (!customerWrapper.Customer.CustomerDetailsEquals(customer, customerInput))
            {
                //Edit existing customer.
                result = TryUpdateCustomer(customerInput.CustomerNr, fnCustomer);
            }
            if (!result.Success)
            {
                throw new InvalidOperationException($"Fortnox customer sync failed: {result?.ErrorMessage}");
            }
        }

        private ActionResult SaveCustomer(CustomerInvoiceDistributionDTO invoice, CustomerDistributionDTO customer)
        {
            var customerWrapper = MakeRequest<FortnoxResponseWrapper>($"customers/{invoice.ActorNr}");
            var fnCustomer = new FortnoxCustomerWrapper()
            {
                Customer = new FortnoxCustomer(customer, invoice, null),
            };

            if (customerWrapper.Customer == null)
            {
                //Assume customer has never been created.
                return TryPostCustomer(fnCustomer);
            }
            else if (customerWrapper.Customer.IsChanged(customer, invoice, null))
            {
                //Edit existing customer.
                return TryUpdateCustomer(invoice.ActorNr, fnCustomer);
            }
            else
            {
                return new ActionResult();
            }
        }

        private ActionResult TryPostCustomer(FortnoxCustomerWrapper fnCustomer)
        {
            var createCustomerWrapper = MakeRequest<FortnoxResponseWrapper>("customers", Method.Post, fnCustomer);
            return CheckError(createCustomerWrapper);
        }

        private ActionResult TryUpdateCustomer(string actorNr, FortnoxCustomerWrapper fnCustomer)
        {
            var wrapper = MakeRequest<FortnoxResponseWrapper>($"customers/{actorNr}", Method.Put, fnCustomer);
            return CheckError(wrapper);
        }

        private ActionResult TryCreateProductIfNotExists(CustomerInvoiceRowDistributionDTO invoiceRow)
        {
            var product = invoiceRow.Product;
            var result = new ActionResult();
            if (_articlesCache.ArticleExists(product.Number))
            {
                return result;
            }

            var fnArticle = new FortnoxArticleWrapper()
            {
                Article = new FortnoxArticle(invoiceRow),
            };

            var wrapper = MakeRequest<FortnoxResponseWrapper>("articles", Method.Post, fnArticle);

            if (IsError(wrapper, out result))
                return result;

            _articlesCache.TryUpdateArticle(wrapper.Article);
            _updatedProducts.Add(product.Number);

            return result;
        }
        private ActionResult TryCreateInvoice(CustomerInvoiceDistributionDTO invoice, CustomerDistributionDTO customer, List<CustomerInvoiceRowDistributionDTO> invoiceRows)
        {
            bool isCredit = invoice.TotalAmount < 0;
            var fnInvoice = new FortnoxInvoice();

            var result = TryCreateInvoiceRows(fnInvoice, invoiceRows, isCredit);

            if (!result.Success)
                return result;

            fnInvoice.SetProps(invoice, customer);

            var fnInvoiceWrapped = new FortnoxInvoiceWrapper()
            {
                Invoice = fnInvoice,
            };
           
            var wrapper = MakeRequest<FortnoxResponseWrapper>("invoices", Method.Post, fnInvoiceWrapped);

            if (IsError(wrapper, out result))
                return result;

            var createdInvoice = wrapper.Invoice;
            string invoiceNumber = createdInvoice.DocumentNumber;

            if (!isCredit)
                result = TryCreateTaxReductionRows(createdInvoice, invoiceRows);

            result.BooleanValue = true; //Created invoice
            result.StringValue = invoiceNumber;

            return result;
        }
        private ActionResult TryCreateInvoiceRows(FortnoxInvoice fnInvoice, List<CustomerInvoiceRowDistributionDTO> invoiceRows, bool isCredit)
        {
            fnInvoice.InvoiceRows = new List<FortnoxInvoiceRow>();

            string hhdType = null;

            var hasHouseWork = invoiceRows.Any(r => r.HousholdDeductionRow != null && r.HousholdDeductionRow.Amount > 0);

            foreach (var row in invoiceRows.Where(r => r.HousholdDeductionRow == null))
            {
                row.Text = FortnoxUtility.CleanInvalidChars(row.Text);
                if (row.IsTextRow)
                {
                    //Text rows
                    if (InvoiceIntegrationUtility.IsDeductionTextRow(row))
                        continue;

                    var fnRow = new FortnoxInvoiceRow(row.Text);
                    fnInvoice.InvoiceRows.Add(fnRow);
                }
                else if (row.Product != null)
                {
                    //Product rows
                    var product = row.Product;
                    product.Name = FortnoxUtility.CleanInvalidChars(product.Name);
                    product.Number = FortnoxUtility.CleanInvalidChars(product.Number);
                    var result = TryCreateProductIfNotExists(row);
                    if (!result.Success)
                        return result;

                    var fnRow = new FortnoxInvoiceRow(row, hasHouseWork);

                    if (hasHouseWork && fnRow.HouseWorkType != null)
                    {
                        if (hhdType == null)
                        {
                            //We need to set the correct deduction type of the invoice
                            var hhdWorkType = (SysHouseholdDeductionWorkTypes)row.Product.HouseholdDeductionType;
                            hhdType = FortnoxUtility.GetHouseworkDeductionType(hhdWorkType);
                        }
                    }

                    if (row.SumAmountCurrency < 0)
                    {
                        //Credit rows should have positive price, negative quantity
                        if (fnRow.DeliveredQuantity > 0) fnRow.DeliveredQuantity *= -1;
                        if (fnRow.Price < 0) fnRow.Price *= -1;
                    }

                    if (isCredit)
                    {
                        fnRow.HouseWork = false;
                    }

                    fnInvoice.InvoiceRows.Add(fnRow);
                }
            }
            fnInvoice.TaxReductionType = hhdType ?? string.Empty;
            return new ActionResult();
        }
        private ActionResult TryCreateTaxReductionRows(FortnoxInvoice fnInvoice, List<CustomerInvoiceRowDistributionDTO> invoiceRows)
        {
            foreach (var row in invoiceRows.Where(r => r.HousholdDeductionRow != null))
            {
                var hhdRow = row.HousholdDeductionRow;
                var fnRow = new FortnoxTaxReductionWrapper()
                {
                    TaxReduction = new FortnoxTaxReduction(fnInvoice, row),
                };
                var wrapper = MakeRequest<FortnoxResponseWrapper>("taxreductions", Method.Post, fnRow);
                if (IsError(wrapper, out ActionResult result))
                    return result;
            }
            return new ActionResult();
        }
        private bool IsError(FortnoxResponseWrapper wrapper, out ActionResult result)
        {
            result = new ActionResult();
            if (wrapper.ErrorInformation != null)
            {
                result = new ActionResult(false, 
                    wrapper.ErrorInformation?.Code ?? 0, 
                    wrapper.ErrorInformation?.Message ?? "Invalid error message");
                return true;
            }
            return false;
        }

        private ActionResult CheckError(FortnoxResponseWrapper wrapper)
        {
            IsError(wrapper, out ActionResult result);
            return result;
        }
    }

    class ArticlesCache
    {
        public List<FortnoxArticle> _articles { get; set; } = new List<FortnoxArticle>();
        public FortnoxConnector _connector { get; set; }
        private bool _didLoadAll { get; set; } = false;
        private ActionResult _result = null;
        private bool _initialized { 
            get
            {
                return _result != null;
            } 
        }

        public ArticlesCache(FortnoxConnector connector)
        {
            _connector = connector;
        }
        
        public ActionResult Init()
        {
            if (_initialized)
                return _result;

            _result = TryGetAllArticles(out List<FortnoxArticle> articles);
            _articles = articles;
            return _result;
        }

        public bool ArticleExists(string articleNumber)
        {
            return _articles.Any(a => a.ArticleNumber == articleNumber);
        }
        public void AddArticle(FortnoxArticle article)
        {
            if (!TryUpdateArticle(article))
                _articles.Add(article);
        }
        public ActionResult GetArticle(string articleNumber, out FortnoxArticle article)
        {
            var actionResult = new ActionResult();

            article = _articles.FirstOrDefault(a => a.ArticleNumber == articleNumber);
            if (article == null && !_didLoadAll)
            {
                actionResult = TryGetArticle(articleNumber, out article);
                if (article != null)
                {
                    _articles.Add(article);
                }
            }
            return actionResult;
        }
        public bool TryUpdateArticle(FortnoxArticle article)
        {
            var index = _articles.FindIndex(a => a.ArticleNumber == article.ArticleNumber);
            if (index != -1)
            {
                _articles[index] = article;
                return true;
            }
            return false;
        }

        public ActionResult TryGetAllArticles(out List<FortnoxArticle> articles)
        {
            _didLoadAll = false;
            int pageLimit = 10;

            var wrapper = _connector.MakeRequest<FortnoxResponseWrapper>("articles?limit=500");

            if (IsError(wrapper, out ActionResult result))
            {
                articles = new List<FortnoxArticle>();
                return result;
            }

            articles = wrapper.Articles;
            var meta = wrapper.MetaInformation;
            int totalPages = meta.TotalPages;
            int currentPage = meta.CurrentPage;

            while (currentPage < totalPages)
            {
                currentPage++;

                wrapper = _connector.MakeRequest<FortnoxResponseWrapper>($"articles?limit=500&page={currentPage}");

                if (IsError(wrapper, out result))
                    return result;

                if (wrapper.Articles != null)
                {
                    articles = articles.Concat(wrapper.Articles).ToList();
                }

                if (currentPage >= pageLimit)
                {
                    return new ActionResult();
                }
            }

            _didLoadAll = true;
            return new ActionResult();
        }

        public ActionResult TryGetArticle(string articleNumber, out FortnoxArticle article)
        {
            var wrapper = _connector.MakeRequest<FortnoxResponseWrapper>($"articles/{articleNumber}");
            if (IsError(wrapper, out ActionResult result))
            {
                article = null;
                return result;
            }
            article = wrapper.Article;
            return new ActionResult();
        }

        private bool IsError(FortnoxResponseWrapper wrapper, out ActionResult result)
        {
            result = new ActionResult();
            if (wrapper.ErrorInformation != null)
            {
                result = new ActionResult(false, 
                    wrapper.ErrorInformation?.Code ?? 0, 
                    wrapper.ErrorInformation?.Message ?? "Invalid error message");
                return true;
            }
            return false;
        }
    }

    public class FortnoxAccessToken
    {
        public string Access_token { get; set; }
        public int Expires_in { get; set; }
        public string Refresh_token { get; set; }
        public string Token_type { get; set; }
        public string Scope { get; set; }
    }
    class ErrorInformation
    {
        public int Error { get; set; }
        public string Message { get; set; }
        public int Code { get; set; }
    }
    class MetaInformation
    {
        [JsonProperty("@TotalResources")]
        public int TotalResources { get; set; }
        [JsonProperty("@TotalPages")]
        public int TotalPages { get; set; }
        [JsonProperty("@CurrentPage")]
        public int CurrentPage { get; set; }
    }
    class FortnoxResponseWrapper 
    {
        public MetaInformation MetaInformation { get; set; }
        public ErrorInformation ErrorInformation { get; set; }
        public FortnoxArticle Article { get; set; }
        public List<FortnoxArticle> Articles { get; set; }
        public FortnoxCustomer Customer { get; set; }
        public List<FortnoxCustomer> Customers { get; set; }
        public FortnoxInvoice Invoice { get; set; }
        public FortnoxTaxReduction TaxReduction { get; set; }
    }
    class FortnoxArticleWrapper
    {
        public FortnoxArticle Article { get; set; }
    }
    class FortnoxArticlesWrapper
    {
        public List<FortnoxArticle> Articles { get; set; }
    }
    class FortnoxCustomerWrapper
    {
        public FortnoxCustomer Customer { get; set; }
    }
    class FortnoxCustomersWrapper
    {
        public List<FortnoxCustomer> Customers { get; set; }
    }
    class FortnoxInvoiceWrapper
    {
        public FortnoxInvoice Invoice { get; set; }
    }
    public class FortnoxTaxReductionWrapper
    {
        public FortnoxTaxReduction TaxReduction { get; set; }
    }
    class FortnoxArticle
    {
        public string ArticleNumber { get; set; }
        public string Description { get; set; }
        public FortnoxArticle() { }

        public FortnoxArticle(CustomerInvoiceRowDistributionDTO row)
        {
            ArticleNumber = row.Product.Number;
            Description = row.Product.Name;
        }

        public bool Equals(CustomerInvoiceRowDistributionDTO row)
        {
            return ArticleNumber == row.Product.Number && Description == row.Product.Name;
        }
    }
    class FortnoxCustomer
    {
        public string Name { get; set; }
        public string CustomerNumber { get; set; }
        public string Type { get; set; } // <- PRIVATE/COMPANY

        public string OrganisationNumber { get; set; }
        public string VATNumber { get; set; }
        public string VATType { get; set; }

        public string CountryCode { get; set; } // <- is 2 char country code
        public string DeliveryAddress1 { get; set; }
        public string DeliveryAddress2 { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryZipCode { get; set; }
        //public string DeliveryCountry { get; set; } // <- is readonly
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public string Email { get; set; }
        public string Phone1 { get; set; }
        public string Country { get; set; } // <- is readonly
        public FortnoxCustomer() {}
        public FortnoxCustomer(CustomerDistributionDTO customer, CustomerInvoiceDistributionDTO invoice, CustomerDTO customerInput)
        {
            CustomerNumber = invoice?.ActorNr;
            Name = invoice?.ActorName;
            OrganisationNumber = invoice?.ActorOrgNr;
            VATNumber = invoice?.ActorVatNr;
            VATType = invoice != null ? FortnoxUtility.GetVatType((TermGroup_InvoiceVatType)invoice.VatType) : null;
            Type = FortnoxUtility.PrivatePersonType(customer.IsPrivatePerson);
            Address1 = customer.BillingAddressStreet ?? string.Empty;
            Address2 = customer.BillingAddressCO ?? string.Empty;
            ZipCode = customer.BillingAddressPostalCode ?? string.Empty;
            City = customer.BillingAddressCity ?? string.Empty;
            CountryCode = customer.CountryCode ?? string.Empty;
            DeliveryAddress1 = customer.DeliveryAddressStreet ?? string.Empty;
            DeliveryAddress2 = customer.DeliveryAddressCO ?? string.Empty;
            DeliveryZipCode = customer.DeliveryAddressPostalCode ?? string.Empty;
            DeliveryCity = customer.DeliveryAddressCity ?? string.Empty;
            Email = customer.Email ?? string.Empty;
            Phone1 = customer.MobilePhone ?? string.Empty;

            if(customerInput != null)
            {
                CustomerNumber = customerInput.CustomerNr;
                Name = customerInput?.Name;
                OrganisationNumber = customerInput.OrgNr;
                VATNumber = customerInput.VatNr;
                VATType = FortnoxUtility.GetVatType(customerInput.VatType);
                Type = FortnoxUtility.PrivatePersonType(customerInput.IsPrivatePerson);
            }
        }

        public bool IsChanged(CustomerDistributionDTO customer, CustomerInvoiceDistributionDTO invoice, CustomerDTO customerInput)
        {
            return !InvoiceDetailsEquals(invoice) || !CustomerDetailsEquals(customer, customerInput);
        }
        public bool InvoiceDetailsEquals(CustomerInvoiceDistributionDTO invoice)
        {
            return CustomerNumber == invoice.ActorNr && 
                Name == invoice.ActorName &&
                OrganisationNumber == invoice.ActorOrgNr &&
                VATNumber == invoice.ActorVatNr;
        }
        public bool CustomerDetailsEquals(CustomerDistributionDTO customer, CustomerDTO customerInput)
        {
            return 
                Type == FortnoxUtility.PrivatePersonType(customer.IsPrivatePerson) && 
                Name == customerInput?.Name &&
                Address1 == customer.BillingAddressStreet &&
                Address2 == customer.BillingAddressCO &&
                ZipCode == customer.BillingAddressPostalCode &&
                City == customer.BillingAddressCity &&
                CountryCode == customer.CountryCode &&
                DeliveryAddress1 == customer.DeliveryAddressStreet &&
                DeliveryAddress2 == customer.DeliveryAddressCO &&
                DeliveryZipCode == customer.DeliveryAddressPostalCode &&
                DeliveryCity == customer.DeliveryAddressCity &&
                Email == customer.Email && 
                Phone1 == customer.MobilePhone;            
        }
    }
    public class FortnoxInvoice
    {
        public string DocumentNumber { get; set; } //Invoice number
        public string OCR { get; set; }

        //Customer
        public string CustomerNumber { get;set; }
        public string CustomerName { get; set; }

        //Amounts
        //public string Currency { get; set; } //SEK, EUR
        //public decimal CurrencyRate { get; set; }

        //Invoice details
        public string InvoiceDate { get; set; }
        public string DueDate { get; set; }

        //Rows
        public decimal AdministrationFee { get; set; }
        //public decimal AdministrationFeeVAT { get; set; }
        public decimal Freight { get; set; }
        //public decimal FreightVAT { get; set; }

        public List<FortnoxInvoiceRow> InvoiceRows { get; set; } = new List<FortnoxInvoiceRow>();

        //Address
        public string Address1 { get; set; } = string.Empty;
        public string Address2 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        public string DeliveryAddress1 { get; set; } = string.Empty;
        public string DeliveryAddress2 { get; set; } = string.Empty;
        public string DeliveryCity { get; set; } = string.Empty;
        public string DeliveryZipCode { get; set; } = string.Empty;
        public string DeliveryCountry { get; set; } = string.Empty;
        public string DeliveryName { get; set; } = string.Empty;

        public FortnoxEmailInformation EmailInformation { get; set; }

        //Terms
        public bool Sent { get; set; }
        public string TermsOfDelivery { get; set; } = string.Empty;
        public string TermsOfPayment { get; set; } = string.Empty; //"30"
        public string YourReference { get; set; } = string.Empty;
        public string OurReference { get; set; } = string.Empty;
        public string TaxReductionType { get; set; } = string.Empty; //rot/rut/green/none

        public string ExternalInvoiceReference1 { get; set; } = string.Empty;
        public string ExternalInvoiceReference2 { get; set; } = string.Empty;

        public string Comments { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;

        public void SetProps(CustomerInvoiceDistributionDTO invoice, CustomerDistributionDTO customer)
        {
            ExternalInvoiceReference1 = invoice.InvoiceId.ToString(); //If we by chance need to fetch a linked invoice in the future.

            CustomerName = invoice.ActorName;
            CustomerNumber = invoice.ActorNr;
            DocumentNumber = invoice.InvoiceNr ?? string.Empty;
            OCR = invoice.OCR ?? string.Empty;

            AdministrationFee = invoice.InvoiceFee;
            Freight = invoice.Freight;

            YourReference = invoice.ReferenceYour ?? string.Empty;
            OurReference = invoice.ReferenceOur ?? string.Empty;
            Comments = invoice.InternalDescription ?? string.Empty;
            Remarks = invoice.InvoiceText ?? string.Empty;

            InvoiceDate = FortnoxUtility.ParseDate(invoice.InvoiceDate);
            DueDate = FortnoxUtility.ParseDate(invoice.DueDate);
            TermsOfPayment = invoice.PaymentConditionCode ?? string.Empty;

            Address1 = customer.BillingAddressStreet ?? string.Empty;
            Address2 = customer.BillingAddressCO ?? string.Empty;
            City = customer.BillingAddressCity ?? string.Empty;
            ZipCode = customer.BillingAddressPostalCode ?? string.Empty;
            Country = customer.BillingCountry ?? string.Empty;

            DeliveryName = customer.DeliveryAddressName ?? string.Empty;
            DeliveryAddress1 = customer.DeliveryAddressStreet ?? string.Empty;
            DeliveryAddress2 = customer.DeliveryAddressCO ?? string.Empty;
            DeliveryCity = customer.DeliveryAddressCity ?? string.Empty;
            DeliveryZipCode = customer.DeliveryAddressPostalCode ?? string.Empty;
            DeliveryCountry = customer.DeliveryCountry ?? string.Empty;

            EmailInformation = new FortnoxEmailInformation()
            {
                EmailAddressTo = customer.Email ?? string.Empty,
            };
        }

    }
    public class FortnoxInvoiceRow
    {
        //public string AccountNumber { get; set; }
        public string ArticleNumber { get; set; }
        public string Description { get; set; }
        public decimal DeliveredQuantity { get; set; }
        public decimal Discount { get; set; }
        public string DiscountType { get; set; } //"PERCENT"/?
        public bool HouseWork { get; set; }
        //public int HouseWorkHoursToReport { get; set; } = 0;
        public string HouseWorkType { get; set; } 
        public decimal Price { get; set; }
        //public decimal Total { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal VAT { get; set; } //"25"
        public FortnoxInvoiceRow() { }
        public FortnoxInvoiceRow(CustomerInvoiceRowDistributionDTO row, bool hasHouseWork)
        {
            //Check if credit, we sometimes allow negative quantity...
            var hhdWorkType = (SysHouseholdDeductionWorkTypes)row.Product.HouseholdDeductionType;
            Price = row.AmountCurrency;
            ArticleNumber = row.Product.Number;
            Description = row.Text;
            Unit = row.Unit ?? string.Empty;
            DeliveredQuantity = row.Quantity;
            Discount = row.DiscountPercent;
            DiscountType = "PERCENT";
            VAT = row.VatRate;
            HouseWork = hasHouseWork ? FortnoxUtility.IsHouseWork(hhdWorkType, row) : false;
            HouseWorkType = hasHouseWork ? FortnoxUtility.GetHouseworkWorkType(hhdWorkType) : string.Empty;
        }
        public FortnoxInvoiceRow(string text)
        {
            //Text row.
            Description = text;
        }
    }
    public class FortnoxEmailInformation
    {
        public string EmailAddressTo { get; set; } = string.Empty;
    }
    public class FortnoxTaxReduction
    {
        public decimal AskedAmount { get; set; }
        public string CustomerName { get; set; }
        public string PropertyDesignation { get; set; }
        public string ReferenceDocumentType { get; set; }
        public string ReferenceNumber { get; set; }
        public string ResidenceAssociationOrganisationNumber { get; set; } //INVOICE, ORDER, OFFER
        public string SocialSecurityNumber { get; set; }
        public string TypeOfReduction { get; set; }
        public FortnoxTaxReduction() { }
        public FortnoxTaxReduction(FortnoxInvoice fnInvoice, CustomerInvoiceRowDistributionDTO invoiceRow)
        {
            var hhdRow = invoiceRow.HousholdDeductionRow;
            TypeOfReduction = FortnoxUtility.AdjustTypeOfReduction(fnInvoice.TaxReductionType);
            ReferenceNumber = fnInvoice.DocumentNumber;
            AskedAmount = hhdRow.Amount;
            ReferenceDocumentType = "INVOICE";
            PropertyDesignation = InvoiceIntegrationUtility.GetPropertyDesignation(hhdRow);
            ResidenceAssociationOrganisationNumber = hhdRow.CooperativeOrgNr ?? string.Empty;
            SocialSecurityNumber = hhdRow.SocialSecNr ?? string.Empty;
            CustomerName = hhdRow.Name ?? string.Empty;
        }
    }
    public static class FortnoxUtility
    {
        public static string GetHouseworkDeductionType(SysHouseholdDeductionWorkTypes type)
        {
            switch (type)
            {
                case SysHouseholdDeductionWorkTypes.Construction:
                case SysHouseholdDeductionWorkTypes.Electricity:
                case SysHouseholdDeductionWorkTypes.GlassMetalWork:
                case SysHouseholdDeductionWorkTypes.GroundDrainageWork:
                case SysHouseholdDeductionWorkTypes.Masonry:
                case SysHouseholdDeductionWorkTypes.PaintingWallPapering:
                case SysHouseholdDeductionWorkTypes.HVAC:
                    return "rot";
                case SysHouseholdDeductionWorkTypes.Cleaning:
                case SysHouseholdDeductionWorkTypes.TextileClothing:
                case SysHouseholdDeductionWorkTypes.SnowPlowing:
                case SysHouseholdDeductionWorkTypes.Gardening:
                case SysHouseholdDeductionWorkTypes.Babysitting:
                case SysHouseholdDeductionWorkTypes.OtherCare:
                case SysHouseholdDeductionWorkTypes.Tutoring:
                case SysHouseholdDeductionWorkTypes.MovingServices:
                case SysHouseholdDeductionWorkTypes.ITServices:
                case SysHouseholdDeductionWorkTypes.MajorApplianceRepair:
                case SysHouseholdDeductionWorkTypes.Cooking:
                case SysHouseholdDeductionWorkTypes.Furnishing:
                case SysHouseholdDeductionWorkTypes.HouseSupervision:
                case SysHouseholdDeductionWorkTypes.SalesTransport:
                case SysHouseholdDeductionWorkTypes.LaundryAtFacility:
                    return "rut";
                case SysHouseholdDeductionWorkTypes.NetworkConnectedSolarPower:
                case SysHouseholdDeductionWorkTypes.SystemForEnergyStorage:
                case SysHouseholdDeductionWorkTypes.ChargingForElectricalVehicle:
                    return "green";
                case SysHouseholdDeductionWorkTypes.OtherCost:
                default:
                    return null;

            }
        }
        public static string GetHouseworkWorkType(SysHouseholdDeductionWorkTypes type)
        {
            switch (type)
            {
                case SysHouseholdDeductionWorkTypes.Construction:
                    return "CONSTRUCTION";
                case SysHouseholdDeductionWorkTypes.Electricity:
                    return "ELECTRICITY";
                case SysHouseholdDeductionWorkTypes.GlassMetalWork:
                    return "GLASSMETALWORK";
                case SysHouseholdDeductionWorkTypes.GroundDrainageWork:
                    return "GROUNDDRAINAGEWORK";
                case SysHouseholdDeductionWorkTypes.Masonry:
                    return "MASONRY";
                case SysHouseholdDeductionWorkTypes.PaintingWallPapering:
                    return "PAINTINGWALLPAPERING";
                case SysHouseholdDeductionWorkTypes.HVAC:
                    return "HVAC";
                case SysHouseholdDeductionWorkTypes.Cleaning:
                    return "CLEANING";
                case SysHouseholdDeductionWorkTypes.TextileClothing:
                    return "TEXTILECLOTHING";
                case SysHouseholdDeductionWorkTypes.SnowPlowing:
                    return "SNOWPLOWING";
                case SysHouseholdDeductionWorkTypes.Gardening:
                    return "GARDENING";
                case SysHouseholdDeductionWorkTypes.Babysitting:
                    return "BABYSITTING";
                case SysHouseholdDeductionWorkTypes.OtherCare:
                    return "OTHERCARE";
                case SysHouseholdDeductionWorkTypes.Tutoring:
                    return "TUTORING";
                case SysHouseholdDeductionWorkTypes.OtherCost:
                    return "OTHERCOSTS";
                case SysHouseholdDeductionWorkTypes.MovingServices:
                    return "MOVINGSERVICES";
                case SysHouseholdDeductionWorkTypes.ITServices:
                    return "ITSERVICES";
                case SysHouseholdDeductionWorkTypes.MajorApplianceRepair:
                    return "MAJORAPPLIANCEREPAIR";
                case SysHouseholdDeductionWorkTypes.Cooking:
                    return "COOKING";
                case SysHouseholdDeductionWorkTypes.Furnishing:
                    return "FURNISHING";
                case SysHouseholdDeductionWorkTypes.HouseSupervision:
                    return "HOMEMAINTENANCE";
                case SysHouseholdDeductionWorkTypes.SalesTransport:
                    return "TRANSPORTATIONSERVICES";
                case SysHouseholdDeductionWorkTypes.LaundryAtFacility:
                    return "WASHINGANDCAREOFCLOTHING";
                case SysHouseholdDeductionWorkTypes.NetworkConnectedSolarPower:
                    return "SOLARCELLS";
                case SysHouseholdDeductionWorkTypes.SystemForEnergyStorage:
                    return "STORAGESELFPRODUCEDELECTRICITY";
                case SysHouseholdDeductionWorkTypes.ChargingForElectricalVehicle:
                    return "CHARGINGSTATIONELECTRICVEHICLE";
                default:
                    return "EMPTYHOUSEWORK";

            }
        }
        public static string GetVatType(TermGroup_InvoiceVatType vatType)
        {
            //FN also has EUREVERSEDVAT, EXPORT
            switch (vatType)
            {
                case TermGroup_InvoiceVatType.Contractor:
                    return "SEREVERSEDVAT";
                case TermGroup_InvoiceVatType.EU:
                    return "EUVAT";
                case TermGroup_InvoiceVatType.Merchandise:
                default:
                    return "SEVAT";
            }
        }
        public static bool IsHouseWork(SysHouseholdDeductionWorkTypes type, CustomerInvoiceRowDistributionDTO row)
        {
            if (row.Product.HouseholdDeductionType == 0)
                return false;

            if (row.Product.VatType == (int)SoeProductType.PayrollProduct)
                //Payroll product and has deduction type
                return true;


            return false;
        }
        public static string ParseDate(DateTime? date)
        {
            if (date == null)
                return string.Empty;
            return date.Value.ToString("yyyy-MM-dd");
        }
        public static int QuantityToMinutes(decimal quantity)
        {
            decimal minutes = quantity * 60;
            decimal.Round(minutes, 0);
            return decimal.ToInt32(minutes);
        }
        public static string AdjustTypeOfReduction(string type)
        {
            if (!string.IsNullOrEmpty(type) && type != "green")
            {
                type = type.ToUpper();
            }
            return string.Empty;
        }

        public static string PrivatePersonType(bool isPrivatePerson)
        {
            return isPrivatePerson ? "PRIVATE" : "COMPANY";
        }

        public static string CleanInvalidChars(string input)
        {
            if (input is null)
                return null;
            string replace = input.Replace("\u00D7", "X");
            //These are the allowed characters in Fortnox, replace others with nothing.
            string pattern = @"[^\p{L}\’\\\\u0308\\u030a\wåäöéáœæøüÅÄÖÉÁÜŒÆØ0-9 –:\.`´’,;\^¤#%§£$€¢¥©™°&\/\(\)=\+\-\*_\!?²³®½\@\\u00a0\n\r]";
            return Regex.Replace(replace, pattern, "");
        }
    }
}
