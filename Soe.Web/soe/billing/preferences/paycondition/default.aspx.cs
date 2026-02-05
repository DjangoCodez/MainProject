using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.preferences.paycondition
{
    public partial class _default : PageBase
    {
        protected SoeModule TargetSoeModule = SoeModule.None;
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Preferences_PayCondition;
            TargetSoeModule = SoeModule.Billing;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Do nothing
        }
    }
}
