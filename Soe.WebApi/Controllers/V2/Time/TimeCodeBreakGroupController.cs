using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/TimeCodeBreakGroup")]
    public class TimeCodeBreakGroupController : SoeApiController
    {
        #region Variables

        private readonly TimeCodeManager tcm;

        #endregion

        #region Constructor

        public TimeCodeBreakGroupController(TimeCodeManager tcm)
        {
            this.tcm = tcm;
        }

        #endregion

        #region TimeCodeBreakGroup

        [HttpGet]
        [Route("Grid")]
        public IHttpActionResult GetTimeCodeBreakGroupsGrid(int? timeCodeBreakId = null)
        {
            return Content(HttpStatusCode.OK, tcm.GetTimeCodeBreakGroups(base.ActorCompanyId, timeCodeBreakId).ToGridDTOs());
        }

        [HttpGet]
        [Route("{timeCodeBreakGroupId:int}")]
        public IHttpActionResult GetTimeCodeBreakGroup(int timeCodeBreakGroupId)
        {
            return Content(HttpStatusCode.OK, tcm.GetTimeCodeBreakGroup(timeCodeBreakGroupId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveTimeCodeBreakGroup(TimeCodeBreakGroupDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tcm.SaveTimeCodeBreakGroup(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{timeCodeBreakGroupId:int}")]
        public IHttpActionResult DeleteTimeCodeBreakGroup(int timeCodeBreakGroupId)
        {
            return Content(HttpStatusCode.OK, tcm.DeleteTimeCodeBreakGroup(timeCodeBreakGroupId, base.ActorCompanyId));
        }

        #endregion

    }
}