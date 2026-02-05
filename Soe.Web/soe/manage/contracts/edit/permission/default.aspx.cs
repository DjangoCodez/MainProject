using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;

namespace SoftOne.Soe.Web.soe.manage.contracts.edit.permission
{
    public partial class _default : PageBase
    {
        #region Variables

        private License license;

        public bool IsAuthorized
        {
            get
            {
                if (license == null)
                    return false;

                //Rule 1: Same License
                if (SoeCompany.LicenseId == license.LicenseId)
                    return true;

                //Rule 2: Administrators on SupportLicense
                if (SoeLicense.Support && SoeUser.IsAdmin)
                    return true;

                return false;
            }
        }

        protected string licenseNr;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Companies_Edit_Permission;
            base.Page_Init(sender, e);
        }

		protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            //Mandatory parameters
			licenseNr = QS["licensenr"];
            if (!String.IsNullOrEmpty(licenseNr))
            {
                license = LicenseCacheManager.Instance.GetLicense(licenseNr);
                if (license == null)
                    throw new SoeEntityNotFoundException("License", this.ToString());
            }
            else
                throw new SoeQuerystringException("licensenr", this.ToString());

			int sysPermissionId;
			if (!Int32.TryParse(QS["permission"], out sysPermissionId))
                throw new SoeQuerystringException("permission", this.ToString());

            #endregion

            #region Authorization

            if (!IsAuthorized)
                RedirectToUnauthorized(UnauthorizationType.DataAuthorityMissing);

            #endregion

            #region Populate

            //Set UserControl parameters
            FeaturePermissionTree.CurrentFeature = this.Feature;
			FeaturePermissionTree.FeatureType = SoeFeatureType.License;
			FeaturePermissionTree.LicenseId = license.LicenseId;
			FeaturePermissionTree.Permission = (Permission) sysPermissionId;
            FeaturePermissionTree.SubTitle = GetText(1604, "för") + " " + GetText(1605, "licens") + " " + license.Name;

			// Clear permission cache if permission tree is saved
			if (IsPostBack)
			{
				RemoveAllOutputCacheItems(Request.Url.AbsolutePath);
            }

            #endregion

            #region Navigation

            if (sysPermissionId == (int)Permission.Readonly)
			{
                FeaturePermissionTree.AddLink(GetText(1080, "Skrivbehörighet"), GetBaseQS() + "&permission=" + (int)Permission.Modify,
					Feature.Manage_Contracts_Edit_Permission, Permission.Readonly);
			}
			else if (sysPermissionId == (int)Permission.Modify)
			{
                FeaturePermissionTree.AddLink(GetText(1077, "Läsbehörighet"), GetBaseQS() + "&permission=" + (int)Permission.Readonly,
					Feature.Manage_Contracts_Edit_Permission, Permission.Readonly);
            }

            #endregion
        }

        #region Help-methods

        private string GetBaseQS(string prefix = "?")
        {
            if (license == null)
                return String.Empty;
            
            return String.Format("{0}license={1}&licenseNr={2}", prefix, license.LicenseId, license.LicenseNr);
        }

        #endregion
	}
}
