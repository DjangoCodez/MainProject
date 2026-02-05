using Newtonsoft.Json;
using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using SoftOneId.Common.DTO;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.Internal.License
{
    [RoutePrefix("Internal/Signature/Signature")]
    public class InternalSignatureController : ApiBase
    {
        #region Constructor

        public InternalSignatureController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {

        }

        #endregion

        #region Methods

        [HttpPost]
        [Route("Attachment/")]
        [ResponseType(typeof(CommunicatorMessageAttachment))]
        public IHttpActionResult GetSignatureFile(CommunicatorCredentials communicatorCredentials)
        {
            var generalManager = new GeneralManager(null);
            var data = generalManager.GetDataStorage(communicatorCredentials.KeyPrimary, communicatorCredentials.TypeKey)?.Data;
            return Content(HttpStatusCode.OK, new CommunicatorMessageAttachment() { DataBase64 = data, Name = communicatorCredentials.KeyPrimary });
        }

        [HttpPost]
        [Route("Attachment/Update")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult UpdateSignatureFile(CommunicatorMessageAttachment attachment)
        {
            var generalManager = new GeneralManager(null);
            var result = generalManager.UpdateDataStorage(attachment);
            return Content(HttpStatusCode.OK, result);
        }

        [HttpPost]
        [Route("Answer")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult SaveDocumentSigningAnswer(IdSigneeAnswerDTO idSigneeAnswer)
        {
            var userManager = new UserManager(null);
            var users = userManager.GetUsers(idSigneeAnswer.IdLoginGuid);

            if (users.IsNullOrEmpty())
                return Content(HttpStatusCode.BadRequest, new ActionResult("User not found"));
            
            User selectedUser = null;
            foreach (var user in users)
            {
                if (selectedUser != null)
                    continue;

                if (userManager.UserValidOnCompany(idSigneeAnswer.ActorCompanyId, user.UserId))
                    selectedUser = user;
            }

            if (selectedUser == null)
                selectedUser = users[0];

            if (!userManager.UserValidOnCompany(idSigneeAnswer.ActorCompanyId, selectedUser.UserId))
                return Content(HttpStatusCode.BadRequest, new ActionResult("Invalid user on company"));

            var attestManager = new AttestManager(GetParameterObject(idSigneeAnswer.ActorCompanyId, selectedUser.UserId));

            if (idSigneeAnswer.Base64Data == null)
                LogCollector.LogError("SaveDocumentSigningAnswer is missing base64Data, json:" + JsonConvert.SerializeObject(idSigneeAnswer));

            return Content(HttpStatusCode.OK, attestManager.SaveDocumentSigningAnswer(idSigneeAnswer.AttestWorkFlowRowId, (SoftOne.Soe.Common.Util.SigneeStatus)idSigneeAnswer.SigneeStatus, idSigneeAnswer.Comment, selectedUser.UserId, idSigneeAnswer.ActorCompanyId, selectedUser.LicenseId, idSigneeAnswer.Base64Data));
        }

        #endregion
    }
}