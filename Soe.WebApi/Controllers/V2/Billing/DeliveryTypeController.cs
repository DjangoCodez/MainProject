using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using Soe.WebApi.Controllers;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/DeliveryType")]
    public class DeliveryTypeController : SoeApiController
    {
        #region Variables

        private readonly InvoiceManager im;

        #endregion

        #region Constructor

        public DeliveryTypeController(InvoiceManager im)
        {
            this.im = im;
        }

        #endregion

        #region DeliveryType

        [HttpGet]
        [Route("Grid")]
        public IHttpActionResult GetDeliveryTypesGrid(int? deliveryTypeId = null)
        {
            return Content(HttpStatusCode.OK, im.GetDeliveryTypes(base.ActorCompanyId, deliveryTypeId).ToGridDTOs());
        }

        [HttpGet]
        [Route("Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetDeliveryTypesDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, im.GetDeliveryTypesDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{deliveryTypeId:int}")]
        public IHttpActionResult GetDeliveryType(int deliveryTypeId)
        {
            return Content(HttpStatusCode.OK, im.GetDeliveryType(deliveryTypeId));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveDeliveryType(DeliveryTypeDTO deliveryType)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveDeliveryType(deliveryType, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{deliveryTypeId:int}")]
        public IHttpActionResult DeleteDeliveryType(int deliveryTypeId)
        {
            return Content(HttpStatusCode.OK, im.DeleteDeliveryType(deliveryTypeId));
        }

        #endregion
    }
}