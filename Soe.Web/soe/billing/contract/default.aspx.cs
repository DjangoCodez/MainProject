using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.contract
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Contract;
            base.Page_Init(sender, e);
        }
    }
}
