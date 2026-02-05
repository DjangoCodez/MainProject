using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.manage.system.syscompany.syscompserver
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_System;
            base.Page_Init(sender, e);
        }
    }
}
