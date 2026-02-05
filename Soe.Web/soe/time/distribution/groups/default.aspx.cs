using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.distribution.groups
{
    public partial class _default : PageBase
	{
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Distribution_Groups;

            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/distribution/groups/default.aspx");
        }
	}
}
