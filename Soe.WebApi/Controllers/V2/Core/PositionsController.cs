using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/SysPosition")]
    public class PositionsController : SoeApiController
    {
        #region Variables

        private readonly EmployeeManager em;

        #endregion

        #region Constructor

        public PositionsController(EmployeeManager em)
        {
            this.em = em;
        }

        #endregion

        #region SysPosition

        [HttpGet]
        [Route("Grid/{positionId:int?}")]
        public IHttpActionResult GetSysPositionsGrid(int? positionId = null)
        {
            return Content(HttpStatusCode.OK, em.GetSysPositions(null, null, null, positionId).ToGridDTOs());
        }

        [HttpGet]
        [Route("{sysCountryId:int}/{sysLanguageId:int}")]
        public IHttpActionResult GetSysPositions(int sysCountryId, int sysLanguageId)
        {
            return Content(HttpStatusCode.OK, em.GetSysPositions(base.ActorCompanyId, sysCountryId, sysLanguageId).ToGridDTOs());
        }

        [HttpGet]
        [Route("{sysCountryId:int}/{sysLanguageId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetSysPositionsDict(int sysCountryId, int sysLanguageId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, em.GetSysPositionsDict(base.ActorCompanyId, sysCountryId, sysLanguageId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{sysPositionId:int}")]
        public IHttpActionResult GetSysPosition(int sysPositionId)
        {
            return Content(HttpStatusCode.OK, em.GetSysPosition(sysPositionId).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveSysPosition(SysPositionDTO sysPosition)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.SaveSysPosition(sysPosition));
        }

        [HttpDelete]
        [Route("{sysPositionId:int}")]
        public IHttpActionResult DeleteSysPosition(int sysPositionId)
        {
            return Content(HttpStatusCode.OK, em.DeleteSysPosition(sysPositionId));
        }

        #endregion
    }
}