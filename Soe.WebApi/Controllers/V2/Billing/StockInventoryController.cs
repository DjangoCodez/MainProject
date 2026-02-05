using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/Stock")]
    public class StockInventoryController: SoeApiController
    {
        #region Variables

        private readonly StockManager sm;

        #endregion

        #region Constructor

        public StockInventoryController(StockManager sm)
        {
            this.sm = sm;
        }

        #endregion

        #region StockInventory Endpoints

        [HttpGet]
        [Route("StockInventories/{includeCompleted:bool}/{stockInventoryId:int?}")]
        public IHttpActionResult GetStockInventories(bool includeCompleted, int? stockInventoryId = null)
        {
            return Content(HttpStatusCode.OK, sm.GetStockInventories(base.ActorCompanyId, includeCompleted, stockInventoryId).ToGridDTOs());
        }

        [HttpGet]
        [Route("StockInventory/{stockInventoryHeadId:int}")]
        public IHttpActionResult GetStockInventory(int stockInventoryHeadId)
        {
            return Content(HttpStatusCode.OK, sm.GetStockInventoryHeadDTO(stockInventoryHeadId, loadRows: true));
        }

        [HttpGet]
        [Route("StockInventoryRows/{stockInventoryHeadId:int}")]
        public IHttpActionResult GetStockInventoryRows(int stockInventoryHeadId)
        {
            return Content(HttpStatusCode.OK, sm.GetStockInventoryRowDTOs(stockInventoryHeadId));
        }

        [HttpPost]
        [Route("GenerateRows")]
        public IHttpActionResult GenerateStockInventoryRows(StockInventoryFilterDTO filter)
        {
            return Content(HttpStatusCode.OK, sm.GenerateStockInventoryRows(filter));
        }

        [HttpPost]
        [Route("SaveInventory")]
        public IHttpActionResult SaveStockInventoryRows(StockInventoryHeadDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sm.SaveStockInventory(base.ActorCompanyId, model, model.StockInventoryRows));
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
            ModelState.Clear();
            byte[] bytes = model.FileString == null ? Array.Empty<byte>() : Convert.FromBase64String(model.FileString);
            model.FileData = new List<byte[]> { bytes };

            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sm.ImportStockInventory(base.ActorCompanyId, model.StockInventoryHeadId, model.FileName, model.FileData));
        }

        #endregion StockInventory
    }
}