using System;
using System.Collections.Generic;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.statistics.product
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Statistics_Product;
            base.Page_Init(sender, e);
        }
    }
}