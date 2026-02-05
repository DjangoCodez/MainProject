using System;
using SoftOne.Soe.Common.Util;
using System.Web;

namespace SoftOne.Soe.Web.soe.communication
{
    public partial class _default : PageBase
    {
        protected bool autoLoadOnStart = false;
        protected string clientIpNr;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.None;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Boolean.TryParse(QS["autoLoadOnStart"], out autoLoadOnStart);

            this.clientIpNr = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (String.IsNullOrEmpty(this.clientIpNr))
                this.clientIpNr = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];           
        }
    }
}