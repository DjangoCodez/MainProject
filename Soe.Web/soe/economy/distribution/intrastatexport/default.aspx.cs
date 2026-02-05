using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.economy.distribution.intrastatexport
{
    public partial class _default : PageBase
    {
        protected AccountManager am = null;
        protected int accountYearId;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Economy_Intrastat_ReportsAndExport;

            base.Page_Init(sender, e);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out _);
        }
    }
}
