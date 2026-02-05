using Soe.WebApi.Binders;
using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Util.ReportGroups;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Soe.WebApi.Controllers
{
    [RoutePrefix("Report")]
    public class ReportController : SoeApiController
    {
        #region Variables

        private readonly CalendarManager calm;
        private readonly CategoryManager cm;
        private readonly EmployeeManager em;
        private readonly ImportExportManager iem;
        private readonly InvoiceDistributionManager idm;
        private readonly ProjectManager pm;
        private readonly ReportManager rm;
        private readonly ReportDataManager rdm;
        private readonly TimeScheduleManager tsm;
        private readonly VoucherManager vm;
        private readonly ScheduledJobManager sjm;
        private readonly BudgetManager bm;

        #endregion

        #region Constructor

        public ReportController(CalendarManager calm, CategoryManager cm, EmployeeManager em, ImportExportManager iem, InvoiceDistributionManager idm, ProjectManager pm, ReportManager rm, ReportDataManager rdm, TimeScheduleManager tsm, VoucherManager vm, ScheduledJobManager sjm, BudgetManager bm)
        {
            this.calm = calm;
            this.cm = cm;
            this.em = em;
            this.iem = iem;
            this.idm = idm;
            this.pm = pm;
            this.rm = rm;
            this.rdm = rdm;
            this.tsm = tsm;
            this.vm = vm;
            this.sjm = sjm;
            this.bm = bm;
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
        [Route("Print/InvoliceReminderPrintUrl/")]
        public IHttpActionResult InvoliceReminderPrintUrl(List<int> customerInvoiceIds)
        {
            return Ok(rm.GetInvoiceReminderPrintUrl(customerInvoiceIds));
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

        #endregion

        #region Reports

        [HttpGet]
        [Route("Reports/{sysReportTemplateTypeId:int}/{onlyOriginal:bool}/{onlyStandard:bool}/{addEmptyRow:bool}/{useRole:bool}")]
        public IHttpActionResult GetReports(int sysReportTemplateTypeId, bool onlyOriginal, bool onlyStandard, bool addEmptyRow, bool useRole)
        {
            return Content(HttpStatusCode.OK, rm.GetReportsByTemplateTypeDict(base.ActorCompanyId, (SoeReportTemplateType)sysReportTemplateTypeId, onlyOriginal, onlyStandard, addEmptyRow, useRole ? base.RoleId : (int?)null).ToSmallGenericTypes());
        }
        [HttpGet]
        [Route("Reports/{actorCompanyId:int}/{sysReportTemplateTypeId:int}/{onlyOriginal:bool}/{onlyStandard:bool}/{addEmptyRow:bool}/{useRole:bool}")]
        public IHttpActionResult GetReports(int actorCompanyId, int sysReportTemplateTypeId, bool onlyOriginal, bool onlyStandard, bool addEmptyRow, bool useRole)
        {
            return Content(HttpStatusCode.OK, rm.GetReportsByTemplateTypeDict(actorCompanyId, (SoeReportTemplateType)sysReportTemplateTypeId, onlyOriginal, onlyStandard, addEmptyRow, useRole ? base.RoleId : (int?)null).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Reports/{module:int}/{onlyOriginal:bool}/{onlyStandard:bool}")]
        public IHttpActionResult GetReportViewsForModule(int module, bool onlyOriginal, bool onlyStandard)
        {
            return Content(HttpStatusCode.OK, rm.GetReports(base.ActorCompanyId, base.RoleId, module: module, onlyOriginal: onlyOriginal, onlyStandard: onlyStandard, loadReportSelection: true, setIsSystemReport: true).ToReportViewGridDTOs());
        }

        [HttpGet]
        [Route("Report/{reportId:int}/{loadReportSelection:bool}/{loadSysReportTemplateType:bool}/{loadReportRolePermission:bool}/{loadSettings:bool}/{loadSysReportTemplateSettings:bool}")]
        public IHttpActionResult GetReport(int reportId, bool loadReportSelection, bool loadSysReportTemplateType, bool loadReportRolePermission, bool loadSettings, bool loadSysReportTemplateSettings)
        {
            return Content(HttpStatusCode.OK, rm.GetReport(reportId, base.ActorCompanyId, false, loadReportSelection, loadReportRolePermission, loadSysReportTemplateType, loadSettings, loadSysReportTemplateSettings).ToDTO());
        }

        [HttpGet]
        [Route("Report/ExportTypes/{sysReportTemplateId:int}/{userReportTemplateId:int}/{sysReportType:int}")]
        public IHttpActionResult GetReportExportTypes(int sysReportTemplateId, int userReportTemplateId, SoeReportType sysReportType)
        {
            return Content(HttpStatusCode.OK, rm.GetReportExportTypes(sysReportTemplateId.ToNullable(), userReportTemplateId.ToNullable(), sysReportType));
        }

        [HttpPost]
        [Route("Report/Save/")]
        public IHttpActionResult SaveReport(ReportDTO reportDTO)
        {
            return Content(HttpStatusCode.OK, rm.SaveReport(reportDTO, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("Report/{reportId}")]
        public IHttpActionResult DeleteReport(int reportId)
        {
            return Content(HttpStatusCode.OK, rm.DeleteReport(reportId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("StandardReport/{settingMainType:int}/{settingType:int}/{reportTemplateType:int}")]
        public IHttpActionResult GetStandardReport(SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType)
        {
            return Content(HttpStatusCode.OK, rm.GetCompanySettingReport(settingMainType, settingType, reportTemplateType, base.ActorCompanyId, base.UserId, base.RoleId).ToDTO());
        }

        [HttpGet]
        [Route("StandardReportId/{settingMainType:int}/{settingType:int}/{reportTemplateType:int}")]
        public IHttpActionResult GetStandardReportId(SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType)
        {
            return Content(HttpStatusCode.OK, rm.GetCompanySettingReportId(settingMainType, settingType, reportTemplateType, base.ActorCompanyId, base.UserId, base.RoleId));
        }

        [HttpGet]
        [Route("SettingOrStandardReportId/{settingMainType:int}/{settingType:int}/{reportTemplateType:int}/{reportType:int}")]
        public IHttpActionResult GetStandardReportId(SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType, SoeReportType reportType)
        {
            var report = rm.GetSettingOrStandardReport(settingMainType, settingType, reportTemplateType, reportType, base.ActorCompanyId, base.UserId, base.RoleId);
            return Content(HttpStatusCode.OK, report != null ? report.ReportId : 0);
        }

        [HttpGet]
        [Route("SettingReportCheckPermission/{settingMainType:int}/{settingType:int}/{reportTemplateType:int}")]
        public IHttpActionResult SettingReportCheckPermission(SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType)
        {
            return Content(HttpStatusCode.OK, rm.GetSettingReport(settingMainType, settingType, reportTemplateType, base.ActorCompanyId, base.UserId, base.RoleId).ToSmallDTO());
        }

        [HttpGet]
        [Route("SettingReportHasPermission/{settingMainType:int}/{settingType:int}/{reportTemplateType:int}")]
        public IHttpActionResult SettingReportHasPermission(SettingMainType settingMainType, CompanySettingType settingType, SoeReportTemplateType reportTemplateType)
        {
            return Content(HttpStatusCode.OK, rm.HasReportRolePermission(settingMainType, settingType, reportTemplateType, base.ActorCompanyId, base.UserId, base.RoleId));
        }

        [HttpGet]
        [Route("Report/{reportId:int}")]
        public IHttpActionResult GetReportName(int reportId)
        {
            if (Request.HasAcceptValue(HttpExtensions.ACCEPT_SMALL_DTO))
                return Content(HttpStatusCode.OK, rm.GetReport(reportId, base.ActorCompanyId).ToSmallDTO());
            else
                return Content(HttpStatusCode.OK, rm.GetReport(reportId, base.ActorCompanyId).ToDTO());
        }

        [HttpGet]
        [Route("ReportsInPackage/{reportPackageId:int}")]
        public IHttpActionResult GetReportViewsInPackage(int reportPackageId)
        {
            return Content(HttpStatusCode.OK, rm.GetReportByPackage(base.ActorCompanyId, base.RoleId, reportPackageId).ToReportViewGridDTOs());
        }

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

        [HttpPost]
        [Route("ReportsForTypes/")]
        public IHttpActionResult GetReportsForTypes(GetReportsForTypesModel model)
        {
            return Content(HttpStatusCode.OK, rm.GetReports(base.ActorCompanyId, base.RoleId, sysReportTemplateTypeIds: model.ReportTemplateTypeIds, module: (int?)model.Module, onlyOriginal: model.OnlyOriginal, onlyStandard: model.OnlyStandard).ToReportViewDTOs());
        }

        [HttpPost]
        [Route("Project/Search/")]
        public IHttpActionResult GetProjectsBySearchNoLimit(ReportProjectSearchModel model)
        {
            return Content(HttpStatusCode.OK, pm.GetProjectsBySearch(base.ActorCompanyId, model.StatusIds, model.CategoryIds, model.StopDate, model.WithoutStopDate, model.SetStatusName, true));
        }

        [HttpGet]
        [Route("ReportImport/{dataStorageId:int}")]
        public IHttpActionResult GetReportsFromFile(int dataStorageId)
        {
            return Content(HttpStatusCode.OK, iem.importReportsFromTransferFile(dataStorageId, base.ActorCompanyId));
        }

        #endregion

        #region ReportsPackage

        [HttpGet]
        [Route("ReportPackage/{module:int}/{loadReport:bool}")]
        public IHttpActionResult GetReportPackagesForModule(int module, bool loadReport)
        {
            return Content(HttpStatusCode.OK, rm.GetReportPackagesForModule(base.ActorCompanyId, module, loadReport).ToGridDTOs());
        }

        #endregion

        #region ReportGroup

        [HttpGet]
        [Route("ReportGroup/GetReportGroupsByModule/{module:int}")]
        public IHttpActionResult GetReportGroupsByModule(int module)
        {
            return Content(HttpStatusCode.OK, rm.GetReportGroups(module, base.ActorCompanyId, false, false));
        }

        [HttpPost]
        [Route("ReportGroup/Delete")]
        public IHttpActionResult DeleteReportGroups(DeleteReportGroupsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, rm.DeleteReportGroups(base.ActorCompanyId, model.ReportGroupIds));
        }

        #endregion

        #region ReportHeader

        [HttpGet]
        [Route("ReportHeader/GetReportHeadersByModule/{module:int}/{loadReportInterval:bool}")]
        public IHttpActionResult GetReportHeadersByModule(int module, bool loadReportInterval)
        {
            return Content(HttpStatusCode.OK, rm.GetReportHeaders(module, base.ActorCompanyId, loadReportInterval));
        }

        [HttpPost]
        [Route("ReportHeader/Delete")]
        public IHttpActionResult UpdateReportHeadersState(DeleteReportHeadersModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, rm.DeleteReportHeaders(base.ActorCompanyId, model.ReportHeaderIds));
            }
        }

        #endregion

        #region ReportTemplates

        [HttpGet]
        [Route("SysReportTemplateTypes/{module:int}")]
        public IHttpActionResult GetSysReportTemplateTypes(int module)
        {
            return Content(HttpStatusCode.OK, rm.GetSysReportTemplateTypesForModuleDict(module, true).ToSmallGenericTypes());
        }
        [HttpGet]
        [Route("GetSysReportTemplateType/{reportTemplateId:int}/{standard:bool}")]
        public IHttpActionResult GetSysReportTemplateType(int reportTemplateId,bool standard)
        {
            var report = new Report() { ReportTemplateId = reportTemplateId ,Standard = standard};
            return Content(HttpStatusCode.OK, rm.GetSysReportTemplateType(report, base.ActorCompanyId).ToDTO());
        }
        

        [HttpGet]
        [Route("SysReportTemplates/{module:int}/{filterOnCountry:bool}")]
        public IHttpActionResult GetSysReportTemplatesForModule(int module, bool filterOnCountry)
        {
            return Content(HttpStatusCode.OK, rm.GetSysReportTemplatesForModule(module, base.ActorCompanyId, filterOnCountry).ToGridDTOs());
        }

        [HttpGet]
        [Route("UserReportTemplates/{module:int}")]
        public IHttpActionResult GetUserReportTemplatesForModule(int module)
        {
            return Content(HttpStatusCode.OK, rm.GetReportTemplatesGridDTOsForModule(base.ActorCompanyId, module));
        }

        [HttpGet]
        [Route("ReportTemplate/{reportTemplateId:int}/{isSystem:bool}")]
        public IHttpActionResult GetUserReportTemplate(int reportTemplateId, bool isSystem)
        {
            if (isSystem)
                return Content(HttpStatusCode.OK, rm.GetSysReportTemplate(reportTemplateId, true, true).ToDTO());
            else
                return Content(HttpStatusCode.OK, rm.GetReportTemplate(reportTemplateId, base.ActorCompanyId).ToDTO());
        }

        [HttpGet]
        [Route("ReportTemplate/ExportTypes")]
        public IHttpActionResult GetExportTypes()
        {
            return Content(HttpStatusCode.OK, rm.GetExportTypes().ToSmallGenericTypes());
        }

        [HttpPost]
        [Route("ReportTemplate/Upload")]
        public async Task<IHttpActionResult> UploadReportTemplate()
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var data = await Request.Content.ParseMultipartAsync();

                Extensions.HttpPostedFile file = data.Files["file"];
                if (file != null)
                {
                    ActionResult result = new ActionResult();
                    try
                    {
                        result.Value = file.File;
                        result.Value2 = file.Filename;
                    }
                    catch (Exception exception)
                    {
                        result.Success = false;
                        result.Exception = exception;
                    }

                    return Content(HttpStatusCode.OK, result);
                }
            }

            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        [HttpPost]
        [Route("ReportTemplate/Save/")]
        public IHttpActionResult SaveReportTemplate(SaveReportTemplateModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                if (model.IsSystem)
                    return Content(HttpStatusCode.OK, rm.SaveSysReportTemplate(model.ReportTemplate, model.TemplateData));
                else
                    return Content(HttpStatusCode.OK, rm.SaveReportTemplate(model.ReportTemplate, model.TemplateData, base.ActorCompanyId));
            }
        }

        [HttpPost]
        [Route("ReportTemplate/Delete")]
        public IHttpActionResult DeleteReportTemplate(DeleteReportTemplateModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else if (model.ReportTemplate.IsSystem)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, rm.DeleteReportTemplate(model.ReportTemplate.ReportTemplateId, base.ActorCompanyId));
        }

        #endregion

        #region ReportMenu

        [HttpGet]
        [Route("Menu/{module:int}/{sysReportType:int}")]
        public IHttpActionResult GetReportsForMenu(int module, int sysReportType)
        {
            return Content(HttpStatusCode.OK, rm.GetReportsForMenu(module, (SoeReportType)sysReportType, base.ActorCompanyId, base.RoleId, base.UserId, base.ParameterObject.IsSupportLoggedIn));
        }

        #region Item

        [HttpGet]
        [Route("Menu/Item/{reportId:int}/{sysReportType:int}")]
        public IHttpActionResult GetReportItem(int reportId, SoeReportType sysReportType)
        {
            return Content(HttpStatusCode.OK, rm.GetReportItem(reportId, base.ActorCompanyId, sysReportType));
        }

        [HttpGet]
        [Route("Menu/Item/GetPrintedReportForMenu/{reportPrintoutId:int}")]
        public IHttpActionResult GetPrintedReportForMenu(int reportPrintoutId)
        {
            return Content(HttpStatusCode.OK, rm.GetPrintedReportForMenu(reportPrintoutId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Menu/Item/GetPrintedXMLForMenu/{reportPrintoutId:int}")]
        public IHttpActionResult GetPrintedXMLForMenu(int reportPrintoutId)
        {
            return Content(HttpStatusCode.OK, rm.GetPrintedXMLForMenu(reportPrintoutId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Menu/Item/Selections/{reportId:int}/{type:int}")]
        public IHttpActionResult GetReportUserSelections(int reportId, int type)
        {
            return Content(HttpStatusCode.OK, rm.GetReportUserSelections(reportId, (ReportUserSelectionType)type, base.UserId, base.RoleId, base.ActorCompanyId).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Menu/Item/Selection/{reportUserSelectionId:int}")]
        public IHttpActionResult GetReportUserSelection(int reportUserSelectionId)
        {
            ReportUserSelection reportUserSelection = rm.GetReportUserSelection(reportUserSelectionId, loadReport: true, loadAccess: true);

            if (reportUserSelection != null)
                reportUserSelection.Report.SysReportTemplateTypeId = (int)rm.GetSoeReportTemplateType(reportUserSelection.Report, base.ActorCompanyId);

            return Content(HttpStatusCode.OK, reportUserSelection.ToDTO());
        }

        [HttpGet]
        [Route("Menu/Item/Selection/FromReportPrintout/{reportPrintoutId:int}")]
        public IHttpActionResult GetReportSelectionFromReportPrintout(int reportPrintoutId)
        {
            return Content(HttpStatusCode.OK, rm.GetReportSelectionFromReportPrintout(reportPrintoutId, base.ActorCompanyId, base.UserId));
        }

        [HttpPost]
        [Route("Menu/Item/Selection")]
        public IHttpActionResult SaveReportUserSelection(ReportUserSelectionDTO dto)
        {
            return Content(HttpStatusCode.Accepted, rm.SaveReportUserSelection(dto));
        }

        [HttpDelete]
        [Route("Menu/Item/Selection/{reportUserSelectionId:int}")]
        public IHttpActionResult DeleteReportUserSelection(int reportUserSelectionId)
        {
            return Content(HttpStatusCode.Accepted, rm.DeleteReportUserSelection(reportUserSelectionId));
        }

        #endregion

        #region Queue

        [HttpGet]
        [Route("Menu/Queue/{reportPrintoutIds}/{showDetails:bool}")]
        public IHttpActionResult GetReportGenerationQueue(string reportPrintoutIds, bool showDetails)
        {
            return Content(HttpStatusCode.OK, rm.GetReportGenerationQueue(base.UserId, base.ActorCompanyId, StringUtility.SplitNumericList(reportPrintoutIds, nullIfEmpty: true), showDetails));
        }

        [HttpGet]
        [Route("Menu/Queue/MatrixGrid/{reportPrintoutId}")]
        public IHttpActionResult GetMatrixGridResult(int reportPrintoutId)
        {
            return Content(HttpStatusCode.OK, rm.GetMatrixGridResult(base.UserId, base.ActorCompanyId, reportPrintoutId));
        }

        [HttpPost]
        [Route("Menu/Queue/Validate")]
        public IHttpActionResult ValidateReportGenerationJob(ReportJobDefinitionDTO job)
        {
            return Content(HttpStatusCode.Accepted, rdm.ValidateMigratedReportDTO(base.ActorCompanyId, base.UserId, job));
        }

        [HttpPost]
        [Route("Menu/Queue")]
        public IHttpActionResult CreateReportGenerationJob(ReportJobDefinitionDTO job)
        {
            return Content(HttpStatusCode.Accepted, rdm.PrintMigratedReportDTO(job, base.ActorCompanyId, base.UserId, base.RoleId));
        }

        [HttpPost]
        [Route("Menu/Queue/{reportPrintoutId:int}/{forceValidation:bool}")]
        public IHttpActionResult ReCreateReportGenerationJob(int reportPrintoutId, bool forceValidation)
        {
            return Content(HttpStatusCode.Accepted, rdm.RePrintMigratedReportDTO(reportPrintoutId, base.ActorCompanyId, base.UserId, base.RoleId, forceValidation));
        }

        [HttpPost]
        [Route("Menu/Queue/Insight")]
        public IHttpActionResult CreateInsight(ReportJobDefinitionDTO job)
        {
            return Content(HttpStatusCode.Accepted, rdm.CreateInsight(job, base.ActorCompanyId, base.UserId, base.RoleId));
        }

        [HttpDelete]
        [Route("Menu/Queue/{reportPrintoutId:int}")]
        public IHttpActionResult DeletePrintedReport(int reportPrintoutId)
        {
            return Content(HttpStatusCode.Accepted, rm.DeletePrintedReport(reportPrintoutId, base.ActorCompanyId, base.UserId));
        }

        #endregion

        #region Favorite

        [HttpPost]
        [Route("Favorite/{reportId:int}")]
        public IHttpActionResult SaveUserReportFavorite(int reportId)
        {
            return Content(HttpStatusCode.OK, rm.SaveUserReportFavorite(reportId, base.ActorCompanyId, base.UserId));
        }

        [HttpPost]
        [Route("Favorite/{reportId:int}/{name}")]
        public IHttpActionResult RenameUserReportFavorite(int reportId, string name)
        {
            return Content(HttpStatusCode.OK, rm.RenameUserReportFavorite(reportId, base.ActorCompanyId, base.UserId, name));
        }

        [HttpDelete]
        [Route("Favorite/{reportId:int}")]
        public IHttpActionResult DeleteUserReportFavorite(int reportId)
        {
            return Content(HttpStatusCode.OK, rm.DeleteUserReportFavorite(reportId, base.ActorCompanyId, base.UserId));
        }

        #endregion

        #region Insights

        [HttpGet]
        [Route("Data/Insights/{sysReportTemplateTypeId:int}/{module:int}")]
        public IHttpActionResult GetInsights(int sysReportTemplateTypeId, int module)
        {
            return Content(HttpStatusCode.OK, rm.GetInsights(sysReportTemplateTypeId, (SoeModule)module, base.ActorCompanyId, base.RoleId));
        }

        #endregion

        #region Data

        [HttpGet]
        [Route("Data/EmployeeCategories")]
        public IHttpActionResult GetEmployeeCategories()
        {
            int employeeId = em.GetEmployeeIdForUser(base.UserId, base.ActorCompanyId);
            return Content(HttpStatusCode.OK, cm.GetCategoriesForRoleFromTypeDict(base.ActorCompanyId, base.UserId, employeeId, SoeCategoryType.Employee, true, true, false).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Data/GroupsAndSorts")]
        public IHttpActionResult GetGroupsAndSorts(HttpRequestMessage message)
        {
            return Content(HttpStatusCode.OK, rm.GetSortingAndGroupingForReport(base.ActorCompanyId, message.GetIntValueFromQS("reportId"), (Feature)message.GetIntValueFromQS("feature")).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Data/MatrixLayoutColumns/{sysReportTemplateTypeId:int}/{module:int}")]
        public IHttpActionResult GetMatrixLayoutColumns(int sysReportTemplateTypeId, int module)
        {
            return Content(HttpStatusCode.OK, rm.GetMatrixLayoutColumns(sysReportTemplateTypeId, base.ActorCompanyId, (SoeModule)module));
        }

        [HttpGet]
        [Route("Data/PayrollTypes")]
        public IHttpActionResult GetPayrollTypes()
        {
            return Content(HttpStatusCode.OK, rm.GetReportPayrollTypes(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Data/Payroll/Months")]
        public IHttpActionResult GetPayrollMonths()
        {
            return Content(HttpStatusCode.OK, rm.GetReportPayrollMonths(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Data/Payroll/Years")]
        public IHttpActionResult GetPayrollYears()
        {
            return Content(HttpStatusCode.OK, rm.GetReportPayrollYears(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Data/ScheduledJobHeads")]
        public IHttpActionResult GetScheduledJobHeads()
        {
            return Content(HttpStatusCode.OK, sjm.GetScheduledJobHeads(base.ActorCompanyId).ToDictionary(x => x.ScheduledJobHeadId, y => y.Name).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Data/ShiftTypes")]
        public IHttpActionResult GetShiftTypes()
        {
            int employeeId = em.GetEmployeeIdForUser(base.UserId, base.ActorCompanyId);
            return Content(HttpStatusCode.OK, tsm.GetShiftTypesDictForUser(null, base.ActorCompanyId, base.RoleId, base.UserId, employeeId, true).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Data/SysTimeIntervals")]
        public IHttpActionResult GetSysTimeIntervals(HttpRequestMessage message)
        {
            return Content(HttpStatusCode.OK, calm.GetSysTimeIntervalsDict().ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Data/SysTimeIntervals/DateRange")]
        public IHttpActionResult GetSysTimeIntervalDateRange(HttpRequestMessage message)
        {
            return Content(HttpStatusCode.OK, calm.GetSysTimeIntervalDateRange(message.GetIntValueFromQS("sysTimeIntervalId")));
        }

        [HttpGet]
        [Route("Data/TimePeriods/Payroll")]
        public IHttpActionResult GetPayrollTimePeriods(HttpRequestMessage message)
        {
            return Content(HttpStatusCode.OK, rm.GetReportPayrollTimePeriods(base.ActorCompanyId).OrderByDescending(t => t.PaymentDate));
        }

        [HttpGet]
        [Route("Data/BudgetHead")]
        public IHttpActionResult GetBudgetHeadDist(HttpRequestMessage message)
        {
            return Content(HttpStatusCode.OK, bm.GetBudgetHeadsDist(base.ActorCompanyId, (int)DistributionCodeBudgetType.AccountingBudget));
        }

        [HttpPost]
        [Route("Data/Employees")]
        public IHttpActionResult GetFilteredEmployees(GetFilteredEmployeesModel model)
        {
            DateTime from = CalendarUtility.GetBeginningOfDay(model.FromDate ?? DateTime.Now.Date);
            DateTime to = CalendarUtility.GetEndOfDay(model.ToDate ?? from);

            return Content(HttpStatusCode.OK, this.em.GetEmployeesByFilter(base.ActorCompanyId, base.UserId, base.RoleId, from, to, model.TimePeriodIds, model.SoeReportTemplateType, model.AccountingType, model.AccountIds, model.CategoryIds, model.EmployeeGroupIds, model.PayrollGroupIds, model.VacationGroupIds, model.IncludeInactive, model.OnlyInactive, model.IncludeEnded, model.IncludeHidden, model.IncludeVacant, doValidateEmployment: true, model.IncludeSecondary).ToDict(false, true, true).ToSmallGenericTypes());
        }

        #endregion  

        #endregion  

        #region ReportUrl

        [HttpGet]
        [Route("PayrollSlipURL/{employeeId:int}/{timePeriodId:int}/{reportId:int}")]
        public IHttpActionResult GetPayrollSlipURL(int employeeId, int timePeriodId, int reportId)
        {
            return Content(HttpStatusCode.OK, rm.GetPayrollSlipReportPrintUrl(base.ActorCompanyId, timePeriodId, employeeId, reportId));
        }

        [HttpGet]
        [Route("TimeMonthlyReportUrl/{employeeId:int}/{startDate}/{stopDate}/{reportId:int}")]
        public IHttpActionResult GetTimeMonthlyReportUrl(int employeeId, string startDate, string stopDate, int reportId)
        {
            return Content(HttpStatusCode.OK, rm.GetTimeMonthlyReportPrintUrl(base.ActorCompanyId, BuildDateTimeFromString(startDate, true).Value, BuildDateTimeFromString(stopDate, true).Value, employeeId, reportId));
        }

        [HttpPost]
        [Route("TimeMonthlyReportUrl/")]
        public IHttpActionResult GetTimeMonthlyReportUrl(GetTimeEmployeeSchedulePrintUrlModel model)
        {
            return Content(HttpStatusCode.OK, rm.GetTimeMonthlyReportPrintUrl(model.EmployeeIds, model.DateFrom, model.DateTo, model.ReportId, model.ReportTemplateType));
        }

        [HttpPost]
        [Route("SaveReportUrl/")]
        public IHttpActionResult SaveReportUrl(SaveReportUrlModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, rm.SaveReportUrl(model.Guid, model.Url, model.ReportId, model.SysReportTemplateTypeId, base.ActorCompanyId));
        }

        #endregion

        #region AltInn

        [HttpPost]
        [Route("AltInn/Login")]
        public IHttpActionResult Login(AltInnUser user)
        {
            var result = new ActionResult();
            var altInn = new SoftOne.Soe.Business.Util.AltInn.AltInn();
            var response = altInn.GetAuthenticationChallenge(user);
            result.Success = response.GetAuthenticationChallengeResult.Status == SoftOne.Soe.Business.Altinn.SystemAuthentication.ChallengeRequestResult.Ok;
            if (result.Success)
            {
                result.DateTimeValue = response.GetAuthenticationChallengeResult.ValidTo;
                result.StringValue = response.GetAuthenticationChallengeResult.Message;
            }
            else
            {
                result.ErrorMessage = response.GetAuthenticationChallengeResult.Status.ToString() + ": " + response.GetAuthenticationChallengeResult.Message;
            }
            return Ok(result);
        }

        #endregion

        #region eDistribution
        [HttpGet]
        [Route("EDistributionItems/{originType:int}/{type:int}/{allItemsSelection:int}")]
        public IHttpActionResult EDistributionItems(int originType, int type, int allItemsSelection)
        {
            return Content(HttpStatusCode.OK, idm.GetDistributionItems(base.ActorCompanyId, (SoeOriginType)originType,(TermGroup_EDistributionType)type, (TermGroup_GridDateSelectionType)allItemsSelection));
        }
        #endregion

        #region SalesEU

        [HttpGet]
        [Route("SalesEU/{startDate}/{stopDate}")]
        public IHttpActionResult SalesEU(string startDate, string stopDate)
        {
            var im = new EUSalesManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, im.GetSales(base.ActorCompanyId, BuildDateTimeFromString(startDate, true).Value,
                                                                        BuildDateTimeFromString(stopDate, true).Value));
        }
        [HttpGet]
        [Route("SalesEUDetails/{actorId}/{startDate}/{stopDate}")]
        public IHttpActionResult SalesEUDetails(int actorId, string startDate, string stopDate)
        {
            var im = new EUSalesManager(this.ParameterObject);
            return Content(HttpStatusCode.OK, im.GetSalesDetails(actorId, BuildDateTimeFromString(startDate, true).Value,
                                                                        BuildDateTimeFromString(stopDate, true).Value));
        }
        [HttpGet]
        [Route("SalesEUExportFile/{periodtype}/{startDate}/{stopDate}")]
        public HttpResponseMessage SalesEUExportFile(int periodType, string startDate, string stopDate)
        {
            var im = new EUSalesManager(this.ParameterObject);
            HttpContext.Current.Response.ContentEncoding = Encoding.ASCII;

            var file = im.GetExportFile(base.ActorCompanyId, (DatePeriodType)periodType, BuildDateTimeFromString(startDate, true).Value, BuildDateTimeFromString(stopDate, true).Value);

            var response = new HttpResponseMessage(HttpStatusCode.Accepted);
            if (file.Success)
            {
                response.Content = new ByteArrayContent(Encoding.ASCII.GetBytes((string)file.Value));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain") { CharSet = "ascii" };
            }
            else
            {
                response.Content = new StringContent(file.ErrorMessage);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain") { CharSet = "utf8" };
            }

            return response;
        }

        #endregion
    }
}
