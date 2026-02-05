using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.economy.accounting.voucher.accountdistribution
{
    public partial class _default : PageBase
    {
        #region Variables

        protected AccountManager am = null;
        protected int accountYearId;
        protected int accountDistributionType;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Accounting_AccountDistributionEntry;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {                           
            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out _);                        
            accountDistributionType = (int)SoeAccountDistributionType.Period;           
        }
    }
}
