using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Web.soe.billing.project.list
{
    public partial class _default : PageBase
    {
        #region Variables

        protected AccountManager am;
        public int accountYearId;
        public bool accountYearIsOpen;
        public int employeeId = 0;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Project_List;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            am = new AccountManager(ParameterObject);
            am.GetAccountYearInfo(CurrentAccountYear, out accountYearId, out accountYearIsOpen);

            EmployeeManager em = new EmployeeManager(ParameterObject);
            Employee employee = em.GetEmployeeForUser(UserId, SoeCompany.ActorCompanyId);
            if (employee != null)
                employeeId = employee.EmployeeId;
        }
    }
}
