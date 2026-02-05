using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Web;

namespace SoftOne.Soe.Web.soe.economy
{
    public partial class _default : PageBase
    {
        protected bool autoLoadOnStart = false;
        protected string clientIpNr;
        protected int accountYearId;
        protected bool accountYearIsOpen;

        protected AccountManager am;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Boolean.TryParse(QS["autoLoadOnStart"], out autoLoadOnStart);

            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);

            this.clientIpNr = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (String.IsNullOrEmpty(this.clientIpNr))
                this.clientIpNr = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];            
        }
    }
}
