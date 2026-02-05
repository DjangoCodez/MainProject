using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Shared
{
    [RoutePrefix("V2/Shared/ExternalCompany")]
    public class ExternalCompanySearchController : SoeApiController
    {
        #region Variables

        private readonly ExternalCompanySearchManager ecsm;

        #endregion

        #region Constructor
        public ExternalCompanySearchController(ExternalCompanySearchManager ecsm)
        {
            this.ecsm = ecsm;
        }
        #endregion

        #region Methods
        [HttpPost]
        [Route("Search/{provider:int}")]
        public IHttpActionResult GetExternalCompanies(int provider, ExternalCompanyFilterDTO filter)
        {
            return Content(HttpStatusCode.OK, ecsm.GetExternalComanyResultDTOs((ExternalCompanySearchProvider)provider, filter));
        }
        #endregion
    }
}