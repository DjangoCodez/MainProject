using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.order.edit
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Order_Orders_Edit;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {            
            Response.Redirect("/soe/billing/order/status/?classificationgroup=" + (int)SoeOriginStatusClassificationGroup.HandleOrders);
        }       
    }
}
