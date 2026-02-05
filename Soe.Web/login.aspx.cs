using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.SysService;
using SoftOne.Soe.Business.Security;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.Security;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Web;

namespace SoftOne.Soe.Web
{
    public partial class Login : PageBase
    {
        private string cookieKey = "LCounter";
        private string cookieSoelicenseid = "soelicenseid";
        //private string changeDomain = "changedomain";
        private bool _useLoadBalancer = true;

        /// <summary>
        /// Initializes the page and handles authentication challenges.
        /// If the user is not authenticated, an authentication challenge is initiated.
        /// </summary>
        protected override void Page_Init(object sender, EventArgs e)
        {
            var authResult = HttpContext.Current.GetOwinContext().Authentication.AuthenticateAsync("Cookies").Result;
            var licenseId = 0;
            if (Request.QueryString.ToString().Contains(cookieSoelicenseid))
            {
                var soelicense = Request.QueryString[cookieSoelicenseid];
                int.TryParse(soelicense, out licenseId);
                if (licenseId != 0)
                    AddToSessionAndCookie(cookieSoelicenseid, LoginHelper.GetLicenseIdCookieKey(licenseId), useSession: false);
            }

            if (authResult == null)
            {
                var addedQueryString = string.Empty;
                if (licenseId != 0)
                    addedQueryString = $"?{cookieSoelicenseid}={licenseId}";
                HttpContext.Current.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = $"/login.aspx{addedQueryString}" },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);
            }
            else
            {
                var actorCompanyIdFromClaims = authResult.Identity.FindFirst(SoeClaimType.ActorCompanyId);
                var userIdFromClaims = authResult.Identity.FindFirst(SoeClaimType.UserId);

                if (ParameterObject != null && HasIncoherentLogin(authResult, actorCompanyIdFromClaims, userIdFromClaims, ParameterObject.ActorCompanyId, ParameterObject.UserId, "Page_Init"))
                {
                    base.RedirectToLogout();
                }
            }

            TimeOutOwinHelper.TryToSlideTimeoutForward(HttpContext.Current.GetOwinContext());
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ConfigurationSetupUtil.GetCurrentUrl();
            var authResult = HttpContext.Current.GetOwinContext().Authentication.AuthenticateAsync("Cookies").Result;
            if (authResult?.Identity != null)
            {
                LoginHelper loginHelper = new LoginHelper(_useLoadBalancer, UserManager, CompanyManager, LoginManager, SysLogManager, ClaimsHelper);
                int licenseId = 0;
                var loggedInWithEvo = false;
                SoeLoginState loginState;
                string trackId = Guid.NewGuid().ToString();

                if (authResult.Identity.FindFirst(SoeClaimType.Evo) != null && authResult.Identity.FindFirst(SoeClaimType.ChoosenLicenseId) != null)
                {
                    try
                    {
                        if (TryToLoginFromEvo(authResult.Identity, loginHelper, authResult, trackId, out loginState))
                        {
                            loggedInWithEvo = true;
                        }
                        else
                        {
                            LogCollector.LogError($"Failed to login user {loginHelper.User?.idLoginGuid} from Evo trackId {trackId} {loginState}");
                            RedirectToSoftOneIdUnauthorized(loginState: loginState);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        SysLogConnector.SaveErrorMessage($"Failed to remove license from Cookie. {ex.Message}");
                    }
                }

                if (!loggedInWithEvo)
                {
                    try
                    {
                        var licenseFromClaims = LoginHelper.GetLicenseIdFromClaims();

                        if (licenseFromClaims.HasValue)
                            licenseId = licenseFromClaims.Value;

                        if (licenseId == 0)
                            licenseId = LoginHelper.GetLicenseIdFromCookie(GetCookie(cookieSoelicenseid));

                        RemoveFromSessionAndCookie(cookieSoelicenseid);
                        try
                        {
                            if (licenseId == 0 && Request.QueryString.ToString().Contains(cookieSoelicenseid))
                            {
                                int.TryParse(Request.QueryString[cookieSoelicenseid], out licenseId);
                            }
                        }
                        catch (Exception ex)
                        {
                            SysLogConnector.SaveErrorMessage("Failed to check for licenseid in url" + ex.Message);
                        }
                    }
                    catch
                    {
                        SysLogConnector.SaveErrorMessage("Failed to remove license from Cookie");
                    }

                    if (!loginHelper.TryAuthenticateUser(authResult.Identity, licenseId, out _))
                    {
                        RedirectDueToFailedTokenRetrieval();
                        return;
                    }
                    else
                    {
                        var actorCompanyIdFromClaims = authResult.Identity.FindFirst(SoeClaimType.ActorCompanyId);
                        var userIdFromClaims = authResult.Identity.FindFirst(SoeClaimType.UserId);
                        var evo = authResult.Identity.FindFirst(SoeClaimType.Evo);

                        if ((actorCompanyIdFromClaims != null || userIdFromClaims != null) && !base.IsUserLoggedIn)
                        {
                            var actorCompanyIdValue = actorCompanyIdFromClaims?.Value?.ToString() ?? "no value";
                            var userIdValue = userIdFromClaims?.Value?.ToString() ?? "no value";

                            if (evo == null)
                            {
                                SysLogConnector.SaveInfoMessage($"Login.aspx Not evo Not logged in but has claims companyid [{actorCompanyIdFromClaims}] userid [{userIdValue}] machine [{Environment.MachineName}]");
                                // this means that we have claims but we are not logged into the system.
                                base.RedirectToLogout();
                                return;
                            }
                            else
                            {
                                SysLogConnector.SaveInfoMessage($"Login.aspx evo is true but not logged in. Has claims companyid [{actorCompanyIdFromClaims}] userid [{userIdValue}] machine [{Environment.MachineName}]");
                            }
                        }

                        if (HasIncoherentLogin(authResult, actorCompanyIdFromClaims, userIdFromClaims, loginHelper.ParameterClaims.ActorCompanyId, loginHelper.ParameterClaims.UserId, "Page_Load"))
                        {
                            var isEvo = evo?.Value != null ? evo.Value : "False";
                            SysLogConnector.SaveErrorMessage("!loginHelper.IsAuthorizedLogin Evo " + isEvo);

                            base.RedirectToLogout();
                            return;
                        }

                        if (!loginHelper.IsAuthorizedLogin)
                        {
                            SysLogConnector.SaveErrorMessage("!loginHelper.IsAuthorizedLogin");
                            RedirectToSoftOneIdUnauthorized(loginState: SoeLoginState.LoginInvalidOrExceededTimeout);
                            return;
                        }
                    }

                    if (loginHelper.ShouldRedirectForLoadBalancing(out string redirectUrl))
                    {
                        try
                        {
                            if (licenseId == 0 && Request.QueryString.ToString().Contains(cookieSoelicenseid))
                            {
                                int.TryParse(Request.QueryString[cookieSoelicenseid], out licenseId);
                            }

                            if (licenseId != 0)
                                redirectUrl = AddUrlParameter(redirectUrl, cookieSoelicenseid, licenseId.ToString());
                        }
                        catch (Exception ex)
                        {
                            SysLogConnector.SaveErrorMessage("Failed to redirect for load balancing: " + ex.Message);
                        }


                        Response.Redirect(redirectUrl, false);
                        return;
                    }
                }

                if (!TryLoginUser(loginHelper.User, out loginState))
                {
                    var info =
                        $"TryLoginUser state: {(int)loginState} " +
                        $"SoeLicense is null: {base.SoeLicense == null} " +
                        $"Soelicense.LicenseNr: {SoeLicense?.LicenseNr} " +
                        $"SoeUser is null: {SoeUser == null} " +
                        $"SoeUser.LicenseNr: {SoeUser?.LicenseNr} " +
                        $"SoeUser.LicenseId: {SoeUser?.LicenseId} " +
                        $"ActorCompanyId: {SoeCompany?.ActorCompanyId}";

                    SysLogConnector.SaveErrorMessage(info);

                    if (loggedInWithEvo)
                        LogCollector.LogError($"Failed to login user from Evo trackId {trackId} {info}");

                    RedirectToSoftOneIdUnauthorized(loginState: loginState);
                    return;
                }

                if (!TrySetUserLoginSession(loginHelper.User, loginHelper.Company, out loginState).Success)
                {
                    SysLogConnector.SaveErrorMessage("TrySetUserLoginSession state: " + (int)loginState);
                    RedirectToSoftOneIdUnauthorized(loginState: loginState);
                    return;
                }

                SetAccountYearInSession(loginHelper.User, loginHelper.Company);

                #region Login and redirect

                var requestedReturnUrl = this.Request.QueryString["returnUrl"] ?? "/";
                var returnUrl = loginHelper.GetReturnUrl(requestedReturnUrl);
                var identity = loginHelper.GetIdentity();
                CreateLegacyLogin(identity, redirect: returnUrl);

                Response.Redirect(returnUrl, false);

                #endregion
            }
            else if (Request.QueryString.ToString().Contains(cookieSoelicenseid))
            {
                var soelicense = Request.QueryString[cookieSoelicenseid];
                int.TryParse(soelicense, out var id);
                if (id != 0)
                    AddToSessionAndCookie(cookieSoelicenseid, LoginHelper.GetLicenseIdCookieKey(id), useSession: false);
            }
        }

        private bool TryToLoginFromEvo(ClaimsIdentity identity, LoginHelper loginHelper, AuthenticateResult authResult, string trackId, out SoeLoginState loginState)
        {
            var licenseGuid = LoginHelper.GetLicenseGuidFromClaims();

            if (licenseGuid == null || licenseGuid == Guid.Empty)
            {
                LogCollector.LogInfo($"TryToLoginFromEvo: Failed to get licenseGuid from claims trackId {trackId}");
                loginState = SoeLoginState.SoftOneOnlineError;
                return false;
            }

            var license = LicenseManager.GetLicenseByGuid(licenseGuid.Value);
            if (license == null)
            {
                LogCollector.LogInfo($"TryToLoginFromEvo: Failed to get license from licenseGuid {licenseGuid} trackId {trackId}");
                loginState = SoeLoginState.LicenseTerminated;
                return false;
            }

            if (!loginHelper.TryAuthenticateUser(authResult.Identity, license.LicenseId, out string errorMessage))
            {
                if (!string.IsNullOrEmpty(errorMessage))
                    LogCollector.LogError($"TryToLoginFromEvo: Failed to authenticate user with licenseId {license.LicenseId} trackId {trackId} {errorMessage}");
                loginState = SoeLoginState.BadLogin;
                RedirectDueToFailedTokenRetrieval();
                return false;
            }

            var actorCompanyIdFromClaims = authResult.Identity.FindFirst(SoeClaimType.ActorCompanyId);
            var userIdFromClaims = authResult.Identity.FindFirst(SoeClaimType.UserId);

            if ((actorCompanyIdFromClaims != null || userIdFromClaims != null))
            {
                if (base.IsUserLoggedIn)
                {
                    if (HasIncoherentLogin(authResult, actorCompanyIdFromClaims, userIdFromClaims, loginHelper.ParameterClaims.ActorCompanyId, loginHelper.ParameterClaims.UserId, "Page_Load"))
                    {
                        LogCollector.LogInfo($"TryToLoginFromEvo: Logged in but incoherentLogin has claims companyid [{actorCompanyIdFromClaims}] userid [{userIdFromClaims}] machine [{Environment.MachineName}] trackId {trackId}");

                        loginState = SoeLoginState.BadLogin;
                        base.RedirectToLogout();
                        return false;
                    }
                }
                else
                {
                    var actorCompanyIdValue = actorCompanyIdFromClaims?.Value?.ToString() ?? "no value";
                    var userIdValue = userIdFromClaims?.Value?.ToString() ?? "no value";
                    LogCollector.LogInfo($"TryToLoginFromEvo: Login.aspx is Evo but not logged in but has claims companyid [{actorCompanyIdFromClaims}] userid [{userIdValue}] machine [{Environment.MachineName}] trackId {trackId}");
                }
            }

            if (HasIncoherentLogin(authResult, actorCompanyIdFromClaims, userIdFromClaims, loginHelper.ParameterClaims.ActorCompanyId, loginHelper.ParameterClaims.UserId, "Page_Load"))
            {
                LogCollector.LogInfo($"TryToLoginFromEvo: HasIncoherentLogin with loginHelper.IsAuthorizedLogin {loginHelper.IsAuthorizedLogin} Evo has claims companyid [{actorCompanyIdFromClaims}] userid [{userIdFromClaims}] machine [{Environment.MachineName}] trackId {trackId} ");
                loginState = SoeLoginState.BadLogin;
                base.RedirectToLogout();
                return false;
            }

            if (!loginHelper.IsAuthorizedLogin)
            {
                SysLogConnector.SaveErrorMessage("!loginHelper.IsAuthorizedLogin");
                loginState = SoeLoginState.LoginInvalidOrExceededTimeout;
                RedirectToSoftOneIdUnauthorized(loginState: SoeLoginState.LoginInvalidOrExceededTimeout);
                return false;
            }

            SessionCache.ReloadUser(base.UserId, base.SoeActorCompanyId ?? 0);
            loginState = SoeLoginState.OK;
            return true;
        }
        private void RedirectDueToFailedTokenRetrieval()
        {
            if (!ShouldRetry())
            {
                RedirectToSoftOneIdUnauthorized(loginState: SoeLoginState.SoftOneOnlineError);
            }
            else
            {
                this.Response.Redirect("/login.aspx", true);
            }
        }

        private bool HasIncoherentLogin(AuthenticateResult authResult, Claim actorCompanyIdFromClaims, Claim userIdFromClaims, int actorCompanyIdFromParam, int userIdFromParam, string source)
        {
            if (actorCompanyIdFromClaims != null || userIdFromClaims != null)
            {
                var message = string.Empty;

                if (actorCompanyIdFromClaims != null && actorCompanyIdFromClaims.Value != actorCompanyIdFromParam.ToString())
                    message += $"ActorCompanyId from claims does not match the one in the parameter claims. {actorCompanyIdFromClaims.Value} vs {actorCompanyIdFromParam}. ";

                if (userIdFromClaims != null && userIdFromClaims.Value != userIdFromParam.ToString())
                    message += $"UserId from claims does not match the one in the parameter claims. {userIdFromClaims.Value} vs {userIdFromParam}. ";

                if (!string.IsNullOrEmpty(message))
                {
                    message += $"Source: {source}";
                    SysLogConnector.SaveErrorMessage(message);

                    // Remove claims from the identity then redirect back to this page
                    var claims = authResult.Identity.Claims.ToList();
                    foreach (var claim in claims)
                    {
                        if (claim.Type.StartsWith("urn:soe"))
                            authResult.Identity.RemoveClaim(claim);
                    }
                    // Clear response buffer to prevent the error
                    HttpContext.Current.Response.Clear();
                    HttpContext.Current.Response.SuppressContent = true;

                    return true;
                }
                else
                    return false;
            }
            return false;
        }
        private bool ShouldRetry()
        {
            int count;
            int.TryParse(GetCookie(cookieKey), out count);

            if (count > 0)
            {
                count--;
                AddToSessionAndCookie(cookieKey, count, useSession: false);
                HttpCookie cookie = Request.Cookies[cookieKey];
                if (cookie != null)
                    cookie.Expires = DateTime.Now.AddMilliseconds(30000);
                return false;
            }
            else
            {
                return true;
            }
        }
        private bool TryLoginUser(User user, out SoeLoginState loginState)
        {
            LoginManager.BlockedFromDateValidation(user);

            loginState = LicenseCacheManager.Instance.LoginUser(user, user.License.LicenseNr, true, false, false, GetUserEnvironmentInfo(), out _);
            return loginState == SoeLoginState.OK;
        }
        private ActionResult TrySetUserLoginSession(User user, Company company, out SoeLoginState loginState)
        {
            var result = UserManager.LoginUserSession(user.UserId, user.LoginName, company.ActorCompanyId, company.Name, softOneIdLogin: true);
            if (result.Success)
            {
                if (!result.StringValue.IsNullOrEmpty() && int.TryParse(result.StringValue, out int userSessionId))
                {
                    using (CompEntities entities = new CompEntities())
                    {
                        var userSession = entities.UserSession.FirstOrDefault(f => f.UserSessonId == userSessionId);

                        if (userSession != null && System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Request != null)
                        {
                            var bc = System.Web.HttpContext.Current.Request.Browser;
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

                AddToSessionAndCookie(Constants.COOKIE_USERSESSIONID, result.StringValue);
                loginState = SoeLoginState.OK;
            }
            else
            {
                RemoveFromSessionAndCookie(Constants.COOKIE_USERSESSIONID);
                loginState = (SoeLoginState)result.IntegerValue;
            }
            return result;
        }

        private static string GetUserEnvironmentInfo()
        {
            return GetHostInfo() + Constants.SOE_ENVIRONMENT_CONFIGURATION_SEPARATOR + GetClientIP();
        }
        private static string GetHostIP()
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
        public static string GetHostName()
        {
            return Dns.GetHostName();
        }

        public static string GetHostInfo()
        {
            return $"{GetHostIP()}_{GetHostName()}";
        }

        protected static string GetClientIP()
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

        private void SetAccountYearInSession(User user, Company company)
        {
            int accountYearId = SettingManager.GetIntSetting(SettingMainType.UserAndCompany, (int)UserSettingType.AccountingAccountYear, user.UserId, company.ActorCompanyId, 0);
            if (accountYearId > 0)
                CurrentAccountYear = AccountManager.GetAccountYear(accountYearId);
        }
    }

    public static class JwtSecurityTokenExtensions
    {
        public static Claim Get(this IEnumerable<Claim> claims, string type)
        {
            return claims.FirstOrDefault(x => x.Type == type);
        }
    }
}