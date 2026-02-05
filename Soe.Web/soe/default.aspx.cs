using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe
{
    public partial class _default : PageBase
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
                    if (!base.IsUserLoggedIn)
                    {
                        // User is not logged in
                        RedirectToLogin();
                        return;
                    }
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
    }
}
