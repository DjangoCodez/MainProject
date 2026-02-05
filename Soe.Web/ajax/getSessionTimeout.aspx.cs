using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class getSessionTimeout : JsonBase
	{
        protected void Page_Load(object sender, EventArgs e)
		{
            string modified = GetSessionAndCookie(Constants.COOKIE_KEEPSESSIONALIVE);
			if(!String.IsNullOrEmpty(modified))
			{
                DateTime sessionModifed = Convert.ToDateTime(modified);
                DateTime warningTime = PageBase.GetTimeoutWarningTime();
                DateTime timeoutTime = PageBase.GetTimeoutLogoutTime();
                DateTime timeoutRedirTime = timeoutTime.AddSeconds(-Constants.SOE_SESSION_TIMEOUT_LOGOUT_SECONDS);
                DateTime now = DateTime.Now;
                TimeSpan timeoutTimeTS = timeoutTime.Subtract(now);
                double warningMS = (warningTime - now).TotalMilliseconds;
                double timeoutMS = (timeoutRedirTime - now).TotalMilliseconds;
                bool doLogout = now >= timeoutRedirTime;
                bool doWarn = !doLogout && now >= warningTime;

                ResponseObject = new
                {
                    Found = true,
                    Modified = sessionModifed.ToString(),
                    WarningTime = warningTime,
                    WarningMS = warningMS,
                    TimeoutTime = timeoutTime,
                    TimeoutMin = (int)timeoutTimeTS.TotalMinutes,
                    TimeoutSec = timeoutTimeTS.Seconds,
                    TimeoutMS = timeoutMS,
                    DoWarn = doWarn,
                    DoLogout = doLogout,
                    SessionCookieName = Constants.COOKIE_KEEPSESSIONALIVE,
                };
			}
			else
			{
				ResponseObject = new
				{
					Found = false,
				};
			}
		}
	}
}
