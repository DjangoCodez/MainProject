using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.manage.system.intrastat.commoditycodes
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_System_Intrastat_StatisticalCommodityCodes;
            base.Page_Init(sender, e);
        }
    }
}
