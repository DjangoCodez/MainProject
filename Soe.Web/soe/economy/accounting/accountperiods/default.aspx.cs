using System;
using SoftOne.Soe.Common.Util;


namespace SoftOne.Soe.Web.soe.economy.accounting.accountperiods
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Accounting_AccountPeriods;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            
        }
    }
}
