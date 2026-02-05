using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.project.list
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Project_List;
            base.Page_Init(sender, e);
        }
    }
}
