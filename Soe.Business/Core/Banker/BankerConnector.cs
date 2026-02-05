using Banker.Shared;
using Banker.Shared.DTO;
using Banker.Shared.Types;
using Newtonsoft.Json;
using RestSharp;
using SoftOne.Soe.Business.Util.WebApiInternal;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.IO;
using System.Text;

namespace SoftOne.Soe.Business.Core.Banker
{
    public class BankerConnector
    {
        private static readonly BankerClientSettings clientSettings;
        //private string _token;
        static BankerConnector()
        {
            if (clientSettings == null)
            {
                clientSettings = new BankerClientSettings("1000");
            }
        }

        #region Admin API
        private static string GetAPIToken()
        {
            return ConnectorBase.GetAccessToken();
        }

        private static RestClient CreateAPIClient(bool testMode)
        {
            var client = new GoRestClient(clientSettings.GetBaseApiUrl(testMode
//#if DEBUG
//                , "http://localhost:5199/"
//#endif
            ));

            return client;
        }

        public static RestRequest CreateAPIRequest(string resource, Method method, string token)
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;

            //request.AddParameter(parameter.Key, parameter.Value, ParameterType.QueryString);

            request.AddHeader("Authorization", "Bearer " + token);
            return request;

        }

        public static List<SoeBankerDownloadFileDTO> GetDownloadFiles(SettingManager settingManager, int requestId)
        {
            bool testMode = GetTestMode(settingManager);

            var client = CreateAPIClient(testMode);
            var request = CreateAPIRequest("api/Banker/DownloadRequestFiles", Method.Post, GetAPIToken());
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(new BankerDownloadRequestFileFilterDTO
            {
                AvaloDownloadRequestId = requestId,
            });

            var response = client.Execute<ActionResult>(request);
            var downloadRequestDTOs = JsonConvert.DeserializeObject<BankerDownloadedFileDTO[]>(response.Content);

            var files = downloadRequestDTOs != null ? downloadRequestDTOs.Select(x => new SoeBankerDownloadFileDTO
            {
                StatusCode = x.StatusCode,
                Status = x.Status,
                Message = x.Message,
                ActorCompanyId = x.ActorCompanyId,
            }).ToList() : new List<SoeBankerDownloadFileDTO>();

            return files;
        }
        public static List<SoeBankerDownloadRequestDTO> GetDownloadRequest(SettingManager settingManager, SoeBankerRequestFilterDTO filter)
        {
            bool testMode = GetTestMode(settingManager);

            var client = CreateAPIClient(testMode);
            var request = CreateAPIRequest("api/Banker/DownloadRequests", Method.Post, GetAPIToken());
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(new BankerDownloadRequestFilterDTO
            {
                CreatedFrom = filter.FromDate,
                CreatedTo = filter.ToDate,
                MaterialType = filter.MaterialType.HasValue ? (MaterialType)filter.MaterialType.Value : MaterialType.Unknown,
                ErrorStatus = filter.OnlyError.GetValueOrDefault(),
                StatusCode = filter.StatusCodes.Select(x => (FileProcessStatus)x).ToList()     
            });

            var response = client.Execute<ActionResult>(request);

            List<SoeBankerDownloadRequestDTO> files;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var downloadRequestDTOs = JsonConvert.DeserializeObject<BankerDownloadRequestDTO[]>(response.Content);
                files = downloadRequestDTOs.Select(x => new SoeBankerDownloadRequestDTO
                {
                    AvaloDownloadRequestId = x.AvaloDownloadRequestId,
                    Created = x.Created,
                    Modified = x.Modified,
                    MaterialCode = x.MaterialType,
                    Material = x.Material,
                    StatusCode = x.StatusCode,
                    Status = x.Status,
                    Message = x.Message,
                    BankName = x.BankName
                }).OrderByDescending(x => x.Created).ToList();
            }
            else
            {
                files = new List<SoeBankerDownloadRequestDTO>();
            }
            return files.OrderByDescending(x => x.Created).ToList();
        }

        public static List<SoeBankerOnboardingDTO> GetOnboardingRequests(SettingManager settingManager)
        {
            bool testMode = GetTestMode(settingManager);

            var client = CreateAPIClient(testMode);
            var request = CreateAPIRequest("api/Banker/OnboardingRequests", Method.Post, GetAPIToken());
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(new BankerOnboardingFilterDTO
            {
                CreatedFrom = new DateTime(2023, 1, 1),
            });

            var response = client.Execute<ActionResult>(request);

            List<SoeBankerOnboardingDTO> files;
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var onboardingRequestDTOs = JsonConvert.DeserializeObject<BankerOnboardingRequestDTO[]>(response.Content);

                files = onboardingRequestDTOs.Select(x => new SoeBankerOnboardingDTO
                {
                    Status = x.Status,
                    StatusCode = x.StatusCode,
                    RegAction = x.RegAction,
                    RegActionCode = x.RegActionCode,
                    CompanyMasterOrgNr = x.CompanyMasterOrgNr,
                    CompanyName = x.CompanyName,
                    Message = x.Message,
                    BankAccounts = x.BankAccounts,
                    CompanyOrgNr = x.CompanyOrgNr,
                    Created = x.Created,
                    Modified = x.Modified,
                    OnBoardingRequestId = x.OnBoardingRequestId,
                    Emails = x.Emails,
                    BankName = x.BankName,
                    SigningTypeName = x.SigningTypeName,
                }).OrderByDescending(x => x.Created).ToList();
            }
            else
            {
                files = new List<SoeBankerOnboardingDTO>();
            }

            return files;
        }
        #endregion

        #region Upload/Downloadning

        public static ActionResult UploadFinvoiceFile(SettingManager settingManager, int actorCompanyId, string companyGuid, int? sysCompDBId, SysBankDTO sysBank, string msgId, byte[] data)
        {

            bool testMode = GetTestMode(settingManager);

            if (sysCompDBId.GetValueOrDefault() == 0)
                throw new Exception("UploadPaymentFile, sysCompDBId is null");

            var uploadRequest = new MaterialUploadRequest
            {
                FileContent = Convert.ToBase64String(data),
                Bank = GetBank(sysBank),
                MaterialType = MaterialType.FinvoiceSend,
                Country = GetCountry(sysBank),
                ActorcompanyId = actorCompanyId,
                CompanyGuid = companyGuid,
                SysCompDBId = sysCompDBId.GetValueOrDefault(),
                MsgId = msgId
            };

            return Upload(uploadRequest.CloneDTO(), testMode);
        }

        public static ActionResult UploadFinvoiceAttachmentFile(SettingManager settingManager, int actorCompanyId, string companyGuid, int? sysCompDBId, SysBankDTO sysBank, string msgId, byte[] data)
        {
            bool testMode = GetTestMode(settingManager);

            if (sysCompDBId.GetValueOrDefault() == 0)
                throw new Exception("UploadPaymentFile, sysCompDBId is null");

            var uploadRequest = new MaterialUploadRequest
            {
                FileContent = Convert.ToBase64String(data),
                Bank = GetBank(sysBank),
                MaterialType = MaterialType.FinvoiceAttachmentSend,
                Country = GetCountry(sysBank),
                ActorcompanyId = actorCompanyId,
                CompanyGuid = companyGuid,
                SysCompDBId = sysCompDBId.GetValueOrDefault(),
                MsgId = msgId
            };

            return Upload(uploadRequest.CloneDTO(), testMode);
        }
        public static ActionResult UploadPaymentFile(SettingManager settingManager, PaymentMethod paymentMethod, int actorCompanyId,string companyGuid, int? sysCompDBId, SysBankDTO sysBank, string msgId, byte[] data)
        {

            bool testMode = GetTestMode(settingManager);

            if ((sysBank == null) || !sysBank.HasIntegration)
            {
                return new ActionResult(settingManager.GetText(7674, 1, "Vald bank är ännu inte godkänd för bankintegration") + $"({sysBank?.Name}:{sysBank?.BIC})");
            }

            if (!paymentMethod.PaymentInformationRow.BankConnected)
            {
                return new ActionResult(settingManager.GetText(7675, 1, "Valt bankkonto är inte aktiverad för bankintegration") + $"({paymentMethod.PaymentInformationRow.BIC}:{paymentMethod.PaymentInformationRow.PaymentNr})");
            }

            if (string.IsNullOrEmpty(msgId))
            {
                return new ActionResult("Payment messageid is not set");
            }

            if (sysCompDBId.GetValueOrDefault() == 0)
                throw new Exception("UploadPaymentFile, sysCompDBId is null");

#if DEBUG
            File.WriteAllBytes(@"c:\Temp\banker\paymentupload_" + msgId + ".xml", data);
#endif

            var uploadRequest = new MaterialUploadRequest
            {
                FileContent = Convert.ToBase64String(data),
                Bank = GetBank(sysBank),
                MaterialType = MaterialType.Payment,
                Country = GetCountry(sysBank),
                ActorcompanyId = actorCompanyId,
                CompanyGuid = companyGuid,
                SysCompDBId = sysCompDBId.GetValueOrDefault(),
                MsgId = msgId
            };

            return Upload(uploadRequest.CloneDTO(), testMode);
        }

        public ActionResult DownloadPaymentFeedback(SettingManager settingManager, SysBankDTO sysBank)
        {
            bool testMode = GetTestMode(settingManager);

            var downloadRquest = new MaterialDownloadRequest
            {
                Status = "New",
                Bank = GetBank(sysBank),
                Country = GetCountry(sysBank),
                MaterialType = MaterialType.PaymentFeedback002,
            };

            return Download(downloadRquest, testMode);
        }

        public ActionResult DownloadPayment(SettingManager settingManager, SysBankDTO sysBank)
        {
            bool testMode = GetTestMode(settingManager);

            var downloadRquest = new MaterialDownloadRequest
            {
                Status = "New",
                Bank = GetBank(sysBank),
                Country = GetCountry(sysBank),
            };

            if (UseCamt53(sysBank))
            {
                downloadRquest.MaterialType = MaterialType.DebetCreditNotification053;
            }
            else
            {
                downloadRquest.MaterialType = MaterialType.CreditNotification054;
            }

            return Download(downloadRquest, testMode);
        }

        public ActionResult DownloadFinvoiceFeedback(SettingManager settingManager, SysBankDTO sysBank)
        {
            bool testMode = GetTestMode(settingManager);

            var downloadRquest = new MaterialDownloadRequest
            {
                Status = "New",
                Bank = GetBank(sysBank),
                Country = GetCountry(sysBank),
                MaterialType = MaterialType.FinvoiceFeedback,
            };

            return Download(downloadRquest, testMode);
        }

        public ActionResult DownloadFinvoice(SettingManager settingManager, SysBankDTO sysBank)
        {
            bool testMode = GetTestMode(settingManager);

            var downloadRquest = new MaterialDownloadRequest
            {
                Status = "New",
                Bank = GetBank(sysBank),
                Country = GetCountry(sysBank),
                MaterialType = MaterialType.FinvoiceDownload,
            };

            return Download(downloadRquest, testMode);
        }

        public ActionResult DownloadFinvoiceAttachment(SettingManager settingManager, SysBankDTO sysBank)
        {
            bool testMode = GetTestMode(settingManager);

            var downloadRquest = new MaterialDownloadRequest
            {
                Status = "New",
                Bank = GetBank(sysBank),
                Country = GetCountry(sysBank),
                MaterialType = MaterialType.FinvoiceAttachmentDownload,
            };

            return Download(downloadRquest, testMode);
        }

        public ActionResult DownloadOnboardingfiles(SettingManager settingManager, SysBankDTO sysBank, MaterialType materialType)
        {
            bool testMode = GetTestMode(settingManager);

            var downloadRquest = new MaterialDownloadRequest
            {
                Status = "New",
                Bank = GetBank(sysBank),
                Country = GetCountry(sysBank),
                MaterialType = materialType
            };

            return Download(downloadRquest, testMode);
        }

        public ActionResult TryReimportDownloadErrors(SettingManager settingManager)
        {
            bool testMode = GetTestMode(settingManager);

            var downloadRquest = new MaterialDownloadRequest
            {
                Status = "Error",
                Bank = Bank.Unknown,
                Country = Country.Unknown,
                MaterialType = MaterialType.Error,
            };

            return Download(downloadRquest, testMode);
        }
        public bool HasOnboardingFile(SysBankDTO sysBank, out MaterialType type)
        {
            if (sysBank.SysBankId == 1) // Handelsbanken sverige
            {
                type = MaterialType.OnboardingSHB;
                return true;
            }
            else if (sysBank.SysBankId == 5)  // SEB sverige
            {
                type = MaterialType.OnboardingAcmt14;
                return true;
            }
            else
            {
                type = MaterialType.Unknown;
                return false;
            }
        }

        public static ActionResult SendOnboardingAuthorizationResponse(SettingManager setting, string requestingUserName, List<int> onboardingRequestIds)
        {
            var onboardingFiles = new List<BankerGenerateOnboardingFileResponseDTO>();
            foreach (var id in onboardingRequestIds)
            {
                // Generate onboarding acknowledgement request for each id
                try
                {
                    onboardingFiles.Add(GenerateOnboardingResponse(setting, id, requestingUserName));
                }
                catch (Exception ex)
                {
                    return new ActionResult(ex);
                }
            }

            foreach (var onboardingRequest in onboardingFiles)
            {
                foreach (var authorizationFile in onboardingRequest.AuthorizationFiles)
                {
                    var uploadRequest = new MaterialUploadRequest
                    {
                        MsgId = Guid.NewGuid().ToString(),
                        FileContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(authorizationFile)),
                        OnboardingRequestId = onboardingRequest.OnboardingRequestId,
                        Bank = onboardingRequest.Bank,
                        Country = onboardingRequest.Country,
                        MaterialType = onboardingRequest.MaterialType,
                    };
                    var uploadResponse = Upload(uploadRequest.CloneDTO(), GetTestMode(setting));
                    if (!uploadResponse.Success)
                    {
                        uploadResponse.ErrorMessage = $"Failed uploading onboarding acknowledgement for OnboardingRequestId {onboardingRequest.OnboardingRequestId}: {uploadResponse.ErrorMessage}";
                        return uploadResponse;
                    }
                }
            }

            return new ActionResult($"Onboarding authorization responses queued for {onboardingRequestIds.Count} onboarding requests")
            {
                Success = true
            };
        }

        public static BankerGenerateOnboardingFileResponseDTO GenerateOnboardingResponse(SettingManager settingManager, int onboardingRequestId, string requestingUserName)
        {
            bool testMode = GetTestMode(settingManager);

            var client = CreateAPIClient(testMode);
            var request = CreateAPIRequest("api/Banker/GenerateOnboardingResponse", Method.Post, GetAPIToken());
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(new BankerGenerateOnboardingResponseParametersDTO
            {
                OnboardingRequestId = onboardingRequestId,
                CreatedBy = requestingUserName
            });

            var response = client.Execute<ActionResult>(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<BankerGenerateOnboardingFileResponseDTO>(response.Content);
            }
            else
            {
                try
                {
                    var error = JsonConvert.DeserializeObject<string>(response.Content);
                    throw new Exception($"Error generating onboarding response: {error}");

                }
                catch (Exception ex) {
                    throw new Exception($"{ex.Message}");
                }
            }
        }

        private static ActionResult Upload(MaterialUploadRequest uploadRequest, bool testMode)
        {
            var result = new ActionResult(false);

            try
            {
                var client = new GoRestClient(clientSettings.GetBaseFuncUrl(testMode
#if DEBUG
                    , "http://localhost:7071/"
#endif
                ));

                var request = new RestRequest(BankerClientSettings.MaterialUploadUrl, Method.Post);
                request.RequestFormat = DataFormat.Json;

                request.AddHeader("x-functions-key", GetFunctionKey(testMode));

                request.AddJsonBody(uploadRequest);
                RestResponse<ActionResult> response = client.Execute<ActionResult>(request);

                if (response.IsSuccessful)
                {
                    var bankerResult = JsonConvert.DeserializeObject<BankerActionResult>(response.Content);
                    result.Success = bankerResult.Success;
                    result.ErrorMessage = bankerResult.Message;
                    result.IntegerValue = bankerResult.Id;
                }
                else
                {
                    result.ErrorMessage = string.IsNullOrEmpty(response.ErrorMessage) ? $"{response.StatusCode}: {response.Content}" : $"{response.StatusCode}: {response.StatusDescription} ( {response.ErrorMessage} )";
                    result.ErrorNumber = (int)response.StatusCode;
                }

                if (!result.Success && string.IsNullOrEmpty(result.ErrorMessage))
                {
                    result.ErrorMessage = "Failed calling Banker";
                }

            }
            catch (Exception ex)
            {
                return new ActionResult(ex);
            }

            return result;
        }

        private static ActionResult Download(MaterialDownloadRequest downloadRequest, bool testMode)
        {
            var result = new ActionResult(false);

            // Bank.Unknown is OK when we are sending the retry import error request.
            if (downloadRequest.MaterialType != MaterialType.Error && downloadRequest.Bank == Bank.Unknown)
            {
                return new ActionResult("Unknown bank for download request");
            }

            try
            {
                var client = new GoRestClient(clientSettings.GetBaseFuncUrl(testMode
#if DEBUG
                    , "http://localhost:7071/"
#endif
                ));

                var request = new RestRequest(BankerClientSettings.MaterialDownloadUrl, Method.Post);
                request.RequestFormat = DataFormat.Json;

                request.AddHeader("x-functions-key", GetFunctionKey(testMode));

                request.AddJsonBody(downloadRequest);
                RestResponse<ActionResult> response = client.Execute<ActionResult>(request);

                if (response.IsSuccessful)
                {
                    var bankerResult = JsonConvert.DeserializeObject<BankerActionResult>(response.Content);
                    result.Success = bankerResult.Success;
                    result.ErrorMessage = bankerResult.Message;
                    result.IntegerValue = bankerResult.Id;
                }
                else
                {
                    result.ErrorMessage = string.IsNullOrEmpty(response.ErrorMessage) ? $"{response.StatusCode}: {response.Content}" : response.ErrorMessage;
                    result.ErrorNumber = (int)response.StatusCode;
                }
            }
            catch (Exception ex)
            {
                return new ActionResult(ex);
            }

            return result;
        }

        private static bool GetTestMode(SettingManager settingManager)
        {
            bool testMode = settingManager.isTest() || settingManager.isDev();
#if DEBUG
            testMode = true;
#endif

            return testMode;
        }

        private static string GetFunctionKey(bool testMode)
        {
            return (testMode) ? "k4JtodLvNS0bS327upwYsoL-_46sUXyjQE5FY9sFvBiwAzFuqIgBCg==" : "AUf4r51t82RGcP6nMtK93oy7JaYdFpHTjUSsAe2_KOIbAzFuxfd3Dg==";
        }

        #endregion

        #region Helpers

        private bool UseCamt53(SysBankDTO sysbank)
        {
            //handelsbanken sverige + nordea sverige
            if (sysbank.SysBankId == 1 || sysbank.SysBankId == 2)
            {
                return true;
            }

            return false;
        }
       
        private static Bank GetBank(SysBankDTO sysBank)
        {
            switch (sysBank.SysBankId)
            {
                case 1:
                case 9:
                    return Bank.Handelsbanken;
                case 2:
                case 7:
                    return Bank.Nordea;
                case 5:
                    return Bank.SEB;
                case 11:
                    return Bank.OP;
                default:
                    return Bank.Unknown;
            }
        }

        private static Country GetCountry(SysBankDTO sysBank)
        {
            switch (sysBank.SysCountryId)
            {
                case 1:
                    return Country.Sweden;
                case 3:
                    return Country.Finland;
                default:
                    return Country.Unknown;
            }
        }

        #endregion
    }
}
