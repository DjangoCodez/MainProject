using System;

namespace SoftOne.Soe.Web
{
    public partial class _ContactInfo : PageBase
    {
        #region Variables


        protected string defaultLic;
        protected string defaultLogin;
        protected string message;
        protected bool showForm;

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            RedirectToLogin();
        }

    }
}
