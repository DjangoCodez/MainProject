using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class refreshSession : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string date = QS["date"];
            if (!string.IsNullOrEmpty(date) && (IsUserLoggedIn || IsSupportAdmin))
            {
                //Accessing the user in Cache extends the sliding expiration to the timeout set in Web.config automatically
                AddToSessionAndCookie(Constants.COOKIE_KEEPSESSIONALIVE, DateTime.Now.ToString());
            }
        }
    }
}
