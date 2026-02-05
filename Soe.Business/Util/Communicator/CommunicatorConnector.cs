using Newtonsoft.Json;
using RestSharp;
using SoftOne.Communicator.Shared.Client;
using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ZXing;

namespace SoftOne.Soe.Business.Util.Communicator
{
    public class CommunicatorConnector
    {
        private static CommunicatorApiClient communicatorApiClient;
        private static CommunicatorApiClient apiClient
        {
            get
            {
                if (communicatorApiClient == null)
                {
                    communicatorApiClient = new CommunicatorApiClient(GetURI().ToString());
                }
                return communicatorApiClient;
            }
        }
        private static bool? isTest { get; set; }

        private static bool IsTest
        {
            get
            {
                if (!isTest.HasValue)
                {
                    SettingManager sm = new SettingManager(null);
                    string address = string.Empty;
                    isTest = sm.isTest();
                }
                return isTest.Value;
            }
        }


        public static Uri GetURI()
        {
#if DEBUG
            //return new Uri("http://localhost:26234/");
#endif
            if (IsTest)
                return new Uri("https://softonecommunicatortest.azurewebsites.net/");
            else
                return new Uri("https://softone-communicator.azurewebsites.net/");
        }

        public static RestRequest CreateRequest(string resource, Method method, object obj = null)
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;
            if (obj != null)
                request.AddJsonBody(obj);

            return request;
        }
        public static ActionResult SendServerAlertMailMessage(string subject, string body)
        {
            List<string> cc = new List<string>();
            string email = "7fde9c16.softone.se@emea.teams.ms";
            MailMessageDTO mailMessageDTO = new MailMessageDTO("xeservice@softone.se", email, new List<string>(), subject, body, false, false);
            return SendMailMessage(mailMessageDTO);
        }

        public static ActionResult SendMailMessageFireAndForget(MailMessageDTO mailMessageDTO)
        {
            Task.Run(() => SendMailMessage(mailMessageDTO));
            Thread.Sleep(10);
            return new ActionResult();
        }

        public static ActionResult SendMailMessage(MailMessageDTO mailMessageDTO)
        {
            return SendCommunicatorMessage(mailMessageDTO.ToCommunicatorMessage());
        }

        public static ActionResult SendCommunicatorMessageBatch(CommunicatorMessageBatch communicatorMessageBatch)
        {
            string content = string.Empty;
            string resp = string.Empty;
            var currentProtocol = ServicePointManager.SecurityProtocol;

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var client = new GoRestClient(GetURI());
                var response = client.Execute(CreateRequest("MailMessage/CommunicatorMessageBatch", Method.Post, communicatorMessageBatch));

                return new ActionResult();
            }
            catch (Exception ex)
            {
                SysLogConnector.SaveErrorMessage(ex.ToString() + " SendCommunicatorMessage " + communicatorMessageBatch.CommunicatorMessages.FirstOrDefault()?.Subject);
                return new ActionResult(ex, "Send email failed");
            }
            finally
            {
                ServicePointManager.SecurityProtocol = currentProtocol;
            }
        }

        private static void UpdateDistributionStatus(Dictionary<string, string> customArgs, ActionResult mailResult)
        {
            var distManager = new InvoiceDistributionManager(null);

            try
            {
                foreach (var item in customArgs)
                {
                    if (item.Key == "SOEInvoiceDistribution")
                    {
                        if (int.TryParse(item.Value.Split('#')[1], out int invoiceDistributionId))
                        {
                            distManager.UpdateEmailStatus(invoiceDistributionId, TermGroup_ReportPrintoutStatus.SentFailed, mailResult.ErrorMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SysLogConnector.SaveErrorMessage(ex.ToString() + "Communicator => UpdateInvoiceDistribution " + ex.Message);
            }
        }

        public static ActionResult SendCommunicatorMessage(CommunicatorMessage message)
        {
            string content = string.Empty;
            var currentProtocol = ServicePointManager.SecurityProtocol;

            try
            {
                ActionResult result;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                var client = new GoRestClient(GetURI());
                var response = client.Execute(CreateRequest("MailMessage/CommunicatorMessage", Method.Post, message));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    result = JsonConvert.DeserializeObject<ActionResult>(response.Content) ?? new ActionResult("Send email failed");
                }
                else
                {
                    result = new ActionResult("Send email failed with:" + response.StatusDescription + " : " + response.Content);
                }

                if (!result.Success)
                {
                    if (!message.CustomArgs.IsNullOrEmpty())
                    {
                        UpdateDistributionStatus(message.CustomArgs, result);
                    }

                    SysLogConnector.SaveErrorMessage("SendCommunicatorMessage " + message.Subject + " " + response.Content);
                }

                return result;
            }
            catch (Exception ex)
            {
                SysLogConnector.SaveErrorMessage(ex.ToString() + " SendCommunicatorMessage " + message.Subject);
                return new ActionResult(ex, "Send email failed");
            }
            finally
            {
                ServicePointManager.SecurityProtocol = currentProtocol;
            }
        }

        public static ActionResult SendPushNotification(string userKey, string subject, string body, List<CommunicatorMetaData> communicatorMetaData = null)
        {
            CommunicatorMessage communicatorMessage = new CommunicatorMessage()
            {
                Subject = subject,
                Body = body,
                Recievers = new List<CommunicatorPerson>() { new CommunicatorPerson() { KeySecondary = userKey } },
                CommunicatorCredentials = new CommunicatorCredentials() { KeyPrimary = "", TypeKey = 2 },
                CommunicatorProvider = CommunicatorProvider.AzureNotificationHub,
                CommunicationType = CommunicationType.AzureNotificationHub,
                CommunicatorMetaData = communicatorMetaData != null ? communicatorMetaData : new List<CommunicatorMetaData>()
            };

            return SendCommunicatorMessage(communicatorMessage);
        }

        public static ActionResult RegisterDevice(RegisterDevice registerDevice)
        {
            var result = Task.Run(() => apiClient.RegisterDeviceAsync(registerDevice)).Result;

            if (result == null) {
                return new ActionResult()
                {
                    Success = false,
                    ErrorMessage = "ErrorMessage response is null",
                };
            }

            return new ActionResult() {
                Success = result.Success,
                ErrorMessage = result.ErrorMessage,
            };
        }

        public static ActionResult DeleteDevice(string installationId)
        {
            var result = Task.Run(() => apiClient.DeleteDeviceAsync(installationId)).Result;

            if (result == null)
            {
                return new ActionResult()
                {
                    Success = false,
                    ErrorMessage = "ErrorMessage response is null",
                };
            }

            return new ActionResult()
            {
                Success = result.Success,
                ErrorMessage = result.ErrorMessage,
            };
        }

        public static ActionResult SendNotification(CommunicatorMessage message)
        {
            var result = Task.Run(() => apiClient.SendNotificationAsync(message)).Result;

            if (result == null)
            {
                return new ActionResult()
                {
                    Success = false,
                    ErrorMessage = "ErrorMessage response is null",
                };
            }

            return new ActionResult()
            {
                Success = result.Success,
                ErrorMessage = result.ErrorMessage,
            };
        }

        public static ActionResult SendSMSMessage(MailMessageDTO mailMessageDTO)
        {
            try
            {
                var client = new GoRestClient(GetURI());
                var esponse = client.Execute(CreateRequest("MailMessage/SmsMessage", Method.Post, mailMessageDTO));
                return new ActionResult();
            }
            catch (Exception ex)
            {
                SysLogConnector.SaveErrorMessage(ex.ToString() + " SendSMSMessage " + mailMessageDTO.subject);
                return new ActionResult(ex, "Send email failed");
            }
        }

        public static List<InboundEmailFilteredRowDTO> GetInboundEmailFilteredRows(InboundEmailFilterDTO filter)
        {
            try
            {
                var client = new GoRestClient(GetURI());
                var response = client.Execute(CreateRequest("Manage/InboundEmails", Method.Post, filter));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<List<InboundEmailFilteredRowDTO>>(response.Content) ?? new List<InboundEmailFilteredRowDTO>();
                }
                else
                {
                    SysLogConnector.SaveErrorMessage("GetInboundEmailFilteredRows failed with: " + response.StatusDescription + " : " + response.Content);
                    return new List<InboundEmailFilteredRowDTO>();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static InboundEmailDTO GetInboundEmail(int id)
        {
            try
            {
                var client = new GoRestClient(GetURI());
                var response = client.Execute(CreateRequest("Manage/InboundEmails/" + id, Method.Get));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<InboundEmailDTO>(response.Content) ?? new InboundEmailDTO();
                }
                else
                {
                    SysLogConnector.SaveErrorMessage("GetInboundEmail failed with: " + response.StatusDescription + " : " + response.Content);
                    return new InboundEmailDTO();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string GetInboundEmailAttachment(int attachmentId)
        {
            try
            {
                var client = new GoRestClient(GetURI());
                var response = client.Execute(CreateRequest("Manage/InboundEmails/Attachment/" + attachmentId, Method.Get));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    return JsonConvert.DeserializeObject<string>(response.Content);
                }
                else
                {
                    SysLogConnector.SaveErrorMessage("GetGetInboundEmailAttachment failed with: " + response.StatusDescription + " : " + response.Content);
                    return null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
