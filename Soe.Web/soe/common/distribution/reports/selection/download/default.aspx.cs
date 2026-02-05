using SoftOne.Soe.Business.Core.Reporting;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Web;

namespace SoftOne.Soe.Web.soe.common.distribution.reports.selection.download
{
    public partial class _default : PageBase
    {
        #region Variables

        private ReportGenManager rgm;

        //Module specifics
        public bool EnableEconomy { get; set; }
        public bool EnableBilling { get; set; }
        public bool EnableTime { get; set; }

        #endregion

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
                    case Feature.Economy_Distribution_Reports_Selection_Download:
                        EnableEconomy = true;
                        break;
                    case Feature.Billing_Distribution_Reports_Selection_Download:
                        EnableBilling = true;
                        break;
                    case Feature.Time_Distribution_Reports_Selection_Download:
                        EnableTime = true;
                        break;
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            rgm = new ReportGenManager(ParameterObject);

            string guid = QS["guid"];

            bool upload = false;
            Boolean.TryParse(QS["upload"], out upload);

            #endregion

            var reportPrintout = Session[Constants.SESSION_DOWNLOAD_REPORT_ITEM + guid] as ReportPrintout;
            if (reportPrintout != null || upload)
            {
                Session[Constants.SESSION_DOWNLOAD_REPORT_ITEM + guid] = null;

                try
                {
                    if (upload)
                    {
                        Response.Write("UploadComplete");
                    }
                    else if (reportPrintout.DeliveryType == (int)TermGroup_ReportPrintoutDeliveryType.Email)
                    {
                        #region Email

                        Response.Write("EmailSent");

                        #endregion
                    }
                    else
                    {
                        #region Export

                        string contentType = "";
                        string fileType = "";
                        string imageSrc = "";
                        rgm.GetResponseContentType((SoeExportFormat)reportPrintout.ExportFormat, out contentType, out fileType, out imageSrc);

                        reportPrintout.ReportName = reportPrintout.ReportName.ToValidFileName().RemoveWhiteSpace();

                        HttpContext.Current.Response.ContentType = contentType;
                        HttpContext.Current.Response.Expires = Constants.SOE_SESSION_TIMEOUT_MINUTES;
                        HttpContext.Current.Response.Clear();
                        HttpContext.Current.Response.ClearContent();
                        HttpContext.Current.Response.ClearHeaders();
                        HttpContext.Current.Response.Cache.SetNoServerCaching();
                        HttpContext.Current.Response.AddHeader("Content-Type", contentType);
                        HttpContext.Current.Response.AddHeader("Content-Disposition", "Attachment; Filename=\"" + (reportPrintout.ReportName += fileType) + "\"");
                        HttpContext.Current.Response.BinaryWrite(reportPrintout.Data);
                        HttpContext.Current.Response.End(); //Causes ThreadAbortException exception
                        HttpContext.Current.ApplicationInstance.CompleteRequest();

                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString(); //prevent compiler warning
                }
            }
        }
    }
}
