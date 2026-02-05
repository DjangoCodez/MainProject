import { IHttpService } from "../../../Core/Services/HttpService";
import { OrderDTO, CustomerInvoiceAccountRowDTO, ProductRowDTO, OriginUserDTO } from "../../../Common/Models/InvoiceDTO";
import { SplitAccountingRowDTO } from "../../../Common/Models/AccountingRowDTO";
import { FileUploadDTO } from "../../../Common/Models/FileUploadDTO";
import { IOrderShiftDTO, IProjectTimeBlockDTO } from "../../../Scripts/TypeLite.Net4";
import { StringKeyValueList } from "../../../Common/Models/StringKeyValue";
import { ChecklistHeadRecordCompactDTO, ChecklistExtendedRowDTO } from "../../../Common/Models/checklistdto";
import { SoeOriginType, SupplierInvoiceOrderLinkType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";

export interface IOrderService {

    // GET
    getOrder(invoiceId: number, includeCategories: boolean, includeRows: boolean): ng.IPromise<OrderDTO>;
    getOrderTemplates(useCache: boolean): ng.IPromise<any>;
    getTemplates(originType: SoeOriginType, useCache: boolean): ng.IPromise<any>;
    getAccountRows(invoiceId: number): ng.IPromise<CustomerInvoiceAccountRowDTO[]>;
    getSplitAccountingRows(customerInvoiceRowId: number, excludeVatRows: boolean): ng.IPromise<SplitAccountingRowDTO[]>;
    getOrderShifts(invoiceId: number): ng.IPromise<IOrderShiftDTO[]>;
    getProjectTimeBlocksLastDate(projectId: number, recordId: number, recordType: number, employeeId: number, loadOnlyForEmployee: boolean): ng.IPromise<Date>;
    getProjectTimeBlocks(projectId: number, recordId: number, recordType: number, employeeId: number, loadOnlyForEmployee: boolean, vatType: number, dateFrom: Date, dateTo: Date): ng.IPromise<IProjectTimeBlockDTO[]>
    getProjectTimeBlocksForInvoiceRow(invoiceId:number, customerInvoiceRowId: number, dateFrom: Date, dateTo: Date): ng.IPromise<IProjectTimeBlockDTO[]>
    changeProjectOnInvoice(projectId: number, invoiceId: number, recordType: number, overwriteDefaultDimes:boolean): ng.IPromise<any>;
    getProject(projectId: number): ng.IPromise<any>;
    getProjectGridDTO(projectId: number): ng.IPromise<any>;
    getContractGroups(): ng.IPromise<any>;
    getEdiEntryInfo(ediEntryId: number): ng.IPromise<any>;
    getOriginUsers(invoiceId: number): ng.IPromise<any>;
    getExpenseRows(invoiceId: number, customerInvoiceRowId?: number): ng.IPromise<any>;
    getExpenseRowsFiltered(employeeId: number, from: Date, to: Date, employees: number[], categories: number[], projects: number[], orders: number[]): ng.IPromise<any>;
    getSupplierInvoicesLinkedToOrder(invoiceId: number);
    getSupplierInvoicesLinkedToProject(invoiceId: number, projectId: number);
    getSupplierInvoicesTransferedToOrder(invoiceId: number);
    getSupplierInvoicesItemsForOrder(invoiceId: number, projectId: number): ng.IPromise<any>;
    getOpenOrdersDict(): ng.IPromise<any>;
    getOrderSummary(invoiceId: number, projectId: number): ng.IPromise<any>;

    // POST
    saveOrder(modifiedFields: any, newRows: ProductRowDTO[], modifiedRows: StringKeyValueList[], checklistHeads: ChecklistHeadRecordCompactDTO[], checklistRows: ChecklistExtendedRowDTO[], originUsers: OriginUserDTO[], files: FileUploadDTO[], discardConcurrencyCheck: boolean, sendXEMail: boolean, autoSave: boolean): ng.IPromise<any>;
    saveOffer(modifiedFields: any, newRows: ProductRowDTO[], modifiedRows: StringKeyValueList[], originUsers: OriginUserDTO[], files: FileUploadDTO[], discardConcurrencyCheck: boolean): ng.IPromise<any>;
    saveContract(modifiedFields: any, newRows: ProductRowDTO[], modifiedRows: StringKeyValueList[], originUsers: OriginUserDTO[], files: FileUploadDTO[], discardConcurrencyCheck: boolean): ng.IPromise<any>;
    unlockOrder(invoiceId: number): ng.IPromise<any>;
    closeOrder(invoiceId: number): ng.IPromise<any>;
    unlockOffer(invoiceId: number): ng.IPromise<any>;
    closeOffer(invoiceId: number): ng.IPromise<any>;
    updateCustomerOnProject(invoiceId: number, projectId: number, customerId: number): ng.IPromise<any>;
    updateReadyState(invoiceId: number, userId: number): ng.IPromise<any>;
    sendReminderForReadyState(invoiceId: number, invoiceNr: string, userIds: number[]): ng.IPromise<any>;
    clearReadyState(invoiceId: number, userIds: number[]): ng.IPromise<any>; 
    updateOrderSupplierInvoiceImage(id: number, type: SupplierInvoiceOrderLinkType, include: boolean): ng.IPromise<any>;
    searchCustomerInvoiceRows(projects: any[], orders: any[], customers: any[], orderTypes: any[], orderContractTypes: any[], dateFrom: Date, dateTo: Date, onlyValid: boolean, onlyMine: boolean): ng.IPromise<any>;
    transferOrdersToInvoice(ids: any[], accountYearId: number, merge: boolean, setStatusToOrigin: boolean): ng.IPromise<any>;
    changeAttestStateOnOrderRows(items: { field1: number, field2: number }[], attestStateId: number): ng.IPromise<any>;
    batchSplitTimeRows(items: any[], from: Date, to: Date): ng.IPromise<any>;
    recalculateTimeRow(customerInvoiceRowId: number): ng.IPromise<any>;

    // DELETE
    deleteOrder(invoiceId: number, deleteProject: boolean): ng.IPromise<any>;    
}

export class OrderService implements IOrderService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET
    getOrder(invoiceId: number, includeCategories: boolean, includeRows: boolean): ng.IPromise<OrderDTO> {
        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER + invoiceId + "/" + includeCategories + "/" + includeRows, false);
    }

    getOrderTemplates(useCache: boolean): ng.IPromise<any> {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_ORDER_TEMPLATE, null, Constants.CACHE_EXPIRE_LONG,!useCache);
    }

    getTemplates(originType: SoeOriginType, useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_ORDER_TEMPLATES + originType, null, Constants.CACHE_EXPIRE_LONG, !useCache);
    }

    getAccountRows(invoiceId: number): ng.IPromise<CustomerInvoiceAccountRowDTO[]> {
        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER_ACCOUNT_ROWS + invoiceId, false);
    }


    getSplitAccountingRows(customerInvoiceRowId: number, excludeVatRows: boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER_SPLIT_ACCOUNTING_ROWS + customerInvoiceRowId + "/" + excludeVatRows, false);
    }

    getOrderShifts(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER_ORDER_SHIFTS + invoiceId, false);
    }

    getProjectTimeBlocksLastDate(projectId: number, recordId: number, recordType: number, employeeId: number, loadOnlyForEmployee: boolean) {

        if (!loadOnlyForEmployee)
            employeeId = 0;

        return this.httpService.get(Constants.WEBAPI_CORE_PROJECT_TIMEBLOCK_LASTDATE + projectId + "/" + recordId + "/" + recordType + "/" + employeeId, false);
    }

    getProjectTimeBlocks(projectId: number, recordId: number, recordType: number, employeeId: number, loadOnlyForEmployee: boolean, vatType: number, dateFrom: Date, dateTo: Date) {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_CORE_PROJECT_TIMEBLOCK + projectId + "/" + recordId + "/" + recordType + "/" + employeeId + "/" + loadOnlyForEmployee + "/" + vatType + "/" + dateFromString + "/" + dateToString, false);
    }

    getProjectTimeBlocksForInvoiceRow(invoiceId:number, customerInvoiceRowId: number, dateFrom: Date, dateTo: Date) {
        var dateFromString: string = null;
        if (dateFrom)
            dateFromString = dateFrom.toDateTimeString();
        var dateToString: string = null;
        if (dateTo)
            dateToString = dateTo.toDateTimeString();
        return this.httpService.get(Constants.WEBAPI_CORE_PROJECT_TIMEBLOCK_INVOICE_ROW + invoiceId + "/" + customerInvoiceRowId + "/" + dateFromString + "/" + dateToString, false);
    }

    changeProjectOnInvoice(projectId: number, invoiceId: number, recordType: number, overwriteDefaultDimes:boolean) {
        return this.httpService.get(Constants.WEBAPI_CORE_PROJECT_CHANGE_PROJECT_ON_INVOICE + projectId + "/" + invoiceId + "/" + recordType + "/" + overwriteDefaultDimes, false);
    }

    getProject(projectId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_PROJECT + projectId, false);
    }

    getProjectGridDTO(projectId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PROJECT_GRIDDTO + projectId, false);
    }

    getContractGroups() {
        return this.httpService.get(Constants.WEBAPI_BILLING_CONTRACT_CONTRACTGROUP, false);
    }

    getEdiEntryInfo(ediEntryId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER_EDIENTRY + ediEntryId, false);
    }

    getOriginUsers(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER_ORIGIN_USERS + invoiceId, false);
    }

    getExpenseRows(invoiceId: number, customerInvoiceRowId: number = 0) {
        return this.httpService.get(Constants.WEBAPI_EXPENSE_ROWS + invoiceId + "/" + customerInvoiceRowId, false);
    }

    getExpenseRowsFiltered(employeeId: number, from: Date, to: Date, employees: number[], categories: number[], projects: number[], orders: number[]) {
        const model = {
            employeeId: employeeId,
            from: from,
            to: to,
            employees: employees,
            projects: projects,
            orders: orders,
            employeeCategories: categories
        }
        return this.httpService.post(Constants.WEBAPI_EXPENSE_ROWS_FILTERED, model);
    }

    getSupplierInvoicesLinkedToOrder(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER_SUPPLIERINVOICES_LINKEDTOORDER + invoiceId, false);
    }
    getSupplierInvoicesLinkedToProject(invoiceId: number, projectId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER_SUPPLIERINVOICES_LINKEDTOPROJECT + invoiceId + "/" + projectId, false);
    }
    getSupplierInvoicesTransferedToOrder(invoiceId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER_SUPPLIERINVOICES_TRANSFEREDTOORDER + invoiceId, false);
    }

    getSupplierInvoicesItemsForOrder(invoiceId: number, projectId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER_SUPPLIERINVOICES_ITEMS + invoiceId + "/" + projectId, false);
    }

    getOpenOrdersDict() {
        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER_OPENDICT, false);
    }

    getOrderSummary(invoiceId: number, projectId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER_SUMMARY + invoiceId + "/" + projectId, false);
    }

    // POST
    saveOrder(modifiedFields: any, newRows: ProductRowDTO[], modifiedRows: StringKeyValueList[], checklistHeads: ChecklistHeadRecordCompactDTO[], checklistRows: ChecklistExtendedRowDTO[], originUsers: OriginUserDTO[], files: FileUploadDTO[], discardConcurrencyCheck: boolean, sendXEMail: boolean, autoSave: boolean): ng.IPromise<any> {
        const model = {
            modifiedFields: modifiedFields,
            newRows: newRows,
            modifiedRows: modifiedRows,
            checklistHeads: checklistHeads,
            checklistRows: checklistRows,
            originUsers: originUsers,
            files: files,
            discardConcurrencyCheck: discardConcurrencyCheck,
            sendXEMail: sendXEMail,
            autoSave: autoSave
        }

        return this.httpService.post(Constants.WEBAPI_BILLING_ORDER, model);
    }

    saveOffer(modifiedFields: any, newRows: ProductRowDTO[], modifiedRows: StringKeyValueList[], originUsers: OriginUserDTO[], files: FileUploadDTO[], discardConcurrencyCheck: boolean) {
        const model = {
            modifiedFields: modifiedFields,
            newRows: newRows,
            modifiedRows: modifiedRows,
            originUsers: originUsers,
            files: files,
            discardConcurrencyCheck: discardConcurrencyCheck
        }

        return this.httpService.post(Constants.WEBAPI_BILLING_OFFER, model);
    }

    saveContract(modifiedFields: any, newRows: ProductRowDTO[], modifiedRows: StringKeyValueList[], originUsers: OriginUserDTO[], files: FileUploadDTO[], discardConcurrencyCheck: boolean) {
        const model = {
            modifiedFields: modifiedFields,
            newRows: newRows,
            modifiedRows: modifiedRows,
            originUsers: originUsers,
            files: files,
            discardConcurrencyCheck: discardConcurrencyCheck
        }

        return this.httpService.post(Constants.WEBAPI_BILLING_CONTRACT, model);
    }

    unlockOffer(invoiceId: number): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_BILLING_OFFER_UNLOCK + invoiceId, null);
    }

    closeOffer(invoiceId: number): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_BILLING_OFFER_CLOSE + invoiceId, null);
    }

    unlockOrder(invoiceId: number): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_BILLING_ORDER_UNLOCK + invoiceId, null);
    }

    closeOrder(invoiceId: number): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_BILLING_ORDER_CLOSE + invoiceId, null);
    }

    updateCustomerOnProject(invoiceId: number, projectId: number, customerId: number): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_BILLING_PROJECT_CHANGE_CUSTOMER_ON_PROJECT + invoiceId + "/" + projectId + "/" + customerId, false);
    }

    updateReadyState(invoiceId: number, userId:number): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_BILLING_ORDER_UPDATE_READYSTATE + invoiceId + "/" + userId, null);
    }

    sendReminderForReadyState(invoiceId:number, invoiceNr:string, userIds: number[]): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_BILLING_ORDER_SEND_REMINDER_FOR_READYSTATE + invoiceId + "/" + invoiceNr + "/" + userIds.join(","), null);
    }

    clearReadyState(invoiceId: number, userIds: number[]): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_BILLING_ORDER_CLEAR_READYSTATE + invoiceId + "/" + userIds.join(","), null);
    }

    updateOrderSupplierInvoiceImage(id: number, type: SupplierInvoiceOrderLinkType, include: boolean): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_BILLING_ORDER_SUPPLIERINVOICES_UPDATEIMAGEINCLUDE + id + "/" + type + "/" + include, null);
    }

    searchCustomerInvoiceRows(projects: any[], orders: any[], customers: any[], orderTypes: any[], orderContractTypes: any[], dateFrom: Date, dateTo: Date, onlyValid: boolean, onlyMine: boolean) {
        const model = {
            projects: projects,
            orders: orders,
            customers: customers,
            orderTypes: orderTypes,
            orderContractTypes: orderContractTypes,
            from: dateFrom,
            to: dateTo,
            onlyValid: onlyValid,
            onlyMine: onlyMine
        };

        return this.httpService.post(Constants.WEBAPI_BILLING_ORDER_HANDLEBILLING_SEARCH, model);
    }

    transferOrdersToInvoice(ids: any[], accountYearId: number, merge: boolean, setStatusToOrigin: boolean) {
        const model = {
            ids: ids,
            accountYearId: accountYearId,
            merge: merge,
            setStatusToOrigin: setStatusToOrigin,
        };

        return this.httpService.post(Constants.WEBAPI_BILLING_ORDER_HANDLEBILLING_TRANSFER, model);
    }

    changeAttestStateOnOrderRows(items: any[], attestStateId: number) {
        const model = {
            items: items,
            attestStateId: attestStateId,
        };

        return this.httpService.post(Constants.WEBAPI_BILLING_ORDER_HANDLEBILLING_CHANGEATTESTSTATE, model);
    }

    batchSplitTimeRows(items: any[], from: Date, to: Date) {
        const model = {
            items: items,
            from: from,
            to: to,
        };

        return this.httpService.post(Constants.WEBAPI_BILLING_ORDER_HANDLEBILLING_BATCHSPLITTIMEROWS, model);
    }

    recalculateTimeRow(customerInvoiceRowId: number): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_BILLING_ORDER_RECALCULATE_TIMEROW + customerInvoiceRowId, null);
    }

    // DELETE
    deleteOrder(invoiceId: number, deleteProject: boolean) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_ORDER + invoiceId + "/" + deleteProject);
    }
    
}
