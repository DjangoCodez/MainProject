using SoftOne.Soe.Business.Core;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Manage
{
    [RoutePrefix("Manage/License")]
    public class LicenseController : SoeApiController
    {
        #region Variables

        private readonly LicenseManager lm;

        #endregion

        #region Constructor

        public LicenseController(LicenseManager lm)
        {
            this.lm = lm;
        }

        #endregion    

        [HttpGet]
        [Route("License/{licenseId:int}")]
        public IHttpActionResult GetLicense(int licenseId)
        {
            return Content(HttpStatusCode.OK, lm.GetLicense(licenseId));
        }
    }
}