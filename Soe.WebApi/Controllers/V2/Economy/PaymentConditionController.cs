using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;


namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/PaymentCondition")]
    public class PaymentConditionController : SoeApiController
    {
        #region Variables
        private readonly PaymentManager pm;
        #endregion

        #region Constructor
        public PaymentConditionController(PaymentManager _pm)
        {
            this.pm = _pm;
        }
        #endregion

        #region PaymentCondition

        [HttpGet]
        [Route("Grid/{paymentConditionId:int?}")]
        public IHttpActionResult GetPaymentConditionsGrid(int? paymentConditionId = null)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentConditions(base.ActorCompanyId, paymentConditionId).ToGridDTOs());
        }

        [HttpGet]
        [Route("SmallGenericType/")]
        public IHttpActionResult GetSmallGenericTypePaymentConditions(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentConditionsDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("DTOs/")]
        public IHttpActionResult GetPaymentConditions()
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentConditions(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("{paymentConditionId:int}")]
        public IHttpActionResult GetPaymentCondition(int paymentConditionId)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentCondition(paymentConditionId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("PaymentCondition")]
        public IHttpActionResult SavePaymentCondition(PaymentConditionDTO paymentConditionDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pm.SavePaymentCondition(paymentConditionDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{paymentConditionId:int}")]
        public IHttpActionResult DeletePaymentCondition(int paymentConditionId)
        {
            return Content(HttpStatusCode.OK, pm.DeletePaymentCondition(paymentConditionId, base.ActorCompanyId));
        }

        #endregion
    }
}