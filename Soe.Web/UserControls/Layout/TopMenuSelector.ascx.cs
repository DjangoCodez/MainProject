using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;

namespace SoftOne.Soe.Web.UserControls.Layout
{
    public partial class TopMenuSelector : ControlBase
    {
        public TopMenuSelectorType Type { get; set; }
        protected string CurrentAccountYear = "";
        protected string CurrentAccountYearClass = "";
        private AccountManager am = null;
        private GeneralManager gm = null;
        private SettingManager sm = null;

        protected void Page_Load(object sender, EventArgs e)
        {
            am = new AccountManager(PageBase.ParameterObject);
            gm = new GeneralManager(PageBase.ParameterObject);
            sm = new SettingManager(PageBase.ParameterObject);

            Render();
        }

        protected void Render()
        {
            Container.Attributes.Add("class", "dropdown");

            List<TopMenuSelectorItem> items = GetItems();
            if (items.Count > 0)
            {
                // Selected
                var selectedItem = items[0];
                HtmlGenericControl selectedItemControl = null;
                HtmlGenericControl span;

                switch (this.Type)
                {
                    case TopMenuSelectorType.Role:
                        #region Role

                        selectedItemControl = selectedItem.GetTopMenuSelectorItemControl(true, items.Count > 1);
                        selectedItemControl.Attributes.Add("title", PageBase.GetText(1116, "Roll"));

                        #endregion
                        break;
                    case TopMenuSelectorType.User:
                        #region User

                        selectedItemControl = selectedItem.GetTopMenuSelectorItemControl(true, items.Count > 1);
                        selectedItemControl.Attributes.Add("title", PageBase.GetText(1940, "Användare"));

                        #endregion
                        break;
                    case TopMenuSelectorType.Favorites:
                        #region Favorites

                        selectedItemControl = new HtmlGenericControl("a");
                        selectedItemControl.Attributes.Add("id", "favoriteSelector");
                        selectedItemControl.Attributes.Add("class", "dropdown-toggle");
                        selectedItemControl.Attributes.Add("data-toggle", "dropdown");
                        selectedItemControl.Attributes.Add("title", PageBase.GetText(2002, "Favoriter"));
                        span = new HtmlGenericControl("span");
                        span.Attributes["class"] = "fal fa-star";
                        selectedItemControl.Controls.Add(span);

                        // Add dummy item because loop below skips first item
                        items.Insert(0, new TopMenuSelectorItem());

                        #endregion
                        break;                    
                    case TopMenuSelectorType.AccountYear:
                        #region AccountYear

                        selectedItemControl = selectedItem.GetTopMenuSelectorItemControl(true, items.Count > 1);
                        selectedItemControl.Attributes.Add("class", "account-year " + CurrentAccountYearClass);
                        selectedItemControl.Attributes.Add("title", PageBase.GetText(5467, "Redovisningsår"));

                        #endregion
                        break;
                    case TopMenuSelectorType.PageStatusBeta:
                        #region PageStatusBeta

                        selectedItemControl = selectedItem.GetTopMenuSelectorItemControl(true, items.Count > 1);
                        selectedItemControl.Attributes.Add("title", PageBase.GetText(11005, "Angularsidans status i interna miljöer samt REF"));

                        #endregion
                        break;
                    case TopMenuSelectorType.PageStatusLive:
                        #region PageStatusLive

                        selectedItemControl = selectedItem.GetTopMenuSelectorItemControl(true, items.Count > 1);
                        selectedItemControl.Attributes.Add("title", PageBase.GetText(11006, "Angularsidans status i skarpa miljöer utöver REF"));

                        #endregion
                        break;
                    case TopMenuSelectorType.AccountHierarchy:
                        #region Account

                        selectedItemControl = selectedItem.GetTopMenuSelectorItemControl(true, items.Count > 1);
                        selectedItemControl.Attributes.Add("title", PageBase.GetText(11779, "Byt konto"));

                        #endregion
                        break;
                }

                Container.Controls.Add(selectedItemControl);

                // Menu
                if (items.Count > 1)
                {
                    var list = GetListControl(this.Type == TopMenuSelectorType.User);

                    // Prevent drop down to expand off screen
                    if (this.Type == TopMenuSelectorType.User || this.Type == TopMenuSelectorType.Favorites || this.Type == TopMenuSelectorType.AccountHierarchy)
                    {
                        list.Style.Add("left", "auto");
                        list.Style.Add("right", "-50px");
                        list.Style.Add("min-width", "300px");
                    }

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
                            if (this.Type == TopMenuSelectorType.AccountYear)
                                ctrl.Attributes.Add("onclick", "SetAccountYear(" + item.ID + "); return false;");
                            else if (this.Type == TopMenuSelectorType.AccountHierarchy)
                                ctrl.Attributes.Add("onclick", "SetAccountHierarchy('" + item.ID + "'); return false;");

                            list.Controls.Add(ctrl);
                        }
                    }
                    Container.Controls.Add(list);
                }
            }
        }

        #region Help-methods

        private List<TopMenuSelectorItem> GetItems()
        {
            List<TopMenuSelectorItem> menuSelectorItems = new List<TopMenuSelectorItem>();
            if (PageBase.SoeCompany == null || PageBase.SoeUser == null)
                return menuSelectorItems;

            #region Init

            // Get base URL
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

            #endregion

            #region Items


            switch (this.Type)
            {
                case TopMenuSelectorType.Role:
                    menuSelectorItems = GetItemsForRole(baseUrl);
                    break;
                case TopMenuSelectorType.User:
                    menuSelectorItems = GetItemsForUser();
                    break;
                case TopMenuSelectorType.Favorites:
                    menuSelectorItems = GetItemsForFavorites();
                    break;
                case TopMenuSelectorType.AccountYear:
                    menuSelectorItems = GetItemsForAccountYear();
                    break;
                case TopMenuSelectorType.PageStatusBeta:
                case TopMenuSelectorType.PageStatusLive:
                    menuSelectorItems = GetItemsForPageStatus();
                    break;
                case TopMenuSelectorType.AccountHierarchy:
                    menuSelectorItems = GetItemsForAccountHierarchy();
                    break;
            }

            #endregion

            return menuSelectorItems;
        }

        private List<TopMenuSelectorItem> GetItemsForRole(string baseUrl)
        {
            List<TopMenuSelectorItem> menuSelectorItems = new List<TopMenuSelectorItem>();

            List<Role> roles = new List<Role>();

            List<UserCompanyRole> userCompanyRoles = CompDbCache.Instance.GetUserCompanyRoles(PageBase.UserId);
            foreach (UserCompanyRole userCompanyRole in userCompanyRoles.Where(i => i.ActorCompanyId == PageBase.SoeCompany.ActorCompanyId))
            {
                if (!roles.Exists(i => i.RoleId == userCompanyRole.RoleId))
                    roles.Add(userCompanyRole.Role);
            }

            // Current Role
            if (PageBase.RoleId > 0)
                menuSelectorItems.Add(GetRoleItem(roles.Find(i => i.RoleId == PageBase.RoleId), baseUrl));

            // Other Roles
            foreach (Role role in roles)
            {
                if (PageBase.RoleId == role.RoleId)
                    continue;
                menuSelectorItems.Add(GetRoleItem(role, baseUrl));
            }

            return menuSelectorItems;
        }

        private List<TopMenuSelectorItem> GetItemsForUser()
        {
            List<TopMenuSelectorItem> menuSelectorItems = new List<TopMenuSelectorItem>();

            menuSelectorItems.Add(new TopMenuSelectorItem()
            {
                Href = "",
                Label = PageBase.SoeUser.LoginName,
            });

            bool addUserSeparator = false;
            if (PageBase.HasRolePermission(Feature.Time_Employee_Employees, Permission.Readonly))
            {
                // Get employee
                var employee = SessionCache.GetEmployeeFromCache(PageBase.UserId, PageBase.ParameterObject.ActorCompanyId);
                if (employee != null && employee.EmployeeId > 0)
                {
                    menuSelectorItems.Add(new TopMenuSelectorItem()
                    {
                        Href = "/soe/time/employee/employees?employeeId=" + employee.EmployeeId,
                        Label = String.Format("({0}) {1}", employee.EmployeeNr, employee.Name),
                    });
                    addUserSeparator = true;
                }
            }

            if (PageBase.HasRolePermission(Feature.Manage_Users, Permission.Readonly))
            {
                menuSelectorItems.Add(new TopMenuSelectorItem()
                {
                    Href = "/soe/manage/users/?c=" + PageBase.SoeCompany.ActorCompanyId + "&license=" + PageBase.SoeLicense.LicenseId + "&company=" + PageBase.SoeCompany.ActorCompanyId + "&user=" + PageBase.UserId,
                    Label = PageBase.GetText(3770, "Min profil"),
                });
                addUserSeparator = true;
            }

            if (addUserSeparator)
                menuSelectorItems.Add(new TopMenuSelectorItem() { IsSeparator = true });

            if (PageBase.HasRolePermission(Feature.Common_Language, Permission.Readonly))
            {
                List<TopMenuSelectorItem> subMenuItems = new List<TopMenuSelectorItem>();

                int langId = PageBase.GetLanguageId();
                var sysLanguages = SysDbCache.Instance.SysLanguages.Where(l => l.Translated && l.SysLanguageId != langId).ToList(); 
                List<GenericType> languageTerms = TermCacheManager.Instance.GetTermGroupContent(TermGroup.Language, sortById: true);

                string prev = HttpUtility.UrlEncode(
                    HttpUtility.UrlEncode(
                        this.Request.Url.PathAndQuery));
                foreach (var sysLanguage in sysLanguages)
                {
                    var languageTerm = languageTerms.FirstOrDefault(l => l.Id == sysLanguage.SysLanguageId);
                    subMenuItems.Add(new TopMenuSelectorItem()
                    {
                        Href = string.Format("/setlang.aspx/?lang={0}&prev={1}", sysLanguage.LangCode, prev),
                        Label = languageTerm.Name,
                    });
                }

                if (subMenuItems.Count > 0)
                {
                    menuSelectorItems.Add(new TopMenuSelectorItem()
                    {
                        Href = "",
                        Label = PageBase.GetText(3772, "Byt språk"),
                        SubItems = subMenuItems,
                    });
                    menuSelectorItems.Add(new TopMenuSelectorItem() { IsSeparator = true });
                }
            }

            menuSelectorItems.Add(new TopMenuSelectorItem()
            {
                Href = "/clearlocalstorage.aspx",
                Label = PageBase.GetText(11014, "Töm lokal cache"),
            });
            menuSelectorItems.Add(new TopMenuSelectorItem() { IsSeparator = true });

            menuSelectorItems.Add(new TopMenuSelectorItem()
            {
                Href = "/logout.aspx",
                Label = PageBase.GetText(18, "Logga ut"),
            });

            return menuSelectorItems;
        }

        private List<TopMenuSelectorItem> GetItemsForFavorites()
        {
            List<TopMenuSelectorItem> menuSelectorItems = new List<TopMenuSelectorItem>();
            List<MenuFavoriteItem> menuFavoriteItems = new List<MenuFavoriteItem>();
            List<CompanyDTO> companies = new List<CompanyDTO>();
            List<FavoriteItem> favoriteItems = SettingCacheManager.Instance.GetUserFavorites(PageBase.UserId);
            foreach (FavoriteItem favoriteItem in favoriteItems)
            {
                if (string.IsNullOrEmpty(favoriteItem.FavoriteName) || string.IsNullOrEmpty(favoriteItem.FavoriteUrl))
                    continue;

                CompanyDTO company = null;
                if (favoriteItem.FavoriteCompany.HasValue && favoriteItem.FavoriteCompany.Value != PageBase.SoeCompany.ActorCompanyId)
                {
                    company = companies.FirstOrDefault(c => c.ActorCompanyId == favoriteItem.FavoriteCompany.Value) ?? SessionCache.GetCompanyFromCache(favoriteItem.FavoriteCompany.Value);
                    if (company != null && !companies.Any(c => c.ActorCompanyId == company.ActorCompanyId))
                        companies.Add(company);
                }

                menuFavoriteItems.Add(new MenuFavoriteItem()
                {
                    FavoriteId = favoriteItem.FavoriteId,
                    Name = company != null ? $"{favoriteItem.FavoriteName} ({company.Name})" : favoriteItem.FavoriteName,
                    Url = favoriteItem.FavoriteCompany.HasValue ? favoriteItem.FavoriteUrl : favoriteItem.FavoriteUrl + "?r=" + PageBase.RoleId,
                    IsSupportFavorite = favoriteItem.FavoriteName.Contains(PageBase.TextService.GetSupportText()),
                });
            }

            menuSelectorItems.Add(new TopMenuSelectorItem()
            {
                Label = PageBase.GetText(5442, "Hantera favoriter"),
                IsHeader = true
            });

            string url = HttpContext.Current?.Request?.Url?.PathAndQuery;
            if (!string.IsNullOrEmpty(url))
            {
                menuSelectorItems.Add(new TopMenuSelectorItem()
                {
                    Href = $"/modalforms/RegFavorite.aspx?c={PageBase.SoeCompany.ActorCompanyId}&url={Server.UrlEncode(url)}",
                    Label = PageBase.GetText(2003, "Lägg till favorit"),
                    IsModal = true
                });
            }

            #region Favorites

            if (menuFavoriteItems.Any(i => !i.IsSupportFavorite))
            {
                menuSelectorItems.Add(new TopMenuSelectorItem()
                {
                    IsSeparator = true
                });

                menuSelectorItems.Add(new TopMenuSelectorItem()
                {
                    Label = PageBase.GetText(2002, "Favoriter"),
                    IsHeader = true
                });

                foreach (MenuFavoriteItem menuFavoriteItem in menuFavoriteItems.Where(i => !i.IsSupportFavorite))
                {
                    menuSelectorItems.Add(new TopMenuSelectorItem()
                    {
                        ID = menuFavoriteItem.FavoriteId.ToString(),
                        Href = menuFavoriteItem.Url,
                        Label = menuFavoriteItem.Name,
                        ShowDelete = true,
                        DeleteToolTip = PageBase.GetText(2192, "Ta bort favorit")
                    });
                }
            }

            #endregion

            #region Support favorites

            if (PageBase.IsSupportLicense && menuFavoriteItems.Any(i => i.IsSupportFavorite))
            {
                menuSelectorItems.Add(new TopMenuSelectorItem()
                {
                    IsSeparator = true
                });

                menuSelectorItems.Add(new TopMenuSelectorItem()
                {
                    Label = PageBase.GetText(5948, "Supportinlogg"),
                    IsHeader = true
                });

                foreach (MenuFavoriteItem menuFavoriteItem in menuFavoriteItems.Where(i => i.IsSupportFavorite))
                {
                    menuSelectorItems.Add(new TopMenuSelectorItem()
                    {
                        ID = menuFavoriteItem.FavoriteId.ToString(),
                        Href = menuFavoriteItem.Url,
                        Label = menuFavoriteItem.Name,
                        ShowDelete = true,
                        DeleteToolTip = PageBase.GetText(2192, "Ta bort favorit")
                    });
                }
            }

            #endregion

            #region External links

            UserCompanySetting setting = sm.GetUserCompanySetting(SettingMainType.Company, (int)CompanySettingType.UseExternalLinks, PageBase.UserId, PageBase.SoeCompany.ActorCompanyId, 0);
            if (setting != null && setting.BoolData == true)
            {
                menuSelectorItems.Add(new TopMenuSelectorItem()
                {
                    IsSeparator = true
                });

                menuSelectorItems.Add(new TopMenuSelectorItem()
                {
                    Label = PageBase.GetText(8281, "Externa länkar"),
                    IsHeader = true
                });

                List<int> settingTypeIds = new List<int>();
                settingTypeIds.Add((int)CompanySettingType.ExternalLink1);
                settingTypeIds.Add((int)CompanySettingType.ExternalLink2);
                settingTypeIds.Add((int)CompanySettingType.ExternalLink3);

                Dictionary<int, object> links = sm.GetUserCompanySettings(SettingMainType.Company, settingTypeIds, PageBase.UserId, PageBase.SoeCompany.ActorCompanyId, 0);
                foreach (KeyValuePair<int, object> link in links)
                {
                    // Display name and URL can be separated by a semi colon (NAME;URL)
                    // or just the url can be specified
                    string linkName = string.Empty;
                    string linkUrl = string.Empty;
                    if (!String.IsNullOrEmpty(link.Value.ToString()))
                    {
                        int index = link.Value.ToString().IndexOf(";");
                        if (index > 0)
                        {
                            linkName = link.Value.ToString().Substring(0, index);
                            linkUrl = link.Value.ToString().Substring(index + 1);
                        }
                        else
                        {
                            linkName = linkUrl = link.Value.ToString();
                        }

                        menuSelectorItems.Add(new TopMenuSelectorItem()
                        {
                            Label = linkName,
                            Href = linkUrl,
                            NewWindow = true
                        });
                    }
                }
            }

            #endregion

            return menuSelectorItems;
        }     

        private List<TopMenuSelectorItem> GetItemsForAccountYear()
        {
            List<TopMenuSelectorItem> menuSelectorItems = new List<TopMenuSelectorItem>();

            // Get last used AccountYear
            int defaultAccountYearId = sm.GetIntSetting(SettingMainType.UserAndCompany, (int)UserSettingType.AccountingAccountYear, PageBase.UserId, PageBase.SoeCompany.ActorCompanyId, 0);
            if (defaultAccountYearId > 0)
            {
                // Update AccountYear if last isnt current
                if (PageBase.CurrentAccountYear == null || PageBase.CurrentAccountYear.AccountYearId != defaultAccountYearId)
                    PageBase.CurrentAccountYear = am.GetAccountYear(defaultAccountYearId);
            }

            // Fix for problem when changing from a Company with CurrentAccountYear to a Company with no AccountYears. Then flush CurrentAccountYear property
            if (PageBase.CurrentAccountYear != null && PageBase.CurrentAccountYear.ActorCompanyId != PageBase.SoeCompany.ActorCompanyId)
                PageBase.CurrentAccountYear = null;

            if (PageBase.CurrentAccountYear != null)
            {
                CurrentAccountYear = PageBase.CurrentAccountYear.From.ToString("yyyyMMdd") + " - " + PageBase.CurrentAccountYear.To.ToString("yyyyMMdd");
                if (PageBase.CurrentAccountYear.Status == (int)TermGroup_AccountStatus.Closed || PageBase.CurrentAccountYear.Status == (int)TermGroup_AccountStatus.Locked)
                    CurrentAccountYearClass = "year-not-open";
            }
            else
                CurrentAccountYear = "[" + PageBase.GetText(5471, "Inget år valt") + "]";

            // Selected
            menuSelectorItems.Add(new TopMenuSelectorItem()
            {
                Label = CurrentAccountYear,
            });

            // Get AccountYears (include Open, Closed and Locked)
            var accountYears = am.GetAccountYearsDict(PageBase.SoeCompany.ActorCompanyId, true, false, true, false).OrderByDescending(a => a.Value);
            foreach (var accountYear in accountYears.Where(a => a.Key != defaultAccountYearId))
            {
                menuSelectorItems.Add(new TopMenuSelectorItem()
                {
                    ID = accountYear.Key.ToString(),
                    Label = accountYear.Value,
                });
            }

            if (!accountYears.Any())
            {
                menuSelectorItems.Add(new TopMenuSelectorItem()
                {
                    Label = PageBase.GetText(1752, "Inga år upplagda"),
                });
            }

            return menuSelectorItems;
        }

        private List<TopMenuSelectorItem> GetItemsForPageStatus()
        {
            List<TopMenuSelectorItem> menuSelectorItems = new List<TopMenuSelectorItem>();

            int currentStatus = 0;

            SysPageStatus sysPageStatus = gm.GetSysPageStatus(PageBase.Feature, true);
            if (sysPageStatus == null)
            {
                menuSelectorItems.Add(new TopMenuSelectorItem()
                {
                    Label = PageBase.TextService.GetText(-1, 502, "Välj status"),
                    IsHeader = true
                });
            }
            else
            {
                // Current status
                currentStatus = this.Type == TopMenuSelectorType.PageStatusBeta ? sysPageStatus.BetaStatus : sysPageStatus.LiveStatus;
                menuSelectorItems.Add(new TopMenuSelectorItem()
                {
                    Label = PageBase.TextService.GetText(currentStatus, 502),
                });
            }

            List<GenericType> statusTypes = TermCacheManager.Instance.GetTermGroupContent(TermGroup.SysPageStatusStatusType, sortById: true);
            foreach (GenericType statusType in statusTypes.Where(s => s.Id != -1))
            {
                // Exclude current status
                if (sysPageStatus != null && statusType.Id == currentStatus)
                    continue;

                // Exclude RFT in live
                if (statusType.Id == (int)TermGroup_SysPageStatusStatusType.RFT && this.Type == TopMenuSelectorType.PageStatusLive)
                    continue;

                // Exclude ActiveForCompany (not used)
                if (statusType.Id == (int)TermGroup_SysPageStatusStatusType.ActiveForCompany)
                    continue;

                menuSelectorItems.Add(GetPageStatusItem(this.Type == TopMenuSelectorType.PageStatusBeta ? TermGroup_SysPageStatusSiteType.Beta : TermGroup_SysPageStatusSiteType.Live, (TermGroup_SysPageStatusStatusType)statusType.Id));
            }

            return menuSelectorItems;
        }

        private List<TopMenuSelectorItem> GetItemsForAccountHierarchy()
        {
            List<TopMenuSelectorItem> menuSelectorItems = new List<TopMenuSelectorItem>();

            Dictionary<string, string> accountHierarchyIds = am.GetAccountHierarchyStringsByUser(PageBase.SoeCompany.ActorCompanyId, PageBase.UserId);
            if (!accountHierarchyIds.IsNullOrEmpty())
            {
                // Get last used AccountHierarchy
                string defaultAccountHierarchyId = sm.GetStringSetting(SettingMainType.UserAndCompany, (int)UserSettingType.AccountHierarchyId, PageBase.UserId, PageBase.SoeCompany.ActorCompanyId, 0);
                string defaultAccountHierarchValue = "";
                bool isEmpty = String.IsNullOrEmpty(defaultAccountHierarchyId);
                bool isShowAll = defaultAccountHierarchyId == "0";
                bool setFirst = false;

                if (!isEmpty && !isShowAll)
                {
                    if (accountHierarchyIds.ContainsKey(defaultAccountHierarchyId))
                        defaultAccountHierarchValue = accountHierarchyIds[defaultAccountHierarchyId];
                    else
                        setFirst = true;
                }
                else if (accountHierarchyIds.Count == 1)
                    setFirst = true;

                if (setFirst)
                {
                    defaultAccountHierarchyId = accountHierarchyIds.First().Key;
                    defaultAccountHierarchValue = accountHierarchyIds.First().Value;
                    sm.UpdateInsertStringSetting(SettingMainType.UserAndCompany, (int)UserSettingType.AccountHierarchyId, defaultAccountHierarchyId, PageBase.UserId, PageBase.SoeCompany.ActorCompanyId, 0);
                }

                // Update AccountYear if last isnt current
                if (PageBase.CurrentAccountHierarchy == null || PageBase.CurrentAccountHierarchy != defaultAccountHierarchyId)
                    PageBase.CurrentAccountHierarchy = defaultAccountHierarchyId;

                // Selected
                if (!defaultAccountHierarchValue.IsNullOrEmpty())
                {
                    menuSelectorItems.Add(new TopMenuSelectorItem()
                    {
                        ID = defaultAccountHierarchyId,
                        Label = defaultAccountHierarchValue,
                    });
                }

                menuSelectorItems.Add(new TopMenuSelectorItem()
                {
                    ID = "0",
                    Label = PageBase.GetText(8815, "Alla dina konton"),
                });

                foreach (var accountHierarchyId in accountHierarchyIds.OrderBy(i => i.Value))
                {
                    menuSelectorItems.Add(new TopMenuSelectorItem()
                    {
                        ID = accountHierarchyId.Key.ToString(),
                        Label = accountHierarchyId.Value,
                    });
                }
            }

            return menuSelectorItems;
        }

        private HtmlGenericControl GetListControl(bool multiLevel)
        {
            HtmlGenericControl ul = new HtmlGenericControl("ul");
            ul.Attributes.Add("class", "dropdown-menu" + (multiLevel ? " multi-level" : ""));

            return ul;
        }

        private TopMenuSelectorItem GetRoleItem(Role role, string baseUrl)
        {
            RoleManager rm = new RoleManager(PageBase.ParameterObject);

            return new TopMenuSelectorItem()
            {
                Href = role != null ? string.Format("/setrole.aspx/?c={0}&r={1}&prev={2}", role.ActorCompanyId, role.RoleId, HttpUtility.UrlEncode(baseUrl + "default.aspx?c=" + role.ActorCompanyId + "&r=" + role.RoleId)) : "#",
                Label = role != null ? rm.GetRoleNameText(role) : "#",
            };
        }

        private TopMenuSelectorItem GetPageStatusItem(TermGroup_SysPageStatusSiteType siteType, TermGroup_SysPageStatusStatusType statusType)
        {
            return new TopMenuSelectorItem()
            {
                Href = String.Format("/setpagestatus.aspx/?sysfeature={0}&sitetype={1}&status={2}", (int)PageBase.Feature, (int)siteType, (int)statusType),
                Label = PageBase.TextService.GetText((int)statusType, 502),
            };
        }

        #endregion
    }
}
