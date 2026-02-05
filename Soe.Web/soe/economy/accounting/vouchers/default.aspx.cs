using System;
using System.Collections.Generic;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.economy.accounting.vouchers
{
    public partial class _default : PageBase
    {
        #region Variables

        private AccountManager am = null;

        public int accountYearId;
        public bool accountYearIsOpen;
        public string accountYearLastDate;
        public string accountYearFirstDate;
        public int voucherHeadId; 
        public string voucherNr;
        public bool openNewVoucher = false;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Accounting_Vouchers;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            am = new AccountManager(ParameterObject);

            // Open Voucher through QS (ex: quicksearch)
            Int32.TryParse(QS["voucherHeadId"], out voucherHeadId);
            voucherNr = QS["voucherNr"] ?? "0";
            bool.TryParse(QS["new"], out openNewVoucher);


            #endregion

            // Get AccountYear info
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);
            accountYearLastDate = (CurrentAccountYear?.To ?? DateTime.Today).ToShortDateString();
            accountYearFirstDate = (CurrentAccountYear?.From ?? DateTime.Today).ToShortDateString();            
        }
    }
}
