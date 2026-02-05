using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using RestSharp;
using SoftOne.Common.KeyVault;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO.SoftOneLogger;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.Logger
{
    public static class LoggerConnector
    {
        public static void Init()
        {
            connection = connection == null ? GetConnectionString() : null;
            queueName = queueName == null ? GetQueueName() : null;
            apiAddress = apiAddress == null ? GetApiAddress() : null;

            if (serviceBusClient == null)
            {
                serviceBusClient = new ServiceBusClient(connection, new ServiceBusClientOptions
                {
                    TransportType = ServiceBusTransportType.AmqpWebSockets
                });
                serviceBusSender = serviceBusClient.CreateSender(queueName);
            }
        }

        private static bool IsInitialized()
        {
            return !string.IsNullOrEmpty(connection) && !string.IsNullOrEmpty(queueName) && !string.IsNullOrEmpty(apiAddress);
        }

        private static string connection { get; set; }
        private static string queueName { get; set; }
        private static string apiAddress { get; set; }

        private static ServiceBusClient serviceBusClient;
        private static ServiceBusSender serviceBusSender;

        private static string GetConnectionString()
        {
            var key = "ServiceBusLoggerConnectionString";
            var backupKey = "Backup" + key;
            string cachedValue = MemoryCache.Default.Get(key) as string;

            if (cachedValue == null)
            {
                cachedValue = KeyVaultSecretsFetcher.GetSecret(key);

                if (!string.IsNullOrEmpty(cachedValue))
                    MemoryCache.Default.Set(key, cachedValue, DateTimeOffset.Now.AddHours(1));
                else
                {
                    cachedValue = MemoryCache.Default.Get(backupKey) as string;
                    MemoryCache.Default.Set(key, cachedValue, DateTimeOffset.Now.AddMinutes(2));
                }
            }
            return cachedValue;
        }

        private static string GetQueueName()
        {
            var key = "ServiceBusPersonaldatalogQueue";
            var backupKey = "Backup" + key;
            string cachedValue = MemoryCache.Default.Get(key) as string;
            if (cachedValue == null)
            {
                cachedValue = KeyVaultSecretsFetcher.GetSecret(key);
                if (!string.IsNullOrEmpty(cachedValue))
                    MemoryCache.Default.Set(key, cachedValue, DateTimeOffset.Now.AddHours(1));
                else
                {
                    cachedValue = MemoryCache.Default.Get(backupKey) as string;
                    MemoryCache.Default.Set(key, cachedValue, DateTimeOffset.Now.AddMinutes(2));
                }
            }
            return cachedValue;
        }

        private static string GetApiAddress()
        {
            var key = "PersonaldatalogSoftOneLoggerApi";
            var backupKey = "Backup" + key;
            string cachedValue = MemoryCache.Default.Get(key) as string;
            if (cachedValue == null)
            {
                cachedValue = KeyVaultSecretsFetcher.GetSecret("SoftOneLoggerApi");
                if (!string.IsNullOrEmpty(cachedValue))
                    MemoryCache.Default.Set(key, cachedValue, DateTimeOffset.Now.AddHours(1));
                else
                {
                    cachedValue = MemoryCache.Default.Get(backupKey) as string;
                    MemoryCache.Default.Set(key, cachedValue, DateTimeOffset.Now.AddMinutes(2));
                }
            }
            return cachedValue;
        }

        public static RestRequest CreateRequest(string resource, Method method, Dictionary<string, object> parameterDict, object obj = null)
        {
            var request = new RestRequest("Api/PersonalData/" + resource, method);
            request.RequestFormat = DataFormat.Json;

            if (parameterDict != null)
            {
                foreach (var parameter in parameterDict)
                {
                    request.AddParameter(parameter.Key, parameter.Value, ParameterType.QueryString);
                }
            }

            if (obj != null)
                request.AddJsonBody(obj);

            request.AddHeader("Token", "e8d7bf57fd1b44a684689cfce813f783");
            return request;
        }

        public static List<PersonalDataLogBatchDTO> GetPersonalDataLogs(int actorCompanyId, int recordId, int? sysCompDBId, TermGroup_PersonalDataType type, TermGroup_PersonalDataInformationType informationType, TermGroup_PersonalDataActionType actionType, DateTime? fromDate = null, DateTime? toDate = null, int? take = null)
        {
            try
            {
                Init();

                if (!sysCompDBId.HasValue)
                    return new List<PersonalDataLogBatchDTO>();

                var options = new RestClientOptions(apiAddress) { Timeout = TimeSpan.FromMilliseconds(300 * 60 * 10) };
                var client = new GoRestClient(options);
                var request = CreateRequest("PersonalDataLogs", Method.Get, null);
                request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                request.AddParameter("recordId", recordId, ParameterType.QueryString);
                request.AddParameter("sysCompDBId", sysCompDBId.Value, ParameterType.QueryString);
                request.AddParameter("type", (int)type, ParameterType.QueryString);
                request.AddParameter("informationType", (int)informationType, ParameterType.QueryString);
                request.AddParameter("actionType", (int)actionType, ParameterType.QueryString);
                request.AddParameter("fromDate", fromDate.HasValue ? fromDate : null, ParameterType.QueryString);
                request.AddParameter("toDate", toDate.HasValue ? toDate : null, ParameterType.QueryString);
                request.AddParameter("take", take.HasValue ? take.Value : 0, ParameterType.QueryString);
                RestResponse response = client.Execute(request);
                return JsonConvert.DeserializeObject<List<PersonalDataLogBatchDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetPersonalDataLogsForEmployee");
                return new List<PersonalDataLogBatchDTO>();
            }
        }

        public static List<PersonalDataLogBatchDTO> GetPersonalDataLogsCausedByUser(int actorCompanyId, int userId, int? recordId, int? sysCompDBId, TermGroup_PersonalDataType type, TermGroup_PersonalDataInformationType informationType, TermGroup_PersonalDataActionType actionType, DateTime? fromDate = null, DateTime? toDate = null, int? take = null)
        {
            try
            {
                Init();

                if (!sysCompDBId.HasValue)
                    return new List<PersonalDataLogBatchDTO>();

                var options = new RestClientOptions(apiAddress) { Timeout = TimeSpan.FromMilliseconds(300 * 60 * 10) };
                var client = new GoRestClient(options);
                var request = CreateRequest("GetPersonalDataLogsCausedByUser", Method.Get, null);
                request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                request.AddParameter("userId", userId, ParameterType.QueryString);
                if (recordId.HasValue)
                    request.AddParameter("recordId", recordId.Value, ParameterType.QueryString);
                request.AddParameter("sysCompDBId", sysCompDBId.Value, ParameterType.QueryString);
                request.AddParameter("type", (int)type, ParameterType.QueryString);
                request.AddParameter("informationType", (int)informationType, ParameterType.QueryString);
                request.AddParameter("actionType", (int)actionType, ParameterType.QueryString);
                request.AddParameter("fromDate", fromDate.HasValue ? fromDate : null, ParameterType.QueryString);
                request.AddParameter("toDate", toDate.HasValue ? toDate : null, ParameterType.QueryString);
                request.AddParameter("take", take.HasValue ? take.Value : 0, ParameterType.QueryString);
                RestResponse response = client.Execute(request);
                return JsonConvert.DeserializeObject<List<PersonalDataLogBatchDTO>>(response.Content);
            }
            catch (Exception ex)
            {
                LogError(ex, "GetPersonalDataLogsCausedByUser");
                return new List<PersonalDataLogBatchDTO>();
            }
        }

        public static void SavePersonalDataLogServiceBusFireAndForget(PersonalDataLogBatchDTO personalDataBatch)
        {
            Init();
            Task.Run(() => SavePersonalDataLogAsync(personalDataBatch));
        }

        public static async Task SavePersonalDataLog(PersonalDataLogBatchDTO personalDataBatch, bool extensiveLogging = false)
        {
            Init();
            await SavePersonalDataLogAsync(personalDataBatch, extensiveLogging);
        }

        public static async Task SavePersonalDataLogAsync(PersonalDataLogBatchDTO personalDataBatch, bool extensiveLogging = false)
        {
            Init();
            try
            {
                if (serviceBusClient == null)
                {
                    serviceBusClient = new ServiceBusClient(connection);
                    serviceBusSender = serviceBusClient.CreateSender(queueName);
                }

                if (personalDataBatch != null)
                {
                    if (extensiveLogging)
                        LogCollector.LogError("SavePersonalDataLogAsync begin");

                    byte[] data = ZipUtility.CompressString(JsonConvert.SerializeObject(personalDataBatch));

                    if (extensiveLogging)
                        LogCollector.LogError("SavePersonalDataLogAsync data.Length:" + data.Length.ToString());

                    if (data.Length < 250000)
                    {
                        try
                        {
                            if (extensiveLogging)
                                LogCollector.LogError("serviceBusSender begin");

                            ServiceBusMessage message = new ServiceBusMessage(data)
                            {
                                MessageId = personalDataBatch.Batch.ToString()
                            };
                            await serviceBusSender.SendMessageAsync(message);

                            if (extensiveLogging)
                                LogCollector.LogError("serviceBusSender closed");
                        }
                        catch (Exception ex)
                        {
                            if (extensiveLogging)
                                LogCollector.LogError("SavePersonalDataLogAsync failed " + ex.ToString());

                            LogError(ex, "SavePersonalDataLogAsync failed");

                            serviceBusClient = new ServiceBusClient(connection);
                            serviceBusSender = serviceBusClient.CreateSender(queueName);

                            #region retry

                            bool retrySucceded = false;

                            try
                            {
                                await Task.Delay(5000);
                                ServiceBusMessage message = new ServiceBusMessage(data)
                                {
                                    MessageId = personalDataBatch.Batch.ToString()
                                };
                                await serviceBusSender.SendMessageAsync(message);
                                retrySucceded = true;
                            }
                            catch (Exception exretry)
                            {
                                LogError(exretry, "SavePersonalDataLogAsync retry failed");
                            }

                            if (retrySucceded)
                                LogCollector.LogError("SavePersonalDataLogAsync retry succeded");

                            #endregion

                            if (!retrySucceded)
                                SavePersonalDataLogFireAndForget(personalDataBatch);
                        }
                        finally
                        {
                            if (extensiveLogging)
                                LogCollector.LogError("finally begin");
                            // await serviceBusSender.CloseAsync();
                            if (extensiveLogging)
                                LogCollector.LogError("finally closed");
                        }
                    }
                    else
                    {
                        if (extensiveLogging)
                            LogCollector.LogError("SavePersonalDataLogAsync Data > 250000");

                        LogError(new Exception("Data > 250000"), "SavePersonalDataLog");
                        SavePersonalDataLogFireAndForget(personalDataBatch);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "SavePersonalDataLog");
                SavePersonalDataLogFireAndForget(personalDataBatch);
            }
        }

        public static void SavePersonalDataLogFireAndForget(PersonalDataLogBatchDTO personalDataBatch, bool extensiveLogging = false)
        {
            Init();
            Task.Run(() => SavePersonalDataLog(personalDataBatch, 0, extensiveLogging));
        }

        public static void SavePersonalDataLog(PersonalDataLogBatchDTO personalDataBatch, int retry, bool extensiveLogging = false)
        {
            Init();
            ActionResult result = new ActionResult();

            if (extensiveLogging)
                LogCollector.LogError("SavePersonalDataLog retry " + retry.ToString() + " address: " + apiAddress);

            if (retry > 5)
                return;

            Random random = new Random();

            if (retry > 0)
                Thread.Sleep(retry * 2000 + random.Next(1, 300));

            try
            {
                var options = new RestClientOptions(apiAddress) { Timeout = TimeSpan.FromMilliseconds(1000 * 120) };
                var client = new GoRestClient(options);
                var response = client.Execute(CreateRequest("SavePersonalDataLog", Method.Post, null, personalDataBatch));

                result = JsonConvert.DeserializeObject<ActionResult>(response.Content);
                if (result == null)
                    result = new ActionResult(false);
            }
            catch (Exception ex)
            {
                if (extensiveLogging)
                    LogCollector.LogError("SavePersonalDataLog failed " + ex.ToString());

                result.Success = false;
                result.ErrorMessage = ex.ToString();
            }

            if (!result.Success)
                SavePersonalDataLog(personalDataBatch, retry + 1);
        }

        #region Help-methods

        public static void LogError(Exception ex, string message = "")
        {
            SysLogConnector.SaveErrorMessage(message + " " + ex.ToString());
        }

        #endregion
    }
}
