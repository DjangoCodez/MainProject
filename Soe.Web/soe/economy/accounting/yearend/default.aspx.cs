using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.economy.accounting.yearend
{
    public partial class _default : PageBase
    {
        protected AccountManager am;

        private bool _accountYearIsOpen;
        public bool accountYearIsOpen
        {
            get { return _accountYearIsOpen; }
        }
        private int _accountYearId;
        public int accountYearId
        {
            get { return _accountYearId; }
        }
        private bool _createYear;
        public bool createYear
        {
            get { return _createYear; }
        }
        private int _openAccountYearId;
        public int openAccountYearId
        {
            get { return _openAccountYearId; }
        }

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Accounting_AccountPeriods;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check if year is to be created
            if (QS["createyear"] != null)
                Boolean.TryParse(QS["createyear"], out _createYear);
            else
                _createYear = false;

            _openAccountYearId = string.IsNullOrEmpty(QS["ay"]) ? 0 : Convert.ToInt32(QS["ay"]);

            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out _accountYearId, out _accountYearIsOpen);
        }
    }
}
