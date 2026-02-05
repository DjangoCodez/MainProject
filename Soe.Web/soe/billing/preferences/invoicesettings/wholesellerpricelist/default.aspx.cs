using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.preferences.invoicesettings.wholesellerpricelist
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Preferences_InvoiceSettings_WholeSellerPriceList;
            base.Page_Init(sender, e);

            //Add javascript method to enable modal window
            Scripts.Add("pricelistupdate.js");
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Do nothing
        }
    }
}
