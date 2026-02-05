using System;
using System.Web;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.WebApiInternal;

namespace SoftOne.Soe.Web.soe.manage.support.logs
{
    public partial class _default : PageBase
    {
        protected SoeLogType logType;
        protected string clientIpNr;
        protected int nrOfLoadsRpdWs;
        protected int nrOfDisposeRpdWs;
        protected int diffRpdWs;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Support_Logs;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Int32.TryParse(QS["type"], out int type))
                this.logType = (SoeLogType)type;
            else
                this.logType = SoeLogType.System_Error;

            this.clientIpNr = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (String.IsNullOrEmpty(this.clientIpNr))
                this.clientIpNr = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            if (UseCrystalService())
            {
                try
                {                    
                    var channel = GetCrystalServiceChannel();
                    this.nrOfLoadsRpdWs = channel.GetNrOfLoads();
                    this.nrOfDisposeRpdWs = channel.GetNrOfDispose();
                    this.diffRpdWs = this.nrOfDisposeRpdWs - this.nrOfLoadsRpdWs;
                }
                catch (Exception ex)
                {
                    SysLogManager.LogError<_default>(ex);
                }
            }
            else if (UseWebApiInternal())
            {
                try
                {
                    var connector = new ReportConnector();
                    this.nrOfLoadsRpdWs = connector.GetNrOfLoads();
                    this.nrOfDisposeRpdWs = connector.GetNrOfDispose();
                    this.diffRpdWs = this.nrOfDisposeRpdWs - this.nrOfLoadsRpdWs;
                }
                catch (Exception ex)
                {
                    SysLogManager.LogError<_default>(ex);
                }
            }            
        }
    }
}
