using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/EndReason")]
    public class EndReasonController : SoeApiController
    {
        #region Variables

        private readonly EmployeeManager em;

        #endregion

        #region Constructor

        public EndReasonController(EmployeeManager em)
        {
            this.em = em;
        }

        #endregion

        #region EndReason

        [HttpGet]
        [Route("Grid")]
        public IHttpActionResult GetEndReasonsGrid(int? endReasonId = null)
        {
            return Content(HttpStatusCode.OK, em.GetEndReasons(base.ActorCompanyId, endReasonId: endReasonId).ToGridDTOs());
        }

        [HttpGet]
        [Route("EndReason/{endReasonId:int}")]
        public IHttpActionResult GetEndReason(int endReasonId)
        {
            return Content(HttpStatusCode.OK, em.GetCompanyEndReason(base.ActorCompanyId, endReasonId).ToDTO());
        }

        [HttpPost]
        [Route("EndReason")]
        public IHttpActionResult SaveEndReason(EndReasonDTO endReasonDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.SaveEndReason(endReasonDTO, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("UpdateState")]
        public IHttpActionResult UpdateEndReasonsState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.UpdateEndReasonsState(model.Dict, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("EndReason/{endReasonId:int}")]
        public IHttpActionResult DeleteEndReason(int endReasonId)
        {
            return Content(HttpStatusCode.OK, em.DeleteEndReason(base.ActorCompanyId, endReasonId));
        }

        #endregion
    }
}