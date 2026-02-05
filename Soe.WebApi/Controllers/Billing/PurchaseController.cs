using System;
using SoftOne.Soe.Business.Core;
using System.Net;
using System.Web.Http;
using Soe.WebApi.Models;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;
using System.Web.Http.ModelBinding;
using Soe.WebApi.Binders;
using SoftOne.Soe.Common.Attributes;
using Soe.WebApi.Extensions;
using System.Net.Http;

namespace Soe.WebApi.Controllers.Billing
{
    [RoutePrefix("Billing/Purchase")]
    public class PurchaseController : SoeApiController
    {
        #region Variables

        private readonly PurchaseManager pm;
        private readonly PurchaseDeliveryManager pdm;

        #endregion

        #region Constructor

        public PurchaseController(PurchaseManager pm, PurchaseDeliveryManager pdm)
        {
            this.pm = pm;
            this.pdm = pdm;
        }

        #endregion

        #region Purchase

        [HttpGet]
        [Route("Status")]
        public IHttpActionResult GetPurchaseStatus()
        {
            return Content(HttpStatusCode.OK, pm.GetPurchaseStatus());
        }

        [HttpGet]
        [Route("Orders")]
        public IHttpActionResult GetPurchaseList(int allItemsSelection, [ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] status)
        {
            return Content(HttpStatusCode.OK, pm.GetPurchaseForGrid(allItemsSelection, status, base.ActorCompanyId));
        }


        [HttpGet]
        [Route("Order/{purchaseId:int}")]
        public IHttpActionResult GetPurchase(HttpRequestMessage message,int purchaseId)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_SMALL_DTO))
                return Content(HttpStatusCode.OK, pm.GetPurchaseSmallDTO(purchaseId, base.ActorCompanyId));
            else
                return Content(HttpStatusCode.OK, pm.GetPurchase(purchaseId, false, base.ActorCompanyId).ToDTO());
        }

        [HttpGet]
        [Route("Order/Rows/{purchaseId:int}")]
        public IHttpActionResult GetPurchaseRows(int purchaseId)
        {
            return Content(HttpStatusCode.OK, pm.GetPurchaseRows(purchaseId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Order/Rows/ForOrder/{invoiceId:int}")]
        public IHttpActionResult GetPurchaseRowsForOrder(int invoiceId)
        {
            return Content(HttpStatusCode.OK, pm.GetPurchaseRowsForOrder(invoiceId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("TraceViews/{purchaseId:int}")]
        public IHttpActionResult GetPurchaseTraceViews(int purchaseId)
        {
            return Content(HttpStatusCode.OK, pm.GetPurchaseTraceViews(purchaseId));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SavePurchase(SavePurchaseModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SavePurchase(model.ModifiedFields, model.OriginUsers, model.NewRows, model.ModifiedRows));
        }

        [HttpPost]
        [Route("Status")]
        public IHttpActionResult SavePurchaseStatus(SavePurchaseStatus model)
        {
            return Content(HttpStatusCode.OK, pm.SavePurchaseStatus(model.PurchaseId,model.Status));
        }

        [HttpDelete]
        [Route("{purchaseId:int}")]
        public IHttpActionResult DeletePurchase(int purchaseId)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.DeletePurchase(purchaseId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("ForSelect/{forDelivery:bool}")]
        public IHttpActionResult GetOpenPurchasesForSelect(bool forDelivery)
        {
            return Content(HttpStatusCode.OK, pm.GetPurchaseForSelect(base.ActorCompanyId, forDelivery));
        }

        [HttpPost]
        [Route("UpdatePurchaseFromOrder")]
        public IHttpActionResult UpdatePurchaseFromOrder(PurchaseFromOrderDTO dto)
        {
            return Content(HttpStatusCode.OK, pm.UpdatePurchaseFromOrder(dto));
        }

        [HttpPost]
        [Route("CreatePurchaseFromStockSuggestion")]
        public IHttpActionResult CreatePurchaseFromStockSuggestion(List<PurchaseRowFromStockDTO> rows)
        {
            return Content(HttpStatusCode.OK, pm.CreatePurchaseFromStock(this.ActorCompanyId, rows));
        }

        [HttpPost]
        [Route("Order/Email")]
        public IHttpActionResult SendPurchaseAsEmail(SendPurchaseEmail dto)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SendPurchaseAsEmail(base.ActorCompanyId, dto.PurchaseId, dto.ReportId, dto.EmailTemplateId, dto.LangId, dto.Recipients, dto.SingleRecipient));
        }
        [HttpPost]
        [Route("Orders/Email")]
        public IHttpActionResult SendPurchasesAsEmail(SendPurchaseEmail dto)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SendPurchasesAsEmail(base.ActorCompanyId, dto.PurchaseIds, dto.EmailTemplateId, dto.LangId));
        }
        [HttpGet]
        [Route("DeliveryAddresses/{customerOrderId:int}")]
        public IHttpActionResult GetDeliveryAddresses(int customerOrderId)
        {
            return Content(HttpStatusCode.OK, pm.GetContactAddresesForPurchase(customerOrderId));
        }

        /*[HttpGet]
        [Route("TraceViews/{purchaseId:int}")]
        public IHttpActionResult GetOfferTraceViews(int purchaseId)
        {
            CountryCurrencyManager ccm = new CountryCurrencyManager(null);
            int baseSysCurrencyId = ccm.GetCompanyBaseSysCurrencyId(base.ActorCompanyId);

            return Content(HttpStatusCode.OK, null);
        }*/

        #endregion

        #region Delivery

        [HttpGet]
        [Route("Deliveries/{allItemsSelection:int}")]
        public IHttpActionResult GetDeliveryList(int allItemsSelection)
        {
            return Content(HttpStatusCode.OK, pdm.GetDeliveryForGrid(allItemsSelection, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Delivery/{purchaseDeliveryId:int}")]
        public IHttpActionResult GetDelivery(int purchaseDeliveryId)
        {
            return Content(HttpStatusCode.OK, pdm.GetPurchaseDeliveryDTO(purchaseDeliveryId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Delivery/PurchaseRows/{purchaseId:int}/{supplierId:int}")]
        public IHttpActionResult GetDeliveryRowsFromPurchase(int purchaseId, int supplierId)
        {
            return Content(HttpStatusCode.OK, pdm.GetDeliveryRowsFromPurchase(purchaseId, supplierId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("DeliveryRows/{purchaseId:int}")]
        public IHttpActionResult GetPurchaseDeliveryRowsByPurchaseId(int purchaseId)
        {
            return Content(HttpStatusCode.OK, pdm.GetPurchaseDeliveryRowsByPurchaseId(purchaseId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Delivery/Rows/{purchaseDeliveryId:int}")]
        public IHttpActionResult GetDeliveryRows(int purchaseDeliveryId)
        {
            return Content(HttpStatusCode.OK, pdm.GetDeliveryRows(purchaseDeliveryId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Delivery")]
        public IHttpActionResult SaveDelivery(PurchaseDeliverySaveDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pdm.SaveDelivery(model, base.ActorCompanyId));
        }

        #endregion

        #region CustomerInvoiceRows
        [HttpGet]
        [Route("CustomerInvoiceRows/{viewType:int}/{id:int}")]
        public IHttpActionResult GetCustomerInvoiceRows(int viewType, int id)
        {
            if (viewType == 0 || id == 0)
                return Error(HttpStatusCode.BadRequest, null, null, null);
            else
                return Content(HttpStatusCode.OK, pm.GetCustomerInvoiceRows(this.ActorCompanyId, viewType, id));
        }
        #endregion

        [HttpPost]
        [Route("Statistics/")]
        public IHttpActionResult GetPurchaseStatistics(GeneralProductStatisticsModel model)
        {
            return Content(HttpStatusCode.OK, pm.GetPurchaseStatistics(model.FromDate, model.ToDate));
        }
    }
}