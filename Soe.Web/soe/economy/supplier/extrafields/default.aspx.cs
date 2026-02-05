using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.economy.supplier.extrafields
{
    public partial class _default : PageBase
    {
        protected SoeEntityType Entity = SoeEntityType.None;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Entity = SoeEntityType.Supplier;
            this.Feature = Feature.Common_ExtraFields_Supplier_Edit;

            base.Page_Init(sender, e);
        }
    }
}
