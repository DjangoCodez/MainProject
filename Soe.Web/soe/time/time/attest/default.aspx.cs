using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Web.soe.time.time.attest
{
    public partial class _default : PageBase
    {
        #region Variables

        protected TimeAttestMode mode = TimeAttestMode.Time;
        protected Employee employee;
        protected EmployeeGroup employeeGroup;
        protected bool isAdmin;
        private EmployeeManager em;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Time_Attest;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            em = new EmployeeManager(ParameterObject);
            isAdmin = base.IsAdmin;

            this.employee = em.GetEmployeeForUser(UserId, SoeCompany.ActorCompanyId, loadEmployment: true);
            this.employeeGroup = this.employee?.GetEmployeeGroup(null, null);
        }
    }
}
