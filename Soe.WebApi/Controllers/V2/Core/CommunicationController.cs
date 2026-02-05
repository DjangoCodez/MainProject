using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Core
{
    [RoutePrefix("V2/Core/Communication")]
    public class CommunicationController : SoeApiController
    {
        #region Variables

        private readonly CommunicationManager cm;

        #endregion

        #region Constructor

        public CommunicationController(CommunicationManager cm)
        {
            this.cm = cm;
        }

        #endregion

        #region Message

        [HttpGet]
        [Route("Message/NbrOfUnreadMessages/")]
        public IHttpActionResult GetNbrOfUnreadMessages()
        {
            return Content(HttpStatusCode.OK, cm.GetNbrOfUnreadMessages(base.LicenseId, base.UserId));
        }

        #endregion
    }
}