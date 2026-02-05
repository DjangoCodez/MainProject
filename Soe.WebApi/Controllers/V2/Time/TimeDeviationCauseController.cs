using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.DTO;
using System;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/TimeDeviationCause")]
    public class TimeDeviationCauseController : SoeApiController
    {
        #region Variables

        private readonly TimeDeviationCauseManager tdcm;

        #endregion

        #region Constructor

        public TimeDeviationCauseController(TimeDeviationCauseManager tdcm)
        {
            this.tdcm = tdcm;
        }

        #endregion

        #region TimeDeviationCause

        [HttpGet] //GetGrid
        [Route("Grid/{timeDeviationCauseId:int?}")]
        public IHttpActionResult GetTimeDeviationCausesGrid(int? timeDeviationCauseId = null)
        {
            return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCauses(base.ActorCompanyId, timeDeviationCauseId: timeDeviationCauseId, sortByName: true, loadTimeCode: true, setTimeDeviationTypeName: true).ToGridDTOs());
        }


        [HttpGet] //Get 
        [Route("{timeDeviationCauseId:int}")]
        public IHttpActionResult GetTimeDeviationCause(int timeDeviationCauseId)
        {
            return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCause(timeDeviationCauseId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost] //Save
        [Route("")]
        public IHttpActionResult SaveTimeDeviationCauses(TimeDeviationCauseDTO model)
        {
            return Content(HttpStatusCode.OK, tdcm.SaveTimeDeviationCauses(base.ActorCompanyId, model));
        }

        [HttpDelete] //Delete
        [Route("{timeDeviationCauseId:int}")]
        public IHttpActionResult DeleteTimeDeviationCause(int timeDeviationCauseId)
        {
            return Content(HttpStatusCode.OK, tdcm.DeleteTimeDeviationCause(timeDeviationCauseId, base.ActorCompanyId));
        }


        [HttpGet] //Get small types
        [Route("Dict/{addEmptyRow:bool}/{removeAbsence:bool}/{removePresence:bool}")]
        public IHttpActionResult GetTimeDeviationCausesDict(bool addEmptyRow = false, bool removeAbsence = false, bool removePresence = false)
        {
            return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCausesDict(base.ActorCompanyId, addEmptyRow, removeAbsence, removePresence).ToSmallGenericTypes());
        }

        [HttpGet] //Get small types
        [Route("Dict/Absence/{addEmptyRow:bool}")]
        public IHttpActionResult GetTimeDeviationCausesAbsenceDict(bool addEmptyRow = false)
        {
            return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCausesAbsenceDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{employeeGroupId:int}/{getEmployeeGroups:bool}/{onlyUseInTimeTerminal:bool}")]
        public IHttpActionResult GetTimeDeviationCauses(int employeeGroupId, bool getEmployeeGroups, bool onlyUseInTimeTerminal)
        {
//            checked if you need to remove this and use the GetTimeDeviationCausesDict instead
            if (employeeGroupId > 0)
                return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCausesByEmployeeGroup(base.ActorCompanyId, employeeGroupId, sort: true, loadTimeCode: true, onlyUseInTimeTerminal: onlyUseInTimeTerminal, setTimeDeviationTypeName: true).ToDTOs());
            else
                return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCauses(base.ActorCompanyId, loadEmployeeGroups: getEmployeeGroups).ToDTOs());
        }

        [HttpGet]
        [Route("FromEmployeeId/Absence/{employeeId:int}/{date}/{onlyUseInTimeTerminal:bool}")]
        public IHttpActionResult GetTimeDeviationCausesAbsenceFromEmployeeId(int employeeId, string date, bool onlyUseInTimeTerminal)
        {
            return Content(HttpStatusCode.OK, tdcm.GetTimeDeviationCausesAbsenceFromEmployeeId(base.ActorCompanyId, employeeId, BuildDateTimeFromString(date, true, DateTime.Today), onlyUseInTimeTerminal).ToDTOs());
        }


        #endregion
    }
}