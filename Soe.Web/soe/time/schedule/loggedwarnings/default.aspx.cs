using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.schedule.loggedwarnings
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Schedule_SchedulePlanning_LoggedWarnings;
            base.Page_Init(sender, e);
        }
    }
}
