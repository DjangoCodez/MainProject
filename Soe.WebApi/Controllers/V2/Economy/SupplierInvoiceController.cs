using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Http;
using Soe.WebApi.Controllers;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;

namespace Soe.WebApi.V2.Economy
{
    [RoutePrefix("V2/Economy/SupplierInvoice")]
    public class SupplierInvoiceController : SoeApiController
    {
        #region Variables

        private readonly SupplierManager sm;
        private readonly SupplierInvoiceManager sim;
        private readonly GeneralManager gm;
        private readonly AccountManager acm;
        private readonly InvoiceManager im;
        private readonly EdiManager edim;
        private readonly ProjectManager pm;
        private readonly EmployeeManager em;
        private readonly TimeCodeManager tcm;
        private readonly AttestManager am;
        private readonly OriginManager om;
        private readonly ReportDataManager rm;

        #endregion

        #region Constructor
        public SupplierInvoiceController(SupplierManager sm, SupplierInvoiceManager sim, GeneralManager gm, AccountManager acm, InvoiceManager im, EdiManager edim, ProjectManager pm, EmployeeManager em, TimeCodeManager tcm, AttestManager am, OriginManager om, ReportDataManager rm)
        {
            this.sm = sm;
            this.sim = sim;
            this.gm = gm;
            this.acm = acm;
            this.im = im;
            this.edim = edim;
            this.pm = pm;
            this.em = em;
            this.tcm = tcm;
            this.am = am;
            this.om = om;
            this.rm = rm;
        }

        #endregion


        [HttpGet]
        [Route("File/{fileId:int}")]
        public IHttpActionResult GetFile(int fileId)
        {
            //TODO: This is a temporary implementation while we wait for the file-remake.

            var record = gm.GetDataStorageRecord(base.ActorCompanyId, fileId);
            if (record != null && record.DataStorage != null && record.DataStorage.Data != null)
                return Content(HttpStatusCode.OK, new { data = System.Convert.ToBase64String(record.DataStorage.Data) });

            return Content(HttpStatusCode.OK, new { });
        }

        [HttpGet]
        [Route("Invoice/ImageByFileId/{fileId:int}")]
        public IHttpActionResult GetSupplierInvoiceImageByFileId(int fileId)
        {
            return Ok(sim.GetSupplierInvoiceImageByFileId(ActorCompanyId, fileId));
        }

        [HttpGet]
        [Route("Invoice/SupplierInvoiceImage/{invoiceId:int}")]
        public IHttpActionResult GetSupplierInvoiceImage(int invoiceId)
        {
            return Ok(sim.GetSupplierInvoiceImage(ActorCompanyId, invoiceId, true));
        }

        [HttpGet]
        [Route("Invoice/SupplierInvoiceImage/edi/{ediEntryId:int}")]
        public IHttpActionResult GetSupplierInvoiceImageFromEdi(int ediEntryId)
        {
            return Ok(edim.GetInvoiceImageFromEdi(ediEntryId, ActorCompanyId));
        }

        [HttpGet]
        [Route("Invoice/{loadOpen:bool}/{loadClosed:bool}/{onlyMine:bool}/{allItemsSelection:int}/{projectId:int}/{includeChildProjects:bool}")]
        public IHttpActionResult GetInvoices(bool loadOpen, bool loadClosed, bool onlyMine, TermGroup_ChangeStatusGridAllItemsSelection allItemsSelection, int projectId, bool includeChildProjects)
        {
            return Content(HttpStatusCode.OK, sim.GetSupplierInvoicesForGrid(loadOpen, loadClosed, allItemsSelection, projectId: projectId, includeChildProjects: includeChildProjects));
        }

        [HttpGet]
        [Route("Invoice/{loadOpen:bool}/{loadClosed:bool}/{onlyMine:bool}/{allItemsSelection:int}/{supplierId:int}")]
        public IHttpActionResult GetInvoicesForSupplier(bool loadOpen, bool loadClosed, bool onlyMine, TermGroup_ChangeStatusGridAllItemsSelection allItemsSelection, int supplierId)
        {
            return Content(HttpStatusCode.OK, sim.GetSupplierInvoicesForGrid(loadOpen, loadClosed, allItemsSelection, supplierId, true));
        }

        [HttpPost]
        [Route("Invoice/ProjectCentral/")]
        public IHttpActionResult GetInvoicesForProjectCentral(InvoicesForProjectCentralModel model)
        {
            return Content(HttpStatusCode.OK, sim.GetSupplierInvoicesGridForProjectCentral(this.ActorCompanyId, model.ProjectId, model.LoadChildProjects, model.FromDate, model.ToDate));
        }

        [HttpPost]
        [Route("Invoice/Filtered/")]
        public IHttpActionResult GetFilteredSupplierInvoices(ExpandoObject filterModels)
        {
            return Content(HttpStatusCode.OK, sim.GetFilteredSupplierInvoices(filterModels));
        }

        [HttpGet]
        [Route("Invoice/{invoiceId:int}/{loadProjectRows:bool}/{loadOrderRows:bool}/{loadProject:bool}/{loadImage:bool}")]
        public IHttpActionResult GetInvoice(int invoiceId, bool loadProjectRows, bool loadOrderRows, bool loadProject, bool loadImage)
        {
            //List<AccountDim> dims = acm.GetAccountDimsByCompany(ActorCompanyId);
            List<AccountDim> dims = null; 
            var image = loadImage ? sim.GetSupplierInvoiceImage(ActorCompanyId, invoiceId, true) : null;
            return Content(HttpStatusCode.OK, sim.GetSupplierInvoice(invoiceId, true, true, true, false, true, true, true, true, loadProjectRows, loadOrderRows, loadProject).ToSupplierInvoiceDTO(true, true, loadProjectRows, loadOrderRows, dims, image));
        }

        [HttpGet]
        [Route("Invoice/GetInvoiceTraceViews/{invoiceId:int}")]
        public IHttpActionResult GetInvoiceTraceViews(int invoiceId)
        {
            CountryCurrencyManager ccm = new CountryCurrencyManager(null);
            int baseSysCurrencyId = ccm.GetCompanyBaseSysCurrencyId(base.ActorCompanyId);

            return Content(HttpStatusCode.OK, im.GetInvoiceTraceViews(invoiceId, baseSysCurrencyId));
        }

        [HttpGet]
        [Route("Invoice/BlockPayment")]
        public IHttpActionResult GetSupplierInvoiceText(InvoiceTextType type, int? invoiceId, int? ediEntryId)
        {
            return Content(HttpStatusCode.OK, sim.GetSupplierInvoiceText(this.ActorCompanyId, invoiceId: invoiceId, ediEntryId: ediEntryId, type).ToDTO());
        }

		[HttpPost]
        [Route("Invoice/BlockPayment/")]
        public IHttpActionResult BlockSupplierInvoicePayment(BlockPaymentModel model)
        {
            return Content(HttpStatusCode.OK, sim.SupplierInvoiceSaveInvoiceTextAction(model.InvoiceId, InvoiceTextType.SupplierInvoiceBlockReason, model.Block, model.Reason));
        }

		[HttpPost]
        [Route("Invoice/InvoiceTextAction/")]
        public IHttpActionResult InvoiceTextAction(InvoiceTextActionModel model)
        {
            if (model.InvoiceId > 0)
                return Content(HttpStatusCode.OK, sim.SupplierInvoiceSaveInvoiceTextAction(model.InvoiceId.Value, model.Type, model.ApplyAction, model.Reason));
            else if (model.EdiEntryId > 0)
                return Content(HttpStatusCode.OK, edim.EdiEntrySaveInvoiceTextAction(model.EdiEntryId.Value, model.Type, model.ApplyAction, model.Reason));
            else
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
        }

        [HttpPost]
        [Route("Invoice/SupplierInvoiceNrAlreadyExist")]
        public IHttpActionResult SupplierInvoiceNrAlreadyExist(SupplierInvoiceNrAlreadyExistModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sim.SupplierInvoiceNumberExist(model.ActorId, model.InvoiceId, model.InvoiceNr));
        }

        [HttpGet]
        [Route("GetOrder/{invoiceId:int}/{includeRows:bool}")]
        public IHttpActionResult GetOrder(int invoiceId, bool includeRows)
        {
            return Content(HttpStatusCode.OK, im.GetOrder(invoiceId, false, includeRows, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("GetOrder/{orderNr}")]
        public IHttpActionResult GetOrderForSupplier(string orderNr)
        {
            return Content(HttpStatusCode.OK, im.GetOrder(this.ActorCompanyId, orderNr));
        }

        [HttpGet]
        [Route("GetOrdersForSupplierInvoiceEdit/")]
        public IHttpActionResult GetOrdersForSupplierInvoiceEdit()
        {
            return Content(HttpStatusCode.OK, im.GetOrdersForSupplierInvoiceEdit());
        }

        [HttpGet]
        [Route("Project/{type:int}/")]
        public IHttpActionResult GetProjectsList(TermGroup_ProjectType type, bool? active = null, bool? getHidden = null, bool? getFinished = null)
        {
            var projects = pm.GetProjectsList(base.ActorCompanyId, type, active, getHidden, getFinished).ToTinyDTOs();
            return Content(HttpStatusCode.OK, projects);
        }

        [HttpGet]
        [Route("Employees/{active}/{addRoleInfo}/{addCategoryInfo}/{addEmployeeGroupInfo}/{addProjectDefaultTimeCode}/{addEmployments}/{useShowOtherEmployeesPermission}/{getHidden}/{loadFactors}/{addPayrollGroupInfo}/{loadEmployeeVacation}")]
        public IHttpActionResult GetEmployees(bool active, bool addRoleInfo, bool addCategoryInfo, bool addEmployeeGroupInfo, bool addProjectDefaultTimeCode, bool addEmployments, bool useShowOtherEmployeesPermission, bool getHidden, bool loadFactors, bool addPayrollGroupInfo, bool loadEmployeeVacation)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeesForUsersAttestRoles(out _, base.ActorCompanyId, base.UserId, base.RoleId, active: active, getHidden: getHidden, useShowOtherEmployeesPermission: useShowOtherEmployeesPermission, addEmployeeAuthModelInfo: addCategoryInfo, addRoleInfo: addRoleInfo, addEmployeeGroupInfo: addEmployeeGroupInfo, addPayrollGroupInfo: addPayrollGroupInfo, addProjectDefaultTimeCode: addProjectDefaultTimeCode).ToDTOs(includeEmployments: addEmployments, includeEmployeeGroup: addEmployeeGroupInfo, includePayrollGroup: addPayrollGroupInfo, includeFactors: loadFactors, includeVacationGroup: loadEmployeeVacation));
        }

        [HttpGet]
        [Route("TimeCodes/{timeCodeType}/{active}/{loadPayrollProducts}/")]
        public IHttpActionResult GetTimeCodes(SoeTimeCodeType timeCodeType, bool active, bool loadPayrollProducts)
        {
            return Content(HttpStatusCode.OK, tcm.GetTimeCodes(base.ActorCompanyId, timeCodeType, active, loadPayrollProducts).ToDTOs(loadPayrollProducts));
        }

        [HttpGet]
        [Route("SupplierInvoiceProjectTransactions/{invoiceId}/")]
        public List<SupplierInvoiceProjectRowDTO> GetSupplierInvoiceProjectTransactions(int invoiceId)
        {
            return pm.GetSupplierInvoiceProjectRows(invoiceId, base.ActorCompanyId, loadOrders: true);
        }

        [HttpGet]
        [Route("Invoice/AccountingRows/{invoiceId:int}")]
        public IHttpActionResult GetSupplierInvoiceAccountingRows(int invoiceId)
        {
            return Ok(sim.GetSupplierInvoiceAccountRows(invoiceId).ToAccountingRowDTO());
        }

        [HttpGet]
        [Route("Invoice/CostOverview/{notLinked:bool}/{partiallyLinked:bool}/{linked:bool}/{allItemsSelection:int}")]
        public IHttpActionResult GetSupplierInvoicesCostOverview(bool notLinked, bool partiallyLinked, bool linked, TermGroup_ChangeStatusGridAllItemsSelection allItemsSelection)
        {
            return Ok(sim.GetSupplierInvoiceCostOverView(notLinked, partiallyLinked, linked, allItemsSelection, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Invoice/OrderRows/{invoiceId:int}")]
        public IHttpActionResult GetSupplierInvoiceOrderRows(int invoiceId)
        {
            return Ok(sim.GetSupplierInvoiceOrderRows(base.ActorCompanyId, invoiceId));
        }

        [HttpGet]
        [Route("Invoice/ProjectRows/{invoiceId:int}")]
        public IHttpActionResult GetSupplierInvoiceProjectRows(int invoiceId)
        {
            return Ok(pm.GetSupplierInvoiceProjectRows(invoiceId, base.ActorCompanyId, loadOrders: true));
        }

        [HttpGet]
        [Route("Invoice/OrderProjectRows/{invoiceId:int}")]
        public IHttpActionResult GetSupplierInvoiceCostAllocationRows(int invoiceId)
        {
            return Ok(sim.GetSupplierInvoiceCostAllocationRows(base.ActorCompanyId, invoiceId));
        }

        [HttpPost]
        [Route("Invoice/OrderProjectRows/")]
        public IHttpActionResult SaveSupplierInvoiceCostAllocationRows(SupplierInvoiceCostAllocationModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sim.SaveSupplierInvoiceCostAllocationRows(model.CostAllocationRows, model.InvoiceId, model.ProjectId, model.CustomerInvoiceId, model.OrderSeqNr, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("SupplierCentralCountersAndBalance/")]
        public List<ChangeStatusGridViewBalanceDTO> GetSupplierCentralCountersAndBalance(GetSupplierCentralCountersAndBalanceModel model)
        {
            return im.GetChangeStatusGridViewsCountersAndBalanceForSupplierCentral(model.CounterTypes, model.SupplierId, base.ActorCompanyId);
        }

        [HttpPost]
        [Route("Invoice/")]
        public IHttpActionResult SaveInvoice(SaveSupplierInvoiceModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sim.SaveSupplierInvoice(model.Invoice, model.PurchaseInvoiceRows, model.AccountingRows, model.ProjectRows, model.OrderRows, model.CostAllocationRows, base.ActorCompanyId, base.RoleId, model.CreateAttestVoucher, false, model.SkipInvoiceNrCheck));
        }

        [HttpDelete]
        [Route("Invoice/{invoiceId:int}/{deleteProject:bool}")]
        public IHttpActionResult DeleteInvoice(int invoiceId, bool deleteProject)
        {

            return Ok(im.DeleteInvoice(invoiceId, base.ActorCompanyId, deleteProject));
        }

        [HttpDelete]
        [Route("DeleteDraftInvoices/{invoices}")]
        public IHttpActionResult DeleteInvoices(string invoices)
        {
            List<int> invoiceIds = StringUtility.SplitNumericList(invoices, true, false);
            return Ok(im.DeleteInvoices(invoiceIds, base.ActorCompanyId, false));
        }

        [HttpPost]
        [Route("Invoice/AttestAccountingRows/{invoiceId:int}")]
        public IHttpActionResult SaveSupplierInvoiceAttestAccountingRows(int invoiceId, List<AccountingRowDTO> accountingRows)
        {
            if (ModelState.IsValid)
                return Ok(sim.SaveSupplierInvoiceAttestRows(invoiceId, accountingRows, ActorCompanyId));
            return BadRequest(ModelState);
        }

        [HttpPost]
        [Route("Invoice/AccountingRows/{invoiceId:int}")]
        public IHttpActionResult SaveSupplierInvoiceAccountingRows(int invoiceId, SaveSupplierInvoiceAccountingRows model)
        {
            if (ModelState.IsValid)
                return Ok(sim.SaveSupplierInvoiceAccountingRows(invoiceId, model.accountingRows, model.currentDimIds, ActorCompanyId));
            return BadRequest(ModelState);
        }

        [HttpPost]
        [Route("AddScanningEntrys/{ediSourceType:int}")]
        public IHttpActionResult AddScanningEntrys(int ediSourceType)
        {
            if (ediSourceType == (int)TermGroup_EDISourceType.EDI)
                return Content(HttpStatusCode.OK, edim.AddEdiEntrys((TermGroup_EDISourceType)ediSourceType, false));
            else
                return Content(HttpStatusCode.OK, edim.AddScanningEntrys((TermGroup_EDISourceType)ediSourceType, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("TransferInvoicesToDefinitive/")]
        public IHttpActionResult TransferSupplierInvoicesToDefinitive(ListIntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, om.UpdateInvoiceOriginStatusFromAngular(model.Numbers, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("TransferInvoicesToVoucher/")]
        public IHttpActionResult TransferSupplierInvoicesToVouchers(TransferModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else if (string.IsNullOrEmpty(model.Guid))
            {
                return Content(HttpStatusCode.OK, sim.TransferSupplierInvoicesToVoucherFromAngular(model.IdsToTransfer, base.ActorCompanyId));
            }
            else
            {
                int actorCompanyId = base.ActorCompanyId;
                Guid guid = Guid.Parse(model.Guid);
                CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                var workingThread = new Thread(() => TransferSupplierInvoicesToVouchersUsingPolling(cultureInfo, guid, model.IdsToTransfer, actorCompanyId));
                workingThread.Start();
                return Content(HttpStatusCode.OK, new SoeProgressInfo(guid));
            }
        }

        [HttpGet]
        [Route("TransferInvoicesToVoucher/{key}")]
        public IHttpActionResult TransferSupplierInvoicesToVouchersResult(Guid key)
        {
            var result = monitor.GetResult(key);
            return Content(HttpStatusCode.OK, result.Any() ? result.FirstOrDefault() as ActionResult : new ActionResult(false));
        }

        private void TransferSupplierInvoicesToVouchersUsingPolling(CultureInfo cultureInfo, Guid key, List<int> idsToTransfer, int actorCompanyId)
        {
            SetLanguage(cultureInfo);

            SoeProgressInfo info = monitor.RegisterNewProgressProcess(key);
            sim.TransferSupplierInvoicesToVoucherFromAngularWithPolling(idsToTransfer, actorCompanyId, ref info, monitor);
        }

        [HttpPost]
        [Route("Invoice/HideUnhandled/")]
        public IHttpActionResult HideUnhandled(HideUnhandledInvoicesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, am.HideUnattestedInvoices(base.ActorCompanyId, model.InvoiceIds));
        }

        [HttpPost]
        [Route("TransferEdiToInvoices/")]
        public IHttpActionResult TransferEdiToInvoices(ListIntModel itemsToTransfer)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, edim.TransferToSupplierInvoicesFromEdiDict(itemsToTransfer.Numbers, base.ActorCompanyId, base.UserId));
        }

        [HttpPost]
        [Route("TransferEdiToOrder/")]
        public IHttpActionResult TransferEdiToOrder(ListIntModel itemsToTransfer)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, edim.TransferToOrdersFromEdi(itemsToTransfer.Numbers, base.ActorCompanyId, false));
        }

        [HttpPost]
        [Route("TransferEdiState/")]
        public IHttpActionResult TransferEdiState(TransferEdiStateModel model)
        {
            return Content(HttpStatusCode.OK, edim.ChangeEdiEntriesState(model.IdsToTransfer, model.StateTo, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("GetEdiEntry/{ediEntryId:int}/{loadSuppliers:bool}")]
        public IHttpActionResult GetEdiEntry(int ediEntryId, bool loadSuppliers)
        {
            return Content(HttpStatusCode.OK, edim.GetEdiEntry(ediEntryId, base.ActorCompanyId, true, loadSuppliers).ToDTO(false, false));
        }

        [HttpPost]
        [Route("UpdateEdiEntries/")]
        public IHttpActionResult TransferEdiState(List<UpdateEdiEntryDTO> ediEntries)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, edim.UpdateEdiEntrys(ediEntries, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("GenerateReportForEdi/")]
        public IHttpActionResult GenerateReportForEdi(List<int> ediEntryIds)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, rm.GenerateReportForEdi(ediEntryIds, ActorCompanyId));
        }

        [HttpPost]
        [Route("GenerateReportForFinvoice/")]
        public IHttpActionResult GenerateReportForFinvoice(List<int> ediEntryIds)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, rm.GenerateReportForFinvoice(ediEntryIds, ActorCompanyId));
        }

        [HttpGet]
        [Route("GetEdiEntryFromInvoice/{invoiceId:int}")]
        public EdiEntryDTO GetEdiEntryFromInvoice(int invoiceId)
        {
            return edim.GetEdiEntryFromInvoice(invoiceId, true).ToDTO(false);
        }

        [HttpPost]
        [Route("SaveInvoicesForImages/")]
        public IHttpActionResult SaveSupplierInvoicesForUploadedImages(List<int> dataStorageIds)
        {
            return Content(HttpStatusCode.OK, sim.SaveSupplierInvoicesForUploadedImages(dataStorageIds, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Invoice/SupplierInvoiceChangeAttestGroup/{invoiceId:int}/{attestGroupId:int}")]
        public IHttpActionResult SaveSupplierInvoiceAttestGroup(int invoiceId, int attestGroupId)
        {
            return Content(HttpStatusCode.OK, sim.SaveSupplierInvoiceChangedAttestGroupId(invoiceId, attestGroupId));
        }

        [HttpPost]
        [Route("SaveSupplierFromFinvoice/{ediEntryId:int}")]
        public IHttpActionResult SaveSupplierFromFinvoice(int ediEntryId)
        {
            return Content(HttpStatusCode.OK, edim.CreateSupplierFromFinvoice(ediEntryId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Invoice/ProductRows/Transfer")]
        public IHttpActionResult TransferSupplierProductRows(TransferSupplierInvoiceRowsToOrderModel model)
        {
            return Content(HttpStatusCode.OK, sim.TransferSupplierInvoiceProductRows(base.ActorCompanyId, model.CustomerInvoiceId, model.SupplierInvoiceId, model.SupplierInvoiceProductRowIds, model.WholesellerId));
        }

        [HttpGet]
        [Route("Invoice/ProductRows/{invoiceId:int}")]
        public IHttpActionResult GetSupplierInvoiceProductRows(int invoiceId)
        {
            return Content(HttpStatusCode.OK, sim.GetSupplierInvoiceProductRows(base.ActorCompanyId, invoiceId));
        }

        [HttpPost]
        [Route("Invoice/TransferToOrder")]
        public IHttpActionResult TransferSupplierInvoicesToOrder(TransferSupplierInvoicesToOrderModel model)
        {
            return Content(HttpStatusCode.OK, sim.TransferSupplierInvoicesToOrder(base.ActorCompanyId, model.Items, model.TransferSupplierInvoiceRows, model.UseMiscProduct));
        }


        #region Scanning

        [HttpGet]
        [Route("Scanning/Interpretation/{ediEntryId:int}")]
        public IHttpActionResult GetScanningInterpretation(int ediEntryId)
        {
            return Content(HttpStatusCode.OK, edim.GetSupplierInvoiceInterpretationDTO(base.ActorCompanyId, ediEntryId));
        }


        #endregion


    }
}