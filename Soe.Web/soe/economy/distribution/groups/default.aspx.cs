using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.economy.distribution.groups
{
	public partial class _default : PageBase
	{
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Distribution_Groups;

            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/distribution/groups/default.aspx");
        }
	}
}
