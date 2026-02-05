using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.economy.supplier.invoice.status
{
    public partial class _default : PageBase
    {
        #region Variables

        private AccountManager am = null;        
        public bool handleSupplierPayments; //NOSONAR
        public int accountYearId; //NOSONAR
        public bool accountYearIsOpen; //NOSONAR
        public int invoiceId; //NOSONAR
        public int paymentId; //NOSONAR

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Supplier_Invoice_Status;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);

            string classificationgroup = QS["classificationgroup"];
            Int32.TryParse(QS["invoiceId"], out invoiceId);
            Int32.TryParse(QS["paymentId"], out paymentId);
            if (classificationgroup != null && Int32.Parse(classificationgroup) == (int)SoeOriginStatusClassificationGroup.HandleSupplierPayments)
                handleSupplierPayments = true;           
        }
    }
}

