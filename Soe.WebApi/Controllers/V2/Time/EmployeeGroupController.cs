using Soe.WebApi.Controllers;
using Soe.WebApi.Extensions;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/EmployeeGroup")]
    public class EmployeeGroupController : SoeApiController
    {
        #region Variables

        private readonly EmployeeManager em;
        private readonly TimeAccumulatorManager tam;

        #endregion

        #region Constructor

        public EmployeeGroupController(EmployeeManager em, TimeAccumulatorManager tam)
        {
            this.em = em;
            this.tam = tam;
        }

        #endregion

        #region EmployeeGroup

        //[HttpGet]
        //[Route("Grid")]
        //public IHttpActionResult GetEmployeeGroupsGrid()
        //{
        //    return Content(HttpStatusCode.OK, em.GetEmployeeGroups(base.ActorCompanyId, loadTimeDeviationCauseMappings: true, loadTimeDeviationCauses: true, loadDayTypes: true).ToDTOs(false, em.GetTermGroupContent(TermGroup.TimeReportType).ToDictionary()));
        //}
        [HttpGet]
        [Route("{employeeGroupId:int}/{loadTimeDeviationCauseTimeCode:bool}/{loadDayTypes:bool}/{loadTimeAccumulators:bool}/{loadTimeDeviationCauseRequests:bool}/{loadTimeDeviationCauseAbsenceAnnouncements:bool}/{loadLinkedTimeCodes:bool}/{loadTimeDeviationCauses:bool}/{loadTimeStampRounding:bool}/{loadAttestTransitions:bool}/{loadRuleWorkTimePeriod:bool}/{loadStdAccounts:bool}/{loadExternalCode:bool}")]
        public IHttpActionResult GetEmployeeGroup(int employeeGroupId, bool loadTimeDeviationCauseTimeCode, bool loadDayTypes, bool loadTimeAccumulators, bool loadTimeDeviationCauseRequests, bool loadTimeDeviationCauseAbsenceAnnouncements, bool loadLinkedTimeCodes, bool loadTimeDeviationCauses, bool loadTimeStampRounding, bool loadAttestTransitions, bool loadRuleWorkTimePeriod, bool loadStdAccounts, bool loadExternalCode)
        {
            //if (Request.HasAcceptValue(HttpExtensions.ACCEPT_SMALL_DTO))
            //    return Content(HttpStatusCode.OK, em.GetEmployeeGroup(employeeGroupId, loadTransitions: true, loadDayType: true, loadTimeDeviationCause: true, loadTimeCode: true, loadRuleWorkTimePeriod: true, loadExternalCode: true, loadAccountInternal: true).ToSmallDTO());

            return Content(HttpStatusCode.OK, em.GetEmployeeGroupNew(employeeGroupId, base.ActorCompanyId, loadTimeDeviationCauseTimeCode, loadDayTypes, loadTimeAccumulators, loadTimeDeviationCauseRequests, loadTimeDeviationCauseAbsenceAnnouncements, loadLinkedTimeCodes, loadTimeDeviationCauses, loadTimeStampRounding, loadAttestTransitions, loadRuleWorkTimePeriod, loadStdAccounts, loadExternalCode).ToDTONew(null));
        }

        [HttpGet]
        [Route("Grid/{employeeGroupId:int?}")]
        public IHttpActionResult GetEmployeeGroupsGrid(int? employeeGroupId = null)
        {
            Dictionary<int, string> timeReportTypes = em.GetTermGroupContent(TermGroup.TimeReportType).ToDictionary();
            return Content(HttpStatusCode.OK, em.GetEmployeeGroups(base.ActorCompanyId, loadTimeDeviationCauseMappings: true, loadTimeDeviationCauses: true, loadDayTypes: true, employeeGroupId: employeeGroupId).ToGridDTOs(timeReportTypes));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveEmployeeGroup(EmployeeGroupDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.SaveEmployeeGroup(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{employeeGroupId:int}")]
        public IHttpActionResult DeleteEmployeeGroup(int employeeGroupId)
        {
            return Content(HttpStatusCode.OK, em.DeleteEmployeeGroupNew(base.ActorCompanyId, employeeGroupId));
        }

        [HttpGet]
        [Route("Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetEmployeeGroupsDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeGroupsDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("DictSmall")]
        public IHttpActionResult GetEmployeeGroupsDictSmall()
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeGroups(base.ActorCompanyId).ToSmallDTOs());
        }
        

        [HttpGet]
        [Route("{employeeId:int}/{dateString}")]
        public IHttpActionResult GetEmployeeGroupId(int employeeId, string dateString)
        {
            EmployeeGroup group = em.GetEmployeeGroupForEmployee(employeeId, base.ActorCompanyId, base.BuildDateTimeFromString(dateString, true).Value);
            return Content(HttpStatusCode.OK, group != null ? group.EmployeeGroupId : 0);
        }

        

        #endregion

        #region TimeAccumulator
        // Put here or somewhere else??
        [HttpGet]
        [Route("TimeAccumulatorDict/{addEmptyRow:bool}/{includeVacationBalance:bool}/{includeTimeAccountBalance:bool}")]
        public IHttpActionResult GetTimeAccumulatorsDict(bool addEmptyRow, bool includeVacationBalance, bool includeTimeAccountBalance)
        {
            return Content(HttpStatusCode.OK, tam.GetTimeAccumulatorsDict(base.ActorCompanyId, addEmptyRow, includeVacationBalance, includeTimeAccountBalance).ToSmallGenericTypes());
        }
        #endregion
    }
}