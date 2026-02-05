using System.Net;
using System.Net.Http;
using System.Web.Http;
using Soe.WebApi.Controllers;
using Soe.WebApi.Extensions;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("Billing/InvoicePriceLists")]
    public class InvoicePriceListsController : SoeApiController
    {
        #region Variables

        private readonly ProductPricelistManager pplm;

        #endregion

        #region Constructor

        public InvoicePriceListsController(ProductPricelistManager pplm)
        {
            this.pplm = pplm;
        }

        #endregion

        #region PriceList

        [HttpGet]
        [Route("")]
        public IHttpActionResult GetPriceLists(HttpRequestMessage message, [FromUri] bool addEmptyRow)
        {
            // TODO: ACCEPT types should not be used
            // Angular client was passing ACCEPT_GENERIC_TYPE, but that is now removed,
            // that is why this code is commented out

            //if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
            return Content(HttpStatusCode.OK, pplm.GetPriceListTypesDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
            //else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
            //    return Content(HttpStatusCode.OK, pplm.GetPriceListTypesForGrid(base.ActorCompanyId));

            //return Content(HttpStatusCode.OK, pplm.GetPriceListTypes(base.ActorCompanyId).ToDTOs());
        }

        #endregion
    }
}