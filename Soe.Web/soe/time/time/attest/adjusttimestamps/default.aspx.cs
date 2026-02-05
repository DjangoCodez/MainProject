using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.time.attest.adjusttimestamps
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Time_Attest_AdjustTimeStamps;
            base.Page_Init(sender, e);
        }
    }
}
