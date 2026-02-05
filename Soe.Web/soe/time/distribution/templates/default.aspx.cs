using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.time.distribution.templates
{
	public partial class _default : PageBase
	{
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Distribution_Templates;

            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/distribution/templates/default.aspx");
        }
	}
}
