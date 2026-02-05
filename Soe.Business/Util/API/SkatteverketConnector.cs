using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using SoftOne.Common.KeyVault;
using SoftOne.Common.KeyVault.Models;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.Util.API.OAuth;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace SoftOne.Soe.Business.Util.API
{
    public static class SkatteverketConnector
    {
        #region CSR

        public static CsrResponseDTO GetCsrReponse(string orgNbr, string socialSecNbr, int year)
        {
            if (string.IsNullOrEmpty(orgNbr))
                return new CsrResponseDTO("Organisationsnummer saknas");

            string clientId;
            string secret;

            try
            {
                var vaultsettings = KeyVaultSettingsHelper.GetKeyVaultSettings();
                if (vaultsettings != null)
                {
                    try
                    {
                        clientId = KeyVaultSecretsFetcher.GetSecret(vaultsettings.CertificateDistinguishedName, vaultsettings.ClientId, vaultsettings.KeyVaultUrl, "Skatteverket-ClientId", vaultsettings.StoreLocation, vaultsettings.TenantId);
                        secret = KeyVaultSecretsFetcher.GetSecret(vaultsettings.CertificateDistinguishedName, vaultsettings.ClientId, vaultsettings.KeyVaultUrl, "Skatteverket-Secret", vaultsettings.StoreLocation, vaultsettings.TenantId);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "Kunde inte hämta inloggningsinformation");

                        clientId = "7607984164a04c47b58b1c2a08bb1423";
                        secret = "E743f54A5a5544c785868b1A7B0b909d";

                        //return new CsrResponseDTO("Key request failed, error logged");
                    }
                }
                else
                {
                    LogError("Initiering mot Skatteverket misslyckades");
                    return new CsrResponseDTO("Initiering mot Skatteverket misslyckades");
                }

                orgNbr = StringUtility.Orgnr16XXXXXX_Dash_XXXX(orgNbr).Replace("-", "");
                socialSecNbr = StringUtility.SocialSecYYYYMMDDXXXX(socialSecNbr);
            }
            catch (Exception ex)
            {
                LogError(ex, "Kontakt med Skatteverker misslyckades");
                return new CsrResponseDTO("Kontakt med Skatteverker misslyckades");
            }

            string errorMessage = string.Empty;

            try
            {
                var client = GetRestClientWithNewtonsoftJson(GetUriCSR());
                 var request = CreateRequest($"arbetsgivare/{orgNbr}/anstallda/{socialSecNbr}", RestSharp.Method.Get, null);
                //request.AddParameter("client_id", "9a64adf6ba6442659ef894f367da8421", ParameterType.QueryString);
                //request.AddParameter("client_secret", "4459bA972a1849D48218408525288Af9", ParameterType.QueryString);
                request.AddParameter("client_id", clientId, ParameterType.QueryString);
                request.AddParameter("client_secret", secret, ParameterType.QueryString);
                request.AddParameter("inkomstar", year.ToString(), ParameterType.QueryString);
                request.AddHeader("SKV-client_correlationid", Guid.NewGuid().ToString().Replace("-", ""));
                RestResponse response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    errorMessage = response.Content;

                if (string.IsNullOrEmpty(errorMessage) && !response.Content.ToLower().Contains("felbeskrivning"))
                {
                    CsrResponseDTO dto = JsonConvert.DeserializeObject<CsrResponseDTO>(response.Content);
                    dto.Year = year;
                    return dto;
                }
                else if (response.Content.ToLower().Contains("felbeskrivning"))
                {
                    csrfel fel = JsonConvert.DeserializeObject<csrfel>(response.Content);
                    return new CsrResponseDTO($"({fel.felkod}) {fel.felbeskrivning}");
                }
                else
                {
                    CsrErrorResponse error = JsonConvert.DeserializeObject<CsrErrorResponse>(response.Content);

                    return new CsrResponseDTO($"({error.felkod}) {error.felmeddelande}");
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "GetCsrReponse");
                return new CsrResponseDTO("Request failed, error logged");
            }
        }

        public static List<CsrResponseDTO> GetCsrReponses(string orgNbr, List<string> socialSecNbrs, int year)
        {
            List<CsrResponseDTO> responses = new List<CsrResponseDTO>();

            while (socialSecNbrs.Any())
            {
                var batch = socialSecNbrs.Take(999);
                socialSecNbrs = socialSecNbrs.Where(w => !batch.Contains(w)).ToList();
                responses.AddRange(GetCsrReponses(orgNbr, JsonConvert.SerializeObject(batch.ToArray()), year));
            }

            return responses;
        }

        private static List<CsrResponseDTO> GetCsrReponses(string orgNbr, string socialSecNbrs, int year)
        {
            var vaultsettings = KeyVaultSettingsHelper.GetKeyVaultSettings();
            string clientId = KeyVaultSecretsFetcher.GetSecret(vaultsettings.CertificateDistinguishedName, vaultsettings.ClientId, vaultsettings.KeyVaultUrl, "Skatteverket-ClientId", vaultsettings.StoreLocation, vaultsettings.TenantId);
            string secret = KeyVaultSecretsFetcher.GetSecret(vaultsettings.CertificateDistinguishedName, vaultsettings.ClientId, vaultsettings.KeyVaultUrl, "Skatteverket-Secret", vaultsettings.StoreLocation, vaultsettings.TenantId);
            orgNbr = StringUtility.Orgnr16XXXXXX_Dash_XXXX(orgNbr).Replace("-", "");

            string errorMessage = string.Empty;
            try
            {
                var client = GetRestClientWithNewtonsoftJson(GetUriCSR());
                var request = CreateRequest($"arbetsgivare/{orgNbr}/anstallda/hamta", RestSharp.Method.Post, socialSecNbrs);
                request.AddParameter("client_id", clientId, ParameterType.QueryString);
                request.AddParameter("client_secret", secret, ParameterType.QueryString);
                request.AddParameter("inkomstar", year, ParameterType.QueryString);
                request.AddHeader("SKV-client_correlationid", Guid.NewGuid().ToString().Replace("-", ""));
                RestResponse response = client.Execute(request);
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    errorMessage = response.Content;

                if (string.IsNullOrEmpty(errorMessage) && !response.Content.ToLower().Contains("felbeskrivning"))
                {
                    List<CsrResponseDTO> dtos = JsonConvert.DeserializeObject<List<CsrResponseDTO>>(response.Content);
                    foreach (CsrResponseDTO dto in dtos)
                    {
                        dto.Year = year;
                    }
                    return dtos;
                }
                else if (response.Content.ToLower().Contains("felbeskrivning"))
                {
                    csrfel fel = JsonConvert.DeserializeObject<csrfel>(response.Content);
                    return new List<CsrResponseDTO>() { new CsrResponseDTO($"({fel.felkod}) {fel.felbeskrivning}") };
                }
                else
                {
                    CsrErrorResponse error = JsonConvert.DeserializeObject<CsrErrorResponse>(response.Content);

                    return new List<CsrResponseDTO>() { new CsrResponseDTO($"({error.felkod}) {error.felmeddelande}") };
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "GetCsrReponse");
                return new List<CsrResponseDTO>() { new CsrResponseDTO("Exception failed, error logged") };
            }
        }

        private static Uri GetUriCSR()
        {
            return new Uri("https://api.skatteverket.se/inkomstbeskattning/CSR-forfragan/v1/");
        }

        #endregion

        #region SkatteAvdrag

        public static CsrResponseDTO GetSkatteAvdrag(string orgNbr, string socialSecNbr, int year, string clientId = "", string secret = "")
        {
            if (string.IsNullOrEmpty(orgNbr))
                return new CsrResponseDTO("Organisationsnummer saknas");

            if (string.IsNullOrEmpty(socialSecNbr))
                return new CsrResponseDTO("Personnummer saknas");

            orgNbr = StringUtility.Orgnr16XXXXXX_Dash_XXXX(orgNbr).Replace("-", "");
            socialSecNbr = StringUtility.SocialSecYYYYMMDDXXXX(socialSecNbr);
            string errorMessage = string.Empty;

            if (string.IsNullOrEmpty(clientId))
                errorMessage = GetClientAndSecret(out clientId, out secret);

            if (!string.IsNullOrEmpty(errorMessage))
                return new CsrResponseDTO(errorMessage);

            try
            {
                var client =  GetRestClientWithNewtonsoftJson(GetUriSkatteavdrag());
                var request = CreateRequest($"{year}/huvudutbetalare/{orgNbr}/anstallda/{socialSecNbr}", RestSharp.Method.Get, null);
                request.AddHeader("client_id", clientId);
                request.AddHeader("client_secret", secret);
                request.AddHeader("SKV-client_correlationid", Guid.NewGuid().ToString().Replace("-", ""));
                RestResponse response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    errorMessage = response.Content;

                if (string.IsNullOrEmpty(errorMessage) && !response.Content.ToLower().Contains("felbeskrivning"))
                {
                    CsrResponseDTO dto = JsonConvert.DeserializeObject<CsrResponseDTO>(response.Content);
                    dto.Year = year;
                    return dto;
                }
                else if (response.Content.ToLower().Contains("felbeskrivning"))
                {
                    csrfel fel = JsonConvert.DeserializeObject<csrfel>(response.Content);
                    return new CsrResponseDTO($"({fel.felkod}) {fel.felbeskrivning}");
                }
                else
                {
                    CsrErrorResponse error = JsonConvert.DeserializeObject<CsrErrorResponse>(response.Content);
                    if (!string.IsNullOrEmpty(errorMessage) && error.felkod == 0 && string.IsNullOrEmpty(error.felmeddelande))
                        return new CsrResponseDTO(errorMessage);
                    else
                        return new CsrResponseDTO($"({error.felkod}) {error.felmeddelande}");
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "GetCsrReponse");
                return new CsrResponseDTO("Request failed, error logged");
            }
        }
        public static List<CsrResponseDTO> GetFleraSkatteavdrag(string orgNbr, SkatteAvdragFleraPersoner skatteAvdragFleraPersoner, int year)
        {
            List<CsrResponseDTO> responses = new List<CsrResponseDTO>();

            while (skatteAvdragFleraPersoner.personnummer.Count > 0)
            {
                var batch = skatteAvdragFleraPersoner.personnummer.Take(999).ToList();
                skatteAvdragFleraPersoner.personnummer = skatteAvdragFleraPersoner.personnummer.Where(w => !batch.Contains(w)).ToList();
                var req = new SkatteAvdragFleraPersoner() { personnummer = batch };
                responses.AddRange(HamtaFleraSkatteavdrag(orgNbr, req, year));
            }

            return responses;
        }

        private static List<CsrResponseDTO> HamtaFleraSkatteavdrag(string orgNbr, SkatteAvdragFleraPersoner skatteAvdragFleraPersoner, int year)
        {
            if (string.IsNullOrEmpty(orgNbr))
                return new List<CsrResponseDTO> { new CsrResponseDTO("Organisationsnummer saknas") };

            if (skatteAvdragFleraPersoner == null || skatteAvdragFleraPersoner.personnummer == null || !skatteAvdragFleraPersoner.personnummer.Any())
                return new List<CsrResponseDTO> { new CsrResponseDTO("Personnummer saknas") };

            string errorMessage = string.Empty;

            string clientId;
            string secret;

            errorMessage = GetClientAndSecret(out clientId, out secret);

            if (!string.IsNullOrEmpty(errorMessage))
                return new List<CsrResponseDTO> { new CsrResponseDTO(errorMessage) };

            try
            {
                var client = GetRestClientWithNewtonsoftJson(GetUriSkatteavdrag());
                var request = CreateRequest($"{year}/huvudutbetalare/{orgNbr}/anstallda/hamta", RestSharp.Method.Post, skatteAvdragFleraPersoner);
                request.AddHeader("client_id", clientId);
                request.AddHeader("client_secret", secret);
                request.AddHeader("SKV-client_correlationid", Guid.NewGuid().ToString().Replace("-", ""));
                request.AddHeader("content-type", "application/json");
                RestResponse response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    errorMessage = response.Content;

                if (string.IsNullOrEmpty(errorMessage) && !response.Content.ToLower().Contains("felbeskrivning"))
                {
                    var content = response.Content.Replace("giltig-from", "giltigfrom").Replace("giltig-tom", "giltigtom");
                    List<CsrResponseDTO> dtos = JsonConvert.DeserializeObject<List<CsrResponseDTO>>(content);
                    dtos.ForEach(dto => dto.Year = year);
                    return dtos;
                }
                else if (response.Content.ToLower().Contains("felbeskrivning"))
                {
                    csrfel fel = JsonConvert.DeserializeObject<csrfel>(response.Content);
                    return new List<CsrResponseDTO> { new CsrResponseDTO($"({fel.felkod}) {fel.felbeskrivning}") };
                }
                else
                {
                    List<CsrResponseDTO> list = new List<CsrResponseDTO>();
                    foreach (var person in skatteAvdragFleraPersoner.personnummer)
                    {
                        list.Add(GetSkatteAvdrag(orgNbr, person, year, clientId, secret));
                    }

                    if (list.Any())
                        return list;

                    CsrErrorResponse error = JsonConvert.DeserializeObject<CsrErrorResponse>(response.Content);

                    return new List<CsrResponseDTO> { new CsrResponseDTO($"({error.felkod}) {error.felmeddelande}") };
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "GetCsrReponse");
                return new List<CsrResponseDTO> { new CsrResponseDTO("Request failed, error logged") };
            }
        }

        private static Uri GetUriSkatteavdrag()
        {
            return new Uri("https://api.skatteverket.se/inkomstbeskattning/fraga-om-skatteavdrag/v1/");
        }

        public static string GetClientAndSecret(out string clientId, out string secret)
        {
            clientId = "";
            secret = "";

            try
            {
                var vaultsettings = KeyVaultSettingsHelper.GetKeyVaultSettings();

                if (vaultsettings != null)
                {
                    try
                    {
                        clientId = KeyVaultSecretsFetcher.GetSecret(vaultsettings.CertificateDistinguishedName, vaultsettings.ClientId, vaultsettings.KeyVaultUrl, "Skatteverket-Api-ClientId", vaultsettings.StoreLocation, vaultsettings.TenantId);
                        secret = KeyVaultSecretsFetcher.GetSecret(vaultsettings.CertificateDistinguishedName, vaultsettings.ClientId, vaultsettings.KeyVaultUrl, "Skatteverket-Api-Secret", vaultsettings.StoreLocation, vaultsettings.TenantId);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "Kunde inte hämta inloggningsinformationen");

                        if (vaultsettings.CertificateDistinguishedName.ToLower().Contains("test"))
                        {
                            clientId = "7607984164a04c47b58b1c2a08bb1423";
                            secret = "E743f54A5a5544c785868b1A7B0b909d";
                        }
                        else
                        {
                            clientId = ""; // "420Bda5Ed32443CeB3480d490eb20499";
                            secret = "";
                        }

                        //return new CsrResponseDTO("Key request failed, error logged");
                    }
                }
                else
                {
                    LogError("Initiering mot Skatteverket misslyckades");
                    return "Initiering mot Skatteverket misslyckades";
                }

            }
            catch (Exception ex)
            {
                LogError(ex, "Kontakt med Skatteverker misslyckades");
                return "Kontakt med Skatteverker misslyckades";
            }

            return "";
        }
        #endregion

        #region FOS

        private static string gw_ClientId;
        private static string gw_ClientSecret;

        public static CsrResponseDTO GetFOS(string orgNbr, string socialSecNbr, int year)
        {
            if (string.IsNullOrEmpty(orgNbr))
                return new CsrResponseDTO("Organisationsnummer saknas");

            if (string.IsNullOrEmpty(socialSecNbr))
                return new CsrResponseDTO("Personnummer saknas");

            orgNbr = StringUtility.Orgnr16XXXXXX_Dash_XXXX(orgNbr).Replace("-", "");
            socialSecNbr = StringUtility.SocialSecYYYYMMDDXXXX(socialSecNbr);
            string errorMessage = string.Empty;

            try
            {
                var client = GetRestClientWithNewtonsoftJson(GetUriFOS());
                var request = CreateRequest($"{year}/huvudutbetalare/{orgNbr}/anstallda/{socialSecNbr}", RestSharp.Method.Get, null);
                OAuthAccessToken token = GetToken();
                request.AddHeader("authorization", "Bearer " + token.Access_token);
                KeyVaultSettings vaultsettings = KeyVaultSettingsHelper.GetKeyVaultSettings();
                gw_ClientId = gw_ClientId ?? KeyVaultSecretsFetcher.GetSecret(vaultsettings.CertificateDistinguishedName, vaultsettings.ClientId, vaultsettings.KeyVaultUrl, "Skatteverket-Api-ClientId", vaultsettings.StoreLocation, vaultsettings.TenantId);
                gw_ClientSecret = gw_ClientSecret ?? KeyVaultSecretsFetcher.GetSecret(vaultsettings.CertificateDistinguishedName, vaultsettings.ClientId, vaultsettings.KeyVaultUrl, "Skatteverket-Api-Secret", vaultsettings.StoreLocation, vaultsettings.TenantId);

                request.AddHeader("client_id", gw_ClientId);
                request.AddHeader("client_secret", gw_ClientSecret);
                request.AddHeader("skv-client_correlationid", Guid.NewGuid().ToString().Replace("-", ""));
                RestResponse response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    errorMessage = response.Content;

                if (string.IsNullOrEmpty(errorMessage) && !response.Content.ToLower().Contains("felbeskrivning"))
                {
                    CsrResponseDTO dto = JsonConvert.DeserializeObject<CsrResponseDTO>(response.Content);
                    dto.Year = year;
                    return dto;
                }
                else if (response.Content.ToLower().Contains("felbeskrivning"))
                {
                    csrfel fel = JsonConvert.DeserializeObject<csrfel>(response.Content);
                    return new CsrResponseDTO($"({fel.felkod}) {fel.felbeskrivning}");
                }
                else
                {
                    CsrErrorResponse error = JsonConvert.DeserializeObject<CsrErrorResponse>(response.Content);
                    if (!string.IsNullOrEmpty(errorMessage) && error.felkod == 0 && string.IsNullOrEmpty(error.felmeddelande))
                        return new CsrResponseDTO(errorMessage);
                    else
                        return new CsrResponseDTO($"({error.felkod}) {error.felmeddelande}");
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "GetCsrReponse");
                return new CsrResponseDTO("Request failed, error logged");
            }
        }

        public static List<CsrResponseDTO> GetFleraFOS(string orgNbr, SkatteAvdragFleraPersoner skatteAvdragFleraPersoner, int year)
        {
            List<CsrResponseDTO> responses = new List<CsrResponseDTO>();

            while (skatteAvdragFleraPersoner.personnummer.Count > 0)
            {
                var batch = skatteAvdragFleraPersoner.personnummer.Take(999).ToList();
                skatteAvdragFleraPersoner.personnummer = skatteAvdragFleraPersoner.personnummer.Where(w => !batch.Contains(w)).ToList();
                var req = new SkatteAvdragFleraPersoner() { personnummer = batch };
                responses.AddRange(HamtaFleraFOS(orgNbr, req, year));
            }

            return responses;
        }

        private static List<CsrResponseDTO> HamtaFleraFOS(string orgNbr, SkatteAvdragFleraPersoner skatteAvdragFleraPersoner, int year)
        {
            if (string.IsNullOrEmpty(orgNbr))
                return new List<CsrResponseDTO> { new CsrResponseDTO("Organisationsnummer saknas") };

            if (skatteAvdragFleraPersoner == null || skatteAvdragFleraPersoner.personnummer == null || !skatteAvdragFleraPersoner.personnummer.Any())
                return new List<CsrResponseDTO> { new CsrResponseDTO("Personnummer saknas") };

            string errorMessage = string.Empty;

            if (!string.IsNullOrEmpty(errorMessage))
                return new List<CsrResponseDTO> { new CsrResponseDTO(errorMessage) };

            var vaultsettings = KeyVaultSettingsHelper.GetKeyVaultSettings();

            try
            {
                var client = GetRestClientWithNewtonsoftJson(GetUriFOS());
                var request = CreateRequest($"{year}/huvudutbetalare/{orgNbr}/anstallda/fragor", RestSharp.Method.Post, skatteAvdragFleraPersoner);
                OAuthAccessToken token = GetToken();
                gw_ClientId = gw_ClientId ?? KeyVaultSecretsFetcher.GetSecret(vaultsettings.CertificateDistinguishedName, vaultsettings.ClientId, vaultsettings.KeyVaultUrl, "Skatteverket-Api-ClientId", vaultsettings.StoreLocation, vaultsettings.TenantId);
                gw_ClientSecret = gw_ClientSecret ?? KeyVaultSecretsFetcher.GetSecret(vaultsettings.CertificateDistinguishedName, vaultsettings.ClientId, vaultsettings.KeyVaultUrl, "Skatteverket-Api-Secret", vaultsettings.StoreLocation, vaultsettings.TenantId);

                request.AddHeader("client_id", gw_ClientId);
                request.AddHeader("client_secret", gw_ClientSecret);
                request.AddHeader("authorization", "Bearer " + token.Access_token);
                request.AddHeader("skv-client_correlationid", Guid.NewGuid().ToString().Replace("-", ""));
                request.AddHeader("content-type", "application/json");
                RestResponse response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    errorMessage = response.Content;

                if (string.IsNullOrEmpty(errorMessage) && !response.Content.ToLower().Contains("felbeskrivning"))
                {
                    var content = response.Content.Replace("giltig-from", "giltigfrom").Replace("giltig-tom", "giltigtom");
                    List<CsrResponseDTO> dtos = JsonConvert.DeserializeObject<List<CsrResponseDTO>>(content);
                    dtos.ForEach(dto => dto.Year = year);
                    return dtos;
                }
                else if (response.Content.ToLower().Contains("felbeskrivning"))
                {
                    csrfel fel = JsonConvert.DeserializeObject<csrfel>(response.Content);
                    return new List<CsrResponseDTO> { new CsrResponseDTO($"({fel.felkod}) {fel.felbeskrivning}") };
                }
                else
                {
                    List<CsrResponseDTO> list = new List<CsrResponseDTO>();
                    foreach (var person in skatteAvdragFleraPersoner.personnummer)
                    {
                        list.Add(GetFOS(orgNbr, person, year));
                    }

                    if (list.Any())
                        return list;

                    CsrErrorResponse error = JsonConvert.DeserializeObject<CsrErrorResponse>(response.Content);

                    return new List<CsrResponseDTO> { new CsrResponseDTO($"({error.felkod}) {error.felmeddelande}") };
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "GetCsrReponse");
                return new List<CsrResponseDTO> { new CsrResponseDTO("Request failed, error logged") };
            }
        }

        private static Uri GetUriFOS()
        {
            if (ConfigurationSetupUtil.GetSiteType() == TermGroup_SysPageStatusSiteType.Test)
                return new Uri("https://api.test.skatteverket.se/inkomstbeskattning/fraga-om-skatteavdrag/v2/");
            else
                return new Uri("https://api.skatteverket.se/inkomstbeskattning/fraga-om-skatteavdrag/v2/");
        }

        private static string FOS_Auth_ClientId;
        private static string FOS_Auth_ClientSecret;
        private static DateTime? FOS_ClientandSecretUpdated;
        private static OAuthAccessToken FOS_Auth_Token;
        private static DateTime? FOS_Auth_TokenExpires;

        public static OAuthAccessToken GetToken()
        {
            if (FOS_Auth_Token != null && FOS_Auth_TokenExpires.HasValue && FOS_Auth_TokenExpires.Value > DateTime.Now)
                return FOS_Auth_Token;

            var vaultsettings = KeyVaultSettingsHelper.GetKeyVaultSettings();

            if (!FOS_ClientandSecretUpdated.HasValue || FOS_ClientandSecretUpdated.Value.AddMinutes(60) < DateTime.Now || FOS_Auth_ClientId == null || FOS_Auth_ClientSecret == null)
            {
                FOS_Auth_ClientId = KeyVaultSecretsFetcher.GetSecret(vaultsettings.CertificateDistinguishedName, vaultsettings.ClientId, vaultsettings.KeyVaultUrl, "Skatteverket-Auth-ClientId", vaultsettings.StoreLocation, vaultsettings.TenantId);
                FOS_Auth_ClientSecret = KeyVaultSecretsFetcher.GetSecret(vaultsettings.CertificateDistinguishedName, vaultsettings.ClientId, vaultsettings.KeyVaultUrl, "Skatteverket-Auth-Secret", vaultsettings.StoreLocation, vaultsettings.TenantId);
                FOS_ClientandSecretUpdated = DateTime.Now;
            }

            var body = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "scope", "fos" },
                { "client_id", FOS_Auth_ClientId},
                { "client_secret", FOS_Auth_ClientSecret }
            };

            string tokenUrl = "https://sysoauth2.skatteverket.se/oauth2/v1/sys/token";
            if (ConfigurationSetupUtil.GetSiteType() == TermGroup_SysPageStatusSiteType.Test)
                tokenUrl = "https://sysoauth2.test.skatteverket.se/oauth2/v1/sys/token";

            using (HttpClient httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
                request.Content = new FormUrlEncodedContent(body);
                var response = httpClient.SendAsync(request).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                FOS_Auth_Token = JsonConvert.DeserializeObject<OAuthAccessToken>(content);
                FOS_Auth_TokenExpires = DateTime.Now.AddSeconds(FOS_Auth_Token.Expires_in - 60 * 20);
                return FOS_Auth_Token;
            }
        }

        #endregion

        #region Help

        public static RestClient GetRestClientWithNewtonsoftJson(Uri uri, int maxTimeOut = 0)
        {
            return GetRestClientWithNewtonsoftJson(uri.ToString(), maxTimeOut);
        }

        public static RestClient GetRestClientWithNewtonsoftJson(string uri, int maxTimeOut = 0)
        {
            RestClientOptions options = new RestClientOptions(new Uri(uri));
            if (maxTimeOut > 0)
                options.Timeout = TimeSpan.FromMilliseconds(maxTimeOut);
            var client = new GoRestClient(options, configureSerialization: s => s.UseNewtonsoftJson());
            return client;
        }

        private static RestRequest CreateRequest(string resource, RestSharp.Method method, object obj = null)
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;

            try
            {
                if (obj != null)
                {
                    request.AddJsonBody(obj);
                }
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
            }

            //  request.AddHeader("Token", "e8d7bf57fd1b44a684689cfce813f783");
            return request;
        }

        public static void LogError(Exception ex, string message = "")
        {
            string error = message + " " + ex.ToString();
            error = error.ToString().Length > 800 ? error.ToString().Substring(0, 800) : error.ToString();
            SysLogConnector.SaveErrorMessage(error);
        }

        public static void LogError(string message)
        {
            string error = Environment.MachineName + " " + message;
            error = error.ToString().Length > 800 ? error.ToString().Substring(0, 800) : error.ToString();
            SysLogConnector.SaveErrorMessage(error);
        }

        #endregion
    }


    public class csrfel
    {
        public string personnummer { get; set; }
        public int felkod { get; set; }
        public string felbeskrivning { get; set; }
    }


    public class CsrErrorResponse
    {
        public int felkod { get; set; }
        public string felmeddelande { get; set; }
    }

}
