import { StockProductDTO } from "../../../Common/Models/StockProductDTO";
import { IHttpService } from "../../../Core/Services/HttpService";
import { IGenerateStockPurchaseSuggestionDTO, IProductSmallDTO, IPurchaseRowFromStockDTO } from "../../../Scripts/TypeLite.Net4";
import { Constants } from "../../../Util/Constants";

export interface IStockService {

    // GET
    getAccountStdsDict(addEmptyRow: boolean): ng.IPromise<any>;
    getStocks(addEmptyRow: boolean): ng.IPromise<any>;
    getStock(stockId: number): ng.IPromise<any>;
    getStocksDict(addEmptyRow: boolean): ng.IPromise<any>;
    getStockByProduct(productId: number, addEmptyRow: boolean): ng.IPromise<any>;
    getStockProductsByProduct(productId: number): ng.IPromise<StockProductDTO[]>;
    getStockProductsByProducts(productIds: number[]): ng.IPromise<StockProductDTO[]>;
    getStockPlaces(addEmptyRow: boolean, stockId: number): ng.IPromise<any>;
    getStockPlace(stockPlaceId: number): ng.IPromise<any>;
    getStockProducts(includeInactive:boolean): ng.IPromise<any>;
    getStockHandledProductsSmall(): ng.IPromise<IProductSmallDTO[]>;
    getStockProduct(stockProductId: number): ng.IPromise<any>;
    getStockInventories(): ng.IPromise<any>;
    getStockInventory(stockInventoryHeadId: number): ng.IPromise<any>;
    getStockInventoryRows(stockInventoryHeadId: number): ng.IPromise<any>;
    generateStockInventoryRows(stockId: number, productNrFrom: string, productNrTo: string, shelfIdFrom: number, shelfIdTo: number): ng.IPromise<any>;
    getStockProductTransactions(stockProductId: number): ng.IPromise<any>;
    getStockProductAvgPrice(stockId:number, productId: number): ng.IPromise<any>;
    closeStockInventory(stockInventoryHeadid: number): ng.IPromise<any>;

    // POST
    saveStock(stock: any): ng.IPromise<any>;
    saveStockPlace(stockPlace: any): ng.IPromise<any>;
    saveStockTransactions(stockTransactions: any[]): ng.IPromise<any>;
    stockTransfer(invoiceProductId: number, fromStockId: number, toStockId: number, quantity: number): ng.IPromise<any>;
    saveStockInventoryRows(stockInventoryHead: any, stockInventoryRows: any[]): ng.IPromise<any>;
    importStockBalances(wholesellerId: number, stockId: number, createVoucher: boolean, fileName: string, fileData: any): ng.IPromise<any>;
    importStockInventory(stockInventoryHeadId: number, fileName: string, fileData: any): ng.IPromise<any>;
    recalculateStockBalance(stockId: number): ng.IPromise<any>;
    generatePurchaseSuggestion(model: IGenerateStockPurchaseSuggestionDTO): ng.IPromise<IPurchaseRowFromStockDTO[]>;

    // DELETE
    deleteStock(stockId: number): ng.IPromise<any>;
    deleteStockPlace(stockPlaceId: number): ng.IPromise<any>;
    deleteStockInventory(stockInventoryHeadId: number): ng.IPromise<any>;
}

export class StockService implements IStockService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET
    getAccountStdsDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_STD + addEmptyRow, false);
    }

    getStocks(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_STOCK + addEmptyRow, false);
    }

    getStock(stockId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_STOCK + stockId, false);
    }

    getStocksDict(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_STOCK_DICT + addEmptyRow, false);
    }

    getStockByProduct(productId: number, addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_STOCK_BY_PRODUCT + productId + "/" + addEmptyRow, false);
    }

    getStockPlaces(addEmptyRow: boolean, stockId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_PLACE + addEmptyRow + "/" + stockId, false);
    }

    getStockPlace(stockPlaceId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_PLACE + stockPlaceId, false);
    }

    getStockProducts(includeInactive:boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_PRODUCTS + "/" + includeInactive, false);
    }

    getStockProductsByProduct(productId: number): ng.IPromise<StockProductDTO[]>
    {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_PRODUCTS + "/" + productId, false, Constants.WEBAPI_ACCEPT_DTO);
    }

    getStockProductAvgPrice(stockId: number, productId: number): ng.IPromise<any> {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_PRODUCT_AVGPRICE + "/" + stockId + "/" + productId, false);
    }

    getStockProductsByProducts(productIds: number[]): ng.IPromise<StockProductDTO[]> {
        const model = {
            productIds: productIds
        }
        return this.httpService.post(Constants.WEBAPI_BILLING_STOCK_PRODUCTS, model);
    }

    getStockHandledProductsSmall(): ng.IPromise<IProductSmallDTO[]> {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_PRODUCTS_PRODUCTS, false);
    }
    

    getStockProduct(stockProductId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_PRODUCT + stockProductId, false);
    }

    getStockInventories() {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_INVENTORIES, false);
    }

    getStockInventory(stockInventoryHeadId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_INVENTORY + stockInventoryHeadId, false);
    }

    getStockInventoryRows(stockInventoryHeadId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_INVENTORY_ROWS + stockInventoryHeadId, false);
    }

    generateStockInventoryRows(stockId: number, productNrFrom: string, productNrTo: string, shelfIdFrom: number, shelfIdTo: number) {
        const path = Constants.WEBAPI_BILLING_STOCK_INVENTORY_ROWS_GENERATE + stockId + "/" + productNrFrom + "/" + productNrTo + "/" + shelfIdFrom + "/" + shelfIdTo;
        return this.httpService.get(path, false);
    }

    getStockProductTransactions(stockProductId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_PRODUCT_TRANSACTIONS + stockProductId, false);
    }

    closeStockInventory(stockInventoryHeadId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_INVENTORY_CLOSE + stockInventoryHeadId, false);
    }

    // POST

    saveStock(stock: any) {
        return this.httpService.post(Constants.WEBAPI_BILLING_STOCK_STOCK, stock);
    }

    saveStockPlace(stockPlace: any) {
        return this.httpService.post(Constants.WEBAPI_BILLING_STOCK_PLACE, stockPlace);
    }

    saveStockTransactions(stockTransactions: any[]) {
        return this.httpService.post(Constants.WEBAPI_BILLING_STOCK_PRODUCT_TRANSACTIONS, stockTransactions);
    }

    stockTransfer(invoiceProductId: number, fromStockId: number, toStockId: number, quantity: number) {
        return this.httpService.post(Constants.WEBAPI_BILLING_STOCK_STOCK_TRANSFER + invoiceProductId + "/" + fromStockId + "/" + toStockId + "/" + quantity, false);
    }

    saveStockInventoryRows(stockInventoryHead: any, stockInventoryRows: any[]) {
        const model = {
            inventoryHead: stockInventoryHead,
            inventoryRows: stockInventoryRows
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_STOCK_INVENTORY_SAVE, model);
    }

    importStockBalances(wholesellerId: number, stockId: number, createVoucher: boolean, fileName: string, fileData: any): ng.IPromise<any> {
        const model = {
            wholesellerId: wholesellerId,
            stockId: stockId,
            createVoucher: createVoucher,
            fileName: fileName,
            fileData: fileData
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_STOCK_IMPORT_STOCK_BALANCES, model);
    }

    recalculateStockBalance(stockId: number) {
        return this.httpService.post(Constants.WEBAPI_BILLING_STOCK_RECALCULATE_BALANCE + stockId, false);
    }

    importStockInventory(stockInventoryHeadId: number, fileName: string, fileData: any): ng.IPromise<any> {
        const model = {
            stockInventoryHeadId: stockInventoryHeadId,
            fileName: fileName,
            fileData: fileData
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_STOCK_IMPORT_STOCK_INVENTORY, model);
    }

    generatePurchaseSuggestion(model: IGenerateStockPurchaseSuggestionDTO): ng.IPromise<IPurchaseRowFromStockDTO[]> {
        return this.httpService.post(Constants.WEBAPI_BILLING_STOCK_PURCHASE_GENERATESUGGESTION, model);
    }

    // DELETE

    deleteStock(stockId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_STOCK_STOCK + stockId);
    }

    deleteStockPlace(stockPlaceId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_STOCK_PLACE + stockPlaceId);
    }

    deleteStockInventory(stockInventoryHeadId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_STOCK_INVENTORY + stockInventoryHeadId);
    }
}