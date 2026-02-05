using System;
using System.Collections.Generic;

using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.preferences.productsettings.materialcode
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Preferences_ProductSettings_MaterialCode;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Do nothing
        }
    }
}