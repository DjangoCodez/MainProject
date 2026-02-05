using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Web.soe.time.time.attestuser
{
    public partial class _default : PageBase
    {
        #region Variables

        private EmployeeManager em;

        protected string subtitle;
        protected TimeAttestMode mode = TimeAttestMode.TimeUser;
        protected Employee employee;
        protected int employeeGroupId;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Time_AttestUser;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            em = new EmployeeManager(ParameterObject);

            #endregion

            this.employee = em.GetEmployeeForUser(UserId, SoeCompany.ActorCompanyId, loadEmployment: true);
            this.employeeGroupId = this.employee?.GetEmployeeGroupId(CalendarUtility.GetBeginningOfMonth(DateTime.Today), CalendarUtility.GetEndOfMonth(DateTime.Today)) ?? 0;
            bool userHasEmployee = this.employee != null && this.employeeGroupId > 0;

            if (userHasEmployee)
            {
                DivSubTitle.Visible = false;
                AngularHost.Visible = true;
            }
            else
            {
                subtitle = GetText(5283, "Ingen anställd kopplad till inloggad användare");
                DivSubTitle.Visible = true;
                AngularHost.Visible = false;
            }
        }
    }
}
