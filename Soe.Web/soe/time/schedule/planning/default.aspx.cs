using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.time.schedule.planning
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Schedule_SchedulePlanning;

            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/time/schedule/planning/schedule/default.aspx");
        }     
    }
}
