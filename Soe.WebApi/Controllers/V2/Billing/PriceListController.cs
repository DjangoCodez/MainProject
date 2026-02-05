using System.Net;
using System.Linq;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using Soe.WebApi.Models;
using Soe.WebApi.Controllers;


namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/PriceList")]
    public class PriceListController : SoeApiController
    {
        #region Variables

        private readonly ProductPricelistManager pplm;
        private readonly ProductManager prom;

        #endregion

        #region Constructor

        public PriceListController(ProductPricelistManager pplm, ProductManager prom)
        {
            this.pplm = pplm;
            this.prom = prom;
        }

        #endregion

        #region PriceListType

        [HttpGet]
        [Route("PriceListTypes/Grid")]
        public IHttpActionResult GetPriceListTypesGrid(int? priceListTypeId = null)
        {
            return Content(HttpStatusCode.OK, pplm.GetPriceListTypesForGrid(base.ActorCompanyId, priceListTypeId));
        }

        [HttpGet]
        [Route("PriceListTypes/")]
        public IHttpActionResult GetPriceListTypes()
        {
            return Content(HttpStatusCode.OK, pplm.GetPriceListTypes(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("PriceListTypes/{priceListTypeId:int}")]
        public IHttpActionResult GetPriceListType(int priceListTypeId)
        {
            return Content(HttpStatusCode.OK, pplm.GetPriceListType(priceListTypeId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("PriceList")]
        public IHttpActionResult SavePriceList(PriceListTypeDTO priceListTypeDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pplm.SavePriceListTypeDTO(priceListTypeDTO, base.ActorCompanyId));
        }


        [HttpPost]
        [Route("PriceListTypes/")]
        public IHttpActionResult SavePriceListType(SavePriceListTypeModel model)
        {
            return Content(HttpStatusCode.OK, pplm.SavePriceListType(model.PriceListType, model.PriceLists));
        }

        [HttpDelete]
        [Route("PriceListTypes/{priceListTypeId:int}")]
        public IHttpActionResult DeletePriceListType(int priceListTypeId)
        {
            return Content(HttpStatusCode.OK, pplm.DeletePriceListType(priceListTypeId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("PriceListTypes/PriceUpdate")]
        public IHttpActionResult PerformPriceUpdate(PriceListUpdateModel model)
        {
            return Content(HttpStatusCode.OK, pplm.UpdatePrices(base.ActorCompanyId, model.PriceListTypeIds, model.PriceUpdate, model.UpdateExisting, model.DateFrom, model.DateTo, model.ProductNrFrom, model.ProductNrTo, model.MaterialCodeId, model.VatType, model.ProductGroupId, model.PriceComparisonDate, model.QuantityFrom, model.QuantityTo));
        }

        #endregion

        #region PriceList

        [HttpGet]
        [Route("PriceListTypes/{priceListTypeId:int}/PriceLists")]
        public IHttpActionResult GetPriceLists(int priceListTypeId)
        {
            return Content(HttpStatusCode.OK, pplm.GetPriceListPrices(base.ActorCompanyId, priceListTypeId, false));
        }

        [HttpGet]
        [Route("Dict")]
        public IHttpActionResult GetPriceListsDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, pplm.GetPriceListTypesDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("ProductPriceList/{comparisonPriceListTypeId:int}/{priceListTypeId:int}/{loadAll:bool}/{priceDate}")]
        public IHttpActionResult GetProductPriceLists(int comparisonPriceListTypeId, int priceListTypeId, bool loadAll, string priceDate)
        {
            return Content(HttpStatusCode.OK, prom.GetProductComparisonDTOs(ActorCompanyId, comparisonPriceListTypeId, priceListTypeId, loadAll, BuildDateTimeFromString(priceDate, true)));
        }

        #endregion
    }
}