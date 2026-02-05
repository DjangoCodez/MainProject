using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.DTO;
using System.Web.Http;
using SoftOne.Soe.Common.Util;
using System.Net;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/Schedule")]
    public class ScheduleCycleController: SoeApiController
    {
        #region Variables

        private readonly TimeScheduleManager tsm;

        #endregion

        #region Constructor
        public ScheduleCycleController(TimeScheduleManager tsm)
        {
            this.tsm = tsm;
        }
        #endregion

        #region ScheduleCycleRuleType
        [HttpGet]
        [Route("ScheduleCycleRuleType/Grid/{scheduleCycleRuleTypeId:int?}")]
        public IHttpActionResult GetScheduleCycleRuleTypesGrid(int? scheduleCycleRuleTypeId = null)
        {
            return Content(HttpStatusCode.OK,
                tsm.GetScheduleCycleRuleTypes(base.ActorCompanyId, scheduleCycleRuleTypeId).ToGridDTOs());
        }

        [HttpGet]
        [Route("ScheduleCycleRuleType/{scheduleCycleRuleTypeId:int}")]
        public IHttpActionResult GetScheduleCycleRuleType(int scheduleCycleRuleTypeId)
        {
            return Content(HttpStatusCode.OK,
                tsm.GetScheduleCycleRuleType(scheduleCycleRuleTypeId).ToDTO());
        }

        [HttpPost]
        [Route("ScheduleCycleRuleType")]
        public IHttpActionResult SaveScheduleCycleRuleType(ScheduleCycleRuleTypeDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);

            return Content(HttpStatusCode.OK,
                tsm.SaveScheduleCycleRuleType(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("ScheduleCycleRuleType/{scheduleCycleRuleTypeId:int}")]
        public IHttpActionResult DeleteScheduleCycleRuleType(int scheduleCycleRuleTypeId)
        {
            return Content(HttpStatusCode.OK,
                tsm.DeleteScheduleCycleRuleType(scheduleCycleRuleTypeId));
        }

        [HttpGet]
        [Route("ScheduleCycleRuleType/Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetScheduleCycleRuleTypesDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK,
                tsm.GetScheduleCycleRuleTypesDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }
        #endregion

        #region ScheduleCycle

        [HttpGet]
        [Route("ScheduleCycle/Grid/{scheduleCycleId:int?}")]
        public IHttpActionResult GetScheduleCyclesGrid(int? scheduleCycleId = null)
        {
            return Content(HttpStatusCode.OK,
                tsm.GetScheduleCycles(base.ActorCompanyId, scheduleCycleId).ToGridDTOs());
        }

        [HttpGet]
        [Route("ScheduleCycle/Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetScheduleCyclesDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK,
                tsm.GetScheduleCyclesDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("ScheduleCycle/{scheduleCycleId:int}")]
        public IHttpActionResult GetScheduleCycle(int scheduleCycleId)
        {
            return Content(HttpStatusCode.OK,
                tsm.GetScheduleCycleWithRules(scheduleCycleId).ToDTO());
        }

        [HttpPost]
        [Route("ScheduleCycle")]
        public IHttpActionResult SaveScheduleCycle(ScheduleCycleDTO scheduleCycle)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            return Content(HttpStatusCode.OK,
                tsm.SaveScheduleCycle(scheduleCycle, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("ScheduleCycle/{scheduleCycleId:int}")]
        public IHttpActionResult DeleteScheduleCycle(int scheduleCycleId)
        {
            return Content(HttpStatusCode.OK,
                tsm.DeleteScheduleCycle(scheduleCycleId));
        }

        #endregion
    }
}
