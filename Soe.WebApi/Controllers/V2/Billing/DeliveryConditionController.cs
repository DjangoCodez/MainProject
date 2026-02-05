using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using Soe.WebApi.Controllers;
using System.Net.Http;
using Soe.WebApi.Extensions;

namespace Soe.WebApi.V2.Billing
{
    [RoutePrefix("V2/Billing/DeliveryCondition")]
    public class DeliveryConditionController : SoeApiController
    {
        #region Variables

        private readonly InvoiceManager im;

        #endregion

        #region Constructor

        public DeliveryConditionController(InvoiceManager im)
        {
            this.im = im;
        }

        #endregion

        #region DeliveryCondition

        [HttpGet]
        [Route("Grid/{deliveryConditionId:int?}")]
        public IHttpActionResult GetDeliveryConditionsGrid(int? deliveryConditionId = null)
        {
            return Content(HttpStatusCode.OK, im.GetDeliveryConditions(base.ActorCompanyId, deliveryConditionId).ToGridDTOs());
        }

        [HttpGet]
        [Route("Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetDeliveryConditions(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, im.GetDeliveryConditionsDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{deliveryConditionId:int}")]
        public IHttpActionResult GetDeliveryCondition(int deliveryConditionId)
        {
            return Content(HttpStatusCode.OK, im.GetDeliveryCondition(deliveryConditionId).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveDeliveryCondition(DeliveryConditionDTO deliveryConditionDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveDeliveryCondition(deliveryConditionDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{deliveryConditionId:int}")]
        public IHttpActionResult DeleteDeliveryCondition(int deliveryConditionId)
        {
            return Content(HttpStatusCode.OK, im.DeleteDeliveryCondition(deliveryConditionId));
        }

        #endregion

    }
}