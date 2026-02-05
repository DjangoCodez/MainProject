using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.errors
{
	public partial class MultipleLicenses : PageBase
	{
        protected string PrevCompanyName;
        protected string PrevLicenseName;
        protected int CurrentCompanyId;
        protected string CurrentCompanyName;
        protected string CurrentLicenseName;
        protected string RedirectUrl;

        protected void Page_Load(object sender, EventArgs e)
		{
            #region Init

            this.PrevCompanyName = StringUtility.NullToEmpty(Context.Items["PrevCompanyName"]);
            this.PrevLicenseName = StringUtility.NullToEmpty(Context.Items["PrevLicenseName"]);
            this.CurrentCompanyId = Convert.ToInt32(StringUtility.NullToEmpty(Context.Items["CurrentCompanyId"]));
            this.CurrentCompanyName = StringUtility.NullToEmpty(Context.Items["CurrentCompanyName"]);
            this.CurrentLicenseName = StringUtility.NullToEmpty(Context.Items["CurrentLicenseName"]);
            this.RedirectUrl = UrlHome + "?c=" + CurrentCompanyId;

            if (SoeUser == null)
                RedirectToHome();

            #endregion
		}
	}
}
