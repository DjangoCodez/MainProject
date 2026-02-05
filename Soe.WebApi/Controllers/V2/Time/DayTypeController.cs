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
    [RoutePrefix("V2/Time/DayType")]
    public class DayTypeController : SoeApiController
    {
        #region Variables

        private readonly CalendarManager cm;

        #endregion

        #region Constructor

        public DayTypeController(CalendarManager cm)
        {
            this.cm = cm;
        }

        #endregion

        #region DayType

        [HttpGet]
        [Route("Grid/{dayTypeId:int?}")]
        public IHttpActionResult GetDayTypesGrid(int? dayTypeId = null)
        {
            return Content(HttpStatusCode.OK, cm.GetDayTypesByCompany(base.ActorCompanyId, false, dayTypeId).ToGridDTOs());
        }

        [HttpGet]
        [Route("Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetDayTypesDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, cm.GetDayTypesByCompanyDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("{dayTypeId:int}")]
        public IHttpActionResult GetDayType(int dayTypeId)
        {
            return Content(HttpStatusCode.OK, cm.GetDayType(dayTypeId, base.ActorCompanyId).ToDTO());
        }

        [HttpGet]
        [Route("DayTypeAndWeekday")]
        public IHttpActionResult GetDayTypesAndWeekdays()
        {
            return Content(HttpStatusCode.OK, cm.GetDayTypesAndWeekdays(base.ActorCompanyId));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveDayType(DayTypeDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, cm.SaveDayType(model, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{dayTypeId:int}")]
        public IHttpActionResult DeleteDayType(int dayTypeId)
        {
            return Content(HttpStatusCode.OK, cm.DeleteDayType(dayTypeId));
        }

        [HttpGet]
        [Route("GetDayTypesByCompanyDict/{addEmptyRow:bool}/{onlyHolidaySalary:bool}")]
        public IHttpActionResult GetDayTypesByCompanyDict(bool addEmptyRow, bool onlyHolidaySalary)
        {
            return Content(HttpStatusCode.OK, cm.GetDayTypesByCompanyDict(ParameterObject.ActorCompanyId, addEmptyRow, onlyHolidaySalary: onlyHolidaySalary).ToSmallGenericTypes());
        }

        #endregion

        #region DayOfWeek

        [HttpGet]
        [Route("DayOfWeek/Dict/{addEmptyRow:bool}")]
        public IHttpActionResult GetDaysOfWeekDict(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, cm.GetDaysOfWeekDict(addEmptyRow).ToSmallGenericTypes());
        }

        #endregion
    }
}