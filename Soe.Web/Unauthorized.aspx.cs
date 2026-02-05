using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.SoftOneId;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using SoftOne.Soe.Web.Security;
using System;
using System.Web;

namespace SoftOne.Soe.Web
{
    public partial class unauthorized : PageBase
    {
        protected SoeLoginState soeLoginState = SoeLoginState.Unknown;
        protected string unauthorizedMessage;
        protected string loginUrl;
        private LoginManager lm = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            this.lm = new LoginManager(ParameterObject);

            if (Int32.TryParse(QS["loginState"], out int loginState))
            {
                this.soeLoginState = (SoeLoginState)loginState;
                this.unauthorizedMessage = lm.GetLoginErrorMessage(this.soeLoginState);
            }
            this.loginUrl = SoftOneIdConnector.GetUri().EnsureTrailingSlash().ToString();
            LogoutUser();
        }

        private string LogoutUser()
        {
            UserDTO user = null;
            if (SoeSupportUserId.HasValue && SoeSupportUserId.Value > 0)
            {
                user = UserManager.GetUser(SoeSupportUserId.Value, loadLicense: true).ToDTO();
            }
            else
                user = SoeUser;

            if (user == null && base.SoeUserId.HasValue)
                user = UserManager.GetUser(SoeUserId.Value, loadLicense: true).ToDTO();

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
            if (userSessionId != null)
            {
                int sessionId;
                if (Int32.TryParse(userSessionId, out sessionId))
                {
                    UserManager.LogoutUserSession(user, userSessionId: sessionId);
                }
            }

            ClearDataFromSession();
            Context.GetOwinContext().Authentication.SignOut("Cookies");

            #region Redirect
            var redirectUrl = SoftOneIdConnector.GetUri().RemoveTrailingSlash() + "/account/logout";
            #endregion

            return redirectUrl;
        }
    }
}
