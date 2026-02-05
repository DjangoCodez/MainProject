using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.Controllers.Manage
{
    [RoutePrefix("Manage/Calendar")]
    public class CalendarController : SoeApiController
    {
        #region Variables

        private readonly CalendarManager cm;

        #endregion

        #region Constructor

        public CalendarController(CalendarManager cm)
        {
            this.cm = cm;
        }

        #endregion

        #region Holiday

        [HttpGet]
        [Route("Holidays/{year:int}/{onlyRedDay:bool}/{onlyHistorical:bool}/{addEmptyRow:bool}")]
        public IHttpActionResult GetHolidays(int year, bool onlyRedDay, bool onlyHistorical, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, cm.GetHolidaysByCompany(base.ActorCompanyId, year, onlyRedDay, onlyHistorical, false));
        }

        #endregion

        #region SchoolHoliday

        [HttpGet]
        [Route("SchoolHoliday/")]
        public IHttpActionResult GetSchoolHolidays()
        {
            return Content(HttpStatusCode.OK, cm.GetSchoolHolidays(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("SchoolHoliday/{schoolHolidayId:int}")]
        public IHttpActionResult GetSchoolHoliday(int schoolHolidayId)
        {
            return Content(HttpStatusCode.OK, cm.GetSchoolHoliday(schoolHolidayId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("SchoolHoliday")]
        public IHttpActionResult SaveSchoolHoliday(SchoolHolidayDTO schoolHolidayDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, cm.SaveSchoolHoliday(schoolHolidayDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("SchoolHoliday/{schoolHolidayId:int}")]
        public IHttpActionResult DeleteSchoolHoliday(int schoolHolidayId)
        {
            return Content(HttpStatusCode.OK, cm.DeleteSchoolHoliday(schoolHolidayId, base.ActorCompanyId));
        }

        #endregion

        #region Year

        [HttpGet]
        [Route("Years/{yearsBack:int}")]
        public IHttpActionResult GetYears(int yearsBack)
        {
            return Content(HttpStatusCode.OK, CalendarUtility.GetYears(yearsBack).ToSmallGenericTypes());
        }

        #endregion
    }
}