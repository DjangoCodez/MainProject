using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.economy.accounting.companygroupadministration
{
    public partial class _default : PageBase
    {
        #region Variables
        
        //private AccountManager am = null;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            //this.Feature = Feature.Economy_Accounting_Concern; //TODO materi
            this.Feature = Feature.Economy_Accounting_CompanyGroup_Companies;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Do nothing     
        }
    }
}
