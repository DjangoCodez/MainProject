using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/Schedule/TimeScheduleType")]
    public class TimeScheduleTypeController : SoeApiController
    {
        #region Variables

        private readonly TimeScheduleManager tsm;
        #endregion

        #region Constructor

        public TimeScheduleTypeController(TimeScheduleManager tsm)
        {
            this.tsm = tsm;
        }

        #endregion

        #region TimeScheduleTypes

        [HttpGet]
        [Route("Dict/{getAll:bool}/{addEmptyRow:bool}")]
        public IHttpActionResult GetTimeScheduleTypesDict(bool getAll, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTypesDict(base.ActorCompanyId, getAll, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{getAll:bool}/{onlyActive:bool}/{loadFactors:bool}")]
        public IHttpActionResult GetTimeScheduleTypes(bool getAll, bool onlyActive, bool loadFactors)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTypes(base.ActorCompanyId, getAll: getAll, onlyActive: onlyActive, loadFactors: loadFactors).ToSmallDTOs(loadFactors));
        }

        [HttpGet]
        [Route("ScheduleType/{getAll:bool}/{onlyActive:bool}/{loadFactors:bool}/{loadTimeDeviationCauses:bool}")]
        public IHttpActionResult GetScheduleTypes(bool getAll, bool onlyActive, bool loadFactors, bool loadTimeDeviationCauses)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTypes(base.ActorCompanyId, getAll: getAll, onlyActive: onlyActive, loadFactors: loadFactors, loadTimeDeviationCauses: loadTimeDeviationCauses).ToDTOs(loadFactors));
        }

        [HttpGet]
        [Route("ScheduleType/Grid/{timeScheduleTypeId:int?}")]
        public IHttpActionResult GetScheduleTypesForGrid(int? timeScheduleTypeId = null)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTypes(base.ActorCompanyId, getAll: true, onlyActive: false, loadFactors: false, loadTimeDeviationCauses: true, timeScheduleTypeId: timeScheduleTypeId).ToGridDTOs());
        }

        [HttpGet]
        [Route("ScheduleType/{timeScheduleTypeId:int}/{loadFactors:bool}")]
        public IHttpActionResult GetScheduleType(int timeScheduleTypeId, bool loadFactors)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleType(timeScheduleTypeId, loadFactors).ToDTO(loadFactors));
        }

        [HttpPost]
        [Route("ScheduleType")]
        public IHttpActionResult SaveScheduleType(TimeScheduleTypeDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveTimeScheduleType(model, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("ScheduleType/UpdateState")]
        public IHttpActionResult UpdateScheduleTypesState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.UpdateTimeScheduleTypesState(model.Dict));
        }

        [HttpDelete]
        [Route("ScheduleType/{timeScheduleTypeId:int}")]
        public IHttpActionResult DeleteScheduleType(int timeScheduleTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteTimeScheduleType(timeScheduleTypeId));
        }

        #endregion
    }
}