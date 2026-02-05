using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/IncomingDelivery")]
    public class IncomingDeliveryController : SoeApiController
    {
        #region Variables

        private readonly TimeScheduleManager tsm;

        #endregion

        #region Constructor

        public IncomingDeliveryController(TimeScheduleManager tsm)
        {
            this.tsm = tsm;
        }

        #endregion

        #region IncomingDeliveryHead

        [HttpGet]
        [Route("Grid/{incomingDeliveryHeadId:int?}")]
        public IHttpActionResult GetIncomingDeliveriesGrid(int? incomingDeliveryHeadId = null)
        {
            return Content(HttpStatusCode.OK, tsm.GetIncomingDeliveries(base.ActorCompanyId, true, true, false, true, incomingDeliveryHeadId).ToGridDTOs());
        }

        //[HttpGet]
        //[Route("GetIncomingDeliveriesForInterval")]
        //public IHttpActionResult GetIncomingDeliveriesForInterval(HttpRequestMessage message)
        //{
        //    return Content(HttpStatusCode.OK, tsm.GetIncomingDeliveries(base.ActorCompanyId, message.GetDateValueFromQS("dateFrom").Value, message.GetDateValueFromQS("dateTo").Value, message.GetIntListValueFromQS("ids"), loadAccounting: true, loadDeliveryType: true).ToDTOs(true, true));
        //}

        [HttpGet]
        [Route("{incomingDeliveryHeadId:int}")]
        public IHttpActionResult GetIncomingDelivery(int incomingDeliveryHeadId)
        {
            return Content(HttpStatusCode.OK, tsm.GetIncomingDelivery(incomingDeliveryHeadId, base.ActorCompanyId, true, true, false, false, true, true).ToDTO(true, true));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveIncomingDelivery(IncomingDeliveryHeadDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveIncomingDelivery(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{incomingDeliveryHeadId:int}")]
        public IHttpActionResult DeleteIncomingDelivery(int incomingDeliveryHeadId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteIncomingDelivery(incomingDeliveryHeadId, base.ActorCompanyId));
        }

        #endregion

        #region IncomingDeliveryRow

        [HttpGet]
        [Route("IncomingDeliveryRow/{incomingDeliveryHeadId:int}")]
        public IHttpActionResult GetIncomingDeliveryRows(int incomingDeliveryHeadId)
        {
            return Content(HttpStatusCode.OK, tsm.GetIncomingDeliveryRows(incomingDeliveryHeadId, false).ToDTOs(false));
        }

        #endregion

        #region IncomingDeliveryType

        [HttpGet]
        [Route("IncomingDeliveryType/Grid")]
        public IHttpActionResult GetIncomingDeliveryTypesGrid(int? incomingDeliveryTypeId = null)
        {
            return Content(HttpStatusCode.OK, tsm.GetIncomingDeliveryTypes(base.ActorCompanyId, incomingDeliveryTypeId).ToGridDTOs());
        }

        [HttpGet]
        [Route("IncomingDeliveryType/Small")]
        public IHttpActionResult GetIncomingDeliveryTypesSmall()
        {
            return Content(HttpStatusCode.OK, tsm.GetIncomingDeliveryTypes(base.ActorCompanyId).ToSmallDTOs());
        }

        [HttpGet]
        [Route("IncomingDeliveryType/Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetIncomingDeliveryTypesDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, tsm.GetIncomingDeliveryTypesDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("IncomingDeliveryType/{incomingDeliveryTypeId:int}")]
        public IHttpActionResult GetIncomingDeliveryType(int incomingDeliveryTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.GetIncomingDeliveryType(incomingDeliveryTypeId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("IncomingDeliveryType")]
        public IHttpActionResult SaveIncomingDeliveryType(IncomingDeliveryTypeDTO incomingDeliveryTypeDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tsm.SaveIncomingDeliveryType(incomingDeliveryTypeDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("IncomingDeliveryType/{incomingDeliveryTypeId:int}")]
        public IHttpActionResult DeleteIncomingDeliveryType(int incomingDeliveryTypeId)
        {
            return Content(HttpStatusCode.OK, tsm.DeleteIncomingDeliveryType(incomingDeliveryTypeId, base.ActorCompanyId));
        }

        #endregion
    }
}