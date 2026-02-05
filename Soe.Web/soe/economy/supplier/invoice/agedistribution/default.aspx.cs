using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.economy.supplier.invoice.agedistribution
{
    public partial class _default : PageBase
    {
        #region Variables

        protected AccountManager am;
        public int accountYearId;
        public bool accountYearIsOpen;
        public int invoiceType;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.invoiceType = (int)SoeInvoiceType.SupplierInvoice;
            this.Feature = Feature.Economy_Supplier_Invoice_AgeDistribution;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);            
        }
    }
}