using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/Schedule/WorkRuleBypassLog")]
    public class WorkRuleBypassLogController : SoeApiController
    {
        #region Variables

        private readonly TimeScheduleManager tsm;

        #endregion

        #region Constructor

        public WorkRuleBypassLogController(TimeScheduleManager tsm)
        {
            this.tsm = tsm;
        }

        #endregion

        #region WorkRuleBypassLog

        [HttpGet]
        [Route("Grid/{dateSelection:int}/{workRuleBypassLogId:int?}")]
        public IHttpActionResult GetWorkRuleBypassLogGrid(int dateSelection, int? workRuleBypassLogId = null)
        {
            var allItemsSelection = (TermGroup_ChangeStatusGridAllItemsSelection)dateSelection;
            var logs = tsm.GetWorkRulebypassLog(base.ActorCompanyId, base.UserId, base.RoleId, allItemsSelection, setActionText: true, workRuleBypassLogId: workRuleBypassLogId);
            return Content(HttpStatusCode.OK, logs.ToGridDTOs());
        }

        #endregion
    }
}
