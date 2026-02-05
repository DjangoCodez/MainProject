using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using SoftOne.Soe.Common.Util;
using Soe.WebApi.Controllers;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using SoftOne.Soe.Business;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/PriceBasedMarkup")]
    public class PriceBasedMarkupController : SoeApiController
    {
        #region Variables

        private readonly MarkupManager mm;
        private readonly ProductPricelistManager pplm;

        #endregion

        #region Constructor

        public PriceBasedMarkupController(MarkupManager mm, ProductPricelistManager pplm)
        {
            this.mm = mm;
            this.pplm = pplm;
        }
        #endregion

        #region PriceBasedMarkup

        [HttpGet]
        [Route("GetPriceBasedMarkup/{id}")]
        public IHttpActionResult GetPriceBasedMarkup(int id)
        {
            using CompEntities entitiesReadOnly = CompEntitiesProvider.LeaseReadOnlyContext();
            return Content(HttpStatusCode.OK, mm.GetPriceBasedMarkup(entitiesReadOnly,base.ActorCompanyId, id).ToDTO());
        }

        [HttpGet]
        [Route("Grid/{priceBasedMarkupId:int?}")]
        public IHttpActionResult GetPriceBasedMarkupGrid(int? priceBasedMarkupId = null)
        {
            return Content(HttpStatusCode.OK, mm.GetPriceBasedMarkups(base.ActorCompanyId, priceBasedMarkupId));
        }

        [HttpPost]
        [Route("Markup/PriceBased")]
        public IHttpActionResult SavePriceBasedMarkup(List<PriceBasedMarkupDTO> priceBasedMarkup)
        {
            return Content(HttpStatusCode.OK, mm.SavePriceBasedMarkup2(priceBasedMarkup, base.ActorCompanyId));
        }

        //This can be removed
        [HttpDelete]
        [Route("Markup/PriceBased/{priceBaseMarkupId:int}")]
        public IHttpActionResult DeletePriceBasedMarkup(int priceBaseMarkupId)
        {
            return Content(HttpStatusCode.OK, mm.DeletePriceBasedMarkup(priceBaseMarkupId));
        }

        [HttpGet]
        [Route("PriceList/")]
        public IHttpActionResult GetPriceLists(HttpRequestMessage message)
        {
            var pplist = pplm.GetPriceListTypesDict(base.ActorCompanyId, false).ToSmallGenericTypes();
            return Content(HttpStatusCode.OK, pplist);
        }

        #endregion
    }
}
