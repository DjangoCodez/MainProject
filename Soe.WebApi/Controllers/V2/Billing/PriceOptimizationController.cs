using Soe.WebApi.Binders;
using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/PriceOptimization")]
    public class PriceOptimizationController : SoeApiController
    {
        #region Variables

        private readonly PriceOptimizationManager pcm;

        #endregion

        #region Constructor

        public PriceOptimizationController(PriceOptimizationManager pcm)
        {
            this.pcm = pcm;
        }

        #endregion

        #region PriceOptimization

        [HttpGet]
        [Route("Grid/{priceOptimizationId:int?}")]
        public IHttpActionResult GetPriceOptimizationsForGrid(int allItemsSelection, [ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] status, int? priceOptimizationId = null)
        {
            return Content(HttpStatusCode.OK, pcm.GetPriceOptimizationsForGrid(allItemsSelection, status, base.ActorCompanyId, priceOptimizationId));
        }

        [HttpGet]
        [Route("{priceOptimizationId:int}")]
        public IHttpActionResult GetPriceOptimization(int priceOptimizationId)
        {
            return Content(HttpStatusCode.OK, pcm.GetPriceOptimization(priceOptimizationId, base.ActorCompanyId).ToDTO());
        }
        
        [HttpPost]
        [Route("")]
        public IHttpActionResult SavePriceOptimization(PurchaseCartDTO priceOptimization)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pcm.SavePriceOptimization(priceOptimization, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("DeletePriceOptimizations")]
        public IHttpActionResult DeletePriceOptimizations(List<PurchaseCartDTO> priceOptimizationModel)
        {
            return Content(HttpStatusCode.OK, pcm.DeletePriceOptimizations(priceOptimizationModel, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("ChangeStatus")]
        public IHttpActionResult ChangePriceOptimizationStatus([FromBody] ChangeCartStateModel model)
        {
            if (model == null) return BadRequest("No body!");
            else
                return Content(HttpStatusCode.OK, pcm.ChangePriceOptimizationStatus(model.Ids, model.StateTo));
        }

        [HttpDelete]
        [Route("{priceOptimizationId:int}")]
        public IHttpActionResult DeletePriceOptimization(int priceOptimizationId)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pcm.Delete(priceOptimizationId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("TraceViews/{priceOptimizationId:int}")]
        public IHttpActionResult GetPriceOptimizationTraceRows(int priceOptimizationId)
        {
            return Content(HttpStatusCode.OK, pcm.GetPriceOptimizationTraceRows(priceOptimizationId, base.ActorCompanyId));
        }

        #endregion

        #region PriceOptimizationRow

        [HttpGet]
        [Route("PriceOptimizationRow/{priceOptimizationId:int}")]
        public IHttpActionResult GetPriceOptimizationRow(int priceOptimizationId)
        {
            return Content(HttpStatusCode.OK, pcm.GetPriceOptimizationRow(priceOptimizationId));
        }

        [HttpPost]
        [Route("PriceOptimizationRow/Prices")]
        public IHttpActionResult GetPriceOptimizationRowPrices([FromBody] List<int> sysProductIds)
        {
            return Content(HttpStatusCode.OK, pcm.GetPriceOptimizationRowPrices(sysProductIds));
        }
        
        [HttpPost]
        [Route("PriceOptimizationRow/Transfer")]
        public IHttpActionResult TransferPriceOptimizationRowsToOrder(TransferInvoiceDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pcm.TransferPriceOptimizationRowsToOrderOffer(model.InvoiceId, model.PurchaseCartId));
        }

        #endregion

    }
}