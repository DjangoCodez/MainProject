using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.contract.edit
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Contract_Contracts_Edit;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {            
            Response.Redirect("/soe/billing/contract/status/?classificationgroup=" + (int)SoeOriginStatusClassificationGroup.HandleContracts);
        }
    }
}
