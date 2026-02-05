using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.manage.attest.time
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Attest_Time;
            base.Page_Init(sender, e);
        }
    }
}
