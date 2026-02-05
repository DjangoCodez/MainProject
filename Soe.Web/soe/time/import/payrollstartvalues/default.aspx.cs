using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.import.payrollstartvalues
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Import_PayrollStartValuesImported;
            base.Page_Init(sender, e);
        }
    }
}