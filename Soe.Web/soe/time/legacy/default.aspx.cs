using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.legacy
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time;
            base.Page_Init(sender, e);
        }
    }
}
