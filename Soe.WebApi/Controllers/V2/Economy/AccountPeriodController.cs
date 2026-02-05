using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Economy/AccountPeriod")]
    public class AccountPeriodController : SoeApiController
    {
        #region Variables

        private readonly VoucherManager vm;
        private readonly AccountManager am;
        private readonly GrossProfitManager gpm;

        #endregion

        #region Constructor

        public AccountPeriodController(VoucherManager vm,AccountManager am, GrossProfitManager gpm)
        {
            this.vm = vm;
            this.am = am;
            this.gpm = gpm;
        }

        #endregion

        #region AccountPeriod

        [HttpGet]
        [Route("AccountPeriod/{accountYearId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetAccountPeriodDict(int accountYearId, bool addEmptyRow)
        {
            var dict = am.GetAccountPeriodsIntervalDict(accountYearId, addEmptyRow).ToSmallGenericTypes();
            dict.Sort((x, y) => x.Name.CompareTo(y.Name));
            return Content(HttpStatusCode.OK, dict);
        }

        [HttpGet]
        [Route("AccountPeriods/{accountYearId:int}")]
        public IHttpActionResult GetAccountPeriods(int accountYearId)
        {
            return Content(HttpStatusCode.OK, am.GetAccountPeriods(accountYearId, false).ToDTOs());
        }

        [HttpGet]
        [Route("AccountPeriod/{accountYearId:int}/{date}/{includeAccountYear:bool}")]
        public IHttpActionResult GetAccountPeriod(int accountYearId, string date, bool includeAccountYear)
        {
            return Content(HttpStatusCode.OK, am.GetAccountPeriod(accountYearId, BuildDateTimeFromString(date, true).Value, base.ActorCompanyId, includeAccountYear).ToDTO());
        }

        [HttpGet]
        [Route("AccountPeriod/Id/{accountYearId:int}/{date}")]
        public IHttpActionResult GetAccountPeriodId(int accountYearId, string date)
        {
            return Content(HttpStatusCode.OK, am.GetAccountPeriodId(accountYearId, base.ActorCompanyId, BuildDateTimeFromString(date, true).Value));
        }

        [HttpPost]
        [Route("AccountPeriod/UpdateStatus/{accountPeriodId:int}/{status:int}")]
        public IHttpActionResult UpdateAccountPeriodStatus(int accountPeriodId, int status)
        {
            return Content(HttpStatusCode.OK, am.UpdateAccountPeriodStatus(accountPeriodId, (TermGroup_AccountStatus)status));
        }

        #endregion
    }
}