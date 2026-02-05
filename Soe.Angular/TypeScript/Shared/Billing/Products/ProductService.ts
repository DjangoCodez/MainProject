import { IHttpService } from "../../../Core/Services/HttpService";
import { ISmallGenericType, IProductSmallDTO, IProductUnitSmallDTO, IProductAccountsItem, IInvoiceProductSearchViewDTO, IInvoiceProductPriceSearchViewDTO, IInvoiceProductPriceResult, IInvoiceProductCopyResult, IInvoiceProductDTO, IProductDTO, IProductUnitConvertDTO, IPriceListDTO, IProductPricesRequestDTO } from "../../../Scripts/TypeLite.Net4";
import { AccountVatRateViewSmallDTO } from "../../../Common/Models/AccountDTO";
import { InvoiceProductDTO, ProductRowsProductDTO } from "../../../Common/Models/ProductDTOs";
import { StockDTO } from "../../../Common/Models/StockDTO";
import { SoeTimeCodeType, TermGroup_InvoiceVatType, SoeSysPriceListProviderType, PriceListOrigin } from "../../../Util/CommonEnumerations";
import { Constants } from "../../../Util/Constants";
import { ProductUnitConvertDTO } from "../../../Common/Models/ProductUnitConvertDTO";
import { ProductUnitDTO } from "../../../Common/Models/ProductUnitDTO";
import { CommodityCodeDTO } from "../../../Common/Models/CommodityCodesDTO";

export interface IProductService {

    // GET

    getAccountVatRates(addVatFreeRow: boolean): ng.IPromise<AccountVatRateViewSmallDTO[]>;
    getHouseholdDeductionTypes(addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>;
    getInvoiceProductsForGrid(active: boolean, loadProductUnitAndGroup: boolean, loadAccounts: boolean, loadCategories: boolean, loadTimeCode: boolean): ng.IPromise<any>;
    getInvoiceProducts(active: boolean, loadProductUnitAndGroup: boolean, loadAccounts: boolean, loadCategories: boolean, loadTimeCode: boolean): ng.IPromise<any>;
    getInvoiceProductsSmall(): ng.IPromise<IProductSmallDTO[]>;
    getInvoiceProductsForSelect(): ng.IPromise<any>;
    getLiftProductsSmall(): ng.IPromise<IProductSmallDTO[]>;
    getInvoiceProduct(productId: number): ng.IPromise<InvoiceProductDTO>;
    getPriceLists(): ng.IPromise<any>;
    getPriceListsGrid(): ng.IPromise<any>;
    getPriceListPrices(priceListTypeId: number, loadAll: boolean): ng.IPromise<any>;
    getProductAccounts(rowId: number, productId: number, projectId: number, customerId: number, employeeId: number, vatType: TermGroup_InvoiceVatType, getSalesAccounts: boolean, getPurchaseAccounts: boolean, getVatAccounts: boolean, getInternalAccounts: boolean, isTimeProjectRow: boolean, triangulationSales: boolean): ng.IPromise<IProductAccountsItem>;
    getProductForProductRows(productId: number): ng.IPromise<ProductRowsProductDTO>;
    getProductsForProductRows(productIds: number[]): ng.IPromise<ProductRowsProductDTO[]>;
    getProductUnits(refreshCache?: boolean): ng.IPromise<IProductUnitSmallDTO[]>;
    getProductUnit(productUnitId: number): ng.IPromise<any>;
    getProductGroups(addEmptyRow: boolean): ng.IPromise<any>;
    getProductStatistics(productId: number, originType: number, allItemSelection: number): ng.IPromise<any>;
    getStockByProduct(productId: number, addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]>;
    getStocksByProduct(productId: number): ng.IPromise<StockDTO[]>;
    getVatCodes(): ng.IPromise<any>;
    getTimeCodes(soeTimeCodeType: SoeTimeCodeType, active: boolean, loadPayrollProducts: boolean): ng.IPromise<any>;
    searchInvoiceProducts(number: string, name: string): ng.IPromise<IInvoiceProductSearchViewDTO[]>;
    searchInvoiceProductsExtended(number: string, name: string, productGroup: string, text: string): ng.IPromise<IInvoiceProductSearchViewDTO[]>;
    searchInvoiceProductPrices(priceListTypeId: number, customerId: number, currencyId: number, number: string, providerType: SoeSysPriceListProviderType): ng.IPromise<IInvoiceProductPriceSearchViewDTO[]>;
    getProductPrice(priceListTypeId: number, productId: number, customerId: number, currencyId: number, wholesellerId: number, quantity:number, returnFormula: boolean, copySysProduct: boolean): ng.IPromise<IInvoiceProductPriceResult>;
    getProductPrices(getProductPrices: IProductPricesRequestDTO): ng.IPromise<IInvoiceProductPriceResult>;
    getProductPriceDecimal(priceListTypeId: number, productId: number): ng.IPromise<number>;
    useEdi(): ng.IPromise<boolean>;
    getProductUnitConverts(productId: number, addEmptyRow: boolean): ng.IPromise<ProductUnitConvertDTO[]>;
    getCustomerCommodityCodes(onlyActive?: boolean, ignoreCodeStateCheck?: boolean): ng.IPromise<CommodityCodeDTO[]>;
    getCustomerCommodityCodesDict(addEmpty?: boolean): ng.IPromise<any[]>;
    getProductExternalUrl(productIds: number[]): ng.IPromise<string[]>;
    getVVSProductGroupsForSearch(): ng.IPromise<any[]>;

    // POST
    copyInvoiceProduct(productId: number, purchasePrice: number, salesPrice: number, productUnit: string, priceListTypeId: number, priceListHeadId: number, sysWholesellerName: string, customerId: number, origin: PriceListOrigin): ng.IPromise<IInvoiceProductCopyResult>;
    saveInvoiceProduct(product: IInvoiceProductDTO, priceLists: IPriceListDTO[], categoryRecords: any[], stocks: any[], translations: any[], extrafields: any[]): ng.IPromise<any>;
    updateProductState(dict: any): ng.IPromise<any>;
    saveProductUnitConvert(productUnitConverts: IProductUnitConvertDTO[]): ng.IPromise<any>;
    parseUnitConversionFile(ids: number[], fileData: any): ng.IPromise<any>;
    saveProductUnit(productUnit: ProductUnitDTO, translations: any[]): ng.IPromise<any>;
    saveCustomerCommodityCodes(dict: any): ng.IPromise<any>;
    savePriceListPrices(priceListTypeId: number, priceList: any[], deletedPriceList: any[]): ng.IPromise<any>;

    // DELETE
    deleteProduct(productId: number): ng.IPromise<any>;
    deleteProductUnit(productUnitId: number): ng.IPromise<any>;

    //UPDATE
    updateTimeCodesState(timeCodes: any): ng.IPromise<any>
}

export class ProductService implements IProductService {

    //@ngInject
    constructor(private httpService: IHttpService, private $q: ng.IQService) { }

    // GET

    getAccountVatRates(addVatFreeRow: boolean): ng.IPromise<AccountVatRateViewSmallDTO[]> {
        return this.httpService.get(Constants.WEBAPI_ECONOMY_ACCOUNTING_ACCOUNT_VAT_RATE + addVatFreeRow, true);
    }

    getHouseholdDeductionTypes(addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_PRODUCT_HOUSEHOLD_DEDUCTION_TYPE + addEmptyRow, null, Constants.CACHE_EXPIRE_LONG);
    }

    getInvoiceProductsForGrid(active: boolean, loadProductUnitAndGroup: boolean, loadAccounts: boolean, loadCategories: boolean, loadTimeCode: boolean): ng.IPromise<IProductDTO[]> {
        //return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_PRODUCTS + "?active=" + active + "&loadProductUnitAndGroup=" + loadProductUnitAndGroup + "&loadAccounts=" + loadAccounts + "&loadCategories=" + loadCategories, false, Constants.WEBAPI_ACCEPT_GRID_DTO);
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_PRODUCTS + active + "/" + loadProductUnitAndGroup + "/" + loadAccounts + "/" + loadCategories + "/" + loadTimeCode, false, Constants.WEBAPI_ACCEPT_GRID_DTO);
    }

    getInvoiceProducts(active: boolean, loadProductUnitAndGroup: boolean, loadAccounts: boolean, loadCategories: boolean, loadTimeCode: boolean): ng.IPromise<IProductDTO[]> {
        //return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_PRODUCTS + "?active=" + active + "&loadProductUnitAndGroup=" + loadProductUnitAndGroup + "&loadAccounts=" + loadAccounts + "&loadCategories=" + loadCategories, false, Constants.WEBAPI_ACCEPT_GRID_DTO);
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_PRODUCTS + active + "/" + loadProductUnitAndGroup + "/" + loadAccounts + "/" + loadCategories + "/" + loadTimeCode, false, Constants.WEBAPI_ACCEPT_DTO);
    }

    getInvoiceProductsSmall(): ng.IPromise<IProductSmallDTO[]> {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_PRODUCTS + true + "/" + false + "/" + false + "/" + false + "/" + false, false, Constants.WEBAPI_ACCEPT_SMALL_DTO);
    }

    getInvoiceProductsForSelect(): ng.IPromise<IProductDTO[]> {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_PRODUCTS_FORSELECT, false);
    }

    getLiftProductsSmall(): ng.IPromise<IProductSmallDTO[]> {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_LIFTPRODUCTS, true);
    }

    getInvoiceProduct(productId: number): ng.IPromise<InvoiceProductDTO> {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_PRODUCTS + productId, false, Constants.WEBAPI_ACCEPT_GRID_DTO);
    }

    getPriceLists() {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PRICE_LIST, false);
    }

    getPriceListsGrid() {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PRICE_LIST, false, Constants.WEBAPI_ACCEPT_GRID_DTO);
    }

    getPriceListPrices(priceListTypeId: number, loadAll: boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_PRICES_PRICELIST + "/" + priceListTypeId + "/" + loadAll, false);
    }

    getProductAccounts(rowId: number, productId: number, projectId: number, customerId: number, employeeId: number, vatType: TermGroup_InvoiceVatType, getSalesAccounts: boolean, getPurchaseAccounts: boolean, getVatAccounts: boolean, getInternalAccounts: boolean, isTimeProjectRow: boolean, triangulationSales: boolean): ng.IPromise<IProductAccountsItem> {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_ACCOUNTS + rowId + "/" + productId + "/" + projectId + "/" + customerId + "/" + employeeId + "/" + vatType + "/" + getSalesAccounts + "/" + getPurchaseAccounts + "/" + getVatAccounts + "/" + getInternalAccounts + "/" + isTimeProjectRow + "/" + triangulationSales, false);
    }

    getProductForProductRows(productId: number): ng.IPromise<ProductRowsProductDTO> {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_PRODUCT_ROWS + productId, false);
    }

    getProductsForProductRows(productIds: number[]): ng.IPromise<ProductRowsProductDTO[]> {
        const model = {
            productIds: productIds
        }
        return this.httpService.post(Constants.WEBAPI_BILLING_PRODUCT_PRODUCT_ROWS_LIST, model);
    }

    getProductExternalUrl(productIds: number[]): ng.IPromise<string[]> {
        const model = {
            productIds: productIds
        }
        return this.httpService.post(Constants.WEBAPI_BILLING_PRODUCT_EXTERNAL_URLS, model);
    }

    getVVSProductGroupsForSearch() {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_VVSGROUPSFORSEARCH, true);
    }

    getProductUnit(productUnitId: number) {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_PRODUCT_UNIT + productUnitId, false);
    }

    getProductUnits(refreshCache: boolean = false): ng.IPromise<IProductUnitSmallDTO[]> {
        return this.httpService.getCache(Constants.WEBAPI_BILLING_PRODUCT_PRODUCT_UNIT, null, Constants.CACHE_EXPIRE_LONG, refreshCache);
    }

    getProductUnitConverts(productId: number, addEmptyRow: boolean): ng.IPromise<ProductUnitConvertDTO[]> {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_PRODUCT_UNIT_CONVERT + productId + "/" + addEmptyRow, false).then((x) => {
            return x.map((y) => {
                var unitConvert = new ProductUnitConvertDTO();
                angular.extend(unitConvert, y);
                return unitConvert;
            });
        });
    }

    getCustomerCommodityCodes(onlyActive = false, ignoreCodeStateCheck = false) {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_COMMODITYCODES + onlyActive + "/" + ignoreCodeStateCheck, false);
    }

    getCustomerCommodityCodesDict(addEmpty = true) {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_COMMODITYCODES_DICT + addEmpty, false);
    }

    getProductGroups(addEmptyRow: boolean) {
        return this.httpService.get(Constants.WEBAPI_BILLING_INVOICE_PRODUCT_GROUP + addEmptyRow, false);
    }

    getStockByProduct(productId: number, addEmptyRow: boolean): ng.IPromise<ISmallGenericType[]> {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_STOCK_BY_PRODUCT + productId + "/" + addEmptyRow, false);
    }

    getStocksByProduct(productId: number): ng.IPromise<StockDTO[]> {
        return this.httpService.get(Constants.WEBAPI_BILLING_STOCK_STOCK_BY_PRODUCT + productId, false);
    }

    getVatCodes() {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_ACCOUNTING_VAT_CODE, null, Constants.CACHE_EXPIRE_LONG);
    }

    getTimeCodes(soeTimeCodeType: SoeTimeCodeType, active: boolean, loadPayrollProducts: boolean) {
        return this.httpService.getCache(Constants.WEBAPI_ECONOMY_SUPPLIER_TIME_CODES + soeTimeCodeType + "/" + active + "/" + loadPayrollProducts, null, Constants.CACHE_EXPIRE_LONG);
    }

    searchInvoiceProducts(number: string, name: string): ng.IPromise<IInvoiceProductSearchViewDTO[]> {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_SEARCH + number + "/" + name, false);
    }

    searchInvoiceProductsExtended(number: string, name: string, productGroup: string, text: string) {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_SEARCH_EXTENDED + number + "/" + name + "/" + productGroup + "/" + text, false);
    }

    searchInvoiceProductPrices(priceListTypeId: number, customerId: number, currencyId: number, number: string, providerType: SoeSysPriceListProviderType): ng.IPromise<IInvoiceProductPriceSearchViewDTO[]> {
        const model = {
            priceListTypeId: priceListTypeId,
            customerId: customerId,
            currencyId: currencyId,
            number: number,
            providerType: providerType
        }
        return this.httpService.post(Constants.WEBAPI_BILLING_PRODUCT_PRICES_SEARCH, model);
    }

    getProductPrice(priceListTypeId: number, productId: number, customerId: number, currencyId: number, wholesellerId: number, quantity:number, returnFormula: boolean, copySysProduct: boolean): ng.IPromise<IInvoiceProductPriceResult> {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_PRICES + priceListTypeId + "/" + productId + "/" + customerId + "/" + currencyId + "/" + wholesellerId + "/" + quantity +  "/" + returnFormula + "/" + copySysProduct, false);
    }


    getProductPrices(getProductPrices: IProductPricesRequestDTO): ng.IPromise<IInvoiceProductPriceResult> {
        return this.httpService.post(Constants.WEBAPI_BILLING_PRODUCT_PRICES_COLLECTION, getProductPrices);
    }

    getProductPriceDecimal(priceListTypeId: number, productId: number): ng.IPromise<number> {
        return this.httpService.get(Constants.WEBAPI_BILLING_PRODUCT_PRICES + priceListTypeId + "/" + productId, false);
    }

    getProductStatistics(productId: number, originType: number, allItemSelection: number) {
        const model = {
            ProductId: productId,
            OriginType: originType,
            AllItemSelection: allItemSelection
        };

        return this.httpService.post(Constants.WEBAPI_BILLING_PRODUCT_STATISTICS, model);
    }

    useEdi(): ng.IPromise<boolean> {
        return this.httpService.get(Constants.WEBAPI_BILLING_ORDER_EDI, true);
    }

    // POST
    copyInvoiceProduct(productId: number, purchasePrice: number, salesPrice: number, productUnit: string, priceListTypeId: number, priceListHeadId: number, sysWholesellerName: string, customerId: number, origin: PriceListOrigin): ng.IPromise<IInvoiceProductCopyResult> {
        const model = {
            productId: productId,
            purchasePrice: purchasePrice,
            salesPrice: salesPrice,
            productUnit: productUnit,
            priceListTypeId: priceListTypeId,
            priceListHeadId: priceListHeadId,
            sysWholesellerName: sysWholesellerName,
            customerId: customerId,
            origin: origin
        }

        return this.httpService.post(Constants.WEBAPI_BILLING_PRODUCT_COPY_INVOICE_PRODUCT, model);
    }

    saveInvoiceProduct(product: IInvoiceProductDTO, priceLists: IPriceListDTO[], categoryRecords: any[], stocks: any[], translations: any[], extrafields: any[]) {
        const model = {
            invoiceProduct: product,
            priceLists: priceLists,
            categoryRecords: categoryRecords,
            stocks: stocks,
            translations: translations,
            extrafields: extrafields
        }

        return this.httpService.post(Constants.WEBAPI_BILLING_PRODUCT_SAVE_INVOICE_PRODUCT, model);
    }

    saveProductUnitConvert(productUnitConverts: IProductUnitConvertDTO[]): ng.IPromise<any> {
        return this.httpService.post(Constants.WEBAPI_BILLING_PRODUCT_SAVE_PRODUCT_UNIT_CONVERT, productUnitConverts);
    }

    savePriceListPrices(priceListTypeId: number, priceList: any[], deletedPriceList: any[]) {
        const model = {
            priceListTypeId: priceListTypeId,
            priceLists: priceList,
            deletedPriceLists: deletedPriceList

        };
        return this.httpService.post(Constants.WEBAPI_BILLING_PRODUCT_PRICES_PRICELIST, model);
    }

    parseUnitConversionFile(ids: number[], fileData: any): ng.IPromise<any> {
        const model = {
            productIds: ids,
            fileData: fileData,
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_PRODUCT_PRODUCT_UNIT_CONVERT_PARSE, model);
    }

    updateProductState(dict: any) {
        const model = {
            dict: dict
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_PRODUCT_PRODUCTS_UPDATE_STATE, model);
    }

    saveProductUnit(productUnit: ProductUnitDTO, translations: any[]) {
        const model = {
            productUnit: productUnit,
            translations: translations
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_PRODUCT_PRODUCT_UNIT, model);
    }

    saveCustomerCommodityCodes(dict: any) {
        const model = {
            dict: dict
        };
        return this.httpService.post(Constants.WEBAPI_BILLING_PRODUCT_COMMODITYCODES, model);
    }

    // DELETE

    deleteProduct(productId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_PRODUCT_PRODUCTS + productId);
    }

    deleteProductUnit(productUnitId: number) {
        return this.httpService.delete(Constants.WEBAPI_BILLING_PRODUCT_PRODUCT_UNIT + productUnitId);
    }

    //UPDATE
    updateTimeCodesState(dict: any) {
        var model = { dict: dict };
        return this.httpService.post(Constants.WEBAPI_TIME_TIME_TIME_CODE_UPDATESTATE, model);
    }
}
