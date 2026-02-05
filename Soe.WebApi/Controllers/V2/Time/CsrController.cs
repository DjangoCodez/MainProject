using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/Csr")]
    public class CsrController : SoeApiController
    {
        #region Variables

        private readonly EmployeeManager em;
        
        #endregion

        #region Constructor

        public CsrController(EmployeeManager em)
        {
            this.em = em;
        }

        #endregion

        #region Csr

        [HttpGet]
        [Route("GetEmployeesForCsrExport/{year:int}")]
        public IHttpActionResult GetEmployeesForCsrExport(int year)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeesForCSRExport(base.ActorCompanyId, year));
        }

        [HttpPost]
        [Route("GetCsrInquiries")]
        public IHttpActionResult GetCsrInquiries(GetCSRResponseModel model)
        {
            return Content(HttpStatusCode.OK, em.CsrInquiries(base.ActorCompanyId, model.IdsToTransfer, model.year));
        }

        #endregion
    }
}