using Soe.WebApi.Controllers;
using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core.TimeEngine;
using SoftOne.Soe.Common.Util;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Time/EarnedHoliday")]
    public class TimeEarnedDaysController : SoeApiController
    {
        #region Variables

        private readonly TimeTransactionManager ttm;
        private readonly TimeEngineManager tem;
        private readonly CalendarManager cm;

        #endregion

        #region Constructor

        public TimeEarnedDaysController(TimeTransactionManager ttm, TimeEngineManager tem, CalendarManager cm)
        {
            this.ttm = ttm;
            this.tem = tem;
            this.cm = cm;
        }

        #endregion

        #region Earned Holiday

        [HttpPost]
        [Route("Load/")]
        public IHttpActionResult LoadEarnedHolidaysContent(EarnedHolidayModel model)
        {
            return Content(HttpStatusCode.OK, ttm.LoadEarnedHolidaysContent(model.HolidayId, model.Year, model.LoadSuggestions, base.UserId, base.RoleId, base.ActorCompanyId, employeeEarnedHolidaysInput: model.EmployeeEarnedHolidays));
        }

        [HttpPost]
        [Route("CreateTransactions/")]
        public IHttpActionResult CreateTransactionsForEarnedHoliday(ManageTransactionsForEarnedHolidayModel model)
        {
            return Content(HttpStatusCode.OK, tem.CreateTransctionsForEarnedHoliday(model.HolidayId, model.EmployeeIds, model.Year));
        }

        [HttpPost]
        [Route("DeleteTransactions/")]
        public IHttpActionResult DeleteTransactionsForEarnedHolidayContent(ManageTransactionsForEarnedHolidayModel model)
        {
            return Content(HttpStatusCode.OK, tem.DeleteTransctionsForEarnedHoliday(model.HolidayId, model.EmployeeIds, model.Year));
        }

        [HttpGet]
        [Route("Years/{yearsBack:int}")]
        public IHttpActionResult GetYears(int yearsBack)
        {
            return Content(HttpStatusCode.OK, CalendarUtility.GetYears(yearsBack).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Holidays/{year:int}/{onlyRedDay:bool}/{onlyHistorical:bool}")]
        public IHttpActionResult GetHolidays(int year, bool onlyRedDay, bool onlyHistorical)
        {
            return Content(HttpStatusCode.OK, cm.GetHolidaysByCompany(base.ActorCompanyId, year, onlyRedDay, onlyHistorical, false));
        }

        #endregion
    }
}