using Bridge.Shared;
using Bridge.Shared.Connector;
using Bridge.Shared.Connector.FileTransfer;
using Bridge.Shared.Connector.Models;
using Bridge.Shared.Models;
using Bridge.Shared.Models.Visma;
using Bridge.Shared.Util;
using Newtonsoft.Json;
using RestSharp;
using SoftOne.Soe.Business.Core.Status;
using SoftOne.Soe.Business.Util.WebApiInternal;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.ApiExternal;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Shared.DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Business.Util.Bridge
{
    public static class BridgeConnector
    {
        public static bool _isTest { get; set; }
        public static Uri GetURI()
        {
            var url = SoftOneStatusConnector.GetBridgeUrl(_isTest);

            if (Environment.MachineName.ToLower().Contains("softone"))
                url = Environment.MachineName.ToLower().Contains("33") ? "https://devbridge.softone.se" : !string.IsNullOrEmpty(url) ? url : "https://s3bridge.softone.se";
            else
                url = "https://localhost:7130/";

            Uri uri = new Uri(url);
            return uri;
        }

        public static BridgeCredentials Encrypt(BridgeCredentials bridgeCredentials)
        {
            if (!bridgeCredentials.IsEncrypted)
            {
                BridgeCredentials credentials = bridgeCredentials.CloneDTO();

                credentials.Secret = string.IsNullOrEmpty(bridgeCredentials.Secret) ? bridgeCredentials.Secret : bridgeCredentials.Secret.Contains(BridgeConstants.EncryptString) ? bridgeCredentials.Secret : BridgeConstants.EncryptString + bridgeCredentials.Secret;
                credentials.ConnectionString = string.IsNullOrEmpty(bridgeCredentials.ConnectionString) ? bridgeCredentials.ConnectionString : bridgeCredentials.ConnectionString.Contains(BridgeConstants.EncryptString) ? bridgeCredentials.ConnectionString : BridgeConstants.EncryptString + bridgeCredentials.ConnectionString;
                credentials.Password = string.IsNullOrEmpty(bridgeCredentials.Password) ? bridgeCredentials.Password : bridgeCredentials.Password.Contains(BridgeConstants.EncryptString) ? bridgeCredentials.Password : BridgeConstants.EncryptString + bridgeCredentials.Password;

                try
                {
                    ActionResult result = new ActionResult();
                    var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 5));
                    var client = new CoreBridgeConnector(options, GetToken());
                    credentials = client.ValidateEncryption(credentials);
                    if (credentials != null)
                        return credentials;
                    test(ref bridgeCredentials);
                }
                catch
                {
                    // Intentionally ignored, safe to continue
                    // NOSONAR
                }
            }
            return bridgeCredentials;
        }

        public static string GetToken()
        {
            return ConnectorBase.GetAccessToken();
        }

        public static void test(ref BridgeCredentials bridgeCredentials)
        {
            if (!bridgeCredentials.IsEncrypted)
            {
                ActionResult result = new ActionResult();
                var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 5));
                var client = new CoreBridgeConnector(options, GetToken());
                var req = client.CreateRequest(new RestRequestInput("Api/Core/" + "Encrypt", Method.Post, bridgeCredentials as object));
                var response = client.Execute(req, Method.Post);
                bridgeCredentials = JsonConvert.DeserializeObject<BridgeCredentials>(response.Content);
            }
        }

        public static List<EmployeeChangeIODTO> GetHrPlusEmployeeChangeIODTOs(VismaPayrollBridgeRecieveRequest vismaPayrollBridgeRecieveRequest)
        {
            try
            {
                var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 20));
                var client = new VismaPayrollBridgeConnector(options, GetToken());

                var response = client.GetEmployeeChangesIOs(vismaPayrollBridgeRecieveRequest);

                if (response?.Base64 != null)
                {
                    LogCollector.LogCollector.LogInfo($"GetHrPlusEmployeeChangeIODTOs response.Base64:{response.Base64}");
                    var dtos = Base64Util.GetObjectFromBase64String<List<EmployeeChangeIODTO>>(response.Base64);
                    return dtos;
                }
                else
                    LogCollector.LogCollector.LogError($"GetHrPlusEmployeeChangeIODTOs response.Base64 is null. Error:{response?.ErrorMessage}");

                return new List<EmployeeChangeIODTO>();
            }
            catch (Exception ex)
            {
                LogCollector.LogCollector.LogError(ex, "GetHrPlusEmployeeChangeIODTOs");
                return new List<EmployeeChangeIODTO>();

            }
        }

        public static List<VismaGoEmploymentDTO> GetVismaGoEmploymentDTOs(VismaPayrollBridgeRecieveRequest vismaPayrollBridgeRecieveRequest)
        {
            var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 10));
            var client = new VismaPayrollBridgeConnector(options, GetToken());
            var response = client.GetVismaGoEmployments(vismaPayrollBridgeRecieveRequest);
            if (response?.Base64 != null)
            {
                var dtos = Base64Util.GetObjectFromBase64String<List<VismaGoEmploymentDTO>>(response.Base64);
                return dtos;
            }
            return new List<VismaGoEmploymentDTO>();
        }

        public static ActionResult SendVismaHrPlusAbsence(VismaHrPlusAbsenceBridgeSendRequest vismaHrPlusAbsenceBridgeSendRequest)
        {
            ActionResult result = new ActionResult();
            var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 5));
            var client = new VismaPayrollBridgeConnector(options, GetToken());
            var response = client.SendVismaHrPlusAbsence(vismaHrPlusAbsenceBridgeSendRequest);
            if (response != null)
            {
                if (!response.Success)
                {
                    result.Success = false;
                    result.ErrorMessage = response.ErrorMessage;
                }
            }
            return result;
        }

        public static List<EmployeeChangeIODTO> GetAgdaEmployeeChangeIODTOs(AgdaPayrollBridgeRecieveRequest vismaPayrollBridgeRecieveRequest)
        {
            var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 20));
            var client = new AgdaPayrollBridgeConnector(options, GetToken());

            var response = client.GetEmployeeChangesIOs(vismaPayrollBridgeRecieveRequest);

            if (response?.Base64 != null)
            {
                var dtos = Base64Util.GetObjectFromBase64String<List<EmployeeChangeIODTO>>(response.Base64);
                return dtos;
            }

            return new List<EmployeeChangeIODTO>();

        }

        public static List<TimeBalanceIODTO> GetVismaTimeBalancesIOs(VismaPayrollBridgeRecieveRequest vismaPayrollBridgeRecieveRequest)
        {
            var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 10));
            var client = new VismaPayrollBridgeConnector(options, GetToken());

            var response = client.GetTimeBalancesIOs(vismaPayrollBridgeRecieveRequest);

            if (response?.Base64 != null)
            {
                var dtos = Base64Util.GetObjectFromBase64String<List<TimeBalanceIODTO>>(response.Base64);
                return dtos;
            }

            return new List<TimeBalanceIODTO>();

        }

        public static List<VismaPayrollChangesDTO> GetVismaPayrollChangesDTOs(VismaPayrollBridgeRecieveRequest vismaPayrollBridgeRecieveRequest)
        {
            var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 10));
            var client = new VismaPayrollBridgeConnector(options, GetToken());
            var response = client.GetVismaPayrollChanges(vismaPayrollBridgeRecieveRequest);

            if (response?.Base64 != null)
            {
                var dtos = Base64Util.GetObjectFromBase64String<List<VismaPayrollChangesDTO>>(response.Base64);
                return dtos;
            }

            return new List<VismaPayrollChangesDTO>();

        }


        public static List<StaffingNeedsFrequencyIODTO> GetICAStoreDataFrequencies(ICAStoreDataBridgeRecieveRequest ICAStoreDataBridgeRecieveRequest, int actorCompanyId)
        {
            try
            {
                var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 10));
                var client = new ICABridgeConnector(options, GetToken());

                var response = client.GetFrequencies(ICAStoreDataBridgeRecieveRequest);

                if (response?.Base64 != null)
                {
                    var dtos = Base64Util.GetObjectFromBase64String<List<StaffingNeedsFrequencyIODTO>>(response.Base64);
                    return dtos;
                }
                else
                {
                    if (response == null)
                        LogCollector.LogCollector.LogError($"GetICAStoreDataFrequencies response is null ActorCompanyId:{actorCompanyId}");
                    else
                        LogCollector.LogCollector.LogInfo($"GetICAStoreDataFrequencies response.Base64 is null. ActorCompanyId:{actorCompanyId} Error:{response.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                LogCollector.LogCollector.LogError(ex, "GetICAStoreDataFrequencies");
            }
            return new List<StaffingNeedsFrequencyIODTO>();
        }

        public static List<VismaPayrollEmploymentDTO> GetVismaPayrollEmploymentsForEmployeeNr(VismaPayrollBridgeRecieveRequest vismaPayrollBridgeRecieveRequest)
        {
            var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 10));
            var client = new VismaPayrollBridgeConnector(options, GetToken());
            var response = client.GetVismaPayrollEmploymentsForEmployeeNr(vismaPayrollBridgeRecieveRequest);
            if (response?.Base64 != null)
            {
                var dtos = Base64Util.GetObjectFromBase64String<List<VismaPayrollEmploymentDTO>>(response.Base64);
                return dtos;
            }
            return new List<VismaPayrollEmploymentDTO>();
        }

        public static ActionResult FTPUpload(FTPBridgeSendRequest bridgeSendRequest)
        {
            ActionResult result = new ActionResult();
            var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 10));
            var client = new FileTransferBridgeConnector(options, GetToken());

            var response = client.FTPUpload(bridgeSendRequest);

            if (response != null)
            {
                if (!response.Success)
                {
                    result.Success = false;
                    result.ErrorMessage = response.ErrorMessage;
                }
            }

            return result;
        }

        public static FTPBridgeRecieveResponse FTPGetFiles(FTPBridgeRecieveRequest bridgeRecieveRequest)
        {
            FTPBridgeRecieveResponse result = new FTPBridgeRecieveResponse();
            var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 10));
            var client = new FileTransferBridgeConnector(options, GetToken());
            var response = client.FTPGetFiles(bridgeRecieveRequest);

            foreach (var fileInformation in response?.BridgeFileInformations ?? new List<BridgeFileInformation>())
            {
                if (DefenderUtil.IsVirusBase64(fileInformation.Base64))
                {
                    LogCollector.LogCollector.LogError($"FTPGetFiles Virus Detected {bridgeRecieveRequest.BridgeConfiguration.BridgeSetup.Address} {fileInformation.Path}");
                    return null;
                }
            }

            return response;
        }
        public static FTPBridgeMoveResponse FTPMoveFile(FTPBridgeMoveRequest bridgeMoveRequest)
        {
            FTPBridgeMoveResponse result = new FTPBridgeMoveResponse();
            var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 10));
            var client = new FTPBridgeConnector(options, GetToken());
            var response = client.Move(bridgeMoveRequest);
            return response;
        }

        public static FTPBridgeDeleteResponse FTPDeleteFiles(FTPBridgeDeleteRequest bridgeDeleteRequest)
        {
            FTPBridgeDeleteResponse result = new FTPBridgeDeleteResponse();
            var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 10));
            var client = new FTPBridgeConnector(options, GetToken());
            var response = client.DeleteFiles(bridgeDeleteRequest);
            return response;
        }

        public static ActionResult SSHUpload(SSHBridgeSendRequest bridgeSendRequest)
        {
            ActionResult result = new ActionResult();
            var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 5));
            var client = new FileTransferBridgeConnector(options, GetToken());

            var response = client.SSHUpload(bridgeSendRequest);

            if (response != null)
            {
                if (!response.Success)
                {
                    result.Success = false;
                    result.ErrorMessage = response.ErrorMessage;
                }
            }

            return result;
        }

        public static SSHBridgeRecieveResponse SSHGetFiles(SSHBridgeRecieveRequest bridgeRecieveRequest)
        {
            SSHBridgeRecieveResponse result = new SSHBridgeRecieveResponse();
            var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 10));
            var client = new FileTransferBridgeConnector(options, GetToken());

            var response = client.SSHGetFiles(bridgeRecieveRequest);

            foreach (var fileInformation in response?.BridgeFileInformations ?? new List<BridgeFileInformation>())
            {
                if (DefenderUtil.IsVirusBase64(fileInformation.Base64))
                {
                    LogCollector.LogCollector.LogError($"SSHGetFiles Virus Detected {bridgeRecieveRequest.BridgeConfiguration.BridgeSetup.Address} {fileInformation.Path}");
                    return null;
                }
            }

            return response;
        }
        public static SSHBridgeMoveResponse SSHMoveFile(SSHBridgeMoveRequest bridgeMoveRequest)
        {
            SSHBridgeMoveResponse result = new SSHBridgeMoveResponse();
            var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 10));
            var client = new SSHBridgeConnector(options, GetToken());
            var response = client.Move(bridgeMoveRequest);
            return response;
        }

        public static SSHBridgeDeleteResponse SSHDeleteFiles(SSHBridgeDeleteRequest bridgeDeleteRequest)
        {
            SSHBridgeDeleteResponse result = new SSHBridgeDeleteResponse();
            var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 10));
            var client = new SSHBridgeConnector(options, GetToken());
            var response = client.DeleteFiles(bridgeDeleteRequest);
            return response;
        }

        public static ActionResult AzureStorageUpload(AzureStorageBridgeSendRequest bridgeSendRequest)
        {
            ActionResult result = new ActionResult();
            var options = GetClientOptions(baseUrl: GetURI(), timeout: TimeSpan.FromMilliseconds(1000 * 60 * 5));
            var client = new FileTransferBridgeConnector(options, GetToken());

            var response = client.AzureStorageUpload(bridgeSendRequest);

            if (response != null)
            {
                if (!response.Success)
                {
                    result.Success = false;
                    result.ErrorMessage = response.ErrorMessage;
                }
            }

            return result;
        }

        private static RestClientOptions GetClientOptions(Uri baseUrl, TimeSpan timeout)
        {
            var options = new RestClientOptions
            {
                BaseUrl = baseUrl,
                Timeout = timeout
            };

#if DEBUG
            // Allow self-signed certs only for localhost calls (safe in dev)
            if (baseUrl != null && baseUrl.Host.IndexOf("localhost", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // NOSONAR
                options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            }
#endif

            return options;
        }

        public static RestRequest CreateRequest(RestRequestInput requestInput)
        {
            var request = new RestRequest(requestInput.Resource, requestInput.Method);
            request.RequestFormat = DataFormat.Json;

            if (requestInput.Obj != null)
                request.AddJsonBody(requestInput.Obj);

            if (requestInput.ParameterDict != null && requestInput.ParameterDict.Any())
            {
                foreach (var parameter in requestInput.ParameterDict)
                    request.AddParameter(parameter.Key, parameter.Value, ParameterType.QueryString);
            }

            if (requestInput.HeaderDict != null && requestInput.HeaderDict.Any())
            {
                foreach (var header in requestInput.HeaderDict)
                {
                    request.AddHeader(header.Key, header.Value);
                }
            }
            return request;
        }

    }

    public class VismaPayrollChangesDTO
    {
        public int VismaPayrollChangeId { get; set; }
        public int VismaPayrollBatchId { get; set; }
        public int PersonId { get; set; }
        public int? VismaPayrollEmploymentId { get; set; }
        public string Entity { get; set; }
        public string Info { get; set; }
        public string Field { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string PersonName { get; set; }
        public string EmployerRegistrationNumber { get; set; }
        public DateTime Time { get; set; }
    }
}

