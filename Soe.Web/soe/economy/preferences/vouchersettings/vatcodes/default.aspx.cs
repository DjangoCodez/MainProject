using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.economy.preferences.vouchersettings.vatcodes
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Preferences_VoucherSettings_VatCodes;
            base.Page_Init(sender, e);
        }
    }
}
