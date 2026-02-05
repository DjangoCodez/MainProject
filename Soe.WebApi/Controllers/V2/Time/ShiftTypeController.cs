using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;
using Soe.WebApi.Extensions;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/Schedule/ShiftType")]
    public class ShiftTypeController : SoeApiController
    {
        #region Variables

        private readonly TimeScheduleManager tsm;
        #endregion

        #region Constructor

        public ShiftTypeController(TimeScheduleManager tsm)
        {
            this.tsm = tsm;
        }

        #endregion

        #region ShiftType

        [HttpGet]
        [Route("Grid/{loadAccounts:bool}/{loadSkills:bool}/{loadEmployeeStatisticsTargets:bool}/{setTimeScheduleTemplateBlockTypeName:bool}/{setCategoryNames:bool}/{setAccountingString:bool}/{setSkillNames:bool}/{setTimeScheduleTypeName:bool}/{loadHierarchyAccounts:bool}/{shiftTypeId:int?}")]
        public IHttpActionResult GetShiftTypeGrid(bool loadAccounts, bool loadSkills, bool loadEmployeeStatisticsTargets, bool setTimeScheduleTemplateBlockTypeName, bool setCategoryNames, bool setAccountingString, bool setSkillNames, bool setTimeScheduleTypeName, bool loadHierarchyAccounts, int? shiftTypeId = null)
        {
            bool loadAccountInternals = false;

            return Content(HttpStatusCode.OK, tsm.GetShiftTypes(base.ActorCompanyId, loadAccountInternals, loadAccounts, loadSkills, loadEmployeeStatisticsTargets, setTimeScheduleTemplateBlockTypeName, setCategoryNames, setAccountingString, setSkillNames, setTimeScheduleTypeName, loadHierarchyAccounts, shiftTypeId: shiftTypeId).ToGridDTOs());
        }

        [HttpGet]
        [Route("{shiftTypeId:int}/{loadAccounts:bool}/{loadSkills:bool}/{loadEmployeeStatisticsTargets:bool}/{setEmployeeStatisticsTargetsTypeName:bool}/{loadCategories:bool}/{loadHierarchyAccounts:bool}")]
        public IHttpActionResult GetShiftType(int shiftTypeId, bool loadAccounts, bool loadSkills, bool loadEmployeeStatisticsTargets, bool setEmployeeStatisticsTargetsTypeName, bool loadCategories, bool loadHierarchyAccounts)
        {
            return Content(HttpStatusCode.OK, tsm.GetShiftType(shiftTypeId, base.ActorCompanyId, loadAccounts, loadSkills, loadEmployeeStatisticsTargets, setEmployeeStatisticsTargetsTypeName, loadCategories, loadHierarchyAccounts: loadHierarchyAccounts).ToDTO(false, loadSkills, loadEmployeeStatisticsTargets, loadAccounts, false, loadCategories: loadCategories, accountingSettingsOrdered: true));
        }

        [HttpGet]
        [Route("{loadAccountInternals:bool}/{loadAccounts:bool}/{loadSkills:bool}/{loadEmployeeStatisticsTargets:bool}/{setTimeScheduleTemplateBlockTypeName:bool}/{setCategoryNames:bool}/{setAccountingString:bool}/{setSkillNames:bool}/{setTimeScheduleTypeName:bool}/{loadHierarchyAccounts:bool}")]
        public IHttpActionResult GetShiftTypes(bool loadAccountInternals, bool loadAccounts, bool loadSkills, bool loadEmployeeStatisticsTargets, bool setTimeScheduleTemplateBlockTypeName, bool setCategoryNames, bool setAccountingString, bool setSkillNames, bool setTimeScheduleTypeName, bool loadHierarchyAccounts)
        {
            return Content(HttpStatusCode.OK, tsm.GetShiftTypes(base.ActorCompanyId, loadAccountInternals, loadAccounts, loadSkills, loadEmployeeStatisticsTargets, setTimeScheduleTemplateBlockTypeName, setCategoryNames, setAccountingString, setSkillNames, setTimeScheduleTypeName, loadHierarchyAccounts).ToDTOs(includeSkills: loadSkills, includeEmployeeStatisticsTargets: loadEmployeeStatisticsTargets, includeAccountingSettings: loadAccounts));       
        }

        [HttpGet]
        [Route("Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetShiftTypesDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, tsm.GetShiftTypesDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("GetShiftTypesForUsersCategories/")]
        public IHttpActionResult GetShiftTypesForUsersCategories(HttpRequestMessage message)
        {
            int employeeId = message.GetIntValueFromQS("employeeId");
            bool isAdmin = message.GetBoolValueFromQS("isAdmin");
            List<TermGroup_TimeScheduleTemplateBlockType> blockTypes = new List<TermGroup_TimeScheduleTemplateBlockType>();
            List<int> typeIds = message.GetIntListValueFromQS("blockTypes");
            foreach (int id in typeIds)
            {
                blockTypes.Add((TermGroup_TimeScheduleTemplateBlockType)id);
            }

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, tsm.GetShiftTypesForUsersCategories(base.ActorCompanyId, base.UserId, employeeId, isAdmin, true, blockTypes, null).ToGridDTOs());

            return Content(HttpStatusCode.OK, tsm.GetShiftTypesForUsersCategories(base.ActorCompanyId, base.UserId, employeeId, isAdmin, true, blockTypes, null).ToDTOs());
        }

        [HttpGet]
        [Route("GetShiftTypeIdsForUser/")]
        public IHttpActionResult GetShiftTypeIdsForUser(HttpRequestMessage message)
        {
            int employeeId = message.GetIntValueFromQS("employeeId");
            bool isAdmin = message.GetBoolValueFromQS("isAdmin");
            bool includeSecondaryCategories = message.GetBoolValueFromQS("includeSecondaryCategories");
            DateTime? dateFrom = message.GetDateValueFromQS("dateFromString");
            DateTime? dateTo = message.GetDateValueFromQS("dateToString");

            List<TermGroup_TimeScheduleTemplateBlockType> blockTypes = new List<TermGroup_TimeScheduleTemplateBlockType>();
            List<int> typeIds = message.GetIntListValueFromQS("blockTypes");
            foreach (int id in typeIds)
            {
                blockTypes.Add((TermGroup_TimeScheduleTemplateBlockType)id);
            }

            return Content(HttpStatusCode.OK, tsm.GetShiftTypeIdsForUser(null, base.ActorCompanyId, base.RoleId, base.UserId, employeeId, isAdmin, dateFrom, dateTo, includeSecondaryCategories, blockTypes));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveShiftType(ShiftTypeDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveShiftType(model, null, null, null, null, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{shiftTypeId:int}")]
        public IHttpActionResult DeleteShiftType(int shiftTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteShiftType(shiftTypeId, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{shiftTypeIds}")]
        public IHttpActionResult DeleteShiftTypes(string shiftTypeIds)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteShiftTypes(StringUtility.SplitNumericList(shiftTypeIds), base.ActorCompanyId));
        }

        #endregion
    }
}