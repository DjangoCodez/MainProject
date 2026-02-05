using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.common.extrafields
{
    public partial class _default : PageBase
    {
        protected SoeEntityType Entity = SoeEntityType.None;

        protected override void Page_Init(object sender, EventArgs e)
        {
            //Set variables to reuse page with different contet
            EnableModuleSpecifics();

            base.Page_Init(sender, e);
        }

        private void EnableModuleSpecifics()
        {
            if (CTX["entity"] != null)
            {
                this.Entity = (SoeEntityType)CTX["entity"];
                switch (this.Entity)
                {
                    case SoeEntityType.InvoiceProduct:
                        Feature = Feature.Billing_Product_Products_ExtraFields;
                        break;
                }
            }
        }
    }
}
