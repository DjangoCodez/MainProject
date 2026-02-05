using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Stock")]
    public class StockV2Controller : SoeApiController
    {
        #region Variables

        private readonly StockManager sm;

        #endregion

        #region Constructor

        public StockV2Controller(StockManager sm)
        {
            this.sm = sm;
        }

        #endregion

        #region Stock

        [HttpGet]
        [Route("StockGrid/{stockId:int?}")]
        public IHttpActionResult GetGridStocks(int? stockId = null)
        {
            return Content(HttpStatusCode.OK, sm.GetStocks(base.ActorCompanyId, stockId).ToGridDTOs());
        }

        [HttpGet]
        [Route("Stock/{addEmptyRow:bool}")]
        public IHttpActionResult GetStocks(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, sm.GetStocks(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("StockSmall/{addEmptyRow:bool}")]
        public IHttpActionResult GetSmallGenericStocks(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, sm.GetStocks(base.ActorCompanyId).ToSmallGenericDTOs());
        }

        [HttpGet]
        [Route("Stock/{stockId:int}/{addStockShelfs:bool}")]
        public IHttpActionResult GetStock(int stockId, bool addStockShelfs = false)
        {
            return Content(HttpStatusCode.OK, sm.GetStockDTO(base.ActorCompanyId, stockId, addStockShelfs, true));
        }

        [HttpGet]
        [Route("Stock/Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetStocksDict(bool addEmptyRow, bool? sort = true)
        {
            return Content(HttpStatusCode.OK, sm.GetStocksDict(base.ActorCompanyId, addEmptyRow, sort).ToSmallGenericTypes());
        }

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
            return Content(HttpStatusCode.OK, sm.MoveSaldoFromStock(base.ActorCompanyId, invoiceProductId, fromStockId, toStockId, quantity));
        }

        [HttpPost]
        [Route("ImportStockBalances")]
        public IHttpActionResult ImportStockBalances(ImportStockBalances model)
        {
            ModelState.Clear();

            byte[] bytes = model.FileString == null ? Array.Empty<byte>() : Convert.FromBase64String(model.FileString);
            model.FileData = new List<byte[]> { bytes };

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

        [HttpGet]
        [Route("StockPlace/Validate/{stockShelfId:int}")]
        public IHttpActionResult ValidateStockPlace(int stockShelfId)
        {
            return Content(HttpStatusCode.OK, sm.ValidateStockShelfForDelete(stockShelfId));
        }

        #endregion

    }
}