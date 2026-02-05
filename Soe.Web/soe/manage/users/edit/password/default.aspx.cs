using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.IO;

namespace SoftOne.Soe.Web.soe.manage.users.edit.password
{
    public partial class _default : PageBase
    {
        #region Variables

        private LoginManager lm;
        private UserManager um;

        protected User user;

        private bool? oldPasswordNeeded;
        private bool OldPasswordNeeded
        {
            get
            {
                if (oldPasswordNeeded.HasValue)
                    return oldPasswordNeeded.Value;

                #region Need password

                if (user == null)
                    return true;

                //Rule 1: Must have old password when changing same User
                if (UserId == user.UserId)
                {
                    oldPasswordNeeded = true;
                    return oldPasswordNeeded.Value;
                }

                #endregion

                #region Dont need password

                //Rule 2: Administrators on SupportLicense dont need old password
                bool supportAdmin = SoeLicense.Support && SoeUser.IsAdmin;
                if (supportAdmin)
                {
                    oldPasswordNeeded = false;
                    return oldPasswordNeeded.Value;
                }

                //Rule 3: Administrators on current Company dont need old password when changing User in same Company
                bool companyAdmin = um.IsUserConnectedToCompany(user.UserId, SoeCompany.ActorCompanyId) && um.IsUserAdminInCompany(SoeUser, SoeCompany.ActorCompanyId);
                if (companyAdmin)
                {
                    oldPasswordNeeded = false;
                    return oldPasswordNeeded.Value;
                }

                #endregion

                //Default
                if (!oldPasswordNeeded.HasValue)
                    oldPasswordNeeded = true;

                return oldPasswordNeeded.Value;
            }
        }

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Users_Edit_Password;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            lm = new LoginManager(ParameterObject);
            um = new UserManager(ParameterObject);

            //Mandatory parameters
            int userId;
            if (Int32.TryParse(QS["user"], out userId))
            {
                user = um.GetUser(userId, loadUserCompanyRole: true, loadLicense: true, loadEmployee: true);
                if (user == null)
                    throw new SoeEntityNotFoundException("User", this.ToString());
            }
            else
            {
                throw new SoeQuerystringException("user", this.ToString());
            }

            //Mode 
            PreOptionalParameterCheck(Request.Url.AbsolutePath + "?user=" + userId, Request.Url.PathAndQuery);

            //Optional parameters

            //Mode
            PostOptionalParameterCheck(Form1, user, true);

            Form1.Title = user != null ? user.LoginName : "";

            #endregion

            #region Populate

            PasswordPolicy.Populate();

            #endregion

            #region Set data

            OldPassword.Visible = OldPasswordNeeded;

            ChangePasswordInstruction.Visible = SoeUser.ChangePassword;

            #endregion

            #region Actions

            if (Form1.IsPosted)
            {
                Save();
            }

            #endregion

            #region MessageFromSelf

            if (!String.IsNullOrEmpty(MessageFromSelf))
            {
                if (MessageFromSelf == "UPDATED")
                    Form1.MessageSuccess = GetText(1581, "Lösenord uppdaterat");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(1582, "Lösenord kunde inte uppdateras");
                else if (MessageFromSelf == "PASSWORD_NOT_CORRECT")
                    Form1.MessageWarning = GetText(1580, "Angivet lösenord är inte korrekt");
                else if (MessageFromSelf == "PASSWORD_DONT_MATCH")
                    Form1.MessageWarning = GetText(1578, "Lösenorden matchar inte");
                else if (MessageFromSelf == "PASSWORD_SAME")
                    Form1.MessageError = GetText(5310, "Nytt och gammalt lösenord är lika");
                else if (MessageFromSelf == "PASSWORD_NOT_STRONG")
                    Form1.MessageWarning = GetText(1579, "Lösenordet uppnår inte lösenordspolicyn");
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            string oldPassword = F["OldPassword"];
            string newPassword = F["NewPassword"];
            string confirmPassword = F["ConfirmPassword"];

            //Check old password
            if (OldPasswordNeeded)
            {
                byte[] oldPasswordhash = lm.GetPasswordHash(user.LoginName, oldPassword);
                User currentUser = um.GetUser(user.License.LicenseNr, user.LoginName, oldPasswordhash, false);
                if (currentUser == null)
                    RedirectToSelf("PASSWORD_NOT_CORRECT", true);
            }

            //Password policy
            if (newPassword != confirmPassword)
                RedirectToSelf("PASSWORD_DONT_MATCH", true);
            if (newPassword == oldPassword)
                RedirectToSelf("PASSWORD_SAME", true);
            if (!um.IsPasswordStrong(newPassword))
                RedirectToSelf("PASSWORD_NOT_STRONG", true);

            //Update password
            user.passwordhash = lm.GetPasswordHash(user.LoginName, Path.GetRandomFileName().Replace(".", ""));
            user.ChangePassword = false;
            if (um.UpdateUser(user).Success)
            {
                if (SoeUser.ChangePassword)
                {
                    ParameterObject.SetSoeUser(um.GetSoeUser(SoeCompany.ActorCompanyId, user));
                    RedirectToHome();
                }
                else
                    RedirectToSelf("UPDATED");
            }
            else
                RedirectToSelf("NOTUPDATED", true);
        }

        #endregion
    }
}
