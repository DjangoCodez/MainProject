using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.economy.supplier.trackchanges
{
    public partial class _default : PageBase
    {
        #region Variables

        protected SoeEntityType EntityType = SoeEntityType.Supplier;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Supplier_Suppliers_TrackChanges;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
           //Do nothing
        }
    }
}
