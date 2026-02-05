using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Serializers;
using RestSharp.Serializers.NewtonsoftJson;
using Soe.CrGen.Common.DTO;
using SoftOne.Soe.Business.Core.Status;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using static Soe.CrGen.Common.Enumerations;

namespace SoftOne.Soe.Business.Core.CrGen.Deprecated
{
    public class CrGenConnector
    {
        private readonly ParameterObject parameterObject;

        public CrGenConnector(ParameterObject parameterObject)
        {
            this.parameterObject = parameterObject;
        }

        public CrGenResultDTO GenerateReport(TermGroup_ReportExportType reportExportType, byte[] reportTemplate, XDocument doc, DataSet dataSet, List<CrGenRequestPicturesDTO> CrGenRequestPicturesDTOs, CultureInfo culture, string schema, string errorInfo)
        {
            CrGenResultDTO crGenResultDTO = new CrGenResultDTO();

            #region Create Xdocument from dataset with schema

            if (doc == null && dataSet != null)
            {
                try
                {
                    using (var stream = new MemoryStream())
                    {
                        dataSet.WriteXml(stream, XmlWriteMode.WriteSchema);
                        stream.Position = 0;

                        XmlReaderSettings settings = new XmlReaderSettings();
                        settings.ConformanceLevel = ConformanceLevel.Fragment;

                        XmlReader reader = XmlReader.Create(stream, settings);
                        reader.MoveToContent();
                        if (reader.IsEmptyElement)
                        {
                            reader.Read();
                            return new CrGenResultDTO();
                        }

                        doc = XDocument.Load(reader);
                    }
                }
                catch (Exception ex)
                {
                    SysServiceManager ssm = new SysServiceManager(parameterObject);
                    ssm.LogError($"CrGenResultDTO GenerateReport: DataSet.WriteXml(stream, XmlWriteMode.WriteSchema): {ex.ToString()}");

                    try
                    {
                        string tempPath = ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL + Guid.NewGuid() + @"\" + Guid.NewGuid();
                        dataSet.WriteXml(tempPath);
                        doc = XDocument.Load(tempPath);
                        File.Delete(tempPath);
                    }
                    catch (Exception ex2)
                    {
                        crGenResultDTO.ErrorMessage = $"CrGenResultDTO GenerateReport: DataSet.WriteXml(tempPath): {ex2.ToString()}";
                        ssm.LogError(crGenResultDTO.ErrorMessage);
                        crGenResultDTO.Success = false;
                        crGenResultDTO.ErrorMessage = $"CrGenResultDTO GenerateReport: {ex2.ToString()}";
                        return crGenResultDTO;
                    }
                }
            }

            #endregion

            try
            {
                CrGenRequestDTO crGenRequestDTO = new CrGenRequestDTO()
                {
                    ReportExportType = (CrReportExportType)reportExportType,
                    Guid = Guid.NewGuid(),
                    ReportTemplate = Convert.ToBase64String(reportTemplate),
                    XDocument = doc,
                    DataSet = new DataSet(),
                    ZippedDataSet = string.Empty,
                    Culture = culture,
                    crGenRequestPicturesDTOs = CrGenRequestPicturesDTOs,
                    Schema = schema
                };

                TryCompressXDocument(crGenRequestDTO);

                var uri = GetUri(new List<Uri>());
                RestClientOptions options = new RestClientOptions(uri);
                SetTimeOut(options);
                var client = new GoRestClient(options, configureSerialization: s => s.UseNewtonsoftJson());
                CheckIfCachedTemplate(client, crGenRequestDTO);


                crGenResultDTO = ExecuteRequest(client, crGenRequestDTO, uri);
                if (crGenResultDTO == null)
                {
                    SysServiceManager ssm = new SysServiceManager(null);
                    ssm.LogError("crGenResultDTO is null " + errorInfo);
                }
                else if (!crGenResultDTO.Success)
                {
                    SysServiceManager ssm = new SysServiceManager(null);
                    ssm.LogError($"!crGenResultDTO.Success: {crGenResultDTO.ErrorMessage}  " + errorInfo);
                }
            }
            catch (Exception ex)
            {
                SysServiceManager ssm = new SysServiceManager(null);
                ssm.LogError($"CrGenResultDTO GenerateReport: {ex.ToString()} " + errorInfo);
                if (crGenResultDTO != null)
                {
                    crGenResultDTO.Success = false;
                    crGenResultDTO.ErrorMessage = $"CrGenResultDTO GenerateReport: {ex.ToString()} " + errorInfo;
                }
            }

            return crGenResultDTO;
        }

        private static void TryCompressXDocument(CrGenRequestDTO crGenRequestDTO)
        {
            var compressedData = CompressionUtil.CompressXDocument(crGenRequestDTO.XDocument);

            if (compressedData != null)
            {
                crGenRequestDTO.ZippedXDocument = Convert.ToBase64String(compressedData);
                crGenRequestDTO.XDocument = null;
            }

        }

        private static void CheckIfCachedTemplate(RestClient client, CrGenRequestDTO crGenRequestDTO)
        {
            var checkSum = crGenRequestDTO.CalculateChecksum(crGenRequestDTO);
            RestResponse checkSumResponse = client.Execute(CreateRequest("crGenRequest/CrGenReportTemplate/" + checkSum, Method.Get));

            if (JsonConvert.DeserializeObject<bool>(checkSumResponse.Content))
            {
                crGenRequestDTO.ReportTemplate = string.Empty;
                crGenRequestDTO.ReportTemplateCheckSum = checkSum;
            }
        }

        private static void SetTimeOut(RestClientOptions options)
        {
            int timeout = 1000 * 3600; // One hour
            options.Timeout = TimeSpan.FromMilliseconds(timeout);
        }

        private static CrGenResultDTO ExecuteRequest(RestClient client, CrGenRequestDTO crGenRequestDTO, Uri uri)
        {
            RestResponse response = client.Execute(CreateRequest("CrGenRequest", Method.Post, crGenRequestDTO));

            if (string.IsNullOrEmpty(response.Content))
            {
                List<Uri> exclude = new List<Uri>() { uri };
                LogCollector.LogError($"ExecuteRequest Crgen on {uri} failed on first attempt. Error: " + response.ErrorMessage);
                var uri2 = GetUri(exclude);
                RestClientOptions options = new RestClientOptions(uri2);
                SetTimeOut(options);
                client = new GoRestClient(options, configureSerialization: s => s.UseNewtonsoftJson());
                response = client.Execute(CreateRequest("CrGenRequest", Method.Post, crGenRequestDTO));

                if (string.IsNullOrEmpty(response.Content))
                {
                    exclude.Add(uri2);
                    LogCollector.LogError($"ExecuteRequest Crgen on {uri2} failed on second attempt. Error: " + response.ErrorMessage);
                    var uri3 = GetUri(exclude);
                    options = new RestClientOptions(uri2);
                    SetTimeOut(options);
                    client = new GoRestClient(options, configureSerialization: s => s.UseNewtonsoftJson());
                    response = client.Execute(CreateRequest("CrGenRequest", Method.Post, crGenRequestDTO));

                    if (string.IsNullOrEmpty(response.Content))
                        LogCollector.LogError($"ExecuteRequest Crgen on {uri3} failed on third attempt. Error: " + response.ErrorMessage);
                }
            }

            return JsonConvert.DeserializeObject<CrGenResultDTO>(response.Content);
        }

        private static string GetJsonFromDataSet(DataSet dataSet)
        {
            var json = JsonConvert.SerializeObject(dataSet, Newtonsoft.Json.Formatting.Indented);

            if (json.Contains(ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL))
            {
                var data = (JObject)JsonConvert.DeserializeObject(json);

                string logopath = data["Atlantic/Canary"].Value<string>();
            }

            return json;
        }

        private static RestRequest CreateRequest(string resource, Method method, object obj = null)
        {
            var request = new RestRequest(resource, method);
            request.RequestFormat = DataFormat.Json;

            try
            {
                if (obj != null)
                {
                    request.AddJsonBody(obj);
                    //request.AddObject(obj);
                    //request.AddJsonBody(JsonConvert.SerializeObject(obj));
                }
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
            }

            request.AddHeader("Accept-Encoding", "gzip");
            return request;

        }

        private static Uri GetUri(List<Uri> exclude)
        {
            List<Uri> uris = new List<Uri>();

            uris.Add(new Uri("https://s4crgen.softone.se/")); //S4crgen.softone.se   Prio 0 
            uris.Add(new Uri("https://s3crgen.softone.se/"));// Prio 1
            uris.Add(new Uri("https://testcrgen.softone.se/"));// Prio 2
            uris.Add(new Uri("http://192.168.50.96/")); //SoftOnes19   Prio 3
            uris = uris.Where(w => !exclude.Contains(w)).ToList();

            return GetShortestQueue(uris, 0);
        }

        private static Uri GetShortestQueue(List<Uri> uris, int nrOfAttempts)
        {
            Dictionary<string, int> queues = new Dictionary<string, int>();

            foreach (var uri in uris)
            {
                string content = string.Empty;
                try
                {
                    var options = new RestClientOptions() { BaseUrl = uri, Timeout =TimeSpan.FromMilliseconds(250) };
                    var client = new GoRestClient(options, configureSerialization: s => s.UseNewtonsoftJson());
                    RestResponse response = client.Execute(CreateRequest("crGenRequest/Queue", Method.Get, null));
                    content = response.Content;

                    if (!int.TryParse(content, out _))
                    {
                        Thread.Sleep(100);
                        response = client.Execute(CreateRequest("crGenRequest/Queue", Method.Get, null));
                        content = response.Content;
                    }

                    if (int.TryParse(content, out int value))
                    {
                        if (value <= 1 && value >= 0 && IsCrGenServerAlive(uri.ToString()))
                            return uri;

                        queues.Add(uri.ToString(), value);
                    }
                }
                catch (Exception ex)
                {
                    LogCollector.LogError(ex, uri.ToString() + " " + content);

                    if (!queues.ContainsKey(uri.ToString()))
                        queues.Add(uri.ToString(), 99);
                }
            }

            if (!queues.Any())
            {
                if (nrOfAttempts < 3)
                {
                    nrOfAttempts++;
                    Thread.Sleep(1000 * nrOfAttempts);
                    return GetShortestQueue(uris, nrOfAttempts);
                }
                else
                {
                    return uris.FirstOrDefault();
                }
            }

            if (queues.All(q => q.Value == queues.First().Value))
                return uris.First();

            return new Uri(queues.OrderBy(q => q.Value).FirstOrDefault().Key);
        }

        private static bool IsCrGenServerAlive(string url)
        {
            string key = "IsCrGenServerAlive#" + url;
            bool? value = BusinessMemoryCache<bool?>.Get(key);
            if (value.HasValue && value.Value)
                return value.Value;

            value = SoftOneStatusConnector.IsCrGenServerAlive(url);
            if (value != null)
                BusinessMemoryCache<bool?>.Set(key, value, 120);

            return value == true;
        }
    } 
}
