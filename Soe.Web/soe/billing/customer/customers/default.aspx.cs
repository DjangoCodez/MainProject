using System;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Business.Core;

namespace SoftOne.Soe.Web.soe.billing.customer.customers
{
    public partial class _default : PageBase
    {        
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Customer_Customers;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Do nothing
        }
    }
}
