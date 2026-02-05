using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.billing.invoice.rut
{
    public partial class _default : PageBase
    {
        #region Variables
        
        private AccountManager am = null;
        public int isRut = 1;
        protected SoeModule TargetSoeModule = SoeModule.Billing;

        public int accountYearId;
        public bool accountYearIsOpen;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Invoice_RUT;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);

            #endregion            
        }
    }
}
