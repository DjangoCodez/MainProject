using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.Util;
using System;
using System.Linq;

namespace SoftOne.Soe.Web.UserControls.Layout
{
    public partial class TopMenu : ControlBase
    {
        protected string AssemblyVersion = "";
        protected string CurrentAccountYear = "";
        protected string CurrentAccountYearClass = "";
        protected bool ShowCurrentAccountYear = false;
        protected string TemplateCompanyLabel = "";
        protected bool ShowTemplateCompany = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            GetAssemblyVersion();
            ShowAccountYear();
            CheckTemplateCompany();
        }

        private void GetAssemblyVersion()
        {
            AssemblyVersion = GeneralManager.GetAssemblyVersion();
        }
        private void ShowAccountYear()
        {
            if (PageBase.SoeCompany != null && PageBase.User != null && PageBase.CurrentAccountYear == null)
            {
                var cachedAccountYearKey = "AccountYear_" + PageBase.ParameterObject.ActorCompanyId + "_" + PageBase.UserId;
                int defaultAccountYearId;
                int? accountYear = BusinessMemoryCache<int?>.Get(cachedAccountYearKey);
                if (accountYear.HasValue)
                    defaultAccountYearId = accountYear.Value;
                else
                {
                    SettingManager sm = new SettingManager(PageBase.ParameterObject);
                    defaultAccountYearId = sm.GetIntSetting(SettingMainType.UserAndCompany, (int)UserSettingType.AccountingAccountYear, PageBase.UserId, PageBase.SoeCompany.ActorCompanyId, 0);

                    var extendCacheTime = defaultAccountYearId == 0 ? HasAccountYears() ? 60 : 300 : 0;
                    BusinessMemoryCache<int?>.Set(cachedAccountYearKey, defaultAccountYearId, 20 + extendCacheTime);
                }

                if (defaultAccountYearId > 0)
                {
                    //Update AccountYear if last isnt current                    
                    AccountManager am = new AccountManager(PageBase.ParameterObject);
                    PageBase.CurrentAccountYear = am.GetAccountYear(defaultAccountYearId);
                }
            }

            //Fix for problem when changing from a Company with CurrentAccountYear to a Company with no AccountYears. Then flush CurrentAccountYear property
            if (PageBase.SoeCompany == null || (PageBase.CurrentAccountYear != null && PageBase.CurrentAccountYear.ActorCompanyId != PageBase.SoeCompany.ActorCompanyId))
                PageBase.CurrentAccountYear = null;

            string module = Module;
            if (module == Constants.SOE_MODULE_ECONOMY || module == Constants.SOE_MODULE_BILLING)
            {
                ShowCurrentAccountYear = true;
                if (PageBase.CurrentAccountYear != null)
                {
                    CurrentAccountYear = PageBase.CurrentAccountYear.From.ToString("yyyyMMdd") + "-" + PageBase.CurrentAccountYear.To.ToString("yyyyMMdd");
                    if (PageBase.CurrentAccountYear.Status == (int)TermGroup_AccountStatus.Closed || PageBase.CurrentAccountYear.Status == (int)TermGroup_AccountStatus.Locked)
                        CurrentAccountYearClass = "year-not-open";
                }
                else
                    CurrentAccountYear = "[" + PageBase.GetText(5471, "Inget år valt") + "]";
            }
        }

        private bool HasAccountYears()
        {
            var key = "AccountYears_" + PageBase.SoeCompany.ActorCompanyId;
            var hasAccountYears = BusinessMemoryCache<bool?>.Get(key);

            if (hasAccountYears.HasValue)
                return hasAccountYears.Value;

            AccountManager accountManager = new AccountManager(PageBase.ParameterObject);
            hasAccountYears = accountManager.GetAccountYears(PageBase.SoeCompany.ActorCompanyId, false, false).Any();
            BusinessMemoryCache<bool?>.Set(key, hasAccountYears, 600);

            return hasAccountYears == true;
        }

        private void CheckTemplateCompany()
        {
            if (PageBase.SoeCompany != null)
            {
                ShowTemplateCompany = PageBase.SoeCompany.Template || PageBase.SoeCompany.Global;
                if (ShowTemplateCompany)
                    TemplateCompanyLabel = PageBase.GetText(4589, "OBS!") + " " + PageBase.GetText(4590, "MALLFÖRETAG");
            }
        }
    }
}