using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.IO;
using System.Text;

namespace SoftOne.Soe.Business.Util.API.OAuth
{
    public class OAuthConnectorBase
    {
        protected string CurrentToken;
        protected int Retries = 0;

        public string GetLoginToken(OAuthLoginConfiguration oauthConfig)
        {
            if (string.IsNullOrEmpty(oauthConfig.GrantType) )
            {
                throw new ActionFailedException("Missing GrantType");
            }

            if (string.IsNullOrEmpty(oauthConfig.ClientID) || string.IsNullOrEmpty(oauthConfig.ClientSecret))
            {
                throw new ActionFailedException("Missing ClientId or ClientSecret");
            }

            if (oauthConfig.GrantType == "password" && ( string.IsNullOrEmpty(oauthConfig.Username) || string.IsNullOrEmpty(oauthConfig.Password)))
            {
                throw new ActionFailedException("Missing Username or Password");
            }

            var client = new GoRestClient(oauthConfig.LoginHost);
            var plainTextBytes = Encoding.UTF8.GetBytes($"{oauthConfig.ClientID}:{oauthConfig.ClientSecret}");
            var base64Secret = Convert.ToBase64String(plainTextBytes);

            var request = new RestRequest(oauthConfig.LoginPath, Method.Post);
            request.RequestFormat = DataFormat.Json;
            request.AddHeader("Authorization", $"basic {base64Secret}");
            request.AddParameter("grant_type", oauthConfig.GrantType);

            if (!string.IsNullOrEmpty(oauthConfig.Scope))
            {
                request.AddParameter("Scope", oauthConfig.Scope);
            }
            if (!string.IsNullOrEmpty(oauthConfig.Username))
            {
                request.AddParameter("Username", oauthConfig.Username);
            }
            if (!string.IsNullOrEmpty(oauthConfig.Password))
            {
                request.AddParameter("Password", oauthConfig.Password);
            }

            var response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var token = JsonConvert.DeserializeObject<OAuthAccessToken>(response.Content);
                CurrentToken = token.Access_token;
            }
            else
            {
                return $"{response.StatusCode} : {response.Content}";
            }
            return "";
        }

        public static RestClient GetRestClientWithNewtonsoftJson(string uri, int maxTimeOut = 0)
        {
            RestClientOptions options = new RestClientOptions(new Uri(uri));
            if (maxTimeOut > 0)
                options.Timeout = TimeSpan.FromMilliseconds(maxTimeOut);
            var client = new GoRestClient(options, configureSerialization: s => s.UseNewtonsoftJson());
            return client;
        }

        protected ActionResult SendObject(string urlHost, string urlResource, object data, bool usePut = false)
        {
            var client = GetRestClientWithNewtonsoftJson(urlHost);
            var request = CreateRequest(urlResource, CurrentToken, usePut ? Method.Put : Method.Post, data);
            var response = client.Execute(request);

            ActionResult result;
            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                case System.Net.HttpStatusCode.Created:
                    Retries = 0;
                    result = new ActionResult(true);
                    break;
                case System.Net.HttpStatusCode.Unauthorized:
                    CurrentToken = null;
                    Retries++;
                    result = new ActionResult((int)response.StatusCode, "OAuthConnector: " + StringUtility.HTMLToText(response.Content));
                    break;
                //429, To many requests....
                case System.Net.HttpStatusCode.ServiceUnavailable:
                case System.Net.HttpStatusCode.InternalServerError:
                case System.Net.HttpStatusCode.Forbidden:
                    result = new ActionResult((int)response.StatusCode, "OAuthConnector: " + StringUtility.HTMLToText(response.Content));
                    break;
                default:
#if DEBUG
                    File.WriteAllText(@"C:\Temp\OAuthConnector_response.xml", response.Content);
#endif
                    result = new ActionResult
                    {
                        StringValue = response.Content,
                        Success = false,
                        ErrorNumber = (int)response.StatusCode,
                        ErrorMessage = response.StatusCode.ToString()
                    };
                    
                    break;
            }

            return result;
        }

        private RestRequest CreateRequest(string resource, string token, RestSharp.Method method, object obj = null)
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;

            request.AddHeader("Authorization", "Bearer " + token);

            if (obj != null)
            {
                request.AddJsonBody(obj);
            }

            return request;
        }
    }
    
    public class OAuthAccessToken
    {
        public string Access_token { get; set; }
        public int Expires_in { get; set; }
        public int Refresh_expires_in { get; set; }
        public string Refresh_token { get; set; }
        public string Token_type { get; set; }
        public string Session_state { get; set; }
        public string Scope { get; set; }
    }

    public class OAuthLoginConfiguration
    {
        public string GrantType { get; set; }
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Scope { get; set; }
        
        public string LoginHost { get; set; }
        public string LoginPath { get; set; }
    }
}
