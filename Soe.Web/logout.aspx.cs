using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Business.Evo.Connectors.Cache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Security;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Web.Security;
using System;
using System.Web;
namespace SoftOne.Soe.Web
{
    public partial class logout : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string idTokenHint = GetIdTokenHint(); // Retrieve the id_token

            var redirectUrl = LogoutUser(idTokenHint); // Pass it to LogoutUser

            ClearDataFromSession(); // Clear local session and cookies AFTER getting the id_token

            if (redirectUrl != null)
                Response.Redirect(redirectUrl);

            if (IsLegacyLogin)
            {
                if (Int32.TryParse(QS["timeout"], out int timeout) && timeout == 1)
                {
                    Context.GetOwinContext().Authentication.SignOut("Cookies");

                    try
                    {
                        redirectUrl = LogoutUser(null);
                        if (redirectUrl != null)
                            Response.Redirect(redirectUrl);
                    }
                    catch
                    {
                        // Intentionally ignored, safe to continue
                        // NOSONAR
                    }
                    RedirectToLoginByTimeout();
                }
                else
                    RedirectToLogin();
            }
        }

        private string GetIdTokenHint()
        {
            var userGuid = ClaimsHelper.GetGuidClaim(Context, SoeClaimType.UserGuid);
            if (userGuid.HasValue && userGuid != Guid.Empty)
                return EvoDistributionCacheConnector.GetCachedValue<string>($"IdToken{userGuid}");

            return null;
        }

        private string LogoutUser(string idTokenHint)
        {
            UserDTO user = SoeSupportUserId.HasValue && SoeSupportUserId.Value > 0 ? UserManager.GetUser(SoeSupportUserId.Value, loadLicense: true).ToDTO() : SoeUser;
            if (user == null && base.SoeUserId.HasValue)
            {
                user = UserManager.GetUser(SoeUserId.Value, loadLicense: true).ToDTO();
            }
            if (user == null)
            {
                var userGuid = LoginHelper.GetIdLoginGuid();
                if (userGuid.HasValue)
                    user = UserManager.GetUser(userGuid.Value, includeLicense: true).ToDTO();
            }
            if (user == null)
            {
                var userId = LoginHelper.GetUserId();
                if (userId.HasValue)
                    user = UserManager.GetUser(userId.Value, loadLicense: true).ToDTO();
            }

            LoginManager.Logout(user);

            var userSessionId = GetSessionAndCookie(Constants.COOKIE_USERSESSIONID);
            if (userSessionId != null && Int32.TryParse(userSessionId, out int sessionId))
                UserManager.LogoutUserSession(user, userSessionId: sessionId);

            var idServerLogoutUrl = SoftOneIdConnector.GetUri().RemoveTrailingSlash() + "/connect/endsession";
            var postLogoutRedirectUri = SoftOneIdConnector.GetUri().EnsureTrailingSlash() + "/account/logout";

            var builder = new UriBuilder(idServerLogoutUrl);
            var query = HttpUtility.ParseQueryString(builder.Query);
            if (!string.IsNullOrEmpty(idTokenHint))
            {
                query["id_token_hint"] = idTokenHint;
            }
            query["post_logout_redirect_uri"] = postLogoutRedirectUri;

            builder.Query = query.ToString();
            var redirectUrl = builder.Uri.ToString();

            // Sign out of the local authentication cookie
            Context.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return redirectUrl; // Return the redirect URL for the initial Response.Redirect in Page_Load
        }
    }
}
