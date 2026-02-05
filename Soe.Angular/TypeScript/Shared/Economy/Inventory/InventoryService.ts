import { IHttpService } from "../../../Core/Services/httpservice";
import { IInventoryDTO, IInventoryNoteDTO } from "../../../Scripts/TypeLite.Net4";
import { TermGroup_InventoryLogType } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";

export interface IInventoryService {

    // GET
    getInventories(statuses: string): ng.IPromise<any>;
    getInventoryAccounts(): ng.IPromise<any>;
    getInventory(inventoryId: number): ng.IPromise<any>;
    getInventoriesDict(): ng.IPromise<any>;
    getNextInventoryNr(): ng.IPromise<any>;
    getInventoryTraceViews(inventoryId: number): ng.IPromise<any>;
    getInventoryWriteOffMethods(): ng.IPromise<any>;
    getInventoryWriteOffMethodsDict(): ng.IPromise<any>;
    getInventoryWriteOffMethod(inventoryWriteOffMethodId: number): ng.IPromise<any>;
    getInventoryWriteOffTemplates(addEmptyRow: boolean): ng.IPromise<any>;
    getInventoryWriteOffTemplatesDict(addEmptyRow: boolean): ng.IPromise<any>;
    getInventoryWriteOffTemplate(inventoryWriteOffTemplateId: number): ng.IPromise<any>;
    getInventoryCategories(): ng.IPromise<any>;

    // POST
    saveInventoryWriteOffMethod(inventoryWriteOffMethod: any): ng.IPromise<any>;
    saveInventoryWriteOffTemplate(templateDTO: any, accountSettings: any[]): ng.IPromise<any>;
    saveInventory(inventory: IInventoryDTO, categoryRecords: any[], accountSettings: any[], debtAccountId: number): ng.IPromise<any>;
    saveAdjustment(inventoryId: number, type: TermGroup_InventoryLogType, voucherSeriesTypeId: number, amount: number, date: Date, note: string, accountingRows: any[]): ng.IPromise<any>;
    saveNotesAndDescription(inventoryNote: IInventoryNoteDTO): ng.IPromise<any>;

    // DELETE
    deleteInventoryWriteOffMethod(inventoryWriteOffMethodId: number): ng.IPromise<any>;
    deleteInventoryWriteOffTemplate(inventoryWriteOffTemplateId: number): ng.IPromise<any>;
    deleteInventory(inventorId: number): ng.IPromise<any>;
}

export class InventoryService implements IInventoryService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET

    getInventories(statuses: string) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORIES + statuses, false);
    }

    getInventoryAccounts() {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORYACCOUNTS,false);
    }

    getInventoriesDict() {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORIES_DICT, false);
    }

    getInventory(inventoryId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_INVENTORY + inventoryId, false);
    }

    getNextInventoryNr() {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_INVENTORY_NEXTINVENTORYNR, false);
    }

    getInventoryTraceViews(inventoryId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORYTRACEVIEWS + inventoryId, false);
    }

    getInventoryWriteOffMethods() {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORY_WRITE_OFF_METHOD, false);
    }

    getInventoryWriteOffMethodsDict() {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORY_WRITE_OFF_METHODS_DICT, false);
    }

    getInventoryWriteOffMethod(inventoryWriteOffMethodId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORY_WRITE_OFF_METHOD + inventoryWriteOffMethodId, false);
    }

    getInventoryWriteOffTemplatesDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORY_WRITE_OFF_TEMPLATES_DICT + addEmptyRow, false);
    }

    getInventoryWriteOffTemplates(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORY_WRITE_OFF_TEMPLATE + addEmptyRow, false);
    }

    getInventoryWriteOffTemplate(inventoryWriteOffTemplateId: number) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORY_WRITE_OFF_TEMPLATE + inventoryWriteOffTemplateId, false);
    }

    getInventoryCategories() {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_INVENTORY_CATEGORIES, false);
    }

    // POST

    saveInventoryWriteOffMethod(inventoryWriteOffMethod: any) {
        return this.httpService.post(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORY_WRITE_OFF_METHOD, inventoryWriteOffMethod);
    }

    saveInventoryWriteOffTemplate(templateDTO: any, accountSettings: any[]) {
        const model = {
            inventoryWriteOffTemplate: templateDTO,
            accountSettings: accountSettings
        };
        return this.httpService.post(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORY_WRITE_OFF_TEMPLATE, model);
    }

    saveInventory(inventory: IInventoryDTO, categoryRecords: any[], accountSettings: any[], debtAccountId: number) {
        //saveInventory(inventory: any, categoryRecords: any[], accountSettings: any[], debtAccountId: number) {
        const model = {
            inventory: inventory,
            categoryRecords: categoryRecords,
            accountSettings: accountSettings,
            debtAccountId: debtAccountId
        }
        return this.httpService.post(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORY, model);
    }

    saveAdjustment(inventoryId: number, type: TermGroup_InventoryLogType, voucherSeriesTypeId: number, amount: number, date: Date, note: string, accountingRows: any[]) {
        const model = {
            inventoryId: inventoryId,
            type: type,
            voucherSeriesTypeId: voucherSeriesTypeId,
            amount: amount,
            date: date,
            note: note,
            accountRowItems: accountingRows,
        }

        return this.httpService.post(Constants.WEBAPI_ECONOMY_INVENTORY_ADJUSTMENT, model);
    }

    saveNotesAndDescription(inventoryNote: IInventoryNoteDTO) {
        const model = {
            inventoryId: inventoryNote.inventoryId,
            notes: inventoryNote.notes,
            description: inventoryNote.description
        }
        return this.httpService.post(Constants.WEBAPI_ECONOMY_INVENTORY_NOTES_AND_DESCRIPTION, model);
    }

    // DELETE

    deleteInventoryWriteOffMethod(inventoryWriteOffMethodId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORY_WRITE_OFF_METHOD + inventoryWriteOffMethodId);
    }

    deleteInventoryWriteOffTemplate(inventoryWriteOffTemplateId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORY_WRITE_OFF_TEMPLATE + inventoryWriteOffTemplateId);
    }

    deleteInventory(inventoryId: number) {
        return this.httpService.delete(Constants.WEBAPI_ECONOMY_INVENTORY_INVENTORY + inventoryId);
    }
}
