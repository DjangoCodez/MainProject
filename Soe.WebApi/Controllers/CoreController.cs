using Soe.WebApi.Binders;
using Soe.WebApi.Extensions;
using Soe.WebApi.Models;
using SoftOne.Soe.Business.Core;
using SoftOne.Soe.Business.Core.Logger;
using SoftOne.Soe.Business.DataCache;
using SoftOne.Soe.Common.DTO;
using SoftOne.Soe.Common.DTO.SignatoryContract;
using SoftOne.Soe.Common.Util;
using SoftOne.Soe.Data;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using static SoftOne.Soe.Util.ZipUtility;

namespace Soe.WebApi.Controllers
{
    [RoutePrefix("Core")]
    public class CoreController : SoeApiController
    {
        #region Variables

        private readonly ActorManager acm;
        private readonly ApiManager apim;
        private readonly ApiDataManager apidm;
        private readonly AnalysisManager am;
        private readonly CalendarManager calm;
        private readonly CategoryManager cm;
        private readonly ChecklistManager clm;
        private readonly CommunicationManager cma;
        private readonly CompanyManager coma;
        private readonly ContactManager com;
        private readonly CountryCurrencyManager ccm;
        private readonly CustomerManager cum;
        private readonly DashboardManager dm;
        private readonly EmployeeManager em;
        private readonly EmailManager emm;
        private readonly FeatureManager fm;
        private readonly GeneralManager gm;
        private readonly GraphicsManager grm;
        private readonly HelpManager hm;
        private readonly InvoiceManager im;
        private readonly ImportExportManager iem;
        private readonly LanguageManager lm;
        private readonly LoggerManager lgm;
        private readonly PaymentManager pm;
        private readonly ProjectManager prm;
        private readonly SettingManager sm;
        private readonly SequenceNumberManager sqm;
        private readonly SysLogManager slm;
        private readonly TermManager tm;
        private readonly TimeScheduleManager tscm;
        private readonly TimeStampManager tsm;
        private readonly TimeTransactionManager ttm;
        private readonly TrackChangesManager tcm;
        private readonly ReportManager rm;
        private readonly UserManager um;
        private readonly ExtraFieldManager efm;
        private readonly BatchUpdateManager bum;
        private readonly ImportDynamicManager idm;
        private readonly ExcelImportManager eim;
        private readonly SignatoryContractManager scm;

        #endregion

        #region Constructor

        public CoreController(ActorManager acm, ApiManager apm, ApiDataManager apdm, AnalysisManager am, CalendarManager calm, CategoryManager cm, ChecklistManager clm, ContactManager com, CountryCurrencyManager ccm, CustomerManager cum, DashboardManager dm, FeatureManager fm, GeneralManager gm, GraphicsManager grm, HelpManager hm, InvoiceManager im, ImportExportManager iem, LanguageManager lm, LoggerManager lgm, PaymentManager pm, ProjectManager prm, SettingManager sm, SequenceNumberManager sqm, SysLogManager slm, TermManager tm, TimeStampManager tsm, EmployeeManager em, TimeTransactionManager ttm, ReportManager rm, CommunicationManager cma, UserManager um, TimeScheduleManager tscm, EmailManager emm, TrackChangesManager tcm, CompanyManager coma, ExtraFieldManager efm, BatchUpdateManager bum, ImportDynamicManager idm, ExcelImportManager eim, SignatoryContractManager scm)
        {
            this.acm = acm;
            this.apim = apm;
            this.apidm = apdm;
            this.am = am;
            this.calm = calm;
            this.cm = cm;
            this.clm = clm;
            this.com = com;
            this.ccm = ccm;
            this.cum = cum;
            this.dm = dm;
            this.em = em;
            this.emm = emm;
            this.fm = fm;
            this.gm = gm;
            this.grm = grm;
            this.hm = hm;
            this.im = im;
            this.iem = iem;
            this.lm = lm;
            this.lgm = lgm;
            this.pm = pm;
            this.prm = prm;
            this.sm = sm;
            this.slm = slm;
            this.tcm = tcm;
            this.tm = tm;
            this.tsm = tsm;
            this.ttm = ttm;
            this.rm = rm;
            this.cma = cma;
            this.um = um;
            this.tscm = tscm;
            this.sqm = sqm;
            this.coma = coma;
            this.efm = efm;
            this.bum = bum;
            this.idm = idm;
            this.eim = eim;
            this.scm = scm;
        }

        #endregion

        #region Accounts

        [HttpGet]
        [Route("Accounts/InventoryTriggerAccounts")]
        public IHttpActionResult GetInventoryTriggerAccounts()
        {
            return Content(HttpStatusCode.OK, sm.GetInventoryEditTriggerAccountsFromSettings(base.ActorCompanyId).ToList());
        }

        #endregion

        #region Address       

        #region AddressRowType

        [HttpGet]
        [Route("Address/AddressRowType/{sysContactTypeId:int}")]
        public IHttpActionResult GetSysContactAddressRowTypeIds(int sysContactTypeId)
        {
            return Content(HttpStatusCode.OK, com.GetSysContactAddressRowTypesWithAddressTypes(sysContactTypeId));
        }

        #endregion

        #region AddressType

        [HttpGet]
        [Route("Address/AddressType/{sysContactTypeId:int}")]
        public IHttpActionResult GetSysContactAddressTypeIds(int sysContactTypeId)
        {
            return Content(HttpStatusCode.OK, com.GetSysContactAddressTypeIds(sysContactTypeId));
        }

        #endregion

        #region EComType

        [HttpGet]
        [Route("Address/EComType/{sysContactTypeId:int}")]
        public IHttpActionResult GetSysContactEComTypeIds(int sysContactTypeId)
        {
            return Content(HttpStatusCode.OK, com.GetSysContactEComsTypeIds(sysContactTypeId));
        }

        #endregion

        #endregion

        #region ApiMessage

        [HttpGet]
        [Route("Api/Messages/{type:int}/{source:int}/{dateFrom}/{dateTo}/{showVerified:bool}/{showOnlyErrors:bool}")]
        public IHttpActionResult GetApiMessages(int type, int source, string dateFrom, string dateTo, bool showVerified, bool showOnlyErrors)
        {
            return Content(HttpStatusCode.OK, apidm.GetApiMessagesForGrid((TermGroup_ApiMessageType)type, (TermGroup_ApiMessageSourceType)source, BuildDateTimeFromString(dateFrom, true, CalendarUtility.DATETIME_DEFAULT).Value, BuildDateTimeFromString(dateTo, true, CalendarUtility.DATETIME_DEFAULT).Value, showVerified, showOnlyErrors));
        }

        [HttpPost]
        [Route("Api/Messages/SetAsVerified")]
        public IHttpActionResult SetApiMessageAsVerified(ListIntModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, apidm.SetApiMessageAsVerified(model.Numbers));
        }

        [HttpPost]
        [Route("Api/Messages/Import/Employees/{onlyLogging}")]
        public async Task<IHttpActionResult> ImportEmployeeApiMessage(bool onlyLogging)
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var data = await Request.Content.ParseMultipartAsync();

                Extensions.HttpPostedFile file = data?.Files["file"];
                var result = apim.ImportEmployeeChangesFromFile(TermGroup_ApiMessageType.Employee, (onlyLogging ? TermGroup_ApiMessageSourceType.APIManualOnlyLogging : TermGroup_ApiMessageSourceType.APIManual), file?.File);
                if (!result.Success && result.ErrorNumber != (int)ActionResultSave.IncorrectInput)
                    result = new ActionResult(true); //Only show error dialog in gui when incorrect input

                return Content(HttpStatusCode.OK, result);
            }

            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        #endregion

        #region ApiSetting

        [HttpGet]
        [Route("Api/Settings/")]
        public IHttpActionResult GetApiMessages()
        {
            return Content(HttpStatusCode.OK, apidm.GetApiSettingsForGrid());
        }

        [HttpPost]
        [Route("Api/Settings/")]
        public IHttpActionResult SaveApiMessages(SaveApiSettingsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, apidm.SaveApiSettings(model.Settings));
        }

        #endregion

        #region AssemblyInfo

        [HttpGet]
        [Route("AssemblyInfo/Date/")]
        public IHttpActionResult GetAssemblyDate()
        {
            return Content(HttpStatusCode.OK, gm.GetAssemblyDate());
        }

        [HttpGet]
        [Route("AssemblyInfo/Version/")]
        public IHttpActionResult GetAssemblyVersion()
        {
            return Content(HttpStatusCode.OK, GeneralManager.GetAssemblyVersion());
        }

        [HttpGet]
        [Route("SiteType/")]
        public IHttpActionResult GetSiteType()
        {
            return Content(HttpStatusCode.OK, CompDbCache.Instance.SiteType);
        }

        #endregion

        #region Checklists

        [HttpGet]
        [Route("Checklists/Heads/Dict/{type:int}")]
        public IHttpActionResult GetChecklistHeadsDict(int type)
        {
            return Content(HttpStatusCode.OK, clm.GetChecklistHeadsDict((TermGroup_ChecklistHeadType)type, base.ActorCompanyId, false));
        }

        [HttpGet]
        [Route("Checklists/Heads/{type:int}/{loadRows:bool}")]
        public IHttpActionResult GetChecklistHeadsForType(int type, bool loadRows)
        {
            return Content(HttpStatusCode.OK, clm.GetChecklistHeadsForType((TermGroup_ChecklistHeadType)type, base.ActorCompanyId, loadRows).ToDTOs(loadRows));
        }

        [HttpGet]
        [Route("Checklists/Heads/Head/{checklistHeadId:int}/{loadRows:bool}")]
        public IHttpActionResult GetChecklistHead(int checklistHeadId, bool loadRows)
        {
            return Content(HttpStatusCode.OK, clm.GetChecklistHead(checklistHeadId, base.ActorCompanyId, loadRows, true).ToDTO(loadRows));
        }

        [HttpGet]
        [Route("Checklists/HeadRecords/{entity:int}/{recordId:int}")]
        public IHttpActionResult GetChecklistHeadRecords(int entity, int recordId)
        {
            return Content(HttpStatusCode.OK, clm.GetChecklistsRecordsWithSignatures((SoeEntityType)entity, recordId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Checklists/RowRecords/{entity:int}/{recordId:int}")]
        public IHttpActionResult GetChecklistRowRecords(int entity, int recordId)
        {
            return Content(HttpStatusCode.OK, clm.GetChecklistRows((SoeEntityType)entity, recordId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Checklists/Signatures/{entity:int}/{recordId:int}/{useThumbnails:bool}")]
        public IHttpActionResult GetChecklistSignatures(int entity, int recordId, bool useThumbnails)
        {
            return Content(HttpStatusCode.OK, clm.GetEntityChecklistsSignatures(base.ActorCompanyId, (SoeEntityType)entity, recordId));
        }

        [HttpPost]
        [Route("Checklists/MultiChoiceQuestions/")]
        public IHttpActionResult GetChecklistMultipleChoiceQuestions(ListIntModel model)
        {
            return Content(HttpStatusCode.OK, clm.GetChecklistMultipleChoiceRows(model.Numbers));
        }

        [HttpPost]
        [Route("Checklists/Rows/")]
        public IHttpActionResult SaveChecklistRows(ChecklistRowModel model)
        {
            return Content(HttpStatusCode.OK, clm.SaveChecklistRecords(model.rows, model.entity, model.recordId, base.ActorCompanyId));
        }

        #endregion

        #region Company

        [HttpGet]
        [Route("Company/{actorCompanyId:int}")]
        public IHttpActionResult GetCompany(int actorCompanyId)
        {
            return Content(HttpStatusCode.OK, coma.GetCompany(actorCompanyId, true).ToCompanyDTO());
        }

        #endregion

        #region ContactAddress

        [HttpGet]
        [Route("ContactAddress/{actorId:int}/{type:int}/{addEmptyRow:bool}/{includeRows:bool}/{includeCareOf:bool}")]
        public IHttpActionResult GetContactAddresses(int actorId, TermGroup_SysContactAddressType type, bool addEmptyRow, bool includeRows, bool includeCareOf)
        {
            int contactId = com.GetContactIdFromActorId(actorId);
            return Content(HttpStatusCode.OK, com.GetContactAddresses(contactId, type, addEmptyRow, includeCareOf).ToDTOs(includeRows));
        }

        [HttpGet]
        [Route("ContactAddressDict/{contactPersonId:int}")]
        public IHttpActionResult GetContactAddressItemsDict(int contactPersonId)
        {
            return Content(HttpStatusCode.OK, com.GetContactAddressItemsDict(contactPersonId).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("ContactAddressItem/{actorId:int}")]
        public IHttpActionResult GetContactAddressItems(int actorId)
        {
            return Content(HttpStatusCode.OK, com.GetContactAddressItems(actorId));
        }

        [HttpGet]
        [Route("ContactAddressItem/ByUser/{userId:int}")]
        public IHttpActionResult GetContactAddressItemsByUser(int userId)
        {
            return Content(HttpStatusCode.OK, com.GetContactAddressItems(um.GetActorContactPersonId(userId)));
        }

        #endregion

        #region ContactPerson

        [HttpGet]
        [Route("ContactPerson/ContactPersons")]
        public IHttpActionResult GetContactPersons()
        {
            return Content(HttpStatusCode.OK, com.GetContactPersonsByActorIdsForGrid());
        }

        [HttpGet]
        [Route("ContactPerson/ContactPersonsByActorId/{actorId}")]
        public IHttpActionResult GetContactPersonsByActorId(int actorId)
        {
            return Content(HttpStatusCode.OK, com.GetContactPersonsByActorId(actorId));
        }

        [HttpGet]
        [Route("ContactPerson/ContactPersonsByActorIds/{actorIds}")]
        public IHttpActionResult GetContactPersonsByActorIds(string actorIds)
        {
            return Content(HttpStatusCode.OK, com.GetContactPersonsByActorIdsForGrid(actorIds));
        }

        [HttpGet]
        [Route("ContactPerson/Export/{actorId}")]
        public IHttpActionResult GetContactPerson(int actorId)
        {
            return Content(HttpStatusCode.OK, com.GetContactPersonForExport(actorId, true).ToDTO());
        }

        [HttpGet]
        [Route("ContactPerson/Categories/{actorId}")]
        public IHttpActionResult GetContactPersonCategories(int actorId)
        {
            return Content(HttpStatusCode.OK, com.GetContactPersonCategories(actorId));
        }

        [HttpPost]
        [Route("ContactPerson/")]
        public IHttpActionResult SaveContactPerson(ContactPersonDTO contactPerson)
        {
            return Content(HttpStatusCode.OK, com.SaveContactPerson(base.ActorCompanyId, contactPerson));
        }

        [HttpPost]
        [Route("ContactPerson/Delete/")]
        public IHttpActionResult DeleteContactPersons(List<int> contactPersonIds)
        {
            return Content(HttpStatusCode.OK, com.DeleteContactPersons(contactPersonIds));
        }

        [HttpDelete]
        [Route("ContactPerson/{contactPersonId:int}")]
        public IHttpActionResult DeleteContactPerson(int contactPersonId)
        {
            return Content(HttpStatusCode.OK, com.DeleteContactPerson(contactPersonId, true, true));
        }

        #endregion

        #region Category

        [HttpGet]
        [Route("Category/")]
        public IHttpActionResult GetCategories(HttpRequestMessage message)
        {
            SoeCategoryType categoryType = (SoeCategoryType)message.GetIntValueFromQS("soeCategoryTypeId");

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, cm.GetCategoriesDict(categoryType, base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());

            return Content(HttpStatusCode.OK, cm.GetCategoryDTOs(categoryType, base.ActorCompanyId, message.GetBoolValueFromQS("loadCompanyCategoryRecord"), message.GetBoolValueFromQS("loadChildren"), message.GetBoolValueFromQS("loadCategoryGroups")));
        }

        [HttpGet]
        [Route("Category/ForRoleFromType/{employeeId:int}/{categoryType:int}/{isAdmin:bool}/{includeSecondary:bool}/{addEmptyRow:bool}")]
        public IHttpActionResult GetCategories(int employeeId, int categoryType, bool isAdmin, bool includeSecondary, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, cm.GetCategoriesForRoleFromTypeDict(base.ActorCompanyId, base.UserId, employeeId, (SoeCategoryType)categoryType, isAdmin, includeSecondary, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Category/AccountsByAccount/{accountId:int}/{loadCategory:bool}")]
        public IHttpActionResult GetCategoryAccounts(int accountId, bool loadCategory)
        {
            return Content(HttpStatusCode.OK, cm.GetCategoryAccountsByAccount(accountId, base.ActorCompanyId, loadCategory).ToDTOs());
        }

        #endregion

        #region Connect

        [HttpGet]
        [Route("Connect/Imports/{module:int}")]
        public IHttpActionResult GetImports(SoeModule module)
        {
            return Content(HttpStatusCode.OK, iem.GetImports(base.ActorCompanyId, module).ToDTOs());
        }

        [HttpGet]
        [Route("Connect/ImportEdit/{importId:int}")]
        public IHttpActionResult GetImport(int importId)
        {
            return Content(HttpStatusCode.OK, iem.GetImport(base.ActorCompanyId, importId, true).ToDTO());
        }

        [HttpGet]
        [Route("Connect/SysImportDefinitions/{module:int}")]
        public IHttpActionResult GetSysImportDefinitions(SoeModule module)
        {
            return Content(HttpStatusCode.OK, iem.GetSysImportDefinitions(module).ToDTOs(false));
        }

        [HttpGet]
        [Route("Connect/SysImportHeads/")]
        public IHttpActionResult GetSysImportHeads()
        {
            return Content(HttpStatusCode.OK, iem.GetSysImportHeads().ToDTOs(false, false));
        }

        [HttpGet]
        [Route("Connect/Batches/{importHeadType:int}/{allItemsSelection:int}")]
        public IHttpActionResult GetImportBatches(int importHeadType, TermGroup_GridDateSelectionType allItemsSelection)
        {
            if (importHeadType > 0)
            {
                return Content(HttpStatusCode.OK, iem.GetImportBatches(base.ActorCompanyId, (TermGroup_IOImportHeadType)importHeadType, allItemsSelection));
            }
            else
            {
                return Content(HttpStatusCode.OK, iem.GetImportBatches(base.ActorCompanyId));
            }
        }

        [HttpGet]
        [Route("Connect/ImportGridColumns/{importHeadType:int}")]
        public IHttpActionResult GetImportGridColumns(int importHeadType)
        {
            return Content(HttpStatusCode.OK, iem.GetImportGridColumnsDTOs(importHeadType));
        }

        [HttpGet]
        [Route("Connect/ImportIOResult/{importHeadType:int}/{batchId}")]
        public IHttpActionResult GetImportIOResult(int importHeadType, string batchId)
        {

            if (importHeadType == (int)TermGroup_IOImportHeadType.Customer)
                return Content(HttpStatusCode.OK, iem.GetCustomerIOResult(base.ActorCompanyId, TermGroup_IOType.XEConnect, TermGroup_IOSource.Connect, batchId).ToDTOs());
            else if (importHeadType == (int)TermGroup_IOImportHeadType.CustomerInvoice)
                return Content(HttpStatusCode.OK, iem.GetCustomerInvoiceHeadIOResult(base.ActorCompanyId, TermGroup_IOType.XEConnect, TermGroup_IOSource.Connect, batchId).ToDTOs(false));
            else if (importHeadType == (int)TermGroup_IOImportHeadType.CustomerInvoiceRow)
                return Content(HttpStatusCode.OK, iem.GetCustomerInvoiceRowIOResult(base.ActorCompanyId, TermGroup_IOType.XEConnect, TermGroup_IOSource.Connect, batchId).ToDTOs());
            else if (importHeadType == (int)TermGroup_IOImportHeadType.Supplier)
                return Content(HttpStatusCode.OK, iem.GetSupplierIOResult(base.ActorCompanyId, TermGroup_IOType.XEConnect, TermGroup_IOSource.Connect, batchId).ToDTOs());
            else if (importHeadType == (int)TermGroup_IOImportHeadType.SupplierInvoice ||
                     importHeadType == (int)TermGroup_IOImportHeadType.SupplierInvoiceAnsjo)
                return Content(HttpStatusCode.OK, iem.GetSupplierInvoiceHeadIOResult(base.ActorCompanyId, TermGroup_IOType.XEConnect, TermGroup_IOSource.Connect, batchId).ToDTOs());
            else if (importHeadType == (int)TermGroup_IOImportHeadType.Voucher)
                return Content(HttpStatusCode.OK, iem.GetVoucherHeadIOResult(base.ActorCompanyId, TermGroup_IOType.XEConnect, TermGroup_IOSource.Connect, batchId).ToDTOs(false));
            else if (importHeadType == (int)TermGroup_IOImportHeadType.Project)
                return Content(HttpStatusCode.OK, iem.GetProjectIOResult(base.ActorCompanyId, TermGroup_IOType.XEConnect, TermGroup_IOSource.Connect, batchId).ToDTOs());
            else
                return Ok();

        }

        [HttpPost]
        [Route("Connect/ImportSelectionGrid/")]
        public IHttpActionResult Import(FilesLookupDTO files)
        {
            return Content(HttpStatusCode.OK, iem.GetImportSelectionGrid(
                base.ActorCompanyId,
                files.Files));
        }

        [HttpPost]
        [Route("Connect/ImportEdit/")]
        public IHttpActionResult SaveImport(ImportDTO import)
        {
            return Content(HttpStatusCode.OK, iem.SaveImport(import, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Connect/ImportFile/")]
        public IHttpActionResult Import(ImportModel model)
        {
            return Content(HttpStatusCode.OK, iem.XEConnectImport(base.ActorCompanyId, model.importId, model.dataStorageIds, model.accountYearId, model.voucherSeriesId, model.importDefinitionId));
        }

        [HttpPost]
        [Route("Connect/ImportIO/")]
        public IHttpActionResult ImportIO(ImportIOModel model)
        {
            IHttpActionResult httpActionResult = null;
            switch (model.importHeadType)
            {
                case TermGroup_IOImportHeadType.Customer:
                    httpActionResult = Content(HttpStatusCode.OK, iem.ImportCustomerIO(model.ioIds, base.ActorCompanyId));
                    break;
                case TermGroup_IOImportHeadType.CustomerInvoice:
                    httpActionResult = Content(HttpStatusCode.OK, iem.ImportCustomerInvoiceHeadIO(model.ioIds, base.ActorCompanyId));
                    break;
                case TermGroup_IOImportHeadType.CustomerInvoiceRow:
                    httpActionResult = Content(HttpStatusCode.OK, iem.ImportCustomerInvoiceRowIO(model.ioIds, base.ActorCompanyId));
                    break;
                case TermGroup_IOImportHeadType.Supplier:
                    httpActionResult = Content(HttpStatusCode.OK, iem.ImportSupplierIO(model.ioIds, base.ActorCompanyId));
                    break;
                case TermGroup_IOImportHeadType.SupplierInvoice:
                case TermGroup_IOImportHeadType.SupplierInvoiceAnsjo:
                    httpActionResult = Content(HttpStatusCode.OK, iem.ImportSupplierInvoiceHeadIO(model.ioIds, base.ActorCompanyId));
                    break;
                case TermGroup_IOImportHeadType.Voucher:
                    httpActionResult = Content(HttpStatusCode.OK, iem.ImportVoucherHeadIO(model.ioIds, base.ActorCompanyId, model.UseAccountDistribution, model.useAccoungDims, model.defaultDim1AccountId, model.defaultDim2AccountId, model.defaultDim3AccountId, model.defaultDim4AccountId, model.defaultDim5AccountId, model.defaultDim6AccountId));
                    break;
                case TermGroup_IOImportHeadType.Project:
                    httpActionResult = Content(HttpStatusCode.OK, iem.ImportProjectIO(model.ioIds, base.ActorCompanyId));
                    break;
            }

            return httpActionResult;
        }

        [HttpPost]
        [Route("Connect/CustomerIODTO/")]
        public IHttpActionResult SaveCustomerIODTO(List<CustomerIODTO> customerIODTOs)
        {
            return Content(HttpStatusCode.OK, iem.UpdateCustomerIO(customerIODTOs));
        }

        [HttpPost]
        [Route("Connect/CustomerInvoiceIODTO/")]
        public IHttpActionResult SaveCustomerInvoiceIODTO(List<CustomerInvoiceIODTO> customerInvoiceIODTOs)
        {
            return Content(HttpStatusCode.OK, iem.UpdateCustomerInvoiceHeadIO(customerInvoiceIODTOs));
        }

        [HttpPost]
        [Route("Connect/CustomerInvoiceRowIODTO/")]
        public IHttpActionResult SaveCustomerInvoiceRowIODTO(List<CustomerInvoiceRowIODTO> customerInvoiceRowIODTOs)
        {
            return Content(HttpStatusCode.OK, iem.UpdateCustomerInvoiceRowIO(customerInvoiceRowIODTOs));
        }

        [HttpPost]
        [Route("Connect/SupplierIODTO/")]
        public IHttpActionResult SaveSupplierIODTO(List<SupplierIODTO> supplierIODTOs)
        {
            return Content(HttpStatusCode.OK, iem.UpdateSupplierIO(supplierIODTOs));
        }

        [HttpPost]
        [Route("Connect/SupplierInvoiceIODTO/")]
        public IHttpActionResult SaveSupplieInvoiceIODTO(List<SupplierInvoiceHeadIODTO> supplierInvoiceIODTOs)
        {
            return Content(HttpStatusCode.OK, iem.UpdateSupplierInvoiceHeadIO(supplierInvoiceIODTOs));
        }

        [HttpPost]
        [Route("Connect/VoucherIODTO/")]
        public IHttpActionResult SaveVoucherIODTO(List<VoucherHeadIODTO> voucherIODTOs)
        {
            return Content(HttpStatusCode.OK, iem.UpdateVoucherHeadIO(voucherIODTOs));
        }

        [HttpPost]
        [Route("Connect/ProjectIODTO/")]
        public IHttpActionResult SaveProjectIODTO(List<ProjectIODTO> projectIODTOs)
        {
            return Content(HttpStatusCode.OK, iem.UpdateProjectIO(projectIODTOs));
        }

        [HttpDelete]
        [Route("Connect/ImportEdit/{importId:int}")]
        public IHttpActionResult DeleteImport(int importId)
        {
            return Content(HttpStatusCode.OK, iem.DeleteImport(importId, base.ActorCompanyId));
        }

        #endregion

        #region CompanyCategory
        [HttpGet]
        [Route("Category/CompCategoryRecords/{soeCategoryTypeId:int}/{categoryRecordEntity:int}/{recordId:int}")]
        public IHttpActionResult GetCompCategoryRecords(int soeCategoryTypeId, int categoryRecordEntity, int recordId)
        {
            return Content(HttpStatusCode.OK, cm.GetCompanyCategoryRecords((SoeCategoryType)soeCategoryTypeId, (SoeCategoryRecordEntity)categoryRecordEntity, recordId, base.ActorCompanyId).ToDTOs(false));
        }
        #endregion

        #region Currency

        [HttpGet]
        [Route("Currency/Sys/")]
        public IHttpActionResult GetSysCurrencies(HttpRequestMessage message)
        {
            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, ccm.GetSysCurrencies(true).ToSmallGenericTypes(message.GetBoolValueFromQS("addEmptyRow"), message.GetBoolValueFromQS("useCode")));

            return Content(HttpStatusCode.OK, ccm.GetSysCurrencies(true).ToDTOs());
        }

        [HttpGet]
        [Route("Currency/Comp/")]
        public IHttpActionResult GetCompCurrencies(HttpRequestMessage message)
        {
            bool loadRates = message.GetBoolValueFromQS("loadRates");

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, ccm.GetCompCurrenciesDict(base.ActorCompanyId, message.GetBoolValueFromQS("addEmptyRow")).ToSmallGenericTypes());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_SMALL_DTO))
                return Content(HttpStatusCode.OK, ccm.GetCompCurrencies(base.ActorCompanyId, false).ToSmallDTOs());
            else if (message.HasAcceptValue(HttpExtensions.ACCEPT_GRID_DTO))
                return Content(HttpStatusCode.OK, ccm.GetCompCurrencies(base.ActorCompanyId, loadRates).ToGridDTOs(loadRates));

            return Content(HttpStatusCode.OK, ccm.GetCompCurrencies(base.ActorCompanyId, loadRates).ToDTOs(loadRates));
        }

        [HttpGet]
        [Route("Currency/Comp/{currencyId:int}")]
        public IHttpActionResult GetCompCurrencyRates(int currencyId)
        {
            return Content(HttpStatusCode.OK, ccm.GetCompCurrencyRates(base.ActorCompanyId, currencyId).ToGridDTOs());
        }

        [HttpGet]
        [Route("Currency/Comp/{sysCurrencyId:int}/{date}/{rateToBase:bool}")]
        public IHttpActionResult GetCompCurrencyRate(int sysCurrencyId, string date, bool rateToBase)
        {
            return Content(HttpStatusCode.OK, ccm.GetCurrencyRate(base.ActorCompanyId, sysCurrencyId, BuildDateTimeFromString(date, true).Value, rateToBase));
        }

        [HttpGet]
        [Route("Currency/Ledger/{actorId:int}")]
        public IHttpActionResult GetLedgerCurrency(int actorId)
        {
            return Content(HttpStatusCode.OK, ccm.GetLedgerCurrency(base.ActorCompanyId, actorId).ToDTO(false));
        }

        [HttpGet]
        [Route("Currency/Enterprise/")]
        public IHttpActionResult GetEnterpriseCurrency()
        {
            return Content(HttpStatusCode.OK, ccm.GetCompanyBaseEntCurrency(base.ActorCompanyId).ToDTO(false));
        }

        [HttpGet]
        [Route("Currency/Comp/BaseCurrency/")]
        public IHttpActionResult GetCompanyCurrency()
        {
            return Content(HttpStatusCode.OK, ccm.GetCompanyBaseCurrency(base.ActorCompanyId).ToDTO(false));
        }

        #endregion

        #region Customer

        [HttpPost]
        [Route("Customer/Search/")]
        public IHttpActionResult GetCustomersBySearch(CustomerSearchModel model)
        {
            CustomerSearchDTO dto = new CustomerSearchDTO()
            {
                ActorCustomerId = model.ActorCustomerId,
                CustomerNr = model.CustomerNr,
                Name = model.Name,
                BillingAddress = model.BillingAddress,
                DeliveryAddress = model.DeliveryAddress,
                Note = model.Note
            };
            return Content(HttpStatusCode.OK, cum.GetCustomersBySearch(dto, base.ActorCompanyId, base.RoleId, base.UserId));
        }

        [HttpGet]
        [Route("Customer/Customer/")]
        public IHttpActionResult GetCustomers(HttpRequestMessage message)
        {
            bool onlyActive = message.GetBoolValueFromQS("onlyActive");

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_GENERIC_TYPE))
                return Content(HttpStatusCode.OK, cum.GetCustomersByCompanyDict(base.ActorCompanyId, onlyActive, message.GetBoolValueFromQS("addEmptyRow"), base.RoleId, base.UserId).ToSmallGenericTypes());

            if (message.HasAcceptValue(HttpExtensions.ACCEPT_SMALL_DTO))
                return Content(HttpStatusCode.OK, cum.GetCustomersByCompanySmall(base.ActorCompanyId, onlyActive, base.RoleId, base.UserId));

            //return Content(HttpStatusCode.OK, cum.GetCustomersByCompany(base.ActorCompanyId, onlyActive, base.RoleId, base.UserId, false, true, true).ToGridDTOs().OrderBy(c => c.CustomerNr));
            return Content(HttpStatusCode.OK, cum.GetCustomersForGrid(base.ActorCompanyId, onlyActive, base.RoleId, base.UserId));
        }

        [HttpGet]
        [Route("Customer/Customer/{customerId:int}/{loadActor:bool}/{loadAccount:bool}/{loadNote:bool}/{loadCustomerUser:bool}/{loadContactAddresses:bool}/{loadCategories:bool}")]
        public IHttpActionResult GetCustomer(int customerId, bool loadActor, bool loadAccount, bool loadNote, bool loadCustomerUser, bool loadContactAddresses, bool loadCategories)
        {
            return Content(HttpStatusCode.OK, cum.GetCustomer(customerId, loadActor, loadAccount, loadCustomerUser, loadContactAddresses, loadCategories, true).ToDTO(loadContactAddresses, loadAccount, loadNote));
        }

        [HttpGet]
        [Route("Customer/Customer/Export/{customerId:int}")]
        public IHttpActionResult GetCustomerForExport(int customerId)
        {
            return Content(HttpStatusCode.OK, cum.GetCustomer(customerId, true, true, true, true, true, true).ToDTO(true, true, true));
        }

        [HttpGet]
        [Route("Customer/Customer/CashCustomer")]
        public IHttpActionResult GetCashCustomer()
        {
            return Content(HttpStatusCode.OK, cum.GetDefaultCashCustomerId(base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Customer/Customer/Statistics/")]
        public IHttpActionResult GetCustomerStatistics(CustomerStatisticsModel model)
        {
            return Content(HttpStatusCode.OK, im.GetProductStatisticsPerCustomer(base.ActorCompanyId, model.CustomerId, model.AllItemSelection));
        }

        [HttpPost]
        [Route("Customer/Customer/StatisticsAllCustomers/")]
        public IHttpActionResult GetCustomerStatisticsAllCustomers(GeneralProductStatisticsModel model)
        {
            return Content(HttpStatusCode.OK, im.GetProductStatistics(model.OriginType, model.FromDate, model.ToDate));
        }

        [HttpGet]
        [Route("Customer/Customer/NextCustomerNr/")]
        public IHttpActionResult GetNextCustomerNr()
        {
            return Content(HttpStatusCode.OK, cum.GetNextCustomerNr(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Customer/Customer/Reference/{customerId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetCustomerReferences(int customerId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, com.GetCustomerReferencesDict(customerId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Customer/Customer/Email/{customerId:int}/{loadContactPersonsEmails:bool}/{addEmptyRow:bool}")]
        public IHttpActionResult GetCustomerEmailAddresses(int customerId, bool loadContactPersonsEmails, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, cum.GetCustomerEmailAddresses(customerId, loadContactPersonsEmails, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Customer/Customer/GLN/{customerId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetCustomerGlnNumbers(int customerId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, cum.GetCustomerGlnNumbers(customerId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpDelete]
        [Route("Customer/Customer/{customerId:int}")]
        public IHttpActionResult DeleteCustomer(int customerId)
        {
            return Content(HttpStatusCode.OK, cum.DeleteCustomer(customerId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Customer/Customer")]
        public IHttpActionResult SaveCustomer(SaveCustomerModel saveModel)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, cum.SaveCustomer(saveModel.Customer, null, saveModel.Customer.ContactPersons, saveModel.HouseHoldTaxApplicants, base.ActorCompanyId, saveModel.ExtraFields));
        }

        [HttpPost]
        [Route("Customer/Customer/UpdateState")]
        public IHttpActionResult UpdateCustomersState(UpdateEntityStatesModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, cum.UpdateCustomersState(model.Dict));
        }

        [HttpPost]
        [Route("Customer/Customer/UpdateIsPrivatePerson")]
        public IHttpActionResult UpdateIsPrivatePerson(List<UpdateIsPrivatePerson> items)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, cum.UpdateCustomersIsPrivatePerson(items.ToDictionary(k => k.id, v => v.isPrivatePerson)));
        }

        #endregion

        #region CustomerExports

        [HttpPost]
        [Route("Customer/Exports/")]
        public IHttpActionResult CreateCustomerExport(EvaluatedSelection selection)
        {

            /*
             HttpResponseMessage
              var returnAnsiFile = selection.ExportFileType == TermGroup_ReportExportFileType.ICACustomerBalance;
              HttpContext.Current.Response.ContentEncoding = Encoding.UTF8;

              var file = iem.CreateCustomerExport(selection);

              var response = new HttpResponseMessage(HttpStatusCode.Accepted);
              if (file.Success)
              {
                  if (returnAnsiFile)
                  {
                      var content = Constants.ENCODING_LATIN1.GetBytes(file.StringValue);
  #if DEBUG
                      File.WriteAllBytes(@"C:\Temp\Inexchange\incoming\kunddat.txt", content);
  #endif
                      HttpContext.Current.Response.ContentEncoding = Encoding.ASCII;
                      response.Content = new ByteArrayContent(content);
                      response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain") { CharSet = "ascii" };
                  }
                  else
                  {
                      response.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(file.StringValue));
                      response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain") { CharSet = "utf8" };
                  }
              }
              else
              {
                  response.Content = new StringContent(file.ErrorMessage);
                  response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain") { CharSet = "utf8" };
              }

              return response;
              */
            return Content(HttpStatusCode.OK, iem.CreateCustomerExport(selection));
        }

        #endregion

        #region CustomerInvoices

        [HttpGet]
        [Route("CustomerInvoices/{classification:int}/{origintype:int}/{loadOpen:bool}/{loadClosed:bool}/{onlyMine:bool}/{loadActive:bool}/{allItemsSelection:int}/{billing:bool}")]
        public IHttpActionResult GetInvoices(int classification, int originType, bool loadOpen, bool loadClosed, bool onlyMine, bool loadActive, TermGroup_ChangeStatusGridAllItemsSelection allItemsSelection, bool billing)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoicesForGrid((SoeOriginStatusClassification)classification, originType, base.ActorCompanyId, base.UserId, loadOpen, loadClosed, onlyMine, loadActive, allItemsSelection, billing));
        }

        [HttpPost]
        [Route("CustomerInvoices/")]
        public IHttpActionResult GetInvoicesForProjectCentral(CustomerInvoicesGridModel model)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoicesForGrid((SoeOriginStatusClassification)model.Classification, model.OriginType, base.ActorCompanyId, base.UserId, model.LoadOpen, model.LoadClosed, model.OnlyMine, model.LoadActive, (TermGroup_ChangeStatusGridAllItemsSelection)model.AllItemsSelection, model.Billing, invoiceIds: model.ModifiedIds));
        }

        [HttpPost]
        [Route("CustomerInvoices/ProjectCentral/")]
        public IHttpActionResult GetInvoicesForProjectCentral(InvoicesForProjectCentralModel model)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoicesForGrid((SoeOriginStatusClassification)model.Classification, model.OriginType, base.ActorCompanyId, base.UserId, true, true, false, true, TermGroup_ChangeStatusGridAllItemsSelection.All, false, model.ProjectId, model.LoadChildProjects, model.InvoiceIds, fromDate: model.FromDate, toDate: model.ToDate));
        }

        [HttpPost]
        [Route("CustomerInvoices/CustomerCentral/")]
        public IHttpActionResult GetInvoicesForCustomerCentral(InvoicesForCustomerCentralModel model)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoicesForGrid((SoeOriginStatusClassification)model.Classification, model.OriginType, base.ActorCompanyId, base.UserId, true, true, model.OnlyMine, true, TermGroup_ChangeStatusGridAllItemsSelection.All, false, actorCustomerId: model.ActorCustomerId));
        }

        [HttpPost]
        [Route("CustomerInvoices/Filtered/")]
        public IHttpActionResult GetFilteredCustomerInvoices(ExpandoObject filterModels)
        {
            return Content(HttpStatusCode.OK, im.GetFilteredCustomerInvoicesForGrid(filterModels));
        }

        [HttpPost]
        [Route("CustomerInvoices/Transfer")]
        public IHttpActionResult TransferCustomerInvoices(TransferCustomerInvoiceAndPaymentModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.TransferCustomerInvoices(model.Items, model.originStatusChange, model.PaymentMethodId, model.MergeInvoices, model.ClaimLevel, model.bulkPayDate, model.bulkInvoiceDate, model.bulkDueDate, model.bulkVoucherDate, model.KeepFixedPriceOrderOpen, model.CheckPartialInvoicing, model.CreateCopiesOfTransferedContractRows, model.SetStatusToOrigin, model.EmailTemplateId, model.ReportId, model.LanguageId, model.MergePdfs, model.OverrideFinvoiceOperatorWarning));
        }

        [HttpPost]
        [Route("CustomerInvoices/Import")]
        public IHttpActionResult ImportProductRows(CustomerInvoiceRowImportModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.ImportProductRows(model.Bytes, model.WholesellerId, model.InvoiceId, model.TypeId, model.ActorCustomerId, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("CustomerInvoices/AutomaticallyDistribute")]
        public IHttpActionResult AutomaticallyDistributeCustomerInvoices(AutomaticDistributionModel model)
        {

            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.AutomaticInvoiceDistribution(this.ActorCompanyId, model.Items));
        }

        [HttpGet]
        [Route("CustomerInvoices/ReminderInformation/{invoiceId:int}")]
        public IHttpActionResult GetReminderPrintedInformation(int invoiceId)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoicePrintedRemindersMessage(invoiceId));
        }

        [HttpGet]
        [Route("CustomerInvoices/NumbersDict/{customerId:int}/{originType:int}/{classification:int}/{registrationType:int}/{orderByNumber:bool}")]
        public IHttpActionResult GetReminderPrintedInformation(int customerId, int originType, int classification, int registrationType, bool orderByNumber)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoiceContractsDict(base.ActorCompanyId, customerId, (SoeOriginType)originType, (SoeOriginStatusClassification)classification, orderByNumber));
        }

        [HttpGet]
        [Route("CustomerInvoices/Rows/{invoiceId:int}")]
        public IHttpActionResult GetCustomerInvoiceRowsForInvoice(int invoiceId)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoiceRows(invoiceId, true).ToProductRowDTOs());
        }

        [HttpGet]
        [Route("CustomerInvoices/RowsSmall/{invoiceId:int}")]
        public IHttpActionResult GetCustomerInvoiceRowsSmallForInvoice(int invoiceId)
        {
            return Content(HttpStatusCode.OK, im.GetCustomerInvoiceDetailsRowsForInvoice(invoiceId));
        }

        [HttpGet]
        [Route("CustomerInvoices/ServiceOrdersForAgreement/{invoiceId:int}")]
        public IHttpActionResult GetServiceOrdersForAgreementDetails(int invoiceId)
        {
            return Content(HttpStatusCode.OK, im.GetServiceOrdersForAgreementDetails(base.ActorCompanyId, invoiceId));
        }

        [HttpPost]
        [Route("CustomerInvoices/CopyRows/")]
        public IHttpActionResult CopyCustomerInvoiceRows(CopyCustomerInvoiceRowsModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.CopyCustomerInvoiceRows(model.RowsToCopy, model.OriginType, base.ActorCompanyId, model.TargetId, model.OriginId.HasValue && model.OriginId.Value > 0 ? model.OriginId.Value : (int?)null, model.UpdateOrigin.HasValue && model.UpdateOrigin.Value, model.Recalculate.HasValue && model.Recalculate.Value));
        }

        [HttpGet]
        [Route("CustomerInvoices/PendingReminders/{customerId:int}/{loadCustomer:bool}/{loadProduct:bool}")]
        public IHttpActionResult GetPendingCustomerInvoiceReminders(int customerId, bool loadCustomer, bool loadProduct)
        {
            return Content(HttpStatusCode.OK, im.GetPendingCustomerInvoiceReminders(customerId, loadCustomer, loadProduct).ToDTOs());
        }

        [HttpGet]
        [Route("CustomerInvoices/PendingInterests/{customerId:int}/{loadCustomer:bool}/{loadProduct:bool}")]
        public IHttpActionResult GetPendingCustomerInvoiceInterests(int customerId, bool loadCustomer, bool loadProduct)
        {
            return Content(HttpStatusCode.OK, im.GetPendingCustomerInvoiceInterests(customerId, loadCustomer, loadProduct).ToDTOs());
        }

        [HttpPost]
        [Route("CustomerInvoices/SearchSmall/")]
        public IHttpActionResult GetInvoicesBySearch(SearchCustomerInvoiceModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.GetInvoicesBySearch(base.ActorCompanyId, model.OriginType, new CustomerInvoiceSearchParamsDTO
                {
                    CustomerId = model.CustomerId,
                    ProjectId = model.ProjectId,
                    Number = model.Number,
                    ExternalNr = model.ExternalNr,
                    CustomerName = model.CustomerName,
                    CustomerNr = model.CustomerNr,
                    InternalText = model.InternalText,
                    ProjectNr = model.ProjectNr,
                    ProjectName = model.ProjectName,
                    IgnoreChildren = model.IgnoreChildren,
                    IgnoreInvoiceId = model.IgnoreInvoiceId,
                    IncludePreliminary = model.IncludePreliminary == true,
                    IncludeVoucher = model.IncludeVoucher == true,
                    UserId = model.UserId,
                    FullyPaid = model.FullyPaid,
                }, 0));
        }

        [HttpDelete]
        [Route("CustomerInvoices/PendingReminders/{customerId:int}")]
        public IHttpActionResult DeletePendingCustomerInvoiceReminders(int customerId)
        {
            return Content(HttpStatusCode.OK, im.DeletePendingCustomerInvoiceReminders(customerId));
        }

        [HttpDelete]
        [Route("CustomerInvoices/PendingInterests/{customerId:int}")]
        public IHttpActionResult DeletePendingCustomerInvoiceInterests(int customerId)
        {
            return Content(HttpStatusCode.OK, im.DeletePendingCustomerInvoiceInterests(customerId));
        }

        #endregion

        #region Dashboard

        [HttpGet]
        [Route("Dashboard/SysGauge/{module:int}")]
        public IHttpActionResult GetSysGauges(SoeModule module)
        {
            return Content(HttpStatusCode.OK, dm.GetSysGauges(module, base.LicenseId, base.ActorCompanyId, base.RoleId).ToDTOs());
        }

        [HttpGet]
        [Route("Dashboard/UserGauge/{module:int}")]
        public IHttpActionResult GetUserGauges(SoeModule module)
        {
            return Content(HttpStatusCode.OK, dm.GetUserGauges(module, base.LicenseId, base.ActorCompanyId, base.RoleId, base.UserId, true).ToDTOs(true));
        }

        [HttpPost]
        [Route("Dashboard/UserGauge")]
        public IHttpActionResult AddUserGauge(UserGaugeDTO dto)
        {
            dto.ActorCompanyId = base.ActorCompanyId;
            dto.RoleId = base.RoleId;
            dto.UserId = base.UserId;
            dto.WindowState = 0;

            var result = dm.AddUserGauge(dto);
            return Content(HttpStatusCode.OK, dm.GetUserGauge(result.IntegerValue, true).ToDTO(true));
        }

        [HttpGet]
        [Route("Dashboard/UserGaugeHead/{userGaugeHeadId:int}/{module:int}")]
        public IHttpActionResult GetUserGaugeHead(int userGaugeHeadId, int? module)
        {
            return Content(HttpStatusCode.OK, dm.GetUserGaugeHead(userGaugeHeadId, (SoeModule)(module ?? 0), this.UserId, this.RoleId, this.ActorCompanyId, this.LicenseId));
        }

        [HttpGet]
        [Route("Dashboard/UserGaugeHeads/")]
        public IHttpActionResult GetUserGaugeHead()
        {
            return Content(HttpStatusCode.OK, dm.GetUserGaugeHeads(this.UserId, this.RoleId, this.ActorCompanyId).ToDTOs());
        }

        [HttpPost]
        [Route("Dashboard/UserGaugeHead/")]
        public IHttpActionResult SaveUserGaugeHead(UserGaugeHeadDTO dto)
        {
            return Content(HttpStatusCode.OK, dm.SaveUserGaugeHead(dto, this.ActorCompanyId, this.UserId));
        }

        [HttpPost]
        [Route("Dashboard/UserGauge/Setting")]
        public IHttpActionResult UpdateUserGaugeSettings(UpdateUserGaugeSettingsModel model)
        {
            return Content(HttpStatusCode.OK, dm.UpdateUserGaugeSettings(model.UserGaugeId, model.Settings));
        }

        [HttpPost]
        [Route("Dashboard/UserGauge/Sort/{userGaugeId:int}/{sort:int}")]
        public IHttpActionResult SaveUserGaugeSort(int userGaugeId, int sort)
        {
            return Content(HttpStatusCode.OK, dm.SaveUserGaugeSort(userGaugeId, sort));
        }

        [HttpDelete]
        [Route("Dashboard/UserGauge/{userGaugeId}")]
        public IHttpActionResult DeleteUserGauge(int userGaugeId)
        {
            return Content(HttpStatusCode.OK, dm.DeleteUserGauge(userGaugeId));
        }

        #region Widgets

        [HttpGet]
        [Route("Dashboard/Widget/Attestflow")]
        public IHttpActionResult GetAttestFlowInvoicesForGauge()
        {
            var invoices = dm.GetAttestFlowInvoices(base.ActorCompanyId);
            return Content(HttpStatusCode.OK, invoices);
        }

        [HttpGet]
        [Route("Dashboard/Widget/EmployeeRequests/{setEmployeeRequestTypeNames:bool}")]
        public IHttpActionResult GetEmployeeRequests(bool setEmployeeRequestTypeNames)
        {
            return Content(HttpStatusCode.OK, dm.GetEmployeeRequests(base.ActorCompanyId, base.UserId, base.RoleId, setEmployeeRequestTypeNames));
        }

        [HttpGet]
        [Route("Dashboard/Widget/Map/Start")]
        public IHttpActionResult GetMapStartAddress()
        {
            return Content(HttpStatusCode.OK, dm.GetMapStartAddress(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Dashboard/Widget/Map/Locations/{dateFrom}")]
        public IHttpActionResult GetMapLocations(string dateFrom)
        {
            return Content(HttpStatusCode.OK, dm.GetMapLocations(base.ActorCompanyId, BuildDateTimeFromString(dateFrom, true)));
        }

        [HttpGet]
        [Route("Dashboard/Widget/Map/PlannedOrders/{date}")]
        public IHttpActionResult GetPlannedOrderMaps(string date)
        {
            return Content(HttpStatusCode.OK, dm.GetPlannedOrderMaps(base.ActorCompanyId, base.RoleId, BuildDateTimeFromString(date, true)));
        }

        [HttpGet]
        [Route("Dashboard/Widget/MySchedule/{employeeId}/{dateFrom}/{dateTo}")]
        public IHttpActionResult GetMySchedule(int employeeId, string dateFrom, string dateTo)
        {
            return Content(HttpStatusCode.OK, dm.GetMySchedule(base.ActorCompanyId, base.UserId, base.RoleId, employeeId, BuildDateTimeFromString(dateFrom, true, CalendarUtility.DATETIME_DEFAULT).Value, BuildDateTimeFromString(dateTo, true, CalendarUtility.DATETIME_DEFAULT).Value));
        }

        [HttpGet]
        [Route("Dashboard/Widget/MySchedule/OpenShifts/{employeeId}/{dateFrom}/{dateTo}")]
        public IHttpActionResult GetOpenShifts(int employeeId, string dateFrom, string dateTo)
        {
            return Content(HttpStatusCode.OK, dm.GetOpenShifts(base.ActorCompanyId, base.UserId, base.RoleId, employeeId, BuildDateTimeFromString(dateFrom, true, CalendarUtility.DATETIME_DEFAULT).Value, BuildDateTimeFromString(dateTo, true, CalendarUtility.DATETIME_DEFAULT).Value));
        }

        [HttpGet]
        [Route("Dashboard/Widget/MySchedule/MyColleaguesSchedule/{employeeId}/{dateFrom}/{dateTo}")]
        public IHttpActionResult GetMyColleaguesSchedule(int employeeId, string dateFrom, string dateTo)
        {
            return Content(HttpStatusCode.OK, dm.GetMyColleaguesSchedule(base.ActorCompanyId, base.UserId, base.RoleId, employeeId, BuildDateTimeFromString(dateFrom, true, CalendarUtility.DATETIME_DEFAULT).Value, BuildDateTimeFromString(dateTo, true, CalendarUtility.DATETIME_DEFAULT).Value));
        }

        [HttpGet]
        [Route("Dashboard/Widget/MyShifts/{employeeId}/{dateFrom}/{dateTo}")]
        public IHttpActionResult GetMyShifts(int employeeId, string dateFrom, string dateTo)
        {
            return Content(HttpStatusCode.OK, dm.GetMyShifts(employeeId, BuildDateTimeFromString(dateFrom, true, CalendarUtility.DATETIME_DEFAULT).Value, BuildDateTimeFromString(dateTo, true, CalendarUtility.DATETIME_DEFAULT).Value).ToList());
        }

        [HttpGet]
        [Route("Dashboard/Widget/OpenShifts/{employeeId}/{dateFrom}/{dateTo}")]
        public IHttpActionResult GetOpenShiftsForGauge(int employeeId, string dateFrom, string dateTo)
        {
            return Content(HttpStatusCode.OK, dm.GetOpenShiftsForGauge(base.ActorCompanyId, base.RoleId, base.UserId, employeeId, BuildDateTimeFromString(dateFrom, true, CalendarUtility.DATETIME_DEFAULT).Value, BuildDateTimeFromString(dateTo, true, CalendarUtility.DATETIME_DEFAULT).Value).ToList());
        }

        [HttpGet]
        [Route("Dashboard/Widget/PerformanceAnalyzer/ServiceTypes/")]
        public IHttpActionResult GetDashboardStatisticServiceTypes()
        {
            List<SmallGenericType> types = new List<SmallGenericType>();

            foreach (SoftOne.Status.Shared.ServiceType type in Enum.GetValues(typeof(SoftOne.Status.Shared.ServiceType)))
            {
                if (type != SoftOne.Status.Shared.ServiceType.Unknown)
                    types.Add(new SmallGenericType((int)type, type.ToString()));
            }

            return Content(HttpStatusCode.OK, types.OrderBy(t => t.Name).ToList());
        }

        [HttpGet]
        [Route("Dashboard/Widget/PerformanceAnalyzer/PerformanceTest/{serviceType:int}")]
        public IHttpActionResult GetDashboardStatisticTypes(SoftOne.Status.Shared.ServiceType serviceType)
        {
            return Content(HttpStatusCode.OK, dm.GetDashboardStatisticTypes(serviceType));
        }

        [HttpGet]
        [Route("Dashboard/Widget/PerformanceAnalyzer/PerformanceTest/{dashboardStatisticTypeKey}/{dateFrom}/{dateTo}/{interval:int}")]
        public IHttpActionResult GetPerformanceTestResults(string dashboardStatisticTypeKey, string dateFrom, string dateTo, TermGroup_PerformanceTestInterval interval)
        {
            return Content(HttpStatusCode.OK, dm.GetPerformanceTestResults(dashboardStatisticTypeKey, BuildDateTimeFromString(dateFrom, true).Value, BuildDateTimeFromString(dateTo, true).Value, interval));
        }

        [HttpGet]
        [Route("Dashboard/Widget/Reports")]
        public IHttpActionResult GetReports()
        {
            var reports = rm.GetReportPrintoutsForGauge(base.UserId, base.ActorCompanyId, 2);

            return Content(HttpStatusCode.OK, reports);
        }

        [HttpDelete]
        [Route("Dashboard/Widget/SystemInfo/Delete/{rowId}")]
        public IHttpActionResult DeleteSystemInfoLogRow(int rowId)
        {
            return Content(HttpStatusCode.OK, gm.DisableSystemInfoLogEntry(base.ActorCompanyId, rowId));
        }

        [HttpGet]
        [Route("Dashboard/Widget/SysLog/{clientIpNr}/{noOfRecords:int}")]
        public IHttpActionResult GetSysLogs(string clientIpNr, int noOfRecords)
        {
            return Content(HttpStatusCode.OK, slm.GetSysLogs(SoeLogType.System_All_Today, false, base.LicenseId, base.ActorCompanyId, base.RoleId, base.UserId, clientIpNr, noOfRecords).ToGridDTOs());
        }

        [HttpGet]
        [Route("Dashboard/Widget/SystemInfo")]
        public IHttpActionResult GetSystemInfo(HttpRequestMessage message)
        {
            return Content(HttpStatusCode.OK, gm.GetSystemInfoLogEntriesByRole(base.ActorCompanyId, base.RoleId).ToDTOs());
        }

        [HttpGet]
        [Route("Dashboard/Widget/TaskWatchLog/Task/{dateFrom}/{dateTo}/{actorCompanyId:int}/{userId:int}")]
        public IHttpActionResult GetTaskWatchLogTasks(string dateFrom, string dateTo, int actorCompanyId, int userId)
        {
            return Content(HttpStatusCode.OK, dm.GetTaskWatchLogTasks(base.BuildDateTimeFromString(dateFrom, true).Value, base.BuildDateTimeFromString(dateTo, true).Value, actorCompanyId != 0 ? actorCompanyId : (int?)null, userId != 0 ? userId : (int?)null));
        }

        [HttpGet]
        [Route("Dashboard/Widget/TaskWatchLog/Result/{task}/{dateFrom}/{dateTo}/{interval:int}/{calculationType:int}/{actorCompanyId:int}/{userId:int}")]
        public IHttpActionResult GetTaskWatchLogResult(string task, string dateFrom, string dateTo, TermGroup_PerformanceTestInterval interval, TermGroup_TaskWatchLogResultCalculationType calculationType, int actorCompanyId, int userId)
        {
            return Content(HttpStatusCode.OK, dm.GetTaskWatchLogResult(task, BuildDateTimeFromString(dateFrom, true).Value, BuildDateTimeFromString(dateTo, true).Value, interval, calculationType, actorCompanyId != 0 ? actorCompanyId : (int?)null, userId != 0 ? userId : (int?)null));
        }

        [HttpGet]
        [Route("Dashboard/Widget/TimeStampAttendance/{showMode:int}/{onlyIn:bool}")]
        public IHttpActionResult GetTimeStampAttendanceForGauge(int showMode, bool onlyIn)
        {
            bool includeMissingEmployees = fm.HasRolePermission(Feature.Time_TimeStampAttendanceGauge_ShowNotStampedIn, Permission.Readonly, base.RoleId, base.ActorCompanyId);
            return Content(HttpStatusCode.OK, dm.GetTimeStampAttendance(base.ActorCompanyId, base.UserId, base.RoleId, (TermGroup_TimeStampAttendanceGaugeShowMode)showMode, onlyIn, includeMissingEmployees: includeMissingEmployees).ToList());
        }

        [HttpGet]
        [Route("Dashboard/Widget/TimeTerminal")]
        public IHttpActionResult GetTimeTerminals(HttpRequestMessage message)
        {
            int type = message.GetIntValueFromQS("type");
            bool onlyActive = message.GetBoolValueFromQS("onlyActive");
            bool onlyRegistered = message.GetBoolValueFromQS("onlyRegistered");
            bool onlySynchronized = message.GetBoolValueFromQS("onlySynchronized");
            bool loadSettings = message.GetBoolValueFromQS("loadSettings");
            bool loadCompanies = message.GetBoolValueFromQS("loadCompanies");
            bool loadTypeNames = message.GetBoolValueFromQS("loadTypeNames");
            bool ignoreLimitToAccount = message.GetBoolValueFromQS("ignoreLimitToAccount");
            int actorCompId = base.ActorCompanyId;
            if (loadCompanies)
                actorCompId = 0;

            return Content(HttpStatusCode.OK, tsm.GetTimeTerminals(actorCompId, (TimeTerminalType)type, onlyActive, onlyRegistered, onlySynchronized, loadSettings, loadCompanies, loadTypeNames, ignoreLimitToAccount).ToDTOs(false, true, true));
        }

        [HttpGet]
        [Route("Dashboard/Widget/WantedShifts/")]
        public IHttpActionResult GetWantedShiftsForGauge()
        {
            return Content(HttpStatusCode.OK, dm.GetWantedShifts(base.ActorCompanyId, base.UserId, base.RoleId).ToList());
        }

        [HttpGet]
        [Route("Dashboard/Widget/IdsForEmployeeAndGroup")]
        public IHttpActionResult GetIdsForEmployeeAndGroup()
        {
            return Content(HttpStatusCode.OK, em.GetEmployeeIdAndGroupIdForUser(base.UserId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Dashboard/Widget/Insights/{reportId:int}/{dataSelectionId:int}/{columnSelectionId:int}")]
        public IHttpActionResult GetInsightsForWidget(int reportId, int dataSelectionId, int columnSelectionId)
        {
            return Content(HttpStatusCode.OK, dm.GetInsightsForDashboard(this.ActorCompanyId, this.RoleId, this.UserId, reportId, dataSelectionId, columnSelectionId));
        }
        #endregion

        #endregion

        #region Date

        [HttpGet]
        [Route("ServerTime/")]
        public IHttpActionResult GetServerTime()
        {
            return Content(HttpStatusCode.OK, DateTime.Now);
        }

        #endregion

        #region Document

        [HttpGet]
        [Route("Document/NewSince/{time}")]
        public IHttpActionResult HasNewDocumwents(string time)
        {
            return Content(HttpStatusCode.OK, gm.HasNewCompanyDocuments(base.ActorCompanyId, BuildDateTimeFromString(time, false).Value));
        }

        [HttpGet]
        [Route("Document/Company/")]
        public IHttpActionResult GetCompanyDocuments()
        {
            return Content(HttpStatusCode.OK, gm.GetCompanyDocuments(base.ActorCompanyId, base.RoleId, base.UserId, includeUserUploaded: true).ToDocumentDTOs());
        }

        [HttpGet]
        [Route("Document/Company/UnreadCount/")]
        public IHttpActionResult GetNbrOfUnreadCompanyDocuments()
        {
            return Content(HttpStatusCode.OK, gm.GetNbrOfUnreadCompanyDocuments(base.ActorCompanyId, base.RoleId, base.UserId));
        }

        [HttpGet]
        [Route("Document/My/")]
        public IHttpActionResult GetMyDocuments()
        {
            return Content(HttpStatusCode.OK, gm.GetMyDocuments(base.ActorCompanyId, base.RoleId, base.UserId));
        }

        [HttpGet]
        [Route("Document/{dataStorageId:int}")]
        public IHttpActionResult GetDocument(int dataStorageId)
        {
            return Content(HttpStatusCode.OK, gm.GetDataStorage(dataStorageId, base.ActorCompanyId, true).ToDocumentDTO());
        }

        [HttpGet]
        [Route("Document/Url/{dataStorageId:int}")]
        public IHttpActionResult GetDocumentUrl(int dataStorageId)
        {
            return Content(HttpStatusCode.OK, gm.GetDocumentUrl(dataStorageId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Document/Folders")]
        public IHttpActionResult GetDocumentFolders()
        {
            return Content(HttpStatusCode.OK, gm.GetDocumentFolders(base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Document/Data/{dataStorageId:int}")]
        public IHttpActionResult GetDocumentData(int dataStorageId)
        {
            return Content(HttpStatusCode.OK, gm.GetDocumentData(dataStorageId, base.ActorCompanyId));
        }

        [HttpGet]
        [Route("Document/RecipientInfo/{dataStorageId:int}")]
        public IHttpActionResult GetDocumentRecipientInfo(int dataStorageId)
        {
            return Content(HttpStatusCode.OK, gm.GetDocumentRecipientInfo(dataStorageId, base.ActorCompanyId, base.RoleId, base.UserId, true));
        }

        [HttpPost]
        [Route("Document/Upload")]
        public async Task<IHttpActionResult> UploadFile()
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
        [Route("Document/")]
        public IHttpActionResult SaveDocument(SaveDocumentModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, gm.SaveDocument(model.Document, model.FileData, base.ActorCompanyId));
        }

        [HttpPost]
        [Route("Document/SetAsRead/")]
        public IHttpActionResult SetDocumentAsRead(SetDocumentAsReadModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, gm.SetDocumentAsRead(model.DataStorageId, base.UserId, model.Confirmed));
        }

        [HttpDelete]
        [Route("Document/{dataStorageId:int}")]
        public IHttpActionResult DeleteDocument(int dataStorageId)
        {
            return Content(HttpStatusCode.OK, gm.DeleteDocument(dataStorageId, base.ActorCompanyId));
        }

        #endregion

        #region Email

        [HttpGet]
        [Route("EmailTemplates/")]
        public IHttpActionResult GetEmailTemplates()
        {
            return Content(HttpStatusCode.OK, emm.GetEmailTemplates(base.ActorCompanyId).ToDTOs());
        }

        [HttpGet]
        [Route("EmailTemplates/ByType/{type:int}")]
        public IHttpActionResult GetEmailTemplatesByType(int type)
        {
            return Content(HttpStatusCode.OK, emm.GetEmailTemplatesByType(base.ActorCompanyId, type).ToDTOs());
        }

        [HttpGet]
        [Route("EmailTemplate/{emailTemplateId:int}")]
        public IHttpActionResult GetEmailTemplate(int emailTemplateId)
        {
            return Content(HttpStatusCode.OK, emm.GetEmailTemplate(emailTemplateId, base.ActorCompanyId).ToDTO());
        }

        [HttpPost]
        [Route("EmailTemplate")]
        public IHttpActionResult SaveEmailTemplate(EmailTemplateDTO emailTemplate)
        {
            return Content(HttpStatusCode.OK, emm.SaveEmailTemplate(emailTemplate, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("EmailTemplate/{emailTemplateId:int}")]
        public IHttpActionResult DeleteEmailTemplate(int emailTemplateId)
        {
            return Content(HttpStatusCode.OK, emm.DeleteEmailTemplate(emailTemplateId, base.ActorCompanyId));
        }

        #endregion

        #region EventHistory

        [HttpGet]
        [Route("EventHistory/{type:int}/{entity:int}/{recordId:int}/{dateFrom}/{dateTo}/{setNames:bool}")]
        public IHttpActionResult GetEventHistoriesByEntity(int type, int entity, int recordId, string dateFrom, string dateTo, bool setNames)
        {
            return Content(HttpStatusCode.OK, gm.GetEventHistories(base.ActorCompanyId, (TermGroup_EventHistoryType)type, (SoeEntityType)entity, recordId, BuildDateTimeFromString(dateFrom, true).Value, BuildDateTimeFromString(dateTo, true).Value, setNames).ToDTOs());
        }

        [HttpGet]
        [Route("EventHistory/{eventHistoryId:int}/{setNames:bool}")]
        public IHttpActionResult GetEventHistory(int eventHistoryId, bool setNames)
        {
            return Content(HttpStatusCode.OK, gm.GetEventHistory(eventHistoryId, base.ActorCompanyId, setNames).ToDTO());
        }

        [HttpGet]
        [Route("EventHistory/BatchCount/{type:int}/{batchId:int}")]
        public IHttpActionResult GetEventHistory(int type, int batchId)
        {
            return Content(HttpStatusCode.OK, gm.GetNbrOfEventsInBatch(base.ActorCompanyId, (TermGroup_EventHistoryType)type, batchId));
        }

        [HttpPost]
        [Route("EventHistory/")]
        public IHttpActionResult SaveEventHistory(EventHistoryDTO eventHistory)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, gm.SaveEventHistory(eventHistory, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("EventHistory/{eventHistoryId:int}")]
        public IHttpActionResult DeleteEventHistory(int eventHistoryId)
        {
            return Content(HttpStatusCode.OK, gm.DeleteEventHistory(eventHistoryId, base.ActorCompanyId));
        }

        [HttpDelete]
        [Route("EventHistory/Batch/{type:int}/{batchId:int}")]
        public IHttpActionResult DeleteEventHistories(int type, int batchId)
        {
            return Content(HttpStatusCode.OK, gm.DeleteEventHistories((TermGroup_EventHistoryType)type, batchId, base.ActorCompanyId));
        }

        #endregion

        #region ExcelTemplates

        [HttpGet]
        [Route("ExcelTemplates/ProductRows")]
        public IHttpActionResult GetProductRowsExcelTemplate()
        {
            return Content(HttpStatusCode.OK, eim.GetProductRowsExcelTemplate());
        }

        #endregion

        #region Export

        [HttpGet]
        [Route("Export/Grid/{module:int}")]
        public IHttpActionResult GetExportsGrid(int module)
        {
            return Content(HttpStatusCode.OK, iem.GetExports(base.ActorCompanyId, (SoeModule)module).ToGridDTOs());
        }

        [HttpGet]
        [Route("Export/{exportId:int}")]
        public IHttpActionResult GetExport(int exportId)
        {
            return Content(HttpStatusCode.OK, iem.GetExport(base.ActorCompanyId, exportId).ToDTO());
        }

        [HttpPost]
        [Route("Export")]
        public IHttpActionResult SaveExport(ExportDTO exportInput)
        {
            return Content(HttpStatusCode.OK, iem.SaveExport(base.ActorCompanyId, exportInput));
        }

        [HttpDelete]
        [Route("Export/{exportId:int}")]
        public IHttpActionResult DeleteExport(int exportId)
        {
            return Content(HttpStatusCode.OK, iem.DeleteExport(base.ActorCompanyId, exportId));
        }

        #endregion

        #region ExportDefinition

        [HttpGet]
        [Route("ExportDefinition/{addEmptyRow:bool}")]
        public IHttpActionResult GetExportDefinitions(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, iem.GetExportDefinitionsDict(base.ActorCompanyId, addEmptyRow).ToSmallGenericTypes());
        }

        #endregion

        #region ExtraFields

        [HttpGet]
        [Route("ExtraField/{extraFieldId:int}")]
        public IHttpActionResult GetExtraField(int extraFieldId)
        {
            return Content(HttpStatusCode.OK, efm.GetExtraField(extraFieldId).ToDTO());
        }

        [HttpGet]
        [Route("ExtraFields/{entity:int}")]
        public IHttpActionResult GetExtraFields(int entity)
        {
            return Content(HttpStatusCode.OK, efm.GetExtraFields(entity, base.ActorCompanyId, true, loadValues: true).ToGridDTOs());
        }

        [HttpGet]
        [Route("ExtraFields/{entity:int}/{connectedEntity:int}/{connectedRecordId:int}/{addEmptyRow:bool}")]
        public IHttpActionResult GetExtraFields(int entity, int connectedEntity, int connectedRecordId, bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, efm.GetExtraFieldsDict(entity, base.ActorCompanyId, connectedEntity, connectedRecordId, addEmptyRow).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("ExtraFieldGrid/{entity:int}/{loadRecords:bool}/{connectedEntity:int}/{connectedRecordId:int}")]
        public IHttpActionResult GetExtraFieldGridDTOs(int entity, bool loadRecords, int connectedEntity, int connectedRecordId)
        {
            return Content(HttpStatusCode.OK, efm.GetExtraFieldGridDTOs(entity, base.ActorCompanyId, loadRecords, connectedEntity, connectedRecordId));
        }

        [HttpPost]
        [Route("ExtraField")]
        public IHttpActionResult SaveExtraField(ExtraFieldDTO extraField)
        {
            if (!ModelState.IsValid)
            {
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            }
            else
            {
                return Content(HttpStatusCode.OK, efm.SaveExtraField(extraField, base.ActorCompanyId));
            }
        }

        [HttpDelete]
        [Route("ExtraField/{extraFieldId:int}")]
        public IHttpActionResult DeleteExtraField(int extraFieldId)
        {
            return Content(HttpStatusCode.OK, efm.DeleteExtraField(extraFieldId));
        }

        #endregion

        #region ExtraFieldRecord

        [HttpGet]
        [Route("ExtraFieldRecord/{extraFieldId:int}/{recordId:int}/{entity:int}")]
        public IHttpActionResult GetExtraFieldRecord(int extraFieldId, int recordId, int entity)
        {
            return Content(HttpStatusCode.OK, efm.GetExtraFieldRecord(extraFieldId, recordId, entity, base.ActorCompanyId).ToDTO());
        }

        [HttpGet]
        [Route("ExtraFieldsWithRecords/{recordId:int}/{entity:int}/{langId:int}/{connectedEntity:int?}/{connectedRecordId:int?}")]
        public IHttpActionResult GetExtraFieldsWitRecords(int recordId, int entity, int langId, int connectedEntity = 0, int connectedRecordId = 0)
        {
            return Content(HttpStatusCode.OK, efm.GetExtraFieldWithRecords(recordId, entity, base.ActorCompanyId, langId, connectedEntity, connectedRecordId));
        }

        [HttpPost]
        [Route("ExtraFieldsWithRecords")]
        public IHttpActionResult SaveExtraFieldRecords(ExtraFieldRecordsModel model)
        {
            if (!ModelState.IsValid)
            {
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            }
            else
            {
                return Content(HttpStatusCode.OK, efm.SaveExtraFieldRecords(model.records, model.entity, model.recordId, base.ActorCompanyId));
            }
        }

        #endregion

        #region Feature

        [HttpGet]
        [Route("Feature/ReadOnlyPermission")]
        public IHttpActionResult HasReadOnlyPermissions([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] featureIds)
        {
            List<Feature> features = new List<Feature>();
            foreach (int featureId in featureIds)
            {
                features.Add((Feature)featureId);
            }
            if (base.ParameterObject != null)
                base.ParameterObject.SetThread("WebApi");
            return Content(HttpStatusCode.OK, fm.HasRolePermissions(features, Permission.Readonly, base.LicenseId, base.ActorCompanyId, base.RoleId));
        }

        [HttpGet]
        [Route("Feature/ModifyPermission")]
        public IHttpActionResult HasModifyPermissions([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] featureIds)
        {
            List<Feature> features = new List<Feature>();
            foreach (int featureId in featureIds)
            {
                features.Add((Feature)featureId);
            }
            if (base.ParameterObject != null)
                base.ParameterObject.SetThread("WebApi");
            return Content(HttpStatusCode.OK, fm.HasRolePermissions(features, Permission.Modify, base.LicenseId, base.ActorCompanyId, base.RoleId));
        }

        #endregion

        #region GDPR

        [HttpGet]
        [Route("GDPR/WithoutConsent/")]
        public IHttpActionResult GetActorsWithoutConsent()
        {
            return Content(HttpStatusCode.OK, acm.GetActorsWithoutConsent());
        }

        [HttpPost]
        [Route("GDPR/WithoutConsent/")]
        public IHttpActionResult GiveConsent(GDPRHandleInfoModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, acm.GiveConsent(model.Date ?? DateTime.Today, model.Customers, model.Suppliers, model.ContactPersons));
        }

        [HttpPost]
        [Route("GDPR/WithoutConsent/Delete")]
        public IHttpActionResult DeleteActorWithoutConsent(GDPRHandleInfoModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, acm.DeleteActorsWithoutConsent(model.Customers, model.Suppliers, model.ContactPersons));
        }

        #endregion

        #region Help

        [HttpGet]
        [Route("Help/{language}")]
        public IHttpActionResult GetHelpTitles(string language)
        {
            return Ok(hm.GetSysHelpTitles(language));
        }

        [HttpGet]
        [Route("Help/Exists/{feature}/{language}")]
        public IHttpActionResult HasHelp(Feature feature, string language)
        {
            return Content(HttpStatusCode.OK, hm.HasSysHelp(feature, language));
        }

        [HttpGet]
        [Route("Help/{feature}/{language}")]
        public IHttpActionResult GetHelp(Feature feature, string language)
        {
            return Content(HttpStatusCode.OK, hm.GetSysHelp(feature, language).ToSmallDTO());
        }

        [HttpPost]
        [Route("Help/")]
        public IHttpActionResult SaveHelp(SysHelpDTO help)
        {
            return Content(HttpStatusCode.OK, hm.SaveSysHelp(help));
        }


        [HttpGet]
        [Route("Help/Form/{formName}")]
        public IHttpActionResult GetFormHelp(string formName)
        {
            return Content(HttpStatusCode.OK, hm.GetFormHelp(formName).ToSmallDTO());
        }

        #endregion

        #region Images

        [HttpGet]
        [Route("Image/{imageType:int}/{entity:int}/{recordId:int}/{useThumbnails:bool}/{projectId:int}")]
        public IHttpActionResult GetImages(SoeEntityImageType imageType, SoeEntityType entity, int recordId, bool useThumbnails, int projectId)
        {
            List<ImagesDTO> items;

            if (entity == SoeEntityType.Employee)
            {
                items = grm.GetImages(base.ActorCompanyId, imageType, entity, recordId).ToDTOs(useThumbnails).ToList();

                if (imageType == SoeEntityImageType.EmployeePortrait)
                {
                    // Just one employee portrait, do not fetch any more images
                    if (!items.Any())
                    {
                        items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.EmployeePortrait, false, true, loadData: true).ToImagesDTOs(true));
                    }
                }
                else if (imageType == SoeEntityImageType.EmployeeFile)
                {
                    items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, loadConfirmationStatus: true, includeDataStorage: true, includeAttestState: true, loadData: false).ToImagesDTOs(false).OrderByDescending(d => d.Created));
                }
                else
                {
                    items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, loadConfirmationStatus: true, includeDataStorage: true, includeAttestState: true, loadData: false).ToImagesDTOs(false).OrderByDescending(d => d.Created));
                }
            }
            else
            {
                var typeNameGeneral = TermCacheManager.Instance.GetText(7464, 1, "Manuellt tillagd");
                var typeNameSignature = TermCacheManager.Instance.GetText(7465, 1, "Signatur");
                var typeNameSupplierInvoice = TermCacheManager.Instance.GetText(31, 1, "Leverantörsfaktura");
                var typeNameEdi = TermCacheManager.Instance.GetText(7467, 1, "EDI");

                items = grm.GetImages(base.ActorCompanyId, imageType, entity, recordId).ToDTOs(useThumbnails, typeNameGeneral, true).ToList();
                if (entity == SoeEntityType.Order)
                {
                    // Get permission to view supplier invoices
                    bool hasSupplierInvoicesPermission = fm.HasRolePermission(Feature.Billing_Order_SupplierInvoices, Permission.Readonly, base.RoleId, base.ActorCompanyId);

                    var invoice = im.GetCustomerInvoice(recordId);

                    // Add head
                    var signatures = grm.GetImages(base.ActorCompanyId, SoeEntityImageType.OrderInvoiceSignature, entity, recordId).ToDTOs(useThumbnails, typeNameSignature, true).ToList();
                    foreach (var signature in signatures)
                    {
                        if (!items.Any(r => r.ImageId == signature.ImageId && r.SourceType == signature.SourceType))
                            items.Add(signature);
                    }

                    var rows = grm.GetImagesFromOrderRows(base.ActorCompanyId, recordId, false, null, null, false, addToDistribution: invoice?.AddSupplierInvoicesToEInvoices ?? false, hasSupplierInvoicesPermission: hasSupplierInvoicesPermission);
                    foreach (var row in rows)
                    {
                        if (!items.Any(r => r.ImageId == row.ImageId && r.SourceType == row.SourceType))
                        {
                            if (row.IncludeWhenDistributed == null && invoice != null)
                                row.IncludeWhenDistributed = invoice.AddSupplierInvoicesToEInvoices;
                            row.ConnectedTypeName = typeNameSupplierInvoice;
                            items.Add(row);
                        }
                    }

                    if (hasSupplierInvoicesPermission)
                    {
                        var links = grm.GetImagesFromLinkToProject(base.ActorCompanyId, recordId, projectId, false, null, false, addToDistribution: invoice?.AddSupplierInvoicesToEInvoices ?? false);
                        foreach (var link in links)
                        {
                            if (!items.Any(r => r.ImageId == link.ImageId && r.SourceType == link.SourceType))
                            {
                                if (link.IncludeWhenDistributed == null && invoice != null)
                                    link.IncludeWhenDistributed = invoice.AddSupplierInvoicesToEInvoices;
                                link.ConnectedTypeName = typeNameSupplierInvoice;
                                items.Add(link);
                            }
                        }
                    }

                    var records = gm.GetDataStorageRecordsForCustomerInvoice(base.ActorCompanyId, base.RoleId, recordId, SoeEntityType.None, new List<SoeDataStorageRecordType> { SoeDataStorageRecordType.OrderInvoiceFileAttachment, SoeDataStorageRecordType.OrderInvoiceSignature }, invoice?.AddSupplierInvoicesToEInvoices ?? false);
                    foreach (var record in records)
                    {
                        if (!items.Any(r => r.ImageId == record.ImageId && r.SourceType == record.SourceType))
                        {
                            if (record.IncludeWhenDistributed == null && invoice != null)
                                record.IncludeWhenDistributed = invoice.AddAttachementsToEInvoice;
                            items.Add(record);
                        }
                    }

                    // Add children
                    var mappings = im.GetChildInvoices(recordId);
                    foreach (var mapping in mappings)
                    {
                        var typeNameChild = TermCacheManager.Instance.GetText(7469, 1, "underorder");

                        var childImages = grm.GetImages(base.ActorCompanyId, imageType, entity, mapping.ChildInvoiceId).ToDTOs(useThumbnails, typeNameGeneral + " (" + typeNameChild + ")", true).ToList();
                        foreach (var images in childImages)
                        {
                            if (!items.Any(r => r.ImageId == images.ImageId && r.SourceType == images.SourceType))
                                items.Add(images);
                        }

                        var childSignatures = grm.GetImages(base.ActorCompanyId, SoeEntityImageType.OrderInvoiceSignature, entity, mapping.ChildInvoiceId).ToDTOs(useThumbnails, typeNameSignature + " (" + typeNameChild + ")", true).ToList();
                        foreach (var signature in childSignatures)
                        {
                            if (!items.Any(r => r.ImageId == signature.ImageId && r.SourceType == signature.SourceType))
                                items.Add(signature);
                        }

                        var childRows = grm.GetImagesFromOrderRows(base.ActorCompanyId, mapping.ChildInvoiceId, false, typeNameEdi + " (" + typeNameChild + ")", null, false, mapping.MainInvoiceId);
                        foreach (var row in childRows)
                        {
                            if (!items.Any(r => r.ImageId == row.ImageId && r.SourceType == row.SourceType))
                            {
                                if (row.IncludeWhenDistributed == null)
                                    row.IncludeWhenDistributed = invoice.AddSupplierInvoicesToEInvoices;
                                row.ConnectedTypeName = typeNameSupplierInvoice + " (" + typeNameChild + ")";
                                items.Add(row);
                            }
                        }

                        var childLinks = grm.GetImagesFromLinkToProject(base.ActorCompanyId, mapping.ChildInvoiceId, projectId, false, null, false, mapping.MainInvoiceId);
                        foreach (var link in childLinks)
                        {
                            if (!items.Any(r => r.ImageId == link.ImageId && r.SourceType == link.SourceType))
                            {
                                if (link.IncludeWhenDistributed == null)
                                    link.IncludeWhenDistributed = invoice.AddSupplierInvoicesToEInvoices;
                                link.ConnectedTypeName = typeNameSupplierInvoice + " (" + typeNameChild + ")";
                                items.Add(link);
                            }
                        }

                        var childRecords = gm.GetDataStorageRecordsForCustomerInvoice(base.ActorCompanyId, base.RoleId, mapping.ChildInvoiceId, SoeEntityType.None, new List<SoeDataStorageRecordType> { SoeDataStorageRecordType.OrderInvoiceFileAttachment, SoeDataStorageRecordType.OrderInvoiceSignature }, invoice?.AddSupplierInvoicesToEInvoices ?? false, mapping.MainInvoiceId);
                        foreach (var record in childRecords)
                        {
                            if (!items.Any(r => r.ImageId == record.ImageId && r.SourceType == record.SourceType))
                            {
                                if (record.IncludeWhenDistributed == null)
                                    record.IncludeWhenDistributed = invoice.AddAttachementsToEInvoice;
                                items.Add(record);
                            }
                        }
                    }
                }
                else if (entity == SoeEntityType.CustomerInvoice)
                {
                    var invoice = im.GetCustomerInvoice(recordId);

                    var records = gm.GetDataStorageRecordsForCustomerInvoice(base.ActorCompanyId, base.RoleId, recordId, SoeEntityType.None, new List<SoeDataStorageRecordType> { SoeDataStorageRecordType.OrderInvoiceFileAttachment, SoeDataStorageRecordType.InvoicePdf, SoeDataStorageRecordType.InvoiceBitmap, SoeDataStorageRecordType.OrderInvoiceSignature }, invoice?.AddSupplierInvoicesToEInvoices ?? false);
                    foreach (var record in records)
                    {
                        if (!items.Any(r => r.ImageId == record.ImageId && r.SourceType == record.SourceType))
                        {
                            if (record.IncludeWhenDistributed == null && invoice != null)
                                record.IncludeWhenDistributed = invoice.AddAttachementsToEInvoice;
                            items.Add(record);
                        }
                    }
                }                
                else if (entity == SoeEntityType.Offer || entity == SoeEntityType.Contract)
                {
                    var invoice = im.GetCustomerInvoice(recordId);

                    var records = gm.GetDataStorageRecordsForCustomerInvoice(base.ActorCompanyId, base.RoleId, recordId, SoeEntityType.None, new List<SoeDataStorageRecordType> { SoeDataStorageRecordType.OrderInvoiceFileAttachment }, invoice?.AddSupplierInvoicesToEInvoices ?? false);
                    foreach (var record in records)
                    {
                        if (!items.Any(r => r.ImageId == record.ImageId && r.SourceType == record.SourceType))
                        {
                            if (record.IncludeWhenDistributed == null && invoice != null)
                                record.IncludeWhenDistributed = invoice.AddAttachementsToEInvoice;
                            items.Add(record);
                        }
                    }
                }
                else if (entity == SoeEntityType.Voucher)
                {
                    items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.VoucherFileAttachment, includeInvoiceAttachment: true).ToImagesDTOs(false));
                }
                else if (entity == SoeEntityType.Inventory)
                {
                    items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.InventoryFileAttachment).ToImagesDTOs(false));
                }
                else if (entity == SoeEntityType.Supplier)
                {
                    items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.SupplierFileAttachment).ToImagesDTOs(false));
                }
                else if (entity == SoeEntityType.Expense)
                {
                    items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.Expense).ToImagesDTOs(false));
                }
                else if (entity == SoeEntityType.Customer)
                {
                    items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.CustomerFileAttachment).ToImagesDTOs(false));
                }
                else if (entity == SoeEntityType.ChecklistHeadRecord)
                {
                    items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, entity, SoeDataStorageRecordType.ChecklistHeadRecord).ToImagesDTOs(false));
                }
                else
                {
                    items.AddRange(gm.GetDataStorageRecords(base.ActorCompanyId, base.RoleId, recordId, SoeEntityType.None, SoeDataStorageRecordType.OrderInvoiceFileAttachment).ToImagesDTOs(false));
                }
            }

            return Content(HttpStatusCode.OK, items);
        }

        [HttpGet]
        [Route("Image/{imageId}")]
        public IHttpActionResult GetImage(int imageId)
        {
            var image = grm.GetImage(imageId);
            if (image != null)
            {
                return Content(HttpStatusCode.OK, image.ToDTO(false));
            }
            else
            {
                var record = gm.GetDataStorageRecord(base.ActorCompanyId, imageId);
                return Content(HttpStatusCode.OK, record.ToImagesDTO(true));
            }
        }

        #endregion

        #region Information

        [HttpGet]
        [Route("Information/NewSince/{time}")]
        public IHttpActionResult HasNewInformations(string time)
        {
            return Content(HttpStatusCode.OK, gm.HasNewInformations(base.ActorCompanyId, BuildDateTimeFromString(time, false).Value));
        }

        [HttpGet]
        [Route("Information/UnreadCount/{language}")]
        public IHttpActionResult GetNbrOfUnreadInformations(string language)
        {
            return Content(HttpStatusCode.OK, gm.GetNbrOfUnreadInformations(base.LicenseId, base.ActorCompanyId, base.RoleId, base.UserId, true, false, false, lm.GetSysLanguageId(language)));
        }

        [HttpGet]
        [Route("Information/Unread/{language}")]
        public IHttpActionResult GetUnreadInformations(string language)
        {
            return Content(HttpStatusCode.OK, gm.GetUnreadInformations(base.LicenseId, base.ActorCompanyId, base.RoleId, base.UserId, true, false, false, lm.GetSysLanguageId(language)));
        }

        [HttpGet]
        [Route("Information/Unread/Severe/{language}")]
        public IHttpActionResult HasSevereUnreadInformation(string language)
        {
            return Content(HttpStatusCode.OK, gm.HasSevereUnreadInformation(base.LicenseId, base.ActorCompanyId, base.RoleId, base.UserId, true, false, false, lm.GetSysLanguageId(language)));
        }

        [HttpGet]
        [Route("Information/Company/{language}")]
        public IHttpActionResult GetCompanyInformations(string language)
        {
            return Content(HttpStatusCode.OK, gm.GetCompanyInformations(base.ActorCompanyId, base.RoleId, base.UserId, true, false, false, lm.GetSysLanguageId(language)));
        }

        [HttpGet]
        [Route("Information/Company/{language}/{informationId:int}")]
        public IHttpActionResult GetCompanyInformation(string language, int informationId)
        {
            return Content(HttpStatusCode.OK, gm.GetCompanyInformation(informationId, base.ActorCompanyId, base.UserId, false));
        }

        [HttpGet]
        [Route("Information/Sys/{language}")]
        public IHttpActionResult GetSysInformations(string language)
        {
            return Content(HttpStatusCode.OK, gm.GetSysInformations(base.LicenseId, base.ActorCompanyId, base.RoleId, base.UserId, true, false, false, lm.GetSysLanguageId(language)));
        }

        [HttpGet]
        [Route("Information/Sys/{language}/{sysInformationId:int}")]
        public IHttpActionResult GetSysInformation(string language, int sysInformationId)
        {
            return Content(HttpStatusCode.OK, gm.GetSysInformation(sysInformationId, false));
        }

        [HttpPost]
        [Route("Information/SetAsRead/")]
        public IHttpActionResult SetInformationAsRead(SetInformationAsReadModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, gm.SetInformationAsRead(model.InformationId, model.SysInformationId, base.UserId, model.Confirmed, model.Hidden));
        }

        #endregion

        #region Files

        [HttpPost]
        [Route("Files/DataStorage/ConnectToEntity/{dataStorageRecordId:int}/{recordId:int}/{removeExisting:bool}/{entity:int}/{storageRecordType:int}")]
        public IHttpActionResult ConnectImageToEntity(int dataStorageRecordId, int recordId, bool removeExisting, int entity, SoeDataStorageRecordType storageRecordType)
        {
            if (removeExisting)
            {
                gm.DeleteDataStorageRecord(recordId, storageRecordType);
            }
            return Content(HttpStatusCode.OK, gm.UpdateDataStorageRecord(dataStorageRecordId, recordId));
        }

        [HttpDelete]
        [Route("Files/DataStorage/{recordId:int}/{storageRecordType:int}")]
        public IHttpActionResult DeleteDataStorage(int recordId, SoeDataStorageRecordType storageRecordType)
        {
            return Content(HttpStatusCode.OK, gm.DeleteDataStorageRecord(recordId, storageRecordType));
        }

        [HttpGet]
        [Route("Files/Exist/{entity:int}/{recordId:int}/{fileName}")]
        public IHttpActionResult FileExists(SoeEntityType entity, int recordId, string fileName)
        {
            return Content(HttpStatusCode.OK, gm.GetDataStorageRecordId(base.ActorCompanyId, entity, recordId, fileName));
        }

        [HttpPost]
        [Route("Files/Existing/")]
        public IHttpActionResult ExistingFiles(FilesLookupDTO model)
        {
            return Content(HttpStatusCode.OK, gm.GetExistingFiles(base.ActorCompanyId, model.Entity, model.Files));
        }

        [HttpGet]
        [Route("Files/RoleIds/{dataStorageRecordId:int}")]
        public IHttpActionResult GetFileRoleIds(int dataStorageRecordId)
        {
            return Content(HttpStatusCode.OK, gm.GetDataStorageRecordRoleIds(dataStorageRecordId));
        }

        [HttpPost]
        [Route("Files/RoleIds/{dataStorageRecordId:int}/{roleIds}")]
        public IHttpActionResult UpdateFileRoleIds(int dataStorageRecordId, string roleIds)
        {
            return Content(HttpStatusCode.OK, gm.UpdateDataStorageRecordRoleIds(base.ActorCompanyId, dataStorageRecordId, StringUtility.SplitNumericList(roleIds)));
        }



        [HttpPost]
        [Route("Files/Intrastat/CommodityCodes/{year:int}")]
        public async Task<IHttpActionResult> UploadCommodityCodesFile(int year)
        {
            string fileName = "";
            string pathOnServer = "";
            var file = await UploadedFileHandler.HandleAsync(Request);
            var result = new ActionResult();
            if (file.Data.Length > 0)
            {
                //has content
                var startingDate = new DateTime(year, 1, 1);

                //Validate
                fileName = eim.ValidatePostedFile(file.FileName, true);

                var extention = Path.GetExtension(file.FileName);
                if (!(extention == ".csv" || extention == ".xlsx" || extention == ".xls"))
                {
                    result.Success = false;
                    result.ErrorMessage = eim.GetFileNotSupportedMessage();
                    return Ok(result);
                }
                if (!(extention == ".csv"))
                {
                    fileName = Path.ChangeExtension(fileName, ".csv");
                }
                //Save temp-file
                pathOnServer = eim.SaveTempFileToServer(file.Data, fileName);

                result = eim.ImportCommodityCodes(pathOnServer, startingDate, extention);
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = eim.GetFileIsEmptyMessage();
            }

            result.StringValue = file.FileName;
            return Ok(result);

        }

        [HttpPost]
        [Route("Files/Invoice/{entity:int}")]
        public async Task<IHttpActionResult> UploadInvoiceFile(SoeEntityType entity)
        {
            var file = await UploadedFileHandler.HandleAsync(Request);

            var record = new DataStorageRecordExtendedDTO
            {
                Data = file.Data,
                Type = GetDataStorageType(file),
                Entity = entity,
                Description = file.FileName,
                RecordNumber = file.FileName,
                RecordId = 0
            };
            var result = gm.SaveDataStorageRecord(ActorCompanyId, record, false);
            result.StringValue = file.FileName;
            return Ok(result);
        }

        [HttpPost]
        [Route("Files/{entity:int}/{type:int}/{recordId}")]
        //extractZip is optional, append uri with ?extractZip=true to use.
        public async Task<IHttpActionResult> UploadInvoiceFile(SoeEntityType entity, SoeEntityImageType type, int recordId, bool extractZip = false)
        {
            var file = await UploadedFileHandler.HandleAsync(Request);
            if (extractZip && IsZipFile(file.Data, file.FileName))
            {
                var actionResults = new List<ActionResult>();
                var extractedFiles = UnzipFilesInZipFile(file.Data);
                foreach (var extractedFile in extractedFiles)
                {
                    var httpFile = new UploadedFileHandler.HttpFile(extractedFile.Key, extractedFile.Value, ImageFormatType.NONE);
                    var actionResult = SaveFileData(entity, type, recordId, httpFile);
                    actionResults.Add(actionResult);
                }
                return Ok(actionResults);
            }
            else
            {
                var actionResult = SaveFileData(entity, type, recordId, file);
                return Ok(actionResult);
            }
        }

        private ActionResult SaveFileData(SoeEntityType entity, SoeEntityImageType type, int recordId, UploadedFileHandler.HttpFile file)
        {
            var record = new DataStorageRecordExtendedDTO
            {
                Data = file.Data,
                Type = GetDataStorageType(file),
                Entity = entity,
                Description = file.FileName,
                RecordNumber = file.FileName,
                RecordId = recordId
            };
            var result = gm.SaveDataStorageRecord(ActorCompanyId, record, false);
            result.StringValue = file.FileName;
            return result;
        }


        [HttpPost]
        [Route("Files/{entity:int}/{type:int}/{recordId:int}/{roles}/{messageGroups}")]
        public async Task<IHttpActionResult> UploadFileWithRolesAndMessageGroups(SoeEntityType entity, SoeEntityImageType type, int recordId, string roles, string messageGroups)
        {
            var file = await UploadedFileHandler.HandleAsync(Request);

            List<int> roleIds = StringUtility.SplitNumericList(roles);
            List<int> messageGroupIds = StringUtility.SplitNumericList(messageGroups);

            DataStorageRecordExtendedDTO record = new DataStorageRecordExtendedDTO
            {
                Data = file.Data,
                Type = SoeDataStorageRecordType.UploadedFile,
                Entity = entity,
                Description = file.FileName,
                RecordNumber = file.FileName,
                RecordId = recordId
            };
            ActionResult result = gm.SaveDataStorageRecord(ActorCompanyId, record, false, roleIds, messageGroupIds);
            result.StringValue = file.FileName;
            return Ok(result);
        }

        [HttpPost]
        [Route("Files/GetArray/")]
        public async Task<IHttpActionResult> GetByteArray()
        {
            var item = await UploadedFileHandler.HandleAsync(Request);
            var result = new ActionResult(item.Data != null);
            result.StringValue = item.FileName;
            result.Value = item.Data;
            return Ok(result);
        }

        private SoeDataStorageRecordType GetDataStorageType(UploadedFileHandler.HttpFile file)
        {
            switch (file.Type)
            {
                case ImageFormatType.JPG:
                case ImageFormatType.PNG:
                    return SoeDataStorageRecordType.InvoiceBitmap;
                case ImageFormatType.PDF:
                    return SoeDataStorageRecordType.InvoicePdf;
                default:
                    return SoeDataStorageRecordType.Unknown;
            }
        }

        [HttpPost]
        [Route("Files/{entity:int}/{type:int}")]
        public async Task<IHttpActionResult> UploadFile(SoeEntityType entity, SoeEntityImageType type)
        {
            var file = await UploadedFileHandler.HandleAsync(Request);
            if (
                 (file.Type == ImageFormatType.NONE || file.Type == ImageFormatType.PDF) ||
                 (entity == SoeEntityType.Voucher || entity == SoeEntityType.Inventory || entity == SoeEntityType.SupplierInvoice || entity == SoeEntityType.Supplier || entity == SoeEntityType.Expense || entity == SoeEntityType.Customer) ||
                 (entity == SoeEntityType.Offer || entity == SoeEntityType.Order || entity == SoeEntityType.Contract || entity == SoeEntityType.CustomerInvoice) ||
                 (entity == SoeEntityType.Employee && type == SoeEntityImageType.EmployeePortrait) ||
                 (entity == SoeEntityType.ChecklistHeadRecord && type == SoeEntityImageType.ChecklistHeadRecord)
               )
            {
                var recordEntity = entity;
                var recordType = SoeDataStorageRecordType.OrderInvoiceFileAttachment; //very strange!!
                switch (entity)
                {
                    case SoeEntityType.Voucher:
                        recordType = SoeDataStorageRecordType.VoucherFileAttachment;
                        break;
                    case SoeEntityType.Inventory:
                        recordType = SoeDataStorageRecordType.InventoryFileAttachment;
                        break;
                    case SoeEntityType.Employee:
                        recordType = SoeDataStorageRecordType.EmployeePortrait;
                        break;
                    case SoeEntityType.Supplier:
                        recordType = SoeDataStorageRecordType.SupplierFileAttachment;
                        break;
                    case SoeEntityType.Expense:
                        recordType = SoeDataStorageRecordType.Expense;
                        break;
                    case SoeEntityType.Customer:
                        recordType = SoeDataStorageRecordType.CustomerFileAttachment;
                        break;
                    case SoeEntityType.ChecklistHeadRecord:
                        recordType = SoeDataStorageRecordType.ChecklistHeadRecord;
                        break;
                    default:
                        recordEntity = SoeEntityType.None;
                        break;
                }

                var record = new DataStorageRecordExtendedDTO
                {
                    Data = file.Data,
                    Type = recordType,
                    Entity = recordEntity,
                    Description = file.FileName,
                    RecordNumber = file.FileName,
                    RecordId = 0
                };

                var result = gm.SaveDataStorageRecord(ActorCompanyId, record, false);
                result.StringValue = file.FileName;

                if (entity == SoeEntityType.Employee && type == SoeEntityImageType.EmployeePortrait && result.IntegerValue > 0)
                {
                    result.StringValue = "image";
                    result.Value = gm.GetDataStorageRecord(ActorCompanyId, result.IntegerValue).ToImagesDTO(true);
                }

                return Ok(result);
            }

            var imagesDto = new ImagesDTO
            {
                Type = type,
                FileName = file.FileName,
                Description = file.FileName,
                Image = file.Data,
                FormatType = file.Type
            };
            return Ok(grm.SaveImageDTO(imagesDto, entity, ActorCompanyId));
        }

        public class UploadedFileHandler
        {
            private static readonly IDictionary<string, ImageFormatType> Types = new Dictionary<string, ImageFormatType>
            {
                { "image/jpg", ImageFormatType.JPG },
                { "image/jpeg", ImageFormatType.JPG },
                { "image/png", ImageFormatType.PNG },
                { "application/pdf", ImageFormatType.PDF }
            };

            public static async Task<HttpFile> HandleAsync(HttpRequestMessage message)
            {
                if (!message.Content.IsMimeMultipartContent())
                    throw new HttpResponseException(message.CreateResponse(HttpStatusCode.NotAcceptable, "This request is not properly formatted"));

                var multipart = await message.Content.ReadAsMultipartAsync();
                var uploadedFile = multipart.Contents.SingleOrDefault();
                if (uploadedFile == null)
                    throw new HttpResponseException(message.CreateResponse(HttpStatusCode.NotAcceptable, "This content of the file is null"));

                return new HttpFile(
                    uploadedFile.Headers.ContentDisposition.FileName.Trim('"'),
                    await uploadedFile.ReadAsByteArrayAsync(),
                    GetImageType(uploadedFile.Headers.ContentType.MediaType)
                );
            }

            public static async Task<byte[]> HandleAsyncGetByteArray(HttpRequestMessage message)
            {
                if (!message.Content.IsMimeMultipartContent())
                    throw new HttpResponseException(message.CreateResponse(HttpStatusCode.NotAcceptable, "This request is not properly formatted"));

                var multipart = await message.Content.ReadAsMultipartAsync();
                var uploadedFile = multipart.Contents.SingleOrDefault();
                if (uploadedFile == null)
                    throw new HttpResponseException(message.CreateResponse(HttpStatusCode.NotAcceptable, "This content of the file is null"));

                return await uploadedFile.ReadAsByteArrayAsync();
            }

            private static ImageFormatType GetImageType(string mediaType)
            {
                return Types.ContainsKey(mediaType) ? Types[mediaType] : ImageFormatType.NONE;
            }

            public class HttpFile
            {
                public string FileName { get; private set; }
                public byte[] Data { get; private set; }
                public ImageFormatType Type { get; private set; }

                public HttpFile(string fileName, byte[] data, ImageFormatType type)
                {
                    FileName = fileName;
                    Data = data;
                    Type = type;
                }
            }
        }

        #endregion

        #region LinkedShifts

        [HttpGet]
        [Route("LinkedShifts/{shiftId:int}")]
        public IHttpActionResult GetLinkedShifts(int shiftId)
        {
            return Content(HttpStatusCode.OK, tscm.GetLinkedTimeScheduleTemplateBlocks(null, base.ActorCompanyId, shiftId, true).ToDTOs());
        }

        #endregion

        #region OriginUsers

        [HttpPost]
        [Route("OriginUsers/")]
        public IHttpActionResult SaveOriginUsers(OriginUsersModel model)
        {
            return null;
            /*if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, im.SaveOriginUsers(model.OriginId, model.OriginUsers, base.ActorCompanyId, base.LicenseId, model.SendXEMail, model.Subject, model.Text));*/
        }

        #endregion

        #region Payment Information

        [HttpGet]
        [Route("PaymentInformation/PaymentInformationFromActor/")]
        public IHttpActionResult GetPaymentInformationFromActor([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] actorIds)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentInformationRowsFromActors(actorIds.ToList()));
        }

        [HttpGet]
        [Route("PaymentInformation/PaymentInformationForPaymentMethod/{paymentMethodId:int}")]
        public IHttpActionResult GetPaymentInformationFromActor(int paymentMethodId, [ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] actorIds)
        {
            return Content(HttpStatusCode.OK, pm.GetPaymentInformationRowsFromActorsFilterByPaymentMethod(actorIds.ToList(), paymentMethodId));
        }

        [HttpGet]
        [Route("PaymentInformation/GetBicFromIban/{iban}")]
        public IHttpActionResult GetPaymentInformationFromActor(string iban)
        {
            return Content(HttpStatusCode.OK, pm.GetBicFromIban(iban));
        }

        #endregion

        #region PersonalDataLog

        [HttpGet]
        [Route("PersonalDataLogs/Employee/{employeeId:int}/{informationType:int}/{actionType:int}/{dateFrom}/{dateTo}/{take:int}")]
        public IHttpActionResult GetPersonalDataLogsForEmployee(int employeeId, TermGroup_PersonalDataInformationType informationType, TermGroup_PersonalDataActionType actionType, string dateFrom, string dateTo, int take)
        {
            return Content(HttpStatusCode.OK, lgm.GetPersonalDataLogs(employeeId, TermGroup_PersonalDataType.Employee, informationType, actionType, BuildDateTimeFromString(dateFrom, true, CalendarUtility.DATETIME_DEFAULT).Value, BuildDateTimeFromString(dateTo, true, CalendarUtility.DATETIME_DEFAULT).Value, take.ToNullable()));
        }

        [HttpGet]
        [Route("PersonalDataLogs/CausedByEmployee/{employeeId:int}/{recordId:int}/{informationType:int}/{actionType:int}/{dateFrom}/{dateTo}/{take:int}")]
        public IHttpActionResult GetPersonalDataLogsCausedByEmployee(int employeeId, int? recordId, TermGroup_PersonalDataInformationType informationType, TermGroup_PersonalDataActionType actionType, string dateFrom, string dateTo, int take)
        {
            return Content(HttpStatusCode.OK, lgm.GetPersonalDataLogsCausedByEmployee(employeeId, recordId.ToNullable(), TermGroup_PersonalDataType.Employee, informationType, actionType, BuildDateTimeFromString(dateFrom, true, CalendarUtility.DATETIME_DEFAULT).Value, BuildDateTimeFromString(dateTo, true, CalendarUtility.DATETIME_DEFAULT).Value, take.ToNullable()));
        }

        [HttpGet]
        [Route("PersonalDataLogs/CausedByUser/{userId:int}/{recordId:int}/{informationType:int}/{actionType:int}/{dateFrom}/{dateTo}/{take:int}")]
        public IHttpActionResult GetPersonalDataLogsCausedByUser(int userId, int? recordId, TermGroup_PersonalDataInformationType informationType, TermGroup_PersonalDataActionType actionType, string dateFrom, string dateTo, int take)
        {
            return Content(HttpStatusCode.OK, lgm.GetPersonalDataLogsCausedByUser(userId, recordId.ToNullable(), TermGroup_PersonalDataType.User, informationType, actionType, BuildDateTimeFromString(dateFrom, true, CalendarUtility.DATETIME_DEFAULT).Value, BuildDateTimeFromString(dateTo, true, CalendarUtility.DATETIME_DEFAULT).Value, take.ToNullable()));
        }

        [HttpGet]
        [Route("SearchPerson/{searchstring}/{searchEntities}")]
        public IHttpActionResult SearchPerson([FromUri] string searchstring, [FromUri] string searchEntities)
        {
            return Content(HttpStatusCode.OK, acm.SearchPerson(searchstring, searchEntities));
        }

        #endregion

        #region ProcessInfo

        [HttpGet]
        [Route("ProgressInfo/{key}")]
        public IHttpActionResult GetProgressInfo(string key)
        {
            Guid guid = Guid.Parse(key);
            return Content(HttpStatusCode.OK, monitor.GetInfo(guid));
        }

        #endregion

        #region RecurrenceInterval

        [HttpGet]
        [Route("RecurrenceInterval/Text/{recurrenceInterval}")]
        public IHttpActionResult GetRecurrenceIntervalText(string recurrenceInterval)
        {
            return Content(HttpStatusCode.OK, calm.GetRecurrenceIntervalText(ParseRecurrenceIntervalString(recurrenceInterval)));
        }

        [HttpGet]
        [Route("RecurrenceInterval/NextExecutionTime/{recurrenceInterval}")]
        public IHttpActionResult GetNextExecutionTime(string recurrenceInterval)
        {
            return Content(HttpStatusCode.OK, SchedulerUtility.GetNextExecutionTime(ParseRecurrenceIntervalString(recurrenceInterval)));
        }

        private string ParseRecurrenceIntervalString(string recurrenceInterval)
        {
            // Can't send * in query string, replace with |
            return Regex.Replace(recurrenceInterval, @"\|", "*");
        }

        #endregion

        #region SequenceNumber

        [HttpGet]
        [Route("SequenceNumber/LastUsed/{entityName}")]
        public IHttpActionResult GetLastUsedSequenceNumber(string entityName)
        {
            return Content(HttpStatusCode.OK, sqm.GetLastUsedSequenceNumber(base.ActorCompanyId, entityName));
        }

        #endregion

        #region StateAnalysis

        [HttpGet]
        [Route("StateAnalysis")]
        public IHttpActionResult GetStateAnalysis([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] analysisIds)
        {
            List<SoeStatesAnalysis> analysis = new List<SoeStatesAnalysis>();
            foreach (int analysisId in analysisIds)
            {
                analysis.Add((SoeStatesAnalysis)analysisId);
            }
            return Content(HttpStatusCode.OK, am.GetStateAnalysis(analysis, base.ActorCompanyId, base.RoleId));
        }

        #endregion

        #region SysCountry

        [HttpGet]
        [Route("SysCountry/{addEmptyRow:bool}/{onlyUsedLanguages:bool}")]
        public IHttpActionResult GetSysCountries(bool addEmptyRow, bool onlyUsedLanguages)
        {
            return Content(HttpStatusCode.OK, ccm.GetSysCountriesDict(addEmptyRow, onlyUsedLanguages).ToSmallGenericTypes());
        }

        #endregion

        #region SysGridState

        [HttpGet]
        [Route("SysGridState/{grid}")]
        public IHttpActionResult GetSysGridState(string grid)
        {
            return Content(HttpStatusCode.OK, sm.GetSysGridStateValue(grid));
        }

        [HttpPost]
        [Route("SysGridState")]
        public IHttpActionResult SaveSysGridState(SaveUserGridStateModel model)
        {
            return Content(HttpStatusCode.OK, sm.SaveSysGridState(model.Grid, model.GridState, base.UserId));
        }

        [HttpDelete]
        [Route("SysGridState/{grid}")]
        public IHttpActionResult DeleteSysGridState(string grid)
        {
            return Content(HttpStatusCode.OK, sm.DeleteSysGridState(grid, base.UserId));
        }

        #endregion

        #region SysLanguage

        [HttpGet]
        [Route("SysLanguage/{addEmptyRow:bool}")]
        public IHttpActionResult GetSysLanguages(bool addEmptyRow)
        {
            return Content(HttpStatusCode.OK, lm.GetSysLanguageDict(addEmptyRow).ToSmallGenericTypes());
        }

        #endregion

        #region SysLog

        [HttpPost]
        [Route("SysLog/Add")]
        public IHttpActionResult AddSysLogErrorMessage(AddSysLogErrorMessageModel model)
        {
            if (model.IsWarning)
                slm.AddSysLogWarningMessage("", "WebApi", model.Message);
            else
                slm.AddSysLogErrorMessage("", "WebApi", model.Message, new Exception(model.Exception), model.RequestUri);
            return Content(HttpStatusCode.OK, "");
        }

        #endregion

        #region SysPosition

        [HttpGet]
        [Route("SysPosition/{sysCountryId:int}/{sysLanguageId:int}")]
        public IHttpActionResult GetSysPositions(int sysCountryId, int sysLanguageId)
        {
            return Content(HttpStatusCode.OK, em.GetSysPositions(base.ActorCompanyId, sysCountryId, sysLanguageId).ToGridDTOs());
        }

        [HttpGet]
        [Route("SysPosition/{sysPositionId:int}")]
        public IHttpActionResult GetSysPosition(int sysPositionId)
        {
            return Content(HttpStatusCode.OK, em.GetSysPosition(sysPositionId).ToDTO());
        }

        [HttpGet]
        [Route("SysPosition/Grid")]
        public IHttpActionResult GetSysPositionsGrid()
        {
            return Content(HttpStatusCode.OK, em.GetSysPositions(null, null, null).ToGridDTOs());
        }

        [HttpPost]
        [Route("SysPosition")]
        public IHttpActionResult SaveSysPosition(SysPositionDTO sysPosition)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, em.SaveSysPosition(sysPosition));
        }

        [HttpDelete]
        [Route("SysPosition/{sysPositionId:int}")]
        public IHttpActionResult DeleteSysPosition(int sysPositionId)
        {
            return Content(HttpStatusCode.OK, em.DeleteSysPosition(sysPositionId));
        }

        #endregion

        #region SysTerm

        [HttpGet]
        [Route("SysTerm/{translationKey}")]
        public IHttpActionResult GetSysTerm(string translationKey)
        {
            return Content(HttpStatusCode.OK, TermCacheManager.Instance.GetText(translationKey));
        }

        #endregion

        #region SysTermGroup

        [HttpGet]
        [Route("SysTermGroup/{sysTermGroupId:int}/{addEmptyRow:bool}/{skipUnknown:bool}/{sortById:bool}")]
        public IHttpActionResult GetTermGroupContent(int sysTermGroupId, bool addEmptyRow, bool skipUnknown, bool sortById)
        {
            return Content(HttpStatusCode.OK, base.GetTermGroupContent((TermGroup)sysTermGroupId, addEmptyRow, skipUnknown, sortById).ToSmallGenericTypes());
        }

        #endregion

        #region TextBlock

        [HttpGet]
        [Route("TextBlock/{textBlockId:int}")]
        public IHttpActionResult GetTextBlock(int textBlockId)
        {
            return Content(HttpStatusCode.OK, gm.GetTextblock(textBlockId).ToDTO());
        }

        [HttpGet]
        [Route("TextBlocks/{entity:int}")]
        public IHttpActionResult GetTextBlocks(int entity)
        {
            return Content(HttpStatusCode.OK, gm.GetTextblocks(entity, base.ActorCompanyId).ToDTOs());
        }

        [HttpPost]
        [Route("TextBlock")]
        public IHttpActionResult SaveTextBlock(TextBlockModel textBlockModel)
        {
            if (!ModelState.IsValid)
            {
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            }
            else
            {
                textBlockModel.TextBlock.ActorCompanyId = this.ActorCompanyId;
                return Content(HttpStatusCode.OK, gm.SaveTextblock(textBlockModel.TextBlock, textBlockModel.Entity, textBlockModel.translations));
            }
        }

        [HttpDelete]
        [Route("TextBlock/{textBlockId:int}")]
        public IHttpActionResult DeleteTextBlock(int textBlockId)
        {
            return Content(HttpStatusCode.OK, gm.DeleteTextblock(textBlockId));
        }

        #endregion

        #region TimeTransactions

        [HttpGet]
        [Route("ProjectTimeInvoiceTransactions/{projectId:int}")]
        public IHttpActionResult GetProjectTimeInvoiceTransactions(int projectId)
        {
            return Content(HttpStatusCode.OK, ttm.GetProjectTimeInvoiceTransactionItems(base.ActorCompanyId, projectId));
        }

        [HttpGet]
        [Route("Project/TimeBlock/LastDate/{projectId:int}/{recordId:int}/{recordType:int}/{employeeId:int}")]
        public IHttpActionResult GetProjectTimeBlocksLastDate(int projectId, int recordId, int recordType, int employeeId)
        {
            return Content(HttpStatusCode.OK, prm.GetProjectTimeBlocksLastDate(projectId, recordId, recordType, employeeId));
        }

        [HttpGet]
        [Route("Project/TimeBlock/{projectId:int}/{recordId:int}/{recordType:int}/{employeeId:int}/{loadOnlyForEmployee:bool}/{vatType:int}/{dateFrom}/{dateTo}")]
        public IHttpActionResult GetProjectTimeBlocks(int projectId, int recordId, int recordType, int employeeId, bool loadOnlyForEmployee, int vatType, string dateFrom, string dateTo)
        {
            return Content(HttpStatusCode.OK, prm.GetProjectTimeBlocks(projectId, recordId, recordType, employeeId, loadOnlyForEmployee, BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true), false, 0, false, true));
        }

        [HttpGet]
        [Route("Project/TimeBlock/InvoiceRow/{invoiceId:int}/{customerInvoiceRowId:int}/{dateFrom}/{dateTo}")]
        public IHttpActionResult GetProjectTimeBlocksForInvoiceRow(int invoiceId, int customerInvoiceRowId, string dateFrom, string dateTo)
        {
            return Content(HttpStatusCode.OK, prm.GetProjectTimeBlocksForInvoiceRow(invoiceId, customerInvoiceRowId, BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true)));
        }

        [HttpGet]
        [Route("Project/TimeBlocksForTimeSheet/{employeeId:int}/{dateFrom}/{dateTo}")]
        public IHttpActionResult GetTimeBlocksForTimeSheet(int employeeId, string dateFrom, string dateTo, [ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] selectedEmployees)
        {
            return Content(HttpStatusCode.OK, prm.LoadProjectTimeBlockForTimeSheet(BuildDateTimeFromString(dateFrom, true), BuildDateTimeFromString(dateTo, true), employeeId, selectedEmployees != null ? selectedEmployees.ToList() : new List<int>()));
        }

        [HttpPost]
        [Route("Project/TimeBlocksForTimeSheetFiltered")]
        public IHttpActionResult GetTimeBlocksForTimeSheetFiltered(GetProjectTimeBlocksForTimesheetModel model)
        {
            return Content(HttpStatusCode.OK, prm.LoadProjectTimeBlockForTimeSheet(model.From, model.To, model.EmployeeId, model.Employees ?? new List<int>(), model.Projects ?? new List<int>(), model.Orders ?? new List<int>(), model.groupByDate, model.incPlannedAbsence, true, model.incInternOrderText, model.EmployeeCategories, model.TimeDeviationCauses));
        }

        [HttpGet]
        [Route("Project/TimeBlocksForTimeSheetFilteredByProject/{fromDate}/{toDate}/{projectId:int}/{includeChildProjects:bool}/{employeeId:int}")]
        public IHttpActionResult GetTimeBlocksForTimeSheetFilteredByProject(string fromDate, string toDate, int projectId, bool includeChildProjects, int employeeId)
        {
            return Content(HttpStatusCode.OK, prm.LoadProjectTimeBlockForTimeSheetByProjectId(BuildDateTimeFromString(fromDate, true), BuildDateTimeFromString(toDate, true), projectId, includeChildProjects, employeeId));
        }

        [HttpGet]
        [Route("Project/ProjectsForTimeSheet/{employeeId:int}")]
        public IHttpActionResult GetProjectsForTimeSheet(int employeeId)
        {   // Get user connected to specified employee
            User user = um.GetUserByEmployeeId(employeeId, base.ActorCompanyId);
            return Content(HttpStatusCode.OK, prm.GetProjectsForTimeSheetWithCustomer(employeeId, base.ActorCompanyId, user != null ? user.UserId : (int?)null).ToSmallDTOs(user != null ? user.UserId : (int?)null, setCustomer: true));
        }

        [HttpGet]
        [Route("Project/ProjectsForTimeSheet/Employees/{projectId:int}")]
        public IHttpActionResult GetProjectsForTimeSheetEmployees(int projectId, [ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] empIds)
        {
            return Content(HttpStatusCode.OK, prm.GetProjectsForTimeSheetEmployees(empIds, projectId));
        }

        [HttpGet]
        [Route("Project/ChangeProjectOnInvoice/{projectId:int}/{invoiceId:int}/{recordType:int}/{owerwriteDefaultDims:bool}")]
        public IHttpActionResult ChangeProjectOnInvoice(int projectId, int invoiceId, int recordType, bool owerwriteDefaultDims)
        {
            return Content(HttpStatusCode.OK, prm.ChangeProjectOnInvoice(base.ActorCompanyId, projectId, invoiceId, recordType, owerwriteDefaultDims));
        }

        [HttpGet]
        [Route("Project/Employees/{projectId:int}")]
        public IHttpActionResult GetEmployeesForTimeProjectRegistration(int projectId)
        {
            return Content(HttpStatusCode.OK, prm.GetEmployeesForTimeProjectRegistration(base.RoleId, projectId));
        }

        [HttpGet]
        [Route("Project/Employees/Dict/{projectId:int}")]
        public IHttpActionResult GetEmployeesForTimeProjectRegistrationDict(int projectId)
        {
            return Content(HttpStatusCode.OK, prm.GetEmployeesForTimeProjectRegistrationDict(base.RoleId, projectId).ToSmallGenericTypes());
        }

        [HttpGet]
        [Route("Project/Employees/Small/{projectId:int}/{fromDateString}/{toDateString}")]
        public IHttpActionResult GetEmployeesForTimeProjectRegistrationSmall(int projectId, string fromDateString, string toDateString)
        {
            return Content(HttpStatusCode.OK, prm.GetEmployeesForTimeProjectRegistrationSmall(base.RoleId, projectId, BuildDateTimeFromString(fromDateString, true), BuildDateTimeFromString(toDateString, true)));
        }

        [HttpGet]
        [Route("Project/Totals/{projectId:int}/{recordId:int}/{recordType:int}")]
        public IHttpActionResult GetProjectTotals(int projectId, int recordId, int recordType)
        {
            return Content(HttpStatusCode.OK, prm.GetProjectTotals(base.ActorCompanyId, projectId, recordId, recordType));
        }

        [HttpPost]
        [Route("Project/ValidateProjectTimeBlockSaveDTO")]
        public IHttpActionResult ValidateProjectTimeBlockSaveDTO(List<ValidateProjectTimeBlockSaveDTO> items)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, prm.ValidateSaveProjectTimeBlocks(items, false));
            }
        }

        [HttpPost]
        [Route("Project/ProjectTimeBlockSaveDTO")]
        public IHttpActionResult SaveProjectTimeBlockSaveDTO(List<ProjectTimeBlockSaveDTO> projectTimeBlockSaveDTOs)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, prm.SaveProjectTimeBlocks(projectTimeBlockSaveDTOs, false));
            }
        }

        [HttpPost]
        [Route("Project/SaveNotesForProjectTimeBlock")]
        public IHttpActionResult SaveNotesForProjectTimeBlock(ProjectTimeBlockSaveDTO projectTimeBlockSaveDTO)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, prm.SaveNotesForProjectTimeBlock(projectTimeBlockSaveDTO));
            }
        }

        [HttpPost]
        [Route("Project/RecalculateWorkTime")]
        public IHttpActionResult RecalculateWorkTime(List<ProjectTimeBlockSaveDTO> projectTimeBlockSaveDTOs)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, prm.RecalculateWorkTime(projectTimeBlockSaveDTOs));
            }
        }

        [HttpPost]
        [Route("Project/MoveTimeRowsToOrder")]
        public IHttpActionResult MoveTimeRowsToOrder(MoveProjectTimeBlocksToOrderModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, prm.MoveTimeRowsToOrder(model.CustomerInvoiceId, model.ProjectTimeBlockIds));
            }
        }

        [HttpPost]
        [Route("Project/MoveTimeRowsToDate")]
        public IHttpActionResult MoveTimeRowsToDate(MoveProjectTimeBlocksToDateModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, prm.MoveTimeRowsToDate(BuildDateTimeFromString(model.SelectedDate, true), model.ProjectTimeBlockIds, false));
            }
        }

        [HttpPost]
        [Route("Project/MoveTimeRowsToOrderRow")]
        public IHttpActionResult MoveTimeRowsToOrderRow(MoveProjectTimeBlocksToOrderModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
            {
                return Content(HttpStatusCode.OK, prm.MoveTimeRowsToOrderRow(model.CustomerInvoiceId, model.CustomerInvoiceRowId, model.ProjectTimeBlockIds));
            }
        }

        [HttpGet]
        [Route("Project/EmployeeScheduleAndTransactionInfo/{employeeId}/{date}")]
        public IHttpActionResult GetEmployeeScheduleAndTransactionInfo(int employeeId, string date)
        {
            return Content(HttpStatusCode.OK, prm.LoadEmployeeScheduleAndTransactionInfo(employeeId, (DateTime)BuildDateTimeFromString(date, true)));
        }

        [HttpGet]
        [Route("Project/EmployeeFirstTime/{employeeId}/{date}")]
        public IHttpActionResult GetEmployeeFirstEligableTime(int employeeId, string date)
        {
            return Content(HttpStatusCode.OK, prm.GetEmployeeFirstEligableTime(employeeId, (DateTime)BuildDateTimeFromString(date, true), base.ActorCompanyId, base.UserId));
        }

        [HttpGet]
        [Route("Project/ProjectStatisticsreport/{invoiceId:int}/{projectId:int}/{dateFrom}/{dateTo}")]
        public IHttpActionResult getProjectStatisticsReportUrl(int invoiceId, int projectId, string dateFrom, string dateTo, [ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] selectedEmployees)
        {
            int billingTimeProjectReportId = sm.GetIntSetting(SettingMainType.Company, (int)CompanySettingType.BillingDefaultTimeProjectReportTemplate, base.UserId, base.ActorCompanyId, 0);
            var reportItem = new BillingInvoiceTimeProjectReportDTO(base.ActorCompanyId, billingTimeProjectReportId, (int)SoeReportTemplateType.TimeProjectReport, new BillingInvoiceReportDTO(base.ActorCompanyId, billingTimeProjectReportId, (int)SoeReportTemplateType.TimeProjectReport, invoiceId, "", invoiceCopy: false, invoiceReminder: false, disableInvoiceCopies: false, includeProjectReport: true, includeOnlyInvoiced: false, reportLanguageId: 0), dateFrom: BuildDateTimeFromString(dateFrom, true), dateTo: BuildDateTimeFromString(dateTo, true));

            rm.SaveReportUrl(reportItem.ReportGuid, reportItem.ToString(false), billingTimeProjectReportId, (int)SoeReportTemplateType.TimeProjectReport, base.ActorCompanyId);
            return Content(HttpStatusCode.OK, reportItem.ToShortString(true));
        }

        #endregion

        #region TrackChanges

        [HttpGet]
        [Route("TrackChanges/{entity:int}/{recordId:int}/{includeChildren:bool}")]
        public IHttpActionResult GetTrackChanges(SoeEntityType entity, int recordId, bool includeChildren)
        {
            return Content(HttpStatusCode.OK, tcm.GetTrackChanges(base.ActorCompanyId, entity, recordId, includeChildren).ToDTOs());
        }

        [HttpGet]
        [Route("TrackChangesLog/Entities/")]
        public IHttpActionResult GetTrackChangesLogEntities()
        {
            return Content(HttpStatusCode.OK, tcm.GetTrackChangesLogEntities());
        }

        [HttpGet]
        [Route("TrackChangesLog/{entity:int}/{recordId:int}/{dateFromString}/{dateToString}")]
        public IHttpActionResult GetTrackChangesLog(SoeEntityType entity, int recordId, string dateFromString, string dateToString)
        {
            return Content(HttpStatusCode.OK, tcm.GetTrackChangesLog(base.ActorCompanyId, entity, recordId, BuildDateTimeFromString(dateFromString, true).Value, BuildDateTimeFromString(dateToString, true).Value));
        }

        [HttpPost]
        [Route("TrackChangesLog/ForEntity/")]
        public IHttpActionResult GetTrackChangesLog(TrackChangesSearchModel model)
        {
            if (!ModelState.IsValid)
                return Error(HttpStatusCode.BadRequest, ModelState, null, null);
            else
                return Content(HttpStatusCode.OK, tcm.GetTrackChangesLogForEntity(base.ActorCompanyId, model.EntityType, model.DateFrom, model.DateTo, model.Users));
        }

        #endregion

        #region Translation

        [HttpGet]
        [Route("Translation/{recordType:int}/{recordId:int}/{loadLangName:bool}")]
        public IHttpActionResult GetTranslations(CompTermsRecordType recordType, int recordId, bool loadLangName)
        {
            return Content(HttpStatusCode.OK, tm.GetCompTermDTOs(recordType, recordId, loadLangName));
        }

        #endregion

        #region UserCompanySetting

        #region Single setting

        [HttpGet]
        [Route("UserCompanySetting/Bool/{settingMainType:int}/{settingType:int}")]
        public IHttpActionResult GetBoolSetting(int settingMainType, int settingType)
        {
            return Content(HttpStatusCode.OK, sm.GetBoolSetting((SettingMainType)settingMainType, settingType, base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpGet]
        [Route("UserCompanySetting/Int/{settingMainType:int}/{settingType:int}")]
        public IHttpActionResult GetIntSetting(int settingMainType, int settingType)
        {
            return Content(HttpStatusCode.OK, sm.GetIntSetting((SettingMainType)settingMainType, settingType, base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpGet]
        [Route("UserCompanySetting/String/{settingMainType:int}/{settingType:int}")]
        public IHttpActionResult GetStringSetting(int settingMainType, int settingType)
        {
            return Content(HttpStatusCode.OK, sm.GetStringSetting((SettingMainType)settingMainType, settingType, base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpPost]
        [Route("UserCompanySetting/Bool")]
        public IHttpActionResult SaveBoolSetting(SaveUserCompanySettingModel model)
        {
            return Content(HttpStatusCode.OK, sm.UpdateInsertBoolSetting((SettingMainType)model.SettingMainType, model.SettingTypeId, model.BoolValue, base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpPost]
        [Route("UserCompanySetting/Int")]
        public IHttpActionResult SaveIntSetting(SaveUserCompanySettingModel model)
        {
            return Content(HttpStatusCode.OK, sm.UpdateInsertIntSetting((SettingMainType)model.SettingMainType, model.SettingTypeId, model.IntValue, base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpPost]
        [Route("UserCompanySetting/String")]
        public IHttpActionResult SaveStringSetting(SaveUserCompanySettingModel model)
        {
            return Content(HttpStatusCode.OK, sm.UpdateInsertStringSetting((SettingMainType)model.SettingMainType, model.SettingTypeId, model.StringValue, base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        #endregion

        #region Multiple settings

        [HttpGet]
        [Route("UserCompanySetting/License")]
        public IHttpActionResult GetLicenseSettings([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] settingTypes)
        {
            return Content(HttpStatusCode.OK, sm.GetUserCompanySettings(SettingMainType.License, settingTypes.ToList(), base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpGet]
        [Route("UserCompanySetting/License/ForEdit")]
        public IHttpActionResult GetLicenseSettingsForEdit()
        {
            return Content(HttpStatusCode.OK, sm.GetLicenseSettingsForEdit(base.LicenseId));
        }

        [HttpGet]
        [Route("UserCompanySetting/Company")]
        public IHttpActionResult GetCompanySettings([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] settingTypes)
        {
            return Content(HttpStatusCode.OK, sm.GetUserCompanySettings(SettingMainType.Company, settingTypes.ToList(), base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpGet]
        [Route("UserCompanySetting/User")]
        public IHttpActionResult GetUserSettings([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] settingTypes)
        {
            return Content(HttpStatusCode.OK, sm.GetUserCompanySettings(SettingMainType.User, settingTypes.ToList(), base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpGet]
        [Route("UserCompanySetting/UserAndCompany")]
        public IHttpActionResult GetUserAndCompanySettings([ModelBinder(typeof(CommaDelimitedArrayModelBinder))] int[] settingTypes)
        {
            return Content(HttpStatusCode.OK, sm.GetUserCompanySettings(SettingMainType.UserAndCompany, settingTypes?.ToList(), base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        [HttpPost]
        [Route("UserCompanySetting")]
        public IHttpActionResult SaveUserCompanySettings(List<UserCompanySettingEditDTO> settings)
        {
            return Content(HttpStatusCode.OK, sm.SaveUserCompanySettings(settings, base.UserId, base.ActorCompanyId, base.LicenseId));
        }

        #endregion

        #region Config settings

        [HttpGet]
        [Route("ConfigSetting/Bool/{name}")]
        public IHttpActionResult GetBoolConfigSetting(string name)
        {
            return Content(HttpStatusCode.OK, sm.GetBoolConfigSetting(name));
        }

        #endregion

        #endregion

        #region UserGridState

        [HttpGet]
        [Route("UserGridState/{grid}")]
        public IHttpActionResult GetUserGridState(string grid)
        {
            return Content(HttpStatusCode.OK, sm.GetUserGridStateValue(grid));
        }

        [HttpPost]
        [Route("UserGridState")]
        public IHttpActionResult SaveUserGridState(SaveUserGridStateModel model)
        {
            return Content(HttpStatusCode.OK, sm.SaveUserGridState(model.Grid, model.GridState));
        }

        [HttpDelete]
        [Route("UserGridState/{grid}")]
        public IHttpActionResult DeleteUserGridState(string grid)
        {
            return Content(HttpStatusCode.OK, sm.DeleteUserGridState(grid));
        }

        #endregion

        #region UsersByAvailability
        [HttpGet]
        [Route("UsersByAvailability")]
        public IHttpActionResult GetUsersByAvailability(HttpRequestMessage message)
        {
            bool onlyActive = message.GetBoolValueFromQS("onlyActive");
            bool setDefaultRoleName = message.GetBoolValueFromQS("onlyActive");
            bool byCompany = message.GetBoolValueFromQS("byCompany");
            string dateFrom = message.RequestUri.ParseQueryString()["dateFrom"];
            string dateTo = message.RequestUri.ParseQueryString()["dateTo"];


            if (byCompany)
                return Content(HttpStatusCode.OK, um.GetUsersByCompanyAndAvailability(base.ActorCompanyId, base.RoleId, base.UserId, BuildDateTimeFromString(dateFrom, false).Value, BuildDateTimeFromString(dateTo, false).Value, setDefaultRoleName, onlyActive));
            else
                return Content(HttpStatusCode.OK, um.GetUsersByLicenseAndAvailability(base.ActorCompanyId, base.RoleId, base.UserId, base.LicenseId, BuildDateTimeFromString(dateFrom, false).Value, BuildDateTimeFromString(dateTo, false).Value, setDefaultRoleName, onlyActive));

        }
        #endregion

        #region Validator

        [HttpGet]
        [Route("Validator/ValidBankNumberSE/")]
        public IHttpActionResult ValidBankNumberSE(string clearing, string bankAccountNr, int? sysPaymentType)
        {
            return Content(HttpStatusCode.OK, SoftOne.Soe.Common.Util.Validator.IsValidBankNumberSE(sysPaymentType, clearing, bankAccountNr));
        }

        [HttpGet]
        [Route("Validator/ValidIBANNumber/")]
        public IHttpActionResult ValidIBANNumber(string iban)
        {
            return Content(HttpStatusCode.OK, SoftOne.Soe.Common.Util.Validator.IsValidIBANNumber(iban));
        }

        [HttpGet]
        [Route("Validator/ValidSocialSecurityNumber/")]
        public IHttpActionResult IsValidSocialSecurityNumber(string source, bool checkValidDate, bool mustSpecifyCentury, bool mustSpecifyDash, TermGroup_Sex sex = TermGroup_Sex.Unknown)
        {
            int companyCountryId = coma.GetCompanySysCountryId(base.ActorCompanyId);
            return Content(HttpStatusCode.OK, CalendarUtility.IsValidSocialSecurityNumber((TermGroup_Country)companyCountryId, source, checkValidDate, mustSpecifyCentury, mustSpecifyDash, sex));
        }

        [HttpGet]
        [Route("Validator/ValidateEcomDeletion/{entityType:int}/{contactType:int}/{contactEcomId:int}")]
        public IHttpActionResult ValidateEcomDeletion(int entityType, int contactType, int contactEcomId)
        {
            return Content(HttpStatusCode.OK, com.ValidateContactEComDeletion((SoeEntityType)entityType, (ContactAddressItemType)contactType, contactEcomId));
        }

        #endregion

        #region XeMail

        [HttpGet]
        [Route("XeMail/Grid/{mailType:int}/{messageId:int}")]
        public IHttpActionResult GetXeMailItems(int mailType, int messageId)
        {
            return Content(HttpStatusCode.OK, cma.GetXEMailItems((XEMailType)mailType, base.LicenseId, messageId: messageId != 0 ? messageId : (int?)null));
        }

        [HttpGet]
        [Route("XeMail/{messageId:int}")]
        public IHttpActionResult GetXEMail(int messageId)
        {
            return Content(HttpStatusCode.OK, cma.GetXEMail(messageId, base.LicenseId, base.UserId));
        }

        [HttpGet]
        [Route("XeMail/NbrOfUnreadMessages/")]
        public IHttpActionResult GetNbrOfUnreadMessages()
        {
            return Content(HttpStatusCode.OK, cma.GetNbrOfUnreadMessages(base.LicenseId, base.UserId));
        }

        [HttpGet]
        [Route("XeMail/EmployeesByAccount/")]
        public IHttpActionResult GetEmployeesByAccount()
        {
            return Content(HttpStatusCode.OK, cma.GetEmployeeIdsByAccount(base.ActorCompanyId, base.RoleId, base.UserId).ToSmallGenericTypes());
        }

        [HttpPost]
        [Route("XeMail/")]
        public IHttpActionResult SendXeMail(MessageEditDTO message)
        {
            if (string.IsNullOrEmpty(message.ShortText))
                message.ShortText = StringUtility.HTMLToText(message.Text, true);
            else if (message.ShortText.Contains(">") && message.ShortText.Contains("<"))
                message.ShortText = StringUtility.HTMLToText(message.ShortText, true);

            message.ShortText = HttpUtility.HtmlDecode(message.ShortText);

            return Content(HttpStatusCode.OK, cma.SendXEMail(message, base.ActorCompanyId, base.RoleId, base.UserId));
        }

        [HttpPost]
        [Route("XeMail/SetReadDate/{date}/{messageId:int}")]
        public IHttpActionResult SetXEMailReadDate(string date, int messageId)
        {
            return Content(HttpStatusCode.OK, cma.SetXEMailReadDate(BuildDateTimeFromString(date, false).Value, messageId, base.UserId));
        }

        [HttpPost]
        [Route("XeMail/SetAsRead/")]
        public IHttpActionResult SetXEMailAsRead(ListIntModel model)
        {
            return Content(HttpStatusCode.OK, cma.SetXEMailAsRead(model.Numbers, base.UserId));
        }

        [HttpPost]
        [Route("XeMail/SetAsUnread/")]
        public IHttpActionResult SetXEMailAsUnread(ListIntModel model)
        {
            return Content(HttpStatusCode.OK, cma.SetXEMailAsUnread(model.Numbers, base.UserId));
        }

        [HttpPost]
        [Route("XeMail/DeleteMessages/Incoming/")]
        public IHttpActionResult DeleteIncomingMessages(ListIntModel model)
        {
            return Content(HttpStatusCode.OK, cma.DeleteIncomingXEMail(model.Numbers, base.UserId));
        }

        [HttpPost]
        [Route("XeMail/DeleteMessages/Outgoing/")]
        public IHttpActionResult DeleteOutgoingMessages(ListIntModel model)
        {
            return Content(HttpStatusCode.OK, cma.DeleteOutgoingXEMail(model.Numbers));
        }

        #endregion

        #region BatchUpdate
        [HttpGet]
        [Route("BatchUpdate/GetBatchUpdateForEntity/{entityType:int}")]
        public IHttpActionResult GetBatchUpdateForEntity(SoeEntityType entityType)
        {
            return Content(HttpStatusCode.OK, bum.GetBatchUpdate(entityType));
        }
        [HttpPost]
        [Route("BatchUpdate/RefreshBatchUpdateOptions")]
        public IHttpActionResult RefreshBatchUpdateOptions(RefreshBatchUpdateOptionsModel model)
        {
            return Content(HttpStatusCode.OK, bum.RefreshBatchUpdateOptions(model.EntityType, model.BatchUpdate));
        }
        [HttpGet]
        [Route("BatchUpdate/FilterOptions/{entityType:int}")]
        public IHttpActionResult GetContactAddressItemsDict(SoeEntityType entityType)
        {
            return Content(HttpStatusCode.OK, bum.GetBatchUpdateFilterOptions(entityType));
        }
        [HttpPost]
        [Route("BatchUpdate/PerformBatchUpdate")]
        public IHttpActionResult PerformBatchUpdate(PerformBatchUpdateModel model)
        {
            return Content(HttpStatusCode.OK, bum.PerformBatchUpdate(model.EntityType, model.BatchUpdates, model.Ids, model.FilterIds));
        }
        #endregion

        #region ImportDynamic
        [HttpPost]
        [Route("ImportDynamic/GetFileContent/{fileType:int}")]
        public async Task<IHttpActionResult> GetFileContent(int fileType)
        {
            if (Request.Content.IsMimeMultipartContent())
            {
                var data = await Request.Content.ParseMultipartAsync();
                Extensions.HttpPostedFile file = data?.Files["file"];
                var result = idm.GetFileContent(fileType, file?.File, file?.Filename);

                return Content(HttpStatusCode.OK, result);
            }
            throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        }

        [HttpPost]
        [Route("ImportDynamic/ParseRows")]
        public IHttpActionResult ParseRows(ParseRowsModel model)
        {
            return Content(HttpStatusCode.OK, idm.ParseRows(model.Fields, model.Options, model.Data));
        }
        #endregion

        #region SignatoryContract
        [HttpGet]
        [Route("SignatoryContract/UsesForPermission/{permissionType:int}")]
        public IHttpActionResult SignatoryContractUsesForPermission(int permissionType)
        {
            return Content(HttpStatusCode.OK, scm.UsesSignatoryContractForPermission((TermGroup_SignatoryContractPermissionType)permissionType));
        }

        [HttpGet]
        [Route("SignatoryContract/Authorize/{permissionType:int}")]
        public IHttpActionResult SignatoryContractAuthorize(int permissionType)
        {
            AuthorizeRequestDTO authorizeRequest = new AuthorizeRequestDTO 
            { 
                PermissionType = (TermGroup_SignatoryContractPermissionType)permissionType 
            };
            return Content(HttpStatusCode.OK, scm.Authorize(authorizeRequest));
        }

        [HttpPost]
        [Route("SignatoryContract/Authenticate")]
        public IHttpActionResult SignatoryContractAuthenticate(AuthenticationResponseDTO authenticationResponse)
        {
            return Content(HttpStatusCode.OK, scm.ValidateAuthenticationResponse(authenticationResponse));
        }
        #endregion
    }
}