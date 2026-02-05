using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.billing.contract.status
{
    public partial class _default : PageBase
    {
        #region Variables

        private AccountManager am;
        protected SoeModule TargetSoeModule = SoeModule.Billing;
        protected Feature FeatureEdit = Feature.Billing_Contract_Status;
        public int employeeId = 0;
        public int accountYearId;
        public bool accountYearIsOpen;
        public int invoiceId = -1;
        public string invoiceNr;


        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Contract_Status;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);

            #endregion

            #region Legacy Navigation

            //Int32.TryParse(QS["invoiceId"], out invoiceId);
            invoiceId = string.IsNullOrEmpty(QS["invoiceId"]) ? -1 : Int32.Parse(QS["invoiceId"]);
            invoiceNr = QS["invoiceNr"] ?? "-1";

            #endregion
        }
    }
}
