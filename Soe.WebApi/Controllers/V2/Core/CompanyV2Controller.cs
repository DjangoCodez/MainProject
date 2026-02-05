using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Data;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core")]
    public class CompanyV2Controller : SoeApiController
    {
        #region Variables

        private readonly CompanyManager coma;

        #endregion

        #region Constructor

        public CompanyV2Controller( CompanyManager coma)
        {
            this.coma = coma;
        }

        #endregion


        [HttpGet]
        [Route("Company/{actorCompanyId:int}")]
        public IHttpActionResult GetCompany(int actorCompanyId)
        {
            return Content(HttpStatusCode.OK, coma.GetCompany(actorCompanyId, true).ToCompanyDTO());
        }
    }
}