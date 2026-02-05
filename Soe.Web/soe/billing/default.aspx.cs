using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;
using System.Web;

namespace SoftOne.Soe.Web.soe.billing
{
    public partial class _default : PageBase
    {
        #region Variables

        protected bool autoLoadOnStart = false;
        protected string clientIpNr;        
        protected int accountYearId;
        protected bool accountYearIsOpen;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing;
            base.Page_Init(sender, e);

            //Check for newer pricelist versions
            WholeSellerManager wm = new WholeSellerManager(ParameterObject);
            bool showPriceListUpdate = !GetSessionAndCookieBool(Constants.SESSION_PRICELIST_CHECKED) && wm.HasCompanyPriceListUpdates(SoeCompany.ActorCompanyId);

            //Check if current date is not in accountyear
            AccountManager am = new AccountManager(ParameterObject);
            bool showCreateAccountYear = !GetSessionAndCookieBool(Constants.SESSION_ACCOUNTYEAR_CHECKED) && !am.IsDateWithinCurrentAccountYear(SoeCompany.ActorCompanyId, DateTime.Now);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);

            if (showPriceListUpdate)
            {
                AddToSessionAndCookie(Constants.SESSION_PRICELIST_CHECKED, true);
                Scripts.Add("invoice/pricelistupdate.js");
            }
            else if (showCreateAccountYear)
            {
                AddToSessionAndCookie(Constants.SESSION_ACCOUNTYEAR_CHECKED, true);
                Scripts.Add("invoice/createAccountYear.js");
            }
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
