using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Economy
{
    [RoutePrefix("Economy/Inventory")]
    public class InventoryController : SoeApiController
    {
        #region Variables

        private readonly InventoryManager im;
        private readonly SettingManager sm;
        private readonly AccountManager am;
        private readonly CategoryManager cm;

		#endregion

		#region Constructor

		public InventoryController(InventoryManager im, SettingManager sm, AccountManager am, CategoryManager cm)
        {
            this.im = im;
            this.sm = sm;
            this.am = am;
            this.cm = cm;
        }

        #endregion

        #region Inventory

        [HttpGet]
        [Route("Inventories/{statuses}")]
        public IHttpActionResult GetInventories(string statuses)
        {
            return Content(HttpStatusCode.OK, im.GetInventories(base.ActorCompanyId, true, 0, statuses, true).ToGridDTOs());
        }

        [HttpGet]
        [Route("Categories")]
        public IHttpActionResult GetInventoryCategories()
        {
            return Content(HttpStatusCode.OK, cm.GetCategoryKeyValues(SoeCategoryType.Inventory, base.ActorCompanyId));
		}


		[HttpGet]
        [Route("InventoryAccounts")]
        public IHttpActionResult GetInventoryAccounts()
        {
            return Content(HttpStatusCode.OK, im.GetInventoryAccounts(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("SettingAccounts")]
        public IHttpActionResult GetSettingAccounts()
        {
            return Content(HttpStatusCode.OK, am.GetAccountStds(ActorCompanyId, sm.GetStringSetting(SettingMainType.Company, (int)CompanySettingType.InventoryEditTriggerAccounts, base.UserId, base.ActorCompanyId, 0)).ToDTOs());
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
        [Route("{inventory}")]
        public IHttpActionResult SaveInventory(SaveInventoryModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveInventory(model.inventory, model.categoryRecords, model.accountSettings, model.debtAccountId, base.ActorCompanyId));
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
        #endregion

        #region InventoryWriteOffMethod

        [HttpGet]
        [Route("InventoryWriteOffMethod")]
        public IHttpActionResult GetInventoryWriteOffMethods()
        {
            return Content(HttpStatusCode.OK, im.GetInventoryWriteOffMethods(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("InventoryWriteOffMethod/Dict")]
        public IHttpActionResult GetInventoryWriteOffMethodsDict()
        {
            return Content(HttpStatusCode.OK, im.GetInventoryWriteOffMethodsDict(base.ActorCompanyId).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("InventoryWriteOffMethod/{inventoryWriteOffMethodId:int}")]
        public IHttpActionResult GetInventoryWriteOffMethod(int inventoryWriteOffMethodId)
        {
            return Content(HttpStatusCode.OK, im.GetInventoryWriteOffMethod(inventoryWriteOffMethodId, false, false));
        }

        [HttpPost]
        [Route("InventoryWriteOffMethod")]
        public IHttpActionResult SaveInventoryWriteOffMethod(InventoryWriteOffMethodDTO inventoryWriteOffMethodDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveInventoryWriteOffMethod(inventoryWriteOffMethodDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("InventoryWriteOffMethod/{inventoryWriteOffMethodId:int}")]
        public IHttpActionResult DeleteInventoryWriteOffMethod(int inventoryWriteOffMethodId)
        {
            return Content(HttpStatusCode.OK, im.DeleteInventoryWriteOffMethod(inventoryWriteOffMethodId));
        }

        #endregion

        #region InventoryWriteOffTemplate

        [HttpGet]
        [Route("InventoryWriteOffTemplate/Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetInventoryWriteOffTemplatesDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, im.GetInventoryWriteOffTemplatesDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("InventoryWriteOffTemplate/{addEmptyRow:bool}")]
        public IHttpActionResult GetInventoryWriteOffTemplates(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, im.GetInventoryWriteOffTemplates(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("InventoryWriteOffTemplate/{inventoryWriteOffTemplateId:int}")]
        public IHttpActionResult GetInventoryWriteOffTemplate(int inventoryWriteOffTemplateId)
        {
            return Content(HttpStatusCode.OK, im.GetInventoryWriteOffTemplate(inventoryWriteOffTemplateId, true).ToDTO(true));
        }

        [HttpPost]
        [Route("InventoryWriteOffTemplate")]
        public IHttpActionResult SaveInventoryWriteOffTemplate(SaveInventoryWriteOffTemplateModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveInventoryWriteOffTemplate(model.inventoryWriteOffTemplate, null, model.accountSettings, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("InventoryWriteOffTemplate/{inventoryWriteOffTemplateId:int}")]
        public IHttpActionResult DeleteInventoryWriteOffTemplate(int inventoryWriteOffTemplateId)
        {
            return Content(HttpStatusCode.OK, im.DeleteInventoryWriteOffTemplate(inventoryWriteOffTemplateId));
        }

        #endregion
    }
}