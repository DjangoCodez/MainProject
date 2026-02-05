using System.Net;
using System.Web.Http;
using Soe.WebApi.Controllers;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/Validator")]
    public class ValidatorController : SoeApiController
    {
        #region Validator

        [HttpGet]
        [Route("ValidIBANNumber/")]
        public IHttpActionResult ValidIBANNumber([FromUri] string iban)
        {
            return Content(HttpStatusCode.OK, SoftOne.Soe.Common.Util.Validator.IsValidIBANNumber(iban));
        }

        #endregion
    }
}