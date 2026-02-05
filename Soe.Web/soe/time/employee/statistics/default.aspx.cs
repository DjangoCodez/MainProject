using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Web.soe.time.employee.statistics
{
    public partial class _default : PageBase
    {
        #region Variables

        private EmployeeManager em;
        protected Employee employee;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Employee_Statistics;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            em = new EmployeeManager(ParameterObject);

            #endregion

            employee = em.GetEmployeeForUser(UserId, SoeCompany.ActorCompanyId);
            
        }
    }
}
