using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.time.schedule
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Schedule;
            base.Page_Init(sender, e);
        }
    }
}
