using System;
using SoftOne.Soe.Common.Util;
namespace SoftOne.Soe.Web.soe.billing.order.planninguser
{
    public partial class _default : PageBase
    {
        #region Variables

        protected string subtitle;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Order_PlanningUser;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
          //Do nothing
        }
    }
}
