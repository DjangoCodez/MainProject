using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.schedule.template.templates
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Schedule_Templates;
            base.Page_Init(sender, e);
        }
    }
}
