using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Billing
{
    [RoutePrefix("Billing/Contract")]
    public class ContractController : SoeApiController
    {
        #region Variables

        private readonly ContractManager cm;
        private readonly InvoiceManager im;

        #endregion

        #region Constructor

        public ContractController(ContractManager cm, InvoiceManager im)
        {
            this.cm = cm;
            this.im = im;
        }

        #endregion

        #region Contract

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveContract(SaveOrderModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveContract(model.ModifiedFields, model.NewRows, model.ModifiedRows, model.OriginUsers, model.Files, model.DiscardConcurrencyCheck, model.RegenerateAccounting));
        }

        #endregion

        #region ContractGroup

        [HttpGet]
        [Route("ContractGroup/")]
        public IHttpActionResult GetContractGroups()
        {
            return Content(HttpStatusCode.OK, cm.GetContractGroupsExtended(base.ActorCompanyId));
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

        #region ContractPrices

        [HttpPost]
        [Route("UpdatePrices/")]
        public IHttpActionResult UpdateContractPrices(UpdateContractPricesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.UpdateContractPrices(model.Percent, model.Amount, model.Rounding, model.InvoiceIds));
        }

        #endregion
    }
}