using Soe.WebApi.Controllers;
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
    [RoutePrefix("V2/Time/Employee")]
    public class EmployeePositionController : SoeApiController
    {
        #region Variables

        private readonly EmployeeManager em;

        #endregion

        #region Constructor

        public EmployeePositionController(EmployeeManager em)
        {
            this.em = em;
        }

        #endregion

        #region Position

        [HttpGet]
        [Route("Position/Grid/{loadSkills:bool}/{positionId:int?}")]
        public IHttpActionResult GetPositionsGrid(bool loadSkills, int? positionId = null)
        {
            return Content(HttpStatusCode.OK, em.GetPositions(base.ActorCompanyId, loadSkills, positionId).ToGridDTOs());
        }

        [HttpGet]
        [Route("Position/Dict")]
        public IHttpActionResult GetPositionsDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, em.GetPositionsDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Position")]
        public IHttpActionResult GetPositions(bool loadSkills)
        {
            return Content(HttpStatusCode.OK, em.GetPositions(base.ActorCompanyId, loadSkills).ToDTOs(loadSkills));
        }

        [HttpGet]
        [Route("Position/{employeePositionId:int}/{loadSkills:bool}")]
        public IHttpActionResult GetPosition(int employeePositionId, bool loadSkills)
        {
            return Content(HttpStatusCode.OK, em.GetPositionIncludingSkill(employeePositionId).ToDTO(loadSkills));
        }

        [HttpPost]
        [Route("Position")]
        public IHttpActionResult SavePosition(PositionDTO position)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.SavePosition(position, position.PositionSkills, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("PositionGridUpdate")]
        public IHttpActionResult UpdatePositionGrid()
        {
            return Content(HttpStatusCode.OK, em.UpdateAllLinkedPositions(base.ActorCompanyId));
        }

        [HttpPost]
        [Route("SysPositionGridUpdateAndLink")]
        public IHttpActionResult UpdateAndLinkSysPositionGrid(List<SysPositionGridDTO> sysPositions)
        {
            return Content(HttpStatusCode.OK, em.CopyAndLinkSysPositions(sysPositions, true, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("SysPositionGridUpdate")]
        public IHttpActionResult UpdateSysPositionGrid(List<SysPositionGridDTO> sysPositions)
        {
            return Content(HttpStatusCode.OK, em.CopyAndLinkSysPositions(sysPositions, false, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("Position/{positionId:int}")]
        public IHttpActionResult DeletePosition(int positionId)
        {
            return Content(HttpStatusCode.OK, em.DeletePosition(positionId));
        }
        #endregion
    }
}