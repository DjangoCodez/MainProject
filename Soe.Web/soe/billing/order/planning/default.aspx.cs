using System;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace SoftOne.Soe.Web.soe.billing.order.planning
{
    public partial class _default : PageBase
    {
        #region Variables        
        private EmployeeManager em;
        protected Employee employee;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Order_Planning;
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
