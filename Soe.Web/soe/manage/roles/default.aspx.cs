using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;

namespace SoftOne.Soe.Web.soe.manage.roles
{
    public partial class _default : PageBase
    {
        #region Variables

        private CompanyManager cm;
        private UserManager um;

        private Company company;

        protected int licenseId;
        protected string licenseNr;
        protected int selectedCompanyId;
        protected bool isAuthorizedForEdit;

        protected string tabHeader;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Roles;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            cm = new CompanyManager(ParameterObject);
            um = new UserManager(ParameterObject);

            //Optional parameters
            Int32.TryParse(QS["company"], out selectedCompanyId);
            if (selectedCompanyId == 0)
                selectedCompanyId = SoeCompany.ActorCompanyId;

            Int32.TryParse(QS["license"], out licenseId);
            if (licenseId == 0)
                licenseId = SoeLicense.LicenseId;

            licenseNr = QS["licenseNr"];
            if (String.IsNullOrEmpty(licenseNr))
                throw new SoeQuerystringException("licenseNr", this.ToString());

            company = cm.GetCompany(selectedCompanyId);

            isAuthorizedForEdit = (SoeLicense.Support && SoeUser.IsAdmin) || (company != null && um.IsUserAdminInCompany(SoeUser, company.ActorCompanyId));

            #endregion

            this.tabHeader = GetTabText();
        }

        private string GetTabText()
        {
            string tabText = GetText(2045, "Roller");
            if (company != null)
                tabText += $" {GetText(1604, "för")} {GetText(1606, "företag")} {company.Name.Replace("'", "\\'")}";

            return tabText;
        }
    }
}
