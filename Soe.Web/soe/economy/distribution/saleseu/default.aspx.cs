using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.economy.distribution.saleseu
{
    public partial class _default : PageBase
    {
        #region Variables
        public int accountYearId;
        public bool accountYearIsOpen;
        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Export_SalesEU;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);
        }
    }
}
