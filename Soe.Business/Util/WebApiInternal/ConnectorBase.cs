using IdentityModel.Client;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers;
using SoftOne.Common.KeyVault;
using SoftOne.Common.KeyVault.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;

namespace SoftOne.Soe.Business.Util.WebApiInternal
{
    public class ConnectorBase
    {
        private static string _accessToken;
        private static DateTime _tokenExpiry;

        public static Uri GetUri()
        {
            SettingManager sm = new SettingManager(null);
            string address = sm.GetStringSetting(SettingMainType.Application, (int)ApplicationSettingType.WebApiInternalUrl, 0, 0, 0);
#if DEBUG
            address = "http://localhost:1362/";

#endif
            return new Uri(address);
        }

        public static RestRequest CreateRequest(string resource, Method method, object obj = null)
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;

            if (obj != null)
                request.AddJsonBody(obj);

            int timeout = 1000 * 3600 * 2; //2 hours
            request.Timeout = TimeSpan.FromMilliseconds(timeout);
            request.AddHeader("Authorization", "Bearer " + GetAccessToken());
            request.AddHeader("Token", "e8d7bf57fd1b44a684689cfce813f783");
            return request;
        }

        public static string GetAccessToken()
        {
            string address = string.Empty;
            string client = string.Empty;
            KeyVaultSettings keyVaultSettings = new KeyVaultSettings();
            try
            {

                if (string.IsNullOrEmpty(_accessToken) || _tokenExpiry < DateTime.Now)
                {
                    var httpClient = new HttpClient();
                    address = SoftOneIdConnector.GetUri().EnsureTrailingSlash() + "connect/token";

                    keyVaultSettings = KeyVaultSettingsHelper.GetKeyVaultSettings();
                    client = KeyVaultSecretsFetcher.GetSecret(keyVaultSettings, SoftOne.Common.KeyVault.Constants.SoftOneIdInternalIdPClientId, keyVaultSettings.StoreLocation);
                    var secret = KeyVaultSecretsFetcher.GetSecret(keyVaultSettings, SoftOne.Common.KeyVault.Constants.SoftOneIdInternalIdPSecret, keyVaultSettings.StoreLocation);

                    if (string.IsNullOrEmpty(client))
                        client = KeyVaultSecretsFetcher.GetSecret(keyVaultSettings, SoftOne.Common.KeyVault.Constants.SoftOneIdWebIdPClientId, keyVaultSettings.StoreLocation);

                    if (string.IsNullOrEmpty(secret))
                        secret = KeyVaultSecretsFetcher.GetSecret(keyVaultSettings, SoftOne.Common.KeyVault.Constants.SoftOneIdWebIdPSecret, keyVaultSettings.StoreLocation);

                    var response = httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                    {
                        Address = address,
                        ClientId = client,
                        ClientSecret = secret,
                        Scope = "SoftOne.Internal"
                    }).Result;

                    if (response.IsError)
                    {
                        throw new Exception("Could not retrieve token from IdP", response.Exception);
                    }

                    _accessToken = response.AccessToken;
                    _tokenExpiry = DateTime.Now.AddSeconds(response.ExpiresIn - 60);
                }
            }
            catch (Exception ex)
            {
                var info = $"GetAccessToken {keyVaultSettings.KeyVaultUrl} {address} {client}";
                LogCollector.LogCollector.LogError(ex, info);
            }

            return _accessToken;
        }
    }

}
