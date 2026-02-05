using System.Net;
using System.Web.Http;
using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;


namespace Soe.WebApi.V2.Manage
{
    [RoutePrefix("V2/Manage/Calendar/SchoolHoliday")]
    public class SchoolHolidayController : SoeApiController
    {
        #region Variables

        private readonly CalendarManager cm;

        #endregion

        #region Constructor

        public SchoolHolidayController(CalendarManager cm)
        {
            this.cm = cm;
        }

        #endregion

        #region SchoolHoliday

        [HttpGet]
        [Route("Grid/{schoolHolidayId:int?}")]
        public IHttpActionResult GetSchoolHolidaysGrid(int? schoolHolidayId = null)
        {
            return Content(HttpStatusCode.OK, cm.GetSchoolHolidays(base.ActorCompanyId, schoolHolidayId).ToGridDTOs());
        }

        [HttpGet]
        [Route("{schoolHolidayId:int}")]
        public IHttpActionResult GetSchoolHoliday(int schoolHolidayId)
        {
            return Content(HttpStatusCode.OK, cm.GetSchoolHoliday(schoolHolidayId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveSchoolHoliday(SchoolHolidayDTO schoolHolidayDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, cm.SaveSchoolHoliday(schoolHolidayDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("{schoolHolidayId:int}")]
        public IHttpActionResult DeleteSchoolHoliday(int schoolHolidayId)
        {
            return Content(HttpStatusCode.OK, cm.DeleteSchoolHoliday(schoolHolidayId, base.ActorCompanyId));
        }

        #endregion
    }
}