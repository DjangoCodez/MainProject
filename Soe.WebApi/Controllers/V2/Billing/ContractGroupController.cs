using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System;
using SoftOne.Soe.Common.DTO;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Contract")]
    public class ContractGroupController : SoeApiController
    {
        #region Variables

        private readonly InvoiceManager im;
        private readonly ContractManager cm;


        #endregion

        #region Constructor

        public ContractGroupController(InvoiceManager im, ContractManager cm)
        {
            this.im = im;
            this.cm = cm;
        }

        #endregion


        #region ContractGroup

        [HttpGet]
        [Route("ContractGroup/")]
        public IHttpActionResult GetContractGroups(int? id = null)
        {
            return Content(HttpStatusCode.OK, cm.GetContractGroupsExtended(base.ActorCompanyId, id));
        }

        [HttpGet]
        [Route("ContractGroup/{contractGroupId:int}")]
        public IHttpActionResult GetContractGroup(int contractGroupId)
        {
            return Content(HttpStatusCode.OK, cm.GetContractGroup(contractGroupId).ToDTO());
        }

        [HttpGet]
        [Route("GetContractTraceViews/{contractId:int}")]
        public IHttpActionResult GetContractTraceViews(int contractId)
        {
            CountryCurrencyManager ccm = new CountryCurrencyManager(null);
            int baseSysCurrencyId = ccm.GetCompanyBaseSysCurrencyId(base.ActorCompanyId);

            return Content(HttpStatusCode.OK, im.GetContractTraceViews(contractId, baseSysCurrencyId));
        }

        [HttpPost]
        [Route("ContractGroup/")]
        public IHttpActionResult SaveContractGroup(ContractGroupDTO contractGroup)
        {
            return Content(HttpStatusCode.OK, cm.SaveContractGroup(contractGroup, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("ContractGroup/{contractGroupId:int}")]
        public IHttpActionResult DeleteContractGroup(int contractGroupId)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, cm.DeleteContractGroup(contractGroupId));
        }

        #endregion

    }
}