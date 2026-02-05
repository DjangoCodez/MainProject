using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/Halfday")]
    public class HalfdayController : SoeApiController
    {
        #region Variables

        private readonly CalendarManager cm;
        private readonly TimeCodeManager tcm;
        private readonly TimeEngineManager tem;

        #endregion

        #region Constructor

        public HalfdayController(CalendarManager cm, TimeCodeManager tcm, TimeEngineManager tem)
        {
            this.cm = cm;
            this.tcm = tcm;
            this.tem = tem;
        }

        #endregion

        #region Halfday

        [HttpGet]
        [Route("Grid")]
        public IHttpActionResult GetHalfdaysGrid(int? halfDayId = null)
        {
            return Content(HttpStatusCode.OK, cm.GetTimeHalfdays(base.ActorCompanyId, true, halfDayId).ToGridDTOs());
        }

        [HttpGet]
        [Route("{halfdayId:int}")]
        public IHttpActionResult GetHalfday(int halfdayId)
        {
            return Content(HttpStatusCode.OK, cm.GetTimeHalfday(halfdayId, base.ActorCompanyId).ToEditDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveHalfday(TimeHalfdayEditDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);

            return Content(HttpStatusCode.OK, cm.SaveTimeHalfday(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{halfdayId:int}")]
        public IHttpActionResult DeleteHalfday(int halfdayId)
        {
            return Content(HttpStatusCode.OK, cm.DeleteTimeHalfday(halfdayId, base.ActorCompanyId));
        }

        #endregion

        #region HalfdayType

        [HttpGet]
        [Route("HalfDayTypesDict/{addEmptyRow:bool}")]
        public IHttpActionResult GetHalfDayTypesDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, cm.GetTimeHalfdayTypesDict(addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("GetDayTypesByCompanyDict/{addEmptyRow:bool}")]
        public IHttpActionResult GetDayTypesByCompanyDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, cm.GetDayTypesByCompanyDict(ParameterObject.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        #endregion

        #region TimeCodeBreaks

        [HttpGet]
        [Route("TimeCodeBrakeDict/{addEmptyRow:bool}")]
        public IHttpActionResult GetTimeCodeBrakeDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, tcm.GetTimeCodesDict(base.ActorCompanyId, addEmptyRow, false, false, (int)SoeTimeCodeType.Break).ToSmallGenericTypes());
        }

        #endregion

        #region UpdateSchedule

        [HttpPost]
        [Route("OnAddHalfDay")]
        public IHttpActionResult OnAddHalfDay(IntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);

            return Content(HttpStatusCode.OK, tem.SaveUniqueDayFromHalfDay(model.Id, false));
        }

        [HttpPost]
        [Route("OnUpdateHalfDay")]
        public IHttpActionResult OnUpdateHalfDay(IntIntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);

            return Content(HttpStatusCode.OK, tem.UpdateUniqueDayFromHalfDay(model.Id1, model.Id2));
        }

        [HttpPost]
        [Route("OnDeleteHalfDay")]
        public IHttpActionResult OnDeleteHalfDay(IntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);

            return Content(HttpStatusCode.OK, tem.SaveUniqueDayFromHalfDay(model.Id, false));
        }

        #endregion
    }
}