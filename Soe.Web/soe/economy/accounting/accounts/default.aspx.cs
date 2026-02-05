using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Web.soe.economy.accounting.accounts
{
    public partial class _default : PageBase
    {
        protected AccountManager am;

        protected AccountDim accountDim;
        protected int accountDimId;
        protected int accountYearId;
        protected bool isStdAccount;
        protected AccountStd accountStd;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Accounting_Accounts;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Mandatory parameters
            if (!Int32.TryParse(QS["dim"], out accountDimId))
            {
                Response.Redirect("/Soe/Economy/Accounting/default.aspx");
                //throw new SoeQuerystringException("dim", this.ToString());
            }

            am = new AccountManager(ParameterObject);
            accountDim = am.GetAccountDim(accountDimId, SoeCompany.ActorCompanyId);

            isStdAccount = accountDim?.AccountDimNr == Constants.ACCOUNTDIM_STANDARD;
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out _);            
        }
    }
}
