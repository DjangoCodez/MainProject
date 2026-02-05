using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Security;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Linq;
using System.Net;
using System.Web;

namespace SoftOne.Soe.Web.soe.manage.companies.edit.remote
{
    public partial class _default : PageBase
    {
        #region Variables

        private int actorCompanyId;
        private int userId;
        private int logout;
        private int login;
        private bool superAdmin;
        private bool forceOut;

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            //Is not a SysFeature

            if (Int32.TryParse(QS["login"], out login) && login > 0)
            {
                if (!SoeLicense.Support)
                    RedirectToUnauthorized(UnauthorizationType.DataAuthorityMissing);

                if (!Int32.TryParse(QS["company"], out actorCompanyId) && !Int32.TryParse(QS["user"], out userId))
                    RedirectToUnauthorized(UnauthorizationType.DataAuthorityMissing);

                Boolean.TryParse(QS["super"], out superAdmin);
                Boolean.TryParse(QS["forceout"], out forceOut);

                if (forceOut)
                    LogoutSupport(acceptNogLoggedIn: true);
                LoginSupport();
            }
            else if (Int32.TryParse(QS["logout"], out logout) && logout > 0)
            {
                LogoutSupport();
            }
            else
            {
                Response.Redirect(Request.UrlReferrer.ToString());
            }
        }

        private void LoginSupport()
        {
            //Check that not already support logged in
            if (ParameterObject == null || ParameterObject.HasSupportUserAndCompany)
                Response.Redirect(Request.UrlReferrer.ToString());

            Company company = null;
            User user = null;
            Role role = null;
            SoeSupportLoginState state = SoeSupportLoginState.Unknown;
            string url = "";
            string validSysServerUrl = "";

            if (actorCompanyId > 0)
                state = LoginManager.LoginSupportByCompany(actorCompanyId, SoeUser, out company, out user, out role);
            else if (userId > 0)
                state = LoginManager.LoginSupportByUser(userId, SoeUser, out company, out user, out role);

            if (company != null && !HasValidLicenseToSupportLogin(company.LicenseId, company.License.LicenseNr))
                state = SoeSupportLoginState.InvalidLicense;

            if (state == SoeSupportLoginState.OK && company != null)
            {
                #region Check SysServer

                string currentUrl = HttpContext.Current.Request.Url.AbsoluteUri;

                if (!UrlUtil.HasXForwardedHeaders(HttpContext.Current.Request) && !currentUrl.Contains("localhost") && !currentUrl.Contains("release.softone.se") && company.License.SysServerId.HasValue && !LoginManager.IsOnValidSysServer(company.License.SysServerId.Value, currentUrl, out validSysServerUrl))
                    state = SoeSupportLoginState.NotLoggedIn;

                #endregion

                if (state == SoeSupportLoginState.OK)
                {
                    #region Company and User

                    UserManager um = new UserManager(null);
                    CompanyManager cm = new CompanyManager(null);
                    var defaultRoleId = um.GetDefaultRoleId(company.ActorCompanyId, user.UserId);
                    ParameterObject.SignInSupport(um.GetSoeUser(actorCompanyId, user), cm.GetSoeCompany(company), superAdmin, actorCompanyId > 0, userId > 0, activeRoleId: defaultRoleId);

                    //Log4net
                    SysLogManager.SetLog4NetUserProperties();

                    #endregion

                    #region UserSession - must be after Company and User

                    if (EnableUserSession)
                    {
                        var result = um.LoginUserSession(UserId, SoeUser.LoginName, SoeCompany.ActorCompanyId, SoeCompany.Name, supportUserId: SoeSupportUserId);
                        if (result.Success)
                        {
                            if (!result.StringValue.IsNullOrEmpty() && int.TryParse(result.StringValue, out int userSessionId))
                            {
                                using (CompEntities entities = new CompEntities())
                                {
                                    var userSession = entities.UserSession.FirstOrDefault(f => f.UserSessonId == userSessionId);

                                    if (userSession != null && HttpContext.Current != null && HttpContext.Current.Request != null)
                                    {
                                        var bc = HttpContext.Current.Request.Browser;
                                        if (bc != null)
                                        {
                                            //Browser
                                            userSession.Browser += bc.Browser + " ";
                                            userSession.Browser += bc.Version + " ";
                                            if (!bc.Cookies)
                                                userSession.Browser += "Cookies:0" + " ";
                                            if (!bc.SupportsCss)
                                                userSession.Browser += "CSS:0" + " ";
                                            if (bc.Beta)
                                                userSession.Screen += "Beta:1" + " ";

                                            //Platform
                                            userSession.Platform += bc.Platform + " ";
                                            if (bc.Win16)
                                                userSession.Platform += "Win16:1" + " ";
                                            else if (bc.Win32)
                                                userSession.Platform += "Win32:1" + " ";

                                            //ClientIP
                                            userSession.ClientIP += GetClientIP();

                                            //Host
                                            userSession.Host += GetHostIP() + " ";
                                            userSession.Host += GetHostName();

                                            //CacheCredentials
                                            userSession.CacheCredentials += GetUserEnvironmentInfo();
                                            entities.SaveChanges();
                                        }
                                    }
                                }
                            }
                            AddToSessionAndCookie(Constants.COOKIE_USERSESSIONID_REMOTELOGIN, result.StringValue);
                        }
                    }

                    #endregion

                    #region URL

                    if (String.IsNullOrEmpty(url))
                        url = "/soe/?c=" + SoeCompany.ActorCompanyId;

                    #endregion
                }
            }

            if (String.IsNullOrEmpty(url))
            {
                if (state == SoeSupportLoginState.NotAllowed)
                    RedirectToRemoteLoginFailed(RemoteLoginFailedType.NotAllowed, company?.AllowSupportLoginTo?.ToString("yyyy-MM-dd HH:mm") ?? String.Empty);
                else if (state == SoeSupportLoginState.InvalidLicense)
                    RedirectToRemoteLoginFailed(RemoteLoginFailedType.InvalidLicense);
                else if (!String.IsNullOrEmpty(validSysServerUrl))
                    RedirectToRemoteLoginFailed(RemoteLoginFailedType.InvalidServer, validSysServerUrl);
                else
                    RedirectToRemoteLoginFailed(RemoteLoginFailedType.Failed);
            }
            else
            {
                Redirect(url);
            }
        }
        private string GetUserEnvironmentInfo()
        {
            return GetHostInfo() + Constants.SOE_ENVIRONMENT_CONFIGURATION_SEPARATOR + GetClientIP();
        }
        private string GetHostIP()
        {
            string ipNr = "";

            IPHostEntry hostEntry = Dns.GetHostEntry(GetHostName());
            if (hostEntry != null)
            {
                IPAddress[] ipHostEntry = hostEntry.AddressList;
                ipNr = ipHostEntry[ipHostEntry.Length - 1].ToString();
            }
            return ipNr;
        }
        private string GetHostName()
        {
            return Dns.GetHostName();
        }

        private string GetHostInfo()
        {
            return $"{GetHostIP()}_{GetHostName()}";
        }
        private string GetClientIP()
        {
            string ipNr = "";

            try
            {
                if (HttpContext.Current?.Request != null)
                {
                    ipNr = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                    if (string.IsNullOrEmpty(ipNr))
                        ipNr = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }
            }
            catch
            {
                //Continue on error on IP
            }

            return ipNr;
        }
        private void LogoutSupport(bool acceptNogLoggedIn = false)
        {
            //Check that support is logged in
            if (ParameterObject == null || !ParameterObject.HasSupportUserAndCompany)
            {
                if (acceptNogLoggedIn)
                    return;
                RedirectToLogout();
            }

            LoginManager lm = new LoginManager(ParameterObject);
            UserManager um = new UserManager(ParameterObject);
            if (lm.Logout(SoeUser, supportLogout: true))
            {
                #region UserSession - muste be before Company and user

                if (EnableUserSession &&
                    GetSessionAndCookie(Constants.COOKIE_USERSESSIONID_REMOTELOGIN) != null &&
                    Int32.TryParse(GetSessionAndCookie(Constants.COOKIE_USERSESSIONID_REMOTELOGIN), out int userSessionId))
                {
                    um.LogoutUserSession(SoeUser, userSessionId, SoeSupportUserId);
                }

                #endregion

                #region Company and user

                ParameterObject.SignOutSupport();

                //Log4net
                SysLogManager.SetLog4NetUserProperties();

                #endregion

                #region Redirect

                Redirect($"/soe/manage/contracts/?c={ParameterObject.ActorCompanyId}");

                #endregion
            }
        }
    }
}
