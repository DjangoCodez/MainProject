using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.economy.customer.invoice.status
{
    public partial class _default : PageBase
    {
        #region Variables

        private AccountManager am = null;
        
        protected SoeModule TargetSoeModule = SoeModule.Economy;
        protected Feature FeatureEdit = Feature.Economy_Customer_Invoice_Status;

        public bool handleCustomerPayments = false;

        public int accountYearId;
        public bool accountYearIsOpen;
        public string invoiceId;
        public string invoiceNr;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Customer_Invoice_Status;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);

            string classificationgroup = QS["classificationgroup"];
            invoiceId = QS["invoiceId"] ?? "-1";
            invoiceNr = QS["invoiceNr"] ?? "-1";

            if (classificationgroup != null && int.Parse(classificationgroup) == (int)SoeOriginStatusClassificationGroup.HandleCustomerPayments )
            {
                FeatureEdit = Feature.Economy_Customer_Payment;
                handleCustomerPayments = true;
            }            
        }
    }
}
