using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.time.payroll.accountprovision.accountprovisionbase
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Payroll_Provision_AccountProvisionBase;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Do nothing
        }
    }
}
