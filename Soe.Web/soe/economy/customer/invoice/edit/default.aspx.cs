using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.economy.customer.invoice.edit
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Customer_Invoice_Invoices_Edit;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("/soe/economy/customer/invoice/status/?classificationgroup=" + (int)SoeOriginStatusClassificationGroup.HandleCustomerInvoices);
        }
    }
}
