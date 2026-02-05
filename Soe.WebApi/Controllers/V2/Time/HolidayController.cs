using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Attributes;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/Holiday")]
    public class HolidayController : SoeApiController
    {
        #region Variables

        private readonly CalendarManager cm;
        private readonly TimeEngineManager tem;

        #endregion

        #region Constructor

        public HolidayController(CalendarManager cm, TimeEngineManager tem)
        {
            this.cm = cm;
            this.tem = tem;
        }

        #endregion

        #region Holiday

        [HttpGet]
        [Route("Grid")]
        public IHttpActionResult GetHolidaysGrid(int? holidayId = null)
        {
            return Content(HttpStatusCode.OK, cm.GetHolidaysByCompany(base.ActorCompanyId, loadDayType: true, holidayId: holidayId).ToGridDTOs());
        }

        [HttpGet]
        [Route("Small/{dateFromString}/{dateToString}")]
        public IHttpActionResult GetHolidaysSmall(string dateFromString, string dateToString)
        {
            return Content(HttpStatusCode.OK, cm.GetHolidaysByCompanySmall(base.ActorCompanyId, BuildDateTimeFromString(dateFromString, true).Value, BuildDateTimeFromString(dateToString, true).Value));
        }

        [HttpGet]
        [Route("{holidayId:int}")]
        public IHttpActionResult GetHoliday(int holidayId)
        {
            return Content(HttpStatusCode.OK, cm.GetHoliday(holidayId, base.ActorCompanyId).ToDTO(true));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveHoliday(HolidayDTO model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, cm.AddHoliday(model, base.ActorCompanyId, model.DayTypeId));
        }

        [HttpDelete]
        [Route("{holidayId:int}")]
        public IHttpActionResult DeleteHoliday(int holidayId)
        {
            return Content(HttpStatusCode.OK, cm.DeleteHoliday(holidayId, base.ActorCompanyId));
        }

        #endregion

        #region SysHolidayType

        [HttpGet]
        [Route("SysHolidayTypes/Dict")]
        public IHttpActionResult GetHolidayTypesDict()
        {
            return Content(HttpStatusCode.OK, cm.GetSysHolidayTypeDTOs().ToDictionary(x => x.SysHolidayTypeId, x => x.Name).ToSmallGenericTypes());
        }

        #endregion

        #region UpdateSchedule

        [HttpPost]
        [Route("OnAddHoliday")]
        public IHttpActionResult OnAddHoliday(IntIntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);

            return Content(HttpStatusCode.OK, tem.AddUniqueDayFromHoliday(model.Id1, model.Id2));
        }

        [HttpPost]
        [Route("OnUpdateHoliday")]
        public IHttpActionResult OnUpdateHoliday(IntIntStringModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            DateTime? oldDateToDelete = null;

            if (!string.IsNullOrEmpty(model.Id3))
            {
                if (DateTime.TryParseExact(
                        model.Id3,
                        "yyyyMMdd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var parsedDate)
                    && CalendarUtility.ToUrlFriendlyDateTime(parsedDate)
                          != CalendarUtility.URL_FRIENDLY_DATETIME_DEFAULT)
                {
                    oldDateToDelete = parsedDate;
                }
            }

            return Content(HttpStatusCode.OK, tem.UpdateUniqueDayFromHoliday(model.Id1, model.Id2, oldDateToDelete: oldDateToDelete));
        }

        [HttpPost]
        [Route("OnDeleteHoliday")]
        public IHttpActionResult onDeleteHoliday(IntIntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);

            return Content(HttpStatusCode.OK, tem.DeleteUniqueDayFromHoliday(model.Id1, model.Id2, null));
        }

        #endregion
    }
}