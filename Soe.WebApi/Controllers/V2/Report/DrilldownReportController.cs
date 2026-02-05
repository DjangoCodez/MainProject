using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Report")]
    public class DrilldownReportController : SoeApiController
    {
        #region Variables

        private readonly ReportManager rm;
        private readonly ReportDataManager rdm;
        private readonly VoucherManager vm;

        #endregion

        #region Constructor

        public DrilldownReportController(ReportManager rm,ReportDataManager rdm, VoucherManager vm)
        {
            this.rm = rm;
            this.rdm = rdm;
            this.vm = vm;
        }

        #endregion

        #region DrilldownReport

        [HttpGet]
        [Route("DrilldownReports/{onlyOriginal:bool}/{onlyStandard:bool}")]
        public IHttpActionResult GetDrilldownReports(bool onlyOriginal, bool onlyStandard)
        {
            return Content(HttpStatusCode.OK, rm.GetReportsWithDrilldown(base.ActorCompanyId, null, onlyOriginal: onlyOriginal, onlyStandard: onlyStandard).ToReportViewDTOs());
        }

        [HttpGet]
        [Route("DrilldownReport/{reportId:int}/{accountPerioIdFrom:int}/{accountPeriodIdTo:int}/{budgetHeadId:int}")]
        public IHttpActionResult GetDrilldownReport(int reportId, int accountPerioIdFrom, int accountPeriodIdTo, int budgetHeadId)
        {
            return Content(HttpStatusCode.OK, rdm.CreateDrilldownReportDataFlattened(reportId, accountPerioIdFrom, accountPeriodIdTo, base.ActorCompanyId, budgetHeadId));
        }

        [HttpPost]
        [Route("DrilldownReport/VoucherRows/")]
        public IHttpActionResult GetDrilldownReportVoucherRows(SearchVoucherRowsAngDTO dto)
        {
            return Content(HttpStatusCode.OK, vm.SearchVoucherRowsDto(base.ActorCompanyId, dto));
        }

        #endregion

    }
}