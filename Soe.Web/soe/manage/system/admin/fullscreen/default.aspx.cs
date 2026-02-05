using System;
using System.Web;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.manage.system.admin.fullscreen
{
    public partial class _default : PageBase
    {
        protected bool autoLoadOnStart = false;
        protected string clientIpNr;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_System;
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