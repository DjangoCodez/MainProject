using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;
using SoftOne.Soe.Business.Util.API.Utility;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Business.Util.API.VismaEAccounting
{
    class VismaEAccountingConnector
    {
        //API documentation reference: https://eaccountingapi.vismaonline.com/swagger/ui/index#/
        private const string ProdUrl = "https://eaccountingapi.vismaonline.com";
        private const string AuthUrl = "https://identity.vismaonline.com";
        private const string CallbackHandler = "https://devbridge.softone.se/api/vismaeaccounting/callback";

        //sandbox
        //private const string ClientId = "softoneabsandbox";
        //private const string ClientSecret = "XK=#CIkL4hB#nM3GNbGWb7w2OTHu5CeKkL8Lbzk8yOpGF4yspQl2WhGQgcjM4XIo";

        //prod
        private const string ClientId = "softone";
        private const string ClientSecret = "~cWhbCGbTm2kC*HVl7CMMfvPk!JlRq9DU0uBEfpH5JFz6GfWKP9aO3J5Uf8oXots";

        private string _credentials
        {
            get
            {
                var plainTextBytes = Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}");
                return Convert.ToBase64String(plainTextBytes);
            }
        }

        private RateLimitChecker _checker = new RateLimitChecker(60, 600); // 600 requests per minute

        private VismaTokenResponse _token { get; set; }

        public static string GetActivationUrl(string finalCallbackDestination)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(finalCallbackDestination);
            var base64Encoded = Convert.ToBase64String(plainTextBytes);
            return $"{AuthUrl}/connect/authorize?client_id={ClientId}&redirect_uri={CallbackHandler}&scope=ea:api%20offline_access%20ea:sales%20ea:accounting&state={base64Encoded}&response_type=code&prompt=select_account&acr_values=service:44643EB1-3F76-4C1C-A672-402AE8085934";
        }

        public void SetAuthToken(string refreshToken, bool isCode = false)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return;

            var client = new GoRestClient(AuthUrl);
            var request = new RestRequest("/connect/token", Method.Post);
            request.AddHeader("Authorization", $"Basic {_credentials}");
            request.AddHeader("content-type", "application/x-www-form-urlencoded;charset=UTF-8");

            if (isCode)
            {
                request.AddParameter("application/x-www-form-urlencoded", $"grant_type=authorization_code&code={refreshToken}&redirect_uri={CallbackHandler}", ParameterType.RequestBody);
            }
            else
            {
                request.AddParameter("application/x-www-form-urlencoded", $"grant_type=refresh_token&refresh_token={refreshToken}", ParameterType.RequestBody);
            }

            var response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var token = JsonConvert.DeserializeObject<VismaTokenResponse>(response.Content);
                this._token = token;
            }
        }
        public string GetRefreshToken()
        {
            return this._token?.RefreshToken;
        }

        private VismaResponseWrapper<T> MakeRequest<T>(string endpoint, Method method = Method.Get, object body = null)
        {
            if (_checker.WaitBeforeRequest(out int waitTime))
                Thread.Sleep(waitTime);

            var client = new GoRestClient(ProdUrl);
            var request = new RestRequest(endpoint, method);

            request.AddHeader("Authorization", $"Bearer {_token.AccessToken}");
            request.AddHeader("Client-Secret", ClientSecret);
            request.AddHeader("Accept", "application/json");
            request.RequestFormat = DataFormat.Json;

            if (body != null)
            {

                request.AddHeader("Content-type", "application/json; charset=utf-8");
                request.AddJsonBody(JsonConvert.SerializeObject(body));
            }

            var response = client.Execute(request);
            var responseWrapper = new VismaResponseWrapper<T>();
            if ((int)response.StatusCode >= 400)
            {
                var error = JsonConvert.DeserializeObject<VismaErrorResponse>(response.Content);
                responseWrapper.Error = error;
            }
            
            var result = JsonConvert.DeserializeObject<T>(response.Content);
            responseWrapper.Data = result;
            
            return responseWrapper;
        }
        private T SimpleGet<T>(string endpoint)
        {
            return this.MakeRequest<T>(endpoint).Data;
        }

        private List<T> FetchAll<T>(string endpoint)
        {
            return this.PerformFetchAll(this.GetPaginatedCallback<T>(endpoint));
        }
        private Func<int, int, VismaSetResponse<T>> GetPaginatedCallback<T>(string endpoint)
        {
            return new Func<int, int, VismaSetResponse<T>>((page, pageSize) =>
            {
                return SimpleGet<VismaSetResponse<T>>($"{endpoint}?$page={page}&$pagesize={pageSize}");
            });
        }
        private List<T> PerformFetchAll<T>(Func<int, int, VismaSetResponse<T>> action)
        {
            var response = action(1, 1000);
            var result = response.Data;
            int page = 1;

            while (page < response.Meta.TotalNumberOfPages)
            {
                page++;
                var next = action(page, 1000);
                result.AddRange(next.Data);
            }

            return result;
        }

        private string FilterEquals(string key, string value)
        {
            return $"$filter= {key} eq '{value}'";
        }

        public List<VismaArticle> GetAllArticles()
        {   
            return FetchAll<VismaArticle>("/v2/articles");
        }

        public List<VismaUnit> GetAllUnits()
        {
            return FetchAll<VismaUnit>("/v2/units");
        }

        public List<VismaArticleAccountCoding> GetAllAccountCodings()
        {
            return FetchAll<VismaArticleAccountCoding>("/v2/articleaccountcodings");
        }
        public List<VismaTermsOfPayment> GetAllTermsOfPayment()
        {
            return FetchAll<VismaTermsOfPayment>("/v2/termsofpayments");
        }
        public VismaResponseWrapper<VismaArticle> CreateNewArticle(VismaArticle article)
        {
            return MakeRequest<VismaArticle>("/v2/articles", Method.Post, article);
        }
        public VismaResponseWrapper<VismaArticle> UpdateArticle(VismaArticle article)
        {
            return MakeRequest<VismaArticle>($"/v2/articles/{article.Id}", Method.Put, article);
        }
        public VismaCustomer GetCustomerByNumber(string customerNumber)
        {
            var response = MakeRequest<VismaSetResponse<VismaCustomer>>($"/v2/customers?{FilterEquals("CustomerNumber", customerNumber)}");

            if (!response.Success || response.Data == null) return null;
            var wrapper = response.Data;
            if (wrapper.Data == null) return null;

            return wrapper.Data.FirstOrDefault();
        }
        public VismaResponseWrapper<VismaCustomer> CreateNewCustomer(VismaCustomer customer)
        {
            return MakeRequest<VismaCustomer>("/v2/customers", Method.Post, customer);
        }
        public VismaResponseWrapper<VismaCustomer> UpdateCustomer(VismaCustomer customer)
        {
            return MakeRequest<VismaCustomer>($"/v2/customers/{customer.Id}", Method.Put, customer);
        }
        public VismaResponseWrapper<VismaCustomerInvoice> CreateNewInvoice(VismaCustomerInvoice invoice)
        {
            return MakeRequest<VismaCustomerInvoice>("/v2/customerinvoices", Method.Post, invoice);
        }
        public VismaResponseWrapper<VismaCompanySettings> GetSettings()
        {
            return MakeRequest<VismaCompanySettings>("/v2/companysettings");
        }
    }
}
