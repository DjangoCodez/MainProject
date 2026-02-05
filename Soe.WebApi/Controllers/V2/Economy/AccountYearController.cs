using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Economy/AccountYear")]
    public class AccountYearController : SoeApiController
    {
        #region Variables

        private readonly VoucherManager vm;
        private readonly AccountManager am;
        private readonly GrossProfitManager gpm;

        #endregion

        #region Constructor

        public AccountYearController(VoucherManager vm,AccountManager am, GrossProfitManager gpm)
        {
            this.vm = vm;
            this.am = am;
            this.gpm = gpm;
        }

        #endregion

        #region AccountYear

        [HttpGet]
        [Route("Current")]
        public IHttpActionResult GetCurrentAccountYear()
        {
            return Content(HttpStatusCode.OK, am.GetCurrentAccountYear(base.ActorCompanyId).ToDTO());
        }

        [HttpGet]
        [Route("Selected")]
        public IHttpActionResult GetSelectedAccountYear()
        {
            return Content(HttpStatusCode.OK, am.GetSelectedAccountYear(base.ActorCompanyId, base.UserId).ToDTO());
        }

        [HttpGet]
        [Route("Dict/{addEmptyRow:bool}/{excludeNew:bool}")]
        public IHttpActionResult GetAccountYears(bool addEmptyRow, bool excludeNew)
        {
            return Content(HttpStatusCode.OK, am.GetAccountYearsDict(base.ActorCompanyId, false, excludeNew, false, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Id/{date}")]
        public IHttpActionResult GetAccountYearIdByDate(string date)
        {
            return Content(HttpStatusCode.OK, am.GetAccountYearId(BuildDateTimeFromString(date, true).Value, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("{id:int}/{loadPeriods:bool}")]
        public IHttpActionResult GetAccountYearId(int id, bool loadPeriods)
        {
            return Content(HttpStatusCode.OK, am.GetAccountYear(id, loadPeriods, loadPeriods).ToDTO(getPeriods: loadPeriods, doVoucherCheck: loadPeriods));
        }

        [HttpGet]
        [Route("{date}")]
        public IHttpActionResult GetAccountYear(string date)
        {
            return Content(HttpStatusCode.OK, am.GetAccountYear(BuildDateTimeFromString(date, true).Value, base.ActorCompanyId).ToDTO());
        }

        [HttpGet]
        [Route("All/{getPeriods:bool}/{excludeNew:bool}")]
        public IHttpActionResult GetAllAccountYears(bool getPeriods, bool excludeNew)
        {
            return Content(HttpStatusCode.OK, am.GetAccountYears(base.ActorCompanyId, false, true, excludeNew).ToDTOs(true, getPeriods));
        }

        [HttpPost]
        [Route("")]
        public IHttpActionResult SaveAccountYear(SaveAccountYearModel model)
        {
            return Content(HttpStatusCode.OK, am.SaveAccountYear(model.AccountYear, model.VoucherSeries, base.ActorCompanyId, model.KeepNumbers));
        }

        [HttpPost]
        [Route("CopyVoucherTemplates/{accountYearId:int}")]
        public IHttpActionResult CopyVoucherTemplatesFromPreviousAccountYear(int accountYearId)
        {
            return Content(HttpStatusCode.OK, vm.CopyVoucherTemplatesCheckExisting(base.ActorCompanyId, accountYearId));
        }

        [HttpPost]
        [Route("CopyGrossProfitCodes/{accountYearId:int}")]
        public IHttpActionResult CopyGrossProfitCodes(int accountYearId)
        {
            return Content(HttpStatusCode.OK, gpm.CopyGrossProfitCodesCheckExisting(base.ActorCompanyId, accountYearId));
        }

        [HttpDelete]
        [Route("{accountYearId:int}")]
        public IHttpActionResult DeleteAccountYear(int accountYearId)
        {
            return Content(HttpStatusCode.OK, am.DeleteAccountYear(accountYearId, base.ActorCompanyId));
        }

        #endregion

    }
}