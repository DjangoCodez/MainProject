using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.time.payroll.massregistration
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Payroll_MassRegistration;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Do nothing
        }
    }
}
