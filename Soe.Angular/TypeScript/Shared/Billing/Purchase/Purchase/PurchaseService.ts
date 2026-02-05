import { OriginUserDTO } from "../../../../Common/Models/InvoiceDTO";
import { PurchaseRowDTO, PurchaseSmallDTO } from "../../../../Common/Models/PurchaseDTO";
import { PurchaseFromOrderDTO } from "../../../../Common/Models/PurchaseFromOrderDTO";
import { SmallGenericType } from "../../../../Common/Models/SmallGenericType";
import { StringKeyValueList } from "../../../../Common/Models/StringKeyValue";
import { IHttpService } from "../../../../Core/Services/httpservice";
import { IPurchaseDeliverySaveDTO, IActionResult, IPurchaseSmallDTO, ICustomerInvoiceRowPurchaseDTO, IPurchaseRowFromStockDTO } from "../../../../Scripts/TypeLite.Net4";
import { PurchaseCustomerInvoiceViewType, SoeOriginStatus } from "../../../../Util/CommonEnumerations";
import { Constants } from "../../../../Util/Constants";

export interface IPurchaseService {
    // GET
    getPurchaseOrders(allItemsSelection: number, selectedPurchaseStatus: number[]): ng.IPromise<any>;
    getPurchaseOrder(purchaseId: number): ng.IPromise<any>;
    getPurchaseOrderSmall(purchaseId: number): ng.IPromise<IPurchaseSmallDTO>;
    getPurchaseRows(purchaseId: number): ng.IPromise<any>;
    getPurchaseRowsForOrder(invoiceId: number): ng.IPromise<any>;
    getDeliveries(allItemsSelection: number): ng.IPromise<any>;
    getDelivery(purchaseDeliveryId: number): ng.IPromise<any>;
    getPurchaseDeliveryRowsByPurchaseId(purchaseId: number): ng.IPromise<any>;
    getDeliveryRowsFromPurchase(purchaseId: number, supplierId: number): ng.IPromise<any>;
    getDeliveryRows(purchaseDeliveryId: number): ng.IPromise<any>;
    getPurchaseOrdersForSelect(forDelivery: boolean): ng.IPromise<IPurchaseSmallDTO[]>;
    getPurchaseStatus(): ng.IPromise<any>;
    getDeliveryAddresses(customerOrderId: number): ng.IPromise<string[]>;
    getCustomerInvoicePurchase(viewType: PurchaseCustomerInvoiceViewType, id: number): ng.IPromise<ICustomerInvoiceRowPurchaseDTO[]>;
    getPurchaseStatistics(fromDate: Date, toDate: Date): ng.IPromise<any>;

    // POST
    savePurchaseOrder(modifiedFields: any, originUsers: OriginUserDTO[], newRows: PurchaseRowDTO[], modifiedRows: StringKeyValueList[]): ng.IPromise<any>;
    updatePurchaseFromOrder(data: PurchaseFromOrderDTO);
    createPurchaseFromStockSuggestion(data: IPurchaseRowFromStockDTO[]): ng.IPromise<{ [tempId: number]: SmallGenericType }>;
    saveDeliveryRows(data: IPurchaseDeliverySaveDTO);
    savePurchaseStatus(id: SoeOriginStatus, purchaseId: number);
    sendPurchaseAsEmail(purchaseId: number, reportId: number, emailTemplateId: number, langId: number, recipients: number[], singleRecipient: string): ng.IPromise<any>;
    sendPurchasesAsEmail(purchaseIds: number[], emailTemplateId: number, langId: number): ng.IPromise<any>;

    // DELETE
    deletePurchase(purchaseId: number): ng.IPromise<any>;
}

export class PurchaseService implements IPurchaseService {
    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    getPurchaseOrders = (allItemsSelection: number, selectedPurchaseStatus: number[]) => {
        if (!allItemsSelection) {
            allItemsSelection = 0;
        }
        return this.httpService.get(Constants.WEBAPI_BILLING_PURCHASE_ORDERS + "?allItemsSelection=" + allItemsSelection + "&status=" + selectedPurchaseStatus.join(','), false);
    }
    getPurchaseStatistics( fromDate: Date, toDate: Date) {
        const model = {
            FromDate: fromDate,
            ToDate: toDate,
        };

        return this.httpService.post(Constants.WEBAPI_BILLING_PURCHASE_STATISTICS, model);
    }

    getPurchaseOrder = (purchaseId: number) => {
        
        return this.httpService.get(Constants.WEBAPI_BILLING_PURCHASE_ORDER + "/" + purchaseId, false);
    }

    getPurchaseOrderSmall = (purchaseId: number) => {

        return this.httpService.get(Constants.WEBAPI_BILLING_PURCHASE_ORDER + "/" + purchaseId, false, Constants.WEBAPI_ACCEPT_SMALL_DTO);
    }

    getPurchaseRows = (purchaseId: number) => {

        return this.httpService.get(Constants.WEBAPI_BILLING_PURCHASE_ORDER_ROWS + "/" + purchaseId, false);
    }

    getPurchaseRowsForOrder = (invoiceId: number) => {

        return this.httpService.get(Constants.WEBAPI_BILLING_PURCHASE_ORDER_ROWS_ORDER + "/" + invoiceId, false);
    }

    getPurchaseOrdersForSelect = (forDelivery: boolean) => {
        return this.httpService.get(Constants.WEBAPI_BILLING_PURCHASE_FORSELECT + "/" + forDelivery, false);
    }

    getPurchaseStatus = () => {
        return this.httpService.get(Constants.WEBAPI_BILLING_PURCHASE_STATUS + "/", false);
    }

    getDeliveryAddresses = (customerOrderId: number) => {
        return this.httpService.get(Constants.WEBAPI_BILLING_PURCHASE_DELIVERYADDRESSES + customerOrderId, false);
    }

    getDeliveries = (allItemsSelection: number) => {
        if (!allItemsSelection) {
            allItemsSelection = 0;
        }

        return this.httpService.get(Constants.WEBAPI_BILLING_PURCHASE_DELIVERIES + "/" + allItemsSelection, false);
    }

    getDelivery = (purchaseDeliveryId: number) => {

        return this.httpService.get(Constants.WEBAPI_BILLING_PURCHASE_DELIVERY + "/" + purchaseDeliveryId, false);
    }

    getDeliveryRows = (purchaseDeliveryId: number) => {

        return this.httpService.get(Constants.WEBAPI_BILLING_PURCHASE_DELIVERY_ROWS + "/" + purchaseDeliveryId, false);
    }

    getPurchaseDeliveryRowsByPurchaseId = (purchaseId: number) => {
        return this.httpService.get(Constants.WEBAPI_BILLING_PURCHASE_DELIVERIES_BY_PURCHASEID+"/" + purchaseId , false);
    }

    getDeliveryRowsFromPurchase = (purchaseId: number, supplierId: number) => {
        return this.httpService.get(Constants.WEBAPI_BILLING_PURCHASE_DELIVERY_ROWS_FROM_PURCHASE + purchaseId + "/" + supplierId, false);
    }

    getCustomerInvoicePurchase = (viewType: PurchaseCustomerInvoiceViewType, id: number) => {
        return this.httpService.get(Constants.WEBAPI_BILLING_PURCHASE_CUSTOMERINVOICEROWS + viewType + "/" + id, false);
    }

    //POST
    savePurchaseOrder = (modifiedFields: any, originUsers: OriginUserDTO[], newRows: PurchaseRowDTO[], modifiedRows: StringKeyValueList[]) => {
        const model = {
            modifiedFields: modifiedFields,
            originUsers: originUsers,
            newRows: newRows,
            modifiedRows: modifiedRows
        }

        return this.httpService.post(Constants.WEBAPI_BILLING_PURCHASE, model);
    }


    saveDeliveryRows = (data: IPurchaseDeliverySaveDTO) => {
        return this.httpService.post(Constants.WEBAPI_BILLING_PURCHASE_DELIVERY, data);
    }

    savePurchaseStatus = (id: SoeOriginStatus, purchaseId: number): ng.IPromise<any> => {
        const model = {
            PurchaseId : purchaseId,
            Status : id
        }
        return this.httpService.post(Constants.WEBAPI_BILLING_PURCHASE_STATUS, model);
    };

    updatePurchaseFromOrder = (data: PurchaseFromOrderDTO): ng.IPromise<IActionResult> => {
        return this.httpService.post(Constants.WEBAPI_BILLING_PURCHASE_UPDATEPURCHASEFROMORDER, data);
    }

    createPurchaseFromStockSuggestion = (data: IPurchaseRowFromStockDTO[]) => {
        return this.httpService.post(Constants.WEBAPI_BILLING_PURCHASE_CREATEPURCHASEFROMSTOCKSUGGESTION, data);
    }

    sendPurchaseAsEmail = (purchaseId: number, reportId: number, emailTemplateId: number, langId: number, recipients: number[], singleRecipient: string): ng.IPromise<any> => {
        const dto = {
            purchaseId,
            reportId,
            emailTemplateId,
            langId,
            recipients, 
            singleRecipient
        }
        return this.httpService.post(Constants.WEBAPI_BILLING_PURCHASE_ORDER_EMAIL, dto);
    }
    sendPurchasesAsEmail = (purchaseIds: number[], emailTemplateId: number, langId: number): ng.IPromise<any> => {
        const dto = {
            purchaseIds,
            emailTemplateId,
            langId
        }

        return this.httpService.post(Constants.WEBAPI_BILLING_PURCHASE_ORDERS_EMAIL, dto);
    }

    // DELETE
    deletePurchase = (purchaseId: number) => {
        return this.httpService.delete(Constants.WEBAPI_BILLING_PURCHASE + purchaseId );
    }
}