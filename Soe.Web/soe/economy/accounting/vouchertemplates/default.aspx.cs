using System;
using System.Collections.Generic;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.economy.accounting.vouchertemplates
{
    public partial class _default : PageBase
    {
        #region Variables

        private AccountManager am = null;

        public int accountYearId;
        public bool accountYearIsOpen;
        public string accountYearLastDate;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Accounting_VoucherTemplateList;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);
            accountYearLastDate = (CurrentAccountYear?.To ?? DateTime.Today).ToShortDateString();
            
        }
    }
}
