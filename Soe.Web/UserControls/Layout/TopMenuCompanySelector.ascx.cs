using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOne.Soe.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.UserControls.Layout
{
    public partial class TopMenuCompanySelector : ControlBase
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            Render();
        }

        protected void Render()
        {
            var companies = GetCompanies();

            if (companies.Count > 10)
            {
                Container.Style.Add("display", "inline");

                Company currentCompany = companies.Find(i => i.ActorCompanyId == PageBase.SoeCompany.ActorCompanyId);
                var selectorItem = GetCompanyDialogItem(currentCompany?.Name ?? string.Empty);
                var selectedItemControl = selectorItem.GetLinkControl();
                selectedItemControl.Attributes.Add("title", PageBase.GetText(4762, "Välj företag"));

                var span = new HtmlGenericControl("span");
                span.Attributes["class"] = "fal fa-building";
                span.Style.Add("margin-left", "5px");
                selectedItemControl.Controls.Add(span);

                Container.Controls.Add(selectedItemControl);
            }
            else
            {
                Container.Attributes.Add("class", "dropdown");
                List<TopMenuSelectorItem> items = GetItems(companies);
                if (items.Count > 0)
                {
                    // Selected
                    var selectedItem = items[0];
                    HtmlGenericControl selectedItemControl = null;

                    selectedItemControl = selectedItem.GetTopMenuSelectorItemControl(true, items.Count > 1);
                    selectedItemControl.Attributes.Add("title", PageBase.GetText(1938, "Företag"));

                    Container.Controls.Add(selectedItemControl);

                    // Menu
                    if (items.Count > 1)
                    {
                        var list = GetListControl(false);

                        for (int i = 1; i < items.Count; i++)
                        {
                            TopMenuSelectorItem item = items[i];
                            if (item.IsSeparator)
                            {
                                var li = new HtmlGenericControl("li");
                                li.Attributes.Add("class", "divider");
                                list.Controls.Add(li);
                            }
                            else if (item.IsHeader)
                            {
                                var li = new HtmlGenericControl("li");
                                li.Attributes.Add("class", "dropdown-header");
                                li.InnerText = item.Label;
                                list.Controls.Add(li);
                            }
                            else
                            {
                                HtmlGenericControl ctrl = item.GetTopMenuSelectorItemControl(false, false);

                                list.Controls.Add(ctrl);
                            }
                        }
                        Container.Controls.Add(list);
                    }
                }
            }
        }

        #region Help-methods

        private List<Company> GetCompanies()
        {
            var companies = new List<Company>();

            if (PageBase.SoeUser != null && PageBase.SoeLicense != null)
            {
                List<UserCompanyRole> userCompanyRoles = CompDbCache.Instance.GetUserCompanyRoles(PageBase.UserId);
                foreach (UserCompanyRole userCompanyRole in userCompanyRoles.Where(i => i.Company.LicenseId == PageBase.SoeLicense.LicenseId))
                {
                    if (!companies.Exists(i => i.ActorCompanyId == userCompanyRole.ActorCompanyId))
                        companies.Add(userCompanyRole.Company);
                }
            }

            return companies;
        }

        private List<TopMenuSelectorItem> GetItems(List<Company> companies)
        {
            List<TopMenuSelectorItem> menuSelectorItems = new List<TopMenuSelectorItem>();
            if (PageBase.SoeCompany == null || PageBase.SoeUser == null)
                return menuSelectorItems;

            #region Init

            // Get base URL
            string module = Module;
            string baseUrl = "/soe/";
            string extraQS = "";

            if (!String.IsNullOrEmpty(module))
            {
                baseUrl += module + "/";

                // Module manage must redirect to Section-root because QS in each Tab-url
                if (module != Constants.SOE_MODULE_MANAGE && !string.IsNullOrEmpty(PageBase.SelectionUrl))
                    baseUrl += PageBase.SelectionUrl + "/";
            }

            extraQS = this.PageBase.AddClassficationToQueryString(extraQS);
            
            #endregion

            #region Items

            // Current Company
            if (PageBase.SoeCompany != null)
            {
                Company currentCompany = companies.Find(i => i.ActorCompanyId == PageBase.SoeCompany.ActorCompanyId);
                menuSelectorItems.Add(GetCompanyItem(currentCompany, baseUrl, true, extraQS));
            }

            // Other Companies
            foreach (Company company in companies.OrderBy(c => c.CompanyNr))
            {
                if (PageBase.SoeCompany != null && PageBase.SoeCompany.ActorCompanyId == company.ActorCompanyId)
                    continue;
                if (PageBase.IsSupportLoggedIn && !company.IsSupportLoginAllowed())
                    continue;

                menuSelectorItems.Add(GetCompanyItem(company, baseUrl, false, extraQS));
            }

            #endregion

            return menuSelectorItems;
        }

        private HtmlGenericControl GetListControl(bool multiLevel)
        {
            HtmlGenericControl ul = new HtmlGenericControl("ul");
            ul.Attributes.Add("class", "dropdown-menu" + (multiLevel ? " multi-level" : ""));

            return ul;
        }

        private TopMenuSelectorItem GetCompanyItem(Company company, string baseUrl, bool current, string extraQS)
        {
            return new TopMenuSelectorItem
            {
                Href = company != null ? string.Format("/setcompany.aspx/?c={0}&prev={1}", company.ActorCompanyId, HttpUtility.UrlEncode(baseUrl + "default.aspx?c=" + company.ActorCompanyId + extraQS)) : "#",
                Label = company != null ? (current ? company.License.LicenseNr + ' ' + company.Name : String.Format("{0} - {1}", company.CompanyNr, company.Name)) : "#",
            };
        }

        private TopMenuSelectorItem GetCompanyDialogItem(string caption)
        {
            return new TopMenuSelectorItem
            {
                Href = "/modalforms/SelectCompany.aspx",
                Label = string.IsNullOrEmpty(caption) ? PageBase.GetText(1938, "Företag") : caption,
                IsModal = true
            };
        }
            
        #endregion
    }
}
