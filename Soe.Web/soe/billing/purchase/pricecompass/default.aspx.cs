using SoftOne.Soe.Common.Util;
using System;


namespace SoftOne.Soe.Web.soe.billing.purchase.pricecompass
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Price_Optimization;
            base.Page_Init(sender, e);
        }
    }
}