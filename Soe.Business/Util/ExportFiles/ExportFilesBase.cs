using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Business.Util.ExportFiles
{
    public class ExportFilesBase : ReportDataManager
    {
        #region Variables

        protected CreateReportResult ReportResult;
        protected EvaluatedSelection es
        {
            get
            {
                return this.ReportResult.EvaluatedSelection;
            }
        }

        #endregion

        #region Ctor

        public ExportFilesBase(ParameterObject parameterObject, CreateReportResult reportResult) : base(parameterObject)
        {
            this.ReportResult = reportResult;
        }

        #endregion

        public string GetYearMonthDay(DateTime day)
        {   
            // We need to force date into yyyymmdd - order instead of relying on toshortdatestring - functionality which depends on culture. 
            // This way it works also in Finland & other countries. 

            string yyyy = day.Year.ToString();
            string mm = day.Month.ToString("00");
            string dd = day.Day.ToString("00");
            return yyyy + mm + dd;
        }     

        public string OrgNrWith16(string orgnr)
        {
            return $"16{OrgNrRemoveDash(orgnr)}";
        }

        public string OrgNrWithout16(string orgnr)
        {
            if (orgnr.StartsWith("16"))
                orgnr = orgnr.Remove(0, 2);

            return OrgNrRemoveDash(orgnr);
        }

        public string OrgNrRemoveDash (string orgnr)
        {
            return orgnr.Replace("-", "");
        }
        
        public string FetchTime(DateTime day)
        {   
            // We need to force date into yyyymmdd - order instead of relying on toshortdatestring - functionality which depends on culture. 
            // This way it works also in Finland & other countries. 

            string yyyy = day.Year.ToString();
            string mm = day.Month.ToString("00");
            string dd = day.Day.ToString("00");
            string hh = day.Hour.ToString("00");
            string mn = day.Minute.ToString("00");
            string se = day.Second.ToString("00");
            string retdate = yyyy + '-' + mm + '-' + dd + 'T' + hh + ':' + mn + ':' + se;  // Format from Skatteverket
            return retdate;
        }
    }
}
