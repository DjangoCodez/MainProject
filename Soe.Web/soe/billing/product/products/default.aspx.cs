using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.product.products
{
    public partial class _default : PageBase
    {
        public int productId = 0;
        public string productNr;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Product_Products;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Legacy Navigation

            Int32.TryParse(QS["productId"], out productId);
            productNr = QS["productNr"] ?? "";

            #endregion
        }
    }
}
