using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/Information")]
    public class InformationController : SoeApiController
    {
        #region Variables

        private readonly GeneralManager gm;
        private readonly LanguageManager lm;

        #endregion

        #region Constructor

        public InformationController(GeneralManager gm, LanguageManager lm)
        {
            this.gm = gm;
            this.lm = lm;
        }

        #endregion

        #region Information

        [HttpGet]
        [Route("NewSince/{time}")]
        public IHttpActionResult HasNewInformations(string time)
        {
            return Content(HttpStatusCode.OK, gm.HasNewInformations(base.ActorCompanyId, BuildDateTimeFromString(time, false).Value));
        }

        [HttpGet]
        [Route("UnreadCount/{language}")]
        public IHttpActionResult GetNbrOfUnreadInformations(string language)
        {
            return Content(HttpStatusCode.OK, gm.GetNbrOfUnreadInformations(base.LicenseId, base.ActorCompanyId, base.RoleId, base.UserId, true, false, false, lm.GetSysLanguageId(language)));
        }

        [HttpGet]
        [Route("Unread/Severe/{language}")]
        public IHttpActionResult HasSevereUnreadInformation(string language)
        {
            return Content(HttpStatusCode.OK, gm.HasSevereUnreadInformation(base.LicenseId, base.ActorCompanyId, base.RoleId, base.UserId, true, false, false, lm.GetSysLanguageId(language)));
        }

        #endregion
    }
}