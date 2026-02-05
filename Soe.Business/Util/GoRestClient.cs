using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Net.Http;

namespace RestSharp
{
    public class GoRestClient : RestClient
    {
        private static readonly ConcurrentDictionary<string, HttpClient> _httpClients = new ConcurrentDictionary<string, HttpClient>();

        public GoRestClient(Uri baseUri, TimeSpan timeout, bool disposeHttpClient = false, ConfigureSerialization configureSerialization = null)
            : base(GetOrCreateHttpClient(new RestClientOptions() { BaseUrl = baseUri, Timeout = timeout }), new RestClientOptions { BaseUrl = baseUri, Timeout = timeout }, disposeHttpClient: disposeHttpClient, configureSerialization: configureSerialization)
        {
        }

        public GoRestClient(string uri, TimeSpan timeout, bool disposeHttpClient = false)
            : base(GetOrCreateHttpClient(new RestClientOptions() { BaseUrl = new Uri(uri), Timeout = timeout }), new RestClientOptions { BaseUrl = new Uri(uri), Timeout = timeout }, disposeHttpClient: disposeHttpClient)
        {
        }

        public GoRestClient(Uri baseUri, bool disposeHttpClient = false, ConfigureSerialization configureSerialization = null)
            : base(GetOrCreateHttpClient(new RestClientOptions() { BaseUrl = baseUri }), new RestClientOptions { BaseUrl = baseUri }, disposeHttpClient: disposeHttpClient, configureSerialization: configureSerialization)
        {
        }

        public GoRestClient(string uri, bool disposeHttpClient = false)
            : base(GetOrCreateHttpClient(new RestClientOptions() { BaseUrl = new Uri(uri) }), new RestClientOptions { BaseUrl = new Uri(uri) }, disposeHttpClient: disposeHttpClient)
        {
        }

        public GoRestClient(RestClientOptions options, bool disposeHttpClient = false, ConfigureSerialization configureSerialization = null)
            : base(GetOrCreateHttpClient(options), options, disposeHttpClient, configureSerialization: configureSerialization)
        {
        }

        private static HttpClient GetOrCreateHttpClient(RestClientOptions options)
        {
            var key = $"{options.BaseUrl}:{options.Timeout}";
            return _httpClients.GetOrAdd(key, _ =>
            {
                var handler = new HttpClientHandler();
                var client = new HttpClient(handler)
                {
                    BaseAddress = options.BaseUrl,
                    Timeout = options.Timeout.HasValue ? options.Timeout.Value : TimeSpan.FromSeconds(100),
                };
                return client;
            });
        }
    }
}