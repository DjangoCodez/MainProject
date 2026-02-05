using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using System.Collections.Generic;


namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/ProductUnitConvert")]
    public class ProductUnitConvertController : SoeApiController
    {
        #region Variables

        private readonly ProductManager pm;

        #endregion

        #region Constructor

        public ProductUnitConvertController(ProductManager pm)
        {
            this.pm = pm;
        }

        #endregion

        #region ProductUnitConverts

        [HttpGet]
        [Route("{productId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetProductUnitConverts(int productId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, pm.GetProductUnitConverts(productId, addEmptyRow).ToDTOs());
        }

        [HttpPost]
        [Route("SaveProductUnitConvert")]
        public IHttpActionResult SaveProductUnitConvert(List<ProductUnitConvertDTO> unitConvertDTOs)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.AddUpdateProductUnitConverts(unitConvertDTOs));
        }

        [HttpPost]
        [Route("Parse")]
        public IHttpActionResult ParseProductUnitConversionFile(ProductUnitFileModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.ParseProductUnitConversionFile(model.ProductIds, model.FileData));
        }

        #endregion
    }
}