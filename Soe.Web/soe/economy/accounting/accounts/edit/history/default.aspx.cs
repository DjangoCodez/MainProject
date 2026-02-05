using System;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.economy.accounting.accounts.edit.history
{
    public partial class _default : PageBase
    {
        protected Account account;
        protected int accountId;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Accounting_Accounts_Edit_History;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Do nothing
        }
    }
}
