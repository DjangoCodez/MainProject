using System;
using System.Collections.Generic;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Web.soe.economy.supplier.suppliers
{
    public partial class _default : PageBase
    {
        #region Variables

        public int supplierId; //NOSONAR

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Supplier_Suppliers;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            string classificationgroup = QS["classificationgroup"];
            Int32.TryParse(QS["supplierId"], out supplierId);
        }
    }
}
