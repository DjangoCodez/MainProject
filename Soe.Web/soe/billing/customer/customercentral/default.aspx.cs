using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.billing.customer.customercentral
{
    public partial class _default : PageBase
    {
        #region Variables        
        private AccountManager am = null;

        public int actorCustomerId = 0;
        public int accountYearId;
        public bool accountYearIsOpen;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Customer_Customers;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (QS["customer"] != null)
                Int32.TryParse(QS["customer"], out actorCustomerId);

            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);            
        }
    }
}
