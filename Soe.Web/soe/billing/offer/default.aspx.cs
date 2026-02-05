using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.billing.offer
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Offer;
            base.Page_Init(sender, e);
        }
    }
}
