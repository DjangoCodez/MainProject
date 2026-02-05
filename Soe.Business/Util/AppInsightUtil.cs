using Microsoft.ApplicationInsights;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;

namespace SoftOne.Soe.Business.Util
{
    public static class AppInsightUtil
    {
        public static readonly string GetTimeSchedulePlanningShifts_Prefix = "GTSPS";
        public static readonly string GetTimeSchedulePlanningPeriods_Prefix = "GTSPP";
        public const string CorrelationIdItemKey = "Soe.CorrelationId";
        static TelemetryClient telemetry = new TelemetryClient();
        
        private static TelemetryClient GetClient()
        {
            if (telemetry == null)
                return new TelemetryClient();
            return telemetry;
        }
        public static void Log(Exception ex, string method, ParameterObject parameterObject)
        {
            try
            {
                if (ex != null)
                {
                    TelemetryClient client = new TelemetryClient();

                    if (string.IsNullOrEmpty(method))
                        method = "Unknown";

                    // Set up some properties:
                    var properties = new Dictionary<string, string> { { "Method", method } };

                    if (parameterObject != null)
                    {
                        properties.Add("LicenseGuid", parameterObject.LicenseGuid?.ToString() ?? "");
                        properties.Add("CompanyGuid", parameterObject.CompanyGuid?.ToString() ?? "");
                        properties.Add("UserGuid", parameterObject.UserId.ToString() ?? "");
                    }

                    // Send the exception telemetry:
                   Task.Run(() => client.TrackException(ex, properties));
                }
            }
            catch
            {
                // Do nothing
                // NOSONAR
            }
        }
        public static void IncrementCounter(string prefix, string metricId)
        {
            try
            {   if (string.IsNullOrEmpty(metricId))
                    metricId = "Unknown";
         
                GetClient()?.GetMetric(prefix + "_" + metricId)?.TrackValue(1);                
            }
            catch
            {
                // Do nothing
                // NOSONAR
            }
        }

        public static string GetCorrelationId()
        {
            try
            {
                var httpContext = HttpContext.Current;
                if (httpContext?.Items != null && httpContext.Items.Contains(CorrelationIdItemKey))
                    return httpContext.Items[CorrelationIdItemKey] as string;

                return null;
            }
            catch
            {
                // Do nothing
                // NOSONAR
                return null;
            }
        }
    }
}
