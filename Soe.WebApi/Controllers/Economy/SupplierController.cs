using Soe.WebApi.Binders;
using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Soe.WebApi.Controllers.Economy
{
    [RoutePrefix("Economy/Supplier")]
    public class SupplierController : SoeApiController
    {
        #region Variables

        private readonly AttestManager am;
        private readonly InvoiceManager im;
        private readonly OriginManager om;
        private readonly SupplierManager sm;
        private readonly EmployeeManager em;
        private readonly TimeCodeManager tcm;
        private readonly ProjectManager pm;
        private readonly GeneralManager gm;
        private readonly EdiManager edm;
        private readonly PaymentManager pam;
        private readonly EdiManager edim;
        private readonly ReportDataManager rm;
        private readonly AccountManager acm;
        private readonly SupplierInvoiceManager sim;
        private readonly PurchaseManager pum;

        #endregion

        #region Constructor

        public SupplierController(AttestManager am, InvoiceManager im, OriginManager om, SupplierManager sm, EmployeeManager em, TimeCodeManager tcm, ProjectManager pm, GeneralManager gm, EdiManager edm, PaymentManager pam, EdiManager edim, ReportDataManager rm, AccountManager acm, PurchaseManager pum, SupplierInvoiceManager sim)
        {
            this.am = am;
            this.im = im;
            this.om = om;
            this.sm = sm;
            this.em = em;
            this.tcm = tcm;
            this.pm = pm;
            this.edm = edm;
            this.gm = gm;
            this.edm = edm;
            this.pam = pam;
            this.edim = edim;
            this.rm = rm;
            this.acm = acm;
            this.sim = sim;
            this.pum = pum;
        }

        #endregion

        #region Attest work flow

        #region Answer

        [HttpPost]
        [Route("AttestWorkFlow/SaveAnswerToAttestFlowRow/")]
        public IHttpActionResult SaveAnswerToAttestFlowRow(SaveAnswersToAttestFlowRowModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, am.SaveAttestWorkFlowRowAnswer(model.RowId, model.Comment, model.Answer, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("AttestWorkFlow/SaveAnswersToAttestFlow/")]
        public IHttpActionResult SaveAnswerToAttestFlow(SaveAnswersToAttestFlowModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, am.SaveAttestWorkFlowRowAnswers(model.InvoiceIds, model.Comment, model.Answer, base.ActorCompanyId, base.RoleId, model.Attachments));
        }

        #endregion

        #region Attest work replace user

        [HttpPost]
        [Route("AttestWorkFlow/ReplaceAttestWorkFlowUser/")]
        public IHttpActionResult ReplaceAttestWorkFlowUser(ReplaceAttestWorkFlowUserModel model)
        {
            return Content(HttpStatusCode.OK, am.ReplaceAttestWorkFlowUser(model.Reason, model.DeletedWorkFlowRowId, model.Comment, model.ReplacementUserId, base.ActorCompanyId, model.InvoiceId, model.SendMail, true));
        }

        #endregion

        #region AttestGroup

        [HttpGet]
        [Route("AttestWorkFlow/AttestGroup/")]
        public IHttpActionResult GetAttestWorkFlowGroups(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, am.GetAttestWorkFlowGroups(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, am.GetAttestWorkFlowGroups(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("AttestWorkFlow/AttestGroup/ById/{id:int}")]
        public IHttpActionResult GetAttestWorkFlowGroup(int id)
        {
            return Content(HttpStatusCode.OK, am.GetAttestWorkFlowGroup(id, base.ActorCompanyId, true).ToAttestGroupDTO(false, false));
        }

        [HttpPost]
        [Route("AttestWorkFlow/AttestGroup/Suggestion")]
        public IHttpActionResult getAttestGroupSuggestion(GetAttestGroupSuggestion model)
        {
            return Content(HttpStatusCode.OK, am.GetAttestGroupSuggestion(base.ActorCompanyId, model.SupplierId, model.ProjectId, model.CostplaceAccountId, model.ReferenceOur).ToAttestGroupDTO(false, false));
        }

        [HttpPost]
        [Route("AttestWorkFlow/AttestGroup/SaveAttestWorkFlow")]
        public IHttpActionResult SaveAttestWorkFlow(AttestGroupDTO head)
        {
            return Content(HttpStatusCode.OK, am.SaveAttestWorkFlow(head, head.Rows, head.SendMessage, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("AttestWorkFlow/AttestGroup/SaveAttestWorkFlowMultiple")]
        public IHttpActionResult SaveAttestWorkFlowMultiple(SaveAttestWorkFlowForMultipleInvoicesModel model)
        {
            return Content(HttpStatusCode.OK, am.SaveAttestWorkFlowForMultipleInvoices(model.AttestWorkFlowHead, model.InvoiceIds, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("AttestWorkFlow/AttestGroup/SaveAttestWorkFlowForInvoices")]
        public IHttpActionResult SaveAttestWorkFlowForInvoices(SaveAttestWorkFlowForInvoicesModel model)
        {
            return Content(HttpStatusCode.OK, am.SaveAttestWorkFlowForInvoices(model.IdsToTransfer, ActorCompanyId, model.SendMessage));
        }

        [HttpDelete]
        [Route("AttestWorkFlow/AttestGroup/DeleteAttestWorkFlow/{attestWorkFlowHeadId:int}")]
        public IHttpActionResult DeleteAttestWorkFlow(int attestWorkFlowHeadId)
        {
            return Content(HttpStatusCode.OK, am.DeleteAttestWorkFlowHead(attestWorkFlowHeadId));
        }

        [HttpDelete]
        [Route("AttestWorkFlow/AttestGroup/DeleteAttestWorkFlows/{attestWorkFlowHeadIds}")]
        public IHttpActionResult DeleteAttestWorkFlows(string attestWorkFlowHeadIds)
        {
            List<int> ids = StringUtility.SplitNumericList(attestWorkFlowHeadIds, true, false);
            return Content(HttpStatusCode.OK, am.DeleteAttestWorkFlowHeads(ids));
        }


        #endregion

        #region TemplateHeads

        [HttpGet]
        [Route("AttestWorkFlow/TemplateHeads/ForCurrentCompany/{entity:int}")]
        public IHttpActionResult GetAttestWorkFlowTemplateHeadsForCompany(TermGroup_AttestEntity entity)
        {
            return Content(HttpStatusCode.OK, am.GetAttestWorkFlowTemplateHeads(base.ActorCompanyId, entity).ToDTOs());
        }

        [HttpGet]
        [Route("AttestWorkFlow/TemplateHeads/Rows/{templateHeadId:int}")]
        public IHttpActionResult GetAttestWorkFlowTemplateHeadRows(int templateHeadId)
        {
            return Ok(am.GetAttestWorkFlowTemplateRows(templateHeadId).ToDTOs(true));
        }

        [HttpGet]
        [Route("AttestWorkFlow/TemplateHeads/Rows/User/{templateHeadId:int}")]
        public IHttpActionResult GetAttestWorkFlowTemplateHeadRowsWithUser(int templateHeadId)
        {
            return Ok(am.GetAttestWorkFlowRowDTOs(templateHeadId, RoleId, UserId, true));
        }

        [HttpGet]
        [Route("AttestWorkFlow/Users/ByAttestTransition/{attestTransitionId:int}")]
        public IHttpActionResult GetAttestWorkFlowUsersByAttestTransition(int attestTransitionId)
        {
            return Content(HttpStatusCode.OK, am.GetUsersByAttestTransition(attestTransitionId).ToSmallDTOs());
        }

        [HttpGet]
        [Route("AttestWorkFlow/AttestRoles/ByAttestTransition/{attestTransitionId:int}")]
        public IHttpActionResult GetAttestWorkFlowAttestRolesByAttestTransition(int attestTransitionId)
        {
            return Content(HttpStatusCode.OK, am.GetAttestRolesForAttestTransition(attestTransitionId).ToDTOs());
        }

        [HttpGet]
        [Route("AttestWorkFlow/AttestWorkFlowHead/{attestWorkFlowHeadId:int}/{setTypeName:bool}/{loadRows:bool}")]
        public IHttpActionResult GetAttestWorkFlowHead(int attestWorkFlowHeadId, bool setTypeName, bool loadRows)
        {
            return Content(HttpStatusCode.OK, am.GetAttestWorkFlowHead(attestWorkFlowHeadId, loadRows).ToDTO(setTypeName, loadRows));
        }

        [HttpGet]
        [Route("AttestWorkFlow/HeadFromInvoiceId/{invoiceId:int}/{setTypeName:bool}/{loadTemplate:bool}/{loadRows:bool}/{loadRemoved:bool}")]
        public IHttpActionResult GetAttestWorkFlowHeadFromInvoiceId(int invoiceId, bool setTypeName, bool loadTemplate, bool loadRows, bool loadRemoved)
        {
            return Content(HttpStatusCode.OK, am.GetAttestWorkFlowHeadFromInvoiceId(invoiceId, setTypeName, loadTemplate: true, loadRows, loadRemoved).ToDTO(setTypeName, loadRows));
        }

        [HttpGet]
        [Route("AttestWorkFlow/RowsFromInvoiceId/{invoiceId:int}")]
        public IHttpActionResult GetAttestWorkFlowRowsFromInvoiceId(int invoiceId)
        {
            return Content(HttpStatusCode.OK, am.GetAttestWorkFlowRowsFromRecordId(SoeEntityType.SupplierInvoice, invoiceId));
        }

        #endregion

        #region AttestReminders
        [HttpPost]
        [Route("AttestWorkFlow/SendAttestReminders/")]
        public IHttpActionResult SendAttestReminders(ListIntModel itemsToSendMessages)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, am.SendSingleAttestReminders(base.ActorCompanyId, base.RoleId, base.UserId, itemsToSendMessages.Numbers));
        }
        #endregion

        #region Overview

        [HttpGet]
        [Route("AttestWorkFlow/Overview/{classification:int}/{allItemsSelection:int}")]
        public IHttpActionResult GetAttestWorkFlowOverview(int classification, TermGroup_ChangeStatusGridAllItemsSelection allItemsSelection)
        {
            return Content(HttpStatusCode.OK, sim.GetAttestWorkFlowOverview((SoeOriginStatusClassification)classification, allItemsSelection));
        }

        #endregion

        #endregion

        #region Invoice

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
        [Route("Invoice/SupplierInvoiceImage/CreateFinvoice/{ediEntryId:int}")]
        public IHttpActionResult CreateFinvoiceImage(int ediEntryId)
        {
            return Ok(edim.CreateFinvoiceImage(ediEntryId, ActorCompanyId));
        }

        [HttpGet]
        [Route("Invoice/{loadOpen:bool}/{loadClosed:bool}/{onlyMine:bool}/{allItemsSelection:int}/{projectId:int}/{includeChildProjects:bool}")]
        public IHttpActionResult GetInvoices(bool loadOpen, bool loadClosed, bool onlyMine, TermGroup_ChangeStatusGridAllItemsSelection allItemsSelection, int? projectId, bool includeChildProjects)
        {
            return Content(HttpStatusCode.OK, sim.GetSupplierInvoicesForGrid(loadOpen, loadClosed, allItemsSelection, projectId: projectId, includeChildProjects: includeChildProjects));
        }

		[HttpGet]
		[Route("Invoices/Grid")]
		public IHttpActionResult GetInvoices(TermGroup_ChangeStatusGridAllItemsSelection allItemsSelection, bool loadOpen, bool loadClosed)
		{
			return Content(HttpStatusCode.OK, sim.GetSupplierInvoicesForGrid(allItemsSelection, loadOpen, loadClosed));
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
        [Route("Invoice/{invoiceId:int}/{loadProjectRows:bool}/{loadOrderRows:bool}/{loadProject:bool}")]
        public IHttpActionResult GetInvoice(int invoiceId, bool loadProjectRows, bool loadOrderRows, bool loadProject)
        {
            List<AccountDim> dims = acm.GetAccountDimsByCompany(ActorCompanyId);
            var image = sim.GetSupplierInvoiceImage(ActorCompanyId, invoiceId, true);
            return Content(HttpStatusCode.OK, sim.GetSupplierInvoice(invoiceId, true, true, true, false, true, true, true, true, loadProjectRows, loadOrderRows, loadProject).ToSupplierInvoiceDTO(true, true, loadProjectRows, loadOrderRows, dims, image));
        }

        [HttpGet]
        [Route("Invoice/GetInvoiceTraceViews/{invoiceId:int}")]
        public IHttpActionResult getInvoiceTraceViews(int invoiceId)
        {
            CountryCurrencyManager ccm = new CountryCurrencyManager(null);
            int baseSysCurrencyId = ccm.GetCompanyBaseSysCurrencyId(base.ActorCompanyId);

            return Content(HttpStatusCode.OK, im.GetInvoiceTraceViews(invoiceId, baseSysCurrencyId));
        }

        [HttpPost]
        [Route("Invoice/BlockPayment/")]
        public IHttpActionResult BlockSupplierInvoicePayment(BlockPaymentModel model)
        {
            return Content(HttpStatusCode.OK, sim.SupplierInvoiceSaveInvoiceTextAction(model.InvoiceId, InvoiceTextType.SupplierInvoiceBlockReason, model.Block, model.Reason));
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
        [Route("Project/{type:int}/{active}/{getHidden}/{getFinished}/")]
        public IHttpActionResult GetProjectsList(TermGroup_ProjectType type, bool? active, bool? getHidden, bool? getFinished)
        {
            var projects = pm.GetProjectsList(base.ActorCompanyId, type, active, getHidden, getFinished).ToTinyDTOs();
            return Content(HttpStatusCode.OK, projects);
        }

        [HttpGet]
        [Route("Employees/{active}/{addRoleInfo}/{addCategoryInfo}/{addEmployeeGroupInfo}/{addProjectDefaultTimeCode}/{addEmployments}/{useShowOtherEmployeesPermission}/{getHidden}/{loadFactors}/{addPayrollGroupInfo}/{loadEmployeeVacation}")]
        public IHttpActionResult GetEmployees(bool? active, bool addRoleInfo, bool addCategoryInfo, bool addEmployeeGroupInfo, bool addProjectDefaultTimeCode, bool addEmployments, bool useShowOtherEmployeesPermission, bool getHidden, bool loadFactors, bool addPayrollGroupInfo, bool loadEmployeeVacation)
        {
            return Content(HttpStatusCode.OK, em.GetEmployeesForUsersAttestRoles(out _, base.ActorCompanyId, base.UserId, base.RoleId, active: active, getHidden: getHidden, useShowOtherEmployeesPermission: useShowOtherEmployeesPermission, addEmployeeAuthModelInfo: addCategoryInfo, addRoleInfo: addRoleInfo, addEmployeeGroupInfo: addEmployeeGroupInfo, addPayrollGroupInfo: addPayrollGroupInfo, addProjectDefaultTimeCode: addProjectDefaultTimeCode).ToDTOs(includeEmployments: addEmployments, includeEmployeeGroup: addEmployeeGroupInfo, includePayrollGroup: addPayrollGroupInfo, includeFactors: loadFactors, includeVacationGroup: loadEmployeeVacation));
        }

        [HttpGet]
        [Route("TimeCodes/{timeCodeType}/{active}/{loadPayrollProducts}/")]
        public IHttpActionResult GetTimeCodes(SoeTimeCodeType timeCodeType, bool? active, bool loadPayrollProducts)
        {
            return Content(HttpStatusCode.OK, tcm.GetTimeCodes(base.ActorCompanyId, timeCodeType, active ?? false, loadPayrollProducts).ToDTOs(loadPayrollProducts));
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
                return Content(HttpStatusCode.OK, sim.SaveSupplierInvoice(model.Invoice, model.PurchaseInvoiceRows, model.AccountingRows, model.ProjectRows, model.OrderRows, model.CostAllocationRows, base.ActorCompanyId, base.RoleId, model.CreateAttestVoucher, false, model.SkipInvoiceNrCheck, model.DisregardConcurrencyCheck));
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
                return Content(HttpStatusCode.OK, edm.AddEdiEntrys((TermGroup_EDISourceType)ediSourceType, false));
            else
                return Content(HttpStatusCode.OK, edm.AddScanningEntrys((TermGroup_EDISourceType)ediSourceType, base.ActorCompanyId));
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
                return Content(HttpStatusCode.OK, edm.TransferToSupplierInvoicesFromEdiDict(itemsToTransfer.Numbers, base.ActorCompanyId, base.UserId));
        }

        [HttpPost]
        [Route("TransferEdiToOrder/")]
        public IHttpActionResult TransferEdiToOrder(ListIntModel itemsToTransfer)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, edm.TransferToOrdersFromEdi(itemsToTransfer.Numbers, base.ActorCompanyId, false));
        }

        [HttpPost]
        [Route("TransferEdiState/")]
        public IHttpActionResult TransferEdiState(TransferEdiStateModel model)
        {
            return Content(HttpStatusCode.OK, edm.ChangeEdiEntriesState(model.IdsToTransfer, model.StateTo, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("GetEdiEntry/{ediEntryId:int}/{loadSuppliers:bool}")]
        public IHttpActionResult GetEdiEntry(int ediEntryId, bool loadSuppliers)
        {
            return Content(HttpStatusCode.OK, edm.GetEdiEntry(ediEntryId, base.ActorCompanyId, true, loadSuppliers).ToDTO(false, false));
        }

        [HttpGet]
        [Route("Scanning/GetInterpretedInvoice/{ediEntryId:int}")]
        public IHttpActionResult GetInterpretedInvoice(int ediEntryId)
        {
            return Content(HttpStatusCode.OK, edm.GetSupplierInvoiceInterpretationDTO(base.ActorCompanyId, ediEntryId));
        }

        [HttpGet]
        [Route("Scanning/GetUnprocessedCount/")]
        public int GetScanningUnprocessedCount()
        {
            return sim.GetScanningUnprocessedCount();
        }

        [HttpGet]
        [Route("Scanning/GetScanningEntry/{ediEntryId:int}")]
        public EdiEntryDTO GetEdiScanningEntry(int ediEntryId)
        {
            return edim.GetEdiScanningEntryInvoice(ediEntryId, base.ActorCompanyId, false).ToDTO(true);
        }

        [HttpGet]
        [Route("Scanning/GetScanningEntryDocumentId/{scanningEntryInvoiceId:int}")]
        public string GetScanningEntryDocumentId(int scanningEntryInvoiceId)
        {
            return edim.GetScanningEntryDocumentId(scanningEntryInvoiceId, base.ActorCompanyId);
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
            return Content(HttpStatusCode.OK, edm.CreateSupplierFromFinvoice(ediEntryId, base.ActorCompanyId));
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

        #endregion

        #region Unpaid invoice

        [HttpGet]
        [Route("Invoice/Unpaid/{supplierId:int}/{addEmpty:bool}")]
        public IHttpActionResult GetUnpaidInvoices(int supplierId, bool addEmpty)
        {
            return Content(HttpStatusCode.OK, im.GetUnpaidInvoices(SoeOriginType.SupplierInvoice, supplierId, ActorCompanyId, addEmpty).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Invoice/Payment/{invoiceId:int}")]
        public IHttpActionResult GetInvoiceForPayment(int invoiceId)
        {
            return Content(HttpStatusCode.OK, sim.GetSupplierInvoiceForPayment(invoiceId, ActorCompanyId).ToSupplierInvoiceDTO(true, true, false, false)); //Set isAngular to false since rows aren't loaded 
        }

        #endregion

        #region Payments
        [HttpGet]
        [Route("GetPaymentRowsSmall/{invoiceId:int}")]
        public IHttpActionResult GetPaymentRowsSmall(int invoiceId)
        {
            var paymentRows = pam.GetPaymentRowsByInvoiceSmall(invoiceId).ToSmallDTOs();
            return Content(HttpStatusCode.OK, paymentRows);
        }

        [HttpGet]
        [Route("GetPaymentRows/{invoiceId:int}")]
        public IHttpActionResult GetPaymentRows(int invoiceId)
        {
            return Content(HttpStatusCode.OK, pam.GetPaymentRowsByInvoice(invoiceId, true).ToDTOs(false, false, null));
        }

        [HttpGet]
        [Route("Payment/{classification:int}/{allItemsSelection:int}")]
        public IHttpActionResult GetPayments(int classification, TermGroup_ChangeStatusGridAllItemsSelection allItemsSelection)
        {
            return Content(HttpStatusCode.OK, sim.GetSupplierPaymentsForGrid((SoeOriginStatusClassification)classification, allItemsSelection));
        }

        [HttpGet]
        [Route("PaymentRow/{paymentRowId:int}/{loadInvoiceAndOrigin:bool}/{loadAccountRows:bool}/{loadAccounts:bool}")]
        public IHttpActionResult GetPayment(int paymentRowId, bool loadInvoiceAndOrigin, bool loadAccountRows, bool loadAccounts)
        {
            List<AccountDim> dims = acm.GetAccountDimsByCompany(ActorCompanyId);
            return Content(HttpStatusCode.OK, pam.GetPaymentRow(paymentRowId, loadInvoiceAndOrigin, loadAccountRows, loadAccounts, true, false, true).ToDTO(loadAccountRows, loadInvoiceAndOrigin, dims));
        }

        [HttpPost]
        [Route("PaymentRow")]
        public IHttpActionResult SaveSupplierPayment(SaveSupplierPaymentModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, pam.SavePaymentRow(model.Payment, model.AccountingRows, true, model.MatchCodeId.Value, base.ActorCompanyId));
            }
        }

        [HttpPost]
        [Route("Payment/Transfer")]
        public IHttpActionResult TransferSupplierPayment(TransferSupplierPaymentModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pam.TransferSupplierPayment(model.Payments, model.originStatusChange, model.AccountYearId, model.PaymentMethodId, model.bulkPayDate, model.SendPaymentFile));
        }

        [HttpDelete]
        [Route("Payment/Cancel/{paymentRowId:int}/{revertVoucher:bool}")]
        public IHttpActionResult CancelSupplierPayment(int paymentRowId, bool revertVoucher)
        {
            return Content(HttpStatusCode.OK, pam.CancelPaymentRow(paymentRowId, base.ActorCompanyId, revertVoucher));
        }

        [HttpDelete]
        [Route("Payment/Cancel/Extended/{paymentRowId:int}")]
        public IHttpActionResult CancelSupplierPaymentWithVoucher(int paymentRowId)
        {
            return Content(HttpStatusCode.OK, pam.CancelPaymentRowWithVoucher(paymentRowId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Payment/Notification")]
        public IHttpActionResult SendPaymentNotification(SendPaymentNotificationModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, pam.SendPaymentNotification(base.ActorCompanyId, model.PaymentMethodId, model.PageUrl, model.Classification));
        }

        #endregion

        #region Supplier

        [HttpGet]
        [Route("Supplier/")]
        public IHttpActionResult GetSuppliers(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, sm.GetSuppliersByCompanyDict(base.ActorCompanyId, message.GetBoolValueFromQS("onlyActive"), message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            //return Content(HttpStatusCode.OK, sm.GetSuppliersByCompany(base.ActorCompanyId, message.GetBoolValueFromQS("onlyActive")).ToGridDTOs());
            return Content(HttpStatusCode.OK, sm.GetSuppliersByCompanyExtended(base.ActorCompanyId, message.GetBoolValueFromQS("onlyActive")));
        }

        [HttpPost]
        [Route("Supplier/ByCompany/")]
        public IHttpActionResult GetSuppliersByCompany(SuppliersByCompanyDTO dto)
        {
            return Content(HttpStatusCode.OK, sm.GetSuppliersByCompanyDict(dto.ActorCompanyId, dto.IsActive, dto.AddEmptyRow).ToSmallGenericTypes());
        }

        [HttpPost]
        [Route("Supplier/BySearch/")]
        public IHttpActionResult GetSuppliersBySearch(SearchSuppliersDTO dto)
        {
            return Content(HttpStatusCode.OK, sm.GetSuppliersBySearch(base.ActorCompanyId, dto.SupplierNumber, dto.SupplierName).ToGridDTOs());
        }

        [HttpGet]
        [Route("Supplier/{supplierId:int}/{loadActor:bool}/{loadAccount:bool}/{loadContactAddresses:bool}/{loadCategories:bool}")]
        public IHttpActionResult GetSupplier(int supplierId, bool loadActor, bool loadAccount, bool loadContactAddresses, bool loadCategories)
        {
            var supplier = sm.GetSupplier(supplierId, loadActor, false, loadContactAddresses, loadCategories)
                .ToDTO(loadContactAddresses);
            if (loadAccount)
                supplier.AccountingSettings = sm.GetSupplierAccountSettings(this.ActorCompanyId, supplierId);
            return Content(HttpStatusCode.OK, supplier);
        }

        [HttpGet]
        [Route("Supplier/Export/{supplierId:int}")]
        public IHttpActionResult GetSupplierForExport(int supplierId)
        {
            var supplier = sm.GetSupplier(supplierId, true, false, true, true)
                .ToDTO(true);
            supplier.AccountingSettings = sm.GetSupplierAccountSettings(this.ActorCompanyId, supplierId);
            return Content(HttpStatusCode.OK, supplier);
        }

        [HttpGet]
        [Route("Supplier/PurchaseDeliveryInvoices/{supplierInvoiceId:int}")]
        public IHttpActionResult GetSupplierPurchaseDeliveryInvoices(int supplierInvoiceId)
        {
            return Content(HttpStatusCode.OK, sim.GetSupplierPurchaseDeliveryInvoices(base.ActorCompanyId, supplierInvoiceId));
        }

        [HttpGet]
        [Route("Supplier/Purchase/{supplierId:int}")]
        public IHttpActionResult GetSupplierPurchase(int supplierId)
        {
            return Content(HttpStatusCode.OK, pum.GetPurchasesForSelectBySupplier(base.ActorCompanyId, supplierId, SoeOriginStatus.PurchaseSent));
        }

        [HttpGet]
        [Route("Invoice/PurchaseRows/{getOnlyDelivered:bool}/{getAlreadyConnected:bool}")]
        public IHttpActionResult GetSupplierPurchaseRows(bool getOnlyDelivered, bool getAlreadyConnected, [ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] purchaseIds)
        {
            return Content(HttpStatusCode.OK, pum.GetPurchaseDeliveryInvoiceFromPurchase(base.ActorCompanyId, purchaseIds.ToList(), getOnlyDelivered, getAlreadyConnected));
        }

        [HttpGet]
        [Route("Supplier/NextSupplierNr/")]
        public IHttpActionResult GetNextSupplierNr()
        {
            return Content(HttpStatusCode.OK, sm.GetNextSupplierNr(base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Supplier")]
        public IHttpActionResult SaveSupplier(SaveSupplierModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sm.SaveSupplier(model.Supplier, model.Supplier.ContactPersons, model.Files, base.ActorCompanyId, model.ExtraFields));
        }

        [HttpPost]
        [Route("Supplier/SupplierInvoiceChangeCompany")]
        public IHttpActionResult SaveSupplierInvoiceChangeCompany(SupplierInvoiceChangeCompanyDTO changeCompanyDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                if (changeCompanyDTO.InvoiceId > 0)
                    return Content(HttpStatusCode.OK, im.SaveInvoiceCompany(changeCompanyDTO.InvoiceId, changeCompanyDTO.CompanyId, changeCompanyDTO.SupplierId, changeCompanyDTO.VoucherSeriesId));
                //TODO later also handle change company for scanningentry's here using the SaveScanningEntryCompany method in EdiManager
                //else if (changeCompanyDTO.ScanningEntryId > 0)
                //    return Content(HttpStatusCode.OK, edm.SaveScanningEntryCompany((changeCompanyDTO.ScanningEntryId, changeCompanyDTO.CompanyId, changeCompanyDTO.SupplierId, changeCompanyDTO.VoucherSeriesId);                                                        
                else
                    return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            }
        }

        [HttpDelete]
        [Route("Supplier/{supplierId:int}")]
        public IHttpActionResult DeleteSupplier(int supplierId)
        {
            return Content(HttpStatusCode.OK, sm.DeleteSupplier(supplierId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Supplier/SupplierInvoiceChangeInvoiceSeqNr")]
        public IHttpActionResult SupplierInvoiceChangeInvoiceSeqNrSuperAdmin(ChangeInvoiceSeqNrStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.ChangeInvoiceSequenceNumber(model.InvoiceId, model.SeqNr));
        }

        [HttpPost]
        [Route("Supplier/SupplierInvoiceNrAlreadyExist")]
        public IHttpActionResult SupplierInvoiceNrAlreadyExist(SupplierInvoiceNrAlreadyExistModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sim.SupplierInvoiceNumberExist(model.ActorId, model.InvoiceId, model.InvoiceNr));
        }

        [HttpPost]
        [Route("Supplier/UpdateState")]
        public IHttpActionResult UpdateSuppliersState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sm.UpdateSuppliersState(model.Dict));
        }

        [HttpPost]
        [Route("Supplier/UpdateIsPrivatePerson")]
        public IHttpActionResult UpdateIsPrivatePerson(List<UpdateIsPrivatePerson> items)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, sm.UpdateSuppliersIsPrivatePerson(items.ToDictionary(k => k.id, v => v.isPrivatePerson)));
        }

        #endregion

        #region AgeDistribution

        [HttpPost]
        [Route("Invoice/AgeDistribution")]
        public IHttpActionResult SearchAgeDistribution(SearchAgeDistributionDTO message)
        {
            var result = im.GetAgeDistribution(ActorCompanyId, message.Type, message.CompareDate, message.InsecureDebts, message.CurrencyType, message.ActorNrFrom, message.ActorNrTo, message.SeqNrFrom, message.SeqNrTo, message.InvNrFrom, message.InvNrTo, message.InvDateFrom, message.InvDateTo, message.ExpDateFrom, message.ExpDateTo);
            return Ok(result);
        }
        #endregion

        #region Matches

        [HttpPost]
        [Route("Invoice/Matches/Payments")]
        public IHttpActionResult SearchInvoicesPaymentsAndMatches(SearchInvoicesPaymentsAndMatchesDTO message)
        {
            return Ok(im.GetInvoicesPaymentsAndMatches(ActorCompanyId, message.ActorId, message.Type, message.AmountFrom, message.AmountTo, message.DateFrom, message.DateTo, message.OriginType));
        }

        [HttpGet]
        [Route("Invoice/Matches")]
        public IHttpActionResult GetMatches([FromUri] int recordId, [FromUri] int actorId, [FromUri] int type)
        {
            return Ok(im.GetMatches(ActorCompanyId, actorId, recordId, type));
        }

        [HttpGet]
        [Route("Invoice/Matches/MatchCodes/{type:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetMatchCodes(SoeInvoiceMatchingType type, bool addEmptyRow)
        {
            return Ok(im.GetMatchCodes(base.ActorCompanyId, type, addEmptyRow).ToDTOs());
        }

        [HttpGet]
        [Route("Invoice/Matches/MatchingCustomerSupplier/{type:int}")]
        public IHttpActionResult GetMatchingCustomerSupplier(SoeOriginType type)
        {
            return Ok(im.GetSuppliersCustomersFromUnpaidInvoices(type, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Invoice/Matches/InvoicePaymentMatchAndVoucher/")]
        public IHttpActionResult InvoicePaymentMatchAndVoucher(InvoicePaymentMatchAndVoucherDTO invoicePaymentMatchAndVoucher)
        {
            return Ok(im.AddInvoicePaymentMatchAndVoucher(invoicePaymentMatchAndVoucher.VoucherHead, invoicePaymentMatchAndVoucher.AccoutningsRows, invoicePaymentMatchAndVoucher.Matchings, ActorCompanyId, invoicePaymentMatchAndVoucher.MatchCodeId));
        }

        [HttpGet]
        [Route("Invoice/Matches/InvoicePaymentsMatches/{actorId:int}/{type:int}")]
        public IHttpActionResult GetInvoicePaymentsMatches(int actorId, SoeOriginType type)
        {
            return Ok(im.GetInvoicePaymentMatches(base.ActorCompanyId, actorId, type));
        }

        #endregion
    }
}