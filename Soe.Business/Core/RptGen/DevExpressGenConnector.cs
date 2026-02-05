using Newtonsoft.Json;
using RestSharp;
using RS.Shared.Common;
using RS.Shared.Enums;
using SoftOne.Soe.Business.DTO;
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
using System.Xml.Linq;

namespace SoftOne.Soe.Business.Core.RptGen
{
    public class DevExpressGenConnector : RptGenConnector
    {
        #region  -----CONSTANTS-----
        private const string CACHE_KEY_SERVER_ALIVE = "IsDxGenServerAlive";
        private const string REPORT_QUEUE_RESOURCE = "dxGenRequest/Queue";
        private const string CACHE_CHECK_RESOURCE = "dxGenRequest/ReportTemplate/";
        private const string POST_REPORT_RESOURCE = "dxGenRequest";
        #endregion

        public DevExpressGenConnector(ParameterObject parameterObject) : base(parameterObject)
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
                    ssm.LogError($"DxReportResultDTO GenerateReport: DataSet.WriteXml(stream, XmlWriteMode.WriteSchema): {ex.ToString()}");

                    try
                    {
                        string tempPath = string.Format(@"{0}{1}\{2}", ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL, Guid.NewGuid(), Guid.NewGuid());
                        dataSet.WriteXml(tempPath);
                        doc = XDocument.Load(tempPath);
                        File.Delete(tempPath);
                    }
                    catch (Exception ex2)
                    {
                        rptGenResultDTO.ErrorMessage = $"DxReportResultDTO GenerateReport: DataSet.WriteXml(tempPath): {ex2.ToString()}";
                        ssm.LogError(rptGenResultDTO.ErrorMessage);
                        rptGenResultDTO.Success = false;
                        rptGenResultDTO.ErrorMessage = $"DxReportResultDTO GenerateReport: {ex2.ToString()}";
                        return rptGenResultDTO;
                    }
                }
            }
            #endregion

            try
            {
                DxReportRequestDTO dxGenRequestDTO = new DxReportRequestDTO()
                {
                    ReportExportType = (DxReportExportType)reportExportType,
                    Guid = Guid.NewGuid(),
                    ReportTemplate = Convert.ToBase64String(reportTemplate),
                    XDocument = doc,
                    DataSet = new DataSet(),
                    ZippedDataSet = string.Empty,
                    Culture = culture,
                    dxRequestPicturesDTOs = GetDxGenRequestPicturesDTO(RptGenRequestPicturesDTOs),
                    Schema = schema
                };

                dxGenRequestDTO.ZippedXDocument = GetCompressXDocument(dxGenRequestDTO.XDocument);
                if (dxGenRequestDTO.ZippedXDocument != null)
                    dxGenRequestDTO.XDocument = null;

                Uri uri = GetUri(new List<Uri>());
                RestClient client = base.GetRestClient(uri);
                string checkSum = dxGenRequestDTO.CalculateChecksum(dxGenRequestDTO);

                if (IsCachedTemplateAvailabe(client, CACHE_CHECK_RESOURCE, checkSum))
                {
                    dxGenRequestDTO.ReportTemplate = string.Empty;
                    dxGenRequestDTO.ReportTemplateCheckSum = checkSum;
                }

                rptGenResultDTO = ExecuteRequest(client, base.GetRestRequest(POST_REPORT_RESOURCE, Method.Post, dxGenRequestDTO), uri);

                if (rptGenResultDTO == null)
                {
                    SysServiceManager ssm = new SysServiceManager(null);
                    ssm.LogError("DxReportResultDTO is null " + errorInfo);
                }
                else if (!rptGenResultDTO.Success)
                {
                    SysServiceManager ssm = new SysServiceManager(null);
                    ssm.LogError($"DxReportResultDTO.Success: {rptGenResultDTO.ErrorMessage}  " + errorInfo);
                }
            }
            catch (Exception ex)
            {
                SysServiceManager ssm = new SysServiceManager(null);
                ssm.LogError($"DxReportResultDTO GenerateReport: {ex.ToString()} " + errorInfo);
                if (rptGenResultDTO != null)
                {
                    rptGenResultDTO.Success = false;
                    rptGenResultDTO.ErrorMessage = $"DxReportResultDTO GenerateReport: {ex.ToString()} " + errorInfo;
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
                LogCollector.LogError($"ExecuteRequest DxReportingService on {defaultBaseUri} failed on first attempt. Error: " + response.ErrorMessage);
                var uri2 = GetUri(exclude);
                restClient = GetRestClient(uri2);
                response = restClient.Execute(request);

                if (string.IsNullOrEmpty(response.Content))
                {
                    exclude.Add(uri2);
                    LogCollector.LogError($"ExecuteRequest DxReportingService on {uri2} failed on second attempt. Error: " + response.ErrorMessage);
                    var uri3 = GetUri(exclude);
                    restClient = GetRestClient(uri3);
                    response = restClient.Execute(request);

                    if (string.IsNullOrEmpty(response.Content))
                        LogCollector.LogError($"ExecuteRequest DxReportingService on {uri3} failed on third attempt. Error: " + response.ErrorMessage);
                }
            }

            return JsonConvert.DeserializeObject<RptGenResultDTO>(response.Content);
        }

        private List<DxRequestPicturesDTO> GetDxGenRequestPicturesDTO(List<RptGenRequestPicturesDTO> pictureList)
        {
            return pictureList.Select(x => new DxRequestPicturesDTO() { Data = x.Data, Path = x.Path }).ToList<DxRequestPicturesDTO>();
        }

        private Uri GetUri(List<Uri> exclude)
        {
            List<Uri> uris = new List<Uri>();

            //uris.Add(new Uri("https://s4crgen.softone.se/")); //S4crgen.softone.se   Prio 0 
            //uris.Add(new Uri("https://s3crgen.softone.se/"));// Prio 1
            //uris.Add(new Uri("http://192.168.50.96/")); //SoftOnes19   Prio 2
            //uris.Add(new Uri("https://testcrgen.softone.se/"));// Prio 3
            uris = uris.Where(w => !exclude.Contains(w)).ToList();

            return GetShortestQueue(uris, 0, REPORT_QUEUE_RESOURCE, CACHE_KEY_SERVER_ALIVE);
        }
    }
}
