using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Stock")]
    public class StockProductController : SoeApiController
    {
        #region Variables

        private readonly StockManager sm;

        #endregion

        #region Constructor

        public StockProductController(StockManager sm)
        {
            this.sm = sm;
        }

        #endregion

        #region StockProduct
        
        [HttpGet]
        [Route("StockProducts")]
        public IHttpActionResult GetStockProducts([FromUri]bool includeInactive, [FromUri]int? stockProductId = null)
        {
            return Content(HttpStatusCode.OK, sm.GetStockProductDTOsWithSaldo(base.ActorCompanyId, includeInactive, stockProductId));
        }

        [HttpGet]
        [Route("GetStockProductsByStockId/{stockId:int}")]
        public IHttpActionResult GetStockProductsByStockId(int stockId)
        {
            return Content(HttpStatusCode.OK, sm.GetStockProductsByStockId(base.ActorCompanyId, stockId));
        }

        [HttpGet]
        [Route("StockProducts/{productId:int}")]
        public IHttpActionResult GetStockProductsByProductId(int productId)
        {
            return Content(HttpStatusCode.OK, sm.GetStockProductDTOs(base.ActorCompanyId, productId));
        }

        [HttpGet]
        [Route("StockProduct/{stockProductId:int}")]
        public IHttpActionResult GetStockProduct(int stockProductId)
        {
            return Content(HttpStatusCode.OK, sm.GetStockProductDTO(stockProductId));
        }


        [HttpGet]
        [Route("StockProduct/Transactions/{stockProductId:int}")]
        public IHttpActionResult GetStockProductTransactions(int stockProductId)
        {
            return Content(HttpStatusCode.OK, sm.GetStockTransactionDTOs(stockProductId,null,null, true));
        }

        [HttpPost]
        [Route("StockProduct/Transactions")]
        public IHttpActionResult SaveStockTransaction(List<StockTransactionDTO> stockTransactionDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sm.SaveStockTransactions(stockTransactionDTO, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("StockProducts/Products/{stockId:int?}/{onlyActive:bool?}")]
        public IHttpActionResult GetStockProductProducts(int? stockId = null, bool? onlyActive = false)
        {
            return Content(HttpStatusCode.OK, sm.GetStockProductProductSmallDTOs(base.ActorCompanyId, stockId, onlyActive));
        }

        [HttpPost]
        [Route("StockProducts/ValidateProductsInStock")]
        public IHttpActionResult ValidateProductsInStock(ProductsInStockModel model)
        {
            return Content(HttpStatusCode.OK, sm.GetProductNotInStocktProductSmallDTOs(base.ActorCompanyId, model.StockId,model.ProductIds));
        }
        #endregion
    }
}