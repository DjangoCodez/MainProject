using SoftOne.Soe.Common.Util;
using System;


namespace SoftOne.Soe.Web.soe.billing.purchase.pricelists
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Purchase_Pricelists;
            base.Page_Init(sender, e);
        }
    }
}