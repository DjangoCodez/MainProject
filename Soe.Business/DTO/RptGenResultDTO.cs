using SoftOne.Soe.Business.Core.RptGen;
using SoftOne.Soe.Common.Util;
using System;
using System.Xml.Linq;

namespace SoftOne.Soe.Business.DTO
{
    public class RptGenResultDTO
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public RptErrorType ErrorType { get; set; }

        public Guid Guid { get; set; }

        public XDocument XDocument { get; set; }

        public byte[] GeneratedReport { get; set; }

        public string ReportName { get; set; }

        public SoeExportFormat ReportExportType { get; set; }

        public RptGenResultDTO()
        {
            Success = true;
            ErrorType = RptErrorType.Unknown;
        }
    }
}
