namespace Soe.WebApi.V2.Report
{
    using Soe.WebApi.Controllers;
    using SoftOne.Soe.Business.Core.Reports;
    using SoftOne.Soe.Common.DTO;
    using SoftOne.Soe.Common.DTO.Reports;
    using SoftOne.Soe.Common.Util;
    using System.Linq;
    using System.Net;
    using System.Web.Http;

    [RoutePrefix("V2/RequestReport/Print")]
    public class RequestReportController : SoeApiController
    {
        #region Variables

        private readonly RequestReportManager rrm;

        #endregion

        #region Constructor

        public RequestReportController(
            RequestReportManager rrm)
        {
            this.rrm = rrm;
        }

        #endregion

        #region Print

        [HttpPost]
        [Route("Project")]
        public IHttpActionResult PrintProjectReport(ProjectPrintDTO model)
        {
            if (!ModelState.IsValid)
            {
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            }
            else
            {
                DownloadFileDTO dto = rrm.PrintProjectReport(model);
                return Content(HttpStatusCode.OK, dto);
            }                
        }

        [HttpPost]
        [Route("Project/Timebook")]
        public IHttpActionResult PrintProjectTimeBook(ProjectTimeBookPrintDTO model)
        {
            if (!ModelState.IsValid)
            {
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            }
            else
            {
                DownloadFileDTO dto = rrm.PrintProjectTimeBook(model);
                return Content(HttpStatusCode.OK, dto);
            }
        }

        [HttpGet]
        [Route("Voucher/{voucherHeadId:int}")]
        public IHttpActionResult PrintVoucher(int voucherHeadId, bool queue = false)
        {
            DownloadFileDTO dto = rrm.PrintVoucherDefaultAccountingOrder(voucherHeadId, queue);
            return Content(HttpStatusCode.OK, dto);
        }

        [HttpPost]
        [Route("VoucherList")]
        public IHttpActionResult PrintVoucherList(ReportPrintDTO model)
        {
            if (!ModelState.IsValid || model?.Ids?.Any() != true)
            {
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            }
            else
            {
                model.Ids = model.Ids.Distinct().ToList();
                DownloadFileDTO dto = rrm.PrintVoucherList(model);
                return Content(HttpStatusCode.OK, dto);
            }
        }

        [HttpGet]
        [Route("Account/{accountId:int}")]
        public IHttpActionResult PrintAccount(
            int accountId, bool queue = false)
        {
            DownloadFileDTO dto = rrm.PrintAccount(accountId, queue);
            return Content(HttpStatusCode.OK, dto);
        }

        [HttpPost]
        [Route("SupplierBalanceList")]
        public IHttpActionResult PrintSupplierBalanceList(
            BalanceListPrintDTO model)
        {
            if (!ModelState.IsValid || model?.Ids?.Any() != true)
            {
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            }
            else
            {
                model.Ids = model.Ids.Distinct().ToList();
                DownloadFileDTO dto = rrm.PrintBalanceList(
                    model,
                    SoeReportTemplateType.SupplierBalanceList);
                return Content(HttpStatusCode.OK, dto);
            }
        }

        [HttpPost]
        [Route("CustomerBalanceList")]
        public IHttpActionResult PrintCustomerBalanceList(
            ReportPrintDTO model)
        {
            if (!ModelState.IsValid || model?.Ids?.Any() != true)
            {
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            }
            else
            {
                model.Ids = model.Ids.Distinct().ToList();
                BalanceListPrintDTO balanceListModel = new BalanceListPrintDTO
                {
                    Ids = model.Ids,
                    Queue = model.Queue,
                    CompanySettingType = CompanySettingType.CustomerDefaultBalanceList
                };

                DownloadFileDTO dto = rrm.PrintBalanceList(
                    balanceListModel,
                    SoeReportTemplateType.CustomerBalanceList);
                return Content(HttpStatusCode.OK, dto);
            }
        }

        [HttpPost]
        [Route("InvoicesJournal")]
        public IHttpActionResult PrintInvoicesJournal(
            ReportPrintDTO model)
        {
            if (!ModelState.IsValid || model?.Ids?.Any() != true)
            {
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            }
            else
            {
                model.Ids = model.Ids.Distinct().ToList();
                DownloadFileDTO dto = rrm.PrintInvoicesJournal(model);
                return Content(HttpStatusCode.OK, dto);
            }
        }

  
        [HttpPost]
        [Route("IOVoucher")]
        public IHttpActionResult PrintIOVoucher(ReportPrintDTO model)
        {
            if (!ModelState.IsValid || model?.Ids?.Any() != true)
            {
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            }
            else
            {
                model.Ids = model.Ids.Distinct().ToList();
                DownloadFileDTO dto = rrm.PrintIOVoucher(model);
                return Content(HttpStatusCode.OK, dto);
            }
        }

        [HttpPost]
        [Route("IOCustomerInvoice")]
        public IHttpActionResult PrintIOCustomerInvoice(ReportPrintDTO model)
        {
            if (!ModelState.IsValid || model?.Ids?.Any() != true)
            {
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            }
            else
            {
                model.Ids = model.Ids.Distinct().ToList();
                DownloadFileDTO dto = rrm.PrintIOCustomerInvoice(model);
                return Content(HttpStatusCode.OK, dto);
            }
        }

        [HttpGet]
        [Route("StockInventory")]
        public IHttpActionResult PrintStockInventory(int reportId, int stockInventoryHeadId, bool queue)
        {
            return Content(HttpStatusCode.OK, rrm.PrintInventoryReport(
                reportId, stockInventoryHeadId, queue));
        }

        [HttpPost]
        [Route("HouseholdTaxDeduction")]
        public IHttpActionResult PrintHouseholdTaxDeduction(HouseholdTaxDeductionPrintDTO model)
        {
            if (!ModelState.IsValid || model?.Ids?.Any() != true)
            {
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            }
            else
            {
                model.Ids = model.Ids.Distinct().ToList();
                DownloadFileDTO dto = rrm.PrintHouseholdTaxDeduction(model);
                return Content(HttpStatusCode.OK, dto);
            }
        }

        [HttpPost]
        [Route("CustomerInvoice")]
        public IHttpActionResult PrintCustomerInvoice(CustomerInvoicePrintDTO model)
        {
            if (!ModelState.IsValid || model?.Ids?.Any() != true)
            {
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            }
            else
            {
                model.Ids = model.Ids.Distinct().ToList();
                return Content(HttpStatusCode.OK, rrm.PrintCustomerInvoice(model, false));
            }
        }


        #endregion

    }
}