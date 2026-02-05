using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.economy.export.customerspecificexports
{
    public partial class _default : PageBase
    {
        #region Variables
        public int accountYearId;
        public bool accountYearIsOpen;
        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Export_CustomerSpecific;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);
        }
    }
}
