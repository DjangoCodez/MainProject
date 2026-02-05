using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.economy.accounting.balance
{
    public partial class _default : PageBase
    {
        protected AccountManager am;

        public bool accountYearIsOpen; //NOSONAR
        public int accountYearId; //NOSONAR

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Accounting_Balance;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);
        }
    }
}
