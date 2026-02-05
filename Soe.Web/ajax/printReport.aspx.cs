using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.ajax
{
    public partial class printReport : JsonBase
    {
        #region Variables

        private ReportManager rm;

        private Dictionary<string, string> parametersDict;
        private int reportId;
        private int sysReportTemplateTypeId;
        private SoeReportTemplateType reportTemplateType;
        private ReportSelectionDTO reportItem;

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            rm = new ReportManager(PageBase.ParameterObject);
                        
            if (!Int32.TryParse(QS["report"], out this.reportId))
                return;
            if (!Int32.TryParse(QS["templatetype"], out sysReportTemplateTypeId))
                return;

            this.reportTemplateType = (SoeReportTemplateType)sysReportTemplateTypeId;
            this.parametersDict = QS.ConvertToDict();

            #endregion

            #region Parse selection

            switch (reportTemplateType)
            {
                case SoeReportTemplateType.GeneralLedger:
                    this.reportItem = new GeneralLedgerReportDTO(SoeCompany.ActorCompanyId, reportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.TimeSalarySpecificationReport:
                    this.reportItem = new TimeSalarySpecificationReportDTO(SoeCompany.ActorCompanyId, reportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.TimeSalaryControlInfoReport:
                    this.reportItem = new TimeSalaryControlInfoReportDTO(SoeCompany.ActorCompanyId, reportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.KU10Report:
                    this.reportItem = new TimeKU10ReportDTO(SoeCompany.ActorCompanyId, reportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.AgdEmployeeReport:
                    this.reportItem = new TimeAgdEmployeeReportDTO(SoeCompany.ActorCompanyId, reportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
                case SoeReportTemplateType.TimeSaumaSalarySpecificationReport:
                    this.reportItem = new TimeSaumaSalarySpecificationReportDTO(SoeCompany.ActorCompanyId, reportId, sysReportTemplateTypeId);
                    break;
                case SoeReportTemplateType.PayrollProductReport:
                    this.reportItem = new PayrollProductReportDTO(SoeCompany.ActorCompanyId, reportId, sysReportTemplateTypeId, this.parametersDict);
                    break;
            }

            #endregion

            if (this.reportItem != null)
            {
                #region Save ReportUrl

                var result = rm.SaveReportUrl(this.reportItem.ReportGuid, this.reportItem.ToString(false), reportId, sysReportTemplateTypeId, SoeCompany.ActorCompanyId);
                if (!result.Success)
                    return;

                #endregion

                #region Redirect

                string url = this.reportItem.GetBaseUrl() + this.reportItem.ToShortString(false);
                Response.Redirect(url);

                #endregion
            }

            if (ResponseObject == null)
            {
                ResponseObject = new
                {
                    Found = false
                };
            }
        }
    }
}