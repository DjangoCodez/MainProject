using SO.Internal.Shared.Api.Blob.DataStorage;
using SO.Internal.Shared.Api.Blob.DataStorages;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Business.Util.WebApiInternal;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace SoftOne.Soe.Business.Core.BlobStorage
{
    public static class EvoDataStorageConnector
    {
        private static string url
        {
            get
            {
                var uri = ConfigurationSetupUtil.GetEvoUrl().RemoveTrailingSlash();
                // uri = new Uri("https://localhost:7257/");
                return uri.ToString();
            }
        }

        public static string token
        {
            get { return ConnectorBase.GetAccessToken(); }
        }

        public static DataStorageUpsertResult UpsertDataStorage(byte[] file, string xml, int dataStorageId, int dbId, int actorCompanyId, string key)
        {
            try
            {
                if (file != null && file.Length == 0)
                    file = null;

                if (xml != null && xml.Length == 0)
                    xml = null;

                DataStorageUpsertRequest request = DataStorageUpsertRequest.Create(dataStorageId, dbId, key, actorCompanyId, file, xml.EmptyToNull());
                return UpsertDataStorage(request);
            }
            catch (Exception ex)
            {
                LogCollector.LogError(ex);
                return new DataStorageUpsertResult(false, ex.ToString());
            }
        }

        public static DataStorageUpsertResult UpsertDataStorage(DataStorageUpsertRequest DataStorageUpsertRequest)
        {
            return Task.Run(() => DataStorageClient.UpsertDataStorageAsync(url, token, DataStorageUpsertRequest)).GetAwaiter().GetResult();
        }

        public static byte[] GetFile(int dataStorageId, int dbId, int actorCompanyId, string key)
        {
            var DataStorage = GetDataStorage(dataStorageId, dbId, actorCompanyId, key);
            return DataStorage?.GetData();
        }

        public static DataStorageResult GetDataStorage(int dataStorageId, int dbId, int actorCompanyId, string key)
        {
            try
            {
                var response = Task.Run(() => DataStorageClient.GetDataStorageAsync(url, token, CreateGetRequest(dataStorageId, dbId, actorCompanyId, key))).GetAwaiter().GetResult();

                var result = response?.DataStorageResults?.FirstOrDefault();
                var message = $"DataStorageId: {dataStorageId}, DbId: {dbId}, Key: {key}, ActorCompanyId: {actorCompanyId} url {url}";

                if (response != null)
                    message += response.Message;

                if (result == null)
                    LogCollector.LogInfo("GetDataStorage Result is null " + message);

                return result;
            }
            catch (Exception ex)
            {
                LogCollector.LogError(ex);
                return null;
            }
        }

        public static DataStorageGetRequest CreateGetRequest(int dataStorageId, int dbId, int actorCompanyId, string key)
        {
            return new DataStorageGetRequest()
            {
                DataStorageId = dataStorageId,
                DbId = dbId,
                Key = key,
                ActorCompanyId = actorCompanyId
            };
        }

        public static List<DataStorageResult> GetDataStorages(List<DataStorageGetRequest> getRequests)
        {
            DataStoragesGetRequest request = new DataStoragesGetRequest() { Requests = getRequests };
            var response = Task.Run(() =>  DataStorageClient.GetDataStoragesAsync(url, token, request)).GetAwaiter().GetResult();
            return response?.DataStorageResults ?? new List<DataStorageResult>();
        }
    }
}
