using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/Employee/FollowUpType")]

    public class FollowupTypesController : SoeApiController
    {
        #region Variables

        private readonly EmployeeManager em;

        #endregion

        #region Constructor

        public FollowupTypesController(EmployeeManager em)
        {
            this.em = em;
        }

        #endregion

        #region FollowUpType

        [HttpGet]
        [Route("Grid/{followUpTypeId:int?}")]
        public IHttpActionResult GetFollowUpTypes(int? followUpTypeId = null)
        {
            return Content(HttpStatusCode.OK, em.GetFollowUpTypes(base.ActorCompanyId, followUpTypeId: followUpTypeId).ToGridDTOs());
        }

        [HttpGet]
        [Route("{followUpTypeId:int}")]
        public IHttpActionResult GetFollowUpType(int followUpTypeId)
        {
            return Content(HttpStatusCode.OK, em.GetFollowUpType(base.ActorCompanyId, followUpTypeId).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveFollowUpType(FollowUpTypeDTO followUpTypeDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.SaveFollowUpType(followUpTypeDTO, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("UpdateState")]
        public IHttpActionResult UpdateFollowUpTypesState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.UpdateFollowUpTypesState(model.Dict, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{followUpTypeId:int}")]
        public IHttpActionResult DeleteFollowUpType(int followUpTypeId)
        {
            return Content(HttpStatusCode.OK, em.DeleteFollowUpType(base.ActorCompanyId, followUpTypeId));
        }

        #endregion

    }
}