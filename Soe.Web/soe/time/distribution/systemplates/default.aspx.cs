using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.time.distribution.systemplates
{
	public partial class _default : PageBase
	{
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Distribution_SysTemplates;

            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/distribution/systemplates/default.aspx");
        }
	}
}
