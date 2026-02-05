using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;

namespace SoftOne.Soe.Web.soe.manage.companies.edit
{
    public partial class _default : PageBase
    {
        #region Variables

        private CompanyManager cm;
        private UserManager um;

        private Company company;

        protected int licenseId;
        protected string licenseNr;
        protected bool licenseSupport;
        protected int actorCompanyId;
        protected int selectedCompanyId;
        protected bool isAuthorizedForEdit;
        protected bool isUserInCompany;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Companies_Edit;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Mandatory parameters
            cm = new CompanyManager(ParameterObject);
            um = new UserManager(ParameterObject);

            //Optional parameters
            Int32.TryParse(QS["company"], out selectedCompanyId);

            Int32.TryParse(QS["c"], out actorCompanyId);
            if (actorCompanyId == 0)
                actorCompanyId = SoeCompany.ActorCompanyId;

            Int32.TryParse(QS["license"], out licenseId);
            if (licenseId == 0)
                licenseId = SoeLicense.LicenseId;

            licenseNr = QS["licenseNr"];
            if (String.IsNullOrEmpty(licenseNr))
                throw new SoeQuerystringException("licenseNr", this.ToString());

            company = cm.GetCompany(selectedCompanyId);

            isAuthorizedForEdit = company != null ? (SoeLicense.Support && SoeUser.IsAdmin) || (um.IsUserAdminInCompany(SoeUser, company.ActorCompanyId)) : this.HasRolePermission(Feature.Manage_Companies_Edit, Permission.Modify);
            licenseSupport = SoeLicense.Support;

            isUserInCompany = true;
            if (company != null)
            {
                bool userInCompany = cm.UserInCompany(UserId, licenseId, company.ActorCompanyId);
                if (!userInCompany && !SoeCompany.LicenseSupport)
                    isUserInCompany = false;
            }
        }
    }
}

