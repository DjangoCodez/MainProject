using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using SoftOne.Soe.Util.Exceptions;
using SoftOne.Soe.Data;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Web.Util;

namespace SoftOne.Soe.Web.soe.manage.system.admin.licenses.users
{
    public partial class _default : PageBase
    {
        private LicenseManager lm;
        private UserManager um;
        protected License license;
        protected LicenseObject licenseObject;
        protected int licenseId;
        private int userId;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_System;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            lm = new LicenseManager(ParameterObject);
            um = new UserManager(ParameterObject);

            //Mandatory parameters
            if (Int32.TryParse(QS["license"], out licenseId))
            {
                license = lm.GetLicense(licenseId);
                if (license != null)
                    licenseObject = LicenseCacheManager.Instance.GetLicenseObject(license.LicenseNr);
                else
                    throw new SoeEntityNotFoundException("License", this.ToString());
            }
            else
                throw new SoeQuerystringException("license", this.ToString());

            SoeGrid1.Title = GetText(5148, "Inloggade användare") + " " + GetText(1604, "för") + " " + GetText(1605, "licens") + " " + license.Name;

            #endregion

            #region Actions

            int logout;
            if (Int32.TryParse(QS["logout"], out logout) && logout == 1)
            {
                if (Int32.TryParse(QS["user"], out userId) && userId > 0)
                {
                    bool loggedOut = LogoutUser();
                    Response.Redirect(Request.Url.AbsolutePath + "?license=" + licenseId);
                }
            }

            #endregion

            List<User> users  = new List<User>();
            if(licenseObject != null)
                users = licenseObject.GetLoggedInUsers();

            SoeGrid1.DataSource = users;
            SoeGrid1.DataBind();
        }

        #region Action-methods

        private bool LogoutUser()
        {
            User user = um.GetUser(userId, loadLicense: true);
            if (user != null)
            {
                LoginManager lm = new LoginManager(ParameterObject);
                if (lm.Logout(user.ToDTO()))
                {
                    #region UserSession

                    if (EnableUserSession)
                        um.LogoutUserSession(user.ToDTO(), supportUserId: UserId);

                    #endregion

                    #region Clear cache

                    #endregion
                }
            }
            return false;
        }

        #endregion
    }
}
