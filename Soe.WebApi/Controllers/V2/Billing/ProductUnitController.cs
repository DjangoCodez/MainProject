using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Common.Util;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Product/ProductUnit")]
    public class ProductUnitController : SoeApiController
    {
        #region Variables

        private readonly ProductManager pm;

        #endregion

        #region Constructor

        public ProductUnitController(ProductManager pm)
        {
            this.pm = pm;
        }

        #endregion

        #region ProductUnit

        [HttpGet]
        [Route("Grid/{productUnitId:int?}")]
        public IHttpActionResult GetProductUnits(int? productUnitId = null)
        {
            return Content(HttpStatusCode.OK, pm.GetProductUnits(base.ActorCompanyId, productUnitId).ToSmallDTOs());
        }

        [HttpGet]
        [Route("Dict")]
        public IHttpActionResult GetProductUnitsDict()
        {
            return Content(HttpStatusCode.OK, pm.GetProductUnitsDict(base.ActorCompanyId).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{productUnitId:int}")]
        public IHttpActionResult GetProductUnit(int productUnitId)
        {
            return Content(HttpStatusCode.OK, pm.GetProductUnit(productUnitId).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveProductUnit(ProductUnitModel model)
        {
            return Content(HttpStatusCode.OK, pm.SaveProductUnit(model.ProductUnit, model.Translations));
        }

        [HttpDelete]
        [Route("{productUnitId:int}")]
        public IHttpActionResult DeleteProductUnit(int productUnitId)
        {
            return Content(HttpStatusCode.OK, pm.DeleteProductUnit(new ProductUnit() { ProductUnitId = productUnitId }));
        }

        #endregion
    }
}