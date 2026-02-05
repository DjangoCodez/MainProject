using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.time.preferences.salarysettings.priceformula
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Preferences_SalarySettings_PriceFormula;
            base.Page_Init(sender, e);
        }
    }
}
