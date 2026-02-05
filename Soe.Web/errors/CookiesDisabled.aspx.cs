using System;

namespace SoftOne.Soe.Web.errors
{
    public partial class CookiesDisabled : PageBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
            #region Init

            if (SoeUser == null)
                RedirectToHome();

            #endregion
		}
	}
}
