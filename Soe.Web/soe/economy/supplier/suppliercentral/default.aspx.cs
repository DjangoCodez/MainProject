using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.economy.supplier.suppliercentral
{
    public partial class _default : PageBase
    {
        #region Variables        
        private AccountManager am = null;

        public int supplierId = 0;
        public int accountYearId;
        public bool accountYearIsOpen;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Supplier_Suppliers;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (QS["supplier"] != null)
                Int32.TryParse(QS["supplier"], out supplierId);

            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);         
        }
    }
}

