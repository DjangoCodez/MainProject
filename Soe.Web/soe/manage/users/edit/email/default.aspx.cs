using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.soe.manage.users.edit.email
{
    public partial class _default : PageBase
    {
        #region Variables

        private UserManager um;

        protected User user;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.None;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

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

            if (Validator.ValidateEmail(user.Email) && HasUserVerifiedEmail())
            {
                RedirectToHome();
                return;
            }

            #endregion

            #region Populate

            InstructionList.HeaderText = GetText(11673, "Ange din epost-adress");
            InstructionList.Instructions = new List<string>()
            {
                GetText(11665, "Vi kommer att inom kort förändra sättet att logga in i SoftOne."),
                GetText(11666, "För att göra denna övergång så smidig som möjligt skulle vi vilja att du fyller i din epost-adress."),
                GetText(11667, "Detta är viktigt eftersom du som användare kommer att bli ansvarig för din egen inloggningsinformation."),
                GetText(11668, "Exempelvis kommer epost-adressen att behövas för att återställa lösenord. Du kan inte fortsätta inloggningen utan att ange epost nedan."),
            };

            if (!String.IsNullOrEmpty(user.Email))
                NewEmail.Value = user.Email;

            #endregion

            #region Set data

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
                    Form1.MessageSuccess = GetText(11685, "Epost uppdaterad");
                else if (MessageFromSelf == "NOTUPDATED")
                    Form1.MessageError = GetText(11671, "Epost kunde inte uppdateras");
                else if (MessageFromSelf == "EMAIL_NOT_CORRECT")
                    Form1.MessageWarning = GetText(11672, "Angiven epost är inte korrekt");
                else if (MessageFromSelf == "EMAIL_DONT_MATCH")
                    Form1.MessageWarning = GetText(11669, "Epost matchar inte");
            }

            #endregion
        }

        #region Action-methods

        protected override void Save()
        {
            string newEmail = F["NewEmail"];
            string confirmEmail = F["ConfirmEmail"];

            //Password policy
            if (!Validator.ValidateEmail(newEmail))
                RedirectToSelf("EMAIL_NOT_CORRECT", true);
            if (newEmail != confirmEmail)
                RedirectToSelf("EMAIL_DONT_MATCH", true);

            //Update password
            user.Email = newEmail;
            if (um.UpdateUser(user).Success)
            {
                SettingManager sm = new SettingManager(ParameterObject);
                UserCompanySettingObject setting = new UserCompanySettingObject()
                {
                    DataTypeId = (int)SettingDataType.Boolean,
                    BoolSetting = true,
                };
                sm.UpdateInsertSetting(SettingMainType.User, (int)UserSettingType.CoreHasUserVerifiedEmail, setting, user.UserId, 0, 0);

                ParameterObject.SetSoeUser(um.GetSoeUser(SoeCompany.ActorCompanyId, user));
                ParameterObject.SoeUser.HasUserVerifiedEmail = true;
                RedirectToHome();
            }
            else
                RedirectToSelf("NOTUPDATED", true);
        }

        #endregion
    }
}
