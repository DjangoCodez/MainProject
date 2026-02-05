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

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Offer")]
    public class OfferV2Controller : SoeApiController
    {
        #region Variables

        private readonly InvoiceManager im;


        #endregion

        #region Constructor

        public OfferV2Controller(InvoiceManager im)
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

        #endregion

    }
}