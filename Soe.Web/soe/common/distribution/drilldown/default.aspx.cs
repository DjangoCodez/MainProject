using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.common.distribution.drilldown
{
    public partial class _default : PageBase
    {
        #region Variables

        private AccountManager am = null;
        public int accountYearId; //NOSONAR
        public bool accountYearIsOpen; //NOSONAR
        public string accountYearLastDate; //NOSONAR

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Distribution_DrillDownReports;
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