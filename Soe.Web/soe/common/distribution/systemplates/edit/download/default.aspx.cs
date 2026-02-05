using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Data;

namespace SoftOne.Soe.Web.soe.common.distribution.systemplates.edit.download
{
    public partial class _default : PageBase
    {
        private ReportManager rm;
        private ReportGenManager rgm;

        //Module specifics
        public bool EnableEconomy { get; set; }
        public bool EnableBilling { get; set; }
        public bool EnableTime { get; set; }

        protected SysReportTemplate sysReportTemplate;

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);

            // Add scripts and style sheets
        }

        private void EnableModuleSpecifics()
        {
            if (CTX["Feature"] != null)
            {
                this.Feature = (Feature)CTX["Feature"];
                switch (this.Feature)
                {
                    case Feature.Economy_Distribution_SysTemplates_Edit_Download:
                        EnableEconomy = true;
                        break;
                    case Feature.Billing_Distribution_SysTemplates_Edit_Download:
                        EnableBilling = true;
                        break;
                    case Feature.Time_Distribution_SysTemplates_Edit_Download:
                        EnableTime = true;
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (rm == null)
                rm = new ReportManager(ParameterObject);
            if (rgm == null)
                rgm = new ReportGenManager(ParameterObject);

            //Mandatory parameters
            int sysReportTemplateId;
            if (Int32.TryParse(QS["systemplate"], out sysReportTemplateId))
            {
                sysReportTemplate = rm.GetSysReportTemplate(sysReportTemplateId);
                if (sysReportTemplate == null)
                    throw new SoeEntityNotFoundException("SysReportTemplate", this.ToString());
            }
            else
                throw new SoeQuerystringException("template", this.ToString());

            try
            {
                switch (sysReportTemplate.SysReportTypeId)
                {
                    case (int)SoeReportType.CrystalReport:
                        ExportCrystalReportToHttpResponse();
                        break;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        private string RemoveLeadingDigits(string name)
        {
            var digits = new[] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            return name.TrimStart(digits).Trim();
        }

        private void SendFile(string name, byte[] bytes)
        {
            Response.Clear();
            Response.ContentType = "application/octet-stream";
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + RemoveLeadingDigits(name));

            Response.BinaryWrite(bytes);
            Response.End();
        }

        private void ExportCrystalReportToHttpResponse()
        {
            //Get SysReportTemplateType
            SoeReportTemplateType reportTemplateType = (SoeReportTemplateType)sysReportTemplate.SysReportTemplateTypeId;
            
            //Read default XML to DataSet
            DataSet ds = rgm.GetDefaultXmlDataSet(reportTemplateType);
            if (ds != null)
            {
                //Load ReportDocument
                SendFile(sysReportTemplate.FileName, sysReportTemplate.Template);
            }
        }
    }
}
