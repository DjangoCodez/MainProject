using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using SoftOne.Soe.Business.Core;
using SoftOne;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace SoftOne.Soe.Web.soe.billing.project.attest
{
    public partial class _default : PageBase
    {
        #region Variables

        public TimeAttestMode mode;
        public Employee employee;

        private EmployeeManager em;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Billing_Project_Attest;
            this.mode = TimeAttestMode.Project;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            em = new EmployeeManager(ParameterObject);
            this.employee = em.GetEmployeeForUser(UserId, SoeCompany.ActorCompanyId);
        }
    }
}
