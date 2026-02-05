using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.economy.supplier.invoice.attest.overview
{
    public partial class _default : PageBase
    {
        #region Variables
        
        private AccountManager am = null;
        public int accountYearId; //NOSONAR
        public bool accountYearIsOpen; //NOSONAR

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {            
            this.Feature = Feature.Economy_Supplier_Invoice_AttestFlow_Overview; 
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);           
        }
    }
}