using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.time.payroll.worktimeaccount
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Payroll_TimeWorkAccount;
            base.Page_Init(sender, e);
        }
    }
}
