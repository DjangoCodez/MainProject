using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class setAccountYear : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                int accountYearId;
                if (Int32.TryParse(QS["accountYearId"], out accountYearId))
                {
                    AccountManager am = new AccountManager(ParameterObject);

                    // Get AccountYear
                    AccountYear accountYear = am.GetAccountYear(accountYearId);
                    if (accountYear != null)
                    {
                        SettingManager sm = new SettingManager(ParameterObject);

                        // Update default AccountYear
                        ActionResult result = sm.UpdateInsertIntSetting(SettingMainType.UserAndCompany, (int)UserSettingType.AccountingAccountYear, accountYear.AccountYearId, UserId, SoeCompany.ActorCompanyId, 0);

                        // Update Session
                        CurrentAccountYear = accountYear;

                        ResponseObject = new { Success = result.Success };
                    }
                }
            }
            catch (Exception)
            {
                ResponseObject = new { Success = false };
            }
        }
    }
}