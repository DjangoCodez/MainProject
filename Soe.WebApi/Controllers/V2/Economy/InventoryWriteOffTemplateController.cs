using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using SoftOne.Soe.Common.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using Soe.WebApi.Controllers;
using Soe.WebApi.Models;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/Inventory/InventoryWriteOffTemplate")]
    public class InventoryWriteOffTemplateController : SoeApiController
    {
        #region Variables

        private readonly InventoryManager im;

        #endregion

        #region Constructor
        public InventoryWriteOffTemplateController(InventoryManager im)
        {
            this.im = im;
        }
        #endregion

        #region InventoryWriteOffTemplate

        [HttpGet]
        [Route("Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetInventoryWriteOffTemplatesDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, im.GetInventoryWriteOffTemplatesDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Grid/{inventoryWriteOffTemplateId:int?}")]
        public IHttpActionResult GetInventoryWriteOffTemplateGrid(int? inventoryWriteOffTemplateId = null)
        {
            return Content(HttpStatusCode.OK, im.GetInventoryWriteOffTemplates(base.ActorCompanyId, inventoryWriteOffTemplateId).ToGridDTOs());
        }

        [HttpGet]
        [Route("{inventoryWriteOffTemplateId:int}")]
        public IHttpActionResult GetInventoryWriteOffTemplate(int inventoryWriteOffTemplateId)
        {
            return Content(HttpStatusCode.OK, im.GetInventoryWriteOffTemplate(inventoryWriteOffTemplateId, true).ToDTO(true));
        }

		[HttpGet]
		[Route("")]
		public IHttpActionResult GetInventoryWriteOffTemplates()
		{
			return Content(HttpStatusCode.OK, im.GetInventoryWriteOffTemplates(base.ActorCompanyId, null, true).ToDTOs(true));
		}

		[HttpPost]
        [Route("")]
        public IHttpActionResult SaveInventoryWriteOffTemplate(SaveInventoryWriteOffTemplateModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveInventoryWriteOffTemplate(model.inventoryWriteOffTemplate, null, model.accountSettings, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{inventoryWriteOffTemplateId:int}")]
        public IHttpActionResult DeleteInventoryWriteOffTemplate(int inventoryWriteOffTemplateId)
        {
            return Content(HttpStatusCode.OK, im.DeleteInventoryWriteOffTemplate(inventoryWriteOffTemplateId));
        }

        #endregion
    }
}