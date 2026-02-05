using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web
{
    public partial class setaccountyear : PageBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int accountYearId;
            if (Int32.TryParse(QS["accountYear"], out accountYearId))
            {
                AccountManager am = new AccountManager(ParameterObject);

                //Get AccountYear
                AccountYear accountYear = am.GetAccountYear(accountYearId);
                if (accountYear != null)
                {
                    SettingManager sm = new SettingManager(ParameterObject);

                    //Update default AccountYear
                    sm.UpdateInsertIntSetting(SettingMainType.UserAndCompany, (int)UserSettingType.AccountingAccountYear, accountYear.AccountYearId, UserId, SoeCompany.ActorCompanyId, 0);

                    //Update Session
                    CurrentAccountYear = accountYear;
                }

                string module = QS["module"];
                if (String.IsNullOrEmpty(module))
                    RedirectToHome();
                else
                    RedirectToModule(module);
            }
        }
    }
}
