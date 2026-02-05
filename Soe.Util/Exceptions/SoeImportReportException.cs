using System;

namespace SoftOne.Soe.Util.Exceptions
{
    [Serializable]
    public class SoeImportReportException : SoeException
    {
        public SoeImportReportException(string message, int importReportId, int destinationReportId, int importCompanyId, int destinationCompanyId, string source)
            : base(message + ". " + GetReportInfo(importReportId, destinationReportId) + ". " +
                                    GetCompanyInfo(importCompanyId, destinationCompanyId))
        {
            base.Source = source;
        }

        public SoeImportReportException(string message, int importReportId, int destinationReportId, int importCompanyId, int destinationCompanyId, Exception innerException, string source)
            : base(message + ". " + GetReportInfo(importReportId, destinationReportId) + ". " +
                                    GetCompanyInfo(importCompanyId, destinationCompanyId),
                                    innerException)
        {
            base.Source = source;    
        }

        private static string GetReportInfo(int importReportId, int destinationReportId)
        {
            return "Import Report [" + importReportId + "]. " +
                   "Destination Report [" + destinationReportId + "]";
        }

        private static string GetCompanyInfo(int importCompanyId, int destinationCompanyId)
        {
            return "Import Company [" + importCompanyId + "]. " +
                   "Destination Company [" + destinationCompanyId + "]";
        }
    }
}
