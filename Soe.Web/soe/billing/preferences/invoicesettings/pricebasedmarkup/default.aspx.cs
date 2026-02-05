using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.preferences.invoicesettings.pricebasedmarkup
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Preferences_InvoiceSettings_PriceBasedMarkup;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Do nothing
        }
    }
}
