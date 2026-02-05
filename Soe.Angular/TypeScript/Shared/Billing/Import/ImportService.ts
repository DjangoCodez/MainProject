import { IHttpService } from "../../../Core/Services/HttpService";
import { EdiEntryViewDTO } from "../../../Common/Models/InvoiceDTO";
import { Constants } from "../../../Util/Constants";

export interface IImportService {

    // GET
    getEdiEntryViews(classification: number, originType: number): ng.IPromise<EdiEntryViewDTO>;
    getSuppliersDict(onlyActive: boolean, addEmptyRow: boolean, useCache: boolean): ng.IPromise<any>;
    getFinvoiceEntryViews(classification: number, allItemsSelection: number, onlyUnHandled:boolean): ng.IPromise<EdiEntryViewDTO>;

    // POST
    generateReportForEdi(ediEntries: any[]): ng.IPromise<any>;
    transferEdiToOrders(idsToTransfer: any): ng.IPromise<any>;
    transferEdiToInvoices(idsToTransfer: any): ng.IPromise<any>;
    changeEdiState(idsToTransfer: number[], stateTo: number): ng.IPromise<any>;
    addEdiEntrys(ediSourceType: number): ng.IPromise<any>;
    updateEdiEntries(ediEntries: any[]): ng.IPromise<any>;
    getFilteredEdiEntryViews(classification: number, originType: number, billingTypes: number[], buyerId: string, dueDate: Date, invoiceDate: Date, orderNr: string, orderStatuses: number[], sellerOrderNr: string, ediStatuses: number[], sum: number, supplierNrName: string, allItemsSelection: number): ng.IPromise<EdiEntryViewDTO>;
    importFinvoiceItems(dataStorageIds: number[]): ng.IPromise<any>;
    // DELETE

}

export class ImportService implements IImportService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET
    getEdiEntryViews(classification: number, originType: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_EDI_EDIENTRYVIEWS + classification + "/" + originType, false);
    }

    getSuppliersDict(onlyActive: boolean, addEmptyRow: boolean, useCache: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_SUPPLIER_SUPPLIER + "?onlyActive=" + onlyActive + "&addEmptyRow=" + addEmptyRow, Constants.WEBAPI_ACCEPT_GENERIC_TYPE, Constants.CACHE_EXPIRE_VERY_SHORT, !useCache);
    }

    getFinvoiceEntryViews(classification: number, allItemsSelection: number, onlyUnHandled:boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_EDI_FINVOICEENTRYVIEWS + classification + "/" + allItemsSelection + "/" + onlyUnHandled, false);
    }

    // POST
    generateReportForEdi(ediEntries: any[]) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_TRANSFER_EDI_GENERATEPDF, ediEntries);
    }

    transferEdiToOrders(idsToTransfer: any) { 
        const model = {
            numbers: idsToTransfer
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_TRANSFER_EDI_TO_ORDER, model);
    }

    transferEdiToInvoices(idsToTransfer: any) { 
        const model = {
            numbers: idsToTransfer
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_TRANSFER_EDI_TO_INVOICE, model);
    }

    changeEdiState(idsToTransfer: number[], stateTo: number) {
        const model = {
            idsToTransfer: idsToTransfer,
            stateTo: stateTo
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_TRANSFER_EDI_STATE, model);
    }

    addEdiEntrys(ediSourceType: number) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_ADD_SCANNING_ENTRYS + ediSourceType, false);
    }

    updateEdiEntries(ediEntries: any[]) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_SUPPLIER_INVOICE_TRANSFER_EDI_UPDATE, ediEntries);
    }

    getFilteredEdiEntryViews(classification: number, originType: number, billingTypes: number[], buyerId: string, dueDate: Date, invoiceDate: Date, orderNr: string, orderStatuses: number[], sellerOrderNr: string, ediStatuses: number[], sum: number, supplierNrName: string, allItemsSelection: number) {
        const model = {
            classification: classification,
            originType: originType,
            billingTypes: billingTypes,
            buyerId: buyerId,
            dueDate: dueDate,
            invoiceDate: invoiceDate,
            orderNr: orderNr,
            orderStatuses: orderStatuses,
            sellerOrderNr: sellerOrderNr,
            ediStatuses: ediStatuses,
            sum: sum,
            supplierNrName: supplierNrName,
            allItemsSelection: allItemsSelection,
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_INVOICE_EDI_EDIENTRYVIEWS_FILTERED, model);
    }

    importFinvoiceItems(dataStorageIds: number[]) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_ACCOUNTING_IMPORT_FINVOICE_ITEMS, dataStorageIds);
    }

    // DELETE

}