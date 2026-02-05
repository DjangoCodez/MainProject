using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.employee.employees.edit
{
    public partial class _default : PageBase
    {
        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Employee_Employees_Edit;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Redirect("/soe/time/employee/employees/");
        }
    }
}