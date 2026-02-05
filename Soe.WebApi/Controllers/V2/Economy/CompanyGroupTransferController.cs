
using Soe.WebApi.Controllers;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Data;
using System.Net;
using System.Web.Http;
using Soe.WebApi.Models;
using SoftOne.Soe.Common.Util;
using System.Linq;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/Accounting/ConsolidatingAccounting")]
    public class CompanyGroupTransferController : SoeApiController
    {
        private readonly CompanyManager com;
        private readonly VoucherManager vm;
        private readonly AccountManager am;

        #region Constructor

        public CompanyGroupTransferController(CompanyManager com, VoucherManager vm, AccountManager am)
        {
            this.com = com;
            this.vm = vm;
            this.am = am;
        }

        #endregion


        #region CompanyGroupTransfer

        [HttpGet]
        [Route("ConsolidatingAccounting/CompanyGroupVoucherHistory/{accountYearId:int}/{transferType:int}")]
        public IHttpActionResult GetCompanyGroupVoucherHistory(int accountYearId, int transferType)
        {
            if (transferType == 1)
                return Content(HttpStatusCode.OK, vm.GetCompanyGroupTransferHistoryResult(accountYearId, base.ActorCompanyId, transferType));
            else if (transferType == 2)
                return Content(HttpStatusCode.OK, com.GetCompanyGroupTransferHistoryBudget(accountYearId, base.ActorCompanyId, transferType));
            else
                return Content(HttpStatusCode.OK, com.GetCompanyGroupTransferHistoryBalance(accountYearId, base.ActorCompanyId, transferType));
        }

        [HttpPost]
        [Route("ConsolidatingAccounting/CompanyGroupVoucherSerie/{accountYearId:int}")]
        public IHttpActionResult SaveCompanyGroupVoucherSeries(int accountYearId)
        {
            return Content(HttpStatusCode.OK, vm.AddCompanyGroupVoucherSeries(accountYearId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("ConsolidatingAccounting/CompanyGroupTransfer/")]
        public IHttpActionResult CompanyGroupTransfer(CompanyGroupTransferModel model)
        {
            var dims = am.GetAccountDimsByCompany(base.ActorCompanyId, false, false).ToDTOs();
            int companyGroupDimId = dims != null ? dims.Where(f => f.AccountDimNr == 1).Select(s => s.AccountDimId).FirstOrDefault() : 0;

            if (model.TransferType == CompanyGroupTransferType.Consolidation)
            {
                return Content(HttpStatusCode.OK, com.TransferCompanyGroupConsolidation(base.ActorCompanyId, base.LicenseId, model.AccountYearId, model.VoucherSeriesId, model.PeriodFrom, model.PeriodTo, model.IncludeIB, companyGroupDimId));
            }
            else if (model.TransferType == CompanyGroupTransferType.Budget)
            {
                return Content(HttpStatusCode.OK, com.TransferCompanyGroupBudget(base.ActorCompanyId, model.AccountYearId, model.BudgetCompanyGroup, model.BudgetCompanyFrom, model.BudgetChild, companyGroupDimId));
            }
            else
            {
                return Content(HttpStatusCode.OK, com.TransferCompanyGroupIncomingBalance(base.ActorCompanyId, model.AccountYearId, companyGroupDimId));
            }
        }

        [HttpPost]
        [Route("ConsolidatingAccounting/CompanyGroupTransfer/Delete/{companyGroupTransferHeadId:int}")]
        public IHttpActionResult DeleteCompanyGroupTransfer(int companyGroupTransferHeadId)
        {
            return Content(HttpStatusCode.OK, com.DeleteCompanyGroupTransfer(base.ActorCompanyId, companyGroupTransferHeadId));
        }

        #endregion
    }
}
