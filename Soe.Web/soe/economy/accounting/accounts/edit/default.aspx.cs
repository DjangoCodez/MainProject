using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.economy.accounting.accounts.edit
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Accounting_Accounts_Edit;
            
            //Add parameters
            Context.Items["Feature"] = this.Feature;

            Server.Transfer("/soe/common/account/default.aspx");
        }
    }
}
