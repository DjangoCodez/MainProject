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
    [RoutePrefix("V2/Economy/SupplierAttestTemplateHeads")]
    public class SupplierTemplateHeadsController : SoeApiController
    {
        #region Variables

        private readonly AttestManager am;

        #endregion

        #region Contructor

        public SupplierTemplateHeadsController(AttestManager am)
        {
            this.am = am;
        }

        #endregion

        #region TemplateHeads

        [HttpGet]
        [Route("AttestWorkFlow/TemplateHeads/ForCurrentCompany/{entity:int}")]
        public IHttpActionResult GetAttestWorkFlowTemplateHeadsForCompany(TermGroup_AttestEntity entity)
        {
            return Content(HttpStatusCode.OK, am.GetAttestWorkFlowTemplateHeads(base.ActorCompanyId, entity).ToDTOs());
        }

        [HttpGet]
        [Route("AttestWorkFlow/TemplateHeads/Rows/{templateHeadId:int}")]
        public IHttpActionResult GetAttestWorkFlowTemplateHeadRows(int templateHeadId)
        {
            return Ok(am.GetAttestWorkFlowTemplateRows(templateHeadId).ToDTOs(true));
        }

        [HttpGet]
        [Route("AttestWorkFlow/TemplateHeads/Rows/User/{templateHeadId:int}")]
        public IHttpActionResult GetAttestWorkFlowTemplateHeadRowsWithUser(int templateHeadId)
        {
            return Ok(am.GetAttestWorkFlowRowDTOs(templateHeadId, RoleId, UserId, true));
        }

        [HttpGet]
        [Route("AttestWorkFlow/Users/ByAttestTransition/{attestTransitionId:int}")]
        public IHttpActionResult GetAttestWorkFlowUsersByAttestTransition(int attestTransitionId)
        {
            return Content(HttpStatusCode.OK, am.GetUsersByAttestTransition(attestTransitionId).ToSmallDTOs());
        }

        [HttpGet]
        [Route("AttestWorkFlow/AttestRoles/ByAttestTransition/{attestTransitionId:int}")]
        public IHttpActionResult GetAttestWorkFlowAttestRolesByAttestTransition(int attestTransitionId)
        {
            return Content(HttpStatusCode.OK, am.GetAttestRolesForAttestTransition(attestTransitionId).ToDTOs());
        }

        [HttpGet]
        [Route("AttestWorkFlow/AttestWorkFlowHead/{attestWorkFlowHeadId:int}/{setTypeName:bool}/{loadRows:bool}")]
        public IHttpActionResult GetAttestWorkFlowHead(int attestWorkFlowHeadId, bool setTypeName, bool loadRows)
        {
            return Content(HttpStatusCode.OK, am.GetAttestWorkFlowHead(attestWorkFlowHeadId, loadRows).ToDTO(setTypeName, loadRows));
        }

        [HttpGet]
        [Route("AttestWorkFlow/HeadFromInvoiceId/{invoiceId:int}/{setTypeName:bool}/{loadTemplate:bool}/{loadRows:bool}/{loadRemoved:bool}")]
        public IHttpActionResult GetAttestWorkFlowHeadFromInvoiceId(int invoiceId, bool setTypeName, bool loadTemplate, bool loadRows, bool loadRemoved)
        {
            return Content(HttpStatusCode.OK, am.GetAttestWorkFlowHeadFromInvoiceId(invoiceId, setTypeName, loadTemplate: true, loadRows, loadRemoved).ToDTO(setTypeName, loadRows));
        }

        [HttpPost]
        [Route("AttestWorkFlow/HeadFromInvoiceIds")]
        public IHttpActionResult GetAttestWorkFlowHeadFromInvoiceIds(
            [FromBody] List<int> invoiceIds)
        {
            List<AttestWorkFlowHeadDTO> results = new List<AttestWorkFlowHeadDTO>();
            if (invoiceIds?.Count > 0)
            {
                List<AttestWorkFlowHead> flowHeads = am.GetAttestWorkFlowHeadFromInvoiceIds(invoiceIds);
                results = flowHeads.ToGroupDTOs(false, false);
            }

            return Content(HttpStatusCode.OK, results);
        }


        [HttpGet]
        [Route("AttestWorkFlow/RowsFromInvoiceId/{invoiceId:int}")]
        public IHttpActionResult GetAttestWorkFlowRowsFromInvoiceId(int invoiceId)
        {
            return Content(HttpStatusCode.OK, am.GetAttestWorkFlowRowsFromRecordId(SoeEntityType.SupplierInvoice, invoiceId));
        }

        #endregion
    }
}