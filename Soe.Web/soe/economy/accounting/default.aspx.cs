using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.economy.accounting
{
    public partial class _default : PageBase
	{
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Accounting;
            base.Page_Init(sender, e);
        }
	}
}
