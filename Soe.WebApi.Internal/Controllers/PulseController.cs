using System.Web.Http;
using SoftOne.Soe.Business.Util;

namespace Soe.Api.Internal.Controllers
{
    [AllowAnonymous]
    public class PulseController : ApiController
    {
        [HttpGet]
        [Route("Pulse")]
        public IHttpActionResult Get()
        {
            return Ok(PulseManager.Pulse());
        }
    }
}
