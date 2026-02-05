using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.economy.supplier.invoice
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Supplier_Invoice;
            base.Page_Init(sender, e);
        }
    }
}
