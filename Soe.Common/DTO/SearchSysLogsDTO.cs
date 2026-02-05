using SoftOne.Soe.Common.Attributes;
using System;

namespace SoftOne.Soe.Common.DTO
{
    [TSInclude]
    public class SearchSysLogsDTO
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public string Level { get; set; }
        public string LicenseSearch { get; set; }
        public string CompanySearch { get; set; }
        public string RoleSearch { get; set; }
        public string UserSearch { get; set; }
        public string IncMessageSearch { get; set; }
        public string ExlMessageSearch { get; set; }
        public string IncExceptionSearch { get; set; }
        public string ExExceptionSearch { get; set; }
        public int? NoOfRecords { get; set; }
        public bool ShowUnique { get; set; }
    }
}
