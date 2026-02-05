using System;

namespace SoftOne.Soe.Web
{
    public partial class _ForgottenPW : PageBase
    {
        #region Variables


        protected string defaultLic;
        protected string defaultLogin;
        protected string message;

        #endregion

        public _ForgottenPW()
        {
            ShouldLoadOidcClientScripts = false;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            RedirectToLogin();
        }
    }
}
