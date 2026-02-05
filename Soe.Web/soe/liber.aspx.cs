using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe
{
    public partial class _liber : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.None;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Do nothing
        }
    }
}
