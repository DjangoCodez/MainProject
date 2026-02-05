using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Web.soe.billing.project.timesheetuser
{
    public partial class _default : PageBase
    {
        #region Variables

        protected string subtitle;
        protected int employeeId = 0;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Project_TimeSheetUser;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            bool noEmployee = false;
            DivSubTitle.Visible = false;

            EmployeeManager em = new EmployeeManager(ParameterObject);
            Employee employee = em.GetEmployeeForUser(UserId, SoeCompany.ActorCompanyId);
            if (employee == null)
                noEmployee = true;
            else
                employeeId = employee.EmployeeId;

            if (noEmployee)
            {
                subtitle = GetText(5283, "Ingen anställd kopplad till inloggad användare");
                DivSubTitle.Visible = true;
            }
        }
    }
}
