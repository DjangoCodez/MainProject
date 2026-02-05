using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;

namespace SoftOne.Soe.Web.soe.manage.companies.edit.permission
{
    public partial class _default : PageBase
    {
        #region Variables

        private LicenseManager lm;
        private CompanyManager cm;

        private Company company;

        protected int licenseId;
        protected string licenseNr;
        protected int actorCompanyId;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Companies_Edit_Permission;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
		{
            #region Init

            lm = new LicenseManager(ParameterObject);
            cm = new CompanyManager(ParameterObject);

            //Mandatory parameters
            if (Int32.TryParse(QS["company"], out actorCompanyId))
            {
                company = cm.GetCompany(actorCompanyId);
                if (company == null)
                    throw new SoeEntityNotFoundException("Company", this.ToString());
            }
            else
                throw new SoeQuerystringException("company", this.ToString());

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

            if (!base.IsAuthorizedInCompany(actorCompanyId))
                RedirectToUnauthorized(UnauthorizationType.DataAuthorityMissing);

            #endregion

            #region Populate

            //Set properties
            FeaturePermissionTree.CurrentFeature = this.Feature;
			FeaturePermissionTree.FeatureType = SoeFeatureType.Company;
			FeaturePermissionTree.LicenseId = lm.GetLicenseByCompany(actorCompanyId).LicenseId;
			FeaturePermissionTree.ActorCompanyId = actorCompanyId;
			FeaturePermissionTree.Permission = (Permission)sysPermissionId;
            FeaturePermissionTree.SubTitle = GetText(1604, "för") + " " + GetText(1606, "företag") + " " + company.Name;

            #endregion

            #region Navigation

			if (sysPermissionId == (int)Permission.Readonly)
			{
				FeaturePermissionTree.AddLink(GetText(1080, "Skrivbehörighet"), GetBaseQS() + "&permission=" + (int)Permission.Modify,
					Feature.Manage_Companies_Edit_Permission, Permission.Readonly);
			}
			else if (sysPermissionId == (int)Permission.Modify)
			{
                FeaturePermissionTree.AddLink(GetText(1077, "Läsbehörighet"), GetBaseQS() + "&permission=" + (int)Permission.Readonly,
					Feature.Manage_Companies_Edit_Permission, Permission.Readonly);
            }

            #endregion
        }

        #region Help-methods

        private string GetBaseQS(string prefix = "?")
        {
            if (company == null)
                return String.Empty;

            return String.Format("{0}license={1}&licenseNr={2}&company={3}", prefix, licenseId, licenseNr, company.ActorCompanyId);
        }

        #endregion
    }
}
