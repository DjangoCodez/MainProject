using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Globalization;

namespace SoftOne.Soe.Web.soe.time.schedule.planning.scenario
{
    public partial class _default : PageBase
    {
        #region Variables

        private EmployeeManager em;
        protected Employee employee;
        protected Boolean isAdmin;
        protected DateTime startDate = DateTime.Today;

        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            if (!HasRolePermission(Feature.Time_Schedule_SchedulePlanning_ScenarioDayView, Permission.Readonly) && !HasRolePermission(Feature.Time_Schedule_SchedulePlanning_ScenarioScheduleView, Permission.Readonly))
                RedirectToUnauthorized(UnauthorizationType.FeaturePermissionMissing);

            this.Feature = Feature.Time_Schedule_SchedulePlanning;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            #region Init

            em = new EmployeeManager(ParameterObject);
            isAdmin = base.IsAdmin;

            string date = QS["date"];
            if (date != null)
                DateTime.TryParse(date, CultureInfo.CurrentUICulture, DateTimeStyles.AssumeLocal, out startDate);

            #endregion

            employee = em.GetEmployeeForUser(UserId, SoeCompany.ActorCompanyId, loadEmployment: true);
        }
    }
}
