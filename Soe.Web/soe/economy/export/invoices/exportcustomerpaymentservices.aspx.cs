using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.Config;
using SoftOne.Soe.Common.Util;
using System;
using System.IO;
using System.Web;

namespace SoftOne.Soe.Web.soe.economy.export.invoices
{
    public partial class exportcustomerpaymentservices : PageBase
    {
        #region Variables

        private AccountManager am = null;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Export_Invoices_PaymentService;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init
            am = new AccountManager(ParameterObject);
            #endregion

            //Optional parameters
            if (QS["filename"] != null && QS["filename"] != String.Empty)
            { 
                string filename = QS["filename"];
                string path = ConfigSettings.SOE_SERVER_DIR_TEMP_PHYSICAL + filename;
                
                try
                {
                    HttpContext.Current.Response.ClearContent();
                    HttpContext.Current.Response.ClearHeaders();
                    HttpContext.Current.Response.ContentType = "text/plain";
                    HttpContext.Current.Response.AddHeader("Content-Disposition", "Attachment; Filename=" + filename);
                    HttpContext.Current.Response.Write(File.ReadAllText(path));
                    HttpContext.Current.Response.End();
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
                catch (Exception ex)
                {
                    SysLogManager.LogError<exportcustomerpaymentservices>(ex);
                }
            }

            am.GetAccountYearInfo(CurrentAccountYear, out _, out _);            
        }
    }
}