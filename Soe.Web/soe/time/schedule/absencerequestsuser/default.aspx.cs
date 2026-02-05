using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;

namespace SoftOne.Soe.Web.soe.time.schedule.absencerequestsuser
{
    public partial class _default : PageBase
    {
        #region Variables

        private EmployeeManager em;
        protected string subtitle;
        protected TermGroup_EmployeeRequestType requestType = TermGroup_EmployeeRequestType.AbsenceRequest;
        protected TimeSchedulePlanningDisplayMode displayMode = TimeSchedulePlanningDisplayMode.User;
        protected Employee employee;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Schedule_AbsenceRequestsUser;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            DivSubTitle.Visible = false;
            em = new EmployeeManager(ParameterObject);
            employee = em.GetEmployeeForUser(UserId, SoeCompany.ActorCompanyId);
            if (employee == null)
            {
                subtitle = GetText(5283, "Ingen anställd kopplad till inloggad användare");
                DivSubTitle.Visible = true;
            }            
        }
    }
}