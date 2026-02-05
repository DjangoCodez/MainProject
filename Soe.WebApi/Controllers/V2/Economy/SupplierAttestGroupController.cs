using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Soe.WebApi.Controllers;
using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/SupplierAttestGroup")]
    public class SupplierAttestGroupController : SoeApiController
    {
        #region Variables

        private readonly AttestManager am;

        #endregion

        #region Contructor

        public SupplierAttestGroupController(AttestManager am)
        {
            this.am = am;
        }

        #endregion

        #region AttestGroup

        [HttpGet]
        [Route("AttestWorkFlow/AttestGroup/")]
        public IHttpActionResult GetAttestWorkFlowGroupsDict([FromUri] bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, am.GetAttestWorkFlowGroups(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("AttestWorkFlow/AttestGroups/")]
        public IHttpActionResult GetAttestWorkFlowGroups(HttpRequestMessage message, [FromUri] bool addEmptyRow, [FromUri] int? attestWorkFlowHeadId)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, am.GetAttestWorkFlowGroups(base.ActorCompanyId, addEmptyRow, attestWorkFlowHeadId).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, am.GetAttestWorkFlowGroups(base.ActorCompanyId, attestWorkFlowHeadId));
        }

        [HttpGet]
        [Route("AttestWorkFlow/AttestGroup/ById/{id:int}")]
        public IHttpActionResult GetAttestWorkFlowGroup(int id)
        {
            return Content(HttpStatusCode.OK, am.GetAttestWorkFlowGroup(id, base.ActorCompanyId, true).ToAttestGroupDTO(false, false));
        }

        [HttpPost]
        [Route("AttestWorkFlow/AttestGroup/Suggestion")]
        public IHttpActionResult getAttestGroupSuggestion(GetAttestGroupSuggestion model)
        {
            return Content(HttpStatusCode.OK, am.GetAttestGroupSuggestion(base.ActorCompanyId, model.SupplierId, model.ProjectId, model.CostplaceAccountId, model.ReferenceOur).ToAttestGroupDTO(false, false));
        }

        [HttpPost]
        [Route("AttestWorkFlow/AttestGroup/SaveAttestWorkFlow")]
        public IHttpActionResult SaveAttestWorkFlow(AttestGroupDTO head)
        {
            return Content(HttpStatusCode.OK, am.SaveAttestWorkFlow(head, head.Rows, head.SendMessage, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("AttestWorkFlow/AttestGroup/SaveAttestWorkFlowMultiple")]
        public IHttpActionResult SaveAttestWorkFlowMultiple(SaveAttestWorkFlowForMultipleInvoicesModel model)
        {
            return Content(HttpStatusCode.OK, am.SaveAttestWorkFlowForMultipleInvoices(model.AttestWorkFlowHead, model.InvoiceIds, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("AttestWorkFlow/AttestGroup/SaveAttestWorkFlowForInvoices")]
        public IHttpActionResult SaveAttestWorkFlowForInvoices(SaveAttestWorkFlowForInvoicesModel model)
        {
            return Content(HttpStatusCode.OK, am.SaveAttestWorkFlowForInvoices(model.IdsToTransfer, ActorCompanyId, model.SendMessage));
        }

        [HttpDelete]
        [Route("AttestWorkFlow/AttestGroup/DeleteAttestWorkFlow/{attestWorkFlowHeadId:int}")]
        public IHttpActionResult DeleteAttestWorkFlow(int attestWorkFlowHeadId)
        {
            return Content(HttpStatusCode.OK, am.DeleteAttestWorkFlowHead(attestWorkFlowHeadId));
        }

        [HttpDelete]
        [Route("AttestWorkFlow/AttestGroup/DeleteAttestWorkFlows/{attestWorkFlowHeadIds}")]
        public IHttpActionResult DeleteAttestWorkFlows(string attestWorkFlowHeadIds)
        {
            List<int> ids = StringUtility.SplitNumericList(attestWorkFlowHeadIds, true, false);
            return Content(HttpStatusCode.OK, am.DeleteAttestWorkFlowHeads(ids));
        }

        #endregion
    }
}