using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;

namespace SoftOne.Soe.Web.soe.manage.users.edit.sessions
{
    public partial class _default : PageBase
    {
        #region Variables

        protected UserManager um;
		
        protected User user;
		protected int userId;

        #endregion

        public bool IsAuthorized
        {
            get
            {
                if (user == null)
                    return false;

                //Rule 1: Same User
                if (UserId == user.UserId)
                    return true;

                //Rule 2: Administrators on SupportLicense
                if (SoeLicense.Support && SoeUser.IsAdmin)
                    return true;

                //Rule 3: Administrators on Company and User connected to Company
                if (um.IsUserAdminInCompany(SoeUser, SoeCompany.ActorCompanyId) && um.IsUserConnectedToCompany(user.UserId, SoeCompany.ActorCompanyId))
                    return true;

                return false;
            }
        }

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Users_Edit_Sessions;
            base.Page_Init(sender, e);
        }

		protected void Page_Load(object sender, EventArgs e)
		{
            um = new UserManager(ParameterObject);

			//Mandatory parameters
            if (Int32.TryParse(QS["user"], out userId))
            {
                user = um.GetUser(userId);
                if(user == null)
                    throw new SoeEntityNotFoundException("User", this.ToString());
            }
            else
                throw new SoeQuerystringException("user", this.ToString());

            SoeGrid1.Title = GetText(1691, "Inloggningar") + " " + GetText(1604, "för") + " " + " " + user.LoginName;

            #region Authorization

            if (!IsAuthorized)
                RedirectToUnauthorized(UnauthorizationType.DataAuthorityMissing);

            #endregion

            SoeGrid1.DataSource = um.GetUserSessionIncludingHistory(userId);
			SoeGrid1.DataBind();
		}
	}
}
