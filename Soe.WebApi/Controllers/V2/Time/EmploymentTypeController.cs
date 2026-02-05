using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/EmploymentType")]
    public class EmploymentTypeController : SoeApiController
    {
        #region Variables

        private readonly EmployeeManager em;

        #endregion

        #region Constructor

        public EmploymentTypeController(EmployeeManager em)
        {
            this.em = em;
        }

        #endregion

        #region EmploymentType

        [HttpGet]
        [Route("Grid/{employmentTypeId:int?}")]
        public IHttpActionResult GetEmploymentTypesGrid(int? employmentTypeId = null)
        {
            return Content(HttpStatusCode.OK, em.GetEmploymentTypesFromDBForGrid(base.ActorCompanyId, employmentTypeId).OrderByDescending(x => x.Standard).ThenBy(x => x.Type).ThenBy(x => x.Name).ToGridDTOs());
        }

        [HttpGet]
        [Route("{employmentTypeId:int}")]
        public IHttpActionResult GetEmploymentType(int employmentTypeId)
        {
            return Content(HttpStatusCode.OK, em.GetEmploymentTypesFromDB(base.ActorCompanyId).GetEmploymentType(employmentTypeId));
        }

        [HttpGet]
        [Route("StandardEmploymentTypes")]
        public IHttpActionResult getStandardEmploymentTypes()
        {
            return Content(HttpStatusCode.OK, em.GetStandardEmploymentTypes(base.ActorCompanyId, TermCacheManager.Instance.GetLang()).OrderBy(e => e.Type).ToList().ToSmallEmploymentTypes());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveEmploymentType(EmploymentTypeDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.SaveEmploymentType(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{employmentTypeId:int}")]
        public IHttpActionResult DeleteEmploymentType(int employmentTypeId)
        {
            return Content(HttpStatusCode.OK, em.DeleteEmploymentType(base.ActorCompanyId, employmentTypeId));
        }

        [HttpPost]
        [Route("UpdateState")]
        public IHttpActionResult UpdateEmploymentTypesState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.UpdateEmploymentTypesState(model.Dict, base.ActorCompanyId, base.RoleId));
        }

        #endregion
    }
}