using Newtonsoft.Json;
using RestSharp;
using SoftOne.Soe.Business.Util.API.AzoraOne.Models;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.API.AzoraOne.Connectors
{
    public class AOBaseConnector
    {
        private const string _baseUrl = "https://api.azora.one/softone_test/v1";
        private const string _clientKey = "cZkABVCBmHZe6NZgF/zFrA";
        private const string _subscriptionKey = "ed75aec03d104331b2af55902de4e057";
        private string _companyIdentifier;
        protected string CompanyIdentifier => _companyIdentifier;
        protected string CompanyEndpoint => $"/companies/{CompanyIdentifier}";

        private RestClient _client;
        protected RestClient Client
        {
            get => _client ?? (_client = CreateClient());
        }

        protected AOBaseConnector(Guid companyIdentifier)
        {
            this._companyIdentifier = companyIdentifier.ToString();
        }

        private RestClient CreateClient()
        {
            var client = new GoRestClient(_baseUrl);
            client.AddDefaultHeader("Client-Key", _clientKey);
            client.AddDefaultHeader("Ocp-Apim-Subscription-Key", _subscriptionKey);
            client.AddDefaultHeader("Accept", "application/json");
            return client;
        }

        protected AOResponseWrapper<T> Post<T>(string endpoint, object body)
        {
            return MakeRequest<T>(endpoint, Method.Post, body);
        }
        protected AOResponseWrapper<T> Get<T>(string endpoint)
        {
            return MakeRequest<T>(endpoint, Method.Get);
        }
        protected AOResponseWrapper<T> Put<T>(string endpoint, object body)
        {
            return MakeRequest<T>(endpoint, Method.Put, body);
        }
        protected void AddBody(RestRequest request, object body)
        {
            request.AddHeader("Content-type", "application/json; charset=utf-8");
            request.AddJsonBody(JsonConvert.SerializeObject(body));
        }
        private AOResponseWrapper<T> MakeRequest<T>(string endpoint, Method method = Method.Get, object body = null)
        {
            var request = new RestRequest(endpoint, method);
            request.RequestFormat = DataFormat.Json;

            if (body != null)
            {
                AddBody(request, body);
            }

            var response = this.Client.Execute(request);
            return HandleResponse<T>(request, response);
        }
        protected AOResponseWrapper<T> HandleResponse<T>(RestRequest request, RestResponse response)
        {
            // Refer to https://developer.azora.one/documentation#responses
            var method = request.Method;
            var content = response.Content;
            var statusCode = (int)response.StatusCode;
            var isGetRequest = method == Method.Get;
            switch ((int)response.StatusCode)
            {
                case 200:
                case 202: //The request has been accepted for processing, but the processing has not been completed.
                    return AOResponseWrapper<T>.Success(Deserialize<AOResponse<T>>(content), isGetRequest, content);
                case 400:
                case 401:
                case 403:
                case 404:
                case 409:
                case 412:
                case 500:
                case 501:
                case 503:
                    return AOResponseWrapper<T>.Failure(Deserialize<AOResponse<List<AOErrorDetails>>>(content), isGetRequest, content);
                case 0:
                default:
                    {
                        // Seems to happen every now and then. Probably connectivity issues.
                        return AOResponseWrapper<T>.Failure(new AOResponse<List<AOErrorDetails>>()
                        {
                            Data = new List<AOErrorDetails>()
                            {
                                new AOErrorDetails()
                                {
                                    Code = (int)response.StatusCode,
                                    Message = response.ErrorMessage ?? "No response from server"
                                }
                            }
                        }, isGetRequest, content);
                    }
            }
        }
        protected T Deserialize<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
