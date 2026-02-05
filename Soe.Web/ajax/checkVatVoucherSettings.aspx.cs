using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.ajax
{
    public partial class checkVatVoucherSettings : JsonBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            int voucherSeriesId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountingVoucherSeriesTypeVat, 0, SoeCompany.ActorCompanyId, 0);
            int accountId = SettingManager.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.AccountCommonVatAccountingKredit, 0, SoeCompany.ActorCompanyId, 0);

            ResponseObject = new
            {
                Found = true,
                AllValid = voucherSeriesId > 0 && accountId > 0,
                VoucherSeriesValid = voucherSeriesId > 0,
                AccountValid = accountId > 0,
            };
        }
    }
}