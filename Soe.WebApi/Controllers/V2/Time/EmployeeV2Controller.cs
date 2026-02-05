using Soe.WebApi.Controllers;
using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/Employee")]
    public class EmployeeV2Controller : SoeApiController
    {
        #region Variables

        private readonly EmployeeManager em;
        private readonly TimeEngineManager tem;

        #endregion

        #region Constructor

        public EmployeeV2Controller(EmployeeManager em, TimeEngineManager tem)
        {
            this.em = em;
            this.tem = tem;
        }

        #endregion

        #region Employee

        [HttpGet]
        [Route("Employees/{addEmptyRow:bool}/{concatNumberAndName:bool}/{getHidden:bool}/{orderByName:bool}")]
        public IHttpActionResult GetAllEmployeeSmallDTOs(bool addEmptyRow, bool concatNumberAndName, bool getHidden, bool orderByName)
        {
            return Content(HttpStatusCode.OK, em.GetAllEmployeeSmallDTOs(base.ActorCompanyId, addEmptyRow, concatNumberAndName, getHidden, orderByName));
        }


        [HttpGet]
        [Route("Planning/{employeeIds}/{getHidden:bool}/{getInactive:bool}/{loadSkills:bool}/{loadAvailability:bool}/{dateFrom}/{dateTo}/{includeSecondaryCategoriesOrAccounts:bool}/{displayMode:int}")]
        public IHttpActionResult GetEmployeeListForPlanning(string employeeIds, bool getHidden, bool getInactive, bool loadSkills, bool loadAvailability, string dateFrom, string dateTo, bool includeSecondaryCategoriesOrAccounts, TimeSchedulePlanningDisplayMode displayMode)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeList(base.ActorCompanyId, base.RoleId, base.UserId, StringUtility.SplitNumericList(employeeIds, true), null, getHidden, getInactive, loadSkills, loadAvailability, false, false, true, BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true), false, false, includeSecondaryCategoriesOrAccounts: includeSecondaryCategoriesOrAccounts, displayMode: displayMode));
        }

        [HttpGet]
        [Route("EmployeeForUser/TimeCode/{date}")]
        public IHttpActionResult GetEmployeeForUserWithTimeCode(string date)
        {
            return Content(HttpStatusCode.OK, em.GetProjectEmployeeForUser(base.UserId, base.ActorCompanyId, false, true).ToEmployeeTimeCodeDTO(BuildDateTimeFromString(date, true), em.GetEmployeeGroupsFromCache(base.ActorCompanyId)));
        }

        [HttpGet]
        [Route("EmployeesForGrid/{date}/{employeeIds}/{showInactive:bool}/{showEnded:bool}/{showNotStarted:bool}/{setAge:bool}/{loadPayrollGroups:bool}/{loadAnnualLeaveGroups:bool}")]
        public IHttpActionResult GetEmployeesForGrid(string date, string employeeIds, bool showInactive, bool showEnded, bool showNotStarted, bool setAge, bool loadPayrollGroups, bool loadAnnualLeaveGroups)
        {
            List<int> employeeFilter = StringUtility.SplitNumericList(employeeIds, true);
            return Content(HttpStatusCode.OK, em.GetEmployeesForGrid(base.ActorCompanyId, base.UserId, base.RoleId, BuildDateTimeFromString(date, true).Value, employeeFilter, showInactive, showEnded, showNotStarted, setAge, loadPayrollGroups, loadAnnualLeaveGroups));
        }

        [HttpGet]
        [Route("EmployeesForGridSmall/{date}/{employeeIds}/{showInactive:bool}/{showEnded:bool}/{showNotStarted:bool}")]
        public IHttpActionResult GetEmployeesForGridSmall(string date, string employeeIds, bool showInactive, bool showEnded, bool showNotStarted)
        {
            List<int> employeeFilter = StringUtility.SplitNumericList(employeeIds, true);
            return Content(HttpStatusCode.OK, em.GetEmployeesForGridSmall(base.ActorCompanyId, base.UserId, base.RoleId, BuildDateTimeFromString(date, true).Value, employeeFilter, showInactive, showEnded, showNotStarted));
        }

        [HttpGet]
        [Route("EmployeesForGridDict/{dateFrom}/{dateTo}/{employeeIds}/{showInactive:bool}/{showEnded:bool}/{showNotStarted:bool}/{filterOnAnnualLeaveAgreement:bool}")]
        public IHttpActionResult GetEmployeesForGridDict(string dateFrom, string dateTo, string employeeIds, bool showInactive, bool showEnded, bool showNotStarted, bool filterOnAnnualLeaveAgreement)
        {
            List<int> employeeFilter = StringUtility.SplitNumericList(employeeIds, true);
            return Content(HttpStatusCode.OK, em.GetEmployeesForGridDict(base.ActorCompanyId, base.UserId, base.RoleId, BuildDateTimeFromString(dateFrom, true).Value, BuildDateTimeFromString(dateTo, true).Value, employeeFilter, showInactive, showEnded, showNotStarted, filterOnAnnualLeaveAgreement: filterOnAnnualLeaveAgreement).ToSmallGenericTypes());
        }

        [HttpPost]
        [Route("Availability")]
        public IHttpActionResult GetEmployeeAvailability(ListIntModel model)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeListAvailability(base.ActorCompanyId, model.Numbers));
        }

        [HttpPost]
        [Route("AvailableEmployees/")]
        public IHttpActionResult GetAvailableEmployees(GetAvailableEmployeesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tem.GetAvailableEmployees(model.TimeScheduleTemplateBlockIds, model.EmployeeIds, model.FilterOnShiftType, model.FilterOnAvailability, model.FilterOnSkills, model.FilterOnWorkRules, model.FilterOnMessageGroupId, true, null, false, false));
        }

        #endregion

        #region EmployeeChild

        [HttpGet]
        [Route("EmployeeChildsDict/{employeeId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetEmployeeChildsDict(int employeeId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeChildsDict(employeeId, addEmptyRow).ToSmallGenericTypes());
        }

        #endregion

        #region Hidden employee

        [HttpGet]
        [Route("HiddenEmployeeId/")]
        public IHttpActionResult GetHiddenEmployeeId()
        {
            return Content(HttpStatusCode.OK, em.GetHiddenEmployeeId(base.ActorCompanyId));
        }

        #endregion

        #region EmployeeChild
        [HttpGet]
        [Route("EmployeeChild/{employeeId}")]
        public IHttpActionResult GetEmployeeChilds(int employeeId)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeChilds(employeeId, base.ActorCompanyId).ToDTOs());
        }
        [HttpGet]
        [Route("EmployeeChildSmall/{employeeId}/{addEmptyRow}")]
        public IHttpActionResult GetEmployeeChildsSmall(int employeeId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeChildsDict(employeeId, addEmptyRow).ToSmallGenericTypes());
        }
        #endregion
    }
}