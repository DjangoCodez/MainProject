using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time")]
    public class TimeScheduleTaskController : SoeApiController
    {
        #region Variables

        private readonly CalendarManager cm;
        private readonly TimeScheduleManager tsm;

        #endregion

        #region Constructor

        public TimeScheduleTaskController(CalendarManager cm, TimeScheduleManager tsm)
        {
            this.cm = cm;
            this.tsm = tsm;
        }

        #endregion

        #region TimeScheduleTask

        [HttpGet]
        [Route("TimeScheduleTask/Grid")]
        public IHttpActionResult GetTimeScheduleTasksGrid(int? timeScheduleTaskId = null)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTasks(base.ActorCompanyId, true, true, true, false, timeScheduleTaskId).ToGridDTOs());
        }

        [HttpGet]
        [Route("TimeScheduleTask/Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetTimeScheduleTasksDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTasksDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("TimeScheduleTask/{timeScheduleTaskId:int}/{loadAccounts:bool}/{loadExcludedDates:bool}/{loadAccountHierarchyAccount:bool}")]
        public IHttpActionResult GetTimeScheduleTask(int timeScheduleTaskId, bool loadAccounts, bool loadExcludedDates, bool loadAccountHierarchyAccount)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTask(timeScheduleTaskId, base.ActorCompanyId, loadAccounts: loadAccounts, loadExcludedDates: loadExcludedDates, loadAccountHierarchyAccount: loadAccountHierarchyAccount, onlyActive: false).ToDTO(loadAccounts));
        }

        [HttpPost]
        [Route("TimeScheduleTask")]
        public IHttpActionResult SaveTimeScheduleTask(TimeScheduleTaskDTO timeScheduleTask)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveTimeScheduleTask(timeScheduleTask, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("TimeScheduleTask/{timeScheduleTaskId:int}")]
        public IHttpActionResult DeleteTimeScheduleTask(int timeScheduleTaskId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteTimeScheduleTask(timeScheduleTaskId, base.ActorCompanyId));
        }

        #endregion

        #region TimeScheduleTaskType

        [HttpGet]
        [Route("TimeScheduleTaskType/Grid")]
        public IHttpActionResult GetTimeScheduleTaskTypesGrid(int? timeScheduleTaskTypeId = null)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTaskTypes(base.ActorCompanyId, timeScheduleTaskTypeId).ToGridDTOs());
        }

        [HttpGet]
        [Route("TimeScheduleTaskType/Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetTimeScheduleTaskTypesDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTaskTypesDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("TimeScheduleTaskType/{timeScheduleTaskTypeId:int}")]
        public IHttpActionResult GetTimeScheduleTaskType(int timeScheduleTaskTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTaskType(timeScheduleTaskTypeId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("TimeScheduleTaskType")]
        public IHttpActionResult SaveTimeScheduleTaskType(TimeScheduleTaskTypeDTO timeScheduleTaskType)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveTimeScheduleTaskType(timeScheduleTaskType, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("TimeScheduleTaskType/{timeScheduleTaskTypeId:int}")]
        public IHttpActionResult DeleteTimeScheduleTaskType(int timeScheduleTaskTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteTimeScheduleTaskType(timeScheduleTaskTypeId, base.ActorCompanyId));
        }

        #endregion

        #region Recurrence

        [HttpGet]
        [Route("Recurrence/Description/{pattern}")]
        public IHttpActionResult GetRecurrenceDescription(string pattern)
        {
            return Content(HttpStatusCode.OK, DailyRecurrencePatternDTO.GetRecurrenceDescription(pattern, base.GetTermGroupContent(TermGroup.RecurrencePattern), 1, cm.GetSysHolidayTypeDTOs()));
        }

        #endregion
    }
}