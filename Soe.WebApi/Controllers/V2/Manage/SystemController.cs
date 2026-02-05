using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Banker;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.Communicator;
using SoftOne.Soe.Common.Util;
using System.Net;
using System.Web.Http;
using Soe.WebApi.Models;

namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Manage/System")]
    [SupportUserAuthorize]
    public class SystemController : SoeApiController
    {
        #region Variables

        private readonly SettingManager sm;
        private readonly CountryCurrencyManager ccm;
        private readonly CommunicationManager cm;

        #endregion

        #region Constructor

        public SystemController(SettingManager sm, CountryCurrencyManager ccm, CommunicationManager cm)
        {
            this.sm = sm;
            this.ccm = ccm;
            this.cm = cm;
        }

        #endregion

        #region Bankintegration

        [HttpGet]
        [Route("Bankintegration/Request/Grid/{fileType:int}")]
        public IHttpActionResult GetBankintegrationRequestGrid(int fileType)
        {
            return Content(HttpStatusCode.OK, BankerConnector.GetDownloadRequest(sm, new SoeBankerRequestFilterDTO {MaterialType= fileType}));
        }

        [HttpPost]
        [Route("Bankintegration/Request/Search/")]
        public IHttpActionResult SearchBankintegrationRequest(SoeBankerRequestFilterDTO filter)
        {
            return Content(HttpStatusCode.OK, BankerConnector.GetDownloadRequest(sm, filter));
        }

        [HttpGet]
        [Route("Bankintegration/Request/Files/{requestId:int}")]
        public IHttpActionResult GetBankintegrationRequestFiles(int requestId)
        {
            return Content(HttpStatusCode.OK, BankerConnector.GetDownloadFiles(sm, requestId));
        }

        [HttpGet]
        [Route("Bankintegration/Onboarding/Grid")]
        public IHttpActionResult GetBankintegrationOnboardingGrid()
        {
            return Content(HttpStatusCode.OK, BankerConnector.GetOnboardingRequests(sm));
        }

        [HttpPost]
        [Route("Bankintegration/Onboarding/SendAuthorizationResponse")]
        public IHttpActionResult SendAuthorizationResponse(SoeBankerAuthorizationRequestModel model)
        {
            var username = $"{this.ParameterObject.LoginName} ({this.ParameterObject.UserId})";
            return Content(HttpStatusCode.OK, BankerConnector.SendOnboardingAuthorizationResponse(sm, username, model.OnboardingRequestIds));
        }

        [HttpGet]
        [Route("Bankintegration/Onboarding/{onboardingrequestId:int}")]
        public IHttpActionResult GetBankintegrationOnboarding(int onboardingrequestId)
        {
            return Content(HttpStatusCode.OK, new ActionResult());
        }

        [HttpGet]
        [Route("Bankintegration/Banks")]
        public IHttpActionResult GetBankintegrationBanks()
        {
            return Content(HttpStatusCode.OK, ccm.GetSysBanksForIntegration());
        }

        #endregion

        #region Communicator

        [HttpPost]
        [Route("Communicator/IncomingEmail/Grid")]
        public IHttpActionResult GetIncomingEmails(IncomingEmailFilterDTO filter)
        {
            return Content(HttpStatusCode.OK, cm.GetIncomingEmailGridDTOs(filter));
        }

        [HttpGet]
        [Route("Communicator/IncomingEmail/{incomingEmailId:int}")]
        public IHttpActionResult GetIncomingEmail(int incomingEmailId)
        {
            return Content(HttpStatusCode.OK, cm.GetIncomingEmailDTO(incomingEmailId));
        }

        [HttpGet]
        [Route("Communicator/IncomingEmail/Attachment/{attachmentId:int}")]
        public IHttpActionResult GetIncomingEmailAttachment(int attachmentId)
        {
            return Content(HttpStatusCode.OK, cm.GetIncomingEmailAttachment(attachmentId));
        }

        #endregion
    }
}