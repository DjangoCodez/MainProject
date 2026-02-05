using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/StaffingNeeds")]
    public class StaffingNeedsController : SoeApiController
    {
        #region Variables

        private readonly TimeScheduleManager tsm;

        #endregion

        #region Constructor

        public StaffingNeedsController(TimeScheduleManager tsm)
        {
            this.tsm = tsm;
        }

        #endregion

        #region StaffingNeedsLocation

        [HttpGet]
        [Route("StaffingNeedsLocation/Grid/{locationId:int?}")]
        public IHttpActionResult GetStaffingNeedsLocationsGrid(int? locationId = null)
        {
            return Content(HttpStatusCode.OK, tsm.GetStaffingNeedsLocations(base.ActorCompanyId, null, locationId).ToGridDTOs(true, true));
        }

        [HttpGet]
        [Route("StaffingNeedsLocation/{locationId:int}")]
        public IHttpActionResult GetStaffingNeedsLocation(int locationId)
        {
            return Content(HttpStatusCode.OK, tsm.GetStaffingNeedsLocation(locationId).ToDTO());
        }

        [HttpPost]
        [Route("StaffingNeedsLocation")]
        public IHttpActionResult SaveStaffingNeedsLocation(StaffingNeedsLocationDTO model)
        {
            return Content(HttpStatusCode.OK, tsm.SaveStaffingNeedsLocation(model));

        }

        [HttpDelete]
        [Route("StaffingNeedsLocation/{locationId:int}")]
        public IHttpActionResult DeleteStaffingNeedsLocation(int locationId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteStaffingNeedsLocation(locationId));
        }

        #endregion

        #region StaffingNeedsLocationGroup

        [HttpGet]
        [Route("StaffingNeedsLocationGroup/Grid/{locationGroupId:int?}")]
        public IHttpActionResult GetStaffingNeedsLocationGroupsGrid(int? locationGroupId = null)
        {
            return Content(HttpStatusCode.OK, tsm.GetStaffingNeedsLocationGroups(base.ActorCompanyId, includeTimeScheduleTask: true, locationGroupId: locationGroupId).ToGridDTOs());
        }

        [HttpGet]
        [Route("StaffingNeedsLocationGroup/Dict/{addEmptyRow:bool}/{includeAccountName:bool}")]
        public IHttpActionResult GetStaffingNeedsLocationGroupsDict(bool addEmptyRow, bool includeAccountName)
        {
            return Content(HttpStatusCode.OK, tsm.GetStaffingNeedsLocationGroupsDict(base.ActorCompanyId, addEmptyRow, includeAccountName).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("StaffingNeedsLocationGroup/{locationGroupId:int}")]
        public IHttpActionResult GetStaffingNeedsLocationGroup(int locationGroupId)
        {
            return Content(HttpStatusCode.OK, tsm.GetStaffingNeedsLocationGroup(locationGroupId).ToDTO(true));
        }

        [HttpPost]
        [Route("StaffingNeedsLocationGroup")]
        public IHttpActionResult SaveStaffingNeedsLocationGroup(SaveStaffingNeedsLocationGroupModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveStaffingNeedsLocationGroup(model.Dto, model.ShiftTypeIds, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("StaffingNeedsLocationGroup/{locationGroupId:int}")]
        public IHttpActionResult DeleteStaffingNeedsLocationGroup(int locationGroupId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteStaffingNeedsLocationGroup(locationGroupId));
        }

        #endregion

        #region StaffingNeedsRule

        [HttpGet]
        [Route("StaffingNeedsRule/Grid/{ruleId:int?}")]
        public IHttpActionResult GetStaffingNeedsRulesGrid(int? ruleId = null)
        {
            return Content(HttpStatusCode.OK, tsm.GetStaffingNeedsRules(base.ActorCompanyId, ruleId: ruleId).ToGridDTOs(true));
        }

        [HttpGet]
        [Route("StaffingNeedsRule/{ruleId:int}")]
        public IHttpActionResult GetStaffingNeedsRule(int ruleId)
        {
            return Content(HttpStatusCode.OK, tsm.GetStaffingNeedsRule(ruleId).ToDTO(true));
        }

        [HttpPost]
        [Route("StaffingNeedsRule")]
        public IHttpActionResult SaveStaffingNeedsRule(StaffingNeedsRuleDTO model)
        {
            return Content(HttpStatusCode.OK, tsm.SaveStaffingNeedsRule(model));
        }

        [HttpDelete]
        [Route("StaffingNeedsRule/{ruleId:int}")]
        public IHttpActionResult DeleteStaffingNeedsRule(int ruleId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteStaffingNeedsRule(ruleId));
        }

        #endregion

        #region GeneratedNeed

        [HttpGet]
        [Route("GetTimeScheduleTaskGeneratedNeeds/{timeScheduleTaskId:int}")]
        public IHttpActionResult GetTimeScheduleTaskGeneratedNeeds(int timeScheduleTaskId)
        {
            return Content(HttpStatusCode.OK, tsm.GetTimeScheduleTaskGeneratedNeeds(timeScheduleTaskId));
        }

        [HttpPost]
        [Route("DeleteGeneratedNeeds/")]
        public IHttpActionResult DeleteGeneratedNeeds(ListIntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.DeleteGeneratedNeeds(model.Numbers));
        }

        #endregion
    }
}