using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace SoftOne.Soe.Web.modalforms
{
    public partial class SelectCompany : PageBase
    {
        #region Variables

        #endregion

        override protected void Page_Init(object sender, EventArgs e)
        {
            base.Page_Init(sender, e);
            Scripts.Add("~/modalforms/SelectCompanyFilter.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            ((ModalFormMaster)Master).HeaderText = GetText(4762, "Välj företag");
            ((ModalFormMaster)Master).Action = Url;
            ((ModalFormMaster)Master).showSubmitButton = false;
            //((ModalFormMaster)Master).showActionButton = true;

            #endregion

            #region Perform

            RenderTable();

            #endregion
        }

        private void RenderTable()
        {
            List<Company> companies = GetCompanies();
            foreach (Company company in companies.OrderBy(i => i.CompanyNr))
            {
                var tblRow = new HtmlTableRow();
                tblCompanies.Controls.Add(tblRow);

                var tblCellCompanyNr = new HtmlTableCell { InnerText = company.CompanyNr.ToString() };
                tblRow.Controls.Add(tblCellCompanyNr);

                var tblCellName = new HtmlTableCell { InnerText = company.Name };
                tblRow.Controls.Add(tblCellName);

                var tblCellShortName = new HtmlTableCell { InnerText = company.ShortName };
                tblRow.Controls.Add(tblCellShortName);

                var tblCellOrgNr = new HtmlTableCell { InnerText = company.OrgNr };
                tblRow.Controls.Add(tblCellOrgNr);

                var tblCellLink = new HtmlTableCell();
                tblRow.Controls.Add(tblCellLink);

                var link = new HyperLink {NavigateUrl = company != null ? (GetBaseUrl() + "default.aspx?c=" + company.ActorCompanyId) : "#"};
                var span = new HtmlGenericControl("span");
                span.Attributes["class"] = "fal fa-building";
                link.Controls.Add(span);
                tblCellLink.Controls.Add( link);

                /*
                var link = new HtmlLink { Href = company != null ? (GetBaseUrl() + "default.aspx?c=" + company.ActorCompanyId) : "#" };
                var span = new HtmlGenericControl("span");
                span.Attributes["class"] = "fal fa-building";
                span.Controls.Add(link);
                tblCellLink.Controls.Add(span);
                */
            }
        }

        private List<Company> GetCompanies()
        {
            var companies = new List<Company>();

            List<UserCompanyRole> userCompanyRoles = CompDbCache.Instance.GetUserCompanyRoles(UserId);
            foreach (UserCompanyRole userCompanyRole in userCompanyRoles.Where(i => i.Company.LicenseId == SoeLicense.LicenseId))
            {
                if (!companies.Any(i => i.ActorCompanyId == userCompanyRole.ActorCompanyId))
                    companies.Add(userCompanyRole.Company);
            }

            return companies;
        }

        private string GetBaseUrl()
        {
            string module = Module;
            string baseUrl = "/soe/";
            if (!String.IsNullOrEmpty(module))
            {
                baseUrl += module + "/";

                // Module manage must redirect to Section-root because QS in each Tab-url
                if (module != Constants.SOE_MODULE_MANAGE)
                {
                    if (!String.IsNullOrEmpty(Section))
                        baseUrl += Section + "/";
                }
            }
            return baseUrl;
        }
    }
}
