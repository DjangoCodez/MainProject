using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.clientmanagement.suppliers.invoices.overview
{
    public partial class _default : PageBase
	{
		protected override void Page_Init(object sender, EventArgs e)
		{
			this.Feature = Feature.ClientManagement_Supplier_Invoices;
			base.Page_Init(sender, e);
		}
    }
}