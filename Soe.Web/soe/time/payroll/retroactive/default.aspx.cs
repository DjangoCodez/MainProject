using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.payroll.retroactive
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Payroll_Retroactive;
            base.Page_Init(sender, e);
        }
    }
}
