using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.billing.purchase.list
{
    public partial class _default : PageBase
    {
        public int purchaseId = 0;
        public string purchaseNr;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Purchase_Purchase_List;

            #region Legacy Navigation

            Int32.TryParse(QS["purchaseId"], out purchaseId);
            purchaseNr = QS["purchaseNr"] ?? "";

            #endregion

            base.Page_Init(sender, e);
        }
    }
}
