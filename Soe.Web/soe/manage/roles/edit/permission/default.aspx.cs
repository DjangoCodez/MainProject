using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;

namespace SoftOne.Soe.Web.soe.manage.roles.edit.permission
{
    public partial class _default : PageBase
    {
        #region Variables

        private LicenseManager lm;
        private CompanyManager cm;
        private RoleManager rm;
        private UserManager um;

        private Role role;

        private int licenseId;
        private string licenseNr;
        private int actorCompanyId;

        public bool IsAuthorized
        {
            get
            {
                if (role == null)
                    return false;

                //Rule 1: Same Role or User has Role
                if (RoleId == role.RoleId || rm.HasUserGivenRole(UserId, role.RoleId))
                    return true;

                //Rule 2: Administrators on SupportLicense
                if (SoeLicense.Support && SoeUser.IsAdmin)
                    return true;

                //Rule 3: Administrators on Company
                if (um.IsUserAdminInCompany(SoeUser, actorCompanyId))
                    return true;

                return false;
            }
        }

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Roles_Edit_Permission;
            base.Page_Init(sender, e);
        }

		protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            lm = new LicenseManager(ParameterObject);
            cm = new CompanyManager(ParameterObject);
            rm = new RoleManager(ParameterObject);
            um = new UserManager(ParameterObject);

            //Mandatory parameters
            if (!Int32.TryParse(QS["company"], out actorCompanyId))
                throw new SoeQuerystringException("company", this.ToString());

			int roleId;
            if (Int32.TryParse(QS["role"], out roleId))
            {
                role = rm.GetRole(roleId, true, true);
                if (role == null)
                    throw new SoeEntityNotFoundException("Role", this.ToString());
            }
            else
                throw new SoeQuerystringException("role", this.ToString());

			int sysPermissionId;
            if (!Int32.TryParse(QS["permission"], out sysPermissionId))
                throw new SoeQuerystringException("permission", this.ToString());

            //Optional parameters
            Int32.TryParse(QS["license"], out licenseId);
            if (licenseId == 0)
                licenseId = SoeLicense.LicenseId;

            licenseNr = QS["licenseNr"];
            if (String.IsNullOrEmpty(licenseNr))
                licenseNr = SoeLicense.LicenseNr;

            #endregion

            #region Authorization

            if (!IsAuthorized)
                RedirectToUnauthorized(UnauthorizationType.DataAuthorityMissing);

            #endregion

            #region Populate

            //Set properties
            FeaturePermissionTree.CurrentFeature = this.Feature;
			FeaturePermissionTree.FeatureType = SoeFeatureType.Role;
			FeaturePermissionTree.ActorCompanyId = cm.GetCompanyByRoleId(roleId).ActorCompanyId;
			FeaturePermissionTree.LicenseId = lm.GetLicenseByCompany(FeaturePermissionTree.ActorCompanyId).LicenseId;
			FeaturePermissionTree.RoleId = roleId;
            FeaturePermissionTree.SubTitle = GetText(1604, "för") + " " + GetText(1608, "roll") + " " + rm.GetRoleNameText(role);
			FeaturePermissionTree.Permission = (Permission) sysPermissionId;

            #endregion

            #region Naviation

			if (sysPermissionId == (int)Permission.Readonly)
			{
                FeaturePermissionTree.AddLink(GetText(1080, "Skrivbehörighet"), GetBaseQS() + "&permission=" + (int)Permission.Modify,
					Feature.Manage_Roles_Edit_Permission, Permission.Readonly);
			}
			else if (sysPermissionId == (int)Permission.Modify)
			{
                FeaturePermissionTree.AddLink(GetText(1077, "Läsbehörighet"), GetBaseQS() + "&permission=" + (int)Permission.Readonly,
					Feature.Manage_Roles_Edit_Permission, Permission.Readonly);
            }

            #endregion
        }

        #region Help-methods

        private string GetBaseQS(string prefix = "?")
        {
            if (role == null)
                return String.Empty;

            return String.Format("{0}license={1}&licenseNr={2}&company={3}&role={4}", prefix, licenseId, licenseNr, actorCompanyId, role.RoleId);
        }

        #endregion
	}
}
