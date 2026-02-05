using SoftOne.Soe.Business.Core;
using System.Net;
using System.Web.Http;
using Soe.WebApi.Models;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;
using System.Web.Http.ModelBinding;
using Soe.WebApi.Binders;
using Soe.WebApi.Controllers;
using SoftOne.Soe.Common.Util;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/PurchaseOrders")]
    public class PurchaseOrdersController : SoeApiController
    {
        #region Variables

        private readonly PurchaseManager pm;

        #endregion

        #region Constructor

        public PurchaseOrdersController(PurchaseManager pm)
        {
            this.pm = pm;
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
        public IHttpActionResult GetPurchaseList(int allItemsSelection, [ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] status, int? purchaseId=null)
        {
            return Content(HttpStatusCode.OK, pm.GetPurchaseForGrid(allItemsSelection, status, base.ActorCompanyId,purchaseId));
        }


        [HttpGet]
        [Route("Order/{purchaseId:int}")]
        public IHttpActionResult GetPurchase(int purchaseId)
        {
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

        [HttpGet]
        [Route("ForSelectDict/{forDelivery:bool}")]
        public IHttpActionResult GetOpenPurchasesForSelectDict(bool forDelivery)
        {
            return Content(HttpStatusCode.OK, pm.GetPurchaseForSelectDict(base.ActorCompanyId, forDelivery).ToSmallGenericTypes());
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

    }
}