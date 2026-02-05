using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;

namespace SoftOne.Soe.Web.soe.manage.users
{
    public partial class _default : PageBase
    {
        #region Variables

        private LicenseManager lm;
        private CompanyManager cm;
        private RoleManager rm;
        private UserManager um;
        private FeatureManager fm;

        protected Role role;
        protected Company company;
        protected License license;
        protected User user;
        protected int licenseId;
        protected int actorCompanyId;
        protected int roleId;
        protected int userId;
        protected bool isAdmin;
        protected bool hasValidLicenseToSupportLogin;
        protected bool simplifiedRegistration;
        protected string tabHeader;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Users;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            lm = new LicenseManager(ParameterObject);
            cm = new CompanyManager(ParameterObject);
            rm = new RoleManager(ParameterObject);
            um = new UserManager(ParameterObject);
            fm = new FeatureManager(ParameterObject);

            Dictionary<string, string> initParams = new Dictionary<string, string>();

            //Optional parameters
            //Overwrite Role/Company/License if requested
            if (Int32.TryParse(QS["license"], out licenseId))
            {
                this.license = lm.GetLicense(licenseId);
                if (!CanSeeUsersForLicense(license))
                    RedirectToUnauthorized(UnauthorizationType.DataAuthorityMissing);

                initParams.Add("selectedLicenseId", license.LicenseId.ToString());
            }
            if (Int32.TryParse(QS["company"], out actorCompanyId))
            {
                this.company = cm.GetCompany(actorCompanyId, true);
                if (!CanSeeUsersForCompany(company))
                    RedirectToUnauthorized(UnauthorizationType.DataAuthorityMissing);

                if (this.license == null)
                    this.license = company.License;
                initParams.Add("selectedCompanyId", company.ActorCompanyId.ToString());
            }
            if (Int32.TryParse(QS["role"], out roleId))
            {
                this.role = rm.GetRole(roleId, loadCompany: true, loadLicense: true);
                if (!CanSeeUsersForRole(role))
                    RedirectToUnauthorized(UnauthorizationType.DataAuthorityMissing);

                if (this.license == null && role.Company != null)
                    this.license = role.Company.License;
                initParams.Add("selectedRoleId", role.RoleId.ToString());
            }
            if (Int32.TryParse(QS["user"], out userId))
            {
                this.user = um.GetUser(userId, loadLicense: true);
                if (!CanSeeUser(user))
                    RedirectToUnauthorized(UnauthorizationType.DataAuthorityMissing);

                if (this.license == null)
                    this.license = user.License;
                initParams.Add("selectedUserId", userId.ToString());
            }

            //Current Company/License is default if no specific Role/Company/License requested
            if (licenseId == 0 && actorCompanyId == 0 && roleId == 0)
            {
                actorCompanyId = SoeCompany.ActorCompanyId;
                licenseId = SoeLicense.LicenseId;
            }

            this.isAdmin = base.IsLicenseAdmin;
            this.hasValidLicenseToSupportLogin = license != null && HasValidLicenseToSupportLogin(license.LicenseId, license.LicenseNr);
            this.simplifiedRegistration = fm.HasRolePermission(Feature.Manage_Users_SimplifiedRegistration, Permission.Readonly, ParameterObject.RoleId, ParameterObject.ActorCompanyId, ParameterObject.LicenseId);
            this.tabHeader = GetTabText();

            initParams.Add("hasValidLicenseToSupportLogin", hasValidLicenseToSupportLogin.ToString());
            initParams.Add("simplifiedRegistration", simplifiedRegistration.ToString());
            initParams.Add("title", this.tabHeader);

            #endregion            
        }

        private string GetTabText()
        {
            string tabText = GetText(1062, "Användare");
            if (user != null)
                tabText = $" {user.LoginName}";
            else if (role != null)
                tabText += $" {GetText(1604, "för")} {GetText(1608, "roll")} {rm.GetRoleNameText(role)}";
            else if (company != null)
                tabText += $" {GetText(1604, "för")} {GetText(1606, "företag")} {company.Name.Replace("'", "\\'")}";
            else if (license != null)
                tabText += $" {GetText(1604, "för")} {GetText(1605, "licens")} {license.Name}";
            return tabText;
        }
    }
}
