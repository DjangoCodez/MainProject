using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO.SoftOneId;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using Common.DTO;

namespace SoftOne.Soe.Business.Core.SoftOneId
{
    public class SoftOneIdConnector
    {
        public static bool ValidateSuperKey(Guid guid, string superKey)
        {
            try
            {
                var client = new GoRestClient(GetUri());
                var request = CreateRequest("api/superkey/ValidateSuperKey", Method.Get, null);
                request.AddParameter("guid", guid, ParameterType.QueryString);
                request.AddParameter("superKey", superKey, ParameterType.QueryString);
                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "ValidateSuperKey");

                return JsonConvert.DeserializeObject<bool>(response.Content);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetSuperKey(Guid guid, IdSuperKeyType type)
        {
            var dto = GetSuperKeyDTO(guid, type);

            if (dto != null)
                return dto.SuperKey;

            return string.Empty;

        }

        public static bool CheckHealth()
        {
            var client = new GoRestClient(GetUri());
            var request = CreateRequest("", Method.Get, null);
            RestResponse response = client.Execute(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return true;

            return false;
        }

        public static Common.DTO.SoftOneId.IdSuperKeyDTO GetSuperKeyDTO(Guid guid, IdSuperKeyType type)
        {
            try
            {
                var client = new GoRestClient(GetUri());
                var request = CreateRequest("api/superkey/CreateAndSaveIdSuperKey", Method.Get, null);
                request.AddParameter("superKeyGuid", guid, ParameterType.QueryString);
                request.AddParameter("type", type, ParameterType.QueryString);
                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "GetSuperKeyDTO");

                return JsonConvert.DeserializeObject<Common.DTO.SoftOneId.IdSuperKeyDTO>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetSuperKeyDTO");
                return new Common.DTO.SoftOneId.IdSuperKeyDTO();
            }
        }

        public static bool SyncAllSitesInfostartdtos()
        {
            try
            {
                var client = new GoRestClient(GetUri());
                var request = CreateRequest("api/idlogin/allsitesinfostartdtos", Method.Get, null);
                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "SyncAllSitesInfostartdtos");

                return JsonConvert.DeserializeObject<bool>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "SyncAllSitesInfostartdtos");
                return false;
            }
        }

        public static ActionResult AddIdLogin(Guid idLoginGuid, string email, string phonenumber, int licenseId, int sysCompDbId, string externalAuthId = null, int? language = null)
        {
            try
            {
                var options = new RestClientOptions(GetUri());
                var client = new GoRestClient(options);

                bool succeded = false;
                try
                {
                    IdLoginConfidential confidential = new IdLoginConfidential()
                    {
                        IdLoginGuid = idLoginGuid,
                        Email = email,
                        PhoneNumber = phonenumber,
                        LicenseId = licenseId,
                        SysCompDbId = sysCompDbId,
                        ExternalAuthId = externalAuthId,
                        Language = language,
                    };

                    var requestPost = CreateRequest("api/idlogin/addidlogin", Method.Post, confidential);
                    requestPost.AddParameter("superKey", GetSuperKey(idLoginGuid, IdSuperKeyType.SoftOneIdRequest));
                    RestResponse responsePost = client.Execute(requestPost);

                    if (responsePost.StatusCode == System.Net.HttpStatusCode.OK)
                        succeded = true;

                    if (responsePost.ErrorException != null)
                    {
                        LogError(responsePost.ErrorException, "AddIdLogin");
                        return new ActionResult(responsePost.ErrorException);
                    }
                    return JsonConvert.DeserializeObject<ActionResult>(responsePost.Content);
                }
                catch
                {
                    // Always continue
                }

                if (!succeded)
                {
                    var request = CreateRequest("api/idlogin/addidlogin", Method.Get, null);
                    request.AddParameter("idLoginGuid", idLoginGuid, ParameterType.QueryString);
                    request.AddParameter("superKey", GetSuperKey(idLoginGuid, IdSuperKeyType.SoftOneIdRequest));
                    request.AddParameter("email", email, ParameterType.QueryString);
                    request.AddParameter("phonenumber", phonenumber, ParameterType.QueryString);
                    RestResponse responseGet = client.Execute(request);
                    return JsonConvert.DeserializeObject<ActionResult>(responseGet.Content);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "AddIdLogin");
            }

            return new ActionResult("failed");
        }

        public static ActionResult AddExternalAuthId(Guid idLoginGuid, string externalAuthId, Guid idProviderGuid)
        {
            try
            {
                IdLoginConfidential confidential = new IdLoginConfidential()
                {
                    IdLoginGuid = idLoginGuid,
                    Confidential = externalAuthId,
                    IdProviderGuid = idProviderGuid
                };
                var client = new GoRestClient(GetUri());
                var request = CreateRequest("api/idlogin/ExternalAuthId", Method.Post, confidential);
                request.AddParameter("superKey", GetSuperKey(idLoginGuid, IdSuperKeyType.SoftOneIdRequest));
                RestResponse response = client.Execute(request);
                if (response.ErrorException != null)
                {
                    LogError(response.ErrorException, "AddExternalAuthId");
                    return new ActionResult(response.ErrorException);
                }
                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "AddExternalAuthId");
                return new ActionResult(ex);
            }
        }

        public static IdLoginConfidential GetIdLoginGuidFromUserLinkConnectionKey(string userLinkConnectionKey, string licenseNr, string email)
        {
            try
            {
                if (userLinkConnectionKey.Contains(licenseNr.Trim() + "##"))
                {
                    Guid guid = Guid.NewGuid();
                    IdLoginConfidential confidential = new IdLoginConfidential()
                    {
                        IdLoginGuid = guid,
                        Confidential = userLinkConnectionKey,
                        Email = email,
                        IntValue = Convert.ToInt32(licenseNr)
                    };
                    var client = new GoRestClient(GetUri());
                    var request = CreateRequest("api/idlogin/UserLinkConnectionKey", Method.Post, confidential);
                    request.AddParameter("superKey", GetSuperKey(guid, IdSuperKeyType.SoftOneIdRequest));
                    RestResponse response = client.Execute(request);
                    if (response.ErrorException != null)
                    {
                        LogError(response.ErrorException, "GetIdLoginGuidFromUserLinkConnectionKey");
                        return null;
                    }
                    return JsonConvert.DeserializeObject<IdLoginConfidential>(response.Content);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "GetIdLoginGuidFromUserLinkConnectionKey");
                return null;
            }

            return new IdLoginConfidential() { UserName = "Invalid license" };
        }

        public static IdLoginConfidential GetIdLoginGuidFromUserMergeKey(string mergeLoginKey, Guid idLoginGuid, string email)
        {
            try
            {
                IdLoginConfidential confidential = new IdLoginConfidential()
                {
                    IdLoginGuid = idLoginGuid,
                    Confidential = mergeLoginKey,
                    Email = email,
                };
                var client = new GoRestClient(GetUri());
                var request = CreateRequest("api/idlogin/UserMergeKey", Method.Post, confidential);
                request.AddParameter("superKey", GetSuperKey(idLoginGuid, IdSuperKeyType.SoftOneIdRequest));
                RestResponse response = client.Execute(request);
                if (response.ErrorException != null)
                {
                    LogError(response.ErrorException, "GetIdLoginGuidFromUserMergeKey");
                    return null;
                }
                return JsonConvert.DeserializeObject<IdLoginConfidential>(response.Content);

            }
            catch (Exception ex)
            {
                LogError(ex, "GetIdLoginGuidFromUserMergeKey");
                return null;
            }
        }

        public static ActionResult AddLifetimeSeconds(Guid idLoginGuid, int lifeTimeSeconds)
        {
            try
            {
                IdLoginConfidential confidential = new IdLoginConfidential()
                {
                    IdLoginGuid = idLoginGuid,
                    IntValue = lifeTimeSeconds
                };
                var client = new GoRestClient(GetUri());
                var request = CreateRequest("api/idlogin/LifeTimeSeconds", Method.Post, confidential);
                request.AddParameter("superKey", GetSuperKey(idLoginGuid, IdSuperKeyType.SoftOneIdRequest));
                RestResponse response = client.Execute(request);
                if (response.ErrorException != null)
                {
                    LogError(response.ErrorException, "AddLifetimeSeconds");
                    return new ActionResult(response.ErrorException);
                }
                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "AddLifetimeSeconds");
                return new ActionResult(ex);
            }
        }

        public static int GetLifetimeSeconds(Guid idLoginGuid)
        {
            try
            {
                var options = new RestClientOptions(GetUri());
                var client = new GoRestClient(options);
                var request = CreateRequest("api/idlogin/LifeTimeSeconds", Method.Get, null);
                request.AddParameter("superKey", GetSuperKey(idLoginGuid, IdSuperKeyType.SoftOneIdRequest));
                request.AddParameter("idLoginGuid", idLoginGuid, ParameterType.QueryString);
                RestResponse response = client.Execute(request);
                return JsonConvert.DeserializeObject<int>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetLifetimeSeconds");
                return 0;
            }
        }

        public static CommunicatorMessage SaveSignature(CommunicatorMessage message)
        {
            try
            {
                var options = new RestClientOptions(GetUri());
                var client = new GoRestClient(options);
                var request = CreateRequest("api/sign/Signature", Method.Post, null);
                request.AddJsonBody(message);
                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "SaveSignature");

                return JsonConvert.DeserializeObject<CommunicatorMessage>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "SaveSignature");
                return null;
            }
        }

        public static List<SignatureStatusDTO> GetSignatureStatuses(int sysCompDbid, int licenseId, DateTime from, DateTime to)
        {
            try
            {
                var options = new RestClientOptions(GetUri());

                var client = new GoRestClient(options);
                var request = CreateRequest("api/sign/SignatureStatus", Method.Get, null);
                request.AddParameter("sysCompDbid", sysCompDbid, ParameterType.QueryString);
                request.AddParameter("licenseId", licenseId, ParameterType.QueryString);
                request.AddParameter("from", from, ParameterType.QueryString);
                request.AddParameter("to", to, ParameterType.QueryString);
                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "GetSignatureStatuses");

                return JsonConvert.DeserializeObject<List<SignatureStatusDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetSignatureStatuses");
                return null;
            }
        }

        public static ActionResult UpdateSignatureCode(string oldCode, string newCode, DateTime oldValidTo, DateTime newValidTo)
        {
            try
            {
                var options = new RestClientOptions(GetUri());

                var client = new GoRestClient(options);
                var request = CreateRequest("api/sign/UpdateSignatureCode", Method.Get, null);
                request.AddParameter("oldCode", oldCode, ParameterType.QueryString);
                request.AddParameter("newCode", newCode, ParameterType.QueryString);
                request.AddParameter("oldValidTo", oldValidTo, ParameterType.QueryString);
                request.AddParameter("newValidTo", newValidTo, ParameterType.QueryString);
                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "UpdateSignatureCode");

                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "UpdateSignatureCode");
                return new ActionResult(ex);
            }
        }

        public static ActionResult AddidProviderGuid(IdLoginConfidential confidential)
        {
            try
            {
                var options = new RestClientOptions(GetUri());
                var client = new GoRestClient(options);
                var request = CreateRequest("api/license/AddIdProviderGuid", Method.Post, confidential);
                request.AddParameter("superKey", GetSuperKey(confidential.IdProviderGuid, IdSuperKeyType.SoftOneIdRequest));
                RestResponse response = client.Execute(request);
                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "AddidProviderGuid");

                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "AddidProviderGuid");
                return new ActionResult(ex);
            }
        }

        public static ActionResult AddSoftForceSetting(IdLoginConfidential confidential)
        {
            try
            {
                var options = new RestClientOptions(GetUri());
                var client = new GoRestClient(options);
                var request = CreateRequest("api/license/AddSoftForceSetting", Method.Post, confidential);
                request.AddParameter("superKey", GetSuperKey(confidential.IdProviderGuid, IdSuperKeyType.SoftOneIdRequest));
                RestResponse response = client.Execute(request);
                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "AddSoftForceSetting");

                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "AddidProviderGuid");
                return new ActionResult(ex);
            }
        }

        public static ActionResult AddSkipActivationEmail(IdLoginConfidential confidential)
        {
            try
            {
                var options = new RestClientOptions(GetUri());

                var client = new GoRestClient(options);
                var request = CreateRequest("api/license/AddSkipActivationEmail", Method.Post, confidential);
                request.AddParameter("superKey", GetSuperKey(confidential.IdProviderGuid, IdSuperKeyType.SoftOneIdRequest));
                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "AddSkipActivationEmail");

                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "AddidProviderGuid");
                return new ActionResult(ex);
            }
        }

        public static int GetChosenMobileApiEndPointSoeLicenseId(Guid idLoginGuid)
        {
            try
            {
                var options = new RestClientOptions(GetUri());

                var client = new GoRestClient(options);
                var request = CreateRequest("api/idlogin/GetChosenMobileApiEndPointSoeLicenseId", Method.Get, null);
                request.AddParameter("superKey", Guid.NewGuid().ToString());
                request.AddParameter("idLoginGuid", idLoginGuid, ParameterType.QueryString);
                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "GetChosenMobileApiEndPointSoeLicenseId");

                return JsonConvert.DeserializeObject<int>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetChosenMobileApiEndPointSoeLicenseId");
                return 0;
            }
        }

        public static string GetExternalAuthId(Guid idLoginGuid, Guid idProviderGuid)
        {
            try
            {
                var options = new RestClientOptions(GetUri());

                var client = new GoRestClient(options);
                var request = CreateRequest("api/idlogin/ExternalAuthId", Method.Get, null);
                request.AddParameter("superKey", GetSuperKey(idLoginGuid, IdSuperKeyType.SoftOneIdRequest));
                request.AddParameter("idLoginGuid", idLoginGuid, ParameterType.QueryString);
                request.AddParameter("idProviderGuid", idProviderGuid, ParameterType.QueryString);
                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "GetExternalAuthId");

                return JsonConvert.DeserializeObject<string>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetExternalAuthId");
                return "";
            }
        }

        public static List<IdLoginConfidential> GetExternalAuthIds(Guid idProviderGuid, int licenseId, int sysCompDbId)
        {
            try
            {
                IdLoginConfidential confidential = new IdLoginConfidential()
                {
                    IdProviderGuid = idProviderGuid,
                    LicenseId = licenseId,
                    SysCompDbId = sysCompDbId,
                };

                var options = new RestClientOptions(GetUri());

                var client = new GoRestClient(options);
                var request = CreateRequest("api/idlogin/ExternalAuthIds", Method.Post, confidential);
                request.AddParameter("superKey", GetSuperKey(idProviderGuid, IdSuperKeyType.SoftOneIdRequest));
                request.AddParameter("idProviderGuid", idProviderGuid, ParameterType.QueryString);
                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "GetExternalAuthIds");

                return JsonConvert.DeserializeObject<List<IdLoginConfidential>>(response.Content) ?? new List<IdLoginConfidential>();
            }
            catch (Exception ex)
            {
                LogError(ex, "GetExternalAuthIds");
                return new List<IdLoginConfidential>();
            }
        }

        public static string GetLoginName(Guid idLoginGuid, Guid idProviderGuid)
        {
            try
            {
                var options = new RestClientOptions(GetUri());

                var client = new GoRestClient(options);
                var request = CreateRequest("api/idlogin/LoginName", Method.Get, null);
                request.AddParameter("superKey", GetSuperKey(idLoginGuid, IdSuperKeyType.SoftOneIdRequest));
                request.AddParameter("idLoginGuid", idLoginGuid, ParameterType.QueryString);
                request.AddParameter("idProviderGuid", idProviderGuid, ParameterType.QueryString);
                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "GetLoginName");

                return JsonConvert.DeserializeObject<string>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetLoginName from SoftOneId");
                return "";
            }
        }

        public static List<IdLoginConfidential> GetLoginNames(Guid idLoginGuid, int licenseId, int sysCompDbId)
        {
            try
            {
                IdLoginConfidential confidential = new IdLoginConfidential()
                {
                    IdLoginGuid = idLoginGuid,
                    LicenseId = licenseId,
                    SysCompDbId = sysCompDbId,
                };

                var options = new RestClientOptions(GetUri());
                var client = new GoRestClient(options);
                var request = CreateRequest("api/idlogin/LoginNames", Method.Post, confidential);
                request.AddParameter("superKey", GetSuperKey(idLoginGuid, IdSuperKeyType.SoftOneIdRequest));
                request.AddParameter("idProviderGuid", idLoginGuid, ParameterType.QueryString);
                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "GetLoginNames");

                return JsonConvert.DeserializeObject<List<IdLoginConfidential>>(response.Content) ?? new List<IdLoginConfidential>();
            }
            catch (Exception ex)
            {
                LogError(ex, "GetLoginNames from SoftOneId");
                return new List<IdLoginConfidential>();
            }
        }

        public static ActionResult SendForgottenUsername(Guid idLoginGuid)
        {
            try
            {
                var options = new RestClientOptions(GetUri());

                var client = new GoRestClient(options);
                var request = CreateRequest("api/idlogin/SendForgottenUsername", Method.Get, null);
                request.AddParameter("superKey", GetSuperKey(idLoginGuid, IdSuperKeyType.SoftOneIdRequest));
                request.AddParameter("idLoginGuid", idLoginGuid, ParameterType.QueryString);
                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "SendForgottenUsername");

                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "AddIdLogin");
                return new ActionResult(ex);
            }
        }

        public static void UpdateDomain(Guid idLoginGuid, int licenseId, int sysCompDbId, string newDomain, Guid? licenseGuid)
        {
            try
            {
                var options = new RestClientOptions(GetUri()) { Timeout = TimeSpan.FromMilliseconds(1000) };

                var client = new GoRestClient(options);
                var request = CreateRequest("api/license/UpdateDomain", Method.Get, null);
                request.AddParameter("guid", idLoginGuid, ParameterType.QueryString);
                request.AddParameter("superKey", GetSuperKey(idLoginGuid, IdSuperKeyType.SoftOneIdRequest));
                request.AddParameter("licenseId", licenseId, ParameterType.QueryString);
                request.AddParameter("sysCompDbId", sysCompDbId, ParameterType.QueryString);
                request.AddParameter("newDomain", newDomain, ParameterType.QueryString);
                request.AddParameter("licenseGuid", licenseGuid, ParameterType.QueryString);
                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "UpdateDomain");

                return;
            }
            catch (Exception ex)
            {
                LogError(ex, "UpdateDomain");
            }
        }
        public static ActionResult AddEmailandPhonenumberToIdlogin(Guid idLoginGuid, string email, string phonenumber)
        {
            try
            {
                var options = new RestClientOptions(GetUri());

                var client = new GoRestClient(options);
                var request = CreateRequest("api/idlogin/addemailandphonenumbertoidlogin", Method.Get, null);
                request.AddParameter("email", email, ParameterType.QueryString);
                request.AddParameter("phonenumber", phonenumber, ParameterType.QueryString);
                request.AddParameter("idLoginGuid", idLoginGuid, ParameterType.QueryString);
                request.AddParameter("superKey", GetSuperKey(idLoginGuid, IdSuperKeyType.SoftOneIdRequest));
                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "AddEmailandPhonenumberToIdlogin");

                return JsonConvert.DeserializeObject<ActionResult>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "SyncAllSitesInfostartdtos");
                return new ActionResult(ex);
            }
        }

        public static bool ValidateLogin(Guid idLoginGuid, string userName, string password)
        {
            try
            {
                IdLoginConfidential confidential = new IdLoginConfidential()
                {
                    IdLoginGuid = idLoginGuid,
                    Confidential = password,
                    UserName = userName
                };
                var options = new RestClientOptions(GetUri());

                var client = new GoRestClient(options);
                var request = CreateRequest("api/idlogin/validateLogin", Method.Post, confidential);
                request.AddParameter("superKey", GetSuperKey(idLoginGuid, IdSuperKeyType.SoftOneIdRequest));
                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "ValidateLogin");

                if (response != null && !string.IsNullOrEmpty(response.Content))
                    return JsonConvert.DeserializeObject<bool>(response.Content);
                else
                    return false;
            }
            catch (Exception ex)
            {
                LogError(ex, "ValidateLogin");
                return false;
            }
        }

        public static bool SendMessage(Guid idLoginGuid, string subject, string body)
        {
            try
            {
                var options = new RestClientOptions(GetUri());

                var client = new GoRestClient(options);
                var request = CreateRequest("api/idlogin/SendMessage", Method.Get);

                request.AddParameter("idLoginGuid", idLoginGuid, ParameterType.QueryString);
                request.AddParameter("superKey", GetSuperKey(idLoginGuid, IdSuperKeyType.SoftOneIdRequest));

                request.AddParameter("subject", subject, ParameterType.QueryString);
                request.AddParameter("body", body, ParameterType.QueryString);
                request.AddParameter("deliveryMethod", 1, ParameterType.QueryString);

                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "ValidateLogin");

                if (response != null && !string.IsNullOrEmpty(response.Content))
                {
                    var result = JsonConvert.DeserializeObject<ActionResultDto>(response.Content);
                    return result.Success;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                LogError(ex, "ValidateLogin");
                return false;
            }
        }

        public static Guid? GetIdLoginGuidUsingUsernameAndPassword(Guid guid, string userName, string password)
        {
            try
            {
                IdLoginConfidential confidential = new IdLoginConfidential()
                {
                    IdLoginGuid = guid,
                    Confidential = password,
                    UserName = userName
                };
                var options = new RestClientOptions(GetUri());

                var client = new GoRestClient(options);
                var request = CreateRequest("api/idlogin/getidloginguid", Method.Post, confidential);
                request.AddParameter("guid", guid, ParameterType.QueryString);
                request.AddParameter("superKey", GetSuperKey(guid, IdSuperKeyType.SoftOneIdRequest));
                RestResponse response = client.Execute(request);

                if (response?.ErrorException != null)
                    LogError(response.ErrorException, "GetidLoginGuid");

                if (response != null && !string.IsNullOrEmpty(response.Content))
                    return JsonConvert.DeserializeObject<Guid?>(response.Content);
                else
                    return null;
            }
            catch (Exception ex)
            {
                LogError(ex, "ValidateLogin");
                return null;
            }
        }

        private static RestRequest CreateRequest(string resource, Method method, object obj = null)
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;
            //request.JsonSerializer = NewtonsoftJsonSerializer.Default;

            try
            {
                if (obj != null)
                    request.AddObject(obj);//    request.AddJsonBody(obj);
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
            SysLogConnector.SaveErrorMessage(message + " " + ex.ToString());
        }

        public static Uri GetUri()
        {
            //Uri uri = new Uri("https://localhost:44316");

            //return uri;

            Uri uri;
            if (CompDbCache.Instance.SiteType == TermGroup_SysPageStatusSiteType.Test)
                uri = new Uri("https://gotest.softone.se");
            else
                uri = new Uri("https://go.softone.se");

            return uri;
        }
    }

    public class SendMessageToUserPayload
    {
        public SendMessageToUser Payload { get; set; }
        public Guid IdLoginGuid { get; set; }
        public string SuperKey { get; set; }
    }
    public class SendMessageToUser
    {
        public int DeliveryMethod { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }

    public class IdLoginConfidential
    {
        public int SysCompDbId { get; set; }
        public int LicenseId { get; set; }
        public Guid IdLoginGuid { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Confidential { get; set; }
        public string ConfidentialSecond { get; set; }
        public Guid IdProviderGuid { get; set; }
        public bool? BoolValue { get; set; }
        public int? IntValue { get; set; }
        public string ExternalAuthId { get; set; }
        /// <summary>
        /// Language for the user, -1 
        /// </summary>
        public int? Language { get; set; }
    }
}

