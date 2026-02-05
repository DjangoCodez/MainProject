using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.employee.extrafields
{
    public partial class _default : PageBase
    {
        protected SoeEntityType Entity = SoeEntityType.None;

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Entity = SoeEntityType.Employee;
            this.Feature = Feature.Common_ExtraFields_Employee_Edit;

            base.Page_Init(sender, e);
        }
    }
}
