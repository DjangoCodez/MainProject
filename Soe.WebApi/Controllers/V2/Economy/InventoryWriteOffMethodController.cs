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

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/Inventory/InventoryWriteOffMethod")]
    public class InventoryWriteOffMethodController : SoeApiController
    {
        #region Variables

        private readonly InventoryManager im;

        #endregion

        #region Constructor
        public InventoryWriteOffMethodController(InventoryManager im)
        {
            this.im = im;
        }
        #endregion

        #region InventoryWriteOffMethod

        [HttpGet]
        [Route("Grid/{writeOffMethodId:int?}")]
        public IHttpActionResult GetInventoryWriteOffMethodsGrid(int? writeOffMethodId=null)
        {
            return Content(HttpStatusCode.OK, im.GetInventoryWriteOffMethods(base.ActorCompanyId, writeOffMethodId).ToGridDTOs());
        }

        [HttpGet]
        [Route("Dict")]
        public IHttpActionResult GetInventoryWriteOffMethodsDict(bool addEmptyValue)
        {
            return Content(HttpStatusCode.OK, im.GetInventoryWriteOffMethodsDict(base.ActorCompanyId, addEmptyValue).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{inventoryWriteOffMethodId:int}")]
        public IHttpActionResult GetInventoryWriteOffMethod(int inventoryWriteOffMethodId)
        {
            return Content(HttpStatusCode.OK, im.GetInventoryWriteOffMethod(inventoryWriteOffMethodId, false, false, true).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveInventoryWriteOffMethod(InventoryWriteOffMethodDTO inventoryWriteOffMethodDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveInventoryWriteOffMethod(inventoryWriteOffMethodDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{inventoryWriteOffMethodId:int}")]
        public IHttpActionResult DeleteInventoryWriteOffMethod(int inventoryWriteOffMethodId)
        {
            return Content(HttpStatusCode.OK, im.DeleteInventoryWriteOffMethod(inventoryWriteOffMethodId));
        }

        #endregion
    }
}