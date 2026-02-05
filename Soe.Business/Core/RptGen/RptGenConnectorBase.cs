using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers;
using RestSharp.Serializers.NewtonsoftJson;
using SoftOne.Soe.Business.Core.CrGen;
using SoftOne.Soe.Business.Core.Status;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.RptGen
{
    public enum RptErrorType
    {
        Unknown,
        NoMatchingCache
    }

    public class RptGenRequestPicturesDTO
    {
        public string Path { get; set; }
        public byte[] Data { get; set; }
    }

    public abstract class RptGenConnector
    {
        protected readonly ParameterObject parameterObject;
        protected RptGenConnector(ParameterObject parameterObject)
        {
            this.parameterObject = parameterObject;
        }

        public static RptGenConnector GetConnector(ParameterObject parameterObject, SoeReportType reportType)
        {
            switch (reportType)
            {
                case SoeReportType.CrystalReport:
                    return new CrGenConnector(parameterObject);
                case SoeReportType.DevExpressReport:
                    return new DevExpressGenConnector(parameterObject);
            }
            throw new Exception("Invalid report Type provided.");
        }

        public abstract RptGenResultDTO GenerateReport(TermGroup_ReportExportType reportExportType, byte[] reportTemplate, XDocument doc, DataSet dataSet, List<RptGenRequestPicturesDTO> RptGenRequestPicturesDTOs, CultureInfo culture, string schema, string errorInfo);

        protected abstract RptGenResultDTO ExecuteRequest(RestClient restClient, RestRequest request, Uri defaultBaseUri);

        protected XDocument GetXDocumentFromDataSet(DataSet dataSet)
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
                    return null;
                }

                return XDocument.Load(reader);
            }
        }

        protected string GetCompressXDocument(XDocument xDocument)
        {
            var compressedData = CompressionUtil.CompressXDocument(xDocument);
            return compressedData != null ? Convert.ToBase64String(compressedData) : null;
        }

        protected bool IsCachedTemplateAvailabe(RestClient client, string resourceUrl, string checksum)
        {
            bool result = false;
            var resource = resourceUrl + checksum;
            var restResponse = client.Execute(GetRestRequest(resource, Method.Get));
            if (restResponse.StatusCode == System.Net.HttpStatusCode.OK)
                result = JsonConvert.DeserializeObject<bool>(restResponse.Content);

            return result;
        }

        protected bool IsRptGenServerAlive(string url, string cacheKey)
        {
            string key = $"{cacheKey}#{url}";
            bool? value = BusinessMemoryCache<bool?>.Get(key);
            if (value.HasValue && value.Value)
                return value.Value;

            value = SoftOneStatusConnector.IsCrGenServerAlive(url);
            if (value != null)
                BusinessMemoryCache<bool?>.Set(key, value, 120);

            return value == true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="maxTimeOut">Default Timeout is set to 1 hour</param>
        /// <returns>RestClient</returns>
        protected RestClient GetRestClient(Uri uri, int maxTimeOut = 3_600_000)
        {
            #region Setting up RestClientOptions
            RestClientOptions options = new RestClientOptions(uri);
            options.Timeout = TimeSpan.FromMilliseconds(maxTimeOut);
            #endregion

            var client = new GoRestClient(options, configureSerialization: s => s.UseNewtonsoftJson());

            return client;
        }

        protected RestRequest GetRestRequest(string resource, Method method, object obj = null)
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

            request.AddHeader("Accept-Encoding", "gzip");
            return request;
        }

        protected Uri GetShortestQueue(List<Uri> uris, int nrOfAttempts, string reportQueueApiRoute, string serverAliveApiRoute)
        {
            Dictionary<string, int> queues = new Dictionary<string, int>();

            foreach (var uri in uris)
            {
                string content = string.Empty;
                try
                {
                    var client = GetRestClient(uri, 250);
                    RestResponse response = client.Execute(GetRestRequest(reportQueueApiRoute, Method.Get, null));
                    content = response.Content;

                    if (!int.TryParse(content, out _))
                    {
                        Thread.Sleep(100);
                        response = client.Execute(GetRestRequest(reportQueueApiRoute, Method.Get, null));
                        content = response.Content;
                    }

                    if (!string.IsNullOrEmpty(serverAliveApiRoute) && !string.IsNullOrWhiteSpace(serverAliveApiRoute) && int.TryParse(content, out int value))
                    {
                        if (value == 1 && IsRptGenServerAlive(uri.ToString(), serverAliveApiRoute))
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
                    return GetShortestQueue(uris, nrOfAttempts, reportQueueApiRoute, serverAliveApiRoute);
                }
                else
                {
                    return uris.FirstOrDefault();
                }
            }

            return new Uri(queues.OrderBy(q => q.Value).FirstOrDefault().Key);
        }
    }
}
