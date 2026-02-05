using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SoftOne.Soe.Web.soe.manage.companies
{
    public partial class _default : PageBase
    {
        #region Variables

        private CompanyManager cm;
        private LicenseManager lm;

        private License license;

		protected string licenseNr;
		protected int licenseId;
        
        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Companies;
            base.Page_Init(sender, e);
        }
        
        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            cm = new CompanyManager(ParameterObject);
			lm = new LicenseManager(ParameterObject);

            //Mandatory parameters
            if (!Int32.TryParse(QS["license"], out licenseId))
                throw new SoeQuerystringException("license", this.ToString());
            license = lm.GetLicense(licenseId);
            if (license == null)
                throw new SoeQuerystringException("license", this.ToString());

            licenseNr = QS["licenseNr"];
            if (String.IsNullOrEmpty(licenseNr))
                throw new SoeQuerystringException("licenseNr", this.ToString());
                        
            #endregion

            //Title
            SoeGrid1.Title = GetText(1057, "Företag");
            if (license != null)
                SoeGrid1.Title += " " + GetText(1604, "för") + " " + GetText(1605, "licens") + " '" + license.Name + "'";

            List<Company> companies = cm.GetCompaniesByLicense(licenseId).OrderBy(i => i.CompanyNr).ToList();
            foreach (Company company in companies)
            {
                company.ShowSupportLogin = CanLoginAsSupportAdmin(company, company.LicenseId, company.License.LicenseNr);
            }

            SoeGrid1.DataSource = companies;
			SoeGrid1.DataBind();

            #region Navigation

            SoeGrid1.AddRegLink(GetText(2030, "Registrera företag"), "edit/" + GetBaseQS(),
                Feature.Manage_Companies_Edit, Permission.Modify, false, null, true);

            SoeGrid1.AddRegLink(GetText(2030, "Registrera företag"),
                "companysetupwizard/?license=" + licenseId + "&licenseNr=" + licenseNr,
                Feature.Manage_Companies_Edit, Permission.Modify, true, "/img/wizard.png",true);

            #endregion
        }

        private string GetBaseQS(string prefix = "?")
        {
            return String.Format("{0}license={1}&licenseNr={2}", prefix, licenseId, licenseNr);
        }
    }
}
