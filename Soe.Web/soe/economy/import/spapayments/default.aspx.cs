using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.economy.import.spapayments
{
    public partial class _default :  PageBase
    {
        #region Variables

        private AccountManager am = null;
        public int accountYearId;
        public bool accountYearIsOpen;
        public int importType;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Import_Payments_Supplier;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);
            importType = (int)ImportPaymentType.SupplierPayment;
        }
    }
}