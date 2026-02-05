using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.distribution.reports.selection.preview
{
	public partial class _default : PageBase
	{
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Distribution_Reports_Selection_Preview;

            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/distribution/reports/selection/download/default.aspx");
        }
	}
}
