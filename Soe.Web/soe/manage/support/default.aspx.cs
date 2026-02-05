using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.manage.support
{
	public partial class _default : PageBase
	{
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Manage_Support;
            base.Page_Init(sender, e);
        }
	}
}
