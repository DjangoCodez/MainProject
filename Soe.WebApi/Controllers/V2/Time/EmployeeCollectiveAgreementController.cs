using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/Employee/EmployeeCollectiveAgreement")]
    public class EmployeeCollectiveAgreementController : SoeApiController
    {
        #region Variables

        private readonly EmployeeManager em;

        #endregion

        #region Constructor

        public EmployeeCollectiveAgreementController(EmployeeManager em)
        {
            this.em = em;
        }

        #endregion

        #region EmployeeCollectiveAgreement

        [HttpGet]
        [Route("Grid/{collectiveAgreementId:int?}")]
        public IHttpActionResult GetEmployeeCollectiveAgreementsGrid(int? collectiveAgreementId = null)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeCollectiveAgreements(base.ActorCompanyId, false, true, true, true, true, true, null, collectiveAgreementId).ToGridDTOs());
        }

        [HttpGet]
        [Route("Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetEmployeeCollectiveAgreementsDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeCollectiveAgreementsDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetEmployeeCollectiveAgreements()
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeCollectiveAgreements(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("{employeeCollectiveAgreementId:int}")]
        public IHttpActionResult GetEmployeeCollectiveAgreement(int employeeCollectiveAgreementId)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeCollectiveAgreement(base.ActorCompanyId, employeeCollectiveAgreementId).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveEmployeeCollectiveAgreement(EmployeeCollectiveAgreementDTO employeeCollectiveAgreementDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.UpsertEmployeeCollectiveAgreement(employeeCollectiveAgreementDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{employeeCollectiveAgreementId:int}")]
        public IHttpActionResult DeleteEmployeeCollectiveAgreement(int employeeCollectiveAgreementId)
        {
            return Content(HttpStatusCode.OK, em.DeleteEmployeeCollectiveAgreement(employeeCollectiveAgreementId));
        }

        #endregion
    }
}