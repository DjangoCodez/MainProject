using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.billing.distribution.edistribution
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Distribution_Reports;
            base.Page_Init(sender, e);
        }
    }
}
