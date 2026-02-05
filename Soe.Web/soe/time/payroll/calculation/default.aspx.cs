using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Web.soe.time.payroll.calculation
{
    public partial class _default : PageBase
    {
        #region Variables

        protected Employee employee;

        private EmployeeManager em;        
        protected Boolean isAdmin;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Payroll_Calculation;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            em = new EmployeeManager(ParameterObject);            
            isAdmin = base.IsAdmin;

            #endregion

            this.employee = em.GetEmployeeForUser(UserId, SoeCompany.ActorCompanyId);
               
        }
    }
}