using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.payroll.calculationuser
{
    public partial class _default : PageBase
    {
        #region Variables

        protected string subtitle;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Payroll_CalculationUser;
            base.Page_Init(sender, e);
        }
    }
}