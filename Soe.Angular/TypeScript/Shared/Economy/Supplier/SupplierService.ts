import { IAccountingRowDTO, IActionResult, IAttestWorkFlowHeadDTO, ISupplierInvoiceProductRowDTO } from "../../../Scripts/TypeLite.Net4";
import { CoreUtility } from "../../../Util/CoreUtility";
import { SupplierPaymentGridDTO, SupplierInvoiceDTO, UpdateEdiEntryDTO, OrderDTO, SupplierInvoiceCostAllocationDTO, InvoiceInterpretationDTO, IInvoiceInterpretationDTO } from "../../../Common/Models/InvoiceDTO";
import { PaymentRowSaveDTO } from "../../../Common/Models/PaymentRowDTO";
import { AccountingRowDTO } from "../../../Common/Models/AccountingRowDTO";
import { SoeOriginStatusClassification, SoeOriginType, TermGroup_ProjectType, SoeTimeCodeType, SoeInvoiceType, SoeInvoiceMatchingType, CompanySettingType, SoeModule, TermGroup_AttestEntity, AttestFlow_ReplaceUserReason } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { IHttpService } from "../../../Core/Services/httpservice";
import { AttestStateDTO } from "../../../Common/Models/AttestStateDTO";
import { FileUploadDTO } from "../../../Common/Models/FileUploadDTO";
import { Guid } from "../../../Util/StringUtility";
import { PurchaseSmallDTO } from "../../../Common/Models/PurchaseDTO";
import { PurchaseDeliveryInvoiceDTO } from "../../../Common/Models/PurchaseDeliveryDTO";
import { CommodityCodeDTO } from "../../../Common/Models/CommodityCodesDTO";

export interface ISupplierService {

    // GET 
    blockSupplierInvoicePayment(invoiceId: number, block: boolean, reason: string): ng.IPromise<any>;
    getAccountStdsDict(addEmptyRow: boolean): ng.IPromise<any>;
    getAttestWorkFlowGroups(useCache?: boolean): ng.IPromise<any>;
    getAttestWorkFlowGroupsDict(addEmptyRow: boolean): ng.IPromise<any>;
    getAttestWorkFlowGroup(id: number): ng.IPromise<any>;
    getAttestWorkFlowTemplateHeadsForCurrentCompany(): ng.IPromise<any>;
    getAttestWorkFlowTemplateHeadRows(templateHeadId: number): ng.IPromise<any>;
    getAttestWorkFlowTemplateHeadRowsUser(templateHeadId: number): ng.IPromise<any>;
    getAttestWorkFlowUsersByAttestTransitionId(attestTransitionId: number): ng.IPromise<any>;
    getAttestWorkFlowAttestRolesByAttestTransitionId(attestTransitionId: number): ng.IPromise<any>;
    getAttestWorkFlowHead(id: number, setTypeName: boolean, loadRows: boolean): ng.IPromise<any>;
    getAttestWorkFlowHeadFromInvoiceId(invoiceId: number, setTypeName: boolean, loadTemplate: boolean, loadRows: boolean, loadRemoved: boolean): ng.IPromise<any>;
    getAttestWorkFlowRowsFromInvoiceId(invoiceId: number): ng.IPromise<any>;
    getAttestWorkFlowOverview(classification: SoeOriginStatusClassification, allItemsSelection: number): ng.IPromise<any>;
    getInvoice(invoiceId: number, loadProjectRows: boolean, loadOrderRows: boolean, loadProject:boolean): ng.IPromise<any>;
    getInvoicePdf(fileId: number): ng.IPromise<any>;
    getInvoicesForGrid(allItemsSelection: number, loadOpen: boolean, loadClosed: boolean): ng.IPromise<any>;
    getInvoicesForSupplier(loadOpen: boolean, loadClosed: boolean, onlyMine: boolean, allItemsSelection: number, supplierId: number): ng.IPromise<any>;
    getNextSupplierNr(): ng.IPromise<any>;
    getPaymentInformationViewsDict(addEmptyRow: boolean): ng.IPromise<any>;
    getPaymentInformationViewsSmall(addEmptyRow: boolean): ng.IPromise<any>;
    getPaymentInformationViews(supplierId: number): ng.IPromise<any>;
    getPaymentConditions(): ng.IPromise<any>;
    getPaymentConditionsDict(addEmptyRow: boolean): ng.IPromise<any>;
    getPaymentConditionsGrid(): ng.IPromise<any>;
    getPaymentMethods(paymentType: SoeOriginType, addEmptyRow: boolean, includePaymentInformationRows: boolean, includeAccount: boolean, useCache: boolean): ng.IPromise<any>;
    getPaymentMethod(paymentMethodId: number, loadAccount: boolean, loadPaymentInformationRow: boolean): ng.IPromise<any>;
    getSuppliers(onlyActive: boolean, useCache: boolean): ng.IPromise<any>;
    getSuppliersDict(onlyActive: boolean, addEmptyRow: boolean, useCache: boolean): ng.IPromise<any>;
    getSupplier(supplierId: number, loadActor: boolean, loadAccount: boolean, loadContactAddresses: boolean, loadCategories: boolean): ng.IPromise<any>;
    getSupplierForExport(supplierId: number): ng.IPromise<any>;
    getSysPaymentMethodsDict(paymentType: SoeOriginType, addEmptyRow: boolean): ng.IPromise<any>;
    getSysWholesellersDict(addEmptyRow: boolean): ng.IPromise<any>;
    getOrdersForSupplierInvoiceEdit(useCache: boolean): ng.IPromise<any>;
    getOrderForSupplierByOrderNr(orderNr: string): ng.IPromise<any>;
    getTimeCodes(soeTimeCodeType: SoeTimeCodeType, active: boolean, loadPayrollProducts: boolean): ng.IPromise<any>;
    getEmployeesDict(addEmptyRow: boolean, concatNumberAndName: boolean, getHidden: boolean, orderByName: boolean): ng.IPromise<any>;
    getSupplierInvoiceProjectTransactions(inovoiceId: number): ng.IPromise<any>;
    getProjectList(type: TermGroup_ProjectType, active: boolean, getHidden: boolean, getFinished: boolean, useCache: boolean): ng.IPromise<any>;
    getProject(projectId: number): ng.IPromise<any>;
    getPaymentRowsSmall(invoiceId: number): ng.IPromise<any>;
    getPaymentRows(invoiceId: number): ng.IPromise<any>;
    getAgeDistribution(parameters: any): ng.IPromise<any>;
    getInvoicesPaymentsAndMatches(parameters: any): ng.IPromise<any>;
    getInvoicesMatches(recordId: number, actorId: number, type: number): ng.IPromise<any>;
    getInvoicePaymentsMatches(supplierId: number, soeInvoiceType: SoeInvoiceType): ng.IPromise<any>;
    getMatchingCustomerSupplier(soeInvoiceType: number): ng.IPromise<any>;
    getMatchCodes(soeInvoiceType: SoeInvoiceMatchingType, addEmptyRow: boolean): ng.IPromise<any>;
    getVoucherSeriesTypes();
    getAccountPeriodId(accountYearId: number, selectedDate: Date): ng.IPromise<any>;
    getDefaultVoucherSeriesId(accountYearId: number, accountingVoucherSeriesTypeManual: CompanySettingType);
    getVoucherSeriesByYear(accountYearId: number, includeTemplate: boolean): ng.IPromise<any>;
    getAccountSysVatRate(accountId: number): ng.IPromise<any>;
    getPayments(classification: SoeOriginStatusClassification, allItemsSelection: number): ng.IPromise<SupplierPaymentGridDTO[]>;
    getUnpaidInvoices(supplierId: number, addEmpty: boolean): ng.IPromise<any>;
    getInvoiceForPayment(invoiceId: number): ng.IPromise<any>;
    getAttestStates(entity: TermGroup_AttestEntity, module: SoeModule, addEmptyRow: boolean): ng.IPromise<AttestStateDTO[]>;
    getPayment(paymentRowId: number, loadInvoiceAndOrigin: boolean, loadAccountRows: boolean, loadAccounts: boolean): ng.IPromise<any>;
    getAttestStatesGenericList(entity: TermGroup_AttestEntity, module: SoeModule, addEmptyRow: boolean, addMultipleRow: boolean): ng.IPromise<any>;
    getPaymentInformationFromActor(actorIds: number[]): ng.IPromise<any>;
    getPaymentInformationFromActorForPaymentMethod(paymentMethodId: number, actorIds: number[]): ng.IPromise<any>;
    getSupplierInvoiceImage(invoiceId: number): ng.IPromise<any>;
    getSupplierInvoiceImageByFileId(fileId: number): ng.IPromise<any>;
    getSupplierInvoiceImageFromEdi(ediEntryId: number): ng.IPromise<any>;
    createFinvoiceImage(ediEntryId: number): ng.IPromise<any>;
    getScanningUnprocessedCount(): ng.IPromise<any>;
    getEdiScanningEntry(ediEntryId: number): ng.IPromise<any>;
    getInterpretedInvoice(ediEntryId: number): ng.IPromise<InvoiceInterpretationDTO>;
    getEdiEntryFromInvoice(InvoiceId: number): ng.IPromise<any>;
    getEdiEntry(ediEntryId: number, loadSupplier: boolean): ng.IPromise<any>;
    getScanningEntryDocumentId(scanningEntryInvoiceId: number): ng.IPromise<any>;
    hasHiddenAttestState(): ng.IPromise<boolean>;
    getOrder(invoiceId: number, includeRows: boolean): ng.IPromise<OrderDTO>;
    getSupplierPurchaseDeliveryInvoices(supplierInvoiceId: number): ng.IPromise<PurchaseDeliveryInvoiceDTO[]>;
    getSupplierPurchases(supplierId: number): ng.IPromise<PurchaseSmallDTO[]>;
    getSupplierPurchaseRows(purchaseIds: number[], getOnlyDelivered: boolean, getAlreadyConnected: boolean): ng.IPromise<PurchaseDeliveryInvoiceDTO[]>;
    getSupplierInvoiceAccountingRows(invoiceId: number): ng.IPromise<any>;
    getSupplierInvoicesCostOverview(notLinked: boolean, partiallyLinked: boolean, fullyLinked: boolean, allItemsSelection: number): ng.IPromise<any>;
    getSupplierReferences(supplierId: number, addEmtyRow: boolean): ng.IPromise<any>;
    getSupplierEmails(supplierId: number, loadContactPersonsEmails: boolean, addEmptyRow: boolean): ng.IPromise<any>;
    getSupplierInvoiceOrderRows(invoiceId: number): ng.IPromise<any>;
    getSupplierInvoiceProjectRows(invoiceId: number): ng.IPromise<any>;
    getSupplierInvoiceOrderProjectRows(invoiceId: number): ng.IPromise<any>;
    getTransferSupplierInvoicesToVoucherResult(guid: Guid): ng.IPromise<any>;
    getCustomerCommodityCodes(onlyActive?: boolean): ng.IPromise<CommodityCodeDTO[]>;
    getCustomerCommodityCodesDict(addEmpty?: boolean): ng.IPromise<any[]>;
    getSupplierProductRows(invoiceId: number): ng.IPromise<ISupplierInvoiceProductRowDTO[]>;

    //From changecompanyservice
    getCompaniesByUser(): ng.IPromise<any>
    getVoucherSeriesByCompany(actorCompanyId: number): ng.IPromise<any>;

    // POST
    saveInvoice(invoice: SupplierInvoiceDTO, purchaseInvoiceRows: PurchaseDeliveryInvoiceDTO[], createAttestVoucher: boolean, skipInvoiceNrCheck: boolean, disregardConcurrencyCheck: boolean): ng.IPromise<any>;
    savePaymentMethod(paymentMethod: any): ng.IPromise<any>;
    saveSupplier(supplier: any, files: FileUploadDTO[], extraFields: any[]): ng.IPromise<any>;
    saveSupplierInvoiceChangeCompany(dto: any): ng.IPromise<any>;
    saveSupplierInvoiceChangeAttestGroup(invoiceId: number, attestGroupId: number): ng.IPromise<any>;
    saveAttestWorkFlow(head: IAttestWorkFlowHeadDTO): ng.IPromise<any>;
    saveAttestWorkFlowForMultipleInvoices(head: IAttestWorkFlowHeadDTO, invoiceIds: number[]): ng.IPromise<any>;
    saveAttestWorkFlowForInvoices(invoiceIds: number[], sendMessage: boolean): ng.IPromise<IActionResult>;
    saveAttestWorkFlowRowAnswer(rowId: number, comment: any, answer: boolean, accountYearId: number): ng.IPromise<any>;
    saveAttestWorkFlowRowAnswers(invoiceIds: number[], comment: any, answer: boolean, accountYearId: number, attachments?: FileUploadDTO[]): ng.IPromise<any>;
    saveSupplierInvoiceAttestAccountingRows(invoiceId: number, accountingRows: IAccountingRowDTO[]);
    saveSupplierInvoiceAccountingRows(invoiceId: number, accountingRows: IAccountingRowDTO[], currentDimIds: any);
    saveSupplierInvoiceCostAllocationRows(invoiceId: number, costAllocationRows: SupplierInvoiceCostAllocationDTO[], projectId: number, customerInvoiceId: number, orderSeqNr: number);
    replaceAttestWorkFlowUser(reason: AttestFlow_ReplaceUserReason, deletedWorkFlowRowId: number, attestFlowComment: any, replacementUserId: number, invoiceId: number, sendMail: boolean): ng.IPromise<any>;
    updateSuppliersState(dict: any): ng.IPromise<any>;
    updateSuppliersIsPrivatePerson(list: any): ng.IPromise<any>;
    changeInvoiceSequenceNumberSuperAdmin(invoiceId: number, seqNr: number): ng.IPromise<any>;
    checkIfInvoiceNumberAlreadyExist(actorId: number, invoiceId: number, invoiceNr: string): ng.IPromise<any>;
    addScanningEntrys(ediSourceType: number): ng.IPromise<any>;
    getAttestGroupSuggestion(supplierId: number, projectId: number, costplaceAccountId: number, referenceOur: string): ng.IPromise<any>;
    getInvoiceAndPaymentStatus(soeOriginType: SoeOriginType, addEmptyRow: boolean);
    getSupplierCentralCountersAndBalance(counterTypes: any, supplierId: number, accountYearId: number, baseSysCurrencyId: number): ng.IPromise<any>;
    TransferSupplierInvoicesToDefinitive(idsToTransfer: any): ng.IPromise<any>;
    transferSupplierInvoicesToVouchers(idsToTransfer: any, guid?: any): ng.IPromise<any>;
    TransferEdiToInvoices(idsToTransfer: any): ng.IPromise<any>;
    TransferEdiState(idsToTransfer: number[], stateTo: number): ng.IPromise<any>;
    SendAttestReminders(idsToSendMessagesTo: number[]): ng.IPromise<any>;
    hideUnhandledInvoices(invoiceIds: number[]): ng.IPromise<any>;
    InvoicePaymentMatchAndVoucher(invoicePaymentMatchAndVoucher: any): ng.IPromise<any>;
    saveSupplierPayment(paymentRowSaveDTO: PaymentRowSaveDTO, accountingRows: AccountingRowDTO[], matchCodeId?: number): ng.IPromise<any>;
    transferSupplierPayments(items: any[], accountYearId: number, originStatusChange: number, paymentMethodId: number, sendPaymentFile: boolean, bulkPayDate?: Date): ng.IPromise<any>;
    updateEdiEntries(ediEntries: UpdateEdiEntryDTO[]): ng.IPromise<any>;
    generateReportForEdi(ediEntries: any[]): ng.IPromise<any>;
    getSupplierInvoicesForProjectCentral(projectId: number, loadChildProjects: boolean, fromDate?: Date, toDate?: Date, invoiceIds?: number[]): ng.IPromise<any>;
    saveInvoicesForUploadedImages(dataStorageIds: number[]): ng.IPromise<any>;
    getFilteredSupplierInvoices(filterModels: any): ng.IPromise<any>;
    saveSupplierFromFinvoice(ediEntryId: number): ng.IPromise<any>;
    transferSupplierProductRowsToOrder(supplierInvoiceId: number, customerInvoiceId: number, supplierInvoiceProductRowIds: number[], wholesellerId: number)
    transferSupplierInvoicesToOrder(items: any[], transferSupplierInvoiceRows: boolean, useMiscProduct: boolean)
    sendPaymentNotification(paymentMethodId: number, pageUrl: string, classification: number): ng.IPromise<any>;
    
    //From changecompanyservice
    getSuppliersByCompany(dto: any): ng.IPromise<any>

    // DELETE
    deleteInvoice(invoiceId: number): ng.IPromise<any>;
    deleteDraftInvoices(invoiceIds: number[]): ng.IPromise<any>;
    deletePaymentMethod(paymentMethod: any): ng.IPromise<any>;
    deleteSupplier(supplierId: number): ng.IPromise<any>;
    deleteAttestWorkFlow(attestWorkFlowHeadId: number): ng.IPromise<any>;
    deleteAttestWorkFlows(attestWorkFlowHeadIds: number[]): ng.IPromise<any>;
    cancelPayment(paymentRowId: number, revertVoucher: boolean): ng.IPromise<any>;
}

export class SupplierService implements ISupplierService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }



    // GET

    blockSupplierInvoicePayment(invoiceId: number, block: boolean, reason: string) {
        var model = {
            invoiceId: invoiceId,
            block: block,
            reason: reason
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_BLOCK_PAYMENT, model);
    }

    getAccountStdsDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_STD + addEmptyRow, false);
    }

    getAttestWorkFlowGroups(useCache: boolean = true) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_ATTEST_GROUP, null, Constants.CACHE_EXPIRE_LONG, !useCache);
    }

    getAttestWorkFlowGroupsDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_ATTEST_GROUP + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG);
    }

    getAttestWorkFlowGroup(id: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_ATTEST_GROUP_BY_ID + id, false);
    }

    getAttestWorkFlowTemplateHeadsForCurrentCompany() {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_TEMPLATE_HEADS_FOR_CURRENT_COMPANY + TermGroup_AttestEntity.SupplierInvoice, null, Constants.CACHE_EXPIRE_LONG);
    }

    getAttestWorkFlowTemplateHeadRows(templateHeadId: number) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_TEMPLATE_HEAD_ROWS + templateHeadId, null, Constants.CACHE_EXPIRE_LONG);
    }

    getAttestWorkFlowTemplateHeadRowsUser(templateHeadId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_TEMPLATE_HEAD_ROWS_USER + templateHeadId, false);
    }

    getAttestWorkFlowUsersByAttestTransitionId(attestTransitionId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_USER_BY_ATTEST_TRANSITION_ID + attestTransitionId, true);
    }

    getAttestWorkFlowAttestRolesByAttestTransitionId(attestTransitionId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_ATTESTROLES_BY_ATTEST_TRANSITION_ID + attestTransitionId, true);
    }

    getAttestWorkFlowHead(id: number, setTypeName: boolean, loadRows: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_HEAD + id + "/" + setTypeName + "/" + loadRows, false);
    }

    getAttestWorkFlowHeadFromInvoiceId(invoiceId: number, setTypeName: boolean, loadTemplate: boolean, loadRows: boolean, loadRemoved: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_HEAD_FROM_INVOICE_ID + invoiceId + "/" + setTypeName + "/" + loadTemplate + "/" + loadRows + "/" + loadRemoved, false);
    }

    getAttestWorkFlowRowsFromInvoiceId(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_ROWS_FROM_INVOICE_ID + invoiceId, false);
    }

    getAttestWorkFlowOverview(classification: SoeOriginStatusClassification, allItemsSelection: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_OVERVIEW + classification + "/" + allItemsSelection, false);
    }

    getAttestGroupSuggestion(supplierId: number, projectId: number, costplaceAccountId: number, referenceOur: any) {
        const model = {
            SupplierId: supplierId,
            ProjectId: projectId,
            CostplaceAccountId: costplaceAccountId,
            ReferenceOur: referenceOur,
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_ATTEST_GROUP_SUGGESTION, model);
    }

    getInvoicesForGrid(allItemsSelection: number, loadOpen: boolean, loadClosed: boolean) {
        return this.httpService.get(`${Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_GRID}?allItemsSelection=${allItemsSelection}&loadOpen=${loadOpen}&loadClosed=${loadClosed}`, false);
    }

    getInvoicesForSupplier(loadOpen: boolean, loadClosed: boolean, onlyMine: boolean, allItemsSelection: number, supplierId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE + loadOpen + "/" + loadClosed + "/" + onlyMine + "/" + allItemsSelection + "/" + supplierId, false);
    }

    getInvoice(invoiceId: number, loadProjectRows: boolean, loadOrderRows: boolean, loadProject: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE + invoiceId + "/" + loadProjectRows + "/" + loadOrderRows + "/" + loadProject, false);
    }

    getInvoiceAndPaymentStatus(soeOriginType: SoeOriginType, addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_CUSTOMER_INVOICE_INVOICEANDPAYMENTSTATUS + soeOriginType + "/" + addEmptyRow, null, Constants.CACHE_EXPIRE_LONG);
    }

    getInvoicePdf(fileId: number) {//TODO: this is really a temp function untill the file-remake is done.
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER + 'File/' + fileId, false);
    }

    getNextSupplierNr() {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIER_NEXT_SUPPLIER_NR, false);
    }

    getOrdersForSupplierInvoiceEdit(useCache: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_GETORDERS, useCache);
    }

    getOrderForSupplierByOrderNr(orderNr: string) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_GETORDER + orderNr, false);
    }

    getOrder(invoiceId: number, includeRows: boolean): ng.IPromise<OrderDTO> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_GETORDER + invoiceId + "/" + includeRows, false);
    }

    getVoucherSeriesTypes() {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SERIES_TYPE, null, Constants.CACHE_EXPIRE_LONG);
    }

    getAccountPeriodId(accountYearId: number, date: Date) {
        var dateString: string = null;
        if (date)
            dateString = date.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_PERIOD_ID + accountYearId + "/" + dateString, true);
    }

    getDefaultVoucherSeriesId(accountYearId: number, type: CompanySettingType) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SERIES + accountYearId + "/" + type, false);
    }

    getVoucherSeriesByYear(accountYearId: number, includeTemplate: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SERIES + accountYearId + "/" + includeTemplate, false);
    }

    getSupplierInvoiceProjectTransactions(inovoiceId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIERINVOICEPROJECTTRANSACTION + inovoiceId, false);
    }

    getAttestStatesGenericList(entity: TermGroup_AttestEntity, module: SoeModule, addEmptyRow: boolean, addMultipleRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_STATE_GL + entity + "/" + module + "/" + addEmptyRow + "/" + addMultipleRow, false);
    }

    getProjectList(type: TermGroup_ProjectType, active: boolean, getHidden: boolean, getFinished: boolean, useCache: boolean): ng.IPromise<any> {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_SUPPLIER_PROJECT + type + "/" + active + "/" + getHidden + "/" + getFinished, null, Constants.CACHE_EXPIRE_MEDIUM, !useCache);
    }

    getProject(projectId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PROJECT + projectId, false);
    }

    getTimeCodes(soeTimeCodeType: SoeTimeCodeType, active: boolean, loadPayrollProducts: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_SUPPLIER_TIME_CODES + soeTimeCodeType + "/" + active + "/" + loadPayrollProducts, null, Constants.CACHE_EXPIRE_LONG);
    }

    getEmployeesDict(addEmptyRow: boolean, concatNumberAndName: boolean, getHidden: boolean, orderByName: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_TIME_EMPLOYEE + "?addEmptyRow=" + addEmptyRow + "&concatNumberAndName=" + concatNumberAndName + "&getHidden=" + getHidden + "&orderByName=" + orderByName, null, Constants.CACHE_EXPIRE_LONG);
    }

    getPaymentInformationViewsDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_COMMON_PAYMENT_INFORMATION + addEmptyRow, false);
    }

    getPaymentInformationViewsSmall(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_COMMON_PAYMENT_INFORMATION_SMALL + addEmptyRow, false);
    }

    getPaymentInformationViews(supplierId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_COMMON_PAYMENT_INFORMATION + supplierId, false);
    }

    getPaymentConditions() {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_PAYMENT_CONDITION, Constants.WEBAPI_ACCEPT_DTO, Constants.CACHE_EXPIRE_LONG);
    }

    getPaymentConditionsDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_PAYMENT_CONDITION + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_LONG);
    }

    getPaymentConditionsGrid() {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_PAYMENT_CONDITION, Constants.WEBAPI_ACCEPT_GRID_DTO, Constants.CACHE_EXPIRE_VERY_SHORT);
    }

    getPaymentMethods(paymentType: SoeOriginType, addEmptyRow: boolean, includePaymentInformationRows: boolean, includeAccount: boolean, useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_COMMON_PAYMENT_METHOD + paymentType + "/" + addEmptyRow + "/" + includePaymentInformationRows + "/" + includeAccount + "/false", null, Constants.CACHE_EXPIRE_LONG, !useCache);
    }

    getPaymentMethod(paymentMethodId: number, loadAccount: boolean, loadPaymentInformationRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_COMMON_PAYMENT_METHOD + paymentMethodId + "/" + loadAccount + "/" + loadPaymentInformationRow, false);
    }

    getSuppliers(onlyActive: boolean, useCache: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIER + "?onlyActive=" + onlyActive, useCache);
    }

    getSuppliersDict(onlyActive: boolean, addEmptyRow: boolean, useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIER + "?onlyActive=" + onlyActive + "&addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_VERY_SHORT, !useCache);
    }

    getSupplierPurchaseDeliveryInvoices(supplierInvoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIER_PURCHASE_DELIVERY_INVOICES + supplierInvoiceId, false);
    }

    getSupplierPurchases(supplierId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIER_PURCHASE + supplierId, false);
    }

    getSupplierPurchaseRows(purchaseIds: number[], getOnlyDelivered: boolean, getAlreadyConnected) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_PURCHASEROW + getOnlyDelivered + "/" + getAlreadyConnected + "/" + "?purchaseIds=" + purchaseIds.join(','), false);
    }

    getSupplier(supplierId: number, loadActor: boolean, loadAccount: boolean, loadContactAddresses: boolean, loadCategories: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIER + supplierId + "/" + loadActor + "/" + loadAccount + "/" + loadContactAddresses + "/" + loadCategories, false);
    }

    getSupplierForExport(supplierId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIER_EXPORT + supplierId, false);
    }

    getSupplierCentralCountersAndBalance(counterTypes: Array<number>, supplierId: number, accountYearId: number, baseSysCurrencyId: number) {
        const model = {
            CounterTypes: counterTypes,
            SupplierId: supplierId,
            AccountYearId: accountYearId,
            BaseSysCurrencyId: baseSysCurrencyId,
        };

        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIERCENTRALCOUNTERSANDBALANCE, model);
    }

    getSupplierReferences(supplierId: number, addEmtyRow: boolean): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER_REFERENCE + supplierId + "/" + addEmtyRow, false);
    }

    getSupplierEmails(supplierId: number, loadContactPersonsEmails: boolean, addEmptyRow: boolean): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_CUSTOMER_CUSTOMER_EMAIL + supplierId + "/" + loadContactPersonsEmails + "/" + addEmptyRow, false);
    }

    getAccountSysVatRate(accountId: number) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_SYS_VAT_RATE + accountId, null, Constants.CACHE_EXPIRE_LONG);
    }

    getAgeDistribution(parameters: any): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_AGEDISTRIBUTION, parameters);
    }

    getSysPaymentMethodsDict(paymentType: SoeOriginType, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_COMMON_SYS_PAYMENT_METHOD + paymentType + "/" + addEmptyRow, true);
    }

    getSysWholesellersDict(addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_INVOICE_SYS_WHOLESELLERS + "?addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE);
    }

    getPaymentRowsSmall(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_GET_PAYMENTROWS_SMALL + invoiceId, false);
    }

    getPaymentRows(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_GET_PAYMENTROWS + invoiceId, false);
    }

    getInvoicesPaymentsAndMatches(parameters: any): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_MATCHES_PAYMENTS, parameters);
    }

    getInvoicesMatches(recordId: number, actorId: number, type: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_MATCHES + `?recordId=${recordId}&actorId=${actorId}&type=${type}`, false);
    }

    getMatchingCustomerSupplier(soeInvoiceType: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_MATCHES_MATCHINGCUSTOMERSUPPLIER + soeInvoiceType, false);
    }

    getMatchCodes(soeInvoiceMatchingType: SoeInvoiceMatchingType, addEmptyRow: boolean = false): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_MATCHES_MATCHCODES + soeInvoiceMatchingType + "/" + addEmptyRow, false);
    }

    getInvoicePaymentsMatches(supplierId: number, soeInvoiceType: SoeInvoiceType): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_MATCHES_INVOICEPAYMENTSMATCHES + supplierId + "/" + soeInvoiceType, false);
    }

    getPayments(classification: SoeOriginStatusClassification, allItemsSelection: number): ng.IPromise<SupplierPaymentGridDTO[]> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_PAYMENT + classification + "/" + allItemsSelection, false);
    }

    getUnpaidInvoices(supplierId: number, addEmpty: boolean): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_UNPAID + supplierId + "/" + addEmpty, false);
    }

    getInvoiceForPayment(invoiceId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_FOR_PAYMENT + invoiceId, false);
    }

    getSupplierInvoiceImage(invoiceId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_SUPPLIERINVOICEIMAGE + invoiceId, true);
    }
    getSupplierInvoiceImageByFileId(fileId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_FILE_SUPPLIERINVOICEIMAGE + fileId, true);
    }

    getSupplierInvoiceImageFromEdi(ediEntryId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_SUPPLIERINVOICEIMAGE_EDI + ediEntryId, true);
    }

    createFinvoiceImage(ediEntryId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_IMAGE_CREATEFINVOICE + ediEntryId, false);
    }

    getAttestStates(entity: TermGroup_AttestEntity, module: SoeModule, addEmptyRow: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_STATE + entity + "/" + module + "/" + addEmptyRow, null, Constants.CACHE_EXPIRE_LONG);
    }

    getPayment(paymentRowId: number, loadInvoiceAndOrigin: boolean, loadAccountRows: boolean, loadAccounts: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_PAYMENTROW + paymentRowId + "/" + loadInvoiceAndOrigin + "/" + loadAccountRows + "/" + loadAccounts, false);
    }

    getPaymentInformationFromActor(actorIds: number[]): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_PAYMENTINFORMATION_FROMACTOR + "?actorIds=" + actorIds.join(','), false);
    }

    getPaymentInformationFromActorForPaymentMethod(paymentMethodId: number, actorIds: number[]): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_CORE_PAYMENTINFORMATION_FORPAYMENTMETHOD + paymentMethodId + "/" + "?actorIds=" + actorIds.join(','), false);
    }

    getScanningUnprocessedCount(): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_SCANNING_UNPROCESSEDCOUNT, false);
    }

    getInterpretedInvoice(ediEntryId: number): ng.IPromise<IInvoiceInterpretationDTO> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_SCANNING_INTERPRETEDINVOICE + ediEntryId, false);
    }

    getEdiScanningEntry(ediEntryId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_SCANNING_SCANNINGENTRY + ediEntryId, false);
    }

    getEdiEntryFromInvoice(invoiceId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_EDI_ENTRY_FROM_INVOICE + invoiceId, false);
    }

    getEdiEntry(ediEntryId: number, loadSupplier: boolean): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_EDIENTRY + ediEntryId + "/" + loadSupplier, false);
    }

    getScanningEntryDocumentId(scanningEntryInvoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_SCANNING_SCANNINGENTRYDOCUMENTID + scanningEntryInvoiceId, false);
    }

    hasHiddenAttestState() {
        return this.httpService.getCache(Constants.WEBAPI_MANAGE_ATTEST_ATTEST_STATE_HAS_HIDDEN + TermGroup_AttestEntity.SupplierInvoice, null, Constants.CACHE_EXPIRE_LONG);
    }

    getSupplierInvoiceAccountingRows(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_ACCOUNTINGROWS + invoiceId, false);
    }

    getSupplierInvoicesCostOverview(notLinked: boolean, partiallyLinked: boolean, fullyLinked: boolean, allItemsSelection: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_COSTOVERVIEW + notLinked + "/" + partiallyLinked + "/" + fullyLinked + "/" + allItemsSelection, false);
    }

    getSupplierInvoiceOrderRows(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_ORDERROWS + invoiceId, false);
    }

    getSupplierInvoiceProjectRows(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_PROJECTROWS + invoiceId, false);
    }

    getSupplierInvoiceOrderProjectRows(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_ORDERPROJECTROWS + invoiceId, false);
    }

    getTransferSupplierInvoicesToVoucherResult(guid: Guid) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_TRANSFER_TO_VOUCHER + guid, false);
    }

    getCustomerCommodityCodes(onlyActive = false) {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_COMMODITYCODES + onlyActive, false);
    }

    getCustomerCommodityCodesDict(addEmpty = true) {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_COMMODITYCODES_DICT + addEmpty, false);
    }

    // From changecompanyservice
    getCompaniesByUser() {
        return this.httpService.get(Constants.WEBAPI_MANAGE_COMPANY_COMPANIES_BY_USER, true);
    }

    getVoucherSeriesByCompany(actorCompanyId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_VOUCHER_SERIES_TYPE_BY_COMPANY + actorCompanyId, true);
    }

    getSupplierProductRows(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_PRODUCTROWS + invoiceId, false);
    }

    // POST
    saveInvoice(invoice: SupplierInvoiceDTO, purchaseInvoiceRows: PurchaseDeliveryInvoiceDTO[], createAttestVoucher: boolean, skipInvoiceNrCheck: boolean, disregardConcurrencyCheck: boolean) {
        const model = {
            invoice: invoice,
            accountingRows: invoice.accountingRows,
            projectRows: invoice.supplierInvoiceProjectRows,
            orderRows: invoice.supplierInvoiceOrderRows,
            purchaseInvoiceRows: purchaseInvoiceRows,
            createAttestVoucher: createAttestVoucher,
            skipInvoiceNrCheck: skipInvoiceNrCheck,
            costAllocationRows: invoice.supplierInvoiceCostAllocationRows,
            disregardConcurrencyCheck: disregardConcurrencyCheck,
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE, model);
    }

    saveSupplierInvoiceAttestAccountingRows(invoiceId: number, accountingRows: IAccountingRowDTO[]) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_ATTEST_ACCOUNTING_ROWS + invoiceId, accountingRows);
    }

    saveSupplierInvoiceAccountingRows(invoiceId: number, accountingRows: IAccountingRowDTO[], currentDimIds:any) {
        const model = {
            accountingRows: accountingRows,
            currentDimIds: currentDimIds
        };

        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_ACCOUNTING_ROWS + invoiceId, model);
    }

    saveSupplierInvoiceCostAllocationRows(invoiceId: number, costAllocationRows: SupplierInvoiceCostAllocationDTO[], projectId: number, customerInvoiceId: number, orderSeqNr: number) {
        const model = {
            invoiceId: invoiceId,
            costAllocationRows: costAllocationRows,
            projectId: projectId,
            customerInvoiceId: customerInvoiceId,
            orderSeqNr: orderSeqNr,
        };

        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_ORDERPROJECTROWS, model);
    }

    savePaymentMethod(paymentMethod: any) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_COMMON_PAYMENT_METHOD, paymentMethod);
    }

    saveSupplier(supplier: any, files: FileUploadDTO[], extraFields: any[]) {
        const model = {
            supplier: supplier,
            files: files,
            extraFields: extraFields
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIER, model);
    }

    saveSupplierInvoiceChangeCompany(dto: any) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLER_INVOICE_CHANGE_COMPANY, dto);
    }

    saveSupplierInvoiceChangeAttestGroup(invoiceId: number, attestGroupId: number) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_CHANGE_ATTESTGROUP + invoiceId + "/" + attestGroupId, null);
    }

    saveAttestWorkFlow(head: IAttestWorkFlowHeadDTO) {
        // This service is sometimes called within a loop where recordId is changed.
        // The model is still the same though, and therefore we need to clone it here,
        // otherwise it will be the last recordId on all heads.
        const model = CoreUtility.cloneDTO(head);
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_ATTEST_GROUP_SAVE, model);
    }

    saveAttestWorkFlowForMultipleInvoices(head: IAttestWorkFlowHeadDTO, invoiceIds: number[]) {
        const model = {
            attestWorkFlowHead: head,
            InvoiceIds: invoiceIds,
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_ATTEST_GROUP_SAVE_MULTIPLE, model);
    }

    saveAttestWorkFlowForInvoices(invoiceIds: number[], sendMessage: boolean) {
        const model = {
            idsToTransfer: invoiceIds,
            sendMessage: sendMessage
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_FOR_INVOICES, model);
    }

    saveAttestWorkFlowRowAnswer(rowId: number, comment: any, answer: boolean, accountYearId: number) {
        const model = {
            rowId: rowId,
            comment: comment,
            answer: answer,
            accountYearId: accountYearId
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_ROW_SAVE_ANSWER, model);
    }

    saveAttestWorkFlowRowAnswers(invoiceIds: number[], comment: any, answer: boolean, accountYearId: number, attachments?: FileUploadDTO[]) {
        const model = {
            invoiceIds: invoiceIds,
            comment: comment,
            answer: answer,
            accountYearId: accountYearId,
            attachments: attachments ?? []
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_ROW_SAVE_ANSWERS, model);
    }

    replaceAttestWorkFlowUser(reason: AttestFlow_ReplaceUserReason, deletedWorkFlowRowId: number, comment: any, replacementUserId: number, invoiceId: number, sendMail: boolean) {
        const model = {
            reason: reason,
            deletedWorkFlowRowId: deletedWorkFlowRowId,
            comment: comment,
            replacementUserId: replacementUserId,
            invoiceId: invoiceId,
            sendMail: sendMail
        };

        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_REPLACE_USER, model);
    }

    updateSuppliersState(dict: any) {
        const model = {
            dict: dict
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIER_UPDATE_STATE, model);
    }

    updateSuppliersIsPrivatePerson(list: any) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIER_UPDATE_IS_PRIVATE_PERSON, list);
    }

    changeInvoiceSequenceNumberSuperAdmin(invoiceId: number, seqNr: number) {
        const model = {
            invoiceId: invoiceId,
            seqnr: seqNr
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLER_INVOICE_CHANGE_SEQNR, model);
    }

    checkIfInvoiceNumberAlreadyExist(actorId: number, invoiceId: number, invoiceNr: string) {
        const model = {
            actorId: actorId,
            invoiceId: invoiceId,
            invoiceNr: invoiceNr
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLER_INVOICE_NR_ALREADY_EXIST, model);
    }

    addScanningEntrys(ediSourceType: number) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_ADD_SCANNING_ENTRYS + ediSourceType, false);
    }

    TransferSupplierInvoicesToDefinitive(idsToTransfer: any) {
        const model = {
            numbers: idsToTransfer
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_TRANSFER_TO_DEFINITIVE, model);
    }

    transferSupplierInvoicesToVouchers(idsToTransfer: any, guid = null) {
        const model = {
            idsToTransfer: idsToTransfer,
            accountYearId: 0,
            guid: guid
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_TRANSFER_TO_VOUCHER, model);
    }

    TransferEdiToInvoices(idsToTransfer: any) {
        const model = {
            numbers: idsToTransfer
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_TRANSFER_EDI_TO_INVOICE, model);
    }

    TransferEdiState(idsToTransfer: number[], stateTo: number) {
        const model = {
            idsToTransfer: idsToTransfer,
            stateTo: stateTo
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_TRANSFER_EDI_STATE, model);
    }

    SendAttestReminders(idsToSendMessagesTo: number[]) {
        const model = {
            numbers: idsToSendMessagesTo
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_SEND_ATTEST_REMINDER, model);
    }

    hideUnhandledInvoices(invoiceIds: number[]) {
        const model = {
            invoiceIds: invoiceIds
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_HIDE_UNHANDLED, model);
    }

    InvoicePaymentMatchAndVoucher(invoicePaymentMatchAndVoucher): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_MATCHES_INVOICEPAYMENTSMATCHANDVOUCHER, invoicePaymentMatchAndVoucher);
    }

    saveSupplierPayment(paymentRowSaveDTO: PaymentRowSaveDTO, accountingRows: AccountingRowDTO[], matchCodeId: number = null) {
        const model = {
            payment: paymentRowSaveDTO,
            accountingRows: _.filter(accountingRows, r => !r.isDeleted && r.dim1Id),
            matchCodeId: matchCodeId
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_PAYMENTROW, model);
    }

    transferSupplierPayments(items: any[], accountYearId: number, originStatusChange: number, paymentMethodId: number, sendPaymentFile: boolean, bulkPayDate?: Date) {
        const model = {
            payments: items,
            accountYearId: accountYearId,
            originStatusChange: originStatusChange,
            paymentMethodId: paymentMethodId,
            bulkPayDate: bulkPayDate,
            sendPaymentFile: sendPaymentFile
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_PAYMENT_TRANSFER, model);
    }

    updateEdiEntries(ediEntries: UpdateEdiEntryDTO[]) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_TRANSFER_EDI_UPDATE, ediEntries);
    }

    generateReportForEdi(ediEntries: any[]) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_TRANSFER_EDI_GENERATEPDF, ediEntries);
    }

    // From changecompanyservice
    getSuppliersByCompany(dto: any) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIERS_BY_COMPANY, dto);
    }

    getSupplierInvoicesForProjectCentral(projectId: number, loadChildProjects: boolean, fromDate: Date = undefined, toDate: Date = undefined, invoiceIds: number[] = undefined) {
        const model = {
            classification: SoeOriginStatusClassification.SupplierInvoicesAll,
            originType: SoeOriginType.SupplierInvoice,
            projectId: projectId,
            loadChildProjects: loadChildProjects,
            invoiceIds: invoiceIds,
            fromDate: fromDate,
            toDate: toDate,
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_PROJECTCENTRAL, model);
    }

    saveInvoicesForUploadedImages(dataStorageIds: number[]) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_SAVE_INVOICESFORIMAGES, dataStorageIds);
    }

    getFilteredSupplierInvoices(filterModels: any) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_FILTERED, filterModels);
    }

    saveSupplierFromFinvoice(ediEntryId: number) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIER_FROM_FINVOICE + ediEntryId, null);
    }

    transferSupplierProductRowsToOrder(supplierInvoiceId: number, customerInvoiceId: number, supplierInvoiceProductRowIds: number[], wholesellerId: number) {
        const model = {
            customerInvoiceId,
            supplierInvoiceId,
            wholesellerId,
            supplierInvoiceProductRowIds,
        }
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_PRODUCTROWS_TRANSFER, model);
    }

    transferSupplierInvoicesToOrder(items: any[], transferSupplierInvoiceRows: boolean, useMiscProduct: boolean) {
        const model = {
            items,
            transferSupplierInvoiceRows,
            useMiscProduct,
        }
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_TRANSFERTOORDER, model);
    }

    sendPaymentNotification(paymentMethodId: number, pageUrl: string, classification: number) {
        const model = {
            paymentMethodId: paymentMethodId,
            pageUrl: pageUrl,
            classification: classification
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_PAYMENT_SEND_NOTIFICATION, model);
    }

    // DELETE

    deleteInvoice(invoiceId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE + invoiceId + "/" + false);
    }

    deleteDraftInvoices(invoiceIds: number[]) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_DELETE_DRAFT_INVOICES + invoiceIds.join(','));
    }

    deletePaymentMethod(paymentMethodId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_COMMON_PAYMENT_METHOD + paymentMethodId);
    }

    deleteSupplier(supplierId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIER + supplierId);
    }

    deleteAttestWorkFlow(attestWorkFlowHeadId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_ATTEST_GROUP_DELETE + attestWorkFlowHeadId);
    }

    deleteAttestWorkFlows(attestWorkFlowHeadIds: number[]) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_SUPPLIER_ATTEST_WORK_FLOW_ATTEST_GROUP_DELETE_MANY + attestWorkFlowHeadIds.join(','));
    }

    cancelPayment(paymentRowId: number, revertVoucher: boolean) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_SUPPLIER_PAYMENT_CANCEL + paymentRowId + "/" + revertVoucher);
    }
}
