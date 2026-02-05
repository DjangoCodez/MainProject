using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.billing.product.extrafields
{
    public partial class _default : PageBase
    {
        protected SoeEntityType Entity = SoeEntityType.None;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Entity = SoeEntityType.InvoiceProduct;
            this.Feature = Feature.Billing_Product_Products_ExtraFields;

            base.Page_Init(sender, e);
        }
    }
}
