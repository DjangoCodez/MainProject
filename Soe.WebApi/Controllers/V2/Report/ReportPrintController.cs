using System.Net;
using System.Web.Http;
using SoftOne.Soe.Business.Core;
using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System.Collections.Generic;
using System.Linq;
using System;
using Org.BouncyCastle.Asn1.X500;
using Soe.WebApi.Extensions;
using SoftOne.Soe.Common.DTO;
using Soe.WebApi.Binders;
using System.Web.Http.ModelBinding;

namespace Soe.WebApi.V2.Report
{
    [RoutePrefix("V2/ReportPrint")]
    public class ReportPrintController : SoeApiController
    {
        #region Variables

        private readonly ReportManager rm;
        private readonly ReportDataManager rdm;
        private readonly VoucherManager vm;
        private readonly ProjectManager pm;
        private readonly ImportExportManager iem;
        private readonly InvoiceDistributionManager idm;

        #endregion

        #region Constructor

        public ReportPrintController(ReportManager rm, ReportDataManager rdm, VoucherManager vm, ProjectManager pm, ImportExportManager iem, InvoiceDistributionManager idm)
        {
            this.rm = rm;
            this.rdm = rdm;
            this.vm = vm;
            this.pm = pm;
            this.iem = iem;
            this.idm = idm;
        }

        #endregion

        #region Print

        [HttpGet]
        [Route("Print/Url/{sysReportTemplateTypeId:int}/{id:int}")]
        public IHttpActionResult GetReportPrintUrl(int sysReportTemplateTypeId, int id)
        {
            return Content(HttpStatusCode.OK, rm.GetReportPrintUrl(sysReportTemplateTypeId, id));
        }

        [HttpGet]
        [Route("Print/BalanceList/Url/{reportId:int}/{sysReportTemplateType:int}")]
        public IHttpActionResult GetBalanceListReportPrintUrl(int reportId, int sysReportTemplateType, [ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] invoiceIds)
        {
            return Content(HttpStatusCode.OK, rm.GetBalanceListPrintUrl(invoiceIds.ToList(), reportId, sysReportTemplateType, new List<int>()));
        }

        [HttpPost]
        [Route("Print/BalanceList/Url/")]
        public IHttpActionResult GetBalanceListReportPrintUrl(GenericPrintUrlModel model)
        {
            return Content(HttpStatusCode.OK, rm.GetBalanceListPrintUrl(model.ItemIds, model.ReportId, model.SysReportTemplateTypeId, model.SecondaryItemIds));
        }

        [HttpPost]
        [Route("Print/InvoiceInterestPrintUrl/")]
        public IHttpActionResult InvoiceInterestPrintUrl(List<int> customerInvoiceIds)
        {
            return Ok(rm.GetInvoiceInterestPrintUrl(customerInvoiceIds));
        }

        [HttpPost]
        [Route("Print/ProductListReportUrl/")]
        public IHttpActionResult ProductListPrintUrl(GetProductListPrintUrlModel model)
        {
            return Ok(rm.GetProductListPrintUrl(model.productIds, model.ReportId, model.SysReportTemplateTypeId));
        }

        [HttpPost]
        [Route("Print/VoucherListPrintUrl/")]
        public IHttpActionResult GetVoucherListPrintUrl(List<int> voucherListIds)
        {
            return Ok(rm.GetVoucherListPrintUrl(voucherListIds));
        }

        [HttpGet]
        [Route("Print/InterestRateCalculationPrintUrl/{reportId:int}/{sysReportTemplateType:int}")]
        public IHttpActionResult GetInterestRateCalculationPrintUrl(int reportId, int sysReportTemplateType, [ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] invoiceIds)
        {
            return Ok(rm.GetInterestRateCalculationPrintUrl(invoiceIds.ToList(), reportId, sysReportTemplateType));
        }

        [HttpPost]
        [Route("Print/CustomerInvoiceIOReportUrl/")]
        public IHttpActionResult GetCustomerInvoiceIOReportUrl(GetIOPrintUrlModel model)
        {
            return Ok(rm.GetCustomerInvoiceIOReportPrintUrl(model.IoIds, model.ReportId, model.SysReportTemplateTypeId));
        }

        [HttpPost]
        [Route("Print/VoucherHeadIOReportUrl/")]
        public IHttpActionResult GetVoucherHeadIOReportUrl(GetIOPrintUrlModel model)
        {
            return Ok(rm.GetVoucherHeadIOReportUrl(model.IoIds, model.ReportId, model.SysReportTemplateTypeId));
        }

        [HttpGet]
        [Route("Print/DefaultAccountingOrderPrintUrl/{voucherHeadId:int}")]
        public IHttpActionResult GetDefaultAccountingOrderPrintUrl(int voucherHeadId)
        {
            return Ok(rm.GetDefaultAccountingOrderPrintUrl(voucherHeadId));
        }

        [HttpPost]
        [Route("Print/OrderPrintUrl/")]
        public IHttpActionResult GetOrderPrintUrl(GetOrderPrintUrlModel model)
        {
            return Ok(rm.GetOrderPrintUrl(model.InvoiceIds, model.ReportId, model.EmailRecipients, model.LanguageId, model.InvoiceNr, model.ActorCustomerId, model.RegistrationType, model.InvoiceCopy));
        }

        [HttpPost]
        [Route("Print/OrderPrintUrl/Single")]
        public IHttpActionResult GetOrderPrintUrlSingle(GetOrderPrintUrlSingleModel model)
        {
            return Ok(rm.GetOrderPrintUrlSingle(model.InvoiceId, model.ReportId, model.EmailRecipients, model.LanguageId, model.InvoiceNr, model.ActorCustomerId, model.PrintTimeReport, model.IncludeOnlyInvoicedTime, model.RegistrationType, model.InvoiceCopy, model.EmailTemplateId, model.AsReminder));
        }

        [HttpPost]
        [Route("Print/PurchasePrintUrl/")]
        public IHttpActionResult GetPurchasePrintUrl(GetPurchasePrintUrlModel model)
        {
            return Ok(rm.GetPurchasePrintUrl(model.PurchaseIds, model.ReportId, model.LanguageId));
        }

        [HttpPost]
        [Route("Print/SendReport/")]
        public IHttpActionResult SendReport(GetOrderPrintUrlSingleModel model)
        {
            return Ok(idm.SendAsEmail(model.InvoiceId, model.ReportId, model.EmailRecipients, model.LanguageId, model.InvoiceNr, model.ActorCustomerId, model.PrintTimeReport, model.IncludeOnlyInvoicedTime, model.RegistrationType, model.InvoiceCopy, model.AsReminder, model.EmailTemplateId, model.AddAttachmentsToEinvoice, model.AttachmentIds, model.ChecklistIds, model.MergePdfs, model.SingleRecipient, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Print/ProjectTransactionsPrintUrl/")]
        public IHttpActionResult GetProjectTransactionsPrintUrl(GetProjectTransactionsPrintUrlModel model)
        {
            return Ok(rm.GetProjectTransactionsPrintUrl(this.ActorCompanyId, model.ReportId, model.SysReportTemplateTypeId, model.ExportType, model.ProjectIds,
                model.OfferNrFrom, model.OfferNrTo, model.OrderNrFrom, model.OrderNrTo, model.InvoiceNrFrom, model.InvoiceNrTo,
                model.EmployeeNrFrom, model.EmployeeNrTo, model.PayrollProductNrFrom, model.PayrollProductNrTo, model.InvoiceProductNrFrom, model.InvoiceProductNrTo,
                model.PayrollTransactionDateFrom, model.PayrollTransactionDateTo, model.InvoiceTransactionDateFrom, model.InvoiceTransactionDateTo,
                model.IncludeChildProjects, model.Dim2Id, model.Dim2From, model.Dim2To,
                model.Dim3Id, model.Dim3From, model.Dim3To, model.Dim4Id, model.Dim4From, model.Dim4To, model.Dim5Id, model.Dim5From, model.Dim5To, model.Dim6Id, model.Dim6From, model.Dim6To, model.ExportType));
        }

        [HttpGet]
        [Route("Print/PayrollProductListPrintUrl/{reportId:int}/{sysReportTemplateType:int}")]
        public IHttpActionResult GetPayrollProductReportPrintUrl(int reportId, int sysReportTemplateType, [ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] productIds)
        {
            return Content(HttpStatusCode.OK, rm.GetPayrollProductListPrintUrl(productIds.ToList(), reportId, sysReportTemplateType));
        }

        [HttpGet]
        [Route("Print/ChecklistPrintUrl/{invoiceId:int}/{headRecordId:int}/{reportId:int}")]
        public IHttpActionResult GetChecklistPrintUrl(int invoiceId, int headRecordId, int reportId)
        {
            return Content(HttpStatusCode.OK, rm.GetChecklistPrintUrl(invoiceId, headRecordId, reportId));
        }

        [HttpPost]
        [Route("Print/TimeEmployeeSchedulePrintUrl/")]
        public IHttpActionResult GetTimeEmployeeSchedulePrintUrl(GetTimeEmployeeSchedulePrintUrlModel model)
        {
            return Content(HttpStatusCode.OK, rm.GetTimeEmployeeSchedulePrintUrl(model.EmployeeIds, model.ShiftTypeIds, model.DateFrom, model.DateTo, model.ReportId, model.ReportTemplateType));
        }

        [HttpPost]
        [Route("Print/TimeScheduleTasksAndDeliverysReportPrintUrl/")]
        public IHttpActionResult GetTimeScheduleTasksAndDeliverysReportPrintUrl(GetTimeScheduleTasksAndDeliverysReportPrintUrlModel model)
        {
            return Content(HttpStatusCode.OK, rm.GetTimeScheduleTasksAndDeliverysReportPrintUrl(model.TimeScheduleTaskIds, model.TimeScheduleDeliveryHeadIds, model.DateFrom, model.DateTo, model.IsDayView));
        }

        [HttpPost]
        [Route("Print/HouseholdTaxDeduction/")]
        public IHttpActionResult GetHouseholdTaxDeductionPrintUrl(HouseholdTaxDeductionPrintUrlModel model)
        {
            return Content(HttpStatusCode.OK, rm.GetHouseholdPrintUrl(model.CustomerInvoiceRowIds, model.ReportId, model.SysReportTemplateTypeId, model.NextSequenceNumber, model.UseGreen));
        }

        [HttpPost]
        [Route("Print/StockInventoryPrintUrl/")]
        public IHttpActionResult StockInventoryPrintUrl(GetStockInventoryPrintUrlModel model)
        {
            return Ok(rm.GetStockInventoryReportPrintUrl(model.StockInventoryIds, model.ReportId));
        }

        #endregion

    }
}