using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

using Soe.WebApi.Models;
using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/TimePeriod")]
    public class TimePeriodController : SoeApiController
    {
        #region Variables

        private readonly TimePeriodManager tpm;
        private readonly PayrollManager pm;
        #endregion

        #region Constructor

        public TimePeriodController(TimePeriodManager tpm, PayrollManager pm)
        {
            this.tpm = tpm;
            this.pm = pm;
        }

        #endregion

        #region TimePeriodHead
        [HttpGet]
        [Route("TimePeriodHeadsDict/{type:int}/{accountId:int}/{addEmptyRow:bool?}")]
        public IHttpActionResult GetTimePeriodHeadsDict(int type, int accountId, bool addEmptyRow = false)
        {
            return Content(HttpStatusCode.OK, tpm.GetTimePeriodHeadsDict(base.ActorCompanyId, (TermGroup_TimePeriodType)type, addEmptyRow, accountId != 0 ? accountId : (int?)null).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("PlanningPeriod/Grid/{periodHeadId:int?}")]
        public IHttpActionResult GetPlanningPeriodGrid(int? timePeriodHeadId = null)
        {
            return Content(HttpStatusCode.OK, tpm.GetTimePeriodHeads(base.ActorCompanyId, TermGroup_TimePeriodType.RuleWorkTime, false, true, null, true, timePeriodHeadId: timePeriodHeadId).ToGridDTOs());

        }
        [HttpGet]
        [Route("TimePeriodHead/{periodHeadId:int?}")]
        public IHttpActionResult GetTimePeriodHead(int timePeriodHeadId, bool loadPeriods = false)
        {
            return Content(HttpStatusCode.OK, tpm.GetTimePeriodHead(timePeriodHeadId, base.ActorCompanyId, loadPeriods).ToDTO(true));
        }

        [HttpPost]
        [Route("TimePeriodHead/")]
        public IHttpActionResult SaveTimePeriod(SaveTimePeriodHeadModel model)
        {
            return Content(HttpStatusCode.OK, tpm.SaveTimePeriodHead(model.TimePeriodHead, base.ActorCompanyId, model.RemovePeriodLinks));
        }

        [HttpDelete]
        [Route("TimePeriodHead/{timePeriodHeadId:int}/{removePeriodLinks:bool}")]
        public IHttpActionResult DeleteTimePeriodHead(int timePeriodHeadId, bool removePeriodLinks)
        {
            return Content(HttpStatusCode.OK, tpm.DeleteTimePeriodHead(timePeriodHeadId, base.ActorCompanyId, removePeriodLinks));
        }

        [HttpGet]
        [Route("TimePeriodHeadIncPeriods/{type:int}")]
        public IHttpActionResult GetTimePeriodHeadsIncludingPeriodsForType(TermGroup_TimePeriodType type)
        {
            return Content(HttpStatusCode.OK, tpm.GetTimePeriodHeadsIncludingPeriodsForType(base.ActorCompanyId, type).ToDTOs(true));
        }
        #endregion

        #region TimePeriod
        [HttpGet]
        [Route("TimePeriodsDict/{timePeriodHeadId:int}/{addEmptyRow:bool?}")]
        public IHttpActionResult GetTimePeriodsDict(int timePeriodHeadId, bool addEmptyRow = false)
        {
           return Content(HttpStatusCode.OK, tpm.GetTimePeriodsDict(timePeriodHeadId, addEmptyRow, base.ActorCompanyId).ToSmallGenericTypes());
        }
        #endregion

        #region DistributionRules

        [HttpGet]
        [Route("PlanningPeriod/DistributionRules/Grid/{payrollProductDistributionRuleHeadId:int?}")]
        public IHttpActionResult GetDistributionRulesForGrid(int? payrollProductDistributionRuleHeadId = null)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollProductDistributionRuleHeads(base.ActorCompanyId, payrollProductDistributionRuleHeadId: payrollProductDistributionRuleHeadId).ToGridDTOs());

        }

        [HttpGet]
        [Route("PlanningPeriod/DistributionRule/Rule/{headId:int}")]
        public IHttpActionResult GetDistributionRuleHead(int headId)
        {
            return Content(HttpStatusCode.OK, pm.GetPayrollProductDistributionRuleHead(base.ActorCompanyId, headId, loadRules: true).ToDTO());

        }
        [HttpPost]
        [Route("PlanningPeriod/DistributionRule/Save")]
        public IHttpActionResult SaveDistributionRuleHead(SavePayrollProductDistributionRuleHeadModel model)
        {
            return Content(HttpStatusCode.OK, pm.SavePayrollProductDistributionRuleHead(model.PayrollProductDistributionRuleHead, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("PlanningPeriod/DistributionRule/Delete")]
        public IHttpActionResult DeleteDistributionRuleHead(int headId)
        {
            return Content(HttpStatusCode.OK, pm.DeletePayrollProductDistributionRuleHead(headId, base.ActorCompanyId));
        }
        #endregion
    }
}