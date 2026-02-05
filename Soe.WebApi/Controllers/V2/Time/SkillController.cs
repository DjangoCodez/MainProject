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
    [RoutePrefix("V2/Time/Skill")]
    public class SkillController : SoeApiController
    {
        #region Variables

        private readonly bool useSkillsPoc = false;
        
        private readonly TimeScheduleManager tsm;

        #endregion

        #region Constructor

        public SkillController(TimeScheduleManager tsm)
        {
            this.tsm = tsm;
        }

        #endregion

        #region Skill

        [HttpGet]
        [Route("Grid/{skillId:int?}")]
        public IHttpActionResult GetSkillsGrid(int? skillId = null)
        {
            return Content(HttpStatusCode.OK, tsm.GetSkills(base.ActorCompanyId, true, skillId).ToGridDTOs());
        }

        [HttpGet]
        [Route("Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetSkillsDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, tsm.GetSkillsDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{skillId:int}")]
        public IHttpActionResult GetSkill(int skillId)
        {
            return Content(HttpStatusCode.OK, tsm.GetSkill(skillId).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveSkill(SkillDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveSkill(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{skillId:int}")]
        public IHttpActionResult DeleteSkill(int skillId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteSkill(skillId));
        }

        #endregion

        #region SkillType

        [HttpGet]
        [Route("SkillType/Grid/{skillTypeId:int?}")]
        public IHttpActionResult GetSkillTypesGrid(int? skillTypeId = null)
        {
            return Content(HttpStatusCode.OK, tsm.GetSkillTypes(base.ActorCompanyId, null, skillTypeId).ToGridDTOs());
        }

        [HttpGet]
        [Route("SkillType/Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetSkillTypesDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, tsm.GetSkillTypesDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("SkillType/{skillTypeId:int}")]
        public IHttpActionResult GetSkillType(int skillTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.GetSkillType(skillTypeId).ToDTO());
        }

        [HttpPost]
        [Route("SkillType")]
        public IHttpActionResult SaveSkillType(SkillTypeDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveSkillType(model, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("SkillType/UpdateState")]
        public IHttpActionResult UpdateSkillTypesState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.UpdateSkillTypesState(model.Dict));
        }

        [HttpDelete]
        [Route("SkillType/{skillTypeId:int}")]
        public IHttpActionResult DeleteSkillType(int skillTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteSkillType(skillTypeId));
        }

        #endregion

        #region EmployeeSkill

        [HttpGet]
        [Route("Employee/{employeeId:int}")]
        public IHttpActionResult GetEmployeeSkills(int employeeId)
        {
            if (useSkillsPoc)
                return Content(HttpStatusCode.OK, tsm.GetEmployeeSkillsDTOs(employeeId));
            else
                return Content(HttpStatusCode.OK, tsm.GetEmployeeSkills(employeeId).ToDTOs(true));
        }

        [HttpGet]
        [Route("Employee/{employeeId:int}/{shiftTypeId:int}/{date}")]
        public IHttpActionResult EmployeeHasShiftTypeSkills(int employeeId, int shiftTypeId, string date)
        {
            return Content(HttpStatusCode.OK, tsm.EmployeeHasShiftTypeSkills(employeeId, shiftTypeId, BuildDateTimeFromString(date, true).Value));
        }


        [HttpGet]
        [Route("MatchEmployees/{shiftTypeId:int}")]
        public IHttpActionResult MatchEmployeesByShiftTypeSkills(int shiftTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.MatchEmployeesByShiftTypeSkills(shiftTypeId, base.ActorCompanyId));
        }

        #endregion

        #region EmployeePostSkill

        [HttpGet]
        [Route("EmployeePost/{employeePostId:int}")]
        public IHttpActionResult GetEmployeePostSkills(int employeePostId)
        {
            return Content(HttpStatusCode.OK, tsm.GetEmployeePostSkills(employeePostId).ToDTOs(true));
        }

        [HttpGet]
        [Route("EmployeePost/{employeePostId:int}/{shiftTypeId:int}/{date}")]
        public IHttpActionResult EmployeePostHasShiftTypeSkills(int employeePostId, int shiftTypeId, string date)
        {
            return Content(HttpStatusCode.OK, tsm.EmployeePostHasShiftTypeSkills(employeePostId, shiftTypeId, BuildDateTimeFromString(date, true).Value));
        }

        #endregion
    }
}