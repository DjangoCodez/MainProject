using SoftOne.Communicator.Shared.DTO;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Business.Util.LogCollector;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;

namespace Soe.Api.Internal.Controllers.Internal.License
{
    [RoutePrefix("Internal/Communicator/Communicator")]
    public class InternalCommunicatorController : ApiBase
    {
        #region Constructor

        public InternalCommunicatorController(WebApiInternalParamObject webApiInternalParamObject) : base(webApiInternalParamObject)
        {

        }

        #endregion

        #region Methods
        /// <summary>
        ///Check if key is valid in this database.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CommunicatorMessage/key")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult CheckKey(string key)
        {
            ActionResult result = new ActionResult();
            key = key.ToLower();
            using (CompEntities entities = new CompEntities())
            {
                var hits = entities.CompanyExternalCode.Where(w => w.State == (int)SoeEntityState.Active && w.Entity == (int)TermGroup_CompanyExternalCodeEntity.InboundEmailMessageEmail && w.ExternalCode == key).ToList();

                if (hits.Count == 1)
                {
                    result.IntegerValue = hits.First().ActorCompanyId;
                    result.IntegerValue2 = CompDbCache.Instance.SysCompDbId;
                }
            }
            return Content(HttpStatusCode.OK, result);
        }


        /// <summary>
        /// Key is reciepient email, message must be to same recipient
        /// </summary>
        /// <param name="key"></param>
        /// <param name="actorCompanyId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CommunicatorMessage")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult HandleCommunicatorMessage(string key, int actorCompanyId, int sysCompDBId, CommunicatorMessage message)
        {
            if (message == null)
                return Content(HttpStatusCode.BadRequest, new ActionResult("message is null"));

            if (key.ToLower() != message.Recievers[0].Email.ToLower())
                return Content(HttpStatusCode.BadRequest, new ActionResult("Key is invalid"));

            if (sysCompDBId == 0)
                return Content(HttpStatusCode.BadRequest, new ActionResult("sysCompDBId is invalid"));

            if (actorCompanyId == 0)
                return Content(HttpStatusCode.BadRequest, new ActionResult("actorCompanyId is invalid"));

            InboundEmailManager inboundEmailManager = new InboundEmailManager(null);
            var result = inboundEmailManager.HandleInboundEmail(actorCompanyId, message);

            if (result.Success)
                return Content(HttpStatusCode.OK, result);

            switch ((ActionResultSave)result.ErrorNumber) {
                case ActionResultSave.ScanningFailed_CreateScanningEntry:
                case ActionResultSave.ScanningFailed_ExtractInterpretationAll:
                case ActionResultSave.ScanningFailed_ParseRawFile:
                case ActionResultSave.ScanningFailed_UploadToDataStorage:
                case ActionResultSave.ScanningFailed_NotActivatedAtProvider:
                        return Content(HttpStatusCode.InternalServerError, result);
                default:
                    return Content(HttpStatusCode.OK, result);
            }
        }

        /// <summary>
        /// Key is reciepient email, message must be to same recipient
        /// </summary>
        /// <param name="key"></param>
        /// <param name="actorCompanyId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CommunicatorMessage/Event")]
        [ResponseType(typeof(ActionResult))]
        public IHttpActionResult HandleCommunicatorMessageEvent(List<CommunicatorMessageEvent> messages)
        {
            if (messages.IsNullOrEmpty())
                return Content(HttpStatusCode.BadRequest, new ActionResult("message is null"));

            var result = new ActionResult();
            var invoiceDistributionManager = new InvoiceDistributionManager(null);
            
            invoiceDistributionManager.UpdateEmailStatusFromCommunicator(messages);

            return Content(HttpStatusCode.OK, result);
        }

        #endregion
    }
}