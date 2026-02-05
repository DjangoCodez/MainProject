using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
using System.Net;
using System.Web.Http;
using Soe.WebApi.Controllers;
using Soe.WebApi.Models;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/Inventory")]
    public class InventoryV2Controller : SoeApiController
    {
        #region Variables

        private readonly InventoryManager im;

        #endregion

        #region Constructor
        public InventoryV2Controller(InventoryManager im)
        {
            this.im = im;
        }
        #endregion

        #region Inventory

        [HttpGet]
        [Route("Inventories")]
        public IHttpActionResult GetInventories(string statuses, int? inventoryId = null)
        {
            return Content(
                HttpStatusCode.OK, 
                im.GetInventories(base.ActorCompanyId, 
                    loadInventoryAccount: true, 
                    userId: 0, 
                    statuses, 
                    loadCategories: true, 
                    loadInventoryWriteOffMethod: true, 
                    loadOnlyActive: true, 
                    inventoryId)
                .ToGridDTOs()
            );
        }

        [HttpGet]
        [Route("Inventories/Dict")]
        public IHttpActionResult GetInventoriesDict()
        {
            return Content(HttpStatusCode.OK, im.GetInventoriesDict(base.ActorCompanyId, true).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{inventoryId:int}")]
        public IHttpActionResult GetInventory(int inventoryId)
        {
            return Content(HttpStatusCode.OK, im.GetInventory(inventoryId, base.ActorCompanyId, true, true, true, true, true).ToDTO(true, true));
        }

        [HttpGet]
        [Route("NextInventoryNr")]
        public IHttpActionResult GetNextInventoryNr()
        {
            return Content(HttpStatusCode.OK, im.GetNextInventoryNr(base.ActorCompanyId));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveInventory(SaveInventoryModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveInventory(model.inventory, model.categoryRecords, null, model.debtAccountId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Adjustment")]
        public IHttpActionResult SaveAdjustment(SaveAdjustmentModel model)
        {
            if (model.type == TermGroup_InventoryLogType.Sold || model.type == TermGroup_InventoryLogType.Discarded)
                return Content(HttpStatusCode.OK, im.SaveDispose(model.inventoryId, model.type, model.voucherSeriesTypeId, model.amount, model.date, model.note, model.accountRowItems, base.ActorCompanyId));
            else
                return Content(HttpStatusCode.OK, im.SaveAdjustment(model.inventoryId, model.type, model.amount, model.date, model.accountRowItems, null, null, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("NotesAndDescription")]
        public IHttpActionResult SaveNotesAndDescription(SaveInventoryNotesModel model)
        {
            return Content(HttpStatusCode.OK, im.SaveNoteAndDescripiton(model.InventoryId, model.Description, model.Notes, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("InventoryTraceViews/{inventoryId:int}")]
        public IHttpActionResult GetInventoryTraceViews(int inventoryId)
        {
            return Content(HttpStatusCode.OK, im.GetInventoryTraceViews(inventoryId));
        }

        [HttpDelete]
        [Route("Inventory/{inventoryId:int}")]
        public IHttpActionResult DeleteInventory(int inventoryId)
        {
            return Content(HttpStatusCode.OK, im.DeleteInventory(inventoryId, base.ActorCompanyId));
        }

        #endregion#endregion
    }
}