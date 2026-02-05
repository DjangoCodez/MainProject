using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using Soe.WebApi.Models;
using System.Net;
using System.Web.Http;
using System.Collections.Generic;
using Soe.WebApi.Extensions;
using System.Net.Http;

namespace Soe.WebApi.Controllers.Billing
{
    [RoutePrefix("Billing/Stock")]
    public class StockController : SoeApiController
    {
        #region Variables

        private readonly StockManager sm;

        #endregion

        #region Constructor

        public StockController(StockManager sm)
        {
            this.sm = sm;
        }

        #endregion

        #region Stock

        [HttpGet]
        [Route("Stock/{addEmptyRow:bool}")]
        public IHttpActionResult GetStocks(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, sm.GetStocks(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("Stock/{stockId:int}")]
        public IHttpActionResult GetStock(int stockId)
        {
            return Content(HttpStatusCode.OK, sm.GetStockDTO(base.ActorCompanyId, stockId, true, true));
        }

        [HttpGet]
        [Route("Stock/Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetStocksDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, sm.GetStocksDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());     }

        [HttpGet]
        [Route("Stock/ByProduct/{productId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetStocksDictForInvoiceProduct(int productId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, sm.GetStocksDictForInvoiceProduct(base.ActorCompanyId, productId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Stock/ByProduct/{productId:int}")]
        public IHttpActionResult GetStocksForInvoiceProduct(int productId)
        {
            return Content(HttpStatusCode.OK, sm.GetStocksForInvoiceProduct(base.ActorCompanyId, productId));
        }

        [HttpPost]
        [Route("Stock")]
        public IHttpActionResult SaveStock(StockDTO stockDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sm.SaveStock(stockDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("Stock/{stockId:int}")]
        public IHttpActionResult DeleteStock(int stockId)
        {
            return Content(HttpStatusCode.OK, sm.DeleteStock(stockId));
        }

        [HttpPost]
        [Route("StockTransfer/{invoiceProductId:int}/{fromStockId:int}/{toStockId:int}/{quantity}")]
        public IHttpActionResult StockTransfer(int invoiceProductId, int fromStockId, int toStockId, int quantity)
        {            
            return Content(HttpStatusCode.OK, sm.MoveSaldoFromStock(base.ActorCompanyId,invoiceProductId, fromStockId, toStockId, quantity));
        }

        [HttpPost]
        [Route("ImportStockBalances")]
        public IHttpActionResult ImportStockBalances(ImportStockBalances model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sm.ImportStockBalances(base.ActorCompanyId, model.WholesellerId, model.StockId, model.FileName, model.FileData));
        }

        [HttpPost]
        [Route("RecalculateStockBalance/{stockId:int}")]
        public IHttpActionResult RecalculateStockBalance(int stockId)
        {
            return Content(HttpStatusCode.OK, sm.RecalculateStockBalance(base.ActorCompanyId, stockId));
        }

        #endregion

        #region StockPlace

        [HttpGet]
        [Route("StockPlace/{addEmptyRow:bool}/{stockId:int}")]
        public IHttpActionResult GetStockPlaces(bool addEmptyRow, int stockId)
        {
            return Content(HttpStatusCode.OK, sm.GetStockShelfDTOs(base.ActorCompanyId, stockId, addEmptyRow));
        }

        [HttpGet]
        [Route("StockPlace/{stockShelfId:int}")]
        public IHttpActionResult GetStockPlace(int stockShelfId)
        {
            return Content(HttpStatusCode.OK, sm.GetStockShelf(stockShelfId).ToDTO());
        }

        [HttpPost]
        [Route("StockPlace")]
        public IHttpActionResult SaveStockPlace(StockShelfDTO stockPlaceDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sm.SaveStockShelf(stockPlaceDTO));
        }

        [HttpDelete]
        [Route("StockPlace/{stockShelfId:int}")]
        public IHttpActionResult DeleteStockPlace(int stockShelfId)
        {
            return Content(HttpStatusCode.OK, sm.DeleteStockShelf(stockShelfId, base.ActorCompanyId));
        }

        #endregion

        #region StockProduct

        [HttpGet]
        [Route("StockProducts/{includeInactive:bool}")]
        public IHttpActionResult GetStockProducts(HttpRequestMessage message, bool includeInactive)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_SMALL_DTO))
                return Content(HttpStatusCode.OK, sm.GetStockProductSmallDTOs(base.ActorCompanyId, includeInactive));
            else
                return Content(HttpStatusCode.OK, sm.GetStockProductDTOsWithSaldo(base.ActorCompanyId, includeInactive));
        }

        [HttpGet]
        [Route("StockProducts/{productId:int}")]
        public IHttpActionResult GetStockProducts(int productId)
        {
            return Content(HttpStatusCode.OK, sm.GetStockProductDTOs(base.ActorCompanyId, productId));
        }

        [HttpPost]
        [Route("StockProducts/")]
        public IHttpActionResult GetStockProducts(ProductsSimpleModel model)
        {
            return Content(HttpStatusCode.OK, sm.GetStockProductDTOs(base.ActorCompanyId, model.ProductIds));
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
            return Content(HttpStatusCode.OK, sm.GetStockTransactionDTOs(stockProductId, null, null, true));
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
        [Route("StockProducts/Products")]
        public IHttpActionResult GetStockProductProducts()
        {
            return Content(HttpStatusCode.OK, sm.GetStockProductProductSmallDTOs(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("StockProduct/AvgPrice/{stockId:int}/{productId:int}")]
        public IHttpActionResult GetStockProductAvgPrice(int stockId, int productId)
        {
            return Content(HttpStatusCode.OK, sm.GetStockProductAvgPriceDTO(stockId, productId, base.ActorCompanyId));
        }

        #endregion

        #region StockInventory

        [HttpGet]
        [Route("StockInventories")]
        public IHttpActionResult GetStockInventories()
        {
            return Content(HttpStatusCode.OK, sm.GetStockInventoryHeadDTOs(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("StockInventory/{stockInventoryHeadId:int}")]
        public IHttpActionResult GetStockInventory(int stockInventoryHeadId)
        {
            return Content(HttpStatusCode.OK, sm.GetStockInventoryHeadDTO(stockInventoryHeadId));
        }

        [HttpGet]
        [Route("StockInventoryRows/{stockInventoryHeadId:int}")]
        public IHttpActionResult GetStockInventoryRows(int stockInventoryHeadId)
        {
            return Content(HttpStatusCode.OK, sm.GetStockInventoryRowDTOs(stockInventoryHeadId));
        }

        [HttpGet]
        [Route("GenerateRows/{stockId:int}/{productNrFrom?}/{productNrTo?}/{shelfIdFrom:int}/{shelfIdTo:int}")]
        //[Route("GenerateRows/{model:GetStockInventoryRowsModel}")]
        public IHttpActionResult GenerateStockInventoryRows(int stockId, string productNrFrom, string productNrTo, int shelfIdFrom, int shelfIdTo)
        //public IHttpActionResult GenerateStockInventoryRows(GetStockInventoryRowsModel model)
        {
            var filter = new StockInventoryFilterDTO
            {
                StockId = stockId,
                ProductNrFrom = productNrFrom,
                ProductNrTo = productNrTo,
            };
            IList<int> shelfIds = new List<int>();
            if (shelfIdFrom != 0) shelfIds.Add(shelfIdFrom);
            if (shelfIdTo != 0) shelfIds.Add(shelfIdTo);
            
            filter.ShelfIds.AddRange(shelfIds);
            return Content(HttpStatusCode.OK, sm.GenerateStockInventoryRows(filter));
        }

        [HttpPost]
        [Route("SaveInventory")]
        public IHttpActionResult SaveStockInventoryRows(SaveNewStockInventory model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sm.SaveStockInventory(base.ActorCompanyId, model.InventoryHead, model.InventoryRows));
        }

        [HttpGet]
        [Route("CloseInventory/{stockInventoryHeadId:int}")]
        public IHttpActionResult CloseStockInventory(int stockInventoryHeadId)
        {
            return Content(HttpStatusCode.OK, sm.CloseInventory(stockInventoryHeadId));
        }
        [HttpDelete]
        [Route("StockInventory/{stockInventoryHeadId:int}")]
        public IHttpActionResult DeleteStockInventory(int stockInventoryHeadId)
        {
            return Content(HttpStatusCode.OK, sm.DeleteStockInventory(stockInventoryHeadId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("ImportStockInventory")]
        public IHttpActionResult ImportStockInventory(ImportStockBalances model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sm.ImportStockInventory(base.ActorCompanyId, model.StockInventoryHeadId, model.FileName, model.FileData));
        }

        #endregion StockInventory

        #region Purchase
        [HttpPost]
        [Route("Purchase/GenerateSuggestion")]
        public IHttpActionResult GeneratePurchaseSuggestion(GenerateStockPurchaseSuggestionDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sm.GetStockPurchaseSugggestion(this.ActorCompanyId, model));
        }
        #endregion
    }
}