using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Time
{
    [RoutePrefix("V2/Economy/VoucherSeries")]
    public class VoucherSeriesController : SoeApiController
    {
        #region Variables

        private readonly VoucherManager vm;
        private readonly AccountManager am;

        #endregion

        #region Constructor

        public VoucherSeriesController(VoucherManager vm,AccountManager am)
        {
            this.vm = vm;
            this.am = am;
        }

        #endregion


        #region VoucherSeries

        [HttpGet]
        [Route("VoucherSeries/{accountYearId:int}/{includeTemplate:bool}")]
        public IHttpActionResult GetVoucherSeriesByYear(int accountYearId, bool includeTemplate)
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherSeriesByYear(accountYearId, base.ActorCompanyId, includeTemplate).ToDTOs());
        }

        [HttpGet]
        [Route("VoucherSeries/{accountYearDate}/{includeTemplate:bool}")]
        public IHttpActionResult GetVoucherSeriesByYear(string accountYearDate, bool includeTemplate)
        {
            var accountYearId = am.GetAccountYearId(BuildDateTimeFromString(accountYearDate, false).Value, base.ActorCompanyId);
            return Content(HttpStatusCode.OK, vm.GetVoucherSeriesByYear(accountYearId, base.ActorCompanyId, includeTemplate).ToDTOs());
        }

        //string periodDate
        [HttpGet]
        [Route("VoucherSeries/{accountYearId:int}/{type:int}")]
        public IHttpActionResult GetDefaultVoucherSeriesId(int accountYearId, CompanySettingType type)
        {
            return Content(HttpStatusCode.OK, vm.GetDefaultVoucherSeriesId(accountYearId, type, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("VoucherSeriesByYearRange/{fromAccountYearId:int}/{toAccountYearId:int}")]
        public IHttpActionResult GetVoucherSeriesByYearRange(int fromAccountYearId, int toAccountYearId)
        {
            return Content(HttpStatusCode.OK, vm.GetVoucherSeriesByYearDict(fromAccountYearId, toAccountYearId, base.ActorCompanyId, false, true));
        }

        [HttpGet]
        [Route("DictByYear/{accountYearId:int}/{addEmptyRow:bool}/{includeTemplate:bool}")]
        public IHttpActionResult GetVoucherSeriesDictByYear(int accountYearId, bool addEmptyRow, bool includeTemplate)
        { 
            return Content(HttpStatusCode.OK, vm.GetVoucherSeriesByYearDict(accountYearId, base.ActorCompanyId, addEmptyRow, includeTemplate).ToSmallGenericTypes());
        }

        #endregion


    }
}