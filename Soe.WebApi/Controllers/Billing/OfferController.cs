using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Billing
{
    [RoutePrefix("Billing/Offer")]
    public class OfferController : SoeApiController
    {
        #region Variables

        private readonly InvoiceManager im;

        #endregion

        #region Constructor

        public OfferController(InvoiceManager im)
        {
            this.im = im;
        }

        #endregion

        #region Offer

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveOffer(SaveOrderModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveOffer(model.ModifiedFields, model.NewRows, model.ModifiedRows, model.OriginUsers, model.Files, model.DiscardConcurrencyCheck, model.RegenerateAccounting));
        }

        [HttpGet]
        [Route("GetOfferTraceViews/{offerId:int}")]
        public IHttpActionResult GetOfferTraceViews(int offerId)
        {
            CountryCurrencyManager ccm = new CountryCurrencyManager(null);
            int baseSysCurrencyId = ccm.GetCompanyBaseSysCurrencyId(base.ActorCompanyId);

            return Content(HttpStatusCode.OK, im.GetOfferTraceViews(offerId, baseSysCurrencyId));
        }

        [HttpPost]
        [Route("Unlock/{offerId:int}")]
        public IHttpActionResult UnlockOffer(int offerId)
        {
            return Content(HttpStatusCode.OK, im.UnlockCustomerInvoice(offerId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Close/{offerId:int}")]
        public IHttpActionResult CloseOrder(int offerId)
        {
            return Content(HttpStatusCode.OK, im.CloseOrderOffer(offerId));
        }

        #endregion

    }
}