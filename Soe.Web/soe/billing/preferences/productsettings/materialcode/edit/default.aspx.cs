using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.billing.preferences.productsettings.materialcode.edit
{
    public partial class dafault : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Preferences_ProductSettings_MaterialCode_Edit;
            base.Page_Init(sender, e);
        }
    }
}