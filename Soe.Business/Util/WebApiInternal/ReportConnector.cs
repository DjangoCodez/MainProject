using Newtonsoft.Json;
using RestSharp;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Status;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Business.Util.WebApiInternal
{
    public class ReportConnector : ConnectorBase
    {
        private static int tryChangeOnCount = 10;

        new public Uri GetUri()
        {
            SettingManager sm = new SettingManager(null);
            string address = string.Empty;
            try
            {
                if (sm.isTest())
                {
                    address = sm.GetApiInternalURL();

                    if (!string.IsNullOrEmpty(address))
                    {
                        SysLogConnector.LogErrorString("using apiInternal to generate report: " + address);
                        return new Uri(UrlUtil.ToValidUrl(address));
                    }
                    else
                        SysLogConnector.LogErrorString("GetApiInternalOnTest failed");
                }

                address = sm.GetStringSetting(SettingMainType.Application, (int)ApplicationSettingType.WebApiInternalUrl, 0, 0, 0);
#if DEBUG
                address = "http://localhost:1362/";

#endif
                if (RunningPrintReport(address) >= tryChangeOnCount)
                {
                    var addressSecondary = sm.GetStringSetting(SettingMainType.Application, (int)ApplicationSettingType.WebApiInternalUrlSecondary, 0, 0, 0);

                    if (!string.IsNullOrEmpty(addressSecondary) && RunningPrintReport(addressSecondary) < tryChangeOnCount)
                        return new Uri(UrlUtil.ToValidUrl(addressSecondary));

                    try
                    {
                        int? sysCompDbId = SysServiceManager.GetSysCompDBIdFromSetting();
                        if (sysCompDbId.HasValue && sysCompDbId.Value != 0)
                        {
                            var urlFromSoftOneStatus = SoftOneStatusConnector.GetApiInternal(sysCompDbId.Value);
                            if (!string.IsNullOrEmpty(urlFromSoftOneStatus) && urlFromSoftOneStatus != address && RunningPrintReport(urlFromSoftOneStatus) < tryChangeOnCount)
                            {
                                SysLogConnector.LogErrorString($"RunningPrintReport(address) >= tryChangeOnCount on url: {address} new url: {urlFromSoftOneStatus}");
                                return new Uri(UrlUtil.ToValidUrl(urlFromSoftOneStatus));
                            }
                            else
                            {
                                urlFromSoftOneStatus = SoftOneStatusConnector.GetApiInternal(sysCompDbId.Value);
                                if (!string.IsNullOrEmpty(urlFromSoftOneStatus))
                                {
                                    SysLogConnector.LogErrorString($"Second try RunningPrintReport(address) >= tryChangeOnCount on url: {address} new url: {urlFromSoftOneStatus}");
                                    return new Uri(UrlUtil.ToValidUrl(urlFromSoftOneStatus));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SysLogConnector.LogErrorString($"Second try RunningPrintReport(address) >= tryChangeOnCount on url: {address} failed on exception: {ex}");
                    }
                }

                return new Uri(UrlUtil.ToValidUrl(address));
            }
            catch (Exception ex2)
            {
                try
                {
                    address = sm.GetApiInternalURL(true);
                    LogCollector.LogCollector.LogError($"Get reportUrl failed {address} with exception2: {ex2} fallback to Adress {address}", 1);
                    return new Uri(UrlUtil.ToValidUrl(address));
                }
                catch (Exception ex3)
                {
                    LogCollector.LogCollector.LogError($"Get reportUrl failed with exception3: {ex3}", 1);
                }
            }

            return new Uri("");
        }

        public int PrintReportPackageData(List<EvaluatedSelection> evaluatedSelections, int actorCompanyId, int userId, string culture)
        {
            RestClientOptions options = new RestClientOptions()
            {
                BaseUrl = GetUri(),
                Timeout = TimeSpan.FromMilliseconds((60 * 10 * 1000))

            };
            var client = new GoRestClient(options);
            var request = CreateRequest("Report/PrintReportPackageData", Method.Post, evaluatedSelections);
            request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
            request.AddParameter("userId", userId, ParameterType.QueryString);
            request.AddParameter("culture", culture, ParameterType.QueryString);
            RestResponse response = client.Execute(request);
            return JsonConvert.DeserializeObject<int>(response.Content);
        }

        public int PrintReport(EvaluatedSelection es, int actorCompanyId, int userId, string culture)
        {
            RestClientOptions options = new RestClientOptions()
            {
                BaseUrl = GetUri(),
                Timeout = TimeSpan.FromMilliseconds((60 * 10 * 1000))

            };
            var client = new GoRestClient(options);
            var request = CreateRequest("Report/PrintReport", Method.Post, es);
            request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
            request.AddParameter("userId", userId, ParameterType.QueryString);
            request.AddParameter("culture", culture, ParameterType.QueryString);
            RestResponse response = client.Execute(request);
            return JsonConvert.DeserializeObject<int>(response.Content);
        }

        public ReportPrintoutDTO PrintMigratedReportDTO(int reportPrintoutId, int actorCompanyId, int userId, int roleId)
        {
            try
            {
                RestClientOptions options = new RestClientOptions()
                {
                    BaseUrl = GetUri(),
                    Timeout = TimeSpan.FromMilliseconds((60 * 10 * 1000))

                };
                var client = new GoRestClient(options);
                var request = CreateRequest("Report/PrintReport/Queue/FromSaved", Method.Post, null);
                request.AddParameter("reportPrintoutId", reportPrintoutId, ParameterType.QueryString);
                request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                request.AddParameter("userId", userId, ParameterType.QueryString);
                request.AddParameter("roleId", roleId, ParameterType.QueryString);
                RestResponse response = client.Execute(request);
                return JsonConvert.DeserializeObject<ReportPrintoutDTO>(response.Content);
            }
            catch (Exception ex)
            {
                SysLogConnector.SaveErrorMessage("Connector PrintMigratedReportDTO failed " + ex.ToString());
                return null;
            }
        }

        public ReportPrintoutDTO PrintMigratedReportDTO(ReportJobDefinitionDTO job, int actorCompanyId, int userId, int roleId, bool forcePrint)
        {
            try
            {
                var client = new GoRestClient(GetUri());
                var request = CreateRequest("Report/PrintReport/Queue", Method.Post, job);
                request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
                request.AddParameter("userId", userId, ParameterType.QueryString);
                request.AddParameter("roleId", roleId, ParameterType.QueryString);
                request.AddParameter("forcePrint", forcePrint, ParameterType.QueryString);
                RestResponse response = client.Execute(request);
                return JsonConvert.DeserializeObject<ReportPrintoutDTO>(response.Content);
            }
            catch (Exception ex)
            {
                SysLogConnector.SaveErrorMessage("Connector PrintMigratedReportDTO failed " + ex.ToString());
                return null;
            }
        }

        public int RunningPrintReport(string url)
        {
            try
            {
                url = UrlUtil.ToValidUrl(url);
                var client = new GoRestClient(url);
                var request = CreateRequest("Report/RunningPrintReport", Method.Get, null);
                RestResponse response = client.Execute(request);
                return JsonConvert.DeserializeObject<int>(response.Content);
            }
            catch
            {
                return tryChangeOnCount;
            }
        }

        public byte[] PrintReportGetData(EvaluatedSelection es, int actorCompanyId, int userId, string culture)
        {
            var client = new GoRestClient(GetUri());
            var request = CreateRequest("Report/PrintReport", Method.Post, es);
            request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
            request.AddParameter("userId", userId, ParameterType.QueryString);
            request.AddParameter("culture", culture, ParameterType.QueryString);
            RestResponse response = client.Execute(request);

            var reportPrintoutDTO = JsonConvert.DeserializeObject<ReportPrintoutDTO>(response.Content);

            if (reportPrintoutDTO != null && reportPrintoutDTO.Data != null)
                return reportPrintoutDTO.Data;
            else
                return new byte[0];
        }

        public void GenerateReportForEdi(List<int> ediEntryIds, int actorCompanyId, int userId, string culture)
        {
            var client = new GoRestClient(GetUri());
            var request = CreateRequest("Report/GenerateReportForEdi", Method.Post, ediEntryIds);
            request.AddParameter("actorCompanyId", actorCompanyId, ParameterType.QueryString);
            request.AddParameter("userId", userId, ParameterType.QueryString);
            request.AddParameter("culture", culture, ParameterType.QueryString);
            RestResponse response = client.Execute(request);

        }

        public int GetNrOfLoads()
        {
            var client = new GoRestClient(GetUri());
            var request = CreateRequest("Report/GetNrOfLoads", Method.Get, null);
            RestResponse response = client.Execute(request);
            return JsonConvert.DeserializeObject<int>(response.Content);
        }

        public int GetNrOfDispose()
        {
            var client = new GoRestClient(GetUri());
            var request = CreateRequest("Report/GetNrOfDispose", Method.Get, null);
            RestResponse response = client.Execute(request);
            return JsonConvert.DeserializeObject<int>(response.Content);
        }
    }
}
