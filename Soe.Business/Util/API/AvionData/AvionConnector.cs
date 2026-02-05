using RestSharp;
using SoftOne.Soe.Business.Util.API.AvionData.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;

namespace SoftOne.Soe.Business.Util.API.AvionData
{
    public class AvionConnector
    {
        private const string _baseUrl = "https://avoindata.prh.fi/opendata-ytj-api/v3";
        private string CompanySearchEndpoint => $"/companies";
        private RestClient _client;
        protected RestClient Client
        {
            get => _client ?? (_client = CreateClient());
        }

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        private RestClient CreateClient()
        {
            var client = new RestClient(_baseUrl);
            client.AddDefaultHeader("Accept", "application/json");
            return client;
        }

        private AvionResponse<TSuccess> Get<TSuccess>(string resource)
        {
            var request = new RestRequest(resource, Method.Get);
            request.RequestFormat = DataFormat.Json;
            var response = this.Client.Execute(request);

            string json = response.Content;

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var successResult = JsonSerializer.Deserialize<TSuccess>(json, JsonOptions);
                    return AvionResponse<TSuccess>.Success(successResult);
                }
                catch (JsonException)
                {
                    return AvionResponse<TSuccess>.Fail(AvionException.JsonParser(response.StatusCode.ToString()));
                }
                catch (Exception ex)
                {
                    return AvionResponse<TSuccess>.Fail(AvionException.Unknown(ex, response.StatusCode.ToString()));
                }
            }
            else
            {
                try
                {
                    var errorResult = JsonSerializer.Deserialize<ErrorResponse>(json, JsonOptions);
                    return AvionResponse<TSuccess>.Fail(errorResult ?? new ErrorResponse
                    {
                        Timestamp = DateTime.UtcNow.ToString("o"),
                        Message = $"Unknown error, status code {response.StatusCode}",
                        Errorcode = response.StatusCode.ToString()
                    });
                }
                catch (JsonException)
                {
                    return AvionResponse<TSuccess>.Fail(AvionException.JsonParser(response.StatusCode.ToString()));
                }
                catch (Exception ex)
                {
                    return AvionResponse<TSuccess>.Fail(AvionException.Unknown(ex,response.StatusCode.ToString()));
                }
            }
        }

        public AvionResponse<List<Company>> SearchCompanies(CompanySearchFilter searchFilter)
        {
            try
            {
                var result = this.Get<CompanyResult>(CompanySearchEndpoint + "?" + searchFilter.ToQueryString());

                List<Company> companies = new List<Company>();
                if (result.IsSuccess)
                {
                    companies.AddRange(result.Result.Companies);
                    int totalResults = result.Result.TotalResults;
                    int pages = (int)Math.Ceiling((double)totalResults / 100);
                    for (int i = 2; i <= pages; i++)
                    {
                        searchFilter.Page = i;
                        //Wait sometime to avoid overwhelming the API
                        Thread.Sleep(1000); // 1 second delay
                        // Fetch next page of results
                        var nextPageResult = this.Get<CompanyResult>(CompanySearchEndpoint + "?" + searchFilter.ToQueryString());
                        if (nextPageResult.IsSuccess)
                        {
                            companies.AddRange(nextPageResult.Result.Companies);
                        }
                        else
                        {
                            return AvionResponse<List<Company>>.Fail(nextPageResult.Error ?? new ErrorResponse
                            {
                                Timestamp = DateTime.UtcNow.ToString("o"),
                                Message = nextPageResult.Error.Message,
                                Errorcode = nextPageResult.Error.Errorcode
                            });
                        }
                    }
                }
                else
                {
                    return AvionResponse<List<Company>>.Fail(result.Error ?? new ErrorResponse
                    {
                        Timestamp = DateTime.UtcNow.ToString("o"),
                        Message = result.Error.Message,
                        Errorcode = result.Error.Errorcode
                    });
                }

                return AvionResponse<List<Company>>.Success(companies);
            }
            catch (Exception ex)
            {
                return AvionResponse<List<Company>>.Fail(new ErrorResponse
                {
                    Timestamp = DateTime.UtcNow.ToString("o"),
                    Message = ex.Message,
                    Errorcode = ""
                });
            }
        }
    }
}
