using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using Soe.WebApi.Controllers;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Purchase")]
    public class PurchaseDeliveryController : SoeApiController
    {
        #region Variables

        private readonly PurchaseDeliveryManager pdm;

        #endregion

        #region Constructor

        public PurchaseDeliveryController(PurchaseDeliveryManager pdm)
        {
            this.pdm = pdm;
        }

        #endregion


        #region Delivery

        [HttpGet]
        [Route("Deliveries/{allItemsSelection:int}")]
        public IHttpActionResult GetDeliveryList(int allItemsSelection, int? purchaseDeliveryId = null)
        {
            return Content(HttpStatusCode.OK, pdm.GetDeliveryForGrid(allItemsSelection, base.ActorCompanyId, purchaseDeliveryId));
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

    }
}