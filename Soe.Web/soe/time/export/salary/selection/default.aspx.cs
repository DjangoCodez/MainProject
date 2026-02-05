using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.export.selection.salary
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Export_Salary;
            base.Page_Init(sender, e);
        }
    }
}
