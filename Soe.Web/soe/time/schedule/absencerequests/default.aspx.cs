using SoftOne.Soe.Common.Util;
using System;

namespace SoftOne.Soe.Web.soe.time.schedule.absencerequests
{
    public partial class _default : PageBase
    {
        #region Variables

        protected string subtitle;

        protected TermGroup_EmployeeRequestType requestType = TermGroup_EmployeeRequestType.AbsenceRequest;
        protected TimeSchedulePlanningDisplayMode displayMode = TimeSchedulePlanningDisplayMode.Admin;

        public int employeeRequestId;
        #endregion

        protected override void Page_Init(object sender, EventArgs e)
        {
            this.Feature = Feature.Time_Schedule_AbsenceRequests;
            base.Page_Init(sender, e);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            Int32.TryParse(QS["employeeRequestId"], out employeeRequestId);
        }
    }
}