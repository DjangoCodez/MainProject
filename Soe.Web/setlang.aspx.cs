using System;
using System.Web;

namespace SoftOne.Soe.Web
{
	public partial class setlang : PageBase
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			string cultureCode = QS["lang"];
            if (!string.IsNullOrEmpty(cultureCode))
			{
				base.SetLanguage(cultureCode);

                string prevousUrl = QS["prev"];
                if (!string.IsNullOrEmpty(prevousUrl))
                {
                    string url = HttpUtility.UrlDecode(prevousUrl);
                    Response.Redirect(url);
                }
                else
                    Response.Redirect(Request.UrlReferrer.ToString());
            }
		}
	}
}
