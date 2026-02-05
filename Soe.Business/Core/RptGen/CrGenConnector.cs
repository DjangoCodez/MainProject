using RestSharp;
using Soe.CrGen.Common.DTO;
using SoftOne.Soe.Business.DTO;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using static Soe.CrGen.Common.Enumerations;
using SoftOne.Soe.Business.Util.LogCollector;
using System.Threading;
using Newtonsoft.Json;

namespace SoftOne.Soe.Business.Core.RptGen
{
    public class CrGenConnector : RptGenConnector
    {
        #region  -----CONSTANTS-----
        private const string CACHE_KEY_SERVER_ALIVE = "IsCrGenServerAlive";
        private const string REPORT_QUEUE_RESOURCE = "crGenRequest/Queue";
        private const string CACHE_CHECK_RESOURCE = "crGenRequest/CrGenReportTemplate/";
        private const string POST_REPORT_RESOURCE = "CrGenRequest";
        #endregion

        public CrGenConnector(ParameterObject parameterObject) : base(parameterObject)
        {
        }

        public override RptGenResultDTO GenerateReport(TermGroup_ReportExportType reportExportType, byte[] reportTemplate, XDocument doc, DataSet dataSet, List<RptGenRequestPicturesDTO> RptGenRequestPicturesDTOs, CultureInfo culture, string schema, string errorInfo)
        {
            RptGenResultDTO rptGenResultDTO = new RptGenResultDTO();

            #region Create Xdocument from dataset with schema
            if (doc == null && dataSet != null)
            {
                try
                {
                    doc = GetXDocumentFromDataSet(dataSet);

                    if (doc == null)
                        return rptGenResultDTO;
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
                        rptGenResultDTO.ErrorMessage = $"CrGenResultDTO GenerateReport: DataSet.WriteXml(tempPath): {ex2.ToString()}";
                        ssm.LogError(rptGenResultDTO.ErrorMessage);
                        rptGenResultDTO.Success = false;
                        rptGenResultDTO.ErrorMessage = $"CrGenResultDTO GenerateReport: {ex2.ToString()}";
                        return rptGenResultDTO;
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
                    crGenRequestPicturesDTOs = GetCrGenRequestPicturesDTO(RptGenRequestPicturesDTOs),
                    Schema = schema
                };

                crGenRequestDTO.ZippedXDocument = GetCompressXDocument(crGenRequestDTO.XDocument);
                if(crGenRequestDTO.ZippedXDocument!=null) 
                    crGenRequestDTO.XDocument = null;

                Uri uri = GetUri(new List<Uri>());
                RestClient client = base.GetRestClient(uri);
                string checkSum = crGenRequestDTO.CalculateChecksum(crGenRequestDTO);

                if (IsCachedTemplateAvailabe(client, CACHE_CHECK_RESOURCE, checkSum))
                {
                    crGenRequestDTO.ReportTemplate = string.Empty;
                    crGenRequestDTO.ReportTemplateCheckSum = checkSum;
                }

                rptGenResultDTO = ExecuteRequest(client, base.GetRestRequest(POST_REPORT_RESOURCE, Method.Post, crGenRequestDTO), uri);

                if (rptGenResultDTO == null)
                {
                    SysServiceManager ssm = new SysServiceManager(null);
                    ssm.LogError("crGenResultDTO is null " + errorInfo);
                }
                else if (!rptGenResultDTO.Success)
                {
                    SysServiceManager ssm = new SysServiceManager(null);
                    ssm.LogError($"!crGenResultDTO.Success: {rptGenResultDTO.ErrorMessage}  " + errorInfo);
                }
            }
            catch (Exception ex)
            {
                SysServiceManager ssm = new SysServiceManager(null);
                ssm.LogError($"CrGenResultDTO GenerateReport: {ex.ToString()} " + errorInfo);
                if (rptGenResultDTO != null)
                {
                    rptGenResultDTO.Success = false;
                    rptGenResultDTO.ErrorMessage = $"CrGenResultDTO GenerateReport: {ex.ToString()} " + errorInfo;
                }
            }

            return rptGenResultDTO;
        }

        protected override RptGenResultDTO ExecuteRequest(RestClient restClient, RestRequest request, Uri defaultBaseUri)
        {
            RestResponse response = restClient.Execute(request);

            if (string.IsNullOrEmpty(response.Content))
            {
                List<Uri> exclude = new List<Uri>() { defaultBaseUri };
                LogCollector.LogError($"ExecuteRequest Crgen on {defaultBaseUri} failed on first attempt. Error: " + response.ErrorMessage);
                var uri2 = GetUri(exclude);
                restClient = GetRestClient(uri2);
                response = restClient.Execute(request);

                if (string.IsNullOrEmpty(response.Content))
                {
                    exclude.Add(uri2);
                    LogCollector.LogError($"ExecuteRequest Crgen on {uri2} failed on second attempt. Error: " + response.ErrorMessage);
                    var uri3 = GetUri(exclude);
                    restClient = GetRestClient(uri3);
                    response = restClient.Execute(request);

                    if (string.IsNullOrEmpty(response.Content))
                        LogCollector.LogError($"ExecuteRequest Crgen on {uri3} failed on third attempt. Error: " + response.ErrorMessage);
                }
            }

            return JsonConvert.DeserializeObject<RptGenResultDTO>(response.Content);
        }

        private List<CrGenRequestPicturesDTO> GetCrGenRequestPicturesDTO(List<RptGenRequestPicturesDTO> pictureList)
        {
            return pictureList.Select(x => new CrGenRequestPicturesDTO() { Data = x.Data, Path = x.Path }).ToList<CrGenRequestPicturesDTO>();
        }

        private Uri GetUri(List<Uri> exclude)
        {
            List<Uri> uris = new List<Uri>();

            uris.Add(new Uri("https://s4crgen.softone.se/")); //S4crgen.softone.se   Prio 0 
            uris.Add(new Uri("https://s3crgen.softone.se/"));// Prio 1
            uris.Add(new Uri("https://testcrgen.softone.se/"));// Prio 2
            uris = uris.Where(w => !exclude.Contains(w)).ToList();

            return GetShortestQueue(uris, 0, REPORT_QUEUE_RESOURCE, CACHE_KEY_SERVER_ALIVE);
        }
    }
}
