using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Business.Core;

namespace SoftOne.Soe.Web.soe
{
    public partial class _defaultHtml : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            #region Cookies

            // Cookie Detection (Set cookie and redirect with qs ce)
            if (Session[Constants.COOKIE_CHECK] == null && StringUtility.GetBool(QS["cd"]))
            {
                // Set the cookie and redirect so we can try to detect it.
                Session[Constants.COOKIE_CHECK] = "1";
                Response.Redirect(Request.Url.AbsolutePath + "?ce=1");
                return;
            }
            // Cookie Enabled (Read cookie)
            else if (StringUtility.GetBool(QS["ce"]))
            {
                if (Session[Constants.COOKIE_CHECK] != null && Session[Constants.COOKIE_CHECK].ToString() == "1")
                {
                    //Cookies are enabled
                    RedirectToSelf(null, false, true);
                    return;
                }
                else
                {
                    // Cookies are disabled
                    Response.Redirect("/errors/CookiesDisabled.aspx");
                    return;
                }
            }

            #endregion

            //Check Cookies before Init (which not will work without cookies enabled)
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            var sm = new SettingManager(ParameterObject);

            #endregion

            #region Browser

            //Also checked before login. But must check again here because users can login from www.softone.se
            if (!IsPostBack)
                Scripts.Add("/browser.js");

            #endregion

            //Get default reports
            bool useExternalLinks = sm.GetBoolSetting((int)SettingMainType.Company, (int)CompanySettingType.UseExternalLinks, SoeUser.UserId, SoeCompany.ActorCompanyId);
            string externalLink1 = sm.GetStringSetting((int)SettingMainType.Company, (int)CompanySettingType.ExternalLink1, SoeUser.UserId, SoeCompany.ActorCompanyId);
            string externalLink2 = sm.GetStringSetting((int)SettingMainType.Company, (int)CompanySettingType.ExternalLink2, SoeUser.UserId, SoeCompany.ActorCompanyId);
            string externalLink3 = sm.GetStringSetting((int)SettingMainType.Company, (int)CompanySettingType.ExternalLink3, SoeUser.UserId, SoeCompany.ActorCompanyId);
            int timeModuleIcon = sm.GetIntSetting((int)SettingMainType.Company, (int)CompanySettingType.TimeModuleIcon, SoeUser.UserId, SoeCompany.ActorCompanyId);
            string timeModuleHeader = sm.GetStringSetting((int)SettingMainType.Company, (int)CompanySettingType.TimeModuleHeader, SoeUser.UserId, SoeCompany.ActorCompanyId);

            billing.Visible = GetRolePermission(Feature.Billing) != Permission.None;
            economy.Visible = GetRolePermission(Feature.Economy) != Permission.None;
            time.Visible = GetRolePermission(Feature.Time) != Permission.None;
            communication.Visible = GetRolePermission(Feature.Communication) != Permission.None;
        }
    }
}