using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.errors
{
    public partial class ReportError : PageBase
    {
        protected string HeaderMessage;
        protected string DetailMessage;

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            if (SoeUser == null)
                RedirectToHome();

            this.HeaderMessage = StringUtility.NullToEmpty(Context.Items["HeaderMessage"]);
            this.DetailMessage = StringUtility.NullToEmpty(Context.Items["DetailMessage"]);

            #endregion
        }
    }
}