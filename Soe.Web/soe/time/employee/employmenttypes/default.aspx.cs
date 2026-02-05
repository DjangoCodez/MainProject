using System;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.time.employee.employmenttypes
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Employee_EmploymentTypes;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {           
            //Do nothing
        }
    }
}
