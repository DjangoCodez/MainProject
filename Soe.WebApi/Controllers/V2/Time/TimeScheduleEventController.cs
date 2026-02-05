using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/Schedule/TimeScheduleEvent")]
    public class TimeScheduleEventController : SoeApiController
    {
        #region Variables

        private readonly TimeScheduleManager tsm;

        #endregion

        #region Constructor

        public TimeScheduleEventController(TimeScheduleManager tsm)
        {
            this.tsm = tsm;
        }

        #endregion

        #region TimeScheduleEvent

        [HttpGet]
        [Route("Grid/{timeScheduleEventId:int?}")]
        public IHttpActionResult GetTimeScheduleEventsGrid(int? timeScheduleEventId=null)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleEvents(base.ActorCompanyId,timeScheduleEventId).ToGridDTOs());
        }

        [HttpGet]
        [Route("Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetTimeScheduleEventsDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleEventsDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("TimeScheduleEvent/{timeScheduleEventId:int}")]
        public IHttpActionResult GetTimeScheduleEvent(int timeScheduleEventId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleEvent(timeScheduleEventId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("TimeScheduleEvent")]
        public IHttpActionResult SaveTimeScheduleEvent(TimeScheduleEventDTO timeScheduleEvent)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveTimeScheduleEvent(timeScheduleEvent, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("TimeScheduleEvent/{timeScheduleEventId:int}")]
        public IHttpActionResult DeleteTimeScheduleEvent(int timeScheduleEventId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteTimeScheduleEvent(timeScheduleEventId, base.ActorCompanyId));
        }

        #endregion
    }
}