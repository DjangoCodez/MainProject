using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Common.DTO.ApiExternal
{
    public class CompanyBatchValidationRequest
    {
        public BatchRequestEndpointType EndpointType { get; set; }
        public List<CompanyRequestKey> RequestKeys { get; set; } = new List<CompanyRequestKey>();
    }

    public class CompanyBatchRequest
    {
        public BatchRequestEndpointType EndpointType { get; set; }
        public List<CompanyRequestKey> RequestKeys { get; set; } = new List<CompanyRequestKey>();
        public CompanySelection Selection { get; set; }
    }

    public class CompanyRequestKey
    {
        public string Key { get; set; }

    }

    public class CompanyBatchResponse
    {
        public List<CompanyBatchConnection> CompanyConnections { get; set; }
        public string Message { get; set; }
        public bool IsSuccess => CompanyConnections != null && CompanyConnections.Count > 0;
    }

    public class CompanyBatchConnection
    {
        public Guid CompanyApiKey { get; set; }
        public CompanyRequestKey RequestKey { get; set; }
        public string CompanyToken { get; set; }
    }

    public enum BatchRequestEndpointType
    {
        Unknown = 0,
        AccountAggregatedTime = 1
    }

    public class CompanySelection
    {
        public DateRangeSelection DateRange { get; set; }
    }

    public class DateRangeSelection
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
